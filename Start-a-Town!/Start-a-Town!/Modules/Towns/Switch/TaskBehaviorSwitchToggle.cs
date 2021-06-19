using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
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
