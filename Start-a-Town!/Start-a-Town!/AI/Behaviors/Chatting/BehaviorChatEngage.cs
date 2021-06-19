using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorChatEngage : Behavior
    {
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var convo = state.CurrentConversation;
            if (convo == null)
                return BehaviorState.Fail;

            //var prog = convo.Progress;
            //prog.Value += 1;
            //if (prog.Percentage >= 1)
            //{
            //    prog.Value = 0;
            //    var social = NeedsComponent.GetNeed(parent, Components.Needs.Need.Types.Social);
            //    social.Value += 1;
            //    if (social.Percentage == 1)
            //        convo.State = AIConversationManager.Conversation.States.Finished;
            //}

            switch (convo.State)
            {
                case AIConversationManager.Conversation.States.Requested:
                    return BehaviorState.Running;// Success;//Running;

                case AIConversationManager.Conversation.States.Accepted:
                case AIConversationManager.Conversation.States.Started:
                    if (convo.Initiator == parent)
                        convo.Tick();
                    return BehaviorState.Running;// Success;//Running;

                case AIConversationManager.Conversation.States.Finished:
                    AIState.ConversationManager.Conversations.Remove(state.CurrentConversation);
                    state.CurrentConversation = null;
                    return BehaviorState.Success;

                default:
                    throw new Exception();
            }
            //return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorChatEngage();
        }
    }
}
