using System;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    internal class BehaviorHandleResources : Behavior
    {
        public override object Clone()
        {
            return new BehaviorHandleResources();
        }

        public override BehaviorState Execute(Actor parent, AIState state)
        {
            return BehaviorState.Fail;

            //if (parent.GetResource(ResourceDef.Stamina).Value <= 0 && parent.IsHauling())
            //    parent.Interact(new InteractionThrow(true));
            //return BehaviorState.Fail;
        }
    }
}