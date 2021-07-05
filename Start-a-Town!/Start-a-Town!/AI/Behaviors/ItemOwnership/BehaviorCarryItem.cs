using System.Collections.Generic;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorCarryItem : BehaviorPerformTask
    {
        public override string Name => "Picking up item";
        
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorGetAtNewNew(this.Task.TargetA, PathingSync.FinishMode.Any);
            yield return new BehaviorInteractionNew(this.Task.TargetA, new HaulNew());
        }
    }
}
