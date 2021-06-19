using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components
{
    class WorkbenchReactionComponent : Component
    {
        public override string ComponentName
        {
            get { return "WorkbenchReactionComponent"; }
        }
        public override object Clone()
        {
            //return new WorkbenchReactionComponent().Initialize(this.Slots.Parent, this.Slots.Capacity);
            var comp = new WorkbenchReactionComponent(this.Storage.Slots.Count, this.Blueprints.Slots.Count);
            using (BinaryWriter w = new BinaryWriter(new MemoryStream()))
            {
                this.Write(w);
                w.BaseStream.Position = 0;
                using (BinaryReader r = new BinaryReader(w.BaseStream))
                    comp.Read(r);
            }
            return comp;
        }
        GameObject Parent { get { return (GameObject)this["Parent"]; } set { this["Parent"] = value; } }
        byte MaterialCapacity { get { return (byte)this["MaterialCapacity"]; } set { this["MaterialCapacity"] = value; } }
        //public ItemContainer BlueprintSlots { get { return (ItemContainer)this["BlueprintSlots"]; } set { this["BlueprintSlots"] = value; } }
        //public ItemContainer Slots { get { return (ItemContainer)this["Slots"]; } set { this["Slots"] = value; } }
        public Container Blueprints { get { return (Container)this["Blueprints"]; } set { this["Blueprints"] = value; } }
        public Container Storage { get { return (Container)this["Storage"]; } set { this["Storage"] = value; } }
        public bool ConsumeMaterials = true;
        public Reaction SelectedReaction;

        public WorkbenchReactionComponent Initialize(GameObject parent, byte materialCapacity)
        {
            this.Parent = parent;
            this.MaterialCapacity = materialCapacity;
            //this.Slots = new ItemContainer(parent, materialCapacity);// { ID = parent.ContainerSequence };
            //BlueprintSlots = new ItemContainer(parent, 8, o => o.HasComponent<BlueprintComponent>());// { ID = parent.ContainerSequence };
            return this;
        }

        public WorkbenchReactionComponent()
        {
            this.MaterialCapacity = 1;
            this.Blueprints = new Container() { Name = "BLueprints" };
            this.Storage = new Container() { Name = "Storage" };
        }
        public WorkbenchReactionComponent(int storageCapacity, int blueprintCapacity)
        {
            this.MaterialCapacity = 1;
            this.Blueprints = new Container(blueprintCapacity) { Name = "BLueprints" };
            this.Storage = new Container(storageCapacity) { Name = "Storage" };
        }
        public override void GetChildren(List<GameObjectSlot> list)
        {
            //list.AddRange(this.Slots);
            //list.AddRange(this.BlueprintSlots);
        }
        public override void MakeChildOf(GameObject parent)
        {
            this.Parent = parent;
            //this.Slots = new ItemContainer(parent, this.MaterialCapacity);
            //this.BlueprintSlots = new ItemContainer(parent, 8, o =>
            //{
            //    return o.HasComponent<BlueprintComponent>();
            //});
            parent.RegisterContainer(this.Storage);
            parent.RegisterContainer(this.Blueprints);
        }
        //public override void ObjectLoaded(GameObject parent)
        //{
        //    this.Slots.Parent = parent;
        //    //this.Slots.ID = parent.ContainerSequence;
        //    //if (Slots.ID > 1)
        //    //    "gamw".ToConsole();
        //    this.BlueprintSlots.Parent = parent;
        //    //this.BlueprintSlots.ID = parent.ContainerSequence;
        //    //foreach (var slot in this.Slots)
        //    //    slot.Parent = parent;
        //    //foreach (var slot in this.BlueprintSlots)
        //    //    slot.Parent = parent;
        //}
        internal override List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();
            //data.Add(new SaveTag(SaveTag.Types.Compound, "Slots", this.Slots.Save()));
            //data.Add(new SaveTag(SaveTag.Types.Byte, "Capacity", this.Slots.Capacity));
            //data.Add(new SaveTag(SaveTag.Types.Compound, "Blueprints", this.BlueprintSlots.Save()));
            data.Add(new SaveTag(SaveTag.Types.Compound, "Storage", this.Storage.Save()));
            data.Add(new SaveTag(SaveTag.Types.Compound, "BlueprintsNew", this.Blueprints.Save()));
            return data;
        }
        internal override void Load(SaveTag compTag)
        {
            //this.MaterialCapacity = compTag.TagValueOrDefault<byte>("Capacity", 2);
            //this.Slots = ItemContainer.Create(this.Parent, compTag["Slots"]);
            //SaveTag bpTag;
            //if (compTag.TryGetTag("Blueprints", out bpTag))
            //    this.BlueprintSlots = ItemContainer.Create(this.Parent, bpTag);
            
            compTag.TryGetTag("Storage", tag => this.Storage.Load(tag));
            compTag.TryGetTag("BlueprintsNew", tag => this.Blueprints.Load(tag));
        }
        public override void Write(System.IO.BinaryWriter writer)
        {
            //this.Slots.Write(writer);
            //this.BlueprintSlots.Write(writer);
            this.Storage.Write(writer);
            this.Blueprints.Write(writer);
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            //this.Slots.Read(reader);
            //this.BlueprintSlots.Read(reader);
            //this.Slots = new ItemContainer(reader);
            //this.BlueprintSlots = new ItemContainer(reader);
            this.Storage.Read(reader);
            this.Blueprints.Read(reader);
        }
        public override void ObjectSynced(GameObject parent)
        {
            //this.Slots.Parent = parent;
            //this.BlueprintSlots.Parent = parent;
        }
        public override void GetContainers(List<Container> list)
        {
            list.Add(this.Storage);
            list.Add(this.Blueprints);
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                //case Message.Types.Activate: // TODO: refactor this for all components that have similar functionality (workbenches, furnaces, etc)
                //    GameObject actor = e.Parameters[0] as GameObject;
                //    GameObjectSlot hauled = actor.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling];
                //    if (hauled.HasValue)
                //        this.Slots.InsertObject(e.Network, hauled);
                //    return true;

                case Message.Types.SlotInteraction:
                    GameObject actor = e.Parameters[0] as GameObject;
                    var slot = e.Parameters[1] as GameObjectSlot;
                    actor.GetComponent<WorkComponent>().Perform(actor, new Components.Interactions.PickUp(), new TargetArgs(slot));
                    return true;

                default:
                    return true;
            }
        }

       
        private static void Craft(GameObject parent, GameObject product)
        {
            Net.Client.PostPlayerInput(Message.Types.StartScript, w =>
            {
                Ability.Write(w, Script.Types.Reaction, new TargetArgs(parent), ww => product.Write(ww));
            });
        }

        public void Craft(GameObject parent, Reaction.Product.ProductMaterialPair product)
        {
            //var p = this.SelectedReaction.Products.First().GetProduct(this.SelectedReaction, Player.Actor, this.Slots).Product;

            //if (parent.Net is Net.Client)
            //    return;
            var server = parent.Net as Net.Server;
            if (server == null)
                return;
            var p = product.Product;

            //// check materials and consume them if they exist, otherwise return
            //foreach (var mat in product.Requirements)
            //{
            //    if (mat.ObjectID == null)
            //        return;
            //    if (!this.Slots.Check(mat.ObjectID, mat.Amount))
            //        return;
            //}

            server.SyncInstantiate(p);
            server.PopLoot(p, parent);
        }

        //public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<Net.GameEvent>> handlers)
        //{
        //    GroupBox box_bps = new GroupBox();
        //    Panel panel_bplist = new Panel() { ClientDimensions = new Vector2(150 - BackgroundStyle.Panel.Border, 150) };// AutoSize = true };
        //    Panel panel_blueprint = new Panel() { Location = panel_bplist.TopRight, ClientDimensions = panel_bplist.ClientDimensions };
        //    panel_blueprint.Controls.Add(new Label() { Text = "No blueprint selected" });
        //    ListBox<GameObject, Button> list_bps = new ListBox<GameObject, Button>(new Rectangle(0, 0, 150, 150));
        //    List<GameObject> bplist;

        //    box_bps.Controls.Add(panel_blueprint, panel_bplist);
        //    Label lbl_materials = "Materials".ToLabel();
        //    SlotGrid<SlotDefault> slots_mats = new SlotGrid<SlotDefault>(Slots, 4) { Location = lbl_materials.BottomLeft };//
        //    Label lbl_bps = "Stored Blueprints".ToLabel(slots_mats.BottomLeft);
        //    SlotGrid<SlotDefault> slots_bps = new SlotGrid<SlotDefault>(this.BlueprintSlots, 4) { Location = lbl_bps.BottomLeft };//

        //    var list = new ListBox<Reaction, Button>(new Rectangle(0, 0, 150, 200));

        //    Panel panel_Selected = new Panel();

        //    Panel panel_Reagents = new Panel() { };//AutoSize = true };
        //    panel_Reagents.ClientSize = list.Size;


        //    List<GameObjectSlot> matSlots = new List<GameObjectSlot>();
        //    //Reaction selected = null;
        //    Action refreshBpList = () =>
        //    {
        //        list.Build(GetAvailableBlueprints(parent), foo => foo.Name, (r, btn) =>
        //        {
        //            btn.LeftClickAction = () =>
        //            {
        //                this.SelectedReaction = r;
        //                RefreshProductVariants(panel_Reagents, panel_Selected, r, matSlots);
        //                panel_Selected.Controls.Clear();
        //            };
        //        });
        //    };
        //    refreshBpList();


        //    RadioButton
        //        rd_All = new RadioButton("All", Vector2.Zero, true)
        //        {
        //            LeftClickAction = () =>
        //            {
        //                refreshBpList();
        //            }
        //        },
        //        rd_MatsReady = new RadioButton("Have Materials", rd_All.TopRight)
        //        {
        //            LeftClickAction = () =>
        //            {
        //                //list.Build(GetAvailableBlueprints(parent).FindAll(foo => BlueprintComponent.MaterialsAvailable(foo, this.Slots)), foo => foo.Name, RecipeListControlInitializer(panel_Selected));
        //            }
        //        };
        //    Panel panel_filters = new Panel() { Location = slots_bps.BottomLeft, AutoSize = true };
        //    panel_filters.Controls.Add(rd_All, rd_MatsReady);

        //    Panel panel_List = new Panel() { Location = panel_filters.BottomLeft, AutoSize = true };
        //    panel_List.Controls.Add(list);

        //    panel_Reagents.Location = panel_List.TopRight;


        //    panel_Selected.Location = panel_List.BottomLeft;

        //    panel_Selected.Size = new Rectangle(0, 0, panel_List.Size.Width + panel_Reagents.Size.Width, panel_Reagents.Size.Height);

        //    Button btn_craft = new Button(panel_Selected.BottomLeft, panel_List.Width + panel_Reagents.Width, "Craft")
        //    {
        //        LeftClickAction = () =>
        //        {
        //            //if (list.SelectedItem.IsNull())
        //            if (this.SelectedReaction == null)
        //                return;
        //            GameObject prod = panel_Selected.Tag as GameObject;
        //            //Net.Client.PostPlayerInput(Message.Types., w =>
        //            //{
        //            //    Ability.Write(w, Script.Types.Reaction, new TargetArgs(parent), ww => product.Write(ww));
        //            //});
        //            var p = this.SelectedReaction.Products.First().GetProduct(this.SelectedReaction, parent, matSlots);
        //            Net.Client.Instance.Send(PacketType.PlayerCraftBench, Net.Network.Serialize(w =>
        //            {
        //                w.Write(Player.Actor.Network.ID);
        //                w.Write(parent.Network.ID);
        //                p.Write(w);
        //            }));

        //            //Craft(parent, prod);
        //        }
        //    };

        //    ui.Controls.Add(
        //        lbl_materials,
        //        slots_mats,
        //        lbl_bps, slots_bps,
        //        panel_filters, panel_List, panel_Reagents, panel_Selected, btn_craft);


        //    handlers.Add(new EventHandler<Net.GameEvent>((sender, e) =>
        //    {
        //        if ((e.Parameters[0] as GameObject) != parent)
        //            return;
        //        switch (e.Type)
        //        {
        //            case Message.Types.InventoryChanged:
        //                refreshBpList();
        //                RefreshSelectedPanel(panel_Selected, this.SelectedReaction, matSlots, this.Slots);
        //                return;

        //            default:
        //                return;
        //        }
        //    }));
        //}

        private List<Reaction> GetAvailableBlueprints(GameObject parent)
        {
            return (from reaction in Reaction.Dictionary.Values
                    //where reaction.Building == parent.ID
                    //where reaction.ValidWorkshops.Contains(parent.GetInfo().ID)//(int)parent.ID)
                    select reaction).ToList();
        }
        private Action<Reaction, Button> RecipeListControlInitializer(Panel panel_Selected)
        {
            return (foo, btn) =>
            {
                btn.LeftClickAction = () =>
                {
                    Reaction obj = foo;
                    return;
                };
            };
        }

        private void RefreshProductVariants(Panel panelReagents, Panel panelSelected, Reaction reaction, List<GameObjectSlot> slots)
        {
            panelReagents.Controls.Clear();
            // List<GameObjectSlot> slots = new List<GameObjectSlot>();
            panelReagents.Controls.Add(new Label(reaction.Name) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            panelReagents.Controls.Add(new Label(panelReagents.Controls.BottomLeft, "Materials") { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold });
            slots.Clear();
            foreach (var reagent in reaction.Reagents)
            {
                GameObjectSlot matSlot = GameObjectSlot.Empty;
                matSlot.Name = reagent.Name;
                slots.Add(matSlot);
                Slot slot = new Slot(panelReagents.Controls.BottomLeft) { Tag = matSlot, CustomTooltip = true };
                //slot.HoverFunc = () => reagent.Condition.ToString();
                slot.HoverFunc = () => { string t = ""; foreach (var filter in reagent.Conditions) { t += filter.ToString(); } return t.TrimEnd('\n'); };//
                Label matName = new Label(slot.TopRight, reagent.Name);
                slot.DragDropAction = a =>
                {
                    slot.Tag.Object = (a as DragDropSlot).Slot.Object;
                    return DragDropEffects.Move;// Link;
                };
                slot.RightClickAction = () =>
                {
                    slot.Tag.Clear();
                };
                matSlot.Filter = reagent.Filter;// reagent.Condition.Condition;
                matSlot.ObjectChanged = o =>
                {
                    if ((from sl in slots where !sl.HasValue select sl).FirstOrDefault() != null)
                    {
                        panelSelected.Controls.Clear();
                        return;
                    }
                    //RefreshSelectedPanel(panelSelected, reaction, slots, this.Slots);
                };
                panelReagents.Controls.Add(slot, matName);
            }
        }
        private void RefreshSelectedPanel(Panel panel_Selected, Reaction reaction, List<GameObjectSlot> materials, List<GameObjectSlot> container)// Reaction reaction)
        {
            if (reaction.IsNull())
                return;
            var matList = (from s in materials where s.HasValue select s.Object).ToList();
            if (matList.Count < materials.Count)
            {
                panel_Selected.Controls.Clear();
                return;
            }
            List<ItemRequirement> reqs = new List<ItemRequirement>();
            foreach (var mat in matList)
            {
                int amount = container.GetAmount(obj => obj.ID == mat.ID);
                reqs.Add(new ItemRequirement(mat.ID, 1, amount));
            }
            //RefreshSelectedPanel(panel_Selected, reaction.Products.First().Create(reaction, matList), reqs);// (from s in materials select new ItemRequirement(s.Object.ID, 1)).ToList());
            RefreshSelectedPanel(panel_Selected, reaction.Products.First().Create(reaction, materials), reqs);// (from s in materials select new ItemRequirement(s.Object.ID, 1)).ToList());
        }
        private void RefreshSelectedPanel(Panel panel_Selected, GameObject product, List<ItemRequirement> materials)// Reaction reaction)
        {
            panel_Selected.Controls.Clear();
            panel_Selected.Tag = product;
            if (product.IsNull())
                return;

            CraftingTooltip tip = new CraftingTooltip(product.ToSlot(), materials);
            panel_Selected.Controls.Add(tip);

            //GroupBox box = new GroupBox();
            //PanelLabeled panelproduct = new PanelLabeled("Product") { AutoSize = true };
            //Slot slot = new Slot() { Tag = product.ToSlot(), Location = panelproduct.Controls.BottomLeft };
            //Label lbl = new Label(product.Name) { Location = slot.TopRight };
            //panelproduct.Controls.Add(slot, lbl);

            //foreach(var item in materials)
            //{
            //    PanelLabeled panelmats = new PanelLabeled("Materials") { AutoSize = true, Location = panelproduct.BottomLeft };
            //    SlotWithText mat = new SlotWithText() { Tag = item.Req.ObjectID.GetObject().ToSlot(), Location = panelmats.Controls.BottomLeft };
            //    mat.Slot.CornerTextFunc = (sl) => item.Req.Max.ToString();
            //    panelmats.Controls.Add(mat);

            //    box.Controls.Add(panelproduct, panelmats);
            //}
            return;
        }

        public override void GetRightClickActions(GameObject parent, List<ContextAction> actions)
        {
            actions.Add(new ContextAction(() => "Examine", () =>
            {
                //parent.GetUi().Show();
                //new Workbench.WorkbenchInterface(parent).Show();

                Workbench.WorkbenchWindow.Instance.Refresh(parent).Show();
                //Workbench.WorkbenchWindow.Instance.Location = UIManager.Mouse;
                return;// true; true;
            }));
        }

        //internal override void GetAvailableTasks(GameObject parent, List<Interactions.Interaction> list)
        //{
        //    list.Add(new Insert(this.Slots));
        //}

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            // TODO: streamline this
            //if (Player.Actor.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling].Object != null)
            if (Player.Actor.GetComponent<HaulComponent>().GetObject() !=null)//.Slot.Object != null)

                tooltip.Controls.Add(new Label(GlobalVars.KeyBindings.PickUp.ToString() + ": Insert") { Location = tooltip.Controls.BottomLeft });
        }

        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> actions)
        {
            actions.Add(new PlayerInput(PlayerActions.Activate), new InteractionUse());
            //actions.Add(new PlayerInput(PlayerActions.Drop), new InteractionInsertMaterial(this.Slots));
            actions.Add(new PlayerInput(PlayerActions.Drop), new InteractionInsertMaterial(this.Storage));
        }

        class InteractionUse : Interaction
        {
            public InteractionUse()
            {
                this.Name = "Use";
            }
            static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                    new RangeCheck()));
            public override TaskConditions Conditions
            {
                get
                {
                    return conds;
                }
            }
            public override void Perform(GameObject a, TargetArgs t)
            {
                base.Perform(a, t);
                //if (a.Net is Net.Server)
                if (a.Net is Net.Client) //TODO: maybe raise an event and open the interface by handling it? (instead of referencing interface inside game logic)
                    Workbench.WorkbenchWindow.Instance.Refresh(t.Object).Show();
            }

            public override object Clone()
            {
                return new InteractionUse();
            }
        }
        class InteractionInsertMaterial : Interaction
        {
            Container Container;

            public InteractionInsertMaterial(Container container)
                : base(
                "Insert Item",
                0
                )
            {
                this.Container = container;
            }
            static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                    new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
                    new TargetTypeCheck(TargetType.Entity)));
            public override TaskConditions Conditions
            {
                get
                {
                    return conds;
                }
            }
            public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
            {
                //return GearComponent.GetSlot(actor, GearType.Hauling).HasValue;
                return actor.GetComponent<HaulComponent>().GetObject()!=null;//.Slot.HasValue;

            }

            public override void Perform(GameObject actor, TargetArgs target)
            {
                //var hauling = actor.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling];
                //var hauling = GearComponent.GetSlot(actor, GearType.Hauling);
                var hauling = actor.GetComponent<HaulComponent>().GetSlot();//.Slot;

                if (hauling.HasValue)
                    this.Container.InsertObject(hauling);
            }

            public override object Clone()
            {
                return new InteractionInsertMaterial(this.Container);
            }
        }
    }
}
