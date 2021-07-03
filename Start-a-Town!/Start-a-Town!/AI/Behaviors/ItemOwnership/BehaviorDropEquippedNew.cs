using System.Collections.Generic;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorDropEquippedNew : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorStartInteraction(new DropEquippedTarget(this.Task.TargetA));
        }
    }
}
