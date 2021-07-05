using System;
using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    [Obsolete]
    class BehaviorGoInteract : BehaviorQueue
    {
        public BehaviorGoInteract(string targetKey, int range, Interaction interaction)
        {
            this.Children = new List<Behavior>()
            {
                new BehaviorDomain(new ConditionAll(new BehaviorTargetEntityExists(targetKey), new IsAt(targetKey, range)),
                    new BehaviorInteractionNew(targetKey, interaction)),
                new BehaviorDomain(new BehaviorTargetEntityExists(targetKey),
                    new BehaviorGetAtNewNew(targetKey))
            };
        }
       
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            return base.Execute(parent, state);
        }
    }
}
