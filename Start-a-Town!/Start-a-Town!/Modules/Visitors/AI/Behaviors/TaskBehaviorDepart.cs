using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_
{
    class TaskBehaviorDepart : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return BehaviorHelper.MoveTo(TargetIndex.A);
            yield return new BehaviorInteractionNew(() => new InteractionDepart());
        }
    }
}
