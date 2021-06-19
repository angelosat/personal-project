using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors.Chatting
{
    class BehaviorIsChattingNew : Behavior
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
                        //break;

                    default:
                        break;
                }
                return BehaviorState.Success;
            }
            else
            {
                convo = AIState.ConversationManager.Conversations.Where(c => c.Target == parent).FirstOrDefault();// as AIConversationManager.Conversation;
                if (convo == null)
                {
                    var npcs = parent.Map.Town.GetAgents();
                    foreach (var npc in npcs)
                    {
                        if (npc == parent)
                            continue;
                        // check if npc is idle?
                        var currentbhav = npc.GetComponent<AIComponent>().GetCurrentBehavior();
                        if (!(currentbhav is BehaviorIdle))
                            continue;
                        convo = AIState.ConversationManager.Start(parent, npc);
                        AIState.GetState(npc).SetTask(new AITaskChatNew());
                        state.CurrentConversation = convo;
                        // todo: must set relevant task to target npc
                        //state.Blackboard[this.TargetName] = new TargetArgs(npc);
                        return BehaviorState.Success;
                    }
                }
                else
                {
                    convo.State = AIConversationManager.Conversation.States.Accepted;
                    state.CurrentConversation = convo;
                    return BehaviorState.Success;
                }
            }
            return BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new BehaviorIsChattingNew();
        }
    }
}
