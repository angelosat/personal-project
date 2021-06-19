using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using Start_a_Town_.AI;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Net;
using Start_a_Town_.PathFinding;

namespace Start_a_Town_.AI.Behaviors
{
    class AINextJobStep : Behavior
    {
        public override string Name
        {
            get
            {
                return "AINextJobStep";
            }
        }
        public override object Clone()
        {
            return new AINextJobStep();
        }

        PathingSync Pathfinder = new PathingSync();

        public override BehaviorState Execute(Actor parent, AIState state)
        {
            switch (this.Pathfinder.State)
            {
                case PathingSync.States.Working:
                    this.Pathfinder.Work();
                    return BehaviorState.Running;

                case PathingSync.States.Finished:
                    var path = this.Pathfinder.GetPath();
                    if (path == null)
                    {
                        state.StopJob();
                        "no path found".ToConsole();
                        //state.AddInaccessibleTarget(this.Pathfinder.FinishPrecise);
                        //var area = PathingSync.FloodFill(parent.Map, this.Pathfinder.Finish);
                        return BehaviorState.Fail;
                        //throw new Exception("no path found");
                    }
                    state.Path = path;
                    "path found".ToConsole();
                    //foreach (var s in path.Stack)
                    //    s.ToConsole();
                    path.ToConsole();
                    if (path.Stack.Count == 1)
                    {
                        var range = state.GetJobOld().CurrentStep.Interaction.RangeCheckCached;
                        //state.MoveTarget = new AITarget(new TargetArgs(state.GetJob().CurrentStep.Target), range.Min, range.Max);// 0, .1f);
                        state.MoveTarget = new AITarget(state.GetJobOld().CurrentStep.Target.Clone(), range.Min, range.Max);// 0, .1f);
                        path.Stack.Pop();
                    }
                    else
                        state.MoveTarget = new AITarget(new TargetArgs(path.Stack.Pop()), 0, .1f);

                        return BehaviorState.Success;

                default:
                    break;
            }

            AIJob job = state.GetJobOld();// state.Job;
            if (job == null)
                return BehaviorState.Fail;
            if (job.Instructions.Count == 0)
                throw new ArgumentException();
            //AIInstruction step = job.Instructions.Peek();
            AIInstruction step = job.CurrentStep;

            TargetArgs target = step.Target;
            Interaction goal = step.Interaction;
            var range2 = goal.Conditions.GetAllChildren().FirstOrDefault(c => c is RangeCheck) as RangeCheck;
            if (range2 != null)
            {
                // TODO: find path here? or in moveto behavior?
                //state.MoveTarget = new AITarget(target, range.Min, range.Max);

                var a = parent.Global;
                var b = target.Global;
                this.Pathfinder.Begin(parent, a, target);

                //if (target != TargetArgs.Empty)
                //    this.Pathfinder.Begin(parent.Map, a, b);
                //else
                //    this.Pathfinder.State = PathingSync.States.Finished;
                //"pathfinder starter".ToConsole();
                parent.MoveToggle(false);
                return BehaviorState.Running;

                //var a = parent.Global;
                //var b = target.Global;
                //var path = (new Pathing()).GetPath(parent.Map, a, b);
                //if (path == null)
                //{
                //    state.StopJob();
                //    "no path found".ToConsole();
                //    state.AddInaccessibleTarget(b);
                //    return BehaviorState.Fail;

                //    throw new Exception("no path found");
                //}
                //state.Path = path;
                //state.MoveTarget = new AITarget(new TargetArgs(path.Stack.Pop()), range.Min, range.Max);// 0, .1f);
            }
            else
            {
                // if the interaction doesn't have a range condition it means it can happen where the actor stands
                //throw new Exception("Interaction doesn't contain a range condition");
                return BehaviorState.Success;

            }
            return BehaviorState.Success;
        }


        public BehaviorState ExecuteOld(GameObject parent, AIState state)
        {
            AIJob job = state.GetJobOld();// state.Job;
            if (job == null)
                return BehaviorState.Fail;
            if (job.Instructions.Count == 0)
                throw new ArgumentException();
            //AIInstruction step = job.Instructions.Peek();
            AIInstruction step = job.CurrentStep;

            TargetArgs target = step.Target;
            Interaction goal = step.Interaction;
            //var range = goal.Conditions.GetConditions().FirstOrDefault(c => c is RangeCheck) as RangeCheck;
            var range = goal.Conditions.GetAllChildren().FirstOrDefault(c => c is RangeCheck) as RangeCheck;
            if (range != null)
            {
                // TODO: find path here? or in moveto behavior?
                //state.MoveTarget = new AITarget(target, range.Min, range.Max);

                //var a = parent.Global;
                //var b = target.Global;
                //var path = (new Pathing()).GetPath(parent.Map, a, b);
                //if (path == null)
                //{
                //    state.StopJob();
                //    "no path found".ToConsole();
                //    state.AddInaccessibleTarget(b);
                //    return BehaviorState.Fail;
 
                //    throw new Exception("no path found");
                //}
                //state.Path = path;
                //state.MoveTarget = new AITarget(new TargetArgs(path.Stack.Pop()), range.Min, range.Max);// 0, .1f);
            }
            else
                throw new Exception("Interaction doesn't contain a range condition");

            return BehaviorState.Success;

            //var net = parent.Net;
            //switch (goal.State)
            //{
            //    case Interaction.States.Unstarted:
            //        (net as Server).AIHandler.AIInteract(parent, goal, target);
            //        //var range = goal.Conditions.GetConditions().FirstOrDefault(c => c is RangeCheck) as RangeCheck;
            //        //if (range != null)
            //        //    //if (state.MoveTarget== null)
            //        //    state.MoveTarget = new AITarget(target, range.Min, range.Max);
            //        //return BehaviorState.Success;
            //        return BehaviorState.Running;

            //        break;

            //    case Interaction.States.Running:
            //        return BehaviorState.Running;//Success;

            //    case Interaction.States.Finished:
            //        state.MoveTarget = null;
            //        job.NextStep();
            //        if (job.IsFinished)
            //          state.StopJob();   
            //        //job.Instructions.Dequeue();
            //        //if (job.Instructions.Count == 0)
            //        //  state.StopJob();    

            //        return BehaviorState.Success;
            //        if (job.Instructions.Count == 0)
            //            return BehaviorState.Success;
            //        else
            //            return BehaviorState.Running;
            //        break;

            //    case Interaction.States.Failed:
            //        state.StopJob();
            //        return BehaviorState.Success;  
            //        break;

            //    default:
            //        break;
            //}
            ////return BehaviorState.Success;
            //return BehaviorState.Running;
        }

    }
}
