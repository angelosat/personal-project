using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorStartMoving : Behavior
    {
        public bool Sprint;
        public BehaviorStartMoving(bool sprint = true)
        {
            this.Sprint = sprint;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (state.Path == null)
                return BehaviorState.Fail;
            parent.MoveToggle(true);
            parent.WalkToggle(!this.Sprint);
            return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorStartMoving(this.Sprint);
        }
    }
}
