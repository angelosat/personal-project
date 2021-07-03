using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Crafting.Smelting;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Items;

namespace Start_a_Town_.Components
{
    public enum States { Stopped, Running }

    class SmelteryComponent : EntityComponent
    {
        public override string ComponentName
        {
            get { return "Smeltery"; }
        }
        static List<Recipe> _Recipes;
        static public List<Recipe> Recipes
        {
            get
            {
                if (_Recipes is null)
                    _Recipes = new List<Recipe>()
                    {
                        //new Recipe(Recipe.Types.IronBar, "Iron Bar", GameObjectSlot.Create(GameObject.Types.Bar), Tuple.Create(GameObject.Objects[GameObject.Types.Ore], 1))
                    };
                return _Recipes;
            }
        }
        static public Recipe GetRecipe(Recipe.Types id)
        {
            return Recipes.Find(r => r.ID == id);
        }

        public ItemContainer Children { get { return (ItemContainer)this["Children"]; } set { this["Children"] = value; } }
        GameObject Parent;// { get { return (GameObject)this["Parent"]; } set { this["Parent"] = value; } }

        public GameObjectSlot Materials { get { return (GameObjectSlot)this["Materials"]; } set { this["Materials"] = value; } }
        public GameObjectSlot Fuel { get { return (GameObjectSlot)this["Fuel"]; } set { this["Fuel"] = value; } }
        public GameObjectSlot Product { get { return (GameObjectSlot)this["Product"]; } set { this["Product"] = value; } }

        //public float Power { get { return (float)this["Power"]; } set { this["Power"] = value; } }
        public Progress Power { get { return (Progress)this["Power"]; } set { this["Power"] = value; } }
        public Progress SmeltProgress { get { return (Progress)this["SmeltProgress"]; } set { this["SmeltProgress"] = value; } }
        public States State
        {
            get { return (States)this["State"]; }
            set
            {
                this["State"] = value;
                if (this.Parent != null)
                    if (this.Parent.Net != null)
                        this.Parent.Net.EventOccured(Message.Types.ObjectStateChanged, this.Parent);
            }
        }

        //public Reaction.Product.ProductMaterialPair SelectedProduct;
        public Crafting.CraftOperation SelectedProduct;

        public Container Input, Output, Fuels;

        public override void MakeChildOf(GameObject parent)
        {
            this.Parent = parent;
            this.Children = new ItemContainer(this.Parent, 16);//, () => this.Parent.ChildrenSequence);
            this.Materials.ID = parent.ChildrenSequence;
            this.Fuel.ID = parent.ChildrenSequence;
            this.Product.ID = parent.ChildrenSequence;

            this.Materials.Parent = parent;
            this.Fuel.Parent = parent;
            this.Product.Parent = parent;

            parent.RegisterContainers(this.Input, this.Output, this.Fuels);
        }

        public override void GetContainers(List<Container> list)
        {
            list.AddRange(new Container[] { this.Input, this.Output, this.Fuels });
        }

        //public SmelteryComponent()
        //{
        //    this.Children = new ItemContainer();
        //    this.Materials = new GameObjectSlot() { Name = "Materials", Filter = item => item.GetInfo().ItemSubType == ItemSubType.Ore };
        //    this.Fuel = new GameObjectSlot() { Name = "Fuel", Filter = item => item.GetComponent<MaterialsComponent>().Parts.Values.Any(mat => mat.Material.Fuel > 0) };
        //    this.Product = new GameObjectSlot() { Name = "Product" };//, Filter = item => false };
        //    this.State = States.Stopped;
        //    this.Power = new Progress();// { Min = 0, Max = 100, Value = 0 }; //0;
        //    this.SmeltProgress = new Progress() { Max = 10 }; // TODO: make max relative to ore material melting point
        //}
        public SmelteryComponent()
        {
            this.Children = new ItemContainer();
            this.Materials = new GameObjectSlot() { Name = "Materials", Filter = item => item.GetInfo().ItemSubType == ItemSubType.Ore };
            this.Fuel = new GameObjectSlot() { Name = "Fuel", Filter = item => item.GetComponent<MaterialsComponent>().Parts.Values.Any(mat => mat.Material.Fuel.Value > 0) };
            this.Product = new GameObjectSlot() { Name = "Product" };//, Filter = item => false };
            this.State = States.Stopped;
            this.Power = new Progress();// { Min = 0, Max = 100, Value = 0 }; //0;
            this.SmeltProgress = new Progress() { Max = 1 }; // TODO: make max relative to ore material melting point

            this.Fuels = new Container() { Name = "Fuel", Filter = item => MaterialsComponent.IsFuel(item) };
            this.Input = new Container() { Name = "Input"};//, Filter = item => item.GetInfo().ItemSubType == ItemSubType.Ore };
            this.Output = new Container() { Name = "Output" };
        }
        public SmelteryComponent(int inCapacity, int outCapacity, int fuelCapacity)
            : this()
        {
            this.Fuels = new Container(fuelCapacity) { Name = "Fuel", Filter = item => MaterialsComponent.IsFuel(item) };
            this.Input = new Container(inCapacity) { Name = "Input"};//, Filter = item => item.GetInfo().ItemSubType == ItemSubType.Ore };
            this.Output = new Container(outCapacity) { Name = "Output" };
        }

        public override void GetChildren(List<GameObjectSlot> list)
        {
            list.AddRange(this.Children);
            list.AddRange(new GameObjectSlot[] { this.Materials, this.Fuel, this.Product });
        }

        public override void Tick(GameObject parent)
        {
            //if (NoFuel)
            //{
            //    Cooldown();
            //    return;
            //}
            ConsumePower();
            if (this.State == States.Stopped)
            {
                Cooldown();
                return;
            };
            //if (this.Materials.HasValue)

            if (NoFuel)
            {
                this.ConsumeFuel();
            }

            if(!NoFuel)
                if (this.SelectedProduct != null)
                {
                    if (!this.MaterialsAvailable())
                        this.Stop(parent);
                    Smelt();
                    if (SmeltingFinished)
                    {
                        this.SmeltProgress.Value = 0;
                        this.Finish(parent);
                        //(this.Materials.StackSize.ToString() + parent.Net.ToString()).ToConsole();
                        //parent.Net.SyncEvent(parent, Message.Types.Finish, w => { });
                    }
                }

            //if (NoFuel)
            //    this.ConsumeFuel();
        }

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

        //public override void Update(IObjectProvider net, GameObject parent, Chunk chunk = null)
        //{
        //    if (this.State == States.Stopped)
        //        return;
        //    //if(this.Power.Value <= 0)
        //    //{
        //    //    this.State = States.Stopped;
        //    //    return;
        //    //}
        //    this.Power.Value -= 0.01f;
        //    //this.SmeltProgress.Value += 0.01f;
        //    //if (this.SmeltProgress.Value >= 1)
        //    //{
        //    //    this.SmeltProgress.Value = 0;
        //    //    this.Finish(parent);
        //    //}
        //    if (Power.Value <= 0)
        //    {
        //        //this.ConsumeFuel(parent);
        //        State = States.Stopped;
        //        this.Finish(parent);
        //    }
        //}

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Insert:
                    GameObjectSlot toInsert = e.Parameters[0] as GameObjectSlot;
                    if (!toInsert.HasValue)
                        throw new Exception("Null item");

                    Container target;
                    if (this.Input.Filter(toInsert.Object))
                        target = this.Input;
                    else if (this.Fuels.Filter(toInsert.Object))
                        target = this.Fuels;
                    else
                        return true;

                    //if (!targetSlot.Filter(toInsert.Object))
                    //    return true;

                    target.Slots.Insert(toInsert);
                    return true;
                //case Message.Types.Insert:
                //    GameObjectSlot toInsert = e.Parameters[0] as GameObjectSlot;
                //    if (!toInsert.HasValue)
                //        throw new Exception("Null item");

                //    GameObjectSlot target;
                //    if (this.Materials.Filter(toInsert.Object))
                //        target = this.Materials;
                //    else if (this.Fuel.Filter(toInsert.Object))
                //        target = this.Fuel;
                //    else
                //        return true;

                //    //if (!targetSlot.Filter(toInsert.Object))
                //    //    return true;

                //    target.Insert(toInsert);
                //    return true;

                case Message.Types.SlotInteraction:
                    var actor = e.Parameters[0] as GameObject;
                    var slot = e.Parameters[1] as GameObjectSlot;
                    e.Network.PostLocalEvent(actor, Message.Types.Insert, slot);
                    return true;

                case Message.Types.Activate:
                    throw new NotImplementedException();
                    //return true;

                case Message.Types.ArrangeInventory:
                    GameObjectSlot
                        sourceSlot = e.Parameters[0] as GameObjectSlot,
                        targetSlot = e.Parameters[1] as GameObjectSlot;
                    sourceSlot.Swap(targetSlot);
                    return true;

                default:
                    return false;
            }

        }
        internal override void HandleRemoteCall(GameObject parent, ObjectEventArgs e)
        {
            switch(e.Type)
            {
                case Message.Types.Start:
                    if (this.State == States.Stopped)
                        this.Start(parent);
                    else
                        this.Stop(parent);
                    break;

                case Message.Types.Retrieve:
                    e.Data.Translate(parent.Net, r =>
                    {
                        int childID = r.ReadByte();// r.ReadInt32();
                        GameObjectSlot slot = parent.GetChild(childID);
                        if(!slot.HasValue)
                            return;
                        parent.Net.PopLoot(slot.Object, parent);
                        // TODO: haul retrieved item instead of popping it
                        slot.Clear();
                    });
                    break;

                case Message.Types.SetProduct:
                    e.Data.Translate(parent.Net, r =>
                    {
                        var craft = new Crafting.CraftOperation(parent.Net, r);
                        var reaction = Reaction.Dictionary[craft.ReactionID];
                        var product = reaction.Products.First().GetProduct(reaction, parent, craft.Materials);
                        this.SelectedProduct = craft;// product;

                        //var product = new Reaction.Product.ProductMaterialPair(r);
                        //this.SelectedProduct = product;
                    });
                    break;

                //case Message.Types.Finish:
                //    this.Finish(parent);
                //    break;

                default:
                    break;
            }
        }

        private void Stop(GameObject parent)
        {
            this.State = States.Stopped;
            parent.Map.SetBlockLuminance(parent.Global, 0);
        }

        private void Start(GameObject parent)
        {
            if (this.SelectedProduct == null)
                return;
            if (!this.MaterialsAvailable())
                return;
            //this.ConsumeFuel();
            this.State = States.Running;
            parent.Map.SetBlockLuminance(parent.Global, 3);
        }

        private bool MaterialsAvailable()
        {
            foreach (var mat in this.SelectedProduct.Materials)//.Requirements)
                if (this.Input.Slots.GetAmount(obj => obj.GetID() == mat.ObjectID) < mat.AmountRequired)
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
        private void ConsumeFuelOld()
        {
            if (!this.Materials.HasValue)
                return;
            // consume fuel
            if (!this.Fuel.HasValue)
                return;

            this.Power.Max = 0;
            foreach (var p in from p in this.Fuel.Object.GetComponent<MaterialsComponent>().Parts.Values select p.Material.Fuel.Value)
                this.Power.Value = this.Power.Max += p;

            if (this.Fuel.Object.StackSize > 1)
                this.Fuel.Object.StackSize--;
            else
            {
                this.Fuel.Object.Net.SyncDisposeObject(this.Fuel.Object);
                this.Fuel.Clear();
            }

            //this.State = States.Running;
        }

        private void ConsumeMaterial(GameObject parent)
        {
            if (this.Materials.Object.StackSize == 1)
                this.Materials.Dispose();
            else
            {
                this.Materials.Object.StackSize--;
                //(this.Materials.Object.StackSize.ToString() + parent.Net.ToString()).ToConsole();
            }
        }
        private void Finish(GameObject parent)
        {
            if (this.SelectedProduct == null)
                return;
            if (!this.MaterialsAvailable())
                return;
            //this.SelectedProduct.Product.Clone();
            //this.Output.InsertObject(this.SelectedProduct.Product.Clone().ToSlot());

            var reaction = Reaction.Dictionary[this.SelectedProduct.ReactionID];
            var productFactory = reaction.Products.First().GetProduct(reaction, parent, this.SelectedProduct.Materials);
            var product = productFactory.Product;
            productFactory.ConsumeMaterials(parent.Net, this.Input.Slots);

            // first existing
            var existing = (from slot in this.Output.GetNonEmpty()
                            where slot.Object.IDType == product.IDType
                            where slot.Object.StackSize < slot.Object.StackMax
                            select slot).FirstOrDefault();
            if(existing!= null)
            {
                existing.StackSize++;
                return;
            }
            var empty = this.Output.GetEmpty().FirstOrDefault();
            if (empty != null)
            {
                //var clone = this.SelectedProduct.Product.Clone();
                //parent.Net.Spawn(clone, empty);
                parent.Net.Spawn(product, empty);
                return;
            }
            //parent.Net.PopLoot(this.SelectedProduct.Product.Clone(), parent);
            parent.Net.PopLoot(product, parent);
            return;

            if (!this.Materials.HasValue)
                return;
            //if (!this.Fuel.HasValue)
            //    return;
            var chain = this.Materials.Object.GetComponent<MaterialsComponent>().Parts["Body"].Material.ProcessingChain;
            var currentStep = chain.FindIndex(item => item.IDType == this.Materials.Object.IDType);
            if (currentStep == chain.Count - 1)
                return;
            var productt = chain[currentStep + 1].Clone().ToSlotLink();
            if (this.Product.HasValue == false)
            {
                parent.Net.Spawn(productt.Object, parent, this.Product.ID);
                //packet to insert child object is received by client before client checks if slot is empty,
                //which means that the new child has already been instantiated by the client BEFORE smelting finishes for the first time, 
                //return;
            }
            else
                //if (!this.Product.Insert(product))
                //    return;
                parent.Net.SyncSlotInsert(this.Product, productt.Object);

            ConsumeMaterial(parent);
            //prod = this.Materials.Object.GetComponent<MaterialsComponent>().Parts["Body"].Material.RawMaterials;
        }


        private Recipe GetRecipe()
        {
            if (!this.Materials.HasValue)
                return null;
            //Recipe recipe = Recipes.FirstOrDefault(r => r.Materials.FirstOrDefault(m => m.Key.IDType == Materials.Object.IDType) is not null);
            Recipe recipe = Recipes.FirstOrDefault(r => r.Materials.Any(m => m.Key.IDType == Materials.Object.IDType));

            return recipe;
        }

        public override object Clone()
        {
            return new SmelteryComponent(this.Input.Capacity, this.Output.Capacity, this.Fuels.Capacity);
        }

        public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<GameEvent>> gameEventHandlers)
        {
            //SmeltingWindow.Instance.Show();
            //ui.Controls.Add(SmeltingInterface.Instance); 

            //var smeltUI = new SmelteryInterface(parent);
            //var smeltUI = SmeltingInterface.Instance;
            var smeltUI = new SmeltingInterfaceNew();
            smeltUI.Refresh(parent);
            //SmeltingInterface.Instance.Location = smeltUI.TopRight;
            ui.Controls.Add(smeltUI);//, SmeltingInterface.Instance); 
        }
        public override void GetRightClickActions(GameObject parent, List<ContextAction> actions)
        {
            actions.Add(new ContextAction(() => "Examine", () => parent.GetUi().Show()));
        }


        private void RefreshUI(Panel panel_recipe)
        {
            panel_recipe.Controls.Clear();
            Recipe recipe = GetRecipe();
            if (recipe is not null)
                panel_recipe.Controls.Add(recipe.ToObject().GetTooltip());
        }

        #region Serialization
        internal override List<SaveTag> Save()
        {
            List<SaveTag> tag = new List<SaveTag>();
            if(this.Materials.HasValue) tag.Add(this.Materials.Save("Ore"));
            if (this.Fuel.HasValue) tag.Add(this.Fuel.Save("Fuel"));
            if (this.Product.HasValue) tag.Add(this.Product.Save("Product"));
            return tag;
        }
        internal override void Load(SaveTag save)
        {
            //this.Materials.Load(save["Ore"]);
            //this.Fuel.Load(save["Fuel"]);
            //this.Product.Load(save["Product"]);
            save.TryGetTag("Ore", tag => this.Materials.Load(tag));
            save.TryGetTag("Fuel", tag => this.Fuel.Load(tag));
            save.TryGetTag("Product", tag => this.Product.Load(tag));
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            this.Materials.Write(w);
            this.Fuel.Write(w);
            this.Product.Write(w);

            this.Input.Write(w);
            this.Output.Write(w);
            this.Fuels.Write(w);
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Materials.Read(r);
            this.Fuel.Read(r);
            this.Product.Read(r);

            this.Input.Read(r);
            this.Output.Read(r);
            this.Fuels.Read(r);
        }
        #endregion

        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interaction> actions)
        {
            actions.Add(PlayerInput.Activate, new InteractionActivate(parent));
            actions.Add(PlayerInput.ActivateHold, new InteractionInsert(parent));
        }

        class InteractionActivate : Interaction
        {
            GameObject Parent;
            public InteractionActivate(GameObject parent)
            {
                this.Parent = parent;
            }
            public override void Perform(GameObject a, TargetArgs t)
            {
                if (a.Net is Net.Client)
                    this.Parent.GetUi().Show();
            }
            public override object Clone()
            {
                return new InteractionActivate(this.Parent);
            }
        }


        class InteractionInsert : Interaction
        {
            GameObject Parent;
            public InteractionInsert(GameObject parent)
            {
                this.Parent = parent;
                this.Name = "Insert";
            }
            public override void Perform(GameObject a, TargetArgs t)
            {
                SmelteryComponent comp = this.Parent.GetComponent<SmelteryComponent>();
                //var hauled = GearComponent.GetSlot(a, GearType.Hauling);
                var hauled = a.GetComponent<HaulComponent>().GetSlot();//.Slot;

                if (hauled.Object == null)
                    return;

                Container target;
                //if (comp.Input.Filter(hauled.Object))
                //    target = comp.Input;
                //else if (comp.Fuels.Filter(hauled.Object))
                //    target = comp.Fuels;
                //else
                //    return;
                if (comp.Fuels.Filter(hauled.Object))
                    target = comp.Fuels;
                else if (comp.Input.Filter(hauled.Object))
                    target = comp.Input;
                else
                    return;
                target.Slots.Insert(hauled);
            }
            public override object Clone()
            {
                return new InteractionInsert(this.Parent);
            }
        }
    }
}
