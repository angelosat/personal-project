using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.Components.Needs.Social
{
    class BehaviorConversationEngage : Behavior
    {
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var convo = state.CurrentConversation;
            if (convo == null)
                return BehaviorState.Fail;
            switch(convo.State)
            {
                case AIConversationManager.Conversation.States.Accepted:
                case AIConversationManager.Conversation.States.Started:
                    return BehaviorState.Running;

                case AIConversationManager.Conversation.States.Finished:
                    return BehaviorState.Success;

                default:
                    throw new Exception();
            }
            //return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorConversationEngage();
        }
    }
}
