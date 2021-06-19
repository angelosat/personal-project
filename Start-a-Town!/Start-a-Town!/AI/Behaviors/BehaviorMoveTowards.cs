using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorMoveTowards : Behavior
    {
        string Target;
        public BehaviorMoveTowards(string target)
        {
            this.Target = target;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            throw new NotImplementedException();
        }
        public override object Clone()
        {
            return new BehaviorMoveTowards(this.Target);
        }
    }
}
