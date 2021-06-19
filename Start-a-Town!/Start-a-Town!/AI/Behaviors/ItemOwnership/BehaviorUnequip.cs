using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI.Behaviors.ItemOwnership
{
    class BehaviorUnequip : BehaviorPerformTask
    {
        public override string Name
        {
            get
            {
                return "Unequipping";
            }
        }
        protected override IEnumerable<Behavior> GetSteps()
        {
            yield return new BehaviorInteractionNew(this.Task.TargetA, new UnequipItem());
        }
    }
}
