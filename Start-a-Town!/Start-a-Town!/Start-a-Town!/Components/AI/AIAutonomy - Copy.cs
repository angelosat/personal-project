using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Components.Jobs;

namespace Start_a_Town_.Components.AI
{
    public enum PlanType { FindInventory, FindNearest }

    enum ConditionType { HaveItem }
    class ConditionParameters
    {
        public object[] Parameters;
        public ConditionParameters(params object[] p)
        {
            Parameters = p;
        }
    }

    class AIAutonomy : Behavior
    {
        static public event EventHandler<ParameterEventArgs> GoalsUpdated;

        Interaction Job { get { return (Interaction)this["Job"]; } set { this["Job"] = value; } }
        float Timer { get { return (float)this["NeedsTimer"]; } set { this["NeedsTimer"] = value; } }
        Func<GameObject, string> FindGoal { get { return (Func<GameObject, string>)this["FindGoal"]; } set { this["FindGoal"] = value; } }
        //SortedList<DateTime, JobEntry> AcceptedJobs { get { return (SortedList<DateTime, JobEntry>)this["Accepted Jobs"]; } set { this["Accepted Jobs"] = value; } }
        JobEntryCollection AcceptedJobs { get { return (JobEntryCollection)this["Accepted Jobs"]; } set { this["Accepted Jobs"] = value; } }
        Queue<Queue<Interaction>> CurrentPlans { get { return (Queue<Queue<Interaction>>)this["CurrentPlans"]; } set { this["CurrentPlans"] = value; } }
        InteractionCollection Goals { get { return (InteractionCollection)this["Goals"]; } set { this["Goals"] = value; } }
        float PlanTimer { get { return (float)this["PlanTimer"]; } set { this["PlanTimer"] = value; } }

        static void OnGoalsUpdated(GameObject agent, params object[] p)
        {
            if (GoalsUpdated != null)
                GoalsUpdated(agent, new ParameterEventArgs(p)); 
        }

        public override object Clone()
        {
            return new AIAutonomy();
        }

        public override string Name
        {
            get
            {
               // return "Planning " + (PlanInteractions.Count > 0 ? PlanInteractions.Peek().Name:"");//PlanInteractions.Peek().Source.Name + " " + PlanInteractions.Peek().Source.Global.ToString() : "");
                string text = "Plan:\n";
               // foreach (Interaction i in PlanInteractions)
                foreach (Queue<Interaction> goal in CurrentPlans)
                    foreach (Interaction i in goal)
                        text += i + "\n";
                return text.TrimEnd('\n');
            }
        }

        public AIAutonomy()
        {
            //this.Condition = agent => agent["Needs"].GetProperty<NeedCollection>("Needs")["Work"].Value < 50;
            this.FindGoal = agent => "Work";
            this.Timer = Engine.TargetFps;
            this.PlanTimer = Engine.TargetFps;
          //  this.Knowledge = knowledge;
        //    Plan = new Stack<Behavior>();
            this.AcceptedJobs = new JobEntryCollection();// new SortedList<DateTime, JobEntry>();
            //PlanInteractions = new Stack<Interaction>();
            this.CurrentPlans = new Queue<Queue<Interaction>>();
            this.Goals = new InteractionCollection();
            //Behaviors = new Dictionary<string, Behavior>() { 
            //{ "Work", new AISatisfyWork() },
            //{ "Item", new AISatisfyItem() },
            //{ "Reach", new AIMove() },
            //{ "Interaction", new AIPerformInteraction() },
            //};
           // Behaviors = new List<Behavior>() { new AISatisfyWork(), new AISatisfyItem(), new AIMove(), new AIPerformInteraction() };
            //PlanTypes = new Dictionary<PlanType, Func<GameObject, Knowledge, InteractionCondition, bool>>(){
            //    {PlanType.FindNearest, FindPlan},
            //    {PlanType.FindInventory, FindInventory},
            //};
        }


        public override BehaviorState Execute(Net.IObjectProvider net, GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {

            ////if (Timer > 0)
            ////    return BehaviorState.Running;
            Timer -= 1;//GlobalVars.DeltaTime;
            if (Timer <= 0)
            {
                Timer = Engine.TargetFps;

                //UpdateNearbyObjects(parent, knowledge);

                //if (PlanInteractions.Count == 0)
                if (CurrentPlans.Count > 0)
                    return BehaviorState.Success;

                if (!EvaluateJobs(parent, knowledge))
                {
                    Queue<Interaction> newGoal = new Queue<Interaction>();
                    NeedsComponent needs;
                    if (!parent.TryGetComponent<NeedsComponent>("Needs", out needs))
                        return BehaviorState.Success;

                    var sortedNeeds = needs.NeedsHierarchy.GetNeeds(foo => foo.Value > 50);
                    foreach (Need need in sortedNeeds)//needs.Needs.Values.OrderBy(n => n.Value))
                    // if (need.Value < 50) //100)
                    {
                        InteractionCondition searchCond = new InteractionCondition((actor, target) => true, "I need " + need.Name.ToLower() + ".",
                            new Precondition(need.Name, i => true, PlanType.FindNearest),
                            new Precondition(
                                need.Name,
                                i => ProductionComponent.CanProduce(i.Source, product => Interaction.HasInteraction(product, parent, inter => inter.NeedEffects.Any(n => n.Name == need.Name))),// inter.Effect.Key == need.Name)),
                                PlanType.FindNearest)
                                );

                        if (!SearchInventory(parent, knowledge, searchCond, newGoal))// new InteractionCondition(need.Name, true, PlanType.FindNearest)))
                            if (!SearchMemory(parent, knowledge, searchCond, newGoal,
                                filter: mem => mem.Needs.Contains(need.Name),// || mem.State == Memory.States.Invalid,// 
                                //filter: mem => mem.State == Memory.States.Invalid,
                                action: mem => mem.Validate(parent)// mem.State = Memory.States.Valid))
                                //filter: mem => mem.Needs.Contains(need.Name),
                                //   onFail: mem => mem.Needs.Remove(need.Name)
                                ))
                                continue;

                        CurrentPlans.Enqueue(newGoal);
                        OnGoalsUpdated(parent);
                        break;

                    }

                }
                return BehaviorState.Running;
            }


            //if (PlanInteractions.Count == 0)
            if (CurrentPlans.Count == 0)
                return BehaviorState.Fail;

            Queue<Interaction> goalCurrent = CurrentPlans.Peek();
            //Interaction current = PlanInteractions.Peek();
            Interaction current = goalCurrent.Peek();

           // if (!current.CheckCondition(parent))
            InteractionConditionCollection failed = new InteractionConditionCollection();
            if (!current.TryConditions(parent, current.Source, failed))
            {
                throw new NotImplementedException();
                //GameObject.PostMessage(parent, Message.Types.InteractionFailed, parent, current, failed);
                OnGoalsUpdated(parent);
                CurrentPlans.Dequeue();
                return BehaviorState.Running;
            }
            //Vector3 difference = (current.Source.Global - parent.Global); //
            //float length = difference.Length();

            //if(!current.Range(length))
            if (!current.Range(current.Source, parent))
            {
                Vector3 difference = (current.Source.Global - parent.Global); //
                float length = difference.Length();
                MultiTile2Component.GetClosest(current.Source, parent, ref difference, ref length);

                //if (length > current.Range)
                //{
                    difference.Normalize();
                    difference.Z = 0;
                    throw new NotImplementedException();
                    //GameObject.PostMessage(parent, Message.Types.Move, parent, difference, 1f);
                    return BehaviorState.Running;
                }
                else
                {
                throw new NotImplementedException();
                    //GameObject.PostMessage(parent, Message.Types.StopWalking);

                }
            //}

            if (parent["Control"]["Task"] == null)
                throw new NotImplementedException();
                //parent.PostMessage(Message.Types.BeginInteraction, null, current);

            else
            //parent.HandleMessage(Message.Types.Perform);
            parent.PostMessage(Message.Types.Perform);

           // // if the interaction is still running, return running
           // if (parent["Control"]["Task"] != null)
           //     return BehaviorState.Running;

           // // the interaction has finished, remove it from the queue
           // goalCurrent.Dequeue();
           // // furthermore, if the goal is complete, remove it from the goals stack
           // if (goalCurrent.Count == 0)
           //     CurrentPlans.Dequeue();
           // OnGoalsUpdated(parent);
           // // check if the interaction was part of an accepted job and update accordingly
           //// foreach (GameObject job in AcceptedJobs)
           // foreach(var job in AcceptedJobs)
           // {
           //    // job.HandleMessage(Message.Types.InteractionFinished, parent, current);
           //     GameObject.PostMessage(job.JobObject, Message.Types.InteractionFinished, parent, current);
           // }
           // //    (job["Job"]["Tasks"] as List<Interaction>).Remove(current);
           //     //if((job["Job"]["Tasks"] as List<Interaction> con

           // if (this.Jobs.Remove(current))
           // {
           //     // TODO: do something in case the interaction was part of a job
           //   //  AcceptedJobs = AcceptedJobs.Where(foo => foo.Value.Goal != current);
           //     //AcceptedJobs = new SortedList<DateTime, JobEntry>(AcceptedJobs.Where(foo => foo.Value.Goal != current).ToDictionary(foo => foo.Key, foo => foo.Value));


           //   //  AcceptedJobs = AcceptedJobs.Where(foo => foo.Goal != current).ToList() as JobEntryCollection;
           //     AcceptedJobs.Remove(AcceptedJobs.Find(foo => foo.Goal == current));
           //     // GameObject.PostMessage(parent, Message.Types.Speak, parent, current.Name + " complete!");
           // }

            return BehaviorState.Running;
            //   }
            //    return BehaviorState.Fail;
        }

       

        bool EvaluateJobs(GameObject parent, Knowledge  memory)
        {
            foreach (var job in this.Goals.ToList())
            {
                //if (!job.Source.Exists)
                //{
                //    GameObject.PostMessage(parent, Message.Types.Speak, parent, job.Source + " doesn't exist!");
                //    parent.PostMessage(Message.Types.Think, parent, "Object missing", "Looks like " + job.Source + " which was part of " + job.Name + " doesn't exist anymore.");
                //    this.Jobs.Remove(job);
                //    continue;
                //}
                Queue<Interaction> solution = new Queue<Interaction>();
                if (!FindPlan(parent, memory, job, solution))
                {
                    JobEntry acceptedJob = AcceptedJobs.Find(foo => foo.Goal == job);
                    if (acceptedJob.Update() == Components.Job.States.Failed)
                    {
                        throw new NotImplementedException();
                        //parent.PostMessage(Message.Types.Think, parent, "Job abandoned", job.Name + " seems impossible to complete so I'm abandoning it.");
                        this.Goals.Remove(job);
                        AcceptedJobs.Remove(acceptedJob);
                    }
                    continue;
                }
                solution.Enqueue(job);
                CurrentPlans.Enqueue(solution);
                throw new NotImplementedException();
                //parent.PostMessage(Message.Types.Think, parent, "Started a job", "I have a plan to complete: " + job.Name + "! I'm getting right on it!");
                return true;
            }
            return false;
        }

        //bool TryGetGoal(GameObject agent,out string goal)
        //{
        //    goal = "";
        //    NeedsComponent needs;
        //    if (!agent.TryGetComponent<NeedsComponent>("Needs", out needs))
        //        return false;
        //    Need lowest = needs.Needs.Values.First();
        //    foreach (Need need in needs.Needs.Values)
        //        if (need.Value < lowest.Value)
        //            lowest = need;
        //    goal = lowest.Name;
        //    return true;
        //}

        //string GetGoal(GameObject agent)
        //{
        //    NeedsComponent needs;
        //    if (!agent.TryGetComponent<NeedsComponent>("Needs", out needs))
        //        return ""; // TODO
        //    Need lowest = needs.Needs.Values.First();
        //    foreach (Need need in needs.Needs.Values)
        //        if (need.Value < lowest.Value)
        //            lowest = need;
        //    return lowest.Name;
        //}

        static bool FindPlan(GameObject agent, Knowledge knowledge, Interaction goal, Queue<Interaction> plan)
        {
            List<InteractionCondition> badConds = new List<InteractionCondition>();
            //if (!goal.ActorConditions.TryGetBadConditions(agent, badConds))
            //{
            //    plan.Enqueue(goal);
            //    return true;
            //}
            if (goal.TryConditions(agent, badConds))
            {
                plan.Enqueue(goal);
                return true;
            }
            else
            {

                InteractionCondition cond = badConds.First();
                    if (!SearchInventory(agent, knowledge, cond, plan))
                        if (!SearchMemory(agent, knowledge, cond, plan))
                        {
                            throw new NotImplementedException();
                            //GameObject.PostMessage(agent, Message.Types.Speak, agent, cond.ErrorMessage);
                            return false;
                        }

            }

            //foreach (InteractionCondition cond in goal.Conditions)
            //{
            //    FindPlan(agent, knowledge, cond);
            //}

           // plan.Enqueue(goal);
            return true;
        }

        static bool SearchMemory(GameObject agent, Knowledge knowledge, InteractionCondition cond, Queue<Interaction> solution, Func<AIMemory, bool> filter = null, Action<AIMemory> action = null, Action<AIMemory> onFail = null)
        {
            // check each precondition for a possible solution
            Func<AIMemory, bool> memFilter = filter ?? (foo=>true);
            Action<AIMemory> a = action ?? (m => { });
            Action<AIMemory> _onFail = onFail ?? (m => { });
            foreach (KeyValuePair<string, Precondition> pre in cond.PreConditions)
            {
                foreach (AIMemory mem in knowledge.Objects.Values.ToList().FindAll(obj => obj.Object.Exists && memFilter(obj)).OrderBy(obj => Vector3.DistanceSquared(obj.Object.Global, agent.Global)))
                {
                    // only check objects that their state has changed since last evaluation
                    //if (mem.State == Memory.MemoryState.Valid)
                    //    continue;

                   // mem.State = Memory.States.Valid;
                    a(mem);

                    List<Interaction> interactions = new List<Interaction>();

                    mem.Object.Query(agent, interactions);

                //    List<Interaction> haveEffect = interactions.FindAll(i => i.Effect != null);

                    //List<Interaction> matchKey = interactions.FindAll(i => i.Effect != null).FindAll(i => i.Effect.Key == pre.Key);
                    List<Interaction> matchKey = interactions.FindAll(i => i.NeedEffects.Any(effect => effect.Name == pre.Key));
                    List<Interaction> possible = matchKey.FindAll(i => cond.Evaluate(i));
                    
                    foreach (Interaction inter in possible)
                    {
                        //solution.Enqueue(inter);

                        //if (inter.ActorConditions == null)
                        //{
                        //    solution.Enqueue(inter);
                        //    return true;
                        //}
                        //InteractionCondition badCondition;
                        //if (inter.ActorConditions.TryFinalCondition(agent, out badCondition))
                        if (inter.TryConditions(agent))
                        {
                            solution.Enqueue(inter);
                            return true;
                        }
                        // if tryfinalcondition fails, search for a plan that can satisfy the interaction's conditions
                        if (FindPlan(agent, knowledge, inter, solution))
                        {
                            // if a plan for this interaction has been found, enqueue the interaction in the plan queue
                            solution.Enqueue(inter);
                            return true;
                        }
                    }
                    //mem.Needs.Remove(pre.Key);
                    _onFail(mem);
                }
            }
            return false;
        }

        static bool SearchInventory(GameObject agent, Knowledge knowledge, InteractionCondition cond, Queue<Interaction> solution)
        {
          //  solution = new Queue<Interaction>();
            InventoryComponent inv;
            if (!agent.TryGetComponent<InventoryComponent>("Inventory", out inv))
                return false;
            GameObjectSlot slot;
            Interaction inter;

            // find an interaction that satisfies the condition in the inventory
            if (inv.TryFind(agent, cond, out slot, out inter))
            {
                //solution.Push(inter); // I DiDN"T HAVE THIS BEFORE AND IT WAS WORKING
                //if (inter.ActorConditions == null)
                //{
                //    solution.Enqueue(inter);
                //    return true;
                //}

                //InteractionCondition badCond;
                //if (inter.ActorConditions.TryFinalCondition(agent, out badCond))
                if (inter.TryConditions(agent))
                {
                    solution.Enqueue(inter.SetRange((a1, a2) => true));
                    return true;
                }

                //if (PlanTypes[badCond.PlanType](agent, knowledge, badCond))
                //    return true;
                if (FindPlan(agent, knowledge, inter, solution))
                {
                    solution.Enqueue(inter.SetRange((a1, a2) => true));
                    return true;
                }
            }

            return false;
        }

        //bool FindPlan(GameObject agent, Knowledge knowledge, InteractionCondition cond)// string goal)
        //{
        //    foreach (KeyValuePair<string, Precondition> pre in cond.PreConditions)
        //    {
        //        //foreach (MemoryEntry mem in knowledge.Objects.Values)//.OrderBy(obj=>obj.Object.global)
        //        foreach (MemoryEntry mem in knowledge.Objects.Values.ToList().FindAll(obj => obj.Object.Exists).OrderBy(obj => Vector3.DistanceSquared(obj.Object.Global, agent.Global)))
        //        {
        //            //if (!mem.Object.Exists)
        //            //    continue;
        //            List<Interaction> interactions = new List<Interaction>();

        //            mem.Object.HandleMessage(Message.Types.Query, agent, interactions);

        //            //iterate through all interaction that satisfy the condition
        //            //  foreach (Interaction inter in interactions.FindAll(i => i.Effect != null).FindAll(i => i.Effect.Key == cond.Key).FindAll(i => cond.Evaluate(i)))

        //            List<Interaction> haveEffect = interactions.FindAll(i => i.Effect != null);
        //            // List<Interaction> matchKey = interactions.FindAll(i => i.Effect != null).FindAll(i => i.Effect.Key == cond.Key);

        //            List<Interaction> matchKey = interactions.FindAll(i => i.Effect != null).FindAll(i => i.Effect.Key == pre.Key);
        //            List<Interaction> valid = matchKey.FindAll(i => cond.Evaluate(i));
        //            foreach (Interaction inter in valid)
        //            {
        //                //    inter.Source = mem.Object;
        //                PlanInteractions.Push(inter);
        //                //  Console.Write(DateTime.Now + " " + inter.Source + "\n");
        //                if (inter.Conditions == null)
        //                    return true;// true; //we found a plan, return true
        //                //if (inter.Conditions.FinalCondition(agent))
        //                //    return true;
        //                InteractionCondition badCondition;
        //                if(inter.Conditions.TryFinalCondition(agent, out badCondition))
        //                    return true;
        //                //if (PlanTypes[inter.Conditions.PlanType](agent, knowledge, inter.Conditions))//.TrySetPrecondition(cond.PreConditions)))//.Condition); (FindPlan(agent, knowledge, inter.Condition))//.Condition);
        //                if (PlanTypes[badCondition.PlanType](agent, knowledge, badCondition))
        //                {
        //                    return true;
        //                }
        //            }
        //        }
        //    }
        //    //we checked all objects and din't find a plan, clear the stack
        //    PlanInteractions.Clear();
        //    return false;

        //    //    Interaction inter = interactions.FindAll(i => i.Effect != null).FindAll(i => cond.Evaluate(i)).FirstOrDefault(); //.FindAll(i => i.Need != null)
        //    //    if (inter == null)
        //    //    {
        //    //        continue;
        //    //    }
        //    //    else
        //    //    {
        //    //        inter.Source = mem.Object;
        //    //        PlanInteractions.Push(inter);
        //    //        if (inter.Condition == null)
        //    //            return true;// true; //we found a plan, return true
        //    //        if (FindPlan(agent, knowledge, inter.Condition))//.Condition);
        //    //            return true;
        //    //    }
        //    //}
        //    ////we checked all objects and din't find a plan, clear the stack
        //    //PlanInteractions.Clear();
        //    //return false;
        //}

        //bool FindInventory(GameObject agent, Knowledge knowledge, InteractionCondition cond)
        //{
        //    InventoryComponent inv;
        //    if (!agent.TryGetComponent<InventoryComponent>("Inventory", out inv))
        //        return false;
        //    GameObjectSlot slot;
        //    Interaction inter;
        //    if (inv.TryFind(agent, cond, out slot, out inter))
        //    {
        //        PlanInteractions.Push(inter); // I DiDN"T HAVE THIS BEFORE AND IT WAS WORKING
        //        if (inter.Conditions == null)
        //            return true;
        //        //if (inter.Conditions.FinalCondition(agent))
        //        //    return true;
        //        //if (PlanTypes[inter.Conditions.PlanType](agent, knowledge, inter.Conditions))
        //        //    return true;
        //        InteractionCondition badCond;
        //        if (inter.Conditions.TryFinalCondition(agent, out badCond))
        //            return true;
        //        if (PlanTypes[badCond.PlanType](agent, knowledge, badCond))
        //            return true;
        //    }
        //    else //if the agent isn't carrying such item, search nearest object for an item that satisfies condition to pickup
        //    {
        //        if (FindPlan(agent, knowledge, cond))//new InteractionCondition("Holding", true, PlanType.FindNearest, cond.Parameters)))
        //        {
                    
        //            return true;
        //        }
        //    }
        //    PlanInteractions.Clear();
        //    return false;
        //}

        bool FindBlueprint(GameObject agent, Knowledge knowledge, InteractionCondition cond)
        {
            List<Interaction> interactions = new List<Interaction>();
            foreach (Blueprint bp in knowledge.Blueprints)
            {
                GameObject.Objects[bp.ProductID].Query(agent, interactions);
                foreach (Interaction i in interactions)
                    if (cond.Evaluate(i))
                        return true;
            }
            return false;
        }

        bool FindEmptyTile(GameObject agent, Knowledge knowledge, InteractionCondition cond)
        {
            return true;
        }

        static public bool IsHauling(GameObject subject, Predicate<GameObject> filter)
        {
            return InventoryComponent.IsHauling(subject, filter);
        }
        static public bool HasNeed(GameObject subject, Predicate<GameObject> filter)
        {
            return InventoryComponent.IsHauling(subject, filter);
        }

        //List<GameObject> UpdateNearbyObjects(GameObject parent, Knowledge knowledge)
        //{
        //    List<GameObject> newObjects = new List<GameObject>();
        //    Map map = parent.Map;
        //    Chunk chunk = Position.GetChunk(map, parent.Global);
        //    List<GameObject> objects = new List<GameObject>();
        //    foreach (Chunk ch in Position.GetChunks(map, chunk.MapCoords))
        //        foreach (GameObject obj in ch.GetObjects())
        //        {
        //            if (obj == parent)
        //                continue;

        //            if ((obj.Global - parent.Global).Length() > 16)
        //                continue;
        //            List<Interaction> interactions = new List<Interaction>();
        //            obj.Query(parent, interactions);
        //            knowledge.Objects[obj] = new Memory(obj, 100, 100, 1, interactions.Select(i => i.Need).ToArray());
        //            newObjects.Add(obj);
        //        }
        //    return newObjects;
        //}

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e)
        {
            switch (e.Type)
            {
                //case Message.Types.GetGoals
                case Message.Types.UpdateJobs:
                    GameObjectSlot objSlot = e.Parameters[0] as GameObjectSlot;
                    if (!objSlot.HasValue)
                        return true;
                    JobComponent jobComp;
                    if (!objSlot.Object.TryGetComponent<JobComponent>("Job", out jobComp))
                        return true;

                    foreach (Interaction goal in jobComp.Tasks)
                    {
                        if (AcceptedJobs.Any(job => job.Goal == goal))
                            continue;
                        
                        this.Goals.Add(goal);
                        JobEntry entry = new JobEntry(goal, parent, objSlot.Object);
                        AcceptedJobs.Add(entry);
                    }
                    throw new NotImplementedException();
                    //objSlot.Object.PostMessage(Message.Types.JobAccepted, parent);
 
                    //OnGoalsUpdated(parent, objSlot.Object);

                    //GameObject.PostMessage(parent, Message.Types.Speak, parent, objSlot.Object.Name + " accepted!");
                    //GameObject.PostMessage(parent, Message.Types.Think, parent, "Jobs updated", "I visited " + e.Sender.Name + " and was updated on the current jobs.");

                    ///// remove the "Work" need from the memory so the AI doesn't check the board again until its state has changed
                    //parent.PostMessage(Message.Types.Remember, parent, e.Sender, new Action<AIMemory>((AIMemory mem) => { mem.Needs.Remove("Work"); }));
                    return true;


                case Message.Types.GetGoals:
                    List<Queue<Queue<Interaction>>> goals = e.Parameters[0] as List<Queue<Queue<Interaction>>>;
                    goals.Add(this.CurrentPlans);
                    return true;

                case Message.Types.JobComplete:
                    throw new NotImplementedException();
                    //GameObject.PostMessage(parent, Message.Types.Speak, parent, e.Sender.Name + " complete!");
                    //GameObject.PostMessage(parent, Message.Types.Think, parent, "Completed a job", "I completed the final goal of " + e.Sender.Name);
                    return true;

                case Message.Types.CollisionCell:
                    throw new NotImplementedException();
                    //GameObject.PostMessage(parent, Message.Types.Jump);
                    return true;

                //case Message.Types.InteractionFinished:
                //    Interaction inter = e.Parameters[0] as Interaction;
                //    if (this.Jobs.Remove(inter))
                //    {
                //        // TODO: do something in case the interaction was part of a job
                //        GameObject.PostMessage(parent, Message.Types.Speak, parent, inter.Name + " complete!");
                //    }
                //    return true;

                case Message.Types.InteractionFailed:
                    string text = "";
                    InteractionConditionCollection failed = (InteractionConditionCollection)e.Parameters[1];
                    failed.ForEach(fail => text += fail.ErrorMessage + "\n");
                    throw new NotImplementedException();
                    //parent.PostMessage(Message.Types.Think, parent, "Interaction failed", text.TrimEnd('\n'));
                    return true;

                case Message.Types.InteractionFinished:
                    Interaction finished = e.Parameters[0] as Interaction;
                    Queue<Interaction> goalCurrent = CurrentPlans.Peek();
                    Interaction current = goalCurrent.Peek();
                    if (current != finished)
                        return true;
                    // the interaction has finished, remove it from the queue
                    goalCurrent.Dequeue();
                    // furthermore, if the goal is complete, remove it from the goals stack
                    if (goalCurrent.Count == 0)
                        CurrentPlans.Dequeue();
                    OnGoalsUpdated(parent);
                    // check if the interaction was part of an accepted job and update accordingly
                    foreach (var job in AcceptedJobs)
                        throw new NotImplementedException();
                        //job.JobObject.PostMessage(Message.Types.InteractionFinished, parent, current);

                    if (this.Goals.Remove(current))
                        // TODO: do something in case the interaction was part of a job
                        AcceptedJobs.Remove(AcceptedJobs.Find(foo => foo.Goal == current));

                    return true;

                default:
                    return false;
            }
        }

        public override void GetDialogueOptions(GameObject parent, GameObject speaker, DialogueOptionCollection options)
        {
            options.AddRange(new DialogueOption[] {
                 "Jobs".ToDialogueOption(HandleConversation)
            });
        }

        public override Conversation.States HandleConversation(GameObject parent, GameObject speaker, string option, out string speech, out DialogueOptionCollection options)
        {
            switch (option)
            {
                case "Jobs":
                    speech = "Here are my current jobs.";
                    Log.Command(Log.EntryTypes.Jobs, parent, AcceptedJobs.ToList());
                    break;
                default:
                    speech = "I don't know about " + option + ".";
                    break;
            }
            speech += "\n\nAnything else?";// I can help you with?";
            options = parent.GetDialogueOptions(speaker);
            return Conversation.States.Running;
        }

        //bool FindEquipment(GameObject agent, Knowledge knowledge, InteractionCondition cond)
        //{
        //    BodyComponent body;
        //    if (!agent.TryGetComponent<BodyComponent>("Inventory", out body))
        //        return false;
        //    GameObjectSlot slot;
        //    Interaction inter;
        //    if (inv.TryFind(agent, cond, out slot, out inter))
        //    {
        //        if (inter.Condition == null)
        //            return true;
        //        if (PlanTypes[inter.Condition.PlanType](agent, knowledge, inter.Condition))
        //            return true;
        //    }
        //    else //if the agent isn't carrying such item, search nearest object for an item that satisfies condition to pickup
        //    {
        //        if (FindPlan(agent, knowledge, cond))//new InteractionCondition("Holding", true, PlanType.FindNearest, cond.Parameters)))
        //            return true;
        //    }
        //    PlanInteractions.Clear();
        //    return false;
        //}


        //bool HasAbility(GameObject agent, Knowledge knowledge, InteractionCondition cond)
        //{
        //    if(cond.Parameters(agent))

        //}

        //static public bool CheckSelf(GameObject agent, Knowledge knowledge, InteractionCondition cond)
        //{
        //    if (cond.Parameters(agent))
        //        return true;
        //    else
        //    {
        //        if(
        //    }
        //}

        //List<GameObject> UpdateKnowledge(GameObject parent)
        //{
        //    List<GameObject> newObjects = new List<GameObject>();
        //    Chunk chunk = Position.GetChunk(parent.Global);
        //    List<GameObject> objects = new List<GameObject>();
        //    foreach (Chunk ch in Position.GetChunks(chunk.MapCoords))

        //        foreach (GameObject obj in ch.GetObjects())
        //        {
        //            if (obj == parent)
        //                continue;

        //            if (Vector3.Distance(obj.Global, parent.Global) > 10)
        //                continue;
        //            List<Interaction> interactions = new List<Interaction>();
        //            obj.HandleMessage(Message.Types.Query, parent, interactions);

        //            Knowledge.Objects[obj] = new MemoryEntry(obj, 100, 100, 1, interactions.Select(i => i.Need).ToArray());

        //            newObjects.Add(obj);
        //        }
        //    return newObjects;
        //}

        //public override object Clone()
        //{
        //    return new AIPlanner(Knowledge);
        //}


    }
}
