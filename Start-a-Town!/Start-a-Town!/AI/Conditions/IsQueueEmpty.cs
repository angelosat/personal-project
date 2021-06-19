using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class IsQueueEmpty : BehaviorCondition
    {
        string QueueKey;
        public IsQueueEmpty(string queueKey)
        {
            this.QueueKey = queueKey;
        }
        public override bool Evaluate(GameObject agent, AIState state)
        {
            var queue = state[this.QueueKey] as Queue<object>;
            var isempty = !queue.Any();
            return isempty;
        }
    }
}
