using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorInteractHeld : Behavior
    {
        readonly Interaction Interaction;
        public BehaviorInteractHeld(Interaction i)
        {
            this.Interaction = i;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var holding = HaulComponent.GetHolding(parent);
            if (holding.Object == null)
                throw new Exception();
            var consumableComp = holding.Object.GetComponent<ConsumableComponent>();
            if (consumableComp == null)
                throw new Exception();

            return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorInteractHeld(this.Interaction);
        }
    }
}
