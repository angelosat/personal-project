using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public partial class CraftOrderNew : ILoadReferencable<CraftOrderNew>, ILoadReferencable, ISerializable
    {
        static readonly Dictionary<int, CraftOrderNew> References = new();

        public static CraftOrderNew GetOrder(int id)
        {
            return References[id];
        }
        public string Name => this.Reaction.Name;

        enum CraftMode { XTimes, UntilX, Forever }
        public CraftOrderFinishMode FinishMode = CraftOrderFinishMode.AllModes.First();
        public int Quantity = 1;
        CraftMode Mode = CraftMode.XTimes;
        public int ID;
        public Reaction Reaction;
        public Vector3 Workstation;
        public MapBase Map;
        public bool HaulOnFinish;
        public bool Enabled;
        public Dictionary<string, IngredientRestrictions> Restrictions = new();
        readonly Dictionary<string, HashSet<int>> ReagentRestrictions = new();
        static ListBoxCollapsible DetailsUIReagents;
        static Control DetailsUIContainer;

        public bool IsActive
        {
            get
            {
                if (!this.GetWorkstation().Orders.Contains(this))
                    return false;
                return this.FinishMode.IsActive(this);
            }
        }

        public CraftOrderNew()
        {

        }
        public CraftOrderNew(int id, int reactionID, MapBase map, Vector3 workstation)
            : this(Reaction.Dictionary[reactionID])
        {
            this.Map = map;
            this.Workstation = workstation;
            this.ID = id;
        }
        public CraftOrderNew(Reaction reaction)
        {
            this.Reaction = reaction;
            foreach (var r in this.Reaction.Reagents)
                this.Restrictions.Add(r.Name, new IngredientRestrictions(r.Ingredient.DefaultRestrictions));
        }

        internal bool IsRestricted(string name, ItemDef i)
        {
            return this.Restrictions[name].IsRestricted(i);
        }
        internal bool IsRestricted(string name, Material i)
        {
            return this.Restrictions[name].IsRestricted(i);
        }
        internal bool IsRestricted(Material i)
        {
            return this.Restrictions.Values.Any(r => r.IsRestricted(i));
        }
        public bool IsAllowed(Material mat)
        {
            if (this.IsRestricted(mat))
                return false;
            foreach (var r in this.Reaction.Reagents)
            {
                var items = r.Ingredient.GetAllValidItemDefs();
                foreach (var i in items)
                    if (i.GetValidMaterials().Contains(mat))
                        return true;
            }
            return false;
        }
       
        public void ToggleReagentRestriction(string reagentName, int itemID)
        {
            if (!this.ReagentRestrictions.TryGetValue(reagentName, out HashSet<int> list))
            {
                list = new HashSet<int>();
                this.ReagentRestrictions.Add(reagentName, list);
            }
            if (!list.Remove(itemID))
                list.Add(itemID);
        }
        public void ToggleReagentRestriction(string reagentName, int itemID, bool add)
        {
            if (!this.ReagentRestrictions.TryGetValue(reagentName, out HashSet<int> list))
            {
                if (!add)
                    return;
                list = new HashSet<int>();
                this.ReagentRestrictions.Add(reagentName, list);
            }
            if (!add)
                list.Remove(itemID);
            else
                list.Add(itemID);
        }

        public bool IsReagentAllowed(string reagentName, int itemID)
        {
            if (!this.ReagentRestrictions.TryGetValue(reagentName, out HashSet<int> list))
                return true;
            return !list.Contains(itemID);
        }
        public bool IsItemAllowed(string reagentName, Entity item)
        {
            return
                !this.Restrictions[reagentName].IsRestricted(item) &&
                this.Reaction.Reagents.Find(r => r.Name == reagentName).Filter(item);
        }
        
        public void Complete(GameObject agent)
        {
            this.FinishMode.OnComplete(this);
            agent.Net.EventOccured(Components.Message.Types.OrdersUpdatedNew, this.Workstation);
        }
        
        public BlockEntityCompWorkstation GetWorkstation()
        {
            return this.Map.GetBlockEntity(this.Workstation).GetComp<BlockEntityCompWorkstation>();
        }
        
        
        internal void ToggleReagentRestrictions(string reagent, ItemDef[] defs, Material[] mats, MaterialType[] matTypes)
        {
            this.Restrictions[reagent].ToggleRestrictions(defs, mats, matTypes);
        }

        public UI GetInterface()
        {
            return new UI(this);
        }
        
        CraftOrderNew(SaveTag tag):this(null, tag)
        {
            
        }
        CraftOrderNew(MapBase map, SaveTag tag)
        {
            tag.TryGetTagValue<int>("ReactionID", p => this.Reaction = Reaction.Dictionary[p]);
            tag.TryGetTagValue<int>("Mode", p => this.Mode = (CraftMode)p);
            tag.TryGetTagValue<int>("FinishMode", p => this.FinishMode = CraftOrderFinishMode.GetMode(p));

            tag.TryGetTagValue<int>("Quantity", p => this.Quantity = p);
            tag.TryGetTagValue<Vector3>("Bench", p => this.Workstation = p);
            this.ReagentRestrictions.Clear();
            tag.TryGetTagValue<List<SaveTag>>("Restrictions", restrictionsTag =>
            {
                
                foreach(var rTag in restrictionsTag)
                {
                    var name = rTag.GetValue<string>("Reagent");
                    rTag.TryGetTagValue<List<SaveTag>>("Items", list =>
                        {
                            foreach(var itemTag in list)
                            {
                                var item = (int)itemTag.Value;
                                this.ToggleReagentRestriction(name, item);
                            }
                        });
                }

            });
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.ID);
            w.Write(this.Reaction.ID);
            w.Write((int)this.Mode);
            w.Write((int)this.FinishMode.Mode);
            w.Write(this.Quantity);
            w.Write(this.Workstation);
            w.Write(this.ReagentRestrictions.Count);
            w.Write(this.HaulOnFinish);
            foreach(var r in this.ReagentRestrictions)
            {
                w.Write(r.Key);
                w.Write(r.Value.Count);
                foreach (var i in r.Value)
                    w.Write(i);
            }

            w.Write(this.Restrictions.Keys.ToArray());
            this.Restrictions.Values.Write(w);
        }

        public void Read(MapBase map, BinaryReader r)
        {
            this.Read(r);
            this.Map = map;
        }
        public ISerializable Read(BinaryReader r)
        {
            this.ID = r.ReadInt32();
            this.Reaction = Reaction.Dictionary[r.ReadInt32()];
            this.Mode = (CraftMode)r.ReadInt32();
            this.FinishMode = CraftOrderFinishMode.GetMode(r.ReadInt32());
            this.Quantity = r.ReadInt32();
            this.Workstation = r.ReadVector3();
            var rCount = r.ReadInt32();
            this.ReagentRestrictions.Clear();
            this.HaulOnFinish = r.ReadBoolean();
            for (int i = 0; i < rCount; i++)
            {
                var rName = r.ReadString();
                var iCount = r.ReadInt32();
                for (int j = 0; j < iCount; j++)
                {
                    var id = r.ReadInt32();
                    this.ToggleReagentRestriction(rName, id);
                }
            }

            var restrictionsKeys = r.ReadStringArray();
            var restrictionValues = new List<IngredientRestrictions>();
            restrictionValues.ReadMutable(r);
            this.Restrictions = restrictionsKeys.ToDictionary(restrictionValues);

            return this;
        }
        public CraftOrderNew(MapBase map, BinaryReader r)
        {
            this.Read(map, r);
        }
       
        internal int GetIndex()
        {
            return this.Map.Town.CraftingManager.GetOrdersNew(this.Workstation).FindIndex(c => c == this);
        }

        internal bool IsValid(MapBase map)
        {
            var manager = map.Town.CraftingManager;
            return manager.OrderExists(this) && this.IsActive;
        }
        internal bool IsCompletable(IEntityCompContainer buildSite)
        {
            if (this.Reaction.Fuel == 0)
                return true;
            return buildSite.GetComp<EntityCompRefuelable>()?.Fuel.Value > this.Reaction.Fuel;
        }
        internal bool IsCompletable(Blocks.BlockEntity buildSite)
        {
            if (this.Reaction.Fuel == 0)
                return true;
            return buildSite.GetComp<EntityCompRefuelable>()?.Fuel.Value > this.Reaction.Fuel;
        }
        internal bool IsCompletable()
        {
            return this.IsCompletable(this.Map.GetBlockEntity(this.Workstation));
        }
        internal static CraftOrderNew Load(SaveTag t)
        {
            var order = new CraftOrderNew(t);
            var id = order.ID;
            if (References.TryGetValue(order.ID, out var existing))
                return existing;
            References[id] = order;
            return order;
        }

        internal static CraftOrderNew Load(MapBase map, SaveTag ordertag)
        {
            var order = Load(ordertag);
            order.Map = map;
            return order;
        }

        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(new SaveTag(SaveTag.Types.Int, "ReactionID", this.Reaction.ID));
            tag.Add(new SaveTag(SaveTag.Types.Int, "Mode", (int)this.Mode));
            tag.Add(new SaveTag(SaveTag.Types.Int, "FinishMode", (int)this.FinishMode.Mode));
            tag.Add(this.ID.Save("ID"));
            this.Enabled.Save(tag, "Enabled");
            tag.Add(new SaveTag(SaveTag.Types.Int, "Quantity", this.Quantity));
            tag.Add(this.Workstation.Save("Bench"));

            var tagRestr = new SaveTag(SaveTag.Types.Compound, "RestrictionsNew");
            tagRestr.Add(this.Restrictions.Keys.Save("Keys"));
            tagRestr.Add(this.Restrictions.Values.SaveNewBEST("Values"));
            tag.Add(tagRestr);

            var tagRestrictions = new SaveTag(SaveTag.Types.List, "Restrictions", SaveTag.Types.Compound);
            foreach (var rest in this.ReagentRestrictions)
            {
                var tagReagent = new SaveTag(SaveTag.Types.Compound, "");
                tagReagent.Add(new SaveTag(SaveTag.Types.String, "Reagent", rest.Key));
                var tagItems = new SaveTag(SaveTag.Types.List, "Items", SaveTag.Types.Int);
                foreach (var item in rest.Value)
                    tagItems.Add(new SaveTag(SaveTag.Types.Int, "", item));
                tagReagent.Add(tagItems);
                tagRestrictions.Add(tagReagent);
            }
            tag.Add(tagRestrictions);
            return tag;
        }

        public string GetUniqueLoadID()
        {
            return $"CraftOrder{this.ID}";
        }

        ISaveable ISaveable.Load(SaveTag tag)
        {
            tag.TryGetTagValue<int>("ReactionID", p => this.Reaction = Reaction.Dictionary[p]);
            tag.TryGetTagValue<int>("Mode", p => this.Mode = (CraftMode)p);
            tag.TryGetTagValue<int>("FinishMode", p => this.FinishMode = CraftOrderFinishMode.GetMode(p));
            tag.TryGetTagValue<int>("ID", out this.ID);
            tag.TryGetTagValue<int>("Quantity", p => this.Quantity = p);
            tag.TryGetTagValue<Vector3>("Bench", p => this.Workstation = p);
            tag.TryGetTagValue("Enabled", out this.Enabled);
            tag.TryGetTag("RestrictionsNew", t =>
                {
                    var keys = t.LoadStringList("Keys");
                    var values = new List<IngredientRestrictions>();
                    values.TryLoadMutable(t, "Values");
                    this.Restrictions = keys.ToDictionary(values);
                });

            this.ReagentRestrictions.Clear();
            tag.TryGetTagValue<List<SaveTag>>("Restrictions", restrictionsTag =>
            {
                foreach (var rTag in restrictionsTag)
                {
                    var name = rTag.GetValue<string>("Reagent");
                    rTag.TryGetTagValue<List<SaveTag>>("Items", list =>
                    {
                        foreach (var itemTag in list)
                        {
                            var item = (int)itemTag.Value;
                            this.ToggleReagentRestriction(name, item);
                        }
                    });
                }
            });
            return this;
        }

        public override string ToString()
        {
            return this.GetUniqueLoadID();
        }

        public void ShowDetailsUI(Action<CraftOrderNew, string, ItemDef[], Material[], MaterialType[]> callback)
        {
            var list = DetailsUIReagents ??= new ListBoxCollapsible(200, 200);
            list.Clear();
            foreach (var r in this.Reaction.Reagents)
            {
                var items = r.Ingredient.GetAllValidItemDefs();
                var itemTypesNode = new ListBoxCollapsibleNode(r.Name);
                list.AddNode(itemTypesNode);
                foreach (var i in items)
                {
                    var mats = i.GetValidMaterials().ToList();

                    var itemNode = new ListBoxCollapsibleNode(i.Name, () => new CheckBoxNew()
                    {
                        TickedFunc = () => !mats.Any(v => this.IsRestricted(r.Name, v)),
                        LeftClickAction = () => callback(
                            this,
                            r.Name,
                            null,
                            mats.GroupBy(m => this.Restrictions[r.Name].Material.Contains(m)).OrderBy(c => c.Count()).First().ToArray(),
                            null)
                    });
                    itemTypesNode.AddNode(itemNode);
                    foreach (var mat in mats)
                    {
                        itemNode.AddLeaf(new CheckBoxNew(mat.Name)
                        {
                            TickedFunc = () => !this.IsRestricted(r.Name, mat),
                            LeftClickAction = () => callback(this, r.Name, null, new Material[] { mat }, null)
                        });
                    }
                }
            }
            list.Build();
            var container = DetailsUIContainer ??= new GroupBox()
                .AddControls(list)
                .ToWindow("Details")
                .SnapToMouse()
                .SetGameEventAction(e =>
                {
                    if (e.Type == Components.Message.Types.OrderDeleted && e.Parameters[0] == DetailsUIContainer.Tag)
                        DetailsUIContainer.Hide();
                });

            if (container.Tag == this)
                container.Toggle();
            else
                container.Show();
            container.Tag = this;
            container.GetWindow().SetTitle($"\"{this.Name}\" details");
        }
    }
}
