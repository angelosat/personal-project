using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorDropHauled : Behavior
    {
        Interaction Goal;

        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (parent.Velocity != Vector3.Zero)
            {
                var acceleration = parent.GetComponent<MobileComponent>().Acceleration;
                if (acceleration != 0)
                    parent.MoveToggle(false);
                return BehaviorState.Running;
            }
            var net = parent.Net;

            if (this.Goal == null)
                this.Goal = new InteractionThrow(true);
            var target = TargetArgs.Null;

            switch (this.Goal.State)
            {
                case Interaction.States.Unstarted:
                    if (!this.Goal.IsValid(parent, target))
                    {
                        // arrived to perform interaction but interaction no longer valid!
                        // TODO: post message to ai log?
                        return BehaviorState.Fail;
                    }
                    (net as Server).AIHandler.AIInteract(parent, this.Goal, target);
                    return BehaviorState.Running;

                case Interaction.States.Running:
                    return BehaviorState.Running;

                case Interaction.States.Finished:
                    this.Goal = null;
                    return BehaviorState.Success;

                case Interaction.States.Failed:
                    return BehaviorState.Fail;

                default:
                    break;
            }
            return BehaviorState.Running;
        }

        public override object Clone()
        {
            return new BehaviorDropHauled();
        }
    }
}
