using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors.Chatting
{
    class BehaviorChattingNew : BehaviorSequence
    {
        public BehaviorChattingNew()
        {
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
    }
}
