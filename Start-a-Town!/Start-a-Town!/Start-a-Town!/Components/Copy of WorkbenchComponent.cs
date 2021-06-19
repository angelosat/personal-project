using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Needs;

namespace Start_a_Town_.Components
{
    class BlueprintCollection : SortedDictionary<GameObject.Types, Blueprint>
    {
        //Dictionary<GameObject.Types, Blueprint> Blueprints;

        public BlueprintCollection()
        {
            //Blueprints = new Dictionary<GameObject.Types, Blueprint>();
        }

        public void Add(Blueprint bp)
        {
            Add(bp.ProductID, bp);
        }
    }

    class WorkbenchComponent : ContainerComponent// Component
    {
        public override string ComponentName
        {
            get
            {
                return "Workbench";
            }
        }

        GameObjectSlot Product { get { return (GameObjectSlot)this["Product"]; } set { this["Product"] = value; } }
        GameObjectSlot Blueprint { get { return (GameObjectSlot)this["Blueprint"]; } set { this["Blueprint"] = value; } }
        List<GameObjectSlot> BlueprintSlots { get { return (List<GameObjectSlot>)this["BlueprintSlots"]; } set { this["BlueprintSlots"] = value; } }
    //    List<GameObjectSlot> Slots { get { return (List<GameObjectSlot>)this["Materials"]; } set { this["Materials"] = value; } }
        int Stage { get { return (int)this["Stage"]; } set { this["Stage"] = value; } }

        static List<GameObject> _Blueprints;
        static public List<GameObject> Blueprints
        {
            get
            {
                if (_Blueprints == null)
                    _Blueprints = GenerateBlueprints();
                return _Blueprints;
            }
        }

        static public List<GameObject> LoadBlueprints()
        {
            return Blueprints;
        }

        public WorkbenchComponent()
        {
            this.Stage = 0;
            Product = GameObjectSlot.Empty;
            Blueprint = GameObjectSlot.Empty;
            BlueprintSlots = new List<GameObjectSlot>();
            this.Slots = new ItemContainer(0);

        }

        public WorkbenchComponent Initialize(byte materialCapacity)
        {
            //this.Stage = 0;
            //Product = GameObjectSlot.Empty;
            //Blueprint = GameObjectSlot.Empty;
            //BlueprintSlots = new List<GameObjectSlot>();
            this.Slots = new ItemContainer(materialCapacity);// new List<GameObjectSlot>(materialCapacity);
            return this;
        }
        WorkbenchComponent(byte materialCapacity)
        {
            this.Stage = 0;
            Product = GameObjectSlot.Empty;
            Blueprint = GameObjectSlot.Empty;
            BlueprintSlots = new List<GameObjectSlot>();
            // Materials = new List<GameObjectSlot>();
            this.Slots = new ItemContainer(materialCapacity);// new List<GameObjectSlot>(materialCapacity);
            //for (int i = 0; i < Materials.Capacity; i++)
            //    Materials.Add(GameObjectSlot.Empty);
        }
        static private List<GameObject> GenerateBlueprints()
        {
            List<GameObject> bps = new List<GameObject>();

            bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintHandle, GameObject.Types.Handle, new BlueprintStage() { { GameObject.Types.Twig, 1 } }));
            bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintPickaxeHead, GameObject.Types.PickaxeHead, new BlueprintStage() { { GameObject.Types.Stone, 1 } }));
            bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintPickaxe, GameObject.Types.Pickaxe, new BlueprintStage() { { GameObject.Types.Handle, 1 }, { GameObject.Types.PickaxeHead, 1 } }));
            bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintShovelHead, GameObject.Types.ShovelHead, new BlueprintStage() { { GameObject.Types.Stone, 1 } }));
            bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintShovel, GameObject.Types.Shovel, new BlueprintStage() { { GameObject.Types.Handle, 1 }, { GameObject.Types.ShovelHead, 1 } }));
            bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintAxe, GameObject.Types.Axe, new BlueprintStage() { { GameObject.Types.AxeHead, 1 }, { GameObject.Types.Twig, 1 } }));
            bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintHandsaw, GameObject.Types.Handsaw, new BlueprintStage() { { GameObject.Types.Cobble, 1 }, { GameObject.Types.Twig, 1 } }));
            bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintAxeHead, GameObject.Types.AxeHead, new BlueprintStage() { { GameObject.Types.Cobble, 1 } }));
            bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintHammer, GameObject.Types.Hammer, new BlueprintStage() { { GameObject.Types.Twig, 1 }, { GameObject.Types.Cobble, 1 } }));
            bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintFurnitureParts, GameObject.Types.FurnitureParts, new BlueprintStage() { { GameObject.Types.WoodenPlank, 2 } }));

            bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintCobblestone, GameObject.Types.Cobblestone, new BlueprintStage() { { GameObject.Types.Stone, 2 } }));
            bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintWoodenDeck, GameObject.Types.WoodenDeck, new BlueprintStage() { { GameObject.Types.WoodenPlank, 1 } }));
            bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintSoil, GameObject.Types.Soil, new BlueprintStage() { { GameObject.Types.Soilbag, 1 } }));            
            bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintScaffold, GameObject.Types.Scaffolding, new BlueprintStage() { { GameObject.Types.WoodenPlank, 1 } }));
            bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintDoor, GameObject.Types.Door, new BlueprintStage() { { GameObject.Types.WoodenPlank, 2 } }));

            bps.Add(Start_a_Town_.Schematic.Create(GameObject.Types.BlueprintBed, GameObject.Types.Bed, new BlueprintStage(actor => ControlComponent.HasAbility(actor, Message.Types.Build)) { { GameObject.Types.FurnitureParts, 2 } }));
            bps.Add(Start_a_Town_.Schematic.Create(GameObject.Types.BlueprintWorkbench, GameObject.Types.Workbench, new BlueprintStage() { { GameObject.Types.WoodenPlank, 2 } }));
            bps.Add(Start_a_Town_.Schematic.Create(GameObject.Types.BlueprintCampfire, GameObject.Types.Campfire, new BlueprintStage() { { GameObject.Types.Log, 1 } }));

            //bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintBed, GameObject.Types.Bed, new BlueprintStage(actor => ControlComponent.HasAbility(actor, Message.Types.Build)) { { GameObject.Types.FurnitureParts, 2 } }));
            //bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintWoodenDeck, GameObject.Types.WoodenDeck, new BlueprintStage() { { GameObject.Types.WoodenPlank, 1 } }));
            //bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintSoil, GameObject.Types.Soil, new BlueprintStage() { { GameObject.Types.Soilbag, 1 } }));
            //bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintWorkbench, GameObject.Types.Workbench, new BlueprintStage() { { GameObject.Types.WoodenPlank, 2 } }));
            //bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintScaffold, GameObject.Types.Scaffolding, new BlueprintStage() { { GameObject.Types.WoodenPlank, 1 } }));
            //bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintCampfire, GameObject.Types.Campfire, new BlueprintStage() { { GameObject.Types.Log, 1 } }));
            //bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintDoor, GameObject.Types.Door, new BlueprintStage() { { GameObject.Types.WoodenPlank, 2 } }));

            //bps.Add(Start_a_Town_.Schematic.Create(GameObject.Types.BlueprintCobblestone, GameObject.Types.Cobblestone, new BlueprintStage() { { GameObject.Types.Stone, 2 } }));

            return bps;
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Activate:
                    //throw new NotImplementedException();
                    //e.Sender.PostMessage(Message.Types.Interface, parent, ContainerComponent.GetContents(parent));
                    e.Network.PostLocalEvent(e.Parameters[0] as GameObject, ObjectEventArgs.Create(Message.Types.Interface, new object[] {parent}));
                    return true;


                case Message.Types.SetBlueprint:
                    // Blueprint = e.Parameters[0] as GameObjectSlot;
                    Blueprint = e.Parameters.ElementAtOrDefault(0) as GameObjectSlot;
                    if (Blueprint.IsNull())
                    {
                        throw new NotImplementedException();
                        //e.Sender.PostMessage(Message.Types.UISetBlueprint, parent);
                        return true;
                    }

                    //if (Blueprint.HasValue)
                    //    Product = new GameObjectSlot(GameObject.Create(Blueprint.Object["Blueprint"].GetProperty<Blueprint>("Blueprint").ProductID));
                    //else
                    //{
                    //    Product = GameObjectSlot.Empty;
                    //    foreach (GameObjectSlot mat in Materials.FindAll(foo => foo.HasValue))
                    //        Loot.PopLoot(parent, mat.Object);
                    //    Materials.Clear();
                    //}


                    return true;
                case Message.Types.Craft://Object:
                    GameObject bpObj = e.Parameters[0] as GameObject;
                    GameObjectSlot productSlot;
                    BlueprintComponent.Craft(e.Sender, bpObj, this.Slots, out productSlot);

                    //Blueprint bp = bpObj["Blueprint"]["Blueprint"] as Blueprint;
                    //if (!BlueprintComponent.MaterialsAvailable(bpObj, this.Materials))
                    //    return true;

                    //foreach (var material in bp.Stages[0])
                    //    this.Materials.Remove(material.Key, material.Value);

                    //Loot.PopLoot(parent, GameObject.Create(bp.ProductID));

                    return true;
                case Message.Types.FinishConstruction:
                    if (!Product.HasValue)
                        return true;

                    GameObject obj;
                    if (Product.Object.Components.ContainsKey("Packable"))
                    {
                        obj = GameObject.Create(GameObject.Types.Package);
                        throw new NotImplementedException();
                        //obj.PostMessage(Message.Types.SetContent, parent, Product.Object);
                    }
                    else
                        obj = GameObject.Create(Product.Object.ID);

                    //obj = Product.Object;
                    Vector3 g = new Vector3(0, 0, (float)parent["Physics"]["Height"]);

                    //Loot.PopLoot(parent, obj);
                    e.Network.PopLoot(obj, parent.Global, parent.Velocity);
                    Product.Object = GameObject.Create(Product.Object.ID);

                    return true;
                case Message.Types.DropOn:
                    GameObjectSlot haulSlot;
                    if (!InventoryComponent.TryGetHeldObject(e.Sender, out haulSlot))
                        return true;
                    GameObject hauling = haulSlot.Object;
                    this.Slots.Insert(haulSlot);
                    //hauling.Remove(); // no need 

                    throw new NotImplementedException();
                    //GameObject.PostMessage(e.Sender, Message.Types.Dropped, parent, haulSlot, haulSlot.Object);
                    return true;


                    if (!GetFilter().Apply(hauling))
                        return true;

                    int reqAmount;
                    if (!Blueprint.Object["Blueprint"].GetProperty<Blueprint>("Blueprint").Stages.First().TryGetValue(hauling.ID, out reqAmount))
                        return true;
                    GameObjectSlot matSlot;
                    if (Slots.ToDictionary(foo => foo.Object.ID).TryGetValue(hauling.ID, out matSlot))
                    {
                        if (matSlot.StackSize == reqAmount)
                            return true;
                        matSlot.StackSize++;
                    }
                    else
                        Slots.Add(new GameObjectSlot(hauling, 1));
                    haulSlot.StackSize--;


                   //hauling.Remove();
                    e.Network.Despawn(hauling);

                    throw new NotImplementedException();
                    //GameObject.PostMessage(e.Sender, Message.Types.Dropped, parent, haulSlot, haulSlot.Object);

                    return true;
                case Message.Types.Give:
                    GameObjectSlot objSlot = e.Parameters[0] as GameObjectSlot;
                    if (!objSlot.HasValue)
                        return true;
                    if ((objSlot.Object.Type != ObjectType.Blueprint) &&
                        (objSlot.Object.Type != ObjectType.Plan))
                        return true;
                    BlueprintSlots.Add(objSlot.Object.ToSlot());
                    objSlot.StackSize--;
                    return true;
                case Message.Types.Retrieve:
                    GameObjectSlot slot = BlueprintSlots.Find(foo => foo == e.Parameters[0] as GameObjectSlot);
                    if (slot.IsNull())
                        return true;

                    if (e.Sender == null)
                        //Loot.PopLoot(parent, slot.Object);
                        e.Network.PopLoot(slot.Object, parent.Global, parent.Velocity);
                    else
                        throw new NotImplementedException();
                        //e.Sender.PostMessage(Message.Types.Give, parent, slot);
                    BlueprintSlots.Remove(slot);

                    return true;

                case Message.Types.Clear:
                    foreach (var mat in Slots)
                        for (int i = 0; i < mat.StackSize; i++)
                            // Loot.PopLoot(parent, mat.Object);
                            e.Network.PopLoot(mat.Object, parent.Global, parent.Velocity);
                    Slots = new ItemContainer(Slots.Capacity);// new List<GameObjectSlot>();
                    return true;

                default:
                    return base.HandleMessage(parent, e);// false;
            }
        }

        Dictionary<GameObject.Types, int> GetCurrentReqMaterials()
        {
            return Blueprint.Object["Blueprint"].GetProperty<Blueprint>("Blueprint").Stages[this.Stage];//Dictionary<GameObject.Types, int>>("CurrentMaterials");
        }


        public override void Query(GameObject parent, List<Interaction> list)//GameObjectEventArgs e)
        {
            list.Add(new Interaction(TimeSpan.Zero, Message.Types.Activate, parent, "Craft"));
            list.Add(new Interaction(TimeSpan.Zero, Message.Types.SetBlueprint, parent, "Set Blueprint"));
            list.Add(new Interaction(TimeSpan.Zero, Message.Types.Retrieve, parent, "Retrieve Blueprint"));

            list.Add(new Interaction(TimeSpan.Zero, Message.Types.Clear, parent, "Clear"));

            // GameObjectSlot obj = e.Parameters.ElementAtOrDefault(4) as GameObjectSlot;

            list.Add(new Interaction(TimeSpan.Zero, Message.Types.ApplyMaterial, parent, "Deposit"));

            if (CheckMaterials())
            {
                list.Add(new Interaction(new TimeSpan(0, 0, 1), Message.Types.Build, parent, "Craft",
                    //   effect: new InteractionEffect("Work"),
                    cond: new InteractionConditionCollection(
                    new InteractionCondition((actor, target) => true, new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory)))));
                return;
            }
            //if (Blueprint.Object.ID == GameObject.Types.BlueprintBlank)
            //    return;
            if (!Blueprint.HasValue)
                return;
            ObjectFilter filter = GetFilter();

            list.Add(new Interaction(new TimeSpan(0, 0, 0, 1), Message.Types.FinishConstruction, parent, "Finish construction",  //TimeSpan.Zero
                // effect: new InteractionEffect("Work"),
                        cond:
                            new InteractionConditionCollection(
                                new InteractionCondition((actor, target) => ControlComponent.HasAbility(actor, Message.Types.Build), "I need a tool to " + Message.Types.Build.ToString().ToLower() + " with.", //, "Requires: " + Message.Types.Build,
                                    new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory)
                                ),
                            //),
                        //targetCond:
                            //new InteractionConditionCollection(
                                new InteractionCondition((actor, target) => Blueprint != null, "Blueprint not set!"),
                                new InteractionCondition((actor, target) => CheckMaterials(), "Materials missing!",
                                    new Precondition("Materials", i => i.Message == Message.Types.ApplyMaterial && i.Source == parent, AI.PlanType.FindNearest)
                                )
                            )
                        )
                    );

            list.Add(
                new Interaction(TimeSpan.Zero, Message.Types.DropOn, parent, "Insert") { SourceComponent = this }
                );
                    //targetCond: 
                    //new InteractionConditionCollection(
                    //    new InteractionCondition(
                    //        foo=>this.Materials.FindAll(bar=>!bar.hasva


            list.Add(new Interaction(new TimeSpan(0, 0, 0, 1), Message.Types.ApplyMaterial, parent, "Apply material",
                        effect: new NeedEffectCollection() { new NeedEffect("Materials") },
                        cond:
                            new InteractionConditionCollection(
                //  new InteractionCondition(agent => InventoryComponent.IsHauling(agent, filter.Apply), "Requires materials!",
                                new InteractionCondition((actor, target) => InventoryComponent.IsHauling(actor, held => this.GetCurrentReqMaterials().ContainsKey(held.ID)), "Requires materials!",
                                    new Precondition("Holding", i => filter.Apply(i.Source), AI.PlanType.FindNearest),
                                    new Precondition("Production", i => ProductionComponent.CanProduce(i.Source, filter.Apply), AI.PlanType.FindNearest)
                                ),
                            //),
                        //targetCond:
                        //    new InteractionConditionCollection(
                                new InteractionCondition((actor, target) => !CheckMaterials(), "Materials already in place!")
                            )
                        )
                    );

            return;
        }

        private ObjectFilter GetFilter()
        {
            ObjectFilter filter = new ObjectFilter(FilterType.Include);
            BlueprintStage stage = Blueprint.Object["Blueprint"].GetProperty<Blueprint>("Blueprint").Stages.First();
            foreach (KeyValuePair<GameObject.Types, int> mat in stage)
            {
                // int currentAmount;
                GameObjectSlot matSlot;
                if (Slots.FindAll(foo => foo.HasValue).ToDictionary(foo => foo.Object.ID).TryGetValue(mat.Key, out matSlot))
                    if (matSlot.StackSize >= mat.Value)
                        continue;
                List<GameObject.Types> existing;
                string type = GameObject.Objects[mat.Key].Type;
                if (!filter.TryGetValue(type, out existing))
                {
                    existing = new List<GameObject.Types>();
                    filter[type] = existing;
                }
                existing.Add(mat.Key);
            }
            return filter;
        }
        bool CheckMaterial(GameObject.Types objID)
        {
            Dictionary<GameObject.Types, GameObjectSlot> mats = Slots.FindAll(foo => foo.HasValue).ToDictionary(foo => foo.Object.ID);
            GameObjectSlot mat;
            if (!mats.TryGetValue(objID, out mat))
                return false;
            if (mat.StackSize < Blueprint.Object["Blueprint"].GetProperty<Blueprint>("Blueprint").Stages.First()[objID])
                return false;
            return true;
        }
        bool CheckMaterials()
        {
            //if (Blueprint.Object.ID == GameObject.Types.BlueprintBlank)
            //    return false;
            if (!Blueprint.HasValue)
                return false;
            Dictionary<GameObject.Types, GameObjectSlot> mats = Slots.FindAll(foo => foo.HasValue).ToDictionary(foo => foo.Object.ID);
            foreach (KeyValuePair<GameObject.Types, int> material in Blueprint.Object["Blueprint"].GetProperty<Blueprint>("Blueprint").Stages.First())
            {
                GameObjectSlot mat;
                if (!mats.TryGetValue(material.Key, out mat))
                    return false;
                if (mat.StackSize < material.Value)
                    return false;
            }
            return true;
        }

        public override object Clone()
        {
            return new WorkbenchComponent(this.Slots.Capacity);
        }

        //public override void GetTooltip(GameObject parent, UI.Control tooltip)
        //{
        //    tooltip.Controls.Add(new Label(tooltip.Controls.Last().BottomLeft, "Producing:"));
        //    tooltip.Controls.Add(

        //       new SlotWithText(tooltip.Controls.Last().BottomLeft) { Tag = Blueprint }
        //        );
        //    if (!Blueprint.HasValue)
        //        return;

        //    Panel panel_mats = new Panel(tooltip.Controls.Last().BottomLeft);
        //    panel_mats.AutoSize = true;
        //    Dictionary<GameObject.Types, GameObjectSlot> mats = Slots.FindAll(foo => foo.HasValue).ToDictionary(foo => foo.Object.ID);
        //    foreach (KeyValuePair<GameObject.Types, int> material in Blueprint.Object["Blueprint"].GetProperty<Blueprint>("Blueprint").Stages.First())
        //    {
        //        GameObjectSlot mat;
        //        int currentAmount = mats.TryGetValue(material.Key, out mat) ? mat.StackSize : 0;

        //        SlotWithText slot = new SlotWithText(panel_mats.Controls.Count > 0 ? panel_mats.Controls.Last().BottomLeft : Vector2.Zero).SetSlotText(currentAmount.ToString() + "/" + material.Value.ToString());
        //        slot.Tag = GameObject.Objects[material.Key].ToSlot();
        //        panel_mats.Controls.Add(slot);
        //    }
        //    if (panel_mats.Controls.Count > 0)
        //        tooltip.Controls.Add(panel_mats);
        //}
        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            tooltip.Controls.Add(new Label(tooltip.Controls.Last().BottomLeft, "Producing:"));
            tooltip.Controls.Add(

               new SlotWithText(tooltip.Controls.Last().BottomLeft) { Tag = Blueprint }
                );
            if (!Blueprint.HasValue)
                return;

            Panel panel_mats = new Panel(tooltip.Controls.Last().BottomLeft);
            panel_mats.AutoSize = true;
            //Dictionary<GameObject.Types, GameObjectSlot> mats = Slots.FindAll(foo => foo.HasValue).ToDictionary(foo => foo.Object.ID);
            foreach (KeyValuePair<GameObject.Types, int> material in Blueprint.Object["Blueprint"].GetProperty<Blueprint>("Blueprint").Stages[0])
            {
              //  GameObjectSlot mat;
                int currentAmount = 0;
                Slots
                    .FindAll(foo => foo.HasValue)
                    .FindAll(foo => foo.Object.ID == material.Key)
                    .ForEach(foo => currentAmount += foo.StackSize);

                SlotWithText slot = new SlotWithText(panel_mats.Controls.Count > 0 ? panel_mats.Controls.Last().BottomLeft : Vector2.Zero).SetSlotText(currentAmount.ToString() + "/" + material.Value.ToString());
                slot.Tag = GameObject.Objects[material.Key].ToSlot();
                panel_mats.Controls.Add(slot);
            }
            if (panel_mats.Controls.Count > 0)
                tooltip.Controls.Add(panel_mats);
        }
        internal override List<SaveTag> Save()
        {
            List<SaveTag> data = new List<SaveTag>();
            data.Add(new SaveTag(SaveTag.Types.Int, "Blueprint", Blueprint.HasValue ? (int)this.Blueprint.Object.ID : -1));
            data.Add(new SaveTag(SaveTag.Types.Compound, "Materials", this.Slots.ConvertAll<SaveTag>(foo => new SaveTag(SaveTag.Types.Compound, "", foo.SaveShallow()))));
            //foreach (var mat in this.Materials)
            //    data.Add(new Tag(Tag.Types.Int, "Material", Product.HasValue ? (int)this.Product.Object.ID : -1));
            data.Add(new SaveTag(SaveTag.Types.Byte, "Capacity", this.Slots.Capacity));
            return data;
        }

        internal override void Load(SaveTag compTag)
        {
            //this.Product.Object = compTag.TagValueOrDefault<int, GameObject>("Product", value => GameObject.Create(value), null);
            GameObjectSlot bp;
            if (!compTag.TagValueOrDefault<int, GameObjectSlot>("Blueprint", value => value != -1 ? GameObjectSlot.Create(value) : GameObjectSlot.Empty, GameObjectSlot.Empty, out bp))
                return;

            this.Blueprint = bp;
            if (!bp.HasValue)
                return;
            this.Product = Blueprint.HasValue ? new GameObjectSlot(GameObject.Create(Blueprint.Object["Blueprint"].GetProperty<Blueprint>("Blueprint").ProductID)) : GameObjectSlot.Empty;

            //this.Materials = compTag.TagValueOrDefault<int, List<GameObjectSlot>>("Capacity", c => new List<GameObjectSlot>(c), new List<GameObjectSlot>(4));
            //for (int i = 0; i < this.Materials.Capacity; i++)
            //    this.Materials.Add(GameObjectSlot.Empty);

            this.Slots = compTag.TagValueOrDefault<byte, ItemContainer>("Capacity", c => new ItemContainer(c), new ItemContainer(0));
        }

        public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<ObjectEventArgs>> handlers)
        {
            //base.GetUI(parent, ui, handlers);

            GroupBox box_bps = new GroupBox();
            Panel panel_bplist = new Panel() { ClientDimensions = new Vector2(150 - BackgroundStyle.Panel.Border, 150) };// AutoSize = true };
            Panel panel_blueprint = new Panel() { Location = panel_bplist.TopRight, ClientDimensions = panel_bplist.ClientDimensions };
            panel_blueprint.Controls.Add(new Label() { Text = "No blueprint selected" });
            ListBox<GameObject, Button> list_bps = new ListBox<GameObject, Button>(new Rectangle(0, 0, 150, 150));
            List<GameObject> bplist; //= ContainerComponent.GetContents(parent).FindAll(foo => foo.HasValue).Select(foo => foo.Object).ToList();
            Action refreshBpList = () =>
            {
                bplist = ContainerComponent.GetContents(parent).FindAll(foo => foo.HasValue).Select(foo => foo.Object).ToList();
                panel_bplist.Controls.Clear();
                if (bplist.Count == 0)
                {
                    panel_bplist.Controls.Add(new Label() { Text = "No stored blueprints" });
                    return;
                }
                list_bps.Build(bplist, foo => foo.Name, (foo, btn) =>
                {
                    btn.LeftClickAction = () =>
                    {
                        panel_blueprint.Controls.Clear();
                        GroupBox gbox = new GroupBox();
                        ScrollableBox box = new ScrollableBox(panel_blueprint.ClientSize);
                        foo.GetTooltip(gbox);
                        box.Controls.Add(gbox);
                        panel_blueprint.Controls.Add(box, new Button(new Vector2(0, panel_blueprint.ClientDimensions.Y), (int)panel_blueprint.ClientDimensions.X, "Clear")
                        {
                            Anchor = Vector2.UnitY,
                            LeftClickAction = () =>
                            {
                                panel_blueprint.Controls.Clear();
                                panel_blueprint.Controls.Add(new Label() { Text = "No blueprint selected" });
                                throw new NotImplementedException();
                                //parent.PostMessage(Message.Types.SetBlueprint, Player.Actor, GameObjectSlot.Empty);
                            }
                        });
                        //this.Blueprint.Object = foo;
                        // send a begininteraction to player instead
                        throw new NotImplementedException();
                        //parent.PostMessage(Message.Types.SetBlueprint, Player.Actor, foo.ToSlot());
                    };
                    panel_bplist.Controls.Add(list_bps);
                });
            };
            refreshBpList();
            box_bps.Controls.Add(panel_blueprint, panel_bplist);

            Label lbl_materials = "Materials:".ToLabel();
            SlotGrid slots_mats = new SlotGrid(Slots, parent, 4) { Location = lbl_materials.BottomLeft };
            //SlotGrid<Slot> slots_mats = new SlotGrid<Slot>(Materials, parent, 4, (s) =>
            //{
            //    s.DragDropAction = (a) =>
            //    {

            //        return DragDropEffects.None;
            //    };
            //}) { Location = box_bps.BottomLeft };

            List<GameObject> recipes = MemoryComponent.GetKnownRecipes(Player.Actor);

            var storedBps = ContainerComponent.GetContents(parent);
            List<GameObject> finalList = recipes;//.Union(storedBps.Select(foo => foo.Object)).ToList();

            var list = new ListBox<GameObject, Button>(new Rectangle(0, 0, 300, 200));

            Panel panel_Selected = new Panel();// { Location = panel_List.BottomLeft, Size = panel_List.Size };

            RadioButton
                rd_All = new RadioButton("All", Vector2.Zero, true)
                {
                    LeftClickAction = () =>
                    {
                        list.Build(finalList, foo => foo.Name,
                            (foo, btn) =>
                            {
                                btn.LeftClickAction = () =>
                                {
                                    //panel_Selected.Controls.Clear();
                                    //ScrollableBox box = new ScrollableBox(new Rectangle(0, 0, 300, 200));
                                    //GroupBox gbox = new GroupBox();
                                    //foo.GetTooltip(gbox);
                                    //box.Add(gbox);
                                    //panel_Selected.Controls.Add(box);

                                    GameObject obj = foo;
                                    panel_Selected.Controls.Clear();
                                    if (obj.IsNull())
                                        return;
                                    Blueprint bp = obj["Blueprint"]["Blueprint"] as Blueprint;
                                    GameObject product = GameObject.Objects[bp.ProductID];

                                    panel_Selected.Controls.Add("Product:".ToLabel(panel_Selected.Controls.BottomLeft));
                                    panel_Selected.Controls.Add(new SlotWithText(panel_Selected.Controls.BottomLeft) { Tag = product.ToSlot() });
                                    panel_Selected.Controls.Add("Materials:".ToLabel(panel_Selected.Controls.BottomLeft));
                                    foreach (var mat in bp.Stages[0])
                                    {
                                        SlotWithText matSlot = new SlotWithText(panel_Selected.Controls.BottomLeft) { Tag = GameObject.Objects[mat.Key].ToSlot() };
                                        int amount = 0;
                                        InventoryComponent.GetSlots(Player.Actor)
                                            .FindAll(s => s.HasValue)
                                            .FindAll(s => s.Object.ID == mat.Key)
                                            .ForEach(s => amount += s.StackSize);
                                        matSlot.Slot.CornerTextFunc = s => (amount.ToString() + "/" + mat.Value.ToString());
                                        panel_Selected.Controls.Add(matSlot);
                                    }
                                };
                            });
                    }
                },
                rd_MatsReady = new RadioButton("Have Materials", rd_All.TopRight) { LeftClickAction = () => { list.Build(finalList.FindAll(foo => BlueprintComponent.MaterialsAvailable(foo, this.Slots)), foo => foo.Name); } };
            Panel panel_filters = new Panel() { Location = slots_mats.BottomLeft, AutoSize = true };
            panel_filters.Controls.Add(rd_All, rd_MatsReady);

            rd_All.PerformLeftClick();

            //list.Build(finalList, foo => foo.Name);
            Panel panel_List = new Panel() { Location = panel_filters.BottomLeft, AutoSize = true };
            panel_List.Controls.Add(list);

            panel_Selected.Location = panel_List.BottomLeft;
            panel_Selected.Size = panel_List.Size;

            Button btn_craft = new Button(panel_Selected.BottomLeft, panel_List.Width, "Craft")// { LeftClickAction = () => parent.PostMessage(Message.Types.FinishConstruction, Player.Actor) };
            {
                LeftClickAction = () =>
                {
                    if (list.SelectedItem.IsNull())
                        return;
                    throw new NotImplementedException();
                    //AbilityComponent.Perform(Script.Types.Crafting, new ActionArgs(Player.Actor, parent, Vector3.Zero, list.SelectedItem));// list.SelectedItem));



                   // Player.Actor.PostMessage(Message.Types.BeginInteraction, null,
                   //     new Interaction(TimeSpan.FromSeconds(1), Message.Types.CraftObject, parent, "Finish construction",
                   //     cond:
                   //         new InteractionConditionCollection(
                   //             new InteractionCondition((actor, target) => ControlComponent.HasAbility(actor, Message.Types.Build), "I need a tool to " + Message.Types.Build.ToString().ToLower() + " with.", //, "Requires: " + Message.Types.Build,
                   //                 new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory)
                   //             ),
                   //         //),
                   //     //targetCond:
                   //     //    new InteractionConditionCollection(
                   //             new InteractionCondition((actor, target) => Blueprint != null, "Blueprint not set!"),
                   //             new InteractionCondition((actor, target) => BlueprintComponent.MaterialsAvailable(list.SelectedItem, this.Slots), "Materials missing!",
                   //                 new Precondition("Materials", i => i.Message == Message.Types.ApplyMaterial && i.Source == parent, AI.PlanType.FindNearest)
                   //             )
                   //        )
                   //    ),
                   //    list.SelectedItem
                   //);
                }
            };

            ui.Controls.Add(//box_bps,
                lbl_materials,
                slots_mats, 
                panel_filters, panel_List, panel_Selected, btn_craft);

            //GameObject.MessageHandled += 
            handlers.Add(new EventHandler<ObjectEventArgs>((sender, e) =>
            {
                
                switch (e.Type)
                {
                    case Message.Types.ArrangeInventory:
                        refreshBpList();
                        return;
                    default:
                        return;
                }
            }));
        }
    }
}
