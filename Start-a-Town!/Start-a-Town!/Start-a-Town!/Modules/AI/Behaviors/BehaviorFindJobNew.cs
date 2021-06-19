using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.AI;
using Start_a_Town_.Net;
using Start_a_Town_.PathFinding;

namespace Start_a_Town_.Components.AI
{
    class BehaviorFindJobNew : Behavior
    {
        //enum States { Running, Success, Failed }
        //AIJob PendingJob;
        //int CurrentEvaluatingStep;
        //Queue<AIJob> JobsToEvaluate = new Queue<AIJob>();
        readonly static int JobFindInterval = Engine.TargetFps;
        //AIJob JobToEvaluate;// { get { return this.JobsToEvaluate.Count == 0 ? null : this.JobsToEvaluate.Dequeue(); } }

        public override string Name
        {
            get
            {
                return "BehaviorFindJobNew";
            }
        }
        public override BehaviorState Execute(GameObject parent, AIState state)
        {
            if (state.JobToEvaluate != null)
            {
                return BehaviorState.Success;
            }
            var net = parent.Net;
            if (state.GetJob() != null)
                return BehaviorState.Success;
            var town = parent.Map.GetTown();
            var jobs = DecideWhatToDo(parent, state);
            foreach (var nextJob in jobs)
            {
                if (nextJob.IsAvailable)
                {
                    if (nextJob.IsCancelled(parent))
                    {
                        town.AIJobs.Remove(nextJob);
                        continue;
                    }
                    //nextJob.EvaluatedBy = parent; // dont set this here, instead, set it when starting to evaluate the job? (inside behaviorevaluatejob)
                    state.JobsToEvaluate.Enqueue(nextJob);
                }
            }
            if (state.JobsToEvaluate.Count > 0)
                return BehaviorState.Success;
            return BehaviorState.Fail;
        }

        static List<AIJob> DecideWhatToDo(GameObject parent, AIState state)
        {
            var list = new List<AIJob>();
            if (state.JobFindTimer < JobFindInterval)
            {
                state.JobFindTimer++;
                return list;
            }

            var hunger = NeedsComponent.GetNeed(parent, Needs.Need.Types.Hunger);
            var needs = parent.GetComponent<NeedsComponent>().NeedsHierarchy;

            //var blocked = false;
            var allinteractions = new Dictionary<GameObject, List<Interactions.Interaction>>();
            foreach (var o in parent.Map.GetObjects())
                if (!AIState.IsItemReserved(o))
                    allinteractions.Add(o, o.GetInteractionsList());
                
            foreach (var category in needs.Inner.Values)
            {
                foreach (var need in category.Values.OrderBy(n => n.Value))// <= n.Max / 2f))
                {
                    //if (need.Value < need.Tolerance)
                    //    blocked = true;
                    var potential = need.FindPotentialJobs(parent, allinteractions);
                    if (potential == null)
                        continue;
                    if (potential.Count == 0)
                        continue;
                    list.AddRange(potential);
                }
                //if (blocked)
                //    break;
            }
            return list;
        }

        public override object Clone()
        {
            return new BehaviorFindJobNew();
        }
    }
}
