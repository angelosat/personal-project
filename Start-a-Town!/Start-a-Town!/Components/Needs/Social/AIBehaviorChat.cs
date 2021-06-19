using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.Components.Needs.Social
{
    class AIBehaviorChat : BehaviorSelector
    {
        public AIBehaviorChat()
        {
            this.Children = new List<Behavior>()
            {
                new BehaviorSequence(
                    new BehaviorHasConversation("target"),
                    new BehaviorSequence(
                        new BehaviorIsConversationInitiator(),
                        new BehaviorGetAtNewNew("target")
                        ),
                    new BehaviorConversationEngage()),

                new BehaviorSelector(
                    new BehaviorConversationRespond("target"),
                    new AIBehaviorFindChatTarget("target")),
                
            };
        }
    }
}
