using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors.Chatting
{
    class TaskBehaviorChat : BehaviorSequence
    {
        public override string Name
        {
            get
            {
                return "Chatting";
            }
        }
        private AITaskChatNew Task;

        public TaskBehaviorChat(AITaskChatNew task)
        {
            // TODO: Complete member initialization
            this.Task = task;
            this.Children = new List<Behavior>()
            {
                new BehaviorIsChattingNew(),
                new BehaviorSelector(
                    new BehaviorInverter(new BehaviorIsConversationInitiator()),
                    new BehaviorSequence(
                        new BehaviorChatGetTarget("target"),
                        new BehaviorMoveTo("target", 1))
                    ),
                new BehaviorChatEngage()
            };
        }
        public override object Clone()
        {
            return new TaskBehaviorChat(this.Task);
        }
        class BehaviorIsChattingNew : Behavior
        {
            public override BehaviorState Execute(Actor parent, AIState state)
            {
                //var convo = state.CurrentConversation;
                //if (convo != null)
                //{
                //    switch (convo.State)
                //    {
                //        case AIConversationManager.Conversation.States.Finished:
                //            state.CurrentConversation = null;
                //            return BehaviorState.Fail;
                //            break;

                //        default:
                //            break;
                //    }
                //    return BehaviorState.Success;
                //}
                //else
                //{
                var convo = AIState.ConversationManager.Conversations.Where(c => c.Target == parent || c.Initiator == parent).FirstOrDefault();// as AIConversationManager.Conversation;
                if (convo.State == AIConversationManager.Conversation.States.Requested)
                {
                    if (parent == convo.Initiator)
                    {
                        return BehaviorState.Running;
                    }
                    else if (parent == convo.Target)
                    {
                        convo.State = AIConversationManager.Conversation.States.Accepted;
                        return BehaviorState.Running;
                    }
                }
                    state.CurrentConversation = convo;
                    return BehaviorState.Success;
                //}
                //return BehaviorState.Fail;
            }

            public override object Clone()
            {
                return new BehaviorIsChattingNew();
            }
        }
    }
}