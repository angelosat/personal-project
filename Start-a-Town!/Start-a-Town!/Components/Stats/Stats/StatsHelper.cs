using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
