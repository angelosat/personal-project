using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class TaskBehaviorChoppingNew : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorGrabTool();
            yield return new BehaviorGetAtNewNew(TargetIndex.A);//, 1);
            yield return new BehaviorInteractionNew(TargetIndex.A, new InteractionChoppingSimple());
        }
        
        public override bool HasFailedOrEnded()
        {
            var tree = this.Task.TargetA.Object;
            var isvalid =
                !this.Task.Tool.IsForbidden &&
                !tree.IsForbidden &&
                tree != null && tree.IsSpawned &&
                this.Actor.Map.Town.ChoppingManager.IsChoppingTask(tree);
            return !isvalid;
        }
    }
}
