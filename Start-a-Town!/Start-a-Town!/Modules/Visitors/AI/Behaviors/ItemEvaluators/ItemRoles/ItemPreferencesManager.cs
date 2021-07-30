using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    class ItemPreferencesManager : IItemPreferencesManager, ISaveable, ISerializable
    {
        static ItemPreferencesManager()
        {
            Init();
        }
        static void Init()
        {
            GenerateItemRolesGear();
            GenerateItemRolesTools();

            foreach (var r in ItemRolesTool.Values.Concat(ItemRolesGear.Values))
                Registry[r.ToString()] = r;
        }
        static readonly Dictionary<string, ItemRole> Registry = new();
        static void GenerateItemRolesGear()
        {
            var geardefs = GearType.Dictionary.Values;
            foreach (var g in geardefs)
                ItemRolesGear.Add(g, new ItemRoleGear(g));
        }
        static void GenerateItemRolesTools()
        {
            var defs = Def.Database.Values.OfType<ToolAbilityDef>();
            foreach (var d in defs)
                ItemRolesTool.Add(d, new ItemRoleTool(d));
        }
        
        static public readonly Dictionary<GearType, ItemRole> ItemRolesGear = new();
        static public readonly Dictionary<ToolAbilityDef, ItemRole> ItemRolesTool = new();
        readonly Dictionary<ItemRole, ItemPreference> PreferencesNew = new();
        readonly HashSet<int> ToDiscard = new();
        readonly HashSet<Entity> Items = new();
        readonly Actor Actor;
        public ItemPreferencesManager(Actor actor)
        {
            this.Actor = actor;
            this.PopulateRoles();
        }

        private void PopulateRoles()
        {
            foreach (var r in ItemRolesTool.Values.Concat(ItemRolesGear.Values))
                this.PreferencesNew.Add(r, new(r));
        }
        public void HandleItem(Entity item)
        {
            foreach (var pref in this.PreferencesNew.Values)
            {
                var role = pref.Role;
                var score = role.Score(this.Actor, item);
                if (score < 0)
                    continue;
                if (score > pref.Score)
                {
                    pref.ItemRefId = item.RefID;
                    pref.Score = score;
                    return; // TODO check 
                }
            }
            if (!this.IsUseful(item))
                this.ToDiscard.Add(item.RefID);
        }

        //public void HandleItem(Entity item)
        //{
        //    foreach (var role in this.Preferences.Keys)
        //    {
        //        var score = role.Score(this.Actor, item);
        //        if (score < 0)
        //            continue;
        //        var existingItemID = this.Preferences[role];
        //        var existingItem = existingItemID != -1 ? this.Actor.Net.GetNetworkObject<Entity>(existingItemID) : null;
        //        var existingScore = existingItem != null ? role.Score(this.Actor, existingItem) : 0;
        //        if (score > existingScore)
        //        {
        //            SetItemPreference(item, role);
        //            return; // TODO check 
        //        }
        //    }
        //    if (!this.IsUseful(item))
        //        this.ToDiscard.Add(item.RefID);
        //}

        private Entity GetPreference(ItemRole role)
        {
            var refid = this.PreferencesNew[role].ItemRefId;
            return refid > 0 ? this.Actor.Net.GetNetworkObject<Entity>(refid) : null;
        }
        //public Entity GetPreference(GearType gt)
        //{
        //    return this.GetPreference(ItemRolesGear[gt]);
        //}
        //public Entity GetPreference(ToolAbilityDef def)
        //{
        //    return this.GetPreference(ItemRolesTool[def]);
        //}
        public Entity GetPreference(object tag)
        {
            return this.GetPreference(Registry.Values.First(r => r.Tag == tag));
        }

        public void ResetPreferences()
        {
            var items = this.Actor.Inventory.GetItems();
            foreach (var i in items)
                this.HandleItem(i);
        }
        public IEnumerable<Entity> GetUselessItems(IEnumerable<Entity> entity)
        {
            var items = this.Actor.Inventory.GetItems();
            foreach (var i in items)
                if(!this.IsUseful(i))
                    yield return i;
        }
        public bool IsUseful(Entity item)
        {
            if (item.Def == ItemDefOf.Coins)
                return true;
            if (this.PreferencesNew.Values.Any(p => p.ItemRefId == item.RefID))
                return true;
            return false;
        }
        public void Validate()
        {
            this.ResetPreferences();
        }

        public IEnumerable<Entity> GetJunk()
        {
            this.Validate();
            var actor = this.Actor;
            var net = actor.Net;
            var items = actor.Inventory.GetItems();
            foreach (var i in this.ToDiscard.ToArray())
            {
                var item = net.GetNetworkObject(i) as Entity;
                if (!items.Contains(item))
                {
                    this.RemoveJunk(item);
                    continue;
                }
                yield return item;
            }
        }
        public void RemoveJunk(Entity item)
        {
            this.ToDiscard.Remove(item.RefID);
        }
        static IEnumerable<ItemRole> AllRoles => ItemRolesGear.Values.Concat(ItemRolesTool.Values);
        public bool AddPreference(Entity item)
        {
            var scored = AllRoles
                .Select(r => (r, r.Score(this.Actor, item)))
                .Where(rs => rs.Item2 > -1)
                .OrderByDescending(rs => rs.Item2);
            if (!scored.Any())
                return false;
            var bestRole = scored.First();
            var pref = this.PreferencesNew[bestRole.r];
            pref.ItemRefId = item.RefID;
            pref.Score = bestRole.Item2;
            return true;
        }
       
        public void RemovePreference(ToolAbilityDef toolUse)
        {
            this.PreferencesNew[ItemRolesTool[toolUse]].Clear();
        }
        public bool IsPreference(Entity item)
        {
            return this.PreferencesNew.Values.Any(p => item.RefID == p.ItemRefId);
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(this.PreferencesNew.Values.Where(p => p.ItemRefId > 0).Save("Preferences"));
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTag("Preferences", pt =>
            {
                foreach (var p in pt.LoadList<ItemPreference>())
                    this.PreferencesNew[p.Role].CopyFrom(p);
            });

            return this;
        }

        public void Write(BinaryWriter w)
        {
            foreach (var r in this.PreferencesNew.Values)
                r.Write(w);
        }

        public ISerializable Read(BinaryReader r)
        {
            foreach (var p in this.PreferencesNew)
                p.Value.Read(r);
            return this;
        }

        class ItemPreference : ISaveable
        {
            public ItemRole Role;
            public int ItemRefId;
            public int Score;
            public ItemPreference()
            {

            }
            public ItemPreference(ItemRole role)
            {
                this.Role = role;
                this.ItemRefId = 0;
                this.Score = 0;
            }
            public void CopyFrom(ItemPreference pref)
            {
                if (this.Role != pref.Role)
                    throw new Exception();
                this.ItemRefId = pref.ItemRefId;
                this.Score = pref.Score;
            }
            public override string ToString()
            {
                return $"{Role}:{ItemRefId}:{Score}";
            }

            public void Write(BinaryWriter w)
            {
                w.Write(this.ItemRefId);
                w.Write(this.Score);
            }

            public void Read(BinaryReader r)
            {
                this.ItemRefId = r.ReadInt32();
                this.Score = r.ReadInt32();
            }

            public SaveTag Save(string name = "")
            {
                var tag = new SaveTag(SaveTag.Types.Compound, name);
                this.Role.ToString().Save(tag, "Role");
                this.ItemRefId.Save(tag, "ItemRefId");
                this.Score.Save(tag, "Score");
                return tag;
            }

            public ISaveable Load(SaveTag tag)
            {
                this.Role = Registry[(string)tag["Role"].Value];
                this.ItemRefId = (int)tag["ItemRefId"].Value;
                this.Score = (int)tag["Score"].Value;
                return this;
            }

            internal void Clear()
            {
                this.ItemRefId = 0;
                this.Score = 0;
            }
        }
    }
}
