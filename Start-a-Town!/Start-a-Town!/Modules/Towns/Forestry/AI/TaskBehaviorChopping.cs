﻿using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class TaskBehaviorChopping : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorGrabTool();
            yield return new BehaviorGetAtNewNew(TargetIndex.A);
            yield return new BehaviorInteractionNew(TargetIndex.A, new InteractionChoppingSimple());
        }
        
        public override bool HasFailedOrEnded()
        {
            var tree = this.Task.TargetA.Object;
            var isvalid =
                !this.Task.Tool.IsForbidden &&
                !tree.IsForbidden &&
                tree != null && tree.Exists &&
                this.Actor.Map.Town.ChoppingManager.IsChoppingTask(tree);
            return !isvalid;
        }

        protected override bool InitExtraReservations()
        {
            return this.Actor.Reserve(TargetIndex.A);
        }
    }
}