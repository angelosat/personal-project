using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    static class ZoneDefOf
    {
        public static readonly ZoneDef Stockpile = new ZoneStockpileDef();
        public static readonly ZoneDef Growing = new ZoneGrowingDef();
        //static ZoneDefOf()
        //{
        //    //Def.Register(Stockpile);
        //    //Def.Register(Growing);
        //    Def.Register(new ZoneStockpileDef());
        //    Def.Register(new ZoneGrowingDef());
        //}

        internal static void Init()
        {
        }
    }
}
