using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorChatting : BehaviorSequence
    {
        public BehaviorChatting()
        {
            this.Children = new List<Behavior>()
            {
                new BehaviorIsChatting(),
                new BehaviorSelector(
                    new BehaviorInverter(new BehaviorIsConversationInitiator()),
                    new BehaviorSequence(
                        new BehaviorChatGetTarget("target"),
                        new BehaviorMoveTo("target", 1))
                    ),
                new BehaviorChatEngage()
            };
        }
        //public override BehaviorState Execute(Entity parent, AIState state)
        //{
        //    throw new NotImplementedException();
        //}
        //public override object Clone()
        //{
        //    return new BehaviorChatting();
        //}
    }
}
