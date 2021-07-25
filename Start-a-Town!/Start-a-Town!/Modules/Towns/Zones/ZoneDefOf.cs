namespace Start_a_Town_
{
    static class ZoneDefOf
    {
        public static readonly ZoneDef Stockpile = new ZoneStockpileDef();
        public static readonly ZoneDef Growing = new ZoneGrowingDef();
       
        internal static void Init()
        {
            Def.Register(Stockpile);
            Def.Register(Growing);
        }
    }
}
