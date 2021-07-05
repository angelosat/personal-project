﻿using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorMoveTo : BehaviorQueue
    {
        public BehaviorMoveTo(string targetKey, int range)
        {
            this.Children = new List<Behavior>()
            {
                new BehaviorDomain(new IsAt(targetKey, range),
                    new BehaviorStopMoving()),
                new BehaviorGetAtNewNew(targetKey)//, range)
            };
        }

        public BehaviorMoveTo(TargetArgs targetArgs, int range)
        {
            this.Children = new List<Behavior>()
            {
                new BehaviorDomain(new IsAt(targetArgs, range),
                    new BehaviorStopMoving()),
                new BehaviorGetAtNewNew(targetArgs)
            };
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var result = base.Execute(parent, state);
            return result;
        }
    }
}
