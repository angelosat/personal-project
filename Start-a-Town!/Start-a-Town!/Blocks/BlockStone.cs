using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Towns.Constructions;

namespace Start_a_Town_
{
    class BlockStone : Block
    {
        public override bool IsMinable => true;

        public BlockStone()
            : base(Block.Types.Cobblestone, 0, 1, true, true)
        {
            this.LoadVariations("stone5height19");

            this.Recipe = new BlockRecipe(
                Reaction.Reagent.Create(new Reaction.Reagent("Base", Reaction.Reagent.IsOfMaterial(MaterialDefOf.Stone), Reaction.Reagent.CanProduce(Reaction.Product.Types.Blocks))),
                new BlockRecipe.Product(this)
                );
            this.Ingredient = new Ingredient(RawMaterialDef.Boulders, MaterialDefOf.Stone, null, 1);
            ConstructionsManager.Walls.Add(this.Recipe);
        }
        public override Particles.ParticleEmitterSphere GetEmitter()
        {
            return base.GetDustEmitter();
        }
        public override ContextAction GetRightClickAction(Vector3 global)
        {
            return new ContextAction(() => "Mine", () => { Net.Client.PlayerInteract(new TargetArgs(global)); });
        }
        public override Material GetMaterial(byte data)
        {
            return MaterialDefOf.Stone;
        }
    }
}
