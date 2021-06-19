using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorIsChatting : Behavior
    {
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var convo = state.CurrentConversation;
            if (convo != null)
            {
                switch(convo.State)
                {
                    case AIConversationManager.Conversation.States.Finished:
                        state.CurrentConversation = null;
                        return BehaviorState.Fail;

                    default:
                        break;
                }
                return BehaviorState.Success;
            }
            else
            {
                convo = AIState.ConversationManager.Conversations.Where(c => c.Target == parent).FirstOrDefault() as AIConversationManager.Conversation;
                if (convo == null)
                    return BehaviorState.Fail;
                convo.State = AIConversationManager.Conversation.States.Accepted;
                state.CurrentConversation = convo;
                return BehaviorState.Success;
            }
            //return BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new BehaviorIsChatting();
        }
    }
}
