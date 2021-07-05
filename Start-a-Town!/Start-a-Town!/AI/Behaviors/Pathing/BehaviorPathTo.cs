using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorPathTo : BehaviorQueue
    {
        string TargetKey;
        public BehaviorPathTo(string targetKey)
        {
            this.TargetKey = targetKey;
            this.Children = new List<Behavior>()
            {
                new BehaviorSequence(
                    new BehaviorChaseTarget("step", .1f),
                    new BehaviorNextPathStep("path", "step")),
                new BehaviorSequence(
                    new BehaviorFindPathNew(targetKey, "path"),
                    new BehaviorNextPathStep("path", "step"))
            };
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            return base.Execute(parent, state);
        }
    }
}
