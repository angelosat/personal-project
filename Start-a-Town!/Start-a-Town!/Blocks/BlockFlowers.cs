namespace Start_a_Town_.Blocks
{
    class BlockFlowers : Block
    {
        public override MaterialDef GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Soil;
        }
        public BlockFlowers():base(Block.Types.FlowersNew,solid:false, opaque: false)
        {
            this.Variations.Add(Block.Atlas.Load("blocks/flowers/flowersred", Block.SliceBlockDepthMap, Block.NormalMap));
            this.Variations.Add(Block.Atlas.Load("blocks/flowers/flowersyellow", Block.SliceBlockDepthMap, Block.NormalMap));
            this.Variations.Add(Block.Atlas.Load("blocks/flowers/flowerswhite", Block.SliceBlockDepthMap, Block.NormalMap));
            this.Variations.Add(Block.Atlas.Load("blocks/flowers/flowerspurple", Block.SliceBlockDepthMap, Block.NormalMap));
        }
    }
}
