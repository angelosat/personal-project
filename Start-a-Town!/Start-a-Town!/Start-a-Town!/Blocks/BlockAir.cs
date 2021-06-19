using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Materials;

namespace Start_a_Town_.Blocks
{
    class BlockAir : Block
    {
        public override Material GetMaterial(byte blockdata)
        {
            return Material.Air;
        }
        public BlockAir()
            : base(Types.Air, 1, 0, false, false)
        {

        }
        public override void Remove(GameModes.IMap map, Microsoft.Xna.Framework.Vector3 global)
        {
            
        }
    }
}
