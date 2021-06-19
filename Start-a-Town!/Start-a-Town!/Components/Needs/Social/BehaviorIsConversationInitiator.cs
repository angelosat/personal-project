﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_.Components.Needs.Social
{
    class BehaviorIsConversationInitiator : Behavior
    {
        public override BehaviorState Execute(Actor parent, Start_a_Town_.AI.AIState state)
        {
            var convo = state.CurrentConversation;
            return convo.Initiator == parent ? BehaviorState.Success : BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new BehaviorIsConversationInitiator();
        }
    }
}
