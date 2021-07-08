namespace Start_a_Town_.Blocks
{
    class BlockBedrock : Block
    {
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Stone;
        }
        public BlockBedrock()
            : base(Block.Types.Stone)
        {
            AssetNames = "smoothstone";
        }
    }
}
