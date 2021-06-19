using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class MultiBlockComponent : Component
    {
        public override object Clone()
        {
            return new MultiBlockComponent();
        }
    }
}
