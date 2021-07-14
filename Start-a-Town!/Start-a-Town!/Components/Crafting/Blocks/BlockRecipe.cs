using System.Collections.Generic;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components.Crafting
{
    public partial class BlockRecipe : ISlottable
    {
        public Towns.Constructions.ConstructionCategory Category;

        public string Name;
        public List<Reaction.Reagent> Reagents;
        public BlockRecipe.Product BlockProduct;
        public ToolAbilityDef Skill;
        public int WorkAmount = 1;
        public Block Block { get { return this.BlockProduct.Block; } }

        public BlockRecipe(List<Reaction.Reagent> reagents, BlockRecipe.Product product, ToolAbilityDef skill = null)
        {
            this.Name = product.Block.ToString();
            this.Skill = skill;
            this.Reagents = reagents;
            this.BlockProduct = product;
        }

        public string GetName()
        {
            return this.BlockProduct.Block.GetName();
        }
        public Icon GetIcon()
        {
            return this.BlockProduct.Block.GetIcon();
        }
        public Color GetSlotColor()
        {
            return Color.White;
        }
        public string GetCornerText()
        {
            return "";
        }
        public void DrawUI(SpriteBatch sb, Vector2 pos)
        {
            this.BlockProduct.Block.DrawUI(sb, pos);
        }
        public void GetTooltipInfo(Tooltip tooltip)
        {

        }
    }
}
