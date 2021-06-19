using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components
{
    class HandsComponent : Component
    {
        public override string ComponentName
        {
            get { return "Hands"; }
        }
        public override object Clone()
        {
            return new HandsComponent();
        }
    }
}
