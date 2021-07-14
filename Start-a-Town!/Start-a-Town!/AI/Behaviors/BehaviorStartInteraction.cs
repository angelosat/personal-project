using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorStartInteraction : Behavior
    {
        TargetArgs Target = TargetArgs.Null;
        Interaction Interaction;
        public BehaviorStartInteraction(TargetArgs targetArgs, Interaction interaction)
        {
            this.Target = targetArgs;
            this.Interaction = interaction;
        }
        public BehaviorStartInteraction(Interaction interaction)
            : this(TargetArgs.Null, interaction)
        {
            
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


            TargetArgs target = this.Target;
            Interaction goal = this.Interaction;

            switch (goal.State)
            {
                case Interaction.States.Unstarted:
                    (net as Server).AIInteract(parent, goal, target);
                    return BehaviorState.Running;

                case Interaction.States.Running:
                    return BehaviorState.Running;

                case Interaction.States.Finished:
                    this.Interaction = goal.Clone() as Interaction;
                    return BehaviorState.Success;

                case Interaction.States.Failed:
                    return BehaviorState.Fail;

                default:
                    break;
            }
            return BehaviorState.Running;
        }
        internal override void ObjectLoaded(GameObject parent)
        {
            // TODO: don't replace fresh stored interaction with null, if parent isn't currently interacting? very hacky
            this.Interaction = parent.GetComponent<WorkComponent>().Task ?? this.Interaction; 
        }
        public override object Clone()
        {
            throw new Exception();
        }
    }
}
