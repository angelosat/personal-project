using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorCarryItem : BehaviorPerformTask
    {
        public override string Name
        {
            get
            {
                return "Picking up item";
            }
        }
        protected override IEnumerable<Behavior> GetSteps()
        {
            //yield return new BehaviorMoveTo(this.Task.Target, 1);
            yield return new BehaviorGetAtNewNew(this.Task.TargetA, PathingSync.FinishMode.Any);
            yield return new BehaviorInteractionNew(this.Task.TargetA, new HaulNew());
        }
    }
}
