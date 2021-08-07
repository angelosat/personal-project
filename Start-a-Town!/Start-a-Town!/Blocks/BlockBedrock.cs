namespace Start_a_Town_.Blocks
{
    class BlockBedrock : Block
    {
        //public override MaterialDef GetMaterial(byte blockdata)
        //{
        //    return MaterialDefOf.Stone;
        //}
        public BlockBedrock()
            : base("Stone")
        {
            this.LoadVariations("smoothstone");
        }
    }
}
