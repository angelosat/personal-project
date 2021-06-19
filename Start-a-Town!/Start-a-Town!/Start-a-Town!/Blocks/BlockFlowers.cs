using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Materials;

namespace Start_a_Town_.Blocks
{
    class BlockFlowers : Block
    {
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Soil;
        }
        public BlockFlowers():base(Block.Types.FlowersNew,solid:false, opaque: false)
        {
            //this.MaterialType = MaterialType.Soil;
            //this.Material = Material.Soil;
            //this.AssetNames = "flowers/flowersred, flowers/flowersyellow, flowers/flowerswhite, flowers/flowerspurple";
            this.Variations.Add(Block.Atlas.Load("blocks/flowers/flowersred", Block.SliceBlockDepthMap, Block.NormalMap));
            this.Variations.Add(Block.Atlas.Load("blocks/flowers/flowersyellow", Block.SliceBlockDepthMap, Block.NormalMap));
            this.Variations.Add(Block.Atlas.Load("blocks/flowers/flowerswhite", Block.SliceBlockDepthMap, Block.NormalMap));
            this.Variations.Add(Block.Atlas.Load("blocks/flowers/flowerspurple", Block.SliceBlockDepthMap, Block.NormalMap));
        }
    }
}
