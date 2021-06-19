using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Terraforming.Mutators
{
    class Empty : Terraformer
    {
        public Empty()
        {
            this.ID = Terraformer.Types.Empty;
            this.Name = "Empty";
        }
        public override Block.Types Initialize(IWorld w, Cell c, int x, int y, int z, Net.RandomThreaded r)
        {
            return Block.Types.Air;
        }
        public override object Clone()
        {
            return new Empty();
        }
    }
}
