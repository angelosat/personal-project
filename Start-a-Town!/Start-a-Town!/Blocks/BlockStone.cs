namespace Start_a_Town_
{
    class BlockStone : Block
    {
        public override bool IsMinable => true;

        public BlockStone()
            : base(Block.Types.Cobblestone, 0, 1, true, true)
        {
            this.LoadVariations("stone5height19");
            this.Ingredient = new Ingredient(RawMaterialDef.Boulders, MaterialDefOf.Stone, null, 1);
            this.ToggleConstructionCategory(ConstructionsManager.Walls, true);
        }
        public override Particles.ParticleEmitterSphere GetEmitter()
        {
            return base.GetDustEmitter();
        }
        public override Material GetMaterial(byte data)
        {
            return MaterialDefOf.Stone;
        }
    }
}
