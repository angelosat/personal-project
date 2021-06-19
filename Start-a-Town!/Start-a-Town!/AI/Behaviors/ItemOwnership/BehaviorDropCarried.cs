using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI.Behaviors.ItemOwnership
{
    class BehaviorDropCarried : BehaviorPerformTask
    {
        public override string Name
        {
            get
            {
                return "Dropping carried";
            }
        }
        protected override IEnumerable<Behavior> GetSteps()
        {
            var target = this.Task.TargetA;
            var amount = this.Task.AmountA;
            yield return new BehaviorGetAtNewNew(target);
            yield return new BehaviorInteractionNew(target, new UseHauledOnTarget(amount));
        }
    }
}
