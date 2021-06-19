using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI
{
    class TraitComposure : Trait
    {
        public override Trait.Types Type
        {
            get { return Types.Composure; }
        }

        public override string Name
        {
            get { return "Composure"; }
        }
    }
}
