using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.AI.Behaviors.ItemOwnership;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_
{
    class TaskBehaviorChoppingNew : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            //if (this.Task.Tool.HasObject)
            //{
                //yield return BehaviorReserve.Reserve(this.Task.Tool);
                yield return new BehaviorGrabTool();
            //}
            yield return new BehaviorGetAtNewNew(TargetIndex.A);//, 1);
            yield return new BehaviorInteractionNew(TargetIndex.A, new InteractionChoppingSimple());
        }
        //protected override IEnumerable<Behavior> GetSteps()
        //{
        //    if (this.Task.Tool.HasObject)
        //    {
        //        //yield return BehaviorReserve.Reserve(this.Task.Tool);
        //        yield return new BehaviorGrabTool(this.Task.Tool);
        //    }
        //    var tree = this.Task.TargetA;
        //    yield return new BehaviorGetAtNewNew(tree);//, 1);
        //    yield return new BehaviorInteractionNew(tree, new InteractionChoppingSimple());
        //}
        public override bool HasFailedOrEnded()
        {
            var tree = this.Task.TargetA.Object;
            //return tree == null || !tree.Exists || !this.Actor.Map.Town.ChoppingManager.IsChoppingTask(tree);// || !this.Actor.Map.Town.FarmingManager.IsChoppableTree(tree);
            var isvalid =
                !this.Task.Tool.IsForbidden &&
                !tree.IsForbidden &&
                (tree != null && tree.IsSpawned) &&
                (this.Actor.Map.Town.ChoppingManager.IsChoppingTask(tree) || this.Actor.Map.Town.FarmingManager.IsChoppableTree(tree));
            return !isvalid;
        }
    }
}
