using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class ZoneStockpileDef : ZoneDef
    {
        //public ZoneStockpileDef()
        //{
        //    this.ZoneType = typeof(Stockpile);
        //}
        public override Type ZoneType => typeof(Stockpile);

        public override bool IsValidLocation(IMap map, IntVec3 global)
        {
            if (!map.IsSolid(global))
                return false;
            if (map.IsSolid(global.Above()))
                return false;
            return true;
        }
    }
}
