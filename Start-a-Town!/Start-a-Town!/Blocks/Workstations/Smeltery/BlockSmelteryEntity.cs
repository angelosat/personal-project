using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Crafting.Smelting;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;
using Start_a_Town_.Net;
using Start_a_Town_.Crafting;
using Start_a_Town_.Towns.Crafting;
using Start_a_Town_.Tokens;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Blocks.Smeltery
{
    public class BlockSmelteryEntityNew : BlockEntity
    {
        public BlockSmelteryEntityNew()
        {
            this.Comps.Add(new BlockEntityCompWorkstation(IsWorkstation.Types.Smeltery));
            this.Comps.Add(new BlockEntityCompDeconstructible());
            this.Comps.Add(new EntityCompRefuelable());
        }
        public override object Clone()
        {
            return new BlockSmelteryEntityNew();
        }
    }


    public class BlockSmelteryEntity : BlockEntityWorkstation// BlockEntity, IBlockEntityWorkstation
        {
            //public Container Input
            //{
            //    get { return this.Input; }
            //}
            public override IsWorkstation.Types Type { get { return IsWorkstation.Types.Smeltery; } }

            public BlockSmelteryEntity()
            {
                //this.Power = new Progress();
                this.Comps.Add(new EntityCompRefuelable());
            }
            public enum States { Stopped, Running }
            public Progress Power { get { return this.GetComp<EntityCompRefuelable>().Fuel; } }
            public Progress SmeltProgress;
            States _State;
            public States State
            {
                get { return this._State; }
                set
                {
                    this._State = value;
                    //if (this.Parent != null)
                    //    if (this.Parent.Net != null)
                    //        this.Parent.Net.EventOccured(Message.Types.BlockEntityStateChanged, this.Parent);
                }
            }

            
            //public override Container GetMaterialsContainer()
            //{
            //    return this.Storage;
            //}
            

            public CraftOperation SelectedProduct;

            public override Container Input { get { return this.Storage; } }
            public Container Storage, Output, Fuels;

            public BlockSmelteryEntity(int inCapacity, int outCapacity, int fuelCapacity):this()
            {
                this.State = States.Stopped;
                //this.Power = new Progress();// { Min = 0, Max = 100, Value = 0 }; //0;
                this.SmeltProgress = new Progress() { Max = 1 }; // TODO: make max relative to ore material melting point
                //this.CraftProgress = new Progress();

                this.Fuels = new Container(fuelCapacity) { Name = "Fuel", Filter = item => item.Body.Material.Fuel.Value > 0 };// MaterialsComponent.IsFuel(item) };
                this.Storage = new Container(inCapacity) { Name = "Input" };//, Filter = item => item.GetInfo().ItemSubType == ItemSubType.Ore };
                this.Output = new Container(outCapacity) { Name = "Output" };
            }


            public override void Break(IMap map, Vector3 global)
            {
                foreach (var slot in this.Storage.GetNonEmpty())
                    map.Net.PopLoot(slot.Object, global, Vector3.Zero);
                foreach (var slot in this.Output.GetNonEmpty())
                    map.Net.PopLoot(slot.Object, global, Vector3.Zero);
                foreach (var slot in this.Fuels.GetNonEmpty())
                    map.Net.PopLoot(slot.Object, global, Vector3.Zero);
            }

          

            //public override void Remove(IMap map, Vector3 global)
            //{
            //    foreach (var slot in this.Storage.GetNonEmpty())
            //        map.GetNetwork().PopLoot(slot.Object, global, Vector3.Zero);
            //    foreach (var slot in this.Output.GetNonEmpty())
            //        map.GetNetwork().PopLoot(slot.Object, global, Vector3.Zero); 
            //    foreach (var slot in this.Fuels.GetNonEmpty())
            //        map.GetNetwork().PopLoot(slot.Object, global, Vector3.Zero);
            //}

            public override void Tick(IObjectProvider net, Vector3 global)
            {

                //ConsumePower();
                //if (this.State == States.Stopped)
                //{
                //    Cooldown();
                //    return;
                //};

                //if (NoFuel)
                //{
                //    this.ConsumeFuel();
                //}

                //if (!NoFuel)
                //    if (this.SelectedProduct != null)
                //    {
                //        if (!this.MaterialsAvailable())
                //            this.Stop(net.Map, global);
                //        Smelt();
                //        if (SmeltingFinished)
                //        {
                //            this.SmeltProgress.Value = 0;
                //            this.Finish(net.Map, global);
                //        }
                //    }
            }

            //private void AdvertiseCurrentOrders(IObjectProvider net)
            //{
            //    if(this.ExecutingOrders)
            //    foreach(var order in this.PendingOrders)//.Where(o=>o.ID == 0))
            //        net.Map.GetTown().CraftingManager.AddOrder(order);
            //}

            
            
            #region Helpers
            private bool SmeltingFinished
            {
                get { return this.SmeltProgress.Value >= this.SmeltProgress.Max; }
            }
            private void Smelt()
            {
                this.SmeltProgress.Value += 0.01f;
            }
            private void ConsumePower()
            {
                this.Power.Value -= 0.001f;
            }
            private void Cooldown()
            {
                this.SmeltProgress.Value -= 0.005f;
            }
            private bool NoFuel
            {
                get { return this.Power.Value <= 0; }
            }
            #endregion

            public override GameObjectSlot GetChild(string containerName, int slotID)
            {
                var dic = new Dictionary<string, Container>() {
                    {this.Storage.Name, this.Storage},
                    {this.Output.Name, this.Output},
                    {this.Fuels.Name, this.Fuels}
                };

                var slot = dic[containerName].GetSlot(slotID);
                return slot;
            }
            //public override List<GameObjectSlot> GetChildren()
            //{
            //    var children = this.Storage.Slots.Concat(this.Output.Slots).Concat(this.Fuels.Slots)
            //        //.Concat(this.Input.Slots)
            //        .ToList();
            //    return children;
            //}

            public bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
            {
                switch (e.Type)
                {
                    case Message.Types.Insert:
                        GameObjectSlot toInsert = e.Parameters[0] as GameObjectSlot;
                        if (!toInsert.HasValue)
                            throw new Exception("Null item");

                        Container target;
                        if (this.Storage.Filter(toInsert.Object))
                            target = this.Storage;
                        else if (this.Fuels.Filter(toInsert.Object))
                            target = this.Fuels;
                        else
                            return true;

                        target.Slots.Insert(toInsert);
                        return true;


                    case Message.Types.SlotInteraction:
                        var actor = e.Parameters[0] as GameObject;
                        var slot = e.Parameters[1] as GameObjectSlot;
                        e.Network.PostLocalEvent(actor, Message.Types.Insert, slot);
                        return true;

                    case Message.Types.Activate:
                        throw new NotImplementedException();

                    case Message.Types.ArrangeInventory:
                        GameObjectSlot
                            sourceSlot = e.Parameters[0] as GameObjectSlot,
                            targetSlot = e.Parameters[1] as GameObjectSlot;
                        sourceSlot.Swap(targetSlot);
                        return true;

                    //case Message.Types.Craft:
                    //    if (!Materials.HasValue)
                    //        return true;
                    //    Recipe recipe = GetRecipe();
                    //    if (recipe.IsNull())
                    //        return true;

                    //    if (!this.Fuel.HasValue)
                    //        return true;
                    //    this.Power.Value += FuelComponent.GetPower(this.Fuel.Object);
                    //    this.Fuel.Clear();
                    //    this.State = States.Running;

                    //    return true;

                    default:
                        return false;
                }

            }
            internal override void HandleRemoteCall(IMap map, Vector3 global, ObjectEventArgs e)
            {
                var net = map.Net;
                switch (e.Type)
                {
                    case Message.Types.Start:
                        if (this.State == States.Stopped)
                            this.Start(map, global);
                        else
                            this.Stop(map, global);
                        break;

                    //case Message.Types.Retrieve:
                    //    e.Data.Translate(parent.Net, r =>
                    //    {
                    //        int childID = r.ReadByte();// r.ReadInt32();
                    //        GameObjectSlot slot = parent.GetChild(childID);
                    //        if (!slot.HasValue)
                    //            return;
                    //        parent.Net.PopLoot(slot.Object, parent);
                    //        // TODO: haul retrieved item instead of popping it
                    //        slot.Clear();
                    //    });
                    //    break;

                    case Message.Types.SetProduct:
                        e.Data.Translate(net, r =>
                        {
                            var reactionID = r.ReadInt32();
                            var matCount = r.ReadInt32();
                            var mats = new List<ItemRequirement>();
                            for (int i = 0; i < matCount; i++)
                                mats.Add(new ItemRequirement(r));
                            var reaction = Reaction.Dictionary[reactionID];
                            var product = reaction.Products.First().GetProduct(reaction, null, mats);
                            var craft = new CraftOperation(reactionID, mats, null, null, this.Storage);
                            this.SelectedProduct = craft;

                            //var craft = new Start_a_Town_.Components.Crafting.CraftOperation(net, r);
                            //var reaction = Reaction.Dictionary[craft.ReactionID];
                            //var product = reaction.Products.First().GetProduct(reaction, null, craft.Materials);
                            //this.SelectedProduct = craft;
                        });
                        break;

                    case Message.Types.AddProduct:
                        e.Data.Translate(net, r =>
                        {
                            //var output = GameObject.CreatePrefab(r);
                            var output = net.GetNetworkObject(r.ReadInt32());
                            this.Out(output);
                        });
                        break;

                    //case Message.Types.Finish:
                    //    this.Finish(parent);
                    //    break;


                    case Message.Types.PlayerSlotRightClick:
                        //var actor = e.Parameters[0] as GameObject;
                        //var child = e.Parameters[1] as GameObject;
                        var actor = e.Sender;
                        var child = e.Parameters[0] as GameObject;
                        //var found = this.Input.Slots.Concat(this.Output.Slots).Concat(this.Fuels.Slots).FirstOrDefault(s => s.Object == child);
                        var found = this.Fuels.Slots.Concat(this.Output.Slots).Concat(this.Storage.Slots).FirstOrDefault(s => s.Object == child);
                        actor.Net.PostLocalEvent(actor, Message.Types.Insert, found);
                        break;

                    //case Message.Types.Craft:
                    //    e.Data.Translate(net, r =>
                    //    {
                    //        this.Craft(map.Net.GetNetworkObject(r.ReadInt32()), r.ReadVector3(), new CraftOperation(net, r));
                    //    });
                    //    break;

                    default:
                        break;
                }
            }

            //private void Craft(Actor actor, Vector3 global, CraftOperation crafting)
            //{
            //    var workstation = actor.Map.GetBlockEntity(global) as BlockSmelteryEntity;
            //    var reaction = Components.Crafting.Reaction.Dictionary[crafting.ReactionID];
            //    if (reaction == null)
            //        return;
            //    var product = reaction.Products.First().GetProduct(reaction, crafting.Building.Object, crafting.Materials, crafting.Tool);
            //    if (product == null)
            //        return;
            //    if (product.Tool != null)
            //        GearComponent.Equip(actor, PersonalInventoryComponent.FindFirst(actor, foo => foo == product.Tool));
            //    throw new Exception();
            //    //actor.GetComponent<WorkComponent>().Perform(actor, new InteractionCraft(product, workstation, crafting.WorkstationEntity), new TargetArgs(global));// crafting.Building);
            //}

            private void Stop(IMap map, Vector3 global)// GameObject parent)
            {
                this.State = States.Stopped;
                //parent.Map.SetBlockLuminance(parent.Global, 0);
                map.SetBlockLuminance(global, 0);
            }

            private void Start(IMap map, Vector3 global)// GameObject parent)
            {
                if (this.SelectedProduct == null)
                    return;
                if (!this.MaterialsAvailable())
                    return;
                //this.ConsumeFuel();
                this.State = States.Running;
                //parent.Map.SetBlockLuminance(parent.Global, 3);
                map.SetBlockLuminance(global, 3);
            }

            private bool MaterialsAvailable()
            {
                foreach (var mat in this.SelectedProduct.Materials)//.Requirements)
                    if (this.Storage.Slots.GetAmount(obj => obj.GetID() == mat.ObjectID) < mat.AmountRequired)
                        return false;
                return true;
            }

            private void ConsumeFuel()
            {
                var found = (from slot in this.Fuels.GetNonEmpty()
                             //where slot.Object.HasComponent<FuelComponent>() 
                             where MaterialsComponent.IsFuel(slot.Object)
                             select slot).FirstOrDefault();
                if (found == null)
                    return;
                this.Power.Max = 0;
                foreach (var p in from p in found.Object.GetComponent<MaterialsComponent>().Parts.Values select p.Material.Fuel.Value)
                    this.Power.Value = this.Power.Max += p;
                found.Consume();
            }


            private void Finish(IMap map, Vector3 global)// GameObject parent)
            {
                if (this.SelectedProduct == null)
                    return;
                if (!this.MaterialsAvailable())
                    return;
                //this.SelectedProduct.Product.Clone();
                //this.Output.InsertObject(this.SelectedProduct.Product.Clone().ToSlot());
                var net = map.Net;
                var reaction = Reaction.Dictionary[this.SelectedProduct.ReactionID];
                var productFactory = reaction.Products.First().GetProduct(reaction, null, this.SelectedProduct.Materials);
                var product = productFactory.Product;

                productFactory.ConsumeMaterials(net, this.Storage.Slots);

                // first existing
                var existing = (from slot in this.Output.GetNonEmpty()
                                where slot.Object.IDType == product.IDType
                                where slot.Object.StackSize < slot.Object.StackMax
                                select slot).FirstOrDefault();
                if (existing != null)
                {
                    existing.StackSize++;
                    return;
                }
                var empty = this.Output.GetEmpty().FirstOrDefault();
                if (empty != null)
                {
                    //net.Spawn(product, empty);
                    var server = net as Server;
                    if (server == null)
                        return;
                    //var data = Network.Serialize(w =>
                    //{
                    //    new TargetArgs(global).Write(w);
                    //    w.Write((int)Message.Types.AddProduct);
                    //    product.Write(w);
                    //});
                    server.SyncInstantiate(product);
                    var data = RPCargs.Create(new TargetArgs(global), Message.Types.AddProduct, w => w.Write(product.RefID));// product.Write(w));
                    server.RemoteProcedureCall(data, global);
                    this.Out(product);
                    return;
                }
                //parent.Net.PopLoot(this.SelectedProduct.Product.Clone(), parent);
                net.PopLoot(product, global + Vector3.UnitZ, Vector3.Zero);
                return;
            }

            public override object Clone()
            {
                return new BlockSmelteryEntity(this.Storage.Capacity, this.Output.Capacity, this.Fuels.Capacity);
            }

            void Out(GameObject item)
            {
                var empty = this.Output.GetEmpty().FirstOrDefault();
                empty.Object = item;
            }

            //public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<Net.GameEvent>> gameEventHandlers)
            //{
            //    var smeltUI = new SmeltingInterfaceNew();
            //    smeltUI.Refresh(parent);
            //    ui.Controls.Add(smeltUI);
            //}

            //public override void GetRightClickActions(GameObject parent, List<ContextAction> actions)
            //{
            //    actions.Add(new ContextAction(() => "Examine", () => parent.GetUi().Show()));
            //}

            //public bool Insert(GameObject material)
            //{
            //    return this.Storage.InsertObject(material);
            //}
            

            //public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> actions)
            //{
            //    actions.Add(PlayerInput.Activate, new InteractionActivate(parent));
            //    actions.Add(PlayerInput.ActivateHold, new InteractionInsert(parent));
            //}
            static public void GetPlayerActionsWorld(Dictionary<PlayerInput, Interaction> actions)
            {
                actions.Add(PlayerInput.Activate, new InteractionActivate());
                actions.Add(PlayerInput.ActivateHold, new InteractionInsert());
            }

            //public override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera cam, Vector3 global)
            //{
            //    //Bar.Draw(sb, cam, global + Vector3.UnitZ, "", this.CraftProgress.Percentage, cam.Zoom * .2f);
            //    var craft = this.GetCurrentOrder();
            //    if (craft != null)
            //        Bar.Draw(sb, cam, global + Vector3.UnitZ, "", craft.CraftProgress.Percentage, cam.Zoom * .2f);

            //    ////Rectangle bounds = camera.GetScreenBounds(global, parent.GetSprite().GetBounds());
            //    //Vector2 scrLoc = cam.GetScreenPositionFloat(global);// new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
            //    //Vector2 barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
            //    //Vector2 textLoc = new Vector2(barLoc.X, scrLoc.Y);
            //    //InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, this.CraftProgress.Percentage);
            //    ////UIManager.DrawStringOutlined(sb, this.Name + this.Time.TotalSeconds.ToString(" #0.##s"), textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
            //    //UIManager.DrawStringOutlined(sb, "test", textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
            //}

            public class InteractionActivate : Interaction
            {
                //GameObject Parent;
                public InteractionActivate()//GameObject parent)
                {
                    //this.Parent = parent;
                    this.Name = "Open";

                }
                static readonly TaskConditions conds = new TaskConditions(new AllCheck(new RangeCheck()));
                public override TaskConditions Conditions
                {
                    get
                    {
                        return conds;
                    }
                }
                public override void Perform(GameObject a, TargetArgs t)
                {
                    //if (a.Net is Client)
                    //    this.Parent.GetUi().Show();
                    if (a.Net is Client)
                    {
                        var entity = a.Map.GetBlockEntity(t.Global) as BlockSmelteryEntity;
                        var window = new WindowEntityInterface(entity, "Smeltery", () => t.Global);
                        //var ui = new SmeltingInterfaceNew(t.Global, entity);//.Refresh(t.Global, entity);
                        var ui = new SmeltingInterfaceNew().Refresh(t.Global, entity);

                        window.Client.Controls.Add(ui);
                        window.Show();
                    }
                }
                public override object Clone()
                {
                    return new InteractionActivate();//this.Parent);
                }
            }
            class InteractionInsert : Interaction
            {
                public InteractionInsert()
                {
                    this.Name = "Insert";
                }
                static readonly TaskConditions conds = new TaskConditions(new AllCheck(new RangeCheck()));
                public override TaskConditions Conditions
                {
                    get
                    {
                        return conds;
                    }
                }
                public override void Perform(GameObject a, TargetArgs t)
                {
                    //Entity comp = this.Parent.GetComponent<Entity>();
                    var comp = a.Map.GetBlockEntity(t.Global) as BlockSmelteryEntity;
                    //var hauled = GearComponent.GetSlot(a, GearType.Hauling);
                    var hauled = a.GetComponent<HaulComponent>().GetSlot();//.Slot;

                    if (hauled.Object == null)
                        return;

                    Container target;

                    if (comp.Fuels.Filter(hauled.Object))
                        target = comp.Fuels;
                    else if (comp.Storage.Filter(hauled.Object))
                        target = comp.Storage;
                    else
                        return;
                    target.Slots.Insert(hauled);
                }
                public override object Clone()
                {
                    return new InteractionInsert();
                }
            }
            //class InteractionCraft : Interaction
            //{
            //    Reaction.Product.ProductMaterialPair Product;
            //    Container MaterialsContainer;
            //    Vector3 WorkstationGlobal;
            //    BlockSmelteryEntity Workbench;

            //    public InteractionCraft(Reaction.Product.ProductMaterialPair product, BlockSmelteryEntity entity, Vector3 workstationGlobal)
            //        : base(
            //        "Craft",
            //        1
            //            //,
            //            //new TaskConditions(
            //            //    new AllCheck(
            //            //        new ScriptTaskCondition("ToolEquipped", (a, t) =>
            //            //        {
            //            //            var tool = GearComponent.GetSlot(a, GearType.Mainhand);
            //            //            return tool.Object.InstanceID == product.Tool.InstanceID;
            //            //        }),
            //            //        new ScriptTaskCondition("Materials", this.MaterialsAvailable)
            //            //    //new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
            //            //    //new TargetTypeCheck(TargetType.Entity))
            //            //    )
            //            //)
            //        )
            //    {
            //        this.Product = product;
            //        this.WorkstationGlobal = workstationGlobal;
            //        this.Workbench = entity;
            //        this.MaterialsContainer = entity.Storage;
            //        //this.Conditions = new TaskConditions(new ScriptTaskCondition("Materials", this.AvailabilityCondition, Message.Types.InteractionFailed));
            //        //this.RunningType = RunningTypes.Indefinite;
            //    }

            //    /// <summary>
            //    /// TODO: find a way to have static conditions for this one. maybe pass the interaction to the conditions as an argument so i can check local variables?
            //    /// </summary>
            //    public override TaskConditions Conditions
            //    {
            //        get
            //        {
            //            return new TaskConditions(
            //            new AllCheck(
            //                //new ScriptTaskCondition("ToolEquipped", (a, t) =>
            //                //{
            //                //    if (product.Tool == null)
            //                //        return true;
            //                //    var tool = GearComponent.GetSlot(a, GearType.Mainhand);
            //                //    if (tool.Object == null)
            //                //        return false;

            //                //    return tool.Object.InstanceID == product.Tool.InstanceID;
            //                //}),
            //                new ScriptTaskCondition("Materials", MaterialsAvailable)));
            //        }
            //    }
            //    public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
            //    {
            //        return this.Conditions.GetFailedCondition(actor, target) == null;
            //    }
            //    List<GameObjectSlot> GetReagentSlots(GameObject actor)
            //    {
            //        return actor.GetComponent<PersonalInventoryComponent>().GetContents().Concat(this.MaterialsContainer.Slots).ToList();
            //    }
            //    bool MaterialsAvailable(GameObject actor, TargetArgs target)
            //    {
            //        var contents = this.GetReagentSlots(actor);
            //        return this.Product.MaterialsAvailable(contents);
            //    }
            //    public override void Perform(GameObject actor, TargetArgs target)
            //    {
            //        // TODO: check here if crafting is legal (check available materials etc)
            //        var contents = this.GetReagentSlots(actor);// actor.GetComponent<PersonalInventoryComponent>().GetContents().Concat(this.MaterialsContainer.Slots).ToList();

            //        //if (!this.Product.MaterialsAvailable(this.MaterialsContainer.Slots))
            //        if (!this.Product.MaterialsAvailable(contents))
            //            return;
            //        //this.Product.ConsumeMaterials(actor.Net, this.MaterialsContainer.Slots);
            //        this.Product.ConsumeMaterials(actor.Net, contents);

            //        actor.Net.EventOccured(Message.Types.InventoryChanged, target.Object);
            //        var server = actor.Net as Server;


            //        if (server == null)
            //            return;
            //        server.SyncInstantiate(this.Product.Product);
            //        (actor.Map.GetBlockEntity(target.Global) as BlockSmelteryEntity).Out(this.Product.Product);
            //        server.RemoteProcedureCall(target, Message.Types.AddProduct, w =>
            //        {
            //            w.Write(this.Product.Product.InstanceID);
            //        });

            //        //this.Product.Product.Global = this.WorkstationGlobal + Vector3.UnitZ;
            //        //server.SyncInstantiate(this.Product.Product);
            //        //server.SyncSpawn(this.Product.Product);
            //        return;
            //    }

            //    public override object Clone()
            //    {
            //        return new InteractionCraft(this.Product, this.Workbench, this.WorkstationGlobal);
            //    }
            //}

            public override void DrawUI(SpriteBatch sb, Camera cam, Vector3 global)
            {
                if (this.Power.Value == 0)
                    RefuelIcon.DrawAboveEntity(sb, cam, global);
                    //DrawIconAbove(sb, cam, global, RefuelIcon, .5f);
            }
            
            //static public readonly Icon RefuelIcon = new Icon(Components.Materials.Logs.SpriteLogs);
        static public readonly Icon RefuelIcon = new Icon(ItemContent.LogsGrayscale);

    }
    //}
}
