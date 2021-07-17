namespace Start_a_Town_
{
    static class StatsHelper
    {
        static public float GetStat(this GameObject parent, StatNewDef statDef)
        {
            return statDef.GetValue(parent);
        }
    }
}
