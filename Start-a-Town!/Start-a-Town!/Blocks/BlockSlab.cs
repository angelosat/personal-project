using System.Collections.Generic;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Blocks
{
    class BlockSlab : Block
    {
        public BlockSlab()
            : base(Block.Types.Slab, opaque: false)
        {
            var txt = Block.Atlas.Load("blocks/slab", Block.QuarterBlockMapDepth, Block.QuarterBlockMapNormal);
            this.Variations.Add(txt);

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            this.Ingredient = new Ingredient(RawMaterialDef.Ingots, null, null, 1);
            Towns.Constructions.ConstructionsManager.Walls.Add(this.Recipe);
        }
        public override float GetPathingCost(byte data)
        {
            return 0;
        }
        public override Microsoft.Xna.Framework.Color[] UV
        {
            get
            {
                return Block.BlockCoordinatesQuarter;
            }
        }
        public override MouseMap MouseMap
        {
            get
            {
                return Block.BlockQuarterMouseMap;
            }
        }
        public override float GetHeight(byte data, float x, float y)
        {
            return this.GetHeight(x, y);
        }
        public override float GetHeight(float x, float y)
        {
            return .25f;
        }
        public override IEnumerable<byte> GetCraftingVariations()
        {
            yield return (byte)MaterialDefOf.Stone.ID;
        }
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Stone;
        }
    }
}
