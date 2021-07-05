using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
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
            : base(Block.Types.Sand, GameObject.Types.Sand)
        {
            this.AssetNames = "sand1";

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterialType(MaterialType.Soil), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                new BlockRecipe.Product(this)
                );

            this.Ingredient = new Ingredient(RawMaterialDef.Bags, MaterialDefOf.Sand, null, 1);
            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(
                    new Reaction.Reagent(
                        "Base",
                        Reaction.Reagent.IsOfMaterial(MaterialDefOf.Sand)
                        )),
                    new BlockRecipe.Product(this),
                    ToolAbilityDef.Building);
            Towns.Constructions.ConstructionsManager.Walls.Add(this.Recipe);
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
