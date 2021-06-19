using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class NeedEnergy : Need
    {
        public NeedEnergy(Actor parent) : base(parent)
        {
        }
        //public override object Clone() => new NeedEnergy();
    }
}
