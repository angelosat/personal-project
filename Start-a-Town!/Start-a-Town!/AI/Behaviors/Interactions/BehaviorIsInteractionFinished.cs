using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorIsInteractionFinished : Behavior
    {
        Interaction Interaction;
        string InteractionKey;

        public BehaviorIsInteractionFinished(Interaction inter)
        {
            this.Interaction = inter;
        }
        public BehaviorIsInteractionFinished(string interactionKey)
        {
            this.InteractionKey = interactionKey;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            var interaction = this.Interaction ?? state[this.InteractionKey] as Interaction;
            if (interaction.State == Interaction.States.Finished)
                return BehaviorState.Success;
            return BehaviorState.Fail;
        }
        public override object Clone()
        {
            return new BehaviorIsInteractionFinished(this.Interaction);
        }
    }
}
