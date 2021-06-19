using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Blocks
{
    class BlockBedrock : Block
    {
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Stone;
        }
        public BlockBedrock()
            : base(Block.Types.Stone, GameObject.Types.Rock)
        {
            //Material = Material.Stone,
            AssetNames = "smoothstone";
        }
    }
}
