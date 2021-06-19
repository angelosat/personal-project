﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorDequeue : Behavior
    {
        string QueueName;//, TargetName;
        string[] Targets;
        public BehaviorDequeue(string queue, params string[] targetVar)
        {
            this.QueueName = queue;
            this.Targets = targetVar;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var queue = state.Blackboard[this.QueueName] as Queue<object>;
            if (queue.Count > 0)
            {
                //state.Blackboard[this.TargetName] = queue.Dequeue();
                for (int i = 0; i < this.Targets.Length; i++)
                {
                    var target = this.Targets[i];
                    state.Blackboard[target] = queue.Dequeue();
                }
                return BehaviorState.Success;
            }
            return BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new BehaviorDequeue(this.QueueName, this.Targets);
        }
    }
}
