using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors.Chatting
{
    class BehaviorInitiateConversationNew : Behavior
    {
        public override string Name
        {
            get
            {
                return "Conversation";
            }
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var existingConvo = state.CurrentConversation;
            if (existingConvo != null)
                return BehaviorState.Success;
            var npcs = parent.Map.Town.GetAgents();
            foreach (var npc in npcs)
            {
                if (npc == parent)
                    continue;
                // check if npc is idle?
                var currentbhav = npc.GetComponent<AIComponent>().GetCurrentBehavior();
                if (!(currentbhav is BehaviorIdle))
                    continue;
                state.CurrentConversation = AIState.ConversationManager.Start(parent, npc);
                // todo: must set relevant task to target npc
                //state.Blackboard[this.TargetName] = new TargetArgs(npc);
                return BehaviorState.Success;
            }
            return BehaviorState.Fail;

        }
        public override object Clone()
        {
            return new BehaviorInitiateConversationNew();
        }
    }
}
