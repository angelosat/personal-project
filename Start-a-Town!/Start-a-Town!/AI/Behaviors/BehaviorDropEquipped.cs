using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorDropEquipped : BehaviorSequence
    {
        public BehaviorDropEquipped(GearType gearType)
        {
            this.Children = new List<Behavior>()
            {
                new BehaviorDomain(new BehaviorCondition((a, s) => GearComponent.GetSlot(a, GearType.Mainhand).Object != null),
                    new BehaviorInteractionNew("", new InteractionDropEquipped(gearType)))
            };
        }
    }
}
