using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components.AI
{
    class AIDoInteraction : Behavior
    {
        public override string Name
        {
            get
            {
                return "AIDoInteraction";
            }
        }
        public override object Clone()
        {
            return new AIDoInteraction();
        }

        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            var net = parent.Net;
            AIJob job = state.GetJob();// state.Job;

            // DONT DO THAT HERE BECAUSE SUCCESSFULLY FINISHING THE INTERACTION MEANS ITS CANCELSTATE IS TRUE!!!
            //if(job.IsCancelled(parent))
            //{
            //    state.StopJob();
            //    (net as Server).AIHandler.AIInterrupt(parent);
            //}

            //AIInstruction step = job.Instructions.Peek();
            AIInstruction step = job.CurrentStep;
            TargetArgs target = step.Target;
            Interaction goal = step.Interaction;

            //var range = goal.Conditions.GetConditions().FirstOrDefault(c => c is RangeCheck) as RangeCheck;
            //if (range != null)
            //    if (state.MoveTarget== null)
            //    state.MoveTarget = new AITarget(target, range.Min, range.Max);

            switch(goal.State)
            {
                case Interaction.States.Unstarted:
                    if (!goal.IsValid(parent, target))
                    {
                        // arrived to perform interaction but interaction no longer valid!
                        // TODO: post message to ai log?
                        state.StopJob();
                        return BehaviorState.Success;
                    }
                    (net as Server).AIHandler.AIInteract(parent, goal, target);
                    //var range = goal.Conditions.GetConditions().FirstOrDefault(c => c is RangeCheck) as RangeCheck;
                    //if (range != null)
                    //    //if (state.MoveTarget== null)
                    //    state.MoveTarget = new AITarget(target, range.Min, range.Max);
                    //return BehaviorState.Success;


                    return BehaviorState.Running;

                    break;

                case Interaction.States.Running:
                    return BehaviorState.Running;//Success;

                case Interaction.States.Finished:
                    //return BehaviorState.Fail;
                    state.MoveTarget = null;
                    //job.Instructions.Dequeue();
                    job.CurrentStep.Completed = true;
                    job.NextStep();

                    if (job.IsFinished)//.Instructions.Count == 0)
                    {
                        state.StopJob();

                        if (parent.Map.GetTown().JobComplete(job, parent))
                            //parent.Map.GetTown().JobComplete(job, parent);
                            Start_a_Town_.AI.AIManager.JobComplete(parent, job.Source.Description);
                    }
                    return BehaviorState.Success;
                    if (job.Instructions.Count == 0)
                        return BehaviorState.Success;
                    else
                        return BehaviorState.Running;
                    break;

                case Interaction.States.Failed:
                    //state.History.Write("Failed: " + goal.ToString());
                    state.StopJob();
                    return BehaviorState.Success;
                    break;

                default:
                    break;
            }
            //return BehaviorState.Success;
            return BehaviorState.Running;


            //AIPlanStep step = state.Plan.Peek();
            //Script goal = step.Script;
            //TargetArgs target = step.Target;
            //switch (goal.ScriptState)
            //{
            //    case ScriptState.Unstarted:
            //        parent.Control.TryStartScript(goal, new ScriptArgs(net, parent, target));
            //        return BehaviorState.Running;

            //    case ScriptState.Running:
            //        return BehaviorState.Running;

            //    case ScriptState.Finished:
            //        state.Target = null;
            //        //state.Goal = null;
            //        state.Plan.Dequeue();
            //        return BehaviorState.Success;
            //}
            //state.Plan.Clear();
            //return BehaviorState.Fail;
        }
    }
}
