using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;

namespace Start_a_Town_.Blocks
{
    class BlockSand : Block
    {
        public override bool IsMinable => true;
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Sand;
        }
        public BlockSand()
            : base(Block.Types.Sand)
        {
            this.LoadVariations("sand1");
            this.Ingredient = new Ingredient(RawMaterialDef.Bags, MaterialDefOf.Sand, null, 1);
            this.ToggleConstructionCategory(ConstructionsManager.Walls, true);
        }
        public override IEnumerable<byte> GetCraftingVariations()
        {
            yield return 0;
        }
        public override ParticleEmitterSphere GetEmitter()
        {
            var e = base.GetDustEmitter();

            e.ColorBegin = e.ColorEnd = Color.Gold;
            return e;
        }
    }
}
