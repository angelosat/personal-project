using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Blocks;
using Start_a_Town_.Crafting;
using Start_a_Town_.Tokens;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class BlockKitchenEntity : BlockEntity//Workstation
    {
        public BlockKitchenEntity()
        {
            this.Comps.Add(new BlockEntityCompWorkstation(IsWorkstation.Types.Baking, IsWorkstation.Types.PlantProcessing));
            this.Comps.Add(new BlockEntityCompDeconstructible());
            this.Comps.Add(new EntityCompRefuelable());
        }
        public override object Clone()
        {
            return new BlockKitchenEntity();
        }
    }
}
