using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components
{
    class LadderComponent : Component
    {
        public override string ComponentName
        {
            get { return "LadderComponent"; }
        }
        public override object Clone()
        {
            return new LadderComponent();
        }
    }
}
