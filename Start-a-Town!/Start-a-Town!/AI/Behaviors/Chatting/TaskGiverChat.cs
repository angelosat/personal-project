using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors.Chatting
{
    class TaskGiverChat : TaskGiver
    {
        protected override AITask TryAssignTask(Actor actor)
        {
            //return null;
            var convo = AIState.ConversationManager.Conversations.Where(c => c.Target == actor || c.Initiator == actor).FirstOrDefault();// as AIConversationManager.Conversation;
            if (convo != null)
            {
                if (convo.Target == actor)
                    return new AITaskChatNew(convo);
                throw new Exception();
            }

            var need = actor.GetNeed(NeedDef.Social);// NeedsComponent.GetNeed(actor, NeedDef.Social);
            if (need.Value > 40)
                return null;
            var npcs = actor.Map.Town.GetAgents();
            foreach (var npc in npcs)
            {
                if (npc == actor)
                    continue;
                var isBusyChatting = AIState.ConversationManager.Conversations.Where(c => c.Target == npc || c.Initiator == npc).Any();
                if (isBusyChatting)
                    continue;
                // check if npc is idle?
                var currentbhav = npc.GetComponent<AIComponent>().GetCurrentBehavior();
                if (!(currentbhav is BehaviorIdle))
                    continue;
                convo = AIState.ConversationManager.Start(actor, npc);
                //AIState.GetState(npc).CurrentTask = new AITaskChatNew(convo);
                return new AITaskChatNew(convo);
            }
            return null;
        }
    }
}
