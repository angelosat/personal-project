using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorInteraction : Behavior
    {
        string TargetVariableName, InteractionName;
        TargetArgs Target;
        Interaction Interaction;
        public BehaviorInteraction(string targetVariableName, string interactionName)
        {
            this.InteractionName = interactionName;
            this.TargetVariableName = targetVariableName;
        }
        public BehaviorInteraction(string targetVariableName, Interaction interaction)
        {
            this.Interaction = interaction;
            this.TargetVariableName = targetVariableName;
        }
        public BehaviorInteraction(TargetArgs target, Interaction interaction)
        {
            this.Interaction = interaction;
            this.Target= target;
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (parent.Velocity != Vector3.Zero)
            {
                var acceleration = parent.GetComponent<MobileComponent>().Acceleration;
                if (acceleration != 0)
                    parent.MoveToggle(false);
                return BehaviorState.Running;
            }
            var net = parent.Net;

            // DONT DO THAT HERE BECAUSE SUCCESSFULLY FINISHING THE INTERACTION MEANS ITS CANCELSTATE IS TRUE!!!
            //if(job.IsCancelled(parent))
            //{
            //    state.StopJob();
            //    (net as Server).AIHandler.AIInterrupt(parent);
            //}

            TargetArgs target = this.Target ?? state.Blackboard[this.TargetVariableName] as TargetArgs;
            //Interaction goal = this.Interaction;
            //Interaction goal = this.Interaction ?? state.Blackboard["interaction"] as Interaction;
            Interaction goal = this.Interaction ?? state.Blackboard[this.InteractionName] as Interaction;

            switch (goal.State)
            {
                case Interaction.States.Unstarted:
                    if (!goal.IsValid(parent, target))
                    {
                        // arrived to perform interaction but interaction no longer valid!
                        // TODO: post message to ai log?
                        return BehaviorState.Fail;
                    }
                    (net as Server).AIHandler.AIInteract(parent, goal, target);
                    return BehaviorState.Running;

                case Interaction.States.Running:
                    return BehaviorState.Running;

                case Interaction.States.Finished:
                    this.Interaction = this.Interaction.Clone() as Interaction;
                    return BehaviorState.Success;

                case Interaction.States.Failed:
                    return BehaviorState.Fail;

                default:
                    break;
            }
            return BehaviorState.Running;
        }

        public override object Clone()
        {
            return new BehaviorInteraction(this.TargetVariableName, this.Interaction.Clone() as Interaction);
        }
    }
}
