using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components.AI
{
    class AIMove : Behavior
    {
        public override BehaviorState Execute(GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            return BehaviorState.Fail;
        }
    }
}
