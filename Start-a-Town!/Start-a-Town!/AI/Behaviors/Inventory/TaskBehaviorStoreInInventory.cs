﻿using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorStoreInInventory : BehaviorPerformTask
    {
        public override string Name => "Storing item in inventory";

        protected override IEnumerable<Behavior> GetSteps()
        {
            var index = TargetIndex.A;
            yield return new BehaviorGetAtNewNew(index);
            yield return new BehaviorInteractionNew(index, () => new InteractionHaul(this.Actor.CurrentTask.GetAmount(index)));
            yield return new BehaviorInteractionNew(index, ()=>new InteractionStoreHauled());
        }
        protected override bool InitExtraReservations()
        {
            return this.Task.Reserve(this.Actor, TargetIndex.A);
        }
    }
}
