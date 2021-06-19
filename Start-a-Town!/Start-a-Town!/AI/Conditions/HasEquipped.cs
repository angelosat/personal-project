using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class HasEquipped : BehaviorCondition
    {
        Func<GameObject, bool> EquippedItemCondition;
        public HasEquipped(Func<GameObject, bool> cond)
        {
            this.EquippedItemCondition = cond;
        }
        public override bool Evaluate(GameObject agent, AIState state)
        {
            var equip = GearComponent.GetSlot(agent, GearType.Mainhand);
            if (equip.Object == null)
                return false;
            return this.EquippedItemCondition(equip.Object);
        }
    }
}
