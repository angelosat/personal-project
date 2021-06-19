using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class FurnaceComponent : Component
    {
        public override string ComponentName
        {
            get { return "Furnace"; }
        }
        static List<Recipe> _Recipes;
        static public List<Recipe> Recipes
        {
            get
            {
                if (_Recipes.IsNull())
                    _Recipes = new List<Recipe>()
                    {
                        //new Recipe(Recipe.Types.Coal, "Coal", GameObjectSlot.Create(GameObject.Types.Coal), Tuple.Create(GameObject.Objects[GameObject.Types.Log], 1))
                    };
                return _Recipes;
            }
        }
        static public Recipe GetRecipe(Recipe.Types id)
        {
            return Recipes.Find(r => r.ID == id);
        }

        ItemContainer Contents;
        //public GameObjectSlot Product { get { return (GameObjectSlot)this["Product"]; } set { this["Product"] = value; } }
        //public GameObjectSlot Materials { get { return (GameObjectSlot)this["Materials"]; } set { this["Materials"] = value; } }
        //public ItemContainer Product { get { return (ItemContainer)this["Product"]; } set { this["Product"] = value; } }
        //public ItemContainer Materials { get { return (ItemContainer)this["Materials"]; } set { this["Materials"] = value; } }
        public GameObjectSlot Product { get { return this.Contents[0]; } }
        public GameObjectSlot Materials { get { return this.Contents[1]; } }
        public ItemContainer Fuel { get { return (ItemContainer)this["Fuel"]; } set { this["Fuel"] = value; } } 
        public Recipe Recipe { get { return (Recipe)this["Recipe"]; } set { this["Recipe"] = value; } }
        public States State { get { return (States)this["State"]; } set { this["State"] = value; } }
        public float Power { get { return (float)this["Power"]; } set { this["Power"] = value; } }

        public bool Overflow { get { return (bool)this["Overflow"]; } set { this["Overflow"] = value; } }
        public override void MakeChildOf(GameObject parent)
        {
            base.MakeChildOf(parent);
            this.Contents = new ItemContainer(parent, 2);
            this.Materials.Filter = o =>
            {
                return !o.HasComponent<ConsumableComponent>();
            };
            this.Fuel = new ItemContainer(parent, 4)
            {
                Filter = o =>
                {
                    return o.HasComponent<ConsumableComponent>();
                }
            };
            //this.Product = new ItemContainer(parent, 1);
            //this.Materials = new ItemContainer(parent, 1);
            //this.Product = new GameObjectSlot() { Parent = parent };
            //this.Materials = new GameObjectSlot() { Parent = parent };
        }
        public FurnaceComponent()
        {
            this.Power = 0;
            this.State = States.Stopped;
            this.Overflow = false;
        }

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            if (this.State == States.Stopped)
                return;

            this.Power -= 1f;
            if (Power < 0)
            {
                this.Materials.StackSize--;
                if (this.Materials.StackSize == 0)
                    State = States.Stopped;
                else
                    Power = Engine.TargetFps;

                if (!this.Product.HasValue)
                    this.Product.Object = GameObject.Create(this.Recipe.Product.Object.ID);
                else
                {
                    if (this.Product.Object.ID != this.Recipe.Product.Object.ID)
                    {
                        if (Overflow)
                            net.PopLoot(GameObject.Create(this.Recipe.Product.Object.ID), parent.Global, parent.Velocity);
                         //   Loot.PopLoot(parent, GameObject.Create(this.Recipe.Product.Object.ID));
                        return;
                    }
                }
                this.Product.StackSize++;
            }
        }

        private bool CheckMaterials(Recipe recipe)
        {
            foreach (var mat in recipe.Materials)
            {
                if (this.Materials.Object.ID != mat.Key.ID)
                    return false;
                if (this.Materials.StackSize < mat.Value)
                    return false;
            }
            return true;
        }

        public override object Clone()
        {
            return new FurnaceComponent();
        }

        public override void GetChildren(List<GameObjectSlot> list)
        {
            list.AddRange(this.Contents);
            list.AddRange(this.Fuel);
            //list.AddRange(this.Product);
            //list.AddRange(this.Materials);
        }
        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Activate:

                   // GameObjectSlot hauled = HaulComponent.GetHeldObject(e.Parameters[0] as GameObject);
                    GameObjectSlot hauled = GearComponent.GetHeldObject(e.Parameters[0] as GameObject);
                    if (!hauled.HasValue)
                        return false;
                    if (!hauled.Object.HasComponent<FuelComponent>())
                        return false;

                    return this.Fuel.InsertObject(e.Network, hauled);

                default:
                    break;
            }
            return false;
        }

        private Recipe GetRecipe()
        {
            if (!this.Materials.HasValue)
                return null;
            Recipe recipe = Recipes.FirstOrDefault(r => !r.Materials.FirstOrDefault(m => m.Key.ID == Materials.Object.ID).IsNull());
            this.Recipe = recipe;
            return recipe;
        }

        public override void GetUI(GameObject parent, UI.Control ui, List<EventHandler<ObjectEventArgs>> handlers)
        {
            ui.Controls.Add(new CheckBox("Allow overflow", ui.Controls.BottomLeft, this.Overflow) { LeftClickAction = () => this.Overflow = !this.Overflow });

            Label lbl_mats = "Material:".ToLabel(ui.Controls.BottomLeft);
            SlotDefault slot_Materials = new SlotDefault(this.Materials) { Location = lbl_mats.BottomLeft };

            Label lbl_product = "Product:".ToLabel(slot_Materials.BottomLeft);
            SlotDefault slot_Product = new SlotDefault(this.Product) { Location = lbl_product.BottomLeft, DragDropCondition = o => false };

            Label lbl_fuel = "Fuel:".ToLabel(slot_Product.BottomLeft);
            SlotGrid<SlotDefault> slots_fuel = new SlotGrid<SlotDefault>(this.Fuel, 4) { Location = lbl_fuel.BottomLeft };


            ui.Controls.Add(lbl_mats, lbl_product, slot_Materials, slot_Product, slots_fuel);

            //ui.Controls.Add(new Bar() { Location = ui.Controls.BottomLeft, PercFunc = () => 1 - this.Power / (float)Engine.TargetFps });

            Panel panel_recipe = new Panel(ui.Controls.BottomLeft) { Dimensions = new Vector2(200, 200) };
            RefreshUI(panel_recipe);

            Button btn_start = new Button(panel_recipe.BottomLeft, "Start")
            {
                LeftClickAction = () =>
                {
                    throw new NotImplementedException();
                    //parent.PostMessage(Message.Types.Craft, Player.Actor);
                }
            };

            ui.Controls.Add(panel_recipe, btn_start);

            handlers.Add((sender, e) =>
            {
                switch (e.Type)
                {
                    case Message.Types.ArrangeInventory:
                        RefreshUI(panel_recipe);
                        return;

                    default:
                        break;
                }
            });
        }

        private void RefreshUI(Panel panel_recipe)
        {
            panel_recipe.Controls.Clear();
            Recipe recipe = GetRecipe();
            if (!recipe.IsNull())
            {
                ScrollableBox box = new ScrollableBox(panel_recipe.ClientSize);
                box.Add(recipe.ToObject().GetTooltip());

                foreach (var mat in recipe.Materials)
                {
                    SlotWithText slot = new SlotWithText(box.Client.Controls.BottomLeft);
                    slot.Tag = mat.Key.ToSlot();
                    //int amount = 0;
                    //if (this.Materials.HasValue)
                    //    if (this.Materials.Object.ID == mat.Key.ID)
                    //        amount += this.Materials.StackSize;
                    slot.Slot.CornerTextFunc = (sl) =>
                    {
                        return (this.Materials.HasValue ? (this.Materials.Object.ID == mat.Key.ID ? this.Materials.StackSize : 0) : 0) + "/" + mat.Value;
                    };
                       
                    box.Add(slot);
                }

                panel_recipe.Controls.Add(box);//recipe.ToObject().GetTooltip());
            }
        }
    }
}
