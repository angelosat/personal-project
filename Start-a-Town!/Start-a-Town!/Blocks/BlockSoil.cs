using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Particles;

namespace Start_a_Town_
{
    class BlockSoil : Block
    {
        public class Placer : BlockPlacer
        {
            protected override Block Block => BlockDefOf.Soil;
        }

        public override bool IsMinable => true;
        public override Color DirtColor
        {
            get
            {
                return Color.SaddleBrown;
            }
        }
        public override ParticleEmitterSphere GetEmitter()
        {
            return base.GetDirtEmitter();
        }

        public BlockSoil()
            : base(Block.Types.Soil)
        {
            this.RequiresConstruction = false;
            this.LoadVariations("soil/soil1", "soil/soil2", "soil/soil3", "soil/soil4");
            
            this.Ingredient = new Ingredient(RawMaterialDef.Bags, MaterialDefOf.Soil, null, 1);

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterialType(MaterialType.Soil), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                new BlockRecipe.Product(this)
                );
            Towns.Constructions.ConstructionsManager.Walls.Add(this.Recipe);
        }
       
        public override void RandomBlockUpdate(IObjectProvider net, IntVec3 global, Cell celll)
        {
            if (net.Map.GetBlock(global + IntVec3.UnitZ) != BlockDefOf.Air)
                return;
            if (net.Map.GetSunLight(global + IntVec3.UnitZ) < 8)
                return;

            // make grass grow anywhere, not just spread from existing grass
            BlockDefOf.Grass.Place(net.Map, global, 0, celll.Variation, 0);

            foreach (var n in global.GetNeighborsDiag())
            {
                if (!net.Map.TryGetCell(n, out Cell cell))
                    continue;
                if (cell.Block.Type != Block.Types.Grass)
                    continue;
                BlockDefOf.Grass.Place(net.Map, global, 0, celll.Variation, 0);
                return;
            }
        }
      
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Soil;
        }
    }
}
