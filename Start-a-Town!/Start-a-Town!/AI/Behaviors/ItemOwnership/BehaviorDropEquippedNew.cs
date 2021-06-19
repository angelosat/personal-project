using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components;
using Start_a_Town_.AI.Behaviors.ItemOwnership;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorDropEquippedNew : BehaviorPerformTask
    {
        protected override IEnumerable<Behavior> GetSteps()
        {
            //var task = this.Task as TaskEquipment;
            //yield return new BehaviorInteractionNew(new DropEquipped(GearType.Mainhand));
            //yield return new BehaviorInteractionNew(this.Task.Target, new DropEquippedTarget());
            yield return new BehaviorStartInteraction(new DropEquippedTarget(this.Task.TargetA));
        }
    }
}
