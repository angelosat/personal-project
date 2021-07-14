namespace Start_a_Town_
{
    class BlockWoodPaneling : Block
    {
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.LightWood;
        }
        public BlockWoodPaneling()
            : base(Block.Types.WoodPaneling, 0, 1, true, true)
        {
            this.LoadVariations("woodvertical");
        }
    }
}
