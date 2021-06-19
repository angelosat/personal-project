using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.Components.Needs.Social
{
    class BehaviorHasConversation : Behavior
    {
        string TargetName;
        public BehaviorHasConversation(string targetName)
        {
            this.TargetName = targetName;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var convo = state.CurrentConversation;
            if (convo == null)
                return BehaviorState.Fail;
            state.Blackboard[this.TargetName] = convo.Initiator == parent ? new TargetArgs(convo.Target) : new TargetArgs(convo.Initiator);
            return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorHasConversation(this.TargetName);
        }
    }
}
