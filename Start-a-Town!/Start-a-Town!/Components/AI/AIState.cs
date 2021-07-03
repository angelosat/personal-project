using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.AI.Behaviors;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI
{
    public class AIState
    {
        public SortedSet<Threat> Threats = new();
        public PathingSync PathFinder = new();
        public bool Autonomy = true;
        public Queue<AITask> TaskQueue = new();

        private AITask currentTask;
        public AITask CurrentTask { get => currentTask;
            set
            {
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
            return this.Task != null ? "Task: " + this.Task.ToString() : this.TaskString;
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

        static public AIConversationManager ConversationManager = new();
        public AIConversationManager.Conversation CurrentConversation;
        
        public Stack<AITarget> AIPath = new();
        public IItemPreferencesManager ItemPreferences;

        public GameObject Parent; //use this?
        public Queue<AIInstruction> Commands = new();
        public Dictionary<string, object> Properties = new();
        public AITarget MoveTarget;
        public Knowledge Knowledge;
        public AILog History = new();

        public GameObject Target;
        public AIJob Job;
        public List<GameObject> NearbyEntities { get; set; }
        public bool InSync;
        public PathFinding.Path Path;
        public Vector3 Leash;
        public int JobFindTimer;
        // dialogue
        public GameObject Talker;
        public Progress Attention = new();
        public Conversation Conversation;
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
        }
        
        public bool HasJob(JobDef job)
        {
            return this.Jobs.TryGetValue(job, out var j) ? j.Enabled : false;
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
        }
        
        public void Write(BinaryWriter w)
        {
            this.Jobs.Sync(w);
        }
        public void Read(BinaryReader r)
        {
            this.Jobs.Sync(r);
        }
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            tag.Add(new SaveTag(SaveTag.Types.Vector3, "Leash", this.Leash));
            if (this.Job != null)
                tag.Add(new SaveTag(SaveTag.Types.Compound, "Job", this.Job.Save()));

            tag.Add((this.CurrentTask != null).Save("HasTask"));
            if(this.CurrentTask!=null)
            {
                tag.Add(this.CurrentTask.Save("Task"));
            }
            if (this.CurrentTaskBehavior != null)
            {
                var bhavtag = this.CurrentTaskBehavior.Save("Behavior");
                bhavtag.Add(this.CurrentTaskBehavior.GetType().FullName.Save("TypeName"));
                tag.Add(bhavtag);
            }
            this.Path.TrySave(tag, "Path");
            this.Jobs.Save(tag, "Jobs", SaveTag.Types.String, key=>key.Name);
            this.ItemPreferences.Save(tag, "ItemPreferences");
            return tag;
        }
        public void Load(SaveTag tag)
        {
            this.Leash = tag.GetValue<Vector3>("Leash");
            tag.TryGetTagValue<bool>("HasTask", out var hastask);
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
            tag.TryLoad("Path", out this.Path);
            this.Jobs.TrySync(tag, "Jobs", keyTag => Def.TryGetDef<JobDef>((string)keyTag.Value));

            tag.TryGetTag("ItemPreferences", t => this.ItemPreferences.Load(t));
        }
        internal void SetPath(PathFinding.Path path)
        {
            this.Path = path;
            if (path.Stack.Count == 1)
            {
                var range = this.Job.CurrentStep.Interaction.RangeCheckCached;
                this.MoveTarget = new AITarget(this.Job.CurrentStep.Target.Clone(), range.Min, range.Max);

                path.Stack.Pop();
            }
            else
                this.MoveTarget = new AITarget(new TargetArgs(path.Stack.Pop()), 0, .1f);
        }
       
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
                this.CurrentTask.ObjectLoaded(parent);
            }
        }

        public BehaviorPerformTask LastBehavior;

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
