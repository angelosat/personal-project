using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class BehaviorWait : Behavior
    {
        public Func<bool> EndCondition = () => false;
        public Action TickAction = () => { };
        public BehaviorWait()
        {

        }
        public BehaviorWait(Func<bool> p)
        {
            this.EndCondition = p;
        }

        public override object Clone()
        {
            throw new NotImplementedException();
        }

        public override BehaviorState Execute(Actor parent, AIState state)
        {
            state.CurrentTask.TicksWaited++;
            this.TickAction();
            if (this.EndCondition())
                return BehaviorState.Success;
            return BehaviorState.Running;
        }
    }
}
