using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorInitializeInteraction : Behavior
    {
        string InteractionKey;
        public BehaviorInitializeInteraction(string interKey)
        {
            this.InteractionKey = interKey;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var inter = state[this.InteractionKey] as Interaction;
            state[this.InteractionKey] = inter.Clone();
            return BehaviorState.Success;
        }
        public override object Clone()
        {
            return new BehaviorInitializeInteraction(this.InteractionKey);
        }
    }
}
