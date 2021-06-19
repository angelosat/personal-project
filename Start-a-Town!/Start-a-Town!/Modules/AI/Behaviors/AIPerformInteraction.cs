using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;
using Start_a_Town_.Components.AI;

namespace Start_a_Town_.AI.Behaviors
{
    class AIPerformInteraction : Behavior
    {
        public override string Name
        {
            get
            {
                return "AIPerformInteraction";
            }
        }

        public override BehaviorState Execute(Actor parent, AIState state)
        {
            AIJob job = state.GetJobOld();// state.Job;
            //AIInstruction step = job.Instructions.Peek();
            AIInstruction step = job.CurrentStep;
            TargetArgs target = step.Target;
            Interaction goal = step.Interaction;

            switch(goal.State)
            {
                case Interaction.States.Unstarted:
                    (parent.Net as Server).AIHandler.AIInteract(parent, goal, target);
                    //var range = goal.Conditions.GetConditions().FirstOrDefault(c => c is RangeCheck) as RangeCheck;
                    //if (range != null)
                    //    //if (state.MoveTarget== null)
                    //    state.MoveTarget = new AITarget(target, range.Min, range.Max);
                    //return BehaviorState.Success;
                    return BehaviorState.Running;


                case Interaction.States.Running:
                    return BehaviorState.Running;//Success;

                case Interaction.States.Finished:
                    state.MoveTarget = null;
                    job.NextStep();
                    if (job.IsFinished)
                      state.StopJob();   
                    //job.Instructions.Dequeue();
                    //if (job.Instructions.Count == 0)
                    //  state.StopJob(); 
                   

                    return BehaviorState.Success;
                    

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

        public override object Clone()
        {
            return new AIPerformInteraction();
        }
    }
}
