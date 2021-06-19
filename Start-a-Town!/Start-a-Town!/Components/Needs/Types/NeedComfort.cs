using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class NeedComfort : Need
    {
        public NeedComfort(Actor parent) : base(parent)
        {
        }

        //public override object Clone() => new NeedComfort(this.Parent);
    }
}
