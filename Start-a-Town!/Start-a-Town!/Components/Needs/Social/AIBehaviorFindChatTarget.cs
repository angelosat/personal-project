using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.Components.Needs.Social
{
    class AIBehaviorFindChatTarget : Behavior
    {
        string TargetName;
        public AIBehaviorFindChatTarget(string targetName)
        {
            this.TargetName = targetName;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            //var socialneed = NeedsComponent.GetNeed(parent, Need.Types.Social);
            var socialneed = parent.GetNeed(NeedDef.Social);
            if (socialneed.Value > 98)
                return BehaviorState.Fail;
            var npcs = parent.Map.Town.GetAgents();
            foreach(var npc in npcs)
            {
                if (npc == parent)
                    continue;
                // check if npc is idle?
                var currentbhav = npc.GetComponent<AIComponent>().GetCurrentBehavior();
                if (!(currentbhav is BehaviorIdle))
                    continue;
                state.CurrentConversation = AIState.ConversationManager.Start(parent, npc);
                state.Blackboard[this.TargetName] = new TargetArgs(npc);
                return BehaviorState.Success;
            }
            return BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new AIBehaviorFindChatTarget(this.TargetName);
        }
    }
}
