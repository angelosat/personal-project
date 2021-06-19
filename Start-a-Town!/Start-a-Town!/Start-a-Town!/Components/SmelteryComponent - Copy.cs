using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Components
{
    public enum States { Stopped, Running }

    class SmelteryComponent : Component
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
                if (_Recipes.IsNull())
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
        GameObject Parent { get { return (GameObject)this["Parent"]; } set { this["Parent"] = value; } }

        public GameObjectSlot Materials { get { return (GameObjectSlot)this["Materials"]; } set { this["Materials"] = value; } }
        public GameObjectSlot Fuel { get { return (GameObjectSlot)this["Fuel"]; } set { this["Fuel"] = value; } }
        public GameObjectSlot Product { get { return (GameObjectSlot)this["Product"]; } set { this["Product"] = value; } }

        //public float Power { get { return (float)this["Power"]; } set { this["Power"] = value; } }
        public Progress Power { get { return (Progress)this["Power"]; } set { this["Power"] = value; } }
        public Progress SmeltProgress { get { return (Progress)this["SmeltProgress"]; } set { this["SmeltProgress"] = value; } }
        public States State { get { return (States)this["State"]; } set { this["State"] = value; } }

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
        }

        public SmelteryComponent()
        {
            this.Children = new ItemContainer();
            this.Materials = new GameObjectSlot() { Name = "Materials", Filter = item => item.GetInfo().ItemSubType == ItemSubType.Ore };
            this.Fuel = new GameObjectSlot() { Name = "Fuel", Filter = item => item.GetComponent<MaterialsComponent>().Parts.Values.Any(mat => mat.Material.Fuel > 0) };
            this.Product = new GameObjectSlot() { Name = "Product" };//, Filter = item => false };
            this.State = States.Stopped;
            this.Power = new Progress();// { Min = 0, Max = 100, Value = 0 }; //0;
            this.SmeltProgress = new Progress() { Max = 10 }; // TODO: make max relative to ore material melting point
        }

        public override void GetChildren(List<GameObjectSlot> list)
        {
            list.AddRange(this.Children);
            list.AddRange(new GameObjectSlot[] { this.Materials, this.Fuel, this.Product });
        }

        public override void Update(GameObject parent)
        {
            //if (this.State == States.Stopped)
            //    return;
            if (NoFuel)
            {
                //this.State = States.Stopped;
                Cooldown();
                return;
            }
            ConsumePower();
            if (this.Materials.HasValue)
            {
                Smelt();
                if (SmeltingFinished)
                {
                    this.SmeltProgress.Value = 0;
                    this.Finish(parent);
                    //(this.Materials.StackSize.ToString() + parent.Net.ToString()).ToConsole();
                    //parent.Net.SyncEvent(parent, Message.Types.Finish, w => { });
                }
            }

            if (NoFuel)
                this.ConsumeFuel();
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

        //public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
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

                    GameObjectSlot target;
                    if (this.Materials.Filter(toInsert.Object))
                        target = this.Materials;
                    else if (this.Fuel.Filter(toInsert.Object))
                        target = this.Fuel;
                    else
                        return true;

                    //if (!targetSlot.Filter(toInsert.Object))
                    //    return true;

                    target.Insert(toInsert);
                    return true;

                case Message.Types.Activate:
                    throw new NotImplementedException();
                    //e.Sender.PostMessage(Message.Types.Interface, parent);
                    return true;

                case Message.Types.ArrangeInventory:
                    GameObjectSlot
                        sourceSlot = e.Parameters[0] as GameObjectSlot,
                        targetSlot = e.Parameters[1] as GameObjectSlot;
                    sourceSlot.Swap(targetSlot);
                    return true;

                case Message.Types.Craft:
                    if (!Materials.HasValue)
                        return true;
                    Recipe recipe = GetRecipe();
                    if (recipe.IsNull())
                        return true;

                    if (!this.Fuel.HasValue)
                        return true;
                    this.Power.Value += FuelComponent.GetPower(this.Fuel.Object);
                    this.Fuel.Clear();
                    this.State = States.Running;

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
                    this.Start(parent);
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

                //case Message.Types.Finish:
                //    this.Finish(parent);
                //    break;

                default:
                    break;
            }
        }

        private void Start(GameObject parent)
        {
            //("start " + this.Materials.StackSize.ToString() + parent.Net.ToString()).ToConsole();
            //if (this.Materials.HasValue)
                this.ConsumeFuel();
            this.State = States.Running;
        }
        private void ConsumeFuel()
        {
            if (!this.Materials.HasValue)
                return;
            // consume fuel
            if (!this.Fuel.HasValue)
                return;

            this.Power.Max = 0;
            foreach (var p in from p in this.Fuel.Object.GetComponent<MaterialsComponent>().Parts.Values select p.Material.Fuel)
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
            if (!this.Materials.HasValue)
                return;
            //if (!this.Fuel.HasValue)
            //    return;
            var chain = this.Materials.Object.GetComponent<MaterialsComponent>().Parts["Body"].Material.ProcessingChain;
            var currentStep = chain.FindIndex(item => item.ID == this.Materials.Object.ID);
            if (currentStep == chain.Count - 1)
                return;
            var product = chain[currentStep + 1].Clone().ToSlot();
            if (this.Product.HasValue == false)
            {
                parent.Net.Spawn(product.Object, parent, this.Product.ID);
                //packet to insert child object is received by client before client checks if slot is empty,
                //which means that the new child has already been instantiated by the client BEFORE smelting finishes for the first time, 
                //return;
            }
            else
                //if (!this.Product.Insert(product))
                //    return;
                parent.Net.SyncSlotInsert(this.Product, product.Object);

            ConsumeMaterial(parent);
            //prod = this.Materials.Object.GetComponent<MaterialsComponent>().Parts["Body"].Material.RawMaterials;
        }


        private Recipe GetRecipe()
        {
            if (!this.Materials.HasValue)
                return null;
            Recipe recipe = Recipes.FirstOrDefault(r => !r.Materials.FirstOrDefault(m => m.Key.ID == Materials.Object.ID).IsNull());
            return recipe;
        }

        public override object Clone()
        {
            return new SmelteryComponent();
        }

        public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<Net.GameEvent>> gameEventHandlers)
        {
            //SmeltingWindow.Instance.Show();
            //ui.Controls.Add(SmeltingInterface.Instance); 
            var smeltUI = new SmelteryInterface(parent);
            SmeltingInterface.Instance.Location = smeltUI.TopRight;
            ui.Controls.Add(smeltUI);//, SmeltingInterface.Instance); 
        }
        public override void GetRightClickActions(GameObject parent, List<ContextAction> actions)
        {
            actions.Add(new ContextAction(() => "Examine", () => parent.GetUi().Show()));
        }

        //public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<ObjectEventArgs>> handlers)
        //{
        //    InventorySlot slot_Materials, slot_Fuel, slot_Product;
        //    Label lbl_mats, lbl_fuel, lbl_product;

        //    lbl_mats = "Material:".ToLabel(ui.Controls.BottomLeft);
        //    slot_Materials = new InventorySlot(this.Materials, parent) { Location = lbl_mats.BottomLeft};

        //    lbl_fuel = "Fuel:".ToLabel(slot_Materials.BottomLeft);
        //    slot_Fuel = new InventorySlot(this.Fuel, parent) { Location = lbl_fuel.BottomLeft, DragDropCondition = obj => FuelComponent.GetPower(obj) > 0 };

        //    lbl_product = "Product:".ToLabel(slot_Fuel.BottomLeft);
        //    slot_Product = new InventorySlot(this.Product, parent) { Location = lbl_product.BottomLeft, DragDropCondition = o => false };

            

        //    ui.Controls.Add(lbl_mats, lbl_fuel, lbl_product,
        //        slot_Materials, slot_Fuel, slot_Product
        //        );
        //        //btn_start);

        //    Panel panel_recipe = new Panel(ui.Controls.BottomLeft) { Dimensions = new Vector2(200, 300) };
        //    RefreshUI(panel_recipe);

        //    Button btn_start = new Button(panel_recipe.BottomLeft, "Start")
        //    {
        //        LeftClickAction = () =>
        //        {
        //            throw new NotImplementedException();
        //            //parent.PostMessage(Message.Types.Craft, Player.Actor);
        //        }
        //    };

        //    ui.Controls.Add(panel_recipe, btn_start);

        //    handlers.Add((sender, e) =>
        //    {
        //        switch (e.Type)
        //        {
        //            case Message.Types.ArrangeInventory:
        //                RefreshUI(panel_recipe);
        //                return;
                        
        //            default:
        //                break;
        //        }
        //    });
        //}

        private void RefreshUI(Panel panel_recipe)
        {
            panel_recipe.Controls.Clear();
            Recipe recipe = GetRecipe();
            if (!recipe.IsNull())
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
        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.Materials.Read(r);
            this.Fuel.Read(r);
            this.Product.Read(r);
        }
        #endregion

        public override void GetPlayerActionsWorld(GameObject parent, Dictionary<PlayerInput, Interactions.Interaction> actions)
        {
            actions.Add(PlayerInput.Activate, new InteractionActivate(parent));
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
    }
}
