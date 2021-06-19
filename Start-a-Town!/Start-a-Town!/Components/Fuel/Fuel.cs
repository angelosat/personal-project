using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public struct Fuel
    {
        public readonly FuelDef Def;
        public float Value;

        public Fuel(FuelDef def, float value)
        {
            Def = def;
            Value = value;
        }
    }
}
