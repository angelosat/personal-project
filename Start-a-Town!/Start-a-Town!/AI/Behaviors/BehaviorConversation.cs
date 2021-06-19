using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorConversation : Behavior
    {
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            //var convos = AIState.ConversationManager.Conversations;
            var convo = AIState.ConversationManager.Conversations.Where(c => c.Target == parent).FirstOrDefault() as AIConversationManager.Conversation;
            convo.State = AIConversationManager.Conversation.States.Accepted;
            return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorConversation();
        }
    }
}
