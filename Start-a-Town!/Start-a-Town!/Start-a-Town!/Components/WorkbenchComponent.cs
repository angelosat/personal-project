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
        public BlueprintCollection()
        {
            //Blueprints = new Dictionary<GameObject.Types, Blueprint>();
        }

        public void Add(Blueprint bp)
        {
            Add(bp.ProductID, bp);
        }
    }

    class WorkbenchComponent : Component// ContainerComponent// Component
    {
        public override string ComponentName
        {
            get
            {
                return "Workbench";
            }
        }
        byte MaterialCapacity { get { return (byte)this["MaterialCapacity"]; } set { this["MaterialCapacity"] = value; } }
        GameObjectSlot Product { get { return (GameObjectSlot)this["Product"]; } set { this["Product"] = value; } }
        GameObjectSlot Blueprint { get { return (GameObjectSlot)this["Blueprint"]; } set { this["Blueprint"] = value; } }
        ItemContainer BlueprintSlots { get { return (ItemContainer)this["BlueprintSlots"]; } set { this["BlueprintSlots"] = value; } }
        int Stage { get { return (int)this["Stage"]; } set { this["Stage"] = value; } }
        public ItemContainer Slots { get { return (ItemContainer)this["Slots"]; } set { this["Slots"] = value; } }


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
            //this.Slots = new ItemContainer(0);
            this.Stage = 0;
            Product = GameObjectSlot.Empty;
            Blueprint = GameObjectSlot.Empty;
            //BlueprintSlots = new ItemContainer(8);// List<GameObjectSlot>();
            //this.BlueprintSlots.Filter = o => o.HasComponent<BlueprintComponent>();
            this.MaterialCapacity = 1;
        }

        public WorkbenchComponent Initialize(GameObject parent, byte materialCapacity)
        {
            this.MaterialCapacity = materialCapacity;
            this.Slots = new ItemContainer(parent, materialCapacity);
            BlueprintSlots = new ItemContainer(parent, 8, o => o.HasComponent<BlueprintComponent>());
            return this;
        }

        static private List<GameObject> GenerateBlueprints()
        {
            List<GameObject> bps = new List<GameObject>();

            bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintHandle, GameObject.Types.Handle, new BlueprintStage() { { GameObject.Types.Twig, 1 } }));
            bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintPickaxeHead, GameObject.Types.PickaxeHead, new BlueprintStage() { { GameObject.Types.Stone, 1 } }));
            //bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintPickaxe, GameObject.Types.Pickaxe, new BlueprintStage() { { GameObject.Types.Handle, 1 }, { GameObject.Types.PickaxeHead, 1 } }));
            bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintShovelHead, GameObject.Types.ShovelHead, new BlueprintStage() { { GameObject.Types.Stone, 1 } }));
            //bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintShovel, GameObject.Types.Shovel, new BlueprintStage() { { GameObject.Types.Handle, 1 }, { GameObject.Types.ShovelHead, 1 } }));
            //bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintAxe, GameObject.Types.Axe, new BlueprintStage() { { GameObject.Types.AxeHead, 1 }, { GameObject.Types.Twig, 1 } }));
            //bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintHandsaw, GameObject.Types.Handsaw, new BlueprintStage() { { GameObject.Types.Cobble, 1 }, { GameObject.Types.Twig, 1 } }));
            bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintAxeHead, GameObject.Types.AxeHead, new BlueprintStage() { { GameObject.Types.Cobble, 1 } }));
            //bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintHammer, GameObject.Types.Hammer, new BlueprintStage() { { GameObject.Types.Twig, 1 }, { GameObject.Types.Cobble, 1 } }));
            //bps.Add(Start_a_Town_.Blueprint.Create(GameObject.Types.BlueprintFurnitureParts, GameObject.Types.FurnitureParts, new BlueprintStage() { { GameObject.Types.WoodenPlank, 2 } }));

            //bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintCobblestoneItem, GameObject.Types.CobblestoneItem, new BlueprintStage() { { GameObject.Types.Stone, 1 } }));
            //bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintWoodenDeck, GameObject.Types.WoodenDeck, new BlueprintStage() { { GameObject.Types.WoodenPlank, 1 } }));
            //bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintSoil, GameObject.Types.Soil, new BlueprintStage() { { GameObject.Types.Soilbag, 1 } }));            
            //bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintScaffold, GameObject.Types.Scaffolding, new BlueprintStage() { { GameObject.Types.WoodenPlank, 1 } }));
            //bps.Add(Start_a_Town_.Plan.Create(GameObject.Types.BlueprintDoor, GameObject.Types.Door, new BlueprintStage() { { GameObject.Types.WoodenPlank, 2 } }));

            //bps.Add(Start_a_Town_.Schematic.Create(GameObject.Types.BlueprintBed, GameObject.Types.Bed, new BlueprintStage(actor => ControlComponent.HasAbility(actor, Message.Types.Build)) { { GameObject.Types.FurnitureParts, 2 } }));
            //bps.Add(Start_a_Town_.Schematic.Create(GameObject.Types.BlueprintWorkbench, GameObject.Types.Workbench, new BlueprintStage() { { GameObject.Types.WoodenPlank, 2 } }));
            //bps.Add(Start_a_Town_.Schematic.Create(GameObject.Types.BlueprintCampfire, GameObject.Types.Campfire, new BlueprintStage() { { GameObject.Types.Log, 1 } }));

            return bps;
        }

        public override void GetChildren(List<GameObjectSlot> list)
        {
            list.AddRange(this.Slots);
            list.AddRange(this.BlueprintSlots);
        }
        public override void MakeChildOf(GameObject parent)
        {
            this.Slots = new ItemContainer(parent, this.MaterialCapacity);
            this.BlueprintSlots = new ItemContainer(parent, 8, o =>
            {
                return o.HasComponent<BlueprintComponent>();
            });
        }

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                case Message.Types.Activate: // TODO: refactor this for all components that have similar functionality (workbenches, furnaces, etc)
                    GameObject actor = e.Parameters[0] as GameObject;
                    //GameObjectSlot hauled = actor.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling];
                    GameObjectSlot hauled = actor.GetComponent<HaulComponent>().GetSlot();//.Slot;

                    if (hauled.HasValue)
                        this.Slots.InsertObject(e.Network, hauled);
                    return true;

                case Message.Types.AddProduct:
                    //parent.Global.GetChunk(e.Network.Map).Invalidate();//.Saved = true;
                    e.Network.Map.GetChunk(parent.Global).Invalidate();//.Saved = true;

                    e.Translate(r =>
                    {
                        GameObject prod= TargetArgs.Read(e.Network, r).Object;
                        //this.Slots.First(s=>!s.HasValue).Set(prod);
                        var tSlot = (from slot in this.Slots
                         where !slot.HasValue || (slot.HasValue && slot.Object.ID == prod.ID && slot.StackSize < slot.StackMax)
                         select slot).FirstOrDefault();
                        if (tSlot.IsNull())
                        {
                            e.Network.PopLoot(prod); 
                            return;
                        }
                        tSlot.SetObject(prod);
                    });
                    
                    return true;

                case Message.Types.Craft://Object:
                    GameObject.Types bpID = (GameObject.Types)e.Parameters[0];
                    GameObject crafter = e.Parameters[1] as GameObject;
                    GameObject bp = GameObject.Objects[bpID];
                    this.Craft(e.Network, parent, crafter, bp);
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

             //   case Message.Types.DropOn:
                //case Message.Types.Activate:
                //    GameObjectSlot haulSlot;
                //    if (!InventoryComponent.TryGetHeldObject(e.Sender, out haulSlot))
                //        return true;
                //    GameObject hauling = haulSlot.Object;
                //    this.Slots.Insert(haulSlot);
                //    //hauling.Remove(); // no need 

                //    throw new NotImplementedException();
                //    //GameObject.PostMessage(e.Sender, Message.Types.Dropped, parent, haulSlot, haulSlot.Object);
                //    return true;


                //    if (!GetFilter().Apply(hauling))
                //        return true;

                //    int reqAmount;
                //    if (!Blueprint.Object["Blueprint"].GetProperty<Blueprint>("Blueprint").Stages.First().TryGetValue(hauling.ID, out reqAmount))
                //        return true;
                //    GameObjectSlot matSlot;
                //    if (Slots.ToDictionary(foo => foo.Object.ID).TryGetValue(hauling.ID, out matSlot))
                //    {
                //        if (matSlot.StackSize == reqAmount)
                //            return true;
                //        matSlot.StackSize++;
                //    }
                //    else
                //        Slots.Add(new GameObjectSlot(hauling, 1));
                //    haulSlot.StackSize--;

                //    e.Network.Despawn(hauling);


                //    throw new NotImplementedException();
                //    //GameObject.PostMessage(e.Sender, Message.Types.Dropped, parent, haulSlot, haulSlot.Object);

                //    return true;

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
                    GameObjectSlot slott = BlueprintSlots.Find(foo => foo == e.Parameters[0] as GameObjectSlot);
                    if (slott.IsNull())
                        return true;

                    if (e.Sender == null)
                        //Loot.PopLoot(parent, slot.Object);
                        e.Network.PopLoot(slott.Object, parent.Global, parent.Velocity);
                    else
                        throw new NotImplementedException();
                        //e.Sender.PostMessage(Message.Types.Give, parent, slot);
                    BlueprintSlots.Remove(slott);

                    return true;

                case Message.Types.Clear:
                    foreach (var mat in Slots)
                        for (int i = 0; i < mat.StackSize; i++)
                            // Loot.PopLoot(parent, mat.Object);
                            e.Network.PopLoot(mat.Object, parent.Global, parent.Velocity);
                    Slots.Empty();// = new ItemContainer(Slots.Capacity);
                    return true;

                case Message.Types.SlotInteraction:
                    actor = e.Parameters[0] as GameObject;
                    GameObjectSlot sl = e.Parameters[1] as GameObjectSlot;
                    e.Network.PostLocalEvent(actor, Message.Types.Insert, sl);
                    return true;


                default:
                    return base.HandleMessage(parent, e);// false;
            }
        }

        private void Craft(Net.IObjectProvider net, GameObject parent, GameObject crafter, GameObject bp)
        {
            //BlueprintComponent bpComp = bp.GetComponent<BlueprintComponent>();
            if (!BlueprintComponent.MaterialsAvailable(bp, this.Slots))
                return;
            BlueprintComponent bpComp = bp.GetComponent<BlueprintComponent>();
            GameObject pr;
            bpComp.Blueprint.Craft(net, parent, this.Slots, parent, this.Slots, out pr);

            // award crafter with exp to crafting skill and knowledge of specific blueprint
            crafter.GetComponent<KnowledgeComponent>().Blueprints.Gain(crafter, bp, 1);
        }

        Dictionary<GameObject.Types, int> GetCurrentReqMaterials()
        {
            return Blueprint.Object["Blueprint"].GetProperty<Blueprint>("Blueprint").Stages[this.Stage];//Dictionary<GameObject.Types, int>>("CurrentMaterials");
        }


        public override void Query(GameObject parent, List<InteractionOld> list)//GameObjectEventArgs e)
        {
            list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.Activate, parent, "Craft"));
            list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.SetBlueprint, parent, "Set Blueprint"));
            list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.Retrieve, parent, "Retrieve Blueprint"));

            list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.Clear, parent, "Clear"));

            // GameObjectSlot obj = e.Parameters.ElementAtOrDefault(4) as GameObjectSlot;

            list.Add(new InteractionOld(TimeSpan.Zero, Message.Types.ApplyMaterial, parent, "Deposit"));

            if (CheckMaterials())
            {
                list.Add(new InteractionOld(new TimeSpan(0, 0, 1), Message.Types.Build, parent, "Craft",
                    //   effect: new InteractionEffect("Work"),
                    cond: new ConditionCollection(
                    new Condition((actor, target) => true, new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory)))));
                return;
            }
            //if (Blueprint.Object.ID == GameObject.Types.BlueprintBlank)
            //    return;
            if (!Blueprint.HasValue)
                return;
            ObjectFilter filter = GetFilter();

            list.Add(new InteractionOld(new TimeSpan(0, 0, 0, 1), Message.Types.FinishConstruction, parent, "Finish construction",  //TimeSpan.Zero
                // effect: new InteractionEffect("Work"),
                        cond:
                            new ConditionCollection(
                                new Condition((actor, target) => ControlComponent.HasAbility(actor, Message.Types.Build), "I need a tool to " + Message.Types.Build.ToString().ToLower() + " with.", //, "Requires: " + Message.Types.Build,
                                    new Precondition("Equipped", i => FunctionComponent.HasAbility(i.Source, Message.Types.Build), AI.PlanType.FindInventory)
                                ),
                            //),
                        //targetCond:
                            //new InteractionConditionCollection(
                                new Condition((actor, target) => Blueprint != null, "Blueprint not set!"),
                                new Condition((actor, target) => CheckMaterials(), "Materials missing!",
                                    new Precondition("Materials", i => i.Message == Message.Types.ApplyMaterial && i.Source == parent, AI.PlanType.FindNearest)
                                )
                            )
                        )
                    );

            list.Add(
                new InteractionOld(TimeSpan.Zero, Message.Types.DropOn, parent, "Insert") { SourceComponent = this }
                );
                    //targetCond: 
                    //new InteractionConditionCollection(
                    //    new InteractionCondition(
                    //        foo=>this.Materials.FindAll(bar=>!bar.hasva


            list.Add(new InteractionOld(new TimeSpan(0, 0, 0, 1), Message.Types.ApplyMaterial, parent, "Apply material",
                        effect: new NeedEffectCollection() { new AIAdvertisement("Materials") },
                        cond:
                            new ConditionCollection(
                //  new InteractionCondition(agent => InventoryComponent.IsHauling(agent, filter.Apply), "Requires materials!",
                                new Condition((actor, target) => InventoryComponent.IsHauling(actor, held => this.GetCurrentReqMaterials().ContainsKey(held.ID)), "Requires materials!",
                                    new Precondition("Holding", i => filter.Apply(i.Source), AI.PlanType.FindNearest),
                                    new Precondition("Production", i => ProductionComponent.CanProduce(i.Source, filter.Apply), AI.PlanType.FindNearest)
                                ),
                            //),
                        //targetCond:
                        //    new InteractionConditionCollection(
                                new Condition((actor, target) => !CheckMaterials(), "Materials already in place!")
                            )
                        )
                    );

            return;
        }

        public override void GetClientActions(GameObject parent, List<ContextAction> actions)
        {
            actions.Add(new ContextAction(() => "Interface", () =>
            {
                parent.GetUi().Show();
                return;// true; true;
            }));
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
            //return new WorkbenchComponent().Initialize(this.Slots.Capacity);
            WorkbenchComponent comp = new WorkbenchComponent();
            return comp.Initialize(this.Slots.Parent, this.Slots.Capacity);
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

            //data.Add(new SaveTag(SaveTag.Types.Compound, "Slots", this.Slots.ConvertAll<SaveTag>(foo => new SaveTag(SaveTag.Types.Compound, "", foo.SaveShallow()))));
            data.Add(new SaveTag(SaveTag.Types.Compound, "Slots", this.Slots.Save()));
            data.Add(new SaveTag(SaveTag.Types.Byte, "Capacity", this.Slots.Capacity));
            data.Add(new SaveTag(SaveTag.Types.Compound, "Blueprints", this.BlueprintSlots.Save()));
            return data;
        }
        internal override void Load(SaveTag compTag)
        {
            //this.Initialize(compTag.TagValueOrDefault<byte>("Capacity", 0));
            this.MaterialCapacity = compTag.TagValueOrDefault<byte>("Capacity", 2);

            //this.Initialize(compTag.TagOrDefault<ItemContainer>("Slots", save => ItemContainer.Load(save), new ItemContainer(MaterialCapacity)));
           // this.Slots = compTag.TagOrDefault<ItemContainer>("Slots", save => ItemContainer.Create(save), new ItemContainer(MaterialCapacity));
            this.Slots.Load(compTag["Slots"]);
            //compTag.TryGetTagValue<SaveTag>("Blueprints", t => this.BlueprintSlots.Load(t));
            compTag.TryGetTag("Blueprints", t => this.BlueprintSlots.Load(t));

            //this.Product.Object = compTag.TagValueOrDefault<int, GameObject>("Product", value => GameObject.Create(value), null);
            GameObjectSlot bp;
            if (!compTag.TagValueOrDefault<int, GameObjectSlot>("Blueprint", value => value != -1 ? GameObjectSlot.Create(value) : GameObjectSlot.Empty, GameObjectSlot.Empty, out bp))
                return;

            this.Blueprint = bp;
            if (!bp.HasValue)
                return;
            this.Product = Blueprint.HasValue ? new GameObjectSlot(GameObject.Create(Blueprint.Object["Blueprint"].GetProperty<Blueprint>("Blueprint").ProductID)) : GameObjectSlot.Empty;


            //this.Slots = compTag.TagValueOrDefault<byte, ItemContainer>("Capacity", c => new ItemContainer(c), new ItemContainer(0));
        
          
        }

        public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<GameEvent>> handlers)
        {
            GroupBox box_bps = new GroupBox();
            Panel panel_bplist = new Panel() { ClientDimensions = new Vector2(150 - BackgroundStyle.Panel.Border, 150) };// AutoSize = true };
            Panel panel_blueprint = new Panel() { Location = panel_bplist.TopRight, ClientDimensions = panel_bplist.ClientDimensions };
            panel_blueprint.Controls.Add(new Label() { Text = "No blueprint selected" });
            ListBox<GameObject, Button> list_bps = new ListBox<GameObject, Button>(new Rectangle(0, 0, 150, 150));
            List<GameObject> bplist;
            //Action refreshBpList = () =>
            //{
            //    bplist = new List<GameObject>();
            //    panel_bplist.Controls.Clear();
            //    if (bplist.Count == 0)
            //    {
            //        panel_bplist.Controls.Add(new Label() { Text = "No stored blueprints" });
            //        return;
            //    }
            //    list_bps.Build(bplist, foo => foo.Name, (foo, btn) =>
            //    {
            //        btn.LeftClickAction = () =>
            //        {
            //            panel_blueprint.Controls.Clear();
            //            GroupBox gbox = new GroupBox();
            //            ScrollableBox box = new ScrollableBox(panel_blueprint.ClientSize);
            //            foo.GetTooltip(gbox);
            //            box.Controls.Add(gbox);
            //            panel_blueprint.Controls.Add(box, new Button(new Vector2(0, panel_blueprint.ClientDimensions.Y), (int)panel_blueprint.ClientDimensions.X, "Clear")
            //            {
            //                Anchor = Vector2.UnitY,
            //                LeftClickAction = () =>
            //                {
            //                    panel_blueprint.Controls.Clear();
            //                    panel_blueprint.Controls.Add(new Label() { Text = "No blueprint selected" });
            //                }
            //            });
            //            Net.Client.PostPlayerInput(parent, Message.Types.SetBlueprint, w => w.Write((int)foo.ID));
            //        };
            //        panel_bplist.Controls.Add(list_bps);
            //    });
            //};
            
            //refreshBpList();
            box_bps.Controls.Add(panel_blueprint, panel_bplist);
            Label lbl_materials = "Materials".ToLabel();
            SlotGrid<SlotDefault> slots_mats = new SlotGrid<SlotDefault>(Slots, 4) { Location = lbl_materials.BottomLeft };//
            Label lbl_bps = "Stored Blueprints".ToLabel(slots_mats.BottomLeft);
            SlotGrid<SlotDefault> slots_bps = new SlotGrid<SlotDefault>(this.BlueprintSlots, 4) { Location = lbl_bps.BottomLeft };//
            //    , 4, slotInit: s =>
            //{
            //    s.DragDropAction = (args) =>
            //    {
            //        var a = args as DragDropSlot;
            //        Net.Client.PostPlayerInput(parent, Message.Types.ContainerOperation, w =>
            //        {
            //            ArrangeInventoryEventArgs.Write(w, new TargetArgs(a.Parent), new TargetArgs(a.Source.Object), new TargetArgs(a.Slot.Object), s.Tag.Container.ID, s.Tag.ID, (byte)a.Slot.StackSize);
            //        });
            //        return DragDropEffects.Move;
            //    };
            //}) { Location = lbl_materials.BottomLeft };


            //List<GameObject> finalList = GetAvailableBlueprints();

            var list = new ListBox<GameObject, Button>(new Rectangle(0, 0, 300, 200));
            Panel panel_Selected = new Panel();

            Action refreshBpList = () =>
            {
                list.Build(GetAvailableBlueprints(), foo => foo.Name,
                        RecipeListControlInitializer(panel_Selected));
            };
            refreshBpList();
            

            RadioButton
                rd_All = new RadioButton("All", Vector2.Zero, true)
                {
                    LeftClickAction = () =>
                    {
                        refreshBpList();
                        //list.Build(finalList, foo => foo.Name,
                        //    RecipeListControlInitializer(panel_Selected));
                    }
                },
                rd_MatsReady = new RadioButton("Have Materials", rd_All.TopRight)
                {
                    LeftClickAction = () =>
                    {
                        list.Build(GetAvailableBlueprints().FindAll(foo => BlueprintComponent.MaterialsAvailable(foo, this.Slots)), foo => foo.Name, RecipeListControlInitializer(panel_Selected));
                    }
                };
            Panel panel_filters = new Panel() { Location = slots_bps.BottomLeft, AutoSize = true };
            panel_filters.Controls.Add(rd_All, rd_MatsReady);

            rd_All.PerformLeftClick();

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

                    Net.Client.PostPlayerInput(Message.Types.StartScript, w =>
                    {
                        Ability.Write(w, Script.Types.CraftingWorkbench, new TargetArgs(parent), BitConverter.GetBytes((int)list.SelectedItem.ID));
                    });
                }
            };

            ui.Controls.Add(
                lbl_materials,
                slots_mats, 
                lbl_bps, slots_bps,
                panel_filters, panel_List, panel_Selected, btn_craft);

            //EventHandler<Net.GameEvent> refresh = (sender, e) => this.RefreshSelectedPanel(panel_Selected, list.SelectedItem);
            //Net.Client.Instance.GameEvent += refresh;
            //ui.GetWindow().HideAction = () =>
            //    { 
            //        Net.Client.Instance.GameEvent -= refresh;
            //    };

            //handlers.Add(new EventHandler<ObjectEventArgs>((sender, e) =>
            handlers.Add(new EventHandler<GameEvent>((sender, e) =>
            {
                if ((e.Parameters[0] as GameObject) != parent)
                    return;
                switch (e.Type)
                {
                    case Message.Types.InventoryChanged:
                        refreshBpList();
                        RefreshSelectedPanel(panel_Selected, list.SelectedItem);
                        return;

                    //case Message.Types.ArrangeInventory:
                    //    refreshBpList();
                    //    return;

                    //case Message.Types.Craft:
                    //    RefreshSelectedPanel(panel_Selected, list.SelectedItem);
                    //    return;

                    default:
                        return;
                }
            }));
        }

        private List<GameObject> GetAvailableBlueprints()
        {
            List<GameObject> recipes = KnowledgeComponent.GetMemorizedBlueprints(Player.Actor);// KnowledgeComponent.GetKnownRecipes(Player.Actor);

            List<GameObjectSlot> storedBps = this.BlueprintSlots.Where(o => o.HasValue).ToList();// new List<GameObjectSlot>();
            List<GameObject> finalList = recipes;//.Join(storedBps, o=>o, o=>o, (o,a)=>o, IEqualityComparer;
            var distinctBps = (from bp in storedBps
                               where recipes.FirstOrDefault(r => r.ID == bp.Object.ID).IsNull()
                               select bp.Object);
            finalList.AddRange(distinctBps);
            return finalList;
        }

        private Action<GameObject, Button> RecipeListControlInitializer(Panel panel_Selected)
        {
            return (foo, btn) =>
            {
                btn.LeftClickAction = () =>
                {
                    GameObject obj = foo;
                    RefreshSelectedPanel(panel_Selected, obj);
                    return;
                };
            };
        }

        private void RefreshSelectedPanel(Panel panel_Selected, GameObject obj)
        {
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
                this.Slots
                    .FindAll(s => s.HasValue)
                    .FindAll(s => s.Object.ID == mat.Key)
                    .ForEach(s => amount += s.StackSize);
                matSlot.Slot.CornerTextFunc = s => (amount.ToString() + "/" + mat.Value.ToString());
                panel_Selected.Controls.Add(matSlot);
            }
        }

        public override void Write(System.IO.BinaryWriter writer)
        {
            this.Slots.Write(writer);
            this.BlueprintSlots.Write(writer);
        }
        public override void Read(System.IO.BinaryReader reader)
        {
            this.Slots.Read(reader);
            this.BlueprintSlots.Read(reader);
        }
    }
}
