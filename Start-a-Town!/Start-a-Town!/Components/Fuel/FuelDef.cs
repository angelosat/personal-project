using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    public class FuelDef : Def
    {
        public FuelDef(string name) : base(name)
        {
          
        }
        //static public readonly FuelDef None = new FuelDef("None");
        static public readonly FuelDef Organic = new FuelDef("Organic");
    }
}
