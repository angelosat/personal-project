using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI
{
    class AIJobConstruction : AIJob
    {
        public override void Reserve(GameObject actor)
        {

        }

        public override object Clone()
        {
            return new AIJobConstruction();
        }
    }
}
