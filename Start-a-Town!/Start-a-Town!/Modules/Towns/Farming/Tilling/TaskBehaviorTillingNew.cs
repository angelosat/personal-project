﻿using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Towns.Farming
{
    class TaskBehaviorTillingNew : BehaviorPerformTask
    {
        public const TargetIndex TargetInd = TargetIndex.A;
        TargetArgs Target { get { return this.Task.GetTarget(TargetInd); } }
        protected override IEnumerable<Behavior> GetSteps()
        {
            bool fail() => !this.Actor.Map.Town.FarmingManager.IsTillable(Target.Global);
            this.FailOn(fail);
            yield return new BehaviorGrabTool().FailOnForbidden(TargetIndex.Tool);
            yield return new BehaviorGetAtNewNew(TargetInd);
            yield return new BehaviorInteractionNew(TargetInd, new InteractionTilling());
        }
        protected override bool InitExtraReservations()
        {
            return this.Actor.Reserve(this.Task.TargetA, 1);
        }
    }
}
