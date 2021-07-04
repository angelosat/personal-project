﻿using System.Collections.Generic;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_
{
    class TaskBehaviorSwitchToggle : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            this.FailOnNoDesignation(TargetIndex.A, DesignationDef.Switch);
            yield return new BehaviorGetAtNewNew(TargetIndex.A);
            yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionSwitch());
        }
    }
}
