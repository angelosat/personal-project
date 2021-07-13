﻿using Start_a_Town_.Blocks;

namespace Start_a_Town_
{
    public class BlockKitchenEntity : BlockEntity
    {
        public BlockKitchenEntity()
        {
            this.Comps.Add(new BlockEntityCompWorkstation(IsWorkstation.Types.Baking, IsWorkstation.Types.PlantProcessing));
            this.Comps.Add(new BlockEntityCompDeconstructible());
            this.Comps.Add(new BlockEntityCompRefuelable());
        }
        public override object Clone()
        {
            return new BlockKitchenEntity();
        }
    }
}
