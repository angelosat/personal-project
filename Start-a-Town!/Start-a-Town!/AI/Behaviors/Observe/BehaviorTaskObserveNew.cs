using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI
{
    class BehaviorTaskObserveNew : BehaviorPerformTask
    {
        public override string Name
        {
            get
            {
                return "Observing";
            }
        }
        protected override IEnumerable<Behavior> GetSteps()
        {
            //yield return new TempAssignParams("target");
            //yield return new BehaviorGoInteract("target", 1, new InteractionObserve());

            yield return new BehaviorGetAtNewNew(this.Task.TargetA);//, 1, false);
            yield return new BehaviorInteractionNew(this.Task.TargetA, new InteractionObserve());
        }
    }
}
