using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorGetTargetFromTask : Behavior
    {
        readonly string TargetKey;
        public BehaviorGetTargetFromTask(string targetKey)
        {
            this.TargetKey = targetKey;
        }

        public override BehaviorState Execute(Actor parent, AIState state)
        {
            state[this.TargetKey] = state.CurrentTask.TargetA;
            return BehaviorState.Success;
        }
        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
