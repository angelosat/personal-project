using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class NeedGuidance : Need
    {
        //public override object Clone() => new NeedGuidance();
        public NeedGuidance(Actor parent) : base(parent)
        {
        }
    }
}
