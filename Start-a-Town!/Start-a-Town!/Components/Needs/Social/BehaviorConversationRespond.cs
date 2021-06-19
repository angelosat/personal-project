﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.AI;

namespace Start_a_Town_.Components.Needs.Social
{
    class BehaviorConversationRespond : Behavior
    {
        string TargetName;
        public BehaviorConversationRespond(string targetName)
        {
            this.TargetName = targetName;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var convo = AIState.ConversationManager.Conversations.Where(c => c.Target == parent).FirstOrDefault() as AIConversationManager.Conversation;
            if (convo == null)
                return BehaviorState.Fail;
            convo.State = AIConversationManager.Conversation.States.Accepted;
            state.CurrentConversation = convo;
            state.Blackboard[this.TargetName] = new TargetArgs(convo.Initiator);
            return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorConversationRespond(this.TargetName);
        }
    }
}
