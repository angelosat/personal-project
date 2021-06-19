using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;

namespace Start_a_Town_.AI.Behaviors
{
    class AIPerformCommand : Behavior
    {
        public override string Name
        {
            get
            {
                return "AIPerformCommand";
            }
        }
        public override object Clone()
        {
            return new AIPerformCommand();
        }

        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            var command = state.Commands.Peek();

            TargetArgs target = command.Target;
            Interaction goal = command.Interaction;

            if (goal == null)
            {
                state.MoveTarget = null;
                state.Commands.Dequeue();
                return BehaviorState.Success;
            }

            switch (goal.State)
            {
                case Interaction.States.Unstarted:
                    (parent.Net as Server).AIHandler.AIInteract(parent, goal, target);
                    return BehaviorState.Running;

                    break;

                case Interaction.States.Running:
                    return BehaviorState.Running;

                case Interaction.States.Finished:
                    state.MoveTarget = null;
                    state.Commands.Dequeue();
                    return BehaviorState.Success;

                default:
                    break;
            }
            return BehaviorState.Running;
        }
    }
}
