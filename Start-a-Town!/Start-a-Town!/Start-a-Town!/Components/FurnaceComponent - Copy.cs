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
                        new Recipe(Recipe.Types.Coal, "Coal", GameObjectSlot.Create(GameObject.Types.Coal), Tuple.Create(GameObject.Objects[GameObject.Types.Log], 1))
                    };
                return _Recipes;
            }
        }
        static public Recipe GetRecipe(Recipe.Types id)
        {
            return Recipes.Find(r => r.ID == id);
        }

        public GameObjectSlot Product { get { return (GameObjectSlot)this["Product"]; } set { this["Product"] = value; } }
        public GameObjectSlot Materials { get { return (GameObjectSlot)this["Materials"]; } set { this["Materials"] = value; } }
        public Recipe Recipe { get { return (Recipe)this["Recipe"]; } set { this["Recipe"] = value; } }
        public States State { get { return (States)this["State"]; } set { this["State"] = value; } }
        public float Power { get { return (float)this["Power"]; } set { this["Power"] = value; } }

        public bool Overflow { get { return (bool)this["Overflow"]; } set { this["Overflow"] = value; } }
        public override void MakeChildOf(GameObject parent)
        {
            base.MakeChildOf(parent);
            this.Product = new GameObjectSlot() { Parent = parent };
            this.Materials = new GameObjectSlot() { Parent = parent };
        }
        public FurnaceComponent(params object[] p)
        {
            //this.Product = new GameObjectSlot();
            //this.Materials = new GameObjectSlot();
            this.Power = 0;
            this.State = States.Stopped;
            this.Overflow = false;

            Queue<object> queue = new Queue<object>(p);
            while (queue.Count > 0)
            {
                this[(string)queue.Dequeue()] = new GameObjectSlot();
            }
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

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Activate:
                    throw new NotImplementedException();
                    //e.Sender.PostMessage(Message.Types.Interface, parent);
                    return true;

                case Message.Types.ArrangeInventory:
                    GameObjectSlot
                        sourceSlot = e.Parameters[0] as GameObjectSlot,
                        targetSlot = e.Parameters[1] as GameObjectSlot;
                    if (!targetSlot.HasValue)
                    {
                        sourceSlot.Swap(targetSlot);
                        return true;
                    };
                    if (targetSlot.Object.ID == sourceSlot.Object.ID)
                        if (targetSlot.StackSize < targetSlot.StackMax)
                        {
                            targetSlot.StackSize++;
                            sourceSlot.StackSize--;
                        }
                    return true;

                case Message.Types.Craft:
                    if (!Materials.HasValue)
                        return true;
                    Recipe recipe = GetRecipe();
                    if (recipe.IsNull())
                        return true;

                    if (!CheckMaterials(recipe))
                        return true;

                    if (this.Product.HasValue)
                        if (this.Product.Object.ID != recipe.Product.Object.ID)
                            if (!Overflow)
                                return true;

                    this.Power = Engine.TargetFps;
                    this.State = States.Running;
                    //this.Materials.StackSize--;
                    //this.Product.Swap(GameObject.Create(this.Recipe.Product.Object.ID).ToSlot());

                    return true;

                default:
                    return false;
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
            //foreach (var slot in this.Properties.Where(foo => foo.Value is GameObjectSlot))
            //{
            //    ui.Controls.Add(slot.Key.ToLabel(ui.Controls.BottomLeft));
            //    ui.Controls.Add(new InventorySlot(slot.Value as GameObjectSlot, parent) { Location = ui.Controls.BottomLeft });
            //}
            ui.Controls.Add(new CheckBox("Allow overflow", ui.Controls.BottomLeft, this.Overflow) { LeftClickAction = () => this.Overflow = !this.Overflow });

            Label lbl_mats = "Material:".ToLabel(ui.Controls.BottomLeft);
            InventorySlot slot_Materials = new InventorySlot(this.Materials, parent) { Location = lbl_mats.BottomLeft };

            Label lbl_product = "Product:".ToLabel(slot_Materials.BottomLeft);
            InventorySlot slot_Product = new InventorySlot(this.Product, parent) { Location = lbl_product.BottomLeft, Condition = o => false };

            ui.Controls.Add(lbl_mats, lbl_product, slot_Materials, slot_Product);

            ui.Controls.Add(new Bar() { Location = ui.Controls.BottomLeft, PercFunc = () => 1 - this.Power / (float)Engine.TargetFps });

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
