using System.Collections.Generic;
using System.Linq;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components.Crafting
{
    public partial class BlockRecipe : ISlottable
    {
        static int _IDSequence = 0;
        public static int IDSequence { get { return _IDSequence++; } }
        public Towns.Constructions.ConstructionCategory Category;

        static Dictionary<int, BlockRecipe> _Dictionary;
        public static Dictionary<int, BlockRecipe> Dictionary
        {
            get
            {
                if (_Dictionary == null)
                    _Dictionary = new Dictionary<int, BlockRecipe>();
                return _Dictionary;
            }
        }

        public int ID { get; set; }
        public string Name { get; set; }
        public List<Reaction.Reagent> Reagents;
        public BlockRecipe.Product BlockProduct;
        public ToolAbilityDef Skill;
        public int WorkAmount = 1;
        public Block Block { get { return this.BlockProduct.Block; } }

        public BlockRecipe(List<Reaction.Reagent> reagents, BlockRecipe.Product product, ToolAbilityDef skill = null)
        {
            this.ID = IDSequence;
            this.Name = product.Block.ToString();
            this.Skill = skill;
            this.Reagents = reagents;
            this.BlockProduct = product;
        }
       
        public List<BlockRecipe.ProductMaterialPair> GetVariants()
        {
            var list = new List<ProductMaterialPair>();
            foreach (var reagent in this.Reagents)
            {
                foreach (var mat in reagent.GetValidCraftingItems())
                {
                    byte data = this.BlockProduct.Block.GetDataFromMaterial(mat);
                    list.Add(
                        new ProductMaterialPair(this.BlockProduct.Block, data, new ItemRequirement(mat.IDType, 1)) { Skill = this.Skill }
                            .AddMaterial(mat.ID, 1));
                }

            }

            return list;
        }
        public ProductMaterialPair GetVariant(byte data)
        {
            var vars = this.GetVariants();
            return vars.First(v => v.Data == data);
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
