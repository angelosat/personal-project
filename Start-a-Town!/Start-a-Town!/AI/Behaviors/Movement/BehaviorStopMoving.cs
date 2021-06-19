using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorStopMoving : Behavior
    {
        public override BehaviorState Execute(Actor parent, AIState state)
        {
                    //(parent.Net as Server).AIHandler.AIStopMove(parent);
            parent.MoveToggle(false);
                return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorStopMoving();
        }
    }
}
