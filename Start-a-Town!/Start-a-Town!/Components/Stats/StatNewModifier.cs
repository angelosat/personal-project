using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class StatNewModifier
    {
        public StatNewModifierDef Def;
        float TicksRemaining;

        public StatNewModifier(StatNewModifierDef def)
        {
            this.Def = def;
        }
    }
}
