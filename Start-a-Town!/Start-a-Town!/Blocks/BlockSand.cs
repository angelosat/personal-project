using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Particles;

namespace Start_a_Town_.Blocks
{
    class BlockSand : Block
    {
        public override bool IsMinable => true;
        //public override MaterialDef GetMaterial(byte blockdata)
        //{
        //    return MaterialDefOf.Sand;
        //}
        public BlockSand()
            : base("Sand")
        {
            this.LoadVariations("sand1");
            this.Ingredient = new Ingredient()//RawMaterialDef.Bags, MaterialDefOf.Sand, null, 1);
                .SetAllow(MaterialDefOf.Sand, true);
            this.ToggleConstructionCategory(ConstructionsManager.Walls, true);
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            yield return MaterialDefOf.Sand;
        }
        public override ParticleEmitterSphere GetEmitter()
        {
            var e = base.GetDustEmitter();

            e.ColorBegin = e.ColorEnd = Color.Gold;
            return e;
        }
    }
}
