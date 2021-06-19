using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.UI;
using Start_a_Town_.Net;
using Start_a_Town_.Crafting;

namespace Start_a_Town_
{
    public partial class CraftOrderNew : ILoadReferencable<CraftOrderNew>, ILoadReferencable, ISerializable
    {

        static readonly Dictionary<int, CraftOrderNew> References = new();

        public static CraftOrderNew GetOrder(int id)
        {
            return References[id];
        }

        enum CraftMode { XTimes, UntilX, Forever }
        public CraftOrderFinishMode FinishMode = CraftOrderFinishMode.AllModes.First();
        public int Quantity = 1;
        CraftMode Mode = CraftMode.XTimes;
        public int ID;
        public Reaction Reaction;
        public Vector3 Workstation;
        public IMap Map;
        public bool HaulOnFinish;
        public bool Enabled;

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
        //internal bool IsValid(string name, Material mat)
        //{
        //    return
        //        !this.Restrictions[name].IsRestricted(mat) &&
        //        this.Reaction.Reagents.Find(r => r.Name == name).Filter(mat);//.Modifiers.All(m => m(mat));
        //}
        //public IngredientRestrictions Restrictions = new IngredientRestrictions();
        public Dictionary<string, IngredientRestrictions> Restrictions = new();
        readonly Dictionary<string, HashSet<int>> ReagentRestrictions = new();
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
        string GetQuantity()
        {
            return this.FinishMode.GetString(this);
            switch(this.Mode)
            {
                case CraftMode.XTimes:
                    return "x" + this.Quantity.ToString();

                case CraftMode.UntilX:
                    return "Until: " + this.Quantity.ToString();

                case CraftMode.Forever:
                    return "Forever";

                default:
                    throw new Exception();
            }
        }
        public CraftOrderNew()
        {

        }
        public CraftOrderNew(int id, int reactionID, IMap map, Vector3 workstation) 
            : this(Reaction.Dictionary[reactionID])
        {
            this.Map = map;
            this.Workstation = workstation;
            this.ID = id;
            //this.Reaction = Reaction.Dictionary[reactionID];
            //foreach (var r in this.Reaction.Reagents)
            //    this.Restrictions.Add(r.Name, new IngredientRestrictions());
        }
        public CraftOrderNew(Reaction reaction)
        {
            this.Reaction = reaction;
            foreach (var r in this.Reaction.Reagents)
                this.Restrictions.Add(r.Name, new IngredientRestrictions(r.Ingredient.DefaultRestrictions));
        }
        public string Name
        {
            get
            {
                return this.Reaction.Name;
            }
        }

        public void Complete(GameObject agent)
        {
            this.FinishMode.OnComplete(this);
            //if (this.Mode == CraftMode.XTimes)
            //    this.Quantity--;
            agent.Net.EventOccured(Components.Message.Types.OrdersUpdatedNew, this.Workstation);
        }
        //public BlockEntityWorkstation GetWorkstation()
        //{
        //    return this.Map.GetBlockEntity<BlockEntityWorkstation>(this.Workstation);
        //}
        public BlockEntityCompWorkstation GetWorkstation()
        {
            return this.Map.GetBlockEntity(this.Workstation).GetComp<BlockEntityCompWorkstation>();
        }
        public bool IsActive()
        {
            if (!this.GetWorkstation().Orders.Contains(this))
                return false;
            return this.FinishMode.IsActive(this);
        }
        public bool IsActive(IMap map)
        {
            return this.IsActive();
            //get
            //{
            //if (!map.Town.CraftingManager.OrderExists(this))
            //    return false;
            if (!this.GetWorkstation().Orders.Contains(this))
                return false;

                return this.FinishMode.IsActive(this);
                return
                    (this.Mode == CraftMode.XTimes && this.Quantity > 0) ||
                    // TODO: include the do until mode when i figure out how to track current town inventory
                    (this.Mode == CraftMode.Forever);

            //}
        }

        internal void ToggleReagentRestrictions(string reagent, ItemDef[] defs, Material[] mats, MaterialType[] matTypes)
        {
            this.Restrictions[reagent].ToggleRestrictions(defs, mats, matTypes);
        }

        //public CraftOperation CreateCraftOperation()
        //{
        //    //var op = new CraftOperation()
        //}

        public Interface GetInterface()
        {
            return new Interface(this);
        }

        
        CraftOrderNew(SaveTag tag):this(null, tag)
        {
            
        }
        CraftOrderNew(IMap map, SaveTag tag)
        {
            tag.TryGetTagValue<int>("ReactionID", p => this.Reaction = Reaction.Dictionary[p]);
            tag.TryGetTagValue<int>("Mode", p => this.Mode = (CraftMode)p);
            //tag.TryGetTagValue<string>("FinishMode", p => this.FinishMode = Activator.CreateInstance(Type.GetType(p)) as CraftOrderFinishMode);
            tag.TryGetTagValue<int>("FinishMode", p => this.FinishMode = CraftOrderFinishMode.GetMode(p));

            tag.TryGetTagValue<int>("Quantity", p => this.Quantity = p);
            tag.TryGetTagValue<Vector3>("Bench", p => this.Workstation = p);
            this.ReagentRestrictions.Clear();
            tag.TryGetTagValue<List<SaveTag>>("Restrictions", restrictionsTag =>
            {
                //var dic = restrictionsTag.Value as Dictionary<string, SaveTag>;
                //foreach (var rTag in dic)
                //{
                //    var item = (int)rTag.Value.Value;
                //    this.ToggleReagentRestriction(rTag.Key, item);
                //}

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

                //foreach (var rTag in reagents)
                //{
                //    var item = (int)rTag.Value;
                //    this.ToggleReagentRestriction(rTag.Name, item);
                //}
            });
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.ID);
            w.Write(this.Reaction.ID);
            w.Write((int)this.Mode);
            
            //w.Write(this.FinishMode.GetType().FullName);
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

        public void Read(IMap map, BinaryReader r)
        {
            this.Read(r);
            this.Map = map;
        }
        public ISerializable Read(BinaryReader r)
        {
            this.ID = r.ReadInt32();
            this.Reaction = Reaction.Dictionary[r.ReadInt32()];
            this.Mode = (CraftMode)r.ReadInt32();
            //this.FinishMode = Activator.CreateInstance(Type.GetType(r.ReadString())) as CraftOrderFinishMode;
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
        public CraftOrderNew(IMap map, BinaryReader r)
        {
            this.Read(map, r);
        }


        public class Interface : GroupBox
        {
            CraftOrderNew Order;
            Label //Quantity,
                OrderName;
            IconButton BtnClose;
            ButtonIcon BtnUp, BtnDown;
            Button BtnPlus, BtnMinus, BtnDetails;
            Vector3 Global;
            CraftOrderDetailsInterface PanelDetails;
            ComboBoxNewNew<CraftOrderFinishMode> ComboFinishMode;
            public Interface(CraftOrderNew order)
            {
                this.Global = order.Workstation;
                this.Order = order;

                this.BtnUp = new ButtonIcon(Icon.ArrowUp) { LeftClickAction = MoveUp };
                this.BtnDown = new ButtonIcon(Icon.ArrowDown) { LeftClickAction = MoveDown, Location = this.BtnUp.BottomLeft };
                this.AddControls(this.BtnUp, this.BtnDown);

                this.OrderName = new Label(order.Reaction.Name) { Location = this.BtnUp.TopRight };
                //this.Quantity = new Label(order.GetQuantity()) { Location = this.OrderName.BottomLeft };
                //this.ComboFinishMode = new ComboBoxNewNew<CraftOrderFinishMode>(CraftOrderFinishMode.AllModes, 100, c => c.GetString(this.Order), ChangeFinishMode, Order.FinishMode) { Location = this.Quantity.BottomLeft };
                this.ComboFinishMode = new ComboBoxNewNew<CraftOrderFinishMode>(CraftOrderFinishMode.AllModes, 100, c => c.GetString(this.Order), ChangeFinishMode, ()=>this.Order.FinishMode) { Location = this.OrderName.BottomLeft };

                this.AddControls(this.OrderName, 
                    //this.Quantity, 
                    this.ComboFinishMode);
                //this.AlignTopToBottom();

                this.BtnClose = new IconButton(Icon.X) { LocationFunc = () => new Vector2(PanelTitled.GetClientLength(290), 0), BackgroundTexture = UIManager.Icon16Background };
                this.BtnClose.Anchor = Vector2.UnitX;
                this.BtnClose.LeftClickAction = RemoveOrder;
                this.AddControls(this.BtnClose);

                this.BtnMinus = new Button("-", Button.DefaultHeight) { Location = this.ComboFinishMode.TopRight, LeftClickAction = Minus };
                this.BtnPlus = new Button("+", Button.DefaultHeight) { Location = this.BtnMinus.TopRight, LeftClickAction = Plus };
                this.AddControls(this.BtnMinus, this.BtnPlus);

                this.BtnDetails = new Button("Details") { Anchor = Vector2.UnitX, LeftClickAction = ToggleDetails };
                //this.BtnDetails.Location = new Vector2(this.BtnClose.Right, this.BtnMinus.Top);
                this.BtnDetails.LocationFunc = () => this.BottomRight;
                this.BtnDetails.Anchor = Vector2.One;
                this.AddControls(this.BtnDetails);

                this.PanelDetails = new CraftOrderDetailsInterface(this.Order);// { AutoSize = true };
                this.PanelDetails.ToWindow(this.Order.Name);

            }

            private void ChangeFinishMode(CraftOrderFinishMode obj)
            {
                PacketCraftOrderChangeMode.Send(this.Order, (int)obj.Mode);
            }

            private void MoveDown()
            {
                this.ChangeOrderPriority(false);
            }
            private void MoveUp()
            {
                this.ChangeOrderPriority(true);
            }

            private void ChangeOrderPriority(bool p)
            {
                //var index = this.GetIndex();
                //global::Start_a_Town_.Towns.Crafting.CraftingManager.WriteOrderModifyPriority(Client.Instance.OutgoingStream, this.Global, index, p);

                global::Start_a_Town_.Towns.Crafting.CraftingManager.WriteOrderModifyPriority(Client.Instance.OutgoingStream, this.Order, p);
            }

            public int GetIndex()
            {
                return this.Order.GetIndex();
                //var index = Client.Instance.Map.Town.CraftingManager.GetOrdersNew(this.Global).FindIndex(c => c == this.Order);
                var index = this.Order.Map.Town.CraftingManager.GetOrdersNew(this.Global).FindIndex(c => c == this.Order);
                return index;
            }

            private void ToggleDetails()
            {
                //var bench = this.Order.Map.GetBlockEntity(this.Order.Workstation) as global::Start_a_Town_.Crafting.BlockEntityWorkstation;
                //this.PanelDetails.Refresh(this.Order.Reaction, bench.Input.Slots);
                var win = this.PanelDetails.GetWindow();
                win.Location = this.BtnDetails.ScreenLocation + this.BtnDetails.Width * Vector2.UnitX;
                win.Toggle();
            }

            private void Minus()
            {
                var index = this.GetIndex();
                //global::Start_a_Town_.Towns.Crafting.CraftingManager.WriteOrderModifyQuantityParams(Client.Instance.OutgoingStream, this.Global, index, -1);
                global::Start_a_Town_.Towns.Crafting.CraftingManager.WriteOrderModifyQuantityParams(Client.Instance.OutgoingStream, this.Order, -1);

            }

            private void Plus()
            {
                var index = this.GetIndex(); //Client.Instance.Map.Town.CraftingManager.GetOrdersNew(this.Global).FindIndex(c => c == this.Order);
                //global::Start_a_Town_.Towns.Crafting.CraftingManager.WriteOrderModifyQuantityParams(Client.Instance.OutgoingStream, this.Global, index, 1);
                global::Start_a_Town_.Towns.Crafting.CraftingManager.WriteOrderModifyQuantityParams(Client.Instance.OutgoingStream, this.Order, 1);

            }

            private void RemoveOrder()
            {
                //PacketOrderRemove.Send(this.Order.Map.Net, this.Order.Workstation, this.Order.GetIndex());
                PacketOrderRemove.Send(this.Order.Map.Net, this.Order);

                return;
                //var index = Client.Instance.Map.Town.CraftingManager.GetOrdersNew(this.Global).FindIndex(c => c == this.Order);
                byte[] data = Network.Serialize(w =>
                {
                    //w.Write(Player.Actor.InstanceID);
                    //w.Write(this.Order.ID);
                    w.Write(this.Order.Workstation);
                    w.Write(this.Order.GetIndex());
                });
                Client.Instance.Send(PacketType.CraftingOrderRemoveNew, data);
            }

            //internal override void OnGameEvent(GameEvent e)
            //{
            //    switch(e.Type)
            //    {
            //        case Components.Message.Types.OrdersUpdated:
            //            this.Quantity.Text = this.Order.GetQuantity();
            //            break;

            //        default:
            //            break;
            //    }
            //}
        }
        //public class InterfaceDetails : GroupBox
        //{
        //    CraftOrderNew Order;
        //    public InterfaceDetails(CraftOrderNew order)
        //    {
        //        this.Order = order;
        //    }

        //}



        internal int GetIndex()
        {
            return this.Map.Town.CraftingManager.GetOrdersNew(this.Workstation).FindIndex(c => c == this);
        }

        internal bool IsValid(IMap map)
        {
            var manager = map.Town.CraftingManager;
            return manager.OrderExists(this) && this.IsActive(map);
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

        internal static CraftOrderNew Load(IMap map, SaveTag ordertag)
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
            //tag.Add(new SaveTag(SaveTag.Types.String, "FinishMode", this.FinishMode.GetType().FullName));
            tag.Add(new SaveTag(SaveTag.Types.Int, "FinishMode", (int)this.FinishMode.Mode));
            tag.Add(this.ID.Save("ID"));
            this.Enabled.Save(tag, "Enabled");
            tag.Add(new SaveTag(SaveTag.Types.Int, "Quantity", this.Quantity));
            //tag.Add(this.Workstation.Save("Bench"));
            tag.Add(this.Workstation.Save("Bench"));


            var tagRestr = new SaveTag(SaveTag.Types.Compound, "RestrictionsNew");
            tagRestr.Add(this.Restrictions.Keys.Save("Keys"));
            tagRestr.Add(this.Restrictions.Values.SaveNewBEST("Values"));
            tag.Add(tagRestr);


            var tagRestrictions = new SaveTag(SaveTag.Types.List, "Restrictions", SaveTag.Types.Compound);
            foreach (var rest in this.ReagentRestrictions)
            {
                //var tagRestriction = new SaveTag(SaveTag.Types.List, rest.Key, SaveTag.Types.Int);
                //foreach (var item in rest.Value)
                //    tagRestriction.Add(new SaveTag(SaveTag.Types.Int, "", item));
                //tagRestrictions.Add(tagRestriction);

                var tagReagent = new SaveTag(SaveTag.Types.Compound, "");
                tagReagent.Add(new SaveTag(SaveTag.Types.String, "Reagent", rest.Key));
                var tagItems = new SaveTag(SaveTag.Types.List, "Items", SaveTag.Types.Int);
                foreach (var item in rest.Value)
                    tagItems.Add(new SaveTag(SaveTag.Types.Int, "", item));
                tagReagent.Add(tagItems);
                tagRestrictions.Add(tagReagent);
                //var tagIDs = new List<SaveTag>();
                //foreach (var item in rest.Value)
                //    tagIDs.Add(new SaveTag(SaveTag.Types.Int, "", item));
                //tagRestrictions.Add(new SaveTag(SaveTag.Types.List, rest.Key, tagIDs));
            }
            tag.Add(tagRestrictions);
            return tag;
        }

        public string GetUniqueLoadID()
        {
            return $"CraftOrder{this.ID}";// string.Format("CraftOrder{0}", this.ID);
        }


        //SaveTag ISaveable.Save(string name)
        //{
        //    throw new NotImplementedException();
        //}

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

        //readonly static Lazy<ListBoxCollapsible> DetailsUIReagents = new(() => new ListBoxCollapsible(200, 200));
        //readonly static Lazy<Control> DetailsUIContainer = new(() => new GroupBox().AddControls(DetailsUIReagents.Value));
        static ListBoxCollapsible DetailsUIReagents;// = new ListBoxCollapsible(200, 200);
        static Control DetailsUIContainer;// = new GroupBox().AddControls(DetailsUIReagents);

        Control GetDetailsUI(Action<CraftOrderNew, string, ItemDef[], Material[], MaterialType[]> callback)
        {
            var box = DetailsUIContainer;//.Value;// new GroupBox();
            box.ClearControls();
            var order = this;
            var list = new ListBoxCollapsible(200, 200);
            foreach (var r in order.Reaction.Reagents)
            {
                var items = r.Ingredient.GetAllValidItemDefs();

                var itemTypesNode = new ListBoxCollapsibleNode(r.Name);

                list.AddNode(itemTypesNode);
                foreach (var i in items)
                {
                    var mats = i.GetValidMaterials().ToList();
                    
                    var itemNode = new ListBoxCollapsibleNode(i.Name, () => new CheckBoxNew()
                    {
                        TickedFunc = () => !mats.Any(v => order.IsRestricted(r.Name, v)),
                        LeftClickAction = () => callback(
                            order,
                            r.Name,
                            null,
                            mats.GroupBy(m => order.Restrictions[r.Name].Material.Contains(m)).OrderBy(c => c.Count()).First().ToArray(),
                            null)
                    });
                    itemTypesNode.AddNode(itemNode);
                    foreach (var mat in mats)
                    {
                        itemNode.AddLeaf(new CheckBoxNew(mat.Name)
                        {
                            TickedFunc = () => !order.IsRestricted(r.Name, mat),
                            LeftClickAction = () => callback(order, r.Name, null, new Material[] { mat }, null)
                        });
                    }
                }
            }
            //return list;
            list.Build();
            box.AddControls(list);
            return box;
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
            //var container = DetailsUIContainer ??= new GroupBox().AddControls(list).ToContextMenuClosable("Details");
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
        //internal SaveTag SaveNew(string name)
        //{
        //    var tag = new SaveTag(SaveTag.Types.Reference, "unicueorderid", this);
        //    return tag;
        //    //tag.Add(this.Reaction.Name.Save("Reaction"));
        //    //tag.Add(new SaveTag(SaveTag.Types.Int, "Mode", (int)this.Mode));
        //    //tag.Add(new SaveTag(SaveTag.Types.Int, "FinishMode", (int)this.FinishMode.Mode));
        //    //tag.Add(this.Quantity.Save("Quantity"));
        //    //tag.Add(this.Workstation.Save("Bench"));
        //}
        //internal void LoadNew(SaveTag tag)
        //{
        //    var defname = tag.GetValue<string>("Reaction");
        //    this.Reaction = Def.GetDef<Reaction>(defname);
        //}
    }
}
