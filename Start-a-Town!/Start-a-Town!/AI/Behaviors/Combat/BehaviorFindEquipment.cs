using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorFindEquipment : BehaviorSequence
    {
        public BehaviorFindEquipment()
        {
            this.Children = new List<Behavior>()
            {
                new BehaviorFindWeapon("weapon"),
                //new BehaviorReserveItem("weapon", 1),
                //new BehaviorGetAt("weapon", 1),
                //new BehaviorInteractionNew("weapon", new Components.Interactions.Equip())
                new BehaviorGoInteract("weapon", 1, new Equip())
            };
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var result = base.Execute(parent, state);;
            return result;
        }
        public override object Clone()
        {
            return new BehaviorFindEquipment();
        }
    }
}
