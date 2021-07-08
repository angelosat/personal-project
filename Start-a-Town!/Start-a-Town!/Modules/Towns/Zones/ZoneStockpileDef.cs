using System;

namespace Start_a_Town_
{
    class ZoneStockpileDef : ZoneDef
    {
        public override Type ZoneType => typeof(Stockpile);

        public override bool IsValidLocation(IMap map, IntVec3 global)
        {
            if (!map.IsSolid(global))
                return false;
            if (map.IsSolid(global.Above))
                return false;
            return true;
        }
    }
}
