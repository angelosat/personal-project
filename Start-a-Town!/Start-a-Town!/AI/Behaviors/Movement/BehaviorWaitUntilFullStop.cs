using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorWaitUntilFullStop : Behavior
    {
        public override string Name
        {
            get
            {
                return "Wait until full stop";
            }
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (parent.Velocity == Vector3.Zero)
                return BehaviorState.Success;

            var acceleration = parent.Acceleration;
            if (acceleration != 0)
                AIManager.AIStopMoveNew(parent);
            return BehaviorState.Running;
        }
        public override object Clone()
        {
            throw new NotImplementedException();
        }
    }
}
