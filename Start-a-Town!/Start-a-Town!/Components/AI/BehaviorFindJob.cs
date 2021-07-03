using System;
using System.Collections.Generic;

namespace Start_a_Town_.AI.Behaviors
{
    class BehaviorFindJob : Behavior
    {
        //enum States { Deciding, EvaluatingJob }
        //States State;
        enum States { Running, Success, Failed }
        AIJob PendingJob;
        int CurrentEvaluatingStep;
        Queue<AIJob> JobsToEvaluate = new Queue<AIJob>();
        readonly static int JobFindInterval = Engine.TicksPerSecond;
        AIJob JobToEvaluate;// { get { return this.JobsToEvaluate.Count == 0 ? null : this.JobsToEvaluate.Dequeue(); } }

        public override string Name
        {
            get
            {
                return "BehaviorFindJob";
            }
        }
        public override BehaviorState Execute(Actor parent, AIState state)
        {
            if (this.JobsToEvaluate.Count > 0 && this.JobToEvaluate == null)
            {
                this.JobToEvaluate = this.JobsToEvaluate.Dequeue();
                this.PendingJob = new AIJob();
                this.PendingJob.Source = this.JobToEvaluate;
            }
            if (this.JobToEvaluate != null)
            {
                switch (EvaluateJob(parent, state, this.JobToEvaluate, ref this.CurrentEvaluatingStep, this.PendingJob))
                {
                    case States.Success:
                        this.CurrentEvaluatingStep = 0;

                        this.JobToEvaluate.ReservedBy = parent;
                        //this.PendingJob.Source = this.JobToEvaluate;
                        state.StartJob(this.PendingJob);

                        this.PendingJob = null;
                        this.JobToEvaluate = null;
                        this.JobsToEvaluate.Clear();
                        return BehaviorState.Success;

                    case States.Failed:
                        this.CurrentEvaluatingStep = 0;

                        //this.JobToEvaluate.Entity = parent;
                        //this.PendingJob.Source = this.JobToEvaluate;
                        //state.StartJob(this.PendingJob);

                        this.PendingJob = null;
                        this.JobToEvaluate = null;
                        return BehaviorState.Fail;//.Running;

                    case States.Running:
                        return BehaviorState.Fail;//.Running;
                }
            }
            var net = parent.Net;
            //if (state.Job != null)
            if (state.GetJobOld() != null)
                return BehaviorState.Success;
            var town = parent.Map.GetTown();
            //var jobs = town.AIJobs.ToList();
            //List<AIJob> jobs = new List<AIJob>();
            var jobs = DecideWhatToDo(parent, state);
            foreach (var nextJob in jobs)
            {
                //if (nextJob != null)
                //if (!nextJob.Cancelled && nextJob.IsAvailable)
                if (nextJob.IsAvailable)
                {
                    if (nextJob.IsCancelled(parent))
                    {
                        town.AIJobs.Remove(nextJob);
                        continue;
                    }
                    this.JobsToEvaluate.Enqueue(nextJob);

                    //var newjob = new AIJob();
                    //newjob.Source = nextJob;
                    //this.JobToEvaluate = nextJob;
                    //this.PendingJob = newjob;


                    //return BehaviorState.Running;
                    //return BehaviorState.Success;
                }
            }
            return BehaviorState.Fail;
        }

        PathingSync Pathfinder = new PathingSync();
        private States EvaluateJob(GameObject parent, AIState state, AIJob jobToEvaluate, ref int currentStep, AIJob newjob)
        {
            if(newjob.State == AIJob.States.CalculatingPaths)
            {
                //if (currentStep == newjob.Instructions.Count)
                //{
                //    newjob.State = AIJob.States.Ready;
                //    return States.Success;
                //}
                switch (Pathfinder.State)
                {
                    case PathingSync.States.Stopped:
                        currentStep = 0;
                        var prevGlobal = parent.Global;
                        //this.Pathfinder.Begin(parent, prevGlobal, newjob.Instructions[currentStep].Target.Global);
                        this.Pathfinder.Begin(parent as Actor, prevGlobal, newjob.Instructions[currentStep].Target);

                        currentStep++;
                        return States.Running;

                    case PathingSync.States.Working:
                        Pathfinder.Work();
                        return States.Running;

                    case PathingSync.States.Finished:
                        var path = Pathfinder.GetPath();

                        if (path == null)
                        {
                            "no path found".ToConsole();
                            newjob.State = AIJob.States.Inaccessible;
                            newjob.Source.State = AIJob.States.Inaccessible;
                            return States.Failed;
                        }
                        if (currentStep == newjob.Instructions.Count)
                        {
                            
                            newjob.State = AIJob.States.Ready;
                            return States.Success;
                        }
                        prevGlobal = newjob.Instructions[currentStep-1].Target.Global;
                        var nextGlobal = newjob.Instructions[currentStep].Target.Global;
                        this.Pathfinder.Begin(parent as Actor, prevGlobal, nextGlobal);
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
            var steps = new List<AIInstruction>();
            if (g.FindPlan(parent, g.Target, state, steps))
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

        //public BehaviorState ExecuteLaster(GameObject parent, AIState state)
        //{
        //    var net = parent.Net;
        //    //if (state.Job != null)
        //    if (state.GetJob() != null)
        //        return BehaviorState.Success;
        //    var town = parent.Map.GetTown();
        //    //var nextJob = town.AIJobs.FirstOrDefault();
        //    foreach (var nextJob in town.AIJobs.ToList())
        //    {
        //        //if (nextJob != null)
        //        //if (!nextJob.Cancelled && nextJob.IsAvailable)
        //        if (nextJob.IsAvailable)
        //        {
        //            if (nextJob.IsCancelled(parent))
        //            {
        //                town.AIJobs.Remove(nextJob);
        //                continue;
        //            }
        //            var newjob = new AIJob();
        //            // create a new job by injecting any necessary steps between original job's steps
        //            var stepsOk = true;
        //            foreach (var g in nextJob.Instructions)
        //            {
        //                List<AIInstruction> steps = new List<AIInstruction>();
        //                if (g.FindPlan(parent, g.Target, state, steps))
        //                {
        //                    if (steps.Count > 0)
        //                        foreach (var s in steps)
        //                            newjob.AddStep(s);
        //                    newjob.AddStep(new AIInstruction(g.Target, g.Interaction.Clone() as Interactions.Interaction));
        //                }
        //                else
        //                {
        //                    stepsOk = false;
        //                    break;
        //                }
        //            }
        //            if (!stepsOk)
        //                continue;

        //            if (ContainsInaccessibleAreas(parent, newjob))
        //                continue;

        //            // TODO: dont remove active jobs from the town, instead, mark them as unavailable. 
        //            // only remove them if they're complete, otherwise mark them as available again if they're cancelled
        //            //town.AIJobs.Remove(nextJob);
        //            //state.Job = newjob;
        //            nextJob.Entity = parent;
        //            newjob.Source = nextJob;
        //            state.StartJob(newjob);
        //            return BehaviorState.Success;
        //        }
        //    }
        //    return BehaviorState.Fail;
        //}

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

        static List<AIJob> DecideWhatToDo(GameObject parent, AIState state)
        {
            throw new Exception();
            //var list = new List<AIJob>();
            //if (state.JobFindTimer < JobFindInterval)
            //{
            //    state.JobFindTimer++;
            //    return list;
            //}

            //var needs = parent.GetComponent<NeedsComponent>().NeedsHierarchy;

            //// TODO: make its need provide its own way of searching for ways to satifsy itself
            //var items = parent.Map.GetObjects().Where(o => !AIState.IsItemReserved(o));

            //foreach (var category in needs.Inner.Values)
            //{
            //    foreach (var need in category.Values.Where(n => n.Value <= n.Max / 2f))
            //    {
            //        foreach (var item in items)
            //        {
            //            var inters = item.GetInteractions();
            //            var potential = inters.Values.FirstOrDefault(i => i.NeedSatisfaction.ContainsKey(need.ID));
            //            if (potential == null)
            //                continue;
            //            var job = new AIJob();
            //            job.Instructions.Add(new AIInstruction(new TargetArgs(item), potential));
            //            list.Add(job);
            //        }
            //    }
            //}
            //if (list.Count > 0)
            //    return list;
            //list = parent.Map.GetTown().AIJobs.ToList();
            //return list;
        }

        //public BehaviorState ExecuteLast(GameObject parent, AIState state)
        //{
        //    var net = parent.Net;
        //    //if (state.Job != null)
        //    if (state.GetJob() != null)
        //        return BehaviorState.Success;
        //    var town = parent.Map.GetTown();
        //    //var nextJob = town.AIJobs.FirstOrDefault();
        //    foreach (var nextJob in town.AIJobs.ToList())
        //    {
        //        //if (nextJob != null)
        //        //if (!nextJob.Cancelled && nextJob.IsAvailable)
        //        if (nextJob.IsAvailable)
        //        {
        //            if (nextJob.IsCancelled(parent))
        //            {
        //                town.AIJobs.Remove(nextJob);
        //                continue;
        //            }
        //            var newjob = new AIJob();
        //            // create a new job by injecting any necessary steps between original job's steps
        //            var stepsOk = true;
        //            foreach (var g in nextJob.Instructions)
        //            {
        //                List<AIInstruction> steps = new List<AIInstruction>();
        //                if (g.FindPlan(parent, g.Target, state, steps))
        //                {
        //                    if (steps.Count > 0)
        //                    {
        //                        foreach (var s in steps)
        //                            //newjob.Instructions.Enqueue(s);
        //                            newjob.AddStep(s);
        //                    }
        //                    //newjob.Instructions.Enqueue(g);
        //                    //newjob.AddStep(g);
        //                    newjob.AddStep(new AIInstruction(g.Target, g.Interaction.Clone() as Interactions.Interaction));
        //                }
        //                else
        //                {
        //                    stepsOk = false;
        //                    break;
        //                    continue;
        //                    return BehaviorState.Success;
        //                }
        //            }
        //            if (!stepsOk)
        //                continue;

        //            // TODO: dont remove active jobs from the town, instead, mark them as unavailable. 
        //            // only remove them if they're complete, otherwise mark them as available again if they're cancelled
        //            //town.AIJobs.Remove(nextJob);
        //            //state.Job = newjob;
        //            nextJob.Entity = parent;
        //            newjob.Source = nextJob;
        //            state.StartJob(newjob);
        //            return BehaviorState.Success;
        //        }
        //    }
        //    return BehaviorState.Fail;
        //}

        //public BehaviorState ExecuteOld(GameObject parent, AIState state)
        //{
        //    var net = parent.Net;
        //    //if (state.Job != null)
        //    if (state.GetJob() != null)
        //        return BehaviorState.Success;
        //    var town = parent.Map.GetTown();
        //    var nextJob = town.AIJobs.FirstOrDefault();
          
        //    if (nextJob != null)
        //        if (!nextJob.Cancelled && nextJob.IsAvailable)
        //        {
        //            var newjob = new AIJob();
        //            // create a new job by injecting any necessary steps between original job's steps
        //            foreach (var g in nextJob.Instructions)
        //            {
        //                List<AIInstruction> steps = new List<AIInstruction>();
        //                if (g.FindPlan(parent, g.Target, state, steps))
        //                {
        //                    if (steps.Count > 0)
        //                    {
        //                        foreach (var s in steps)
        //                            //newjob.Instructions.Enqueue(s);
        //                            newjob.AddStep(s);
        //                    }
        //                    //newjob.Instructions.Enqueue(g);
        //                    newjob.AddStep(g);
        //                }
        //                else
        //                {
        //                    return BehaviorState.Success;
        //                }
        //            }
        //            // TODO: dont remove active jobs from the town, instead, mark them as unavailable. 
        //            // only remove them if they're complete, otherwise mark them as available again if they're cancelled
        //            //town.AIJobs.Remove(nextJob);
        //            //state.Job = newjob;
        //            nextJob.Entity = parent;
        //            state.StartJob(newjob);
        //            return BehaviorState.Success;

        //            // TODO: inject additional necessary steps between instructions
        //            parent.Map.GetTown().AIJobs.Remove(nextJob);
        //            AIInstruction goal = nextJob.Instructions.First();
        //            AIJob newJob = goal.Interaction.GetJob(parent, goal.Target, state);
        //            //newJob.ToConsole();
        //            //state.Job = newJob;
        //            state.StartJob(newjob);

        //            //net.EventOccured(Message.Types.JobAccepted, parent);
        //            (net as Server).AIHandler.AIJobAccepted(parent, newJob);
        //            if (newJob == null)
        //                throw new InvalidOperationException();
        //            return BehaviorState.Success;
        //        }
        //    return BehaviorState.Fail;

        //    HashSet<TownJob> jobs = new HashSet<TownJob>();
        //    if (!parent.TryGetComponent<WorkerComponent>(c => jobs = c.Jobs))
        //        return BehaviorState.Fail;

        //    foreach (var job in jobs)
        //    {
        //        foreach (var step in job.Steps)
        //        {
        //            AIPlan plan = new AIPlan();
        //            if (Ability.GetScript(step.Script).BuildPlan(parent, step.Target, state.NearbyEntities, plan))
        //            {
        //                state.Plan = plan;
        //                return BehaviorState.Success;
        //            }
        //        }
        //    }
        //    return BehaviorState.Fail;
        //}



        //public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        //{
        //    switch(e.Type)
        //    {
        //        case Message.Types.InteractionFailed:
        //        case Message.Types.InteractionFinished:
        //        case Message.Types.InteractionSuccessful:
        //            // TODO: stop current job and unreserve items here?
        //            AIComponent.GetState(parent).StopJob();
        //            break;

        //        default:
        //            break;
        //    }
        //    return false;
        //}
        public override object Clone()
        {
            return new BehaviorFindJob();
        }
    }
}
