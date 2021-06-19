using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public struct ToolAbility
    {
        public readonly ToolAbilityDef Def;
        public int Efficiency;

        public ToolAbility(ToolAbilityDef def, int efficiency)
        {
            this.Def = def;
            this.Efficiency = efficiency;
        }
    }
}
