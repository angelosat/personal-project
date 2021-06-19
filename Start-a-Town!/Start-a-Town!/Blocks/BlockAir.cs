using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Blocks
{
    class BlockAir : Block
    {
        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Air;
        }
        public BlockAir()
            : base(Types.Air, 1, 0, false, false)
        {

        }
        public override void Remove(IMap map, Microsoft.Xna.Framework.Vector3 global, bool notify = true)
        {
        }
    }
}
