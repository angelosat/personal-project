﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class TaskBehaviorHaulFromInventory : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            // for now, just haul item from inventory and finish behavior, because cleaning up after the behavior, drops the current carried item if it's not an itemrole preference
            yield return new BehaviorInteractionNew(TargetIndex.A, () => new InteractionHaul());
            yield return new BehaviorInteractionNew(() => new InteractionThrow());

        }
    }
}
