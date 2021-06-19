using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorInRange : Behavior
    {
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var threat = state.Threats.First();
            return (threat.Entity.Global - parent.Global).Length() <= 1 ? BehaviorState.Success : BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new BehaviorInRange();
        }
    }
}
