using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorTargetEntityExists : BehaviorCondition
    {
        string TargetKey;
        public BehaviorTargetEntityExists(string targetKey)
        {
            this.TargetKey = targetKey;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var target = state[this.TargetKey] as TargetArgs;
            if (target.Type == TargetType.Position)
                return BehaviorState.Success;
            var exists = target.Object != null ? target.Object.Exists : false;
            return exists ? BehaviorState.Success : BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new BehaviorTargetEntityExists(this.TargetKey);
        }
    }
}
