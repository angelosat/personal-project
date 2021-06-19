using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.Components.Needs.Social
{
    class BehaviorInitiateConversation : Behavior
    {
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var existingConvo = state.CurrentConversation;
            if (existingConvo != null)
                return BehaviorState.Fail;
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
                //state.Blackboard[this.TargetName] = new TargetArgs(npc);
                return BehaviorState.Success;
            }
            return BehaviorState.Fail;

            //state.CurrentConversation=AIState.ConversationManager.Start(parent)
        }
        public override object Clone()
        {
            return new BehaviorInitiateConversation();
        }
    }
}
