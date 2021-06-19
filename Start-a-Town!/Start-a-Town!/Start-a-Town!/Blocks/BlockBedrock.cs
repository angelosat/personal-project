using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Materials;

namespace Start_a_Town_.Blocks
{
    class BlockBedrock : Block
    {
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Stone;
        }
        public BlockBedrock()
            : base(Block.Types.Stone, GameObject.Types.Rock)
        {
            //Material = Material.Stone,
            AssetNames = "smoothstone";
        }
    }
}
