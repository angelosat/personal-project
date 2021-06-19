using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorChatGetTarget : Behavior
    {
        string TargetName;
        public BehaviorChatGetTarget(string targetName)
        {
            this.TargetName = targetName;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            state.Blackboard[this.TargetName] = new TargetArgs(state.CurrentConversation.Target);
            return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorChatGetTarget(this.TargetName);
        }
    }
}
