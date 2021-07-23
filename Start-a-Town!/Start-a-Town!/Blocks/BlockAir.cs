﻿namespace Start_a_Town_.Blocks
{
    class BlockAir : Block
    {
        public override MaterialDef GetMaterial(byte blockdata)
        {
            return MaterialDefOf.Air;
        }
        public BlockAir()
            : base(Types.Air, 1, 0, false, false)
        {

        }
    }
}
