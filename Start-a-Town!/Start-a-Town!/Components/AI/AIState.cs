using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.PathFinding;
using Start_a_Town_.Net;
using Start_a_Town_.Components.AI;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI
{
    //public class AIPlan : Queue<AIPlanStep> { }
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
    public class AIItemReservation
    {
        public GameObject Item;
        public Dictionary<GameObject, int> OwnersQuantities = new();
        public AIItemReservation(GameObject item)
        {
            this.Item = item;
        }
        public void Add(GameObject actor, int quantity)
        {
            //this.OwnersQuantities.Add(actor, quantity);
            this.OwnersQuantities.AddOrUpdate(actor, quantity, p => p + quantity);
            var total = this.OwnersQuantities.Values.Sum();
            if (total > this.Item.StackSize)
                throw new Exception();
        }
    }
    public class AIState
    {
        //Dictionary<GameObject, int> InaccessibleEntities = new Dictionary<GameObject, int>();
        Dictionary<Vector3, int> InaccessibleLocations = new();
        //Dictionary<TargetArgs, int> InaccessibleTargets = new Dictionary<TargetArgs, int>();
        Dictionary<GameObject, int> Reserved = new();
        //AITask _Task;
        public SortedSet<Threat> Threats = new();
        public PathingSync PathFinder = new();
        public bool Autonomy = true;
        public Queue<AITask> TaskQueue = new();

        private AITask currentTask;// { get { return this.TaskQueue.Peek(); } }
        public AITask CurrentTask { get => currentTask;
            set
            {
                //if(this.Parent.Net is Server)
                //    Start_a_Town_.Modules.AI.Net.PacketTaskUpdate.Send(this.Parent.Net as Net.Server, this.Parent.InstanceID, value?.Name ?? "none");
                currentTask = value;
            }
        }

        private BehaviorPerformTask currentTaskBehavior;
        public BehaviorPerformTask CurrentTaskBehavior
        {
            get => currentTaskBehavior;
            set {
                if (this.Parent.Net is Server)
                    Start_a_Town_.Modules.AI.Net.PacketTaskUpdate.Send(this.Parent.Net as Net.Server, this.Parent.RefID, value?.GetType().Name ?? "Idle");
                currentTaskBehavior = value;
            }
        }

        [Obsolete]
        public AITask Task; // remove
        public string TaskString = "none";
        public override string ToString()
        {
            return this.Task != null ? "Task: " + this.Task.ToString() : this.TaskString;// "Task: none";
        }
        
        public void SetTask(AITask task)
        {
            this.Task = task;
            task.Reserve(this.Parent);
            Start_a_Town_.Modules.AI.Net.PacketTaskUpdate.Send(this.Parent.Net as Server, this.Parent.RefID, this.Task.Name);
        }
        static public void SetTask(GameObject actor, AITask task)
        {
            AIState.GetState(actor).SetTask(task);
        }
        
        public HashSet<JobDef> Labors = JobDefOf.All;
        readonly Dictionary<JobDef, Job> Jobs = JobDefOf.All.ToDictionary(i => i, i => new Job(i));

        public Dictionary<string, object> Blackboard = new();
        public object this[string key]
        {
            get { return this.Blackboard[key]; }
            set { this.Blackboard[key] = value; }
        }

        static Dictionary<GameObject, AIReservedItem> ReservedItems = new();
        static readonly Dictionary<GameObject, AIItemReservation> ReservedItemsNew = new();

        static public AIConversationManager ConversationManager = new();
        public AIConversationManager.Conversation CurrentConversation;
        static public bool IsItemReservedBy(GameObject obj, GameObject actor)
        {
            if (ReservedItems.TryGetValue(obj, out AIReservedItem reservedBy))
                return actor == reservedBy.Owner;
            return false;
        }
        static public bool IsItemReserved(GameObject obj)
        {
            //return ReservedItems.Contains(obj);
            return ReservedItems.ContainsKey(obj);
        }
        static public void ReserveItem(GameObject item, GameObject owner)
        {
            var reservation = new AIReservedItem(item, owner);
            ReservedItems.Add(item, reservation); // let this throw because if we try to reserve an already reserved item it means something is wrong
            // TODOL also notify clients
            item.Net.EventOccured(Message.Types.ItemReserved, item, owner);
        }
        public void Reserve(GameObject parent, GameObject item, int quantity)
        {
            //this.Reserved.Add(item, quantity);
            this.Reserved.AddOrUpdate(item, quantity, p => p + quantity);
            if (this.Reserved[item] > item.StackMax)
                throw new Exception();

            var existing = ReservedItemsNew.GetValueOrDefault(item);
            if (existing == null)
            {
                existing = new AIItemReservation(item);
                ReservedItemsNew.Add(item, existing);
            }
            existing.Add(parent, quantity);
            //ReservedItemsNew.
        }
        
        public Stack<AITarget> AIPath = new();
        public IItemPreferencesManager ItemPreferences;// = new ItemPreferencesManager();

        public GameObject Parent; //use this?
        public Queue<AIInstruction> Commands = new();
        public Dictionary<string, object> Properties = new();
        public AITarget MoveTarget;
        public Knowledge Knowledge;// { get; set; }
        //public Personality Personality;// { get; set; }

        public AILog History = new();

        //public Script Goal { get; set; }
        public GameObject Target;
        public AIJob Job;
        //public AIPlan Plan { get; set; }
        public List<GameObject> NearbyEntities { get; set; }
        public bool InSync;
        public PathFinding.Path Path;
        public Vector3 Leash;
        public int JobFindTimer;
        //internal Dictionary<int, Behavior.BhavProperties> BehavProps = new Dictionary<int, Behavior.BhavProperties>();
        // dialogue
        public GameObject Talker;
        public Progress Attention = new();
        public Conversation Conversation;
        //public ConversationNew ConversationNew;
        public Actor ConversationPartner, TradingPartner;
        public Dictionary<Actor, ConversationTopic> CommunicationPending = new();
        public float AttentionDecayDefault = 1;
        public float AttentionDecay = 1;

        public AIState(Actor actor)
        {
            this.Parent = actor;
            this.NearbyEntities = new List<GameObject>();
            this.ItemPreferences = new ItemPreferencesManager(actor);
        }
       

        internal AIJob GetJobOld()
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
                if (!AIState.IsItemReservedBy(step.Target.Object, this.Parent))
                    throw new Exception();
                continue;
                //if (IsItemReserved(item))
                //    return false;// throw new Exception(); 
                // TODO: when 2 jobs are being evaluated at the same time, the first one starts and reserves the item without the chance for the other one to cancel
                // reserve items at job creation!!!
               

                AIReservedItem reservedBy;
                if (ReservedItems.TryGetValue(item, out reservedBy))
                    if (reservedBy.Owner != Parent)
                        return false;

                if (item != null)
                    //ReservedItems.Add(item);
                    ReserveItem(item, this.Parent);
            }
            
            this.Job = job;
            //this.History.WriteEntry(this.Parent, "JOB STARTED: " + job.Description);
            this.Parent.Map.Town.JobStarted(this.Parent, job.Source ?? job);
            //AIManager.SyncLogWrite(this.Parent, "JOB STARTED: " + job.Description);
            return true;
        }
        internal void StopJob()
        {
            this.Job.Source.Release();
            this.Job.Dispose(this.Parent); // TODO: call this from fewer places
            var all = this.Job.Instructions;//.Concat(this.Job.FinishedInstructions);
            foreach (var step in all)
            {
                var item = step.Target.Object;
                if (item == null)
                    continue;
                ReservedItems.Remove(item); // TODO: maybe fire and catch an event whenever a job is stopped? and unreserve item when handling the event
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
        
        public bool HasJob(JobDef job)
        {
            return this.Jobs.TryGetValue(job, out var j) ? j.Enabled : false;
            return this.Jobs.ContainsKey(job);
            //return this.Labors.Contains(labor);
        }
        public void ToggleJob(JobDef job)
        {
            this.Jobs[job].Toggle();
        }
        public bool IsJobEnabled(JobDef job)
        {
            return this.Jobs[job].Enabled;
        }
        public Job GetJob(JobDef def)
        {
            return this.Jobs[def];
        }
        public IEnumerable<Job> GetJobs()
        {
            foreach (var j in this.Jobs.Values)
                yield return j;
        }
        internal void Generate(GameObject npc, RandomThreaded random)
        {
            //this.Personality.Randomize(npc, random);
        }
        
        public void Write(BinaryWriter w)
        {
            this.Jobs.Sync(w);
            return;
            //this.Personality.Write(w);
            w.Write(this.Leash);
            w.Write(this.Job != null);
            if (this.Job != null)
                this.Job.Write(w);
            var hasTask = this.CurrentTask != null;
            w.Write(hasTask);
            if (hasTask)
            {
                this.CurrentTask.Write(w);
                this.CurrentTask.WriteBlackboard(w, this.Blackboard);
            }

            var hasBehav = this.CurrentTaskBehavior != null;
            w.Write(hasBehav);
            if (!hasBehav)
                return;
            w.Write(this.CurrentTaskBehavior.GetType().FullName);
            this.CurrentTaskBehavior.Write(w);
            //this.Jobs.Values.Sync(w);

            var haspath = this.Path != null;
            return; // i dont want/need to send path to clients
            w.Write(haspath);
            if(haspath)
            {
                w.Write(this.Path.Stack.ToList());
            }
            this.ItemPreferences.Write(w);
        }
        public void Read(BinaryReader r)
        {
            this.Jobs.Sync(r);
            return;
            //this.Personality.Read(r);
            this.Leash = r.ReadVector3();
            if (r.ReadBoolean())
                this.Job = new AIJob(this.Parent.Net, r);
            var hasTask = r.ReadBoolean();
            if (hasTask)
            {
                this.CurrentTask = AITask.Load(r);
                this.CurrentTask.ReadBlackboard(r, this.Blackboard);
                this.TaskQueue = new Queue<AITask>(new AITask[] { this.CurrentTask });
            }

            var hasBehav = r.ReadBoolean();
            if (!hasBehav)
                return;
            var bhavtype = r.ReadString();
            this.CurrentTaskBehavior = Activator.CreateInstance(Type.GetType(bhavtype)) as BehaviorPerformTask;
            this.CurrentTaskBehavior.Read(r);
            return;// i dont want/need to send path to clients
            var haspath = r.ReadBoolean();
            if(haspath)
            {
                var steps = r.ReadListVector3();
                this.Path = new PathFinding.Path(steps);
            }
            this.ItemPreferences.Read(r);
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(new SaveTag(SaveTag.Types.Vector3, "Leash", this.Leash));
            //tag.Add(this.Personality.Save());
            if (this.Job != null)
                tag.Add(new SaveTag(SaveTag.Types.Compound, "Job", this.Job.Save()));

            //tag.Add(new SaveTag(SaveTag.Types.String, "TaskName", (this.CurrentTask != null) ? this.CurrentTask.GetType().FullName : ""));
            tag.Add((this.CurrentTask != null).Save("HasTask"));
            if(this.CurrentTask!=null)
            {
                tag.Add(this.CurrentTask.Save("Task"));
                //if(this.Blackboard.Any())
                //    tag.Add(this.CurrentTask.SaveBlackboard("Blackboard", this.Blackboard));
            }
            //tag.Add(this.CurrentTask != null ? this.CurrentTask.Save("Task") : new SaveTag(SaveTag.Types.Compound, "Task"));
            //tag.Add(this.CurrentTask != null ? this.CurrentTask.SaveBlackboard("Blackboard", this.Blackboard) : new SaveTag(SaveTag.Types.Compound, "Blackboard"));
            if (this.CurrentTaskBehavior != null)
            {
                var bhavtag = this.CurrentTaskBehavior.Save("Behavior");
                bhavtag.Add(this.CurrentTaskBehavior.GetType().FullName.Save("TypeName"));
                tag.Add(bhavtag);
            }
            //if (this.Path != null)
            //{
            //    tag.Add(this.Path.Stack.ToList().Save("PathOld"));
            //}
            this.Path.TrySave(tag, "Path");
            //this.Jobs.Values.SaveNewBEST(tag, "Jobs");
            this.Jobs.Save(tag, "Jobs", SaveTag.Types.String, key=>key.Name);
            this.ItemPreferences.Save(tag, "ItemPreferences");
            return tag;
        }
        public void Load(SaveTag tag)
        {
            this.Leash = tag.GetValue<Vector3>("Leash");
            bool hastask;
            tag.TryGetTagValue<bool>("HasTask", out hastask);
            if (hastask)
            {
                    var task = AITask.Load(tag["Task"]);
                    this.CurrentTask = task;
            }
            tag.TryGetTag("Behavior", t =>
            {
                var bhavtype = (string)t["TypeName"].Value;
                this.CurrentTaskBehavior = Activator.CreateInstance(Type.GetType(bhavtype)) as BehaviorPerformTask;
                this.CurrentTaskBehavior.Load(t);
            });
            //this.Path.TryLoad(tag, "Path");
            tag.TryLoad("Path", out this.Path);
            //this.Jobs.TryLoad(tag, "Jobs", keyTag => Def.GetDef<JobDef>((string)keyTag.Value));
            this.Jobs.TrySync(tag, "Jobs", keyTag => Def.TryGetDef<JobDef>((string)keyTag.Value));

            //tag.TryGetTag("Path", t => this.Path = new PathFinding.Path().Load(t) as PathFinding.Path);

            //tag.TryGetTag("PathOld", t =>
            //{
            //    var list = new List<Vector3>().Load(t);
            //    list.Reverse();
            //    this.Path = new PathFinding.Path(list);
            //});
            tag.TryGetTag("ItemPreferences", t => this.ItemPreferences.Load(t));
        }
        internal void SetPath(PathFinding.Path path)
        {
            this.Path = path;
            if (path.Stack.Count == 1)
            {
                var range = this.Job.CurrentStep.Interaction.RangeCheckCached;
                //this.MoveTarget = new AITarget(new TargetArgs(this.Job.CurrentStep.Target), range.Min, range.Max);// 0, .1f);
                this.MoveTarget = new AITarget(this.Job.CurrentStep.Target.Clone(), range.Min, range.Max);// 0, .1f);

                path.Stack.Pop();
            }
            else
                this.MoveTarget = new AITarget(new TargetArgs(path.Stack.Pop()), 0, .1f);
        }

      
        // TODO: MAKE THIS STATIC?
        internal void OnDespawn()
        {
            // TODO: clean this
            ReservedItems = ReservedItems.Where(i => i.Value.Owner == this.Parent).ToDictionary(i => i.Key, i => i.Value);
        }

        internal void OnJobCancelled(string jobDesc)
        {
            return;
            this.StopJob();
            this.History.WriteEntry(this.Parent, "JOB CANCELLED: " + jobDesc);
        }



        //internal T1 GetBlackboardValue<T1>(string p) where T1 : new()
        internal T1 GetBlackboardValue<T1>(string p)
        {
            return (T1)this.Blackboard[p];
        }
        internal T1 GetBlackboardValueOrDefault<T1>(string p, T1 defValue)
        {
            if (this.Blackboard.ContainsKey(p))
                return (T1)this.Blackboard[p];
            else return defValue;
        }

        /// <summary>
        /// TODO: very hacky, find better way
        /// </summary>
        /// <param name="parent"></param>
        internal void MapLoaded(Actor parent)
        {
            var targets = from v in this.Blackboard.Values
                          where v is TargetArgs
                          select v as TargetArgs;
            foreach (var t in targets)
                //t.Network = parent.Net;
                t.Map = parent.Map;
            if (this.CurrentTask != null)
                this.CurrentTask.MapLoaded(parent);
            if (this.CurrentTaskBehavior != null)
                this.CurrentTaskBehavior.Actor = parent;
        }

        internal void ObjectLoaded(GameObject parent)
        {
            if (this.CurrentTask != null)
            {
                //this.CurrentTask.Behavior.ObjectLoaded(parent);
                this.CurrentTask.ObjectLoaded(parent);

            }
        }

        public BehaviorPerformTask LastBehavior;
        //public TargetArgs MoveOrder = TargetArgs.Null;

        public Queue<TargetArgs> MoveOrders = new();
        public AITask ForcedTask;

        public TargetArgs MoveOrder
        {
            get
            {
                return this.MoveOrders.Any() ? this.MoveOrders.Peek() : TargetArgs.Null;
            }
        }


        internal void AddMoveOrder(TargetArgs target, bool enqueue)
        {
            if (!enqueue)
                this.MoveOrders.Clear();
            this.MoveOrders.Enqueue(target);
        }

        internal void ForceTask(AITask task)
        {
            this.ForcedTask = task;
        }
    }
}
