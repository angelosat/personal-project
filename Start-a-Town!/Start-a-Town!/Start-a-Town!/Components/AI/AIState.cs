using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.PathFinding;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components.AI
{
    public class AIPlan : Queue<AIPlanStep> { }
    public class AIReservedItem
    {
        public GameObject Item;
        public GameObject Owner;
        public AIReservedItem(GameObject item, GameObject owner)
        {
            this.Item = item;
            this.Owner = owner;
        }
    }
    public class AIState
    {
        Dictionary<GameObject, int> InaccessibleEntities = new Dictionary<GameObject, int>();
        Dictionary<Vector3, int> InaccessibleLocations = new Dictionary<Vector3, int>();
        Dictionary<TargetArgs, int> InaccessibleTargets = new Dictionary<TargetArgs, int>();

        public Queue<AIJob> JobsToEvaluate = new Queue<AIJob>();
        public AIJob JobToEvaluate;// { get { return this.JobsToEvaluate.Count == 0 ? null : this.JobsToEvaluate.Dequeue(); } }
        public AIJob PendingJob;
        public int CurrentEvaluatingStep;
        public PathingSync Pathfinder = new PathingSync();

        public HashSet<AILabor> Labors = AILabor.All;

        /// <summary>
        // TODO: make the reserved collection a dictionary containing the actor who reserved the item
        // OR a reserved item entry with 2 fields: the resered item and the owner
        /// </summary>
        //static HashSet<GameObject> ReservedItems = new HashSet<GameObject>();
        static Dictionary<GameObject, AIReservedItem> ReservedItems = new Dictionary<GameObject, AIReservedItem>();

        static public bool IsItemReserved(GameObject obj)
        {
            //return ReservedItems.Contains(obj);
            return ReservedItems.ContainsKey(obj);
        }
        static public void ReserveItem(GameObject item, GameObject owner)
        {
            var reservation = new AIReservedItem(item, owner);
            ReservedItems.Add(item, reservation);
        }
        static public void UnreserveItem(GameObject obj)
        {
            ReservedItems.Remove(obj);
        }
        //public object this[string name]
        //{
        //    get { return this.Properties[name]; }
        //    set { this.Properties[name] = value; }
        //}
        public Stack<AITarget> AIPath = new Stack<AITarget>();

        public GameObject Parent; //use this?
        public Queue<AIInstruction> Commands = new Queue<AIInstruction>();
        public Dictionary<string, object> Properties = new Dictionary<string, object>();
        public AITarget MoveTarget;
        public Knowledge Knowledge { get; set; }
        public Personality Personality { get; set; }

        public AILog History = new AILog();

        //public Script Goal { get; set; }
        public GameObject Target { get; set; }
        AIJob Job;
        public AIPlan Plan { get; set; }
        public List<GameObject> NearbyEntities { get; set; }
        public bool InSync;
        public PathFinding.Path Path;
        public Vector3 Leash;
        public int JobFindTimer;
        //internal Dictionary<int, Behavior.BhavProperties> BehavProps = new Dictionary<int, Behavior.BhavProperties>();

        // dialogue
        public GameObject Talker;
        public Progress Attention = new Progress();
        public Conversation Conversation;
        public float AttentionDecayDefault = 1;
        public float AttentionDecay = 1;

        public AIState()
        {
            this.NearbyEntities = new List<GameObject>();
        }
        //public GameObject Parent { get; set; }
        //public AIState(Knowledge knowledge, Personality personality)
        //{
        //    this.Knowledge = knowledge;
        //    this.Personality = personality;
        //}
        //public AIState(Net.IObjectProvider net, GameObject parent, Knowledge knowledge, Personality personality)
        //{
        //    this.Net = net;
        //    this.Parent = parent;
        //    this.Knowledge = knowledge;
        //    this.Personality = personality;
        //}

        internal AIJob GetJob()
        {
            return this.Job;
        }
        internal bool StartJob(AIJob job)
        {
            foreach (var step in job.Instructions)
            {
                // RESERVE ITEMS
                if (step.Target.Type != TargetType.Entity)
                    continue;
                var item = step.Target.Object;
                if (IsItemReserved(item))
                    return false;// throw new Exception(); // TODO: when 2 jobs are being evaluated at the same time, the first one starts and reserves the item without the chance for the other one to cancel
                if (item != null)
                    //ReservedItems.Add(item);
                    ReserveItem(item, this.Parent);
            }
            this.Job = job;
            return true;
        }
        internal void StopJob()
        {
            this.Job.Source.Release();
            var all = this.Job.Instructions;//.Concat(this.Job.FinishedInstructions);
            foreach (var step in all)
            {
                var item = step.Target.Object;
                if (item == null)
                    continue;
                ReservedItems.Remove(item);
            }
            this.Job = null;
        }

        internal void AddInaccessibleTarget(Vector3 b)
        {
            this.InaccessibleLocations.Add(b, 1);
        }

        internal bool IsInaccessible(Vector3 vector3)
        {
            return this.InaccessibleLocations.ContainsKey(vector3);
        }
        static public bool TryGetState(GameObject entity, out AIState state)
        {
            AIComponent ai;
            if (entity.TryGetComponent<AIComponent>(out ai))
                state = ai.State;
            else
                state = null;
            return state != null;
        }
        static public AIState GetState(GameObject entity)
        {
            return entity.GetComponent<AIComponent>().State;
            //var ai = entity.GetComponent<AIComponent>();
            //if (ai != null)
            //    return ai.State;
            //return null;
        }
        public bool HasLabor(AILabor labor)
        {
            return this.Labors.Contains(labor);
        }

        internal void Generate(GameObject npc, RandomThreaded random)
        {
            this.Personality.Generate(npc, random);
        }

        public void Write(BinaryWriter w)
        {
            this.Personality.Write(w);
        }
        public void Read(BinaryReader r)
        {
            this.Personality.Read(r);
        }

        internal void SetPath(PathFinding.Path path)
        {
            this.Path = path;
            if (path.Stack.Count == 1)
            {
                var range = this.Job.CurrentStep.Interaction.RangeCheckCached;//.Conditions.GetAllChildren().FirstOrDefault(c => c is RangeCheck) as RangeCheck;
                //state.MoveTarget = new AITarget(new TargetArgs(path.Stack.Pop()), range.Min, range.Max);// 0, .1f);
                this.MoveTarget = new AITarget(new TargetArgs(this.Job.CurrentStep.Target), range.Min, range.Max);// 0, .1f);
                path.Stack.Pop();
            }
            else
                this.MoveTarget = new AITarget(new TargetArgs(path.Stack.Pop()), 0, .1f);
        }

        //internal void PathTo()
        //{
        //    this.Pathfinder.Begin(this.Parent.Map, this.Parent.Map.Global, goal);
        //    while (this.Pathfinder.State != PathFinding.PathingSync.States.Finished)
        //        this.Pathfinder.Work();
        //    //state.Path = state.Pathfinder.GetPath();
        //    this.SetPath(state.Pathfinder.GetPath());
        //}

        // TODO: MAKE THIS STATIC?
        internal void OnDespawn()
        {
            // TODO: clean this
            ReservedItems = ReservedItems.Where(i => i.Value.Owner == this.Parent).ToDictionary(i => i.Key, i => i.Value);
        }
    }
}
