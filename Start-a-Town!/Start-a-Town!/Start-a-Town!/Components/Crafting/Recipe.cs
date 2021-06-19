using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    public class Recipe : Component
    {
        public override string ComponentName
        {
            get { return "Recipe"; }
        }
        public new enum Types { IronBar, Coal }

        public Types ID { get; set; }
        public string Name { get; set; }
        //Dictionary<GameObject, int> Materials { get; set; }
        public Dictionary<GameObject, int> Materials { get; set; }
        public GameObjectSlot Product { get; set; }

        public Recipe(Types id, string name, GameObjectSlot product, params Tuple<GameObject, int>[] mats)
        {
            this.ID = id;
            this.Name = name;
            this.Product = product;
            this.Materials = new Dictionary<GameObject, int>();
            foreach (var mat in mats)
                this.Materials[mat.Item1] = mat.Item2;
        }

        public override object Clone()
        {
            return new Recipe(this.ID, this.Name, this.Product) { Materials = this.Materials };
        }
        public GameObject ToObject()
        {
            GameObject obj = new GameObject();
            obj["Info"] = new GeneralComponent(0, ObjectType.Blueprint, "Recipe: " + this.Product.Object.Name, "A Recipe");
            obj["Gui"] = this.Product.Object.GetGui();
            obj["Physics"] = new PhysicsComponent();
            obj["Sprite"] = new SpriteComponent(this.Product.Object.GetSprite());
            obj["Blueprint"] = this;
            return obj;
        }

        public override void GetTooltip(GameObject parent, UI.Control tooltip)
        {
            tooltip.Controls.Add("Materials:".ToLabel(tooltip.Controls.BottomLeft));
            foreach (var mat in this.Materials)
            {
                //SlotWithText slot = new SlotWithText(tooltip.Controls.BottomLeft);
                //slot.Tag = mat.Key.ToSlot();
                //slot.Slot.CornerTextFunc = () => {  };
                //tooltip.Controls.Add(slot);
                tooltip.Controls.Add(new Label(tooltip.Controls.BottomLeft, mat.Key.Name + " x" + mat.Value)
                {
                    Active = true,
                    Font = UI.UIManager.FontBold,
                    TooltipFunc = (t) => mat.Key.GetTooltip(t),
                    LeftClickAction = () => mat.Key.GetTooltip().ToWindow().Show()
                }); //(mat.Key.Name + " x" + mat.Value).ToLabel(tooltip.Controls.BottomLeft)
            }
        }

        static public List<Recipe> GetRecipes(GameObject material)
        {
            List<Recipe> found = new List<Recipe>();
            found.AddRange(FurnaceComponent.Recipes.FindAll(r => r.Materials.Count(m => m.Key.ID == material.ID) > 0));
            found.AddRange(SmelteryComponent.Recipes.FindAll(r => r.Materials.Count(m => m.Key.ID == material.ID) > 0));
            return found;
        }
    }
}
