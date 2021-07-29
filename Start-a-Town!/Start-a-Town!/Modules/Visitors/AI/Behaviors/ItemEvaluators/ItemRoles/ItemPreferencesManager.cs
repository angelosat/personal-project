using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_
{
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
        static Dictionary<string, ItemRole> Registry = new();
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
        readonly Dictionary<ItemRole, int> Preferences = new();
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
            {
                this.Preferences.Add(r, -1);
                this.PreferencesNew.Add(r, default);
            }
        }

        public void HandleItem(Entity item)
        {
            foreach (var role in this.Preferences.Keys)
            {
                var score = role.Score(this.Actor, item);
                if (score < 0)
                    continue;
                var existingItemID = this.Preferences[role];
                var existingItem = existingItemID != -1 ? this.Actor.Net.GetNetworkObject<Entity>(existingItemID) : null;
                var existingScore = existingItem != null ? role.Score(this.Actor, existingItem) : 0;
                if (score > existingScore)
                {
                    SetItemPreference(item, role);
                    return; // TODO check 
                }
            }
            if (!this.IsUseful(item))
                this.ToDiscard.Add(item.RefID);
        }

        private void SetItemPreference(Entity item, ItemRole role)
        {
            this.Preferences[role] = item.RefID;
        }
        private Entity GetPreference(ItemRole role)
        {
            var refid = this.Preferences[role];
            return refid == -1 ? null : this.Actor.Net.GetNetworkObject<Entity>(refid);
        }
        public Entity GetPreference(GearType gt)
        {
            return this.GetPreference(ItemRolesGear[gt]);
        }
        public Entity GetPreference(ToolAbilityDef def)
        {
            return this.GetPreference(ItemRolesTool[def]);
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
            if (this.Preferences.Values.Contains(item.RefID))
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
            this.PreferencesNew[bestRole.r] = new() { ItemRefId = item.RefID, Score = bestRole.Item2 };
            return true;
        }
        public void AddPreferenceTool(Entity tool)
        {
            //var t = tool as Tool;
            var toolUse = tool.ToolComponent.Props.Ability.Def;
            this.SetItemPreference(tool, ItemRolesTool[toolUse]);
        }

        public void RemovePreference(ToolAbilityDef toolUse)
        {
            this.Preferences[ItemRolesTool[toolUse]] = -1;
        }
        public bool IsPreference(Entity item)
        {
            return this.PreferencesNew.Values.Any(p => item.RefID == p.ItemRefId);
            return this.Preferences.Any(p => p.Key.Score(this.Actor, item) > -1 && item.RefID == p.Value);
        }

        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);

            var dicGear = ItemRolesGear.Where(r => this.Preferences[r.Value] != -1).ToDictionary(r => r.Key.Name, r => this.Preferences[r.Value]);
            tag.Add(dicGear.Save("PreferencesGear"));

            var dicTool = ItemRolesTool.Where(r => this.Preferences[r.Value] != -1).ToDictionary(r => r.Key.Name, r => this.Preferences[r.Value]);
            tag.Add(dicTool.Save("PreferencesTool"));

            this.PreferencesNew.Where(p => p.Value.ItemRefId > 0).ToDictionary(p => p.Key.ToString(), p => p.Value).SaveNew(tag, "Preferences");

            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            var dicGear = new Dictionary<string, int>().Load(tag["PreferencesGear"]).ToDictionary(i => Def.GetDef<GearType>(i.Key), i => i.Value);
            foreach (var i in dicGear)
                this.Preferences[ItemRolesGear[i.Key]] = i.Value;
            var dicTool = new Dictionary<string, int>().Load(tag["PreferencesTool"]).ToDictionary(i => Def.GetDef<ToolAbilityDef>(i.Key), i => i.Value);
            foreach (var i in dicTool)
                this.Preferences[ItemRolesTool[i.Key]] = i.Value;

            tag.TryGetTag("Preferences", pt =>
            {
                var dicPrefs = new Dictionary<string, ItemPreference>().LoadNew(pt);
                foreach (var d in dicPrefs)
                    this.PreferencesNew[Registry[d.Key]] = d.Value;
            });

            return this;
        }

        public void Write(BinaryWriter w)
        {
            var dicGear = ItemRolesGear.Where(r => this.Preferences[r.Value] != -1).ToDictionary(r => r.Key.Name, r => this.Preferences[r.Value]);
            dicGear.WriteNew(w, key => w.Write(key), value => w.Write(value));
            var dicTool = ItemRolesTool.Where(r => this.Preferences[r.Value] != -1).ToDictionary(r => r.Key.Name, r => this.Preferences[r.Value]);
            dicTool.WriteNew(w, key => w.Write(key), value => w.Write(value));

            foreach (var r in this.PreferencesNew.Values)
                r.Write(w);
        }

        public ISerializable Read(BinaryReader r)
        {
            var dicGear = new Dictionary<GearType, int>().ReadNew(r, r => Def.GetDef<GearType>(r.ReadString()), r => r.ReadInt32());
            foreach (var i in dicGear)
                this.Preferences[ItemRolesGear[i.Key]] = i.Value;
            var dicTool = new Dictionary<ToolAbilityDef, int>().ReadNew(r, r => Def.GetDef<ToolAbilityDef>(r.ReadString()), r => r.ReadInt32());
            foreach (var i in dicTool)
                this.Preferences[ItemRolesTool[i.Key]] = i.Value;

            foreach (var p in this.PreferencesNew)
                p.Value.Read(r);
            return this;
        }

        struct ItemPreference : ISaveable
        {
            public int ItemRefId;
            public int Score;
            public override string ToString()
            {
                return $"{ItemRefId}:{Score}";
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

            public void Save(SaveTag tag, string name)
            {
                this.ItemRefId.Save(tag, "ItemRefId");
                this.Score.Save(tag, "Score");
            }

            public void Load(SaveTag tag, string name)
            {
                this.ItemRefId = (int)tag[name]["ItemRefId"].Value;
                this.Score = (int)tag[name]["Score"].Value;
            }

            public SaveTag Save(string name = "")
            {
                var tag = new SaveTag(SaveTag.Types.Compound, name);
                this.ItemRefId.Save(tag, "ItemRefId");
                this.Score.Save(tag, "Score");
                return tag;
            }

            public ISaveable Load(SaveTag tag)
            {
                this.ItemRefId = (int)tag["ItemRefId"].Value;
                this.Score = (int)tag["Score"].Value;
                return this;
            }
        }
    }
}
