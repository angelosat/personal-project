using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Needs;
using Start_a_Town_.Components.Jobs;

namespace Start_a_Town_.Components.AI
{
    class BehaviorFindGoal : Behavior
    {
        InteractionOld Job { get { return (InteractionOld)this["Job"]; } set { this["Job"] = value; } }
        float Timer { get; set; }
        float Period { get; set; }
        Func<GameObject, string> FindGoal { get { return (Func<GameObject, string>)this["FindGoal"]; } set { this["FindGoal"] = value; } }
        JobEntryCollection AcceptedJobs { get { return (JobEntryCollection)this["Accepted Jobs"]; } set { this["Accepted Jobs"] = value; } }
        Queue<Queue<InteractionOld>> CurrentPlans { get { return (Queue<Queue<InteractionOld>>)this["CurrentPlans"]; } set { this["CurrentPlans"] = value; } }
        InteractionCollection Goals { get { return (InteractionCollection)this["Goals"]; } set { this["Goals"] = value; } }
        float PlanTimer { get { return (float)this["PlanTimer"]; } set { this["PlanTimer"] = value; } }

        public override object Clone()
        {
            return new BehaviorFindGoal();
        }

        public override string Name
        {
            get
            {
                return "AIFindGoal";
                //string text = "Plan:\n";
                //foreach (Queue<Interaction> goal in CurrentPlans)
                //    foreach (Interaction i in goal)
                //        text += i + "\n";
                //return text.TrimEnd('\n');
            }
        }

        public BehaviorFindGoal()
        {
            this.FindGoal = agent => "Work";
            this.Timer = 0;
            this.Period = Engine.TargetFps;
            this.PlanTimer = Engine.TargetFps;
            this.AcceptedJobs = new JobEntryCollection();
            this.CurrentPlans = new Queue<Queue<InteractionOld>>();
            this.Goals = new InteractionCollection();
        }

        public override BehaviorState Execute(GameObject parent, AIState state)//Net.IObjectProvider net, GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        {
            base.Execute(parent, state);//net, parent, personality, knowledge, p);
            if (this.Timer < Period)
            {
                this.Timer++;
                return BehaviorState.Running;
            }
            this.Timer = 0;
          //  Script script;
            if (TrySatisfyNeed(parent.Net, parent, state))
                return BehaviorState.Success;
            return BehaviorState.Fail;
        }

        private bool TrySatisfyNeed(Net.IObjectProvider net, GameObject parent, AIState state)
        {
            //script = null;
            state.Plan = new AIPlan();
            var lowNeeds =
                (from needs in parent.GetComponent<NeedsComponent>().NeedsHierarchy.Values
                 from need in needs.Values
                 where need.Value < need.Tolerance
                 select need).ToList();
            lowNeeds.OrderBy(n => n.Value);
            if (lowNeeds.Count == 0)
                return false;
            
            //var sortedEntities = (
            //    from memory in state.Knowledge.Objects.Values
            //    let obj = memory.Object
            //    where obj.Exists
            //    // filter by range in relation to personality (for example, a function of determination and need score?)
            //    orderby Vector3.Distance(parent.Global, obj.Global)
            //    select obj).ToList();

            foreach (var need in lowNeeds)
                foreach (var obj in state.NearbyEntities)// sortedEntities)
                {
                    List<AIAction> actions = new List<AIAction>();
                    obj.AIQuery(parent, actions);
                    foreach (var action in
                        from a in actions
                        where a.Need.NeedID == need.ID
                        select a)
                    {
                        if (parent.Control.RunningScripts.ContainsKey(action.Script))
                            return false;
                        Script script = Ability.GetScript(action.Script);
                        var abar = new ScriptArgs(net, parent, new TargetArgs(obj));
                        if (BuildPlan(parent, obj, script, state.NearbyEntities, state.Plan))
                            return true;


                        //List<InteractionCondition> failed = new List<InteractionCondition>();
                        //if (!script.Requirements.Pass(parent, obj, failed))
                        //    return false;
                        //state.Goal = script;
                        //state.Target = obj;
                        //return true;
                    }
                    //{
                    //    Script script;
                    //    if (state.Parent.GetComponent<ControlComponent>().TryStartScript(action.Script, new AbilityArgs(state.Net, state.Parent, new TargetArgs(obj)), out script))
                    //    {
                    //        state.Goal = script;
                    //        state.Target = obj;
                    //        return true;
                    //    }
                    //}
                }
            state.Plan.Clear();
            return false;


            //foreach (
            //    var obj in
            //    from memory in knowledge.Objects.Values
            //    let obj = memory.Object
            //    where obj.Exists
            //    // filter by range in relation to personality
            //    orderby Vector3.Distance(parent.Global, obj.Global)
            //    select obj
            //    )
            //{
            //    obj.AIQuery(parent, actions);
            //    foreach (var need in lowNeeds)
            //    {
            //        foreach (var action in actions)
            //            if (action.Need.NeedID == need.ID)
            //                if (parent.GetComponent<ControlComponent>().TryStartScript(action.Script, new AbilityArgs(net, parent, new TargetArgs(obj))))
            //                    return;
            //    }
            //}
        }

        static bool BuildPlan(GameObject parent, GameObject target, Script script, List<GameObject> sortedEntities, AIPlan plan)
        {
            List<Condition> failed = new List<Condition>();
            if (script.Conditions.Pass(parent, target, failed))
            {
                plan.Enqueue(new AIPlanStep(new TargetArgs(target), script));
                return true;
            }
       //   if (!script.Requirements.Pass(parent, target, failed))
                foreach (var fail in failed)
                    foreach (var pre in fail.Preconditions)
                        foreach (var obj in
                            from obj in sortedEntities
                            where pre.TargetSelector(obj)
                            select obj)
                        {
                            // check if the object offers the script or start it straight away?
                            Script nextscript = Ability.GetScript(pre.Solution);
                            if (BuildPlan(parent, obj, nextscript, sortedEntities, plan))
                            {
                                plan.Enqueue(new AIPlanStep(new TargetArgs(target), script));
                                return true;
                            }
                        }
            return false;
        }


        //static bool BuildPlan(GameObject parent, GameObject target, Script script, List<GameObject> sortedEntities, Stack<AIPlanStep> plan)
        //{
        //    List<InteractionCondition> failed = new List<InteractionCondition>();
        //    if (script.Requirements.Pass(parent, target, failed))
        //    {
        //        plan.Push(new AIPlanStep(target, script));
        //        return true;
        //    }
        //    foreach (var fail in failed)
        //        foreach (var pre in fail.Preconditions)
        //            foreach (var obj in 
        //                from obj in sortedEntities
        //                where pre.TargetSelector(obj)
        //                select obj)
        //            {
        //                // check if the object offers the script or start it straight away?
        //                Script nextscript = Ability.GetScript(pre.Solution);
        //                if (BuildPlan(parent, obj, nextscript, sortedEntities, plan))
        //                    return true;

        //                //List<AIAction> actions = new List<AIAction>();
        //                //obj.AIQuery(parent, actions);
        //                //foreach (var action in
        //                //    from a in actions
        //                //    //where a.Script == pre.Solution
        //                //    where pre.ScriptSelector(a.Script)
        //                //    select a)
        //                //{
        //                //    if (parent.Control.RunningScripts.ContainsKey(action.Script))
        //                //        return false;
        //                //    Script nextscript = Ability.GetScript(action.Script);
        //                //    if (BuildPlan(parent, obj, nextscript, sortedEntities, plan))
        //                //        return true;
        //                //}
        //            }
        //    return false;
        //}


        //public override BehaviorState Execute(Net.IObjectProvider net, GameObject parent, Personality personality, Knowledge knowledge, params object[] p)
        //{
        //    Timer -= 1;
        //    if (Timer <= 0)
        //    {
        //        Timer = Engine.TargetFps;

        //        if (CurrentPlans.Count > 0)
        //            return BehaviorState.Success;

        //        if (!EvaluateJobs(parent, knowledge))
        //        {
        //            Queue<Interaction> newGoal = new Queue<Interaction>();
        //            NeedsComponent needs;
        //            if (!parent.TryGetComponent<NeedsComponent>("Needs", out needs))
        //                return BehaviorState.Success;

        //            var sortedNeeds = needs.NeedsHierarchy.GetNeeds(foo => foo.Value > 50);
        //            foreach (Need need in sortedNeeds)
        //            {
        //                InteractionCondition searchCond = new InteractionCondition((actor, target) => true, "I need " + need.Name.ToLower() + ".",
        //                    new Precondition(need.Name, i => true, PlanType.FindNearest),
        //                    new Precondition(
        //                        need.Name,
        //                        i => ProductionComponent.CanProduce(i.Source, product => Interaction.HasInteraction(product, parent, inter => inter.NeedEffects.Any(n => n.Name == need.Name))),                                PlanType.FindNearest)
        //                        );

        //                if (!SearchInventory(parent, knowledge, searchCond, newGoal))
        //                    if (!SearchMemory(parent, knowledge, searchCond, newGoal,
        //                        filter: mem => mem.Needs.Contains(need.Name),
        //                        action: mem => mem.Validate(parent)
        //                        ))
        //                        continue;

        //                CurrentPlans.Enqueue(newGoal);
        //                break;

        //            }

        //        }
        //        return BehaviorState.Running;
        //    }

        //    if (CurrentPlans.Count == 0)
        //        return BehaviorState.Fail;

        //    Queue<Interaction> goalCurrent = CurrentPlans.Peek();
        //    Interaction current = goalCurrent.Peek();

        //    InteractionConditionCollection failed = new InteractionConditionCollection();
        //    if (!current.TryConditions(parent, current.Source, failed))
        //    {
        //        CurrentPlans.Dequeue();
        //        return BehaviorState.Running;
        //    }

        //    if (!current.Range(current.Source, parent))
        //    {
        //        Vector3 difference = (current.Source.Global - parent.Global); //
        //        float length = difference.Length();
        //        MultiTile2Component.GetClosest(current.Source, parent, ref difference, ref length);


        //        difference.Normalize();
        //        difference.Z = 0;
        //        throw new NotImplementedException();
        //        //GameObject.PostMessage(parent, Message.Types.Move, parent, difference, 1f);
        //        return BehaviorState.Running;
        //    }
        //    else
        //    {
        //        throw new NotImplementedException();
        //        //GameObject.PostMessage(parent, Message.Types.StopWalking);
        //    }


        //    if (parent["Control"]["Task"] == null)
        //        throw new NotImplementedException();

        //    else
        //        parent.PostMessage(Message.Types.Perform);


        //    return BehaviorState.Running;
        //    //   }
        //    //    return BehaviorState.Fail;
        //}



        bool EvaluateJobs(GameObject parent, Knowledge memory)
        {
            foreach (var job in this.Goals.ToList())
            {
                Queue<InteractionOld> solution = new Queue<InteractionOld>();
                if (!FindPlan(parent, memory, job, solution))
                {
                    JobEntry acceptedJob = AcceptedJobs.Find(foo => foo.Goal == job);
                    if (acceptedJob.Update() == Components.Job.States.Failed)
                    {
                        throw new NotImplementedException();
                        this.Goals.Remove(job);
                        AcceptedJobs.Remove(acceptedJob);
                    }
                    continue;
                }
                solution.Enqueue(job);
                CurrentPlans.Enqueue(solution);
                throw new NotImplementedException();
                return true;
            }
            return false;
        }


        static bool FindPlan(GameObject agent, Knowledge knowledge, InteractionOld goal, Queue<InteractionOld> plan)
        {
            List<Condition> badConds = new List<Condition>();
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
                Condition cond = badConds.First();
                if (!SearchInventory(agent, knowledge, cond, plan))
                    if (!SearchMemory(agent, knowledge, cond, plan))
                    {
                        throw new NotImplementedException();
                        //GameObject.PostMessage(agent, Message.Types.Speak, agent, cond.ErrorMessage);
                        return false;
                    }
            }

            return true;
        }

        static bool SearchMemory(GameObject agent, Knowledge knowledge, Condition cond, Queue<InteractionOld> solution, Func<Memory, bool> filter = null, Action<Memory> action = null, Action<Memory> onFail = null)
        {
            // check each precondition for a possible solution
            Func<Memory, bool> memFilter = filter ?? (foo => true);
            Action<Memory> a = action ?? (m => { });
            Action<Memory> _onFail = onFail ?? (m => { });
            foreach (KeyValuePair<string, Precondition> pre in cond.PreConditionsOld)
            {
                foreach (Memory mem in knowledge.Objects.Values.ToList().FindAll(obj => obj.Object.Exists && memFilter(obj)).OrderBy(obj => Vector3.DistanceSquared(obj.Object.Global, agent.Global)))
                {
                    // only check objects that their state has changed since last evaluation
                    a(mem);

                    List<InteractionOld> interactions = new List<InteractionOld>();

                    mem.Object.Query(agent, interactions);

                    List<InteractionOld> matchKey = interactions.FindAll(i => i.NeedEffects.Any(effect => effect.Name == pre.Key));
                    List<InteractionOld> possible = matchKey.FindAll(i => cond.Evaluate(i));

                    foreach (InteractionOld inter in possible)
                    {
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

                    _onFail(mem);
                }
            }
            return false;
        }

        static bool SearchInventory(GameObject agent, Knowledge knowledge, Condition cond, Queue<InteractionOld> solution)
        {

            InventoryComponent inv;
            if (!agent.TryGetComponent<InventoryComponent>("Inventory", out inv))
                return false;
            GameObjectSlot slot;
            InteractionOld inter;

            // find an interaction that satisfies the condition in the inventory
            if (inv.TryFind(agent, cond, out slot, out inter))
            {
                if (inter.TryConditions(agent))
                {
                    solution.Enqueue(inter.SetRange((a1, a2) => true));
                    return true;
                }

                if (FindPlan(agent, knowledge, inter, solution))
                {
                    solution.Enqueue(inter.SetRange((a1, a2) => true));
                    return true;
                }
            }

            return false;
        }

        bool FindBlueprint(GameObject agent, Knowledge knowledge, Condition cond)
        {
            List<InteractionOld> interactions = new List<InteractionOld>();
            foreach (Blueprint bp in knowledge.Blueprints)
            {
                GameObject.Objects[bp.ProductID].Query(agent, interactions);
                foreach (InteractionOld i in interactions)
                    if (cond.Evaluate(i))
                        return true;
            }
            return false;
        }

        bool FindEmptyTile(GameObject agent, Knowledge knowledge, Condition cond)
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

                    foreach (InteractionOld goal in jobComp.Tasks)
                    {
                        if (AcceptedJobs.Any(job => job.Goal == goal))
                            continue;

                        this.Goals.Add(goal);
                        JobEntry entry = new JobEntry(goal, parent, objSlot.Object);
                        AcceptedJobs.Add(entry);
                    }
                    throw new NotImplementedException();
                    return true;


                case Message.Types.GetGoals:
                    List<Queue<Queue<InteractionOld>>> goals = e.Parameters[0] as List<Queue<Queue<InteractionOld>>>;
                    goals.Add(this.CurrentPlans);
                    return true;

                case Message.Types.JobComplete:
                    throw new NotImplementedException();
                    //GameObject.PostMessage(parent, Message.Types.Speak, parent, e.Sender.Name + " complete!");
                    //GameObject.PostMessage(parent, Message.Types.Think, parent, "Completed a job", "I completed the final goal of " + e.Sender.Name);
                    return true;

                case Message.Types.BlockCollision:
                    throw new NotImplementedException();
                    //GameObject.PostMessage(parent, Message.Types.Jump);
                    return true;


                case Message.Types.InteractionFailed:
                    string text = "";
                    ConditionCollection failed = (ConditionCollection)e.Parameters[1];
                    failed.ForEach(fail => text += fail.ErrorMessage + "\n");
                    throw new NotImplementedException();
                    //parent.PostMessage(Message.Types.Think, parent, "Interaction failed", text.TrimEnd('\n'));
                    return true;

                case Message.Types.InteractionFinished:
                    InteractionOld finished = e.Parameters[0] as InteractionOld;
                    Queue<InteractionOld> goalCurrent = CurrentPlans.Peek();
                    InteractionOld current = goalCurrent.Peek();
                    if (current != finished)
                        return true;
                    // the interaction has finished, remove it from the queue
                    goalCurrent.Dequeue();
                    // furthermore, if the goal is complete, remove it from the goals stack
                    if (goalCurrent.Count == 0)
                        CurrentPlans.Dequeue();

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
            return Conversation.States.InProgress;
        }


    }
}
