﻿namespace Start_a_Town_.Blocks
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
        public override void Remove(MapBase map, IntVec3 global, bool notify = true)
        {
        }
    }
}
