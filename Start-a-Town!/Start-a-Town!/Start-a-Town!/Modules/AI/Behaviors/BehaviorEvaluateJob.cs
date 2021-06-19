using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.PathFinding;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorEvaluateJob : Behavior
    {
        enum States { Running, Success, Failed }

        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            if (state.JobsToEvaluate.Count > 0 && state.JobToEvaluate == null)
            {
                state.JobToEvaluate = state.JobsToEvaluate.Dequeue();
                state.PendingJob = new AIJob();
                state.PendingJob.Source = state.JobToEvaluate;
            }
            if (state.JobToEvaluate != null)
            {
                switch (EvaluateJob(parent, state, state.JobToEvaluate, ref state.CurrentEvaluatingStep, state.PendingJob))
                {
                    case States.Success:
                        state.CurrentEvaluatingStep = 0;

                        state.JobToEvaluate.Entity = parent;
                        state.JobToEvaluate.EvaluatedBy = null;
                        //this.PendingJob.Source = this.JobToEvaluate;
                        if(!state.StartJob(state.PendingJob))
                            return BehaviorState.Fail;//.Running;

                        state.PendingJob = null;
                        state.JobToEvaluate = null;
                        state.JobsToEvaluate.Clear();
                        return BehaviorState.Success;
                        break;

                    case States.Failed:
                        state.CurrentEvaluatingStep = 0;

                        //this.JobToEvaluate.Entity = parent;
                        //this.PendingJob.Source = this.JobToEvaluate;
                        //state.StartJob(this.PendingJob);
                        state.JobToEvaluate.EvaluatedBy = null;

                        state.PendingJob = null;
                        state.JobToEvaluate = null;
                        return BehaviorState.Fail;//.Running;

                        break;

                    case States.Running:
                        return BehaviorState.Fail;//.Running;
                        break;
                }
            }
            return BehaviorState.Success;
        }

        private States EvaluateJob(GameObject parent, AIState state, AIJob jobToEvaluate, ref int currentStep, AIJob newjob)
        {
            jobToEvaluate.EvaluatedBy = parent;
            if (newjob.State == AIJob.States.CalculatingPaths)
            {
                //if (currentStep == newjob.Instructions.Count)
                //{
                //    newjob.State = AIJob.States.Ready;
                //    return States.Success;
                //}
                switch (state.Pathfinder.State)
                {
                    case PathingSync.States.Stopped:
                        currentStep = 0;
                        var prevGlobal = parent.Global;
                        state.Pathfinder.Begin(parent.Map, prevGlobal, newjob.Instructions[currentStep].Target.Global);
                        currentStep++;
                        return States.Running;

                    case PathingSync.States.Working:
                        state.Pathfinder.Work();
                        return States.Running;

                    case PathingSync.States.Finished:
                        var path = state.Pathfinder.GetPath();

                        if (path == null)
                        {
                            "no path found".ToConsole();
                            newjob.State = AIJob.States.Inaccessible;
                            newjob.Source.State = AIJob.States.Inaccessible;
                            return States.Failed;
                        }
                        if (currentStep == newjob.Instructions.Count)
                        {
                            // evaluation sucesful, set state to ready and return success so the job can be assigned to the aistate
                            newjob.State = AIJob.States.Ready;
                            return States.Success;
                        }
                        prevGlobal = newjob.Instructions[currentStep - 1].Target.Global;
                        var nextGlobal = newjob.Instructions[currentStep].Target.Global;
                        state.Pathfinder.Begin(parent.Map, prevGlobal, nextGlobal);
                        currentStep++;
                        return States.Running;

                    default:
                        break;
                }
                //var prevGlobal = currentStep == 0 ? parent.Global : newjob.Instructions[currentStep - 1].Target.Global;
                //currentStep++;

            }
            var stepsOk = true;

            var g = jobToEvaluate.Instructions[currentStep];
            currentStep++;
            List<AIInstruction> steps = new List<AIInstruction>();

            // TODO: clean this up
            var isUnreserved = g.Target.Type == TargetType.Entity ? !AIState.IsItemReserved(g.Target.Object) : true; 

            if (g.FindPlan(parent, g.Target, state, steps) && isUnreserved)
            {
                if (steps.Count > 0)
                    foreach (var s in steps)
                        newjob.AddStep(s);
                newjob.AddStep(new AIInstruction(g.Target, g.Interaction.Clone() as Interaction));
            }
            else
            {
                stepsOk = false;
                //return false;
                return States.Failed;
            }
            if (!stepsOk)
                //return false;
                return States.Failed;

            if (ContainsInaccessibleAreas(parent, newjob))
            //return false;
            {
                newjob.Source.State = AIJob.States.Inaccessible;
                return States.Failed;
            }
            //return currentStep == jobToEvaluate.Instructions.Count ? States.Success : States.Running;
            if (currentStep == jobToEvaluate.Instructions.Count)
            {
                newjob.State = AIJob.States.CalculatingPaths;
                currentStep = 0;
                return States.Running;
                //newjob.AddStep(new AIInstruction(g.Target, g.Interaction.Clone() as Interactions.Interaction));
                //return States.Success;
            }
            return States.Running;
        }

        private static bool ContainsInaccessibleAreas(GameObject parent, AIJob newjob)
        {
            var inaccessible = false;
            foreach (var instr in newjob.Instructions)
            {
                if (instr.Target.Type == TargetType.Position)
                    //if (state.IsInaccessible(instr.Target.Global))
                    if (PathingSync.IsInaccessible(parent.Map, instr.Target.Global))
                    {
                        inaccessible = true;
                        break;
                    }
            }
            return inaccessible;
        }

        public override object Clone()
        {
            return new BehaviorEvaluateJob();
        }
    }
}
