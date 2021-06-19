using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Graphics;
using Start_a_Town_.AI;
using Start_a_Town_.Animations;
using Start_a_Town_.Components;

namespace Start_a_Town_//.Components.Interactions
{
    public abstract class Interaction : ICloneable
    {
        public const float DefaultRange = 2;// (float)Math.Sqrt(3);
        public bool IsFinished => this.State == States.Finished;
        //static Dictionary<string, Func<SaveTag, Interaction>> Factory = new Dictionary<string, Func<SaveTag, Interaction>>();
        ////static public void AddTask(Type type, Func<SaveTag, Interaction> factory)
        ////{
        ////    Factory[type.FullName] = factory;
        ////}
        //static public void AddInteraction<T>(Func<SaveTag, Interaction> factory)
        //{
        //    Factory[typeof(T).FullName] = factory;
        //}
        //static T Load<T>(SaveTag tag) where T : Interaction
        //{
        //    //return Factory[type.FullName](tag) as T;
        //    return Factory[typeof(T).FullName](tag) as T;
        //}
        readonly static Dictionary<string, Func<Interaction>> Factory = new();
        //static public void AddTask(Type type, Func<SaveTag, Interaction> factory)
        //{
        //    Factory[type.FullName] = factory;
        //}
        static public void AddInteraction<T>(Func<Interaction> factory)
        {
            Factory[typeof(T).FullName] = factory;
        }
        static public void AddInteraction<T>() where T : Interaction, new()
        {
            Factory[typeof(T).FullName] = () => new T();
        }
        //static public T Load<T>() where T : Interaction
        //{
        //    //return Factory[type.FullName](tag) as T;
        //    return Factory[typeof(T).FullName]() as T;
        //}

        internal virtual void OnToolContact(GameObject parent, TargetArgs target)
        {
        }

        static public Interaction Create(string typeName)
        {
            var inter = Activator.CreateInstance(Type.GetType(typeName)) as Interaction;
            return inter;
            //return Factory[typeName]();
        }
        public static void Initialize()
        {
            //AddInteraction<PlantComponent.InteractionHarvest>();
            //AddInteraction<Haul>();
            //AddInteraction<UseHauledOnTarget>();
            //AddInteraction<Equip>();
            //AddInteraction<EquipFromInventory>();
            //AddInteraction<DropEquipped>();
            //AddInteraction<InteractionObserve>();
        }
        public override string ToString()
        {
            return "Interaction: " + this.Name;
        }

        public enum States { Unstarted, Running, Finished, Failed }
        public enum RunningTypes { Once, Continuous }

        
        public int GetID()
        {
            return this.Name.GetHashCode();
        }
        public RangeCheck RangeCheckCached
        {
            get
            {
                return this.Conditions.GetAllChildren().FirstOrDefault(c => c is RangeCheck) as RangeCheck;
            }
        }
        public virtual bool InRange(GameObject a, TargetArgs t)
        {
            return this.RangeCheckCached.Condition(a, t);
        }
        
        public virtual ScriptTaskCondition CancelState { get; set; }

        public bool IsCancelled(GameObject a, TargetArgs t)
        {
            //if (t.Type == TargetType.Entity)
            //    if (AIState.IsItemReserved(t.Object))
            //    {
            //        // TODO: make the reserved collection a dictionary containing the actor who reserved the item
            //        //"WARNING! "
            //        //return true;
            //    }
            ////return this.CancelState == null ? false : !this.CancelState.IsMet(a, t);
            return this.CancelState != null && !this.CancelState.Condition(a, t);
        }

        //public States State = States.Unstarted;
        public States State { get; protected set; } = States.Unstarted;

        public RunningTypes RunningType = RunningTypes.Once;
        

        public string Name { get; set; }
        public string Verb { get; set; }
        public Action<GameObject, TargetArgs> Callback { get; set; }
        
        readonly TaskConditions defConditions = new();
        // TODO: use an static readonly empty condition collection to return in the virtual method
        public virtual TaskConditions Conditions { get { return defConditions; } }//set; }
        public float Length { get; set; }
        //public TimeSpan Time { get; set; }
        public float CurrentTick;// { get; set; }
        public ToolAbilityDef Skill { get; set; }
        public float Seconds { get; set; }
        public Animation Animation = new(AnimationDef.Work);// AnimationWork.Create();

        // TODO: i need a method that returns satisfaction score based on ai entity's state
        //public HashSet<Needs.Need.Types> NeedSatisfaction = new HashSet<Needs.Need.Types>();
        static readonly Dictionary<Need.Types, float> _NeedSatisfaction = new();
        public virtual Dictionary<Need.Types, float> NeedSatisfaction //= new Dictionary<Needs.Need.Types, float>();
        {
            get { return _NeedSatisfaction; }
        }

        public Interaction()
        {
            this.Callback = (a, t) => { };
            //this.Conditions = new TaskConditions();
        }
        //public TargetArgs Target { get; set; }
        public Interaction(string name, Action<GameObject, TargetArgs> callback) : this(name, 0, callback, new TaskConditions(), null) { }
        public Interaction(string name, float seconds, Action<GameObject, TargetArgs> callback) : this(name, seconds, callback, new TaskConditions(), null) { }
        public Interaction(string name, float seconds, Action<GameObject, TargetArgs> callback, ToolAbilityDef skill) : this(name, seconds, callback, new TaskConditions(), skill) { }
        public Interaction(string name, float seconds, Action<GameObject, TargetArgs> callback, TaskConditions conditions) : this(name, seconds, callback, conditions, null) { }
        public Interaction(string name, float seconds, Action<GameObject, TargetArgs> callback, TaskConditions conditions, ToolAbilityDef skill)
        {
            this.Name = name;
            this.Callback = callback;
            this.Seconds = seconds;
            //this.Length = seconds * 1000;
            //this.Time = TimeSpan.FromMilliseconds(this.Length);
            this.CurrentTick = this.Length = seconds * Engine.TicksPerSecond;
            //this.Conditions = conditions;
            this.Skill = skill;
        }

        public void SetSeconds(float seconds)
        {
            this.CurrentTick = this.Length = seconds * Engine.TicksPerSecond;
        }

        protected Interaction(string name, float seconds = 0) //: this(name, seconds, new TaskConditions(), null)
            :this()
        {
            this.Name = name;
            this.Seconds = seconds;
            this.CurrentTick = this.Length = seconds * Engine.TicksPerSecond;
        }
        //protected Interaction(string name, float seconds, Skill skill) : this(name, seconds, new TaskConditions(), skill) { }
        //protected Interaction(string name, float seconds, TaskConditions conditions) : this(name, seconds, conditions, null) { }
        //protected Interaction(string name, float seconds, TaskConditions conditions, Skill skill)
        //    : this(name, seconds, (a, t) => { }, conditions, skill)
        //{
        //    //this.Name = name;
        //    //this.Callback = (a, t) => { };
        //    //this.Seconds = seconds;
        //    //this.Length = seconds * 1000;
        //    ////this.Time = TimeSpan.FromMilliseconds(this.Length);
        //    //this.Time = seconds * Engine.TargetFps;
        //    //this.Conditions = conditions;
        //    //this.Skill = skill;
        //}

        public virtual void Interrupt(GameObject parent, bool success)
        {
            if(!success)
                parent.Net.EventOccured(Message.Types.InteractionInterrupted, parent, this);
            this.State = States.Finished;
            //parent.Body.FadeOutAnimationAndRemove(this.Animation);
            this.Animation?.FadeOutAndRemove();

        }

        public virtual void Perform(GameObject a, TargetArgs t)
        {
            this.Callback(a, t);
        }

        public virtual void Start(GameObject a, TargetArgs t)
        {
            //this.Animation = AnimationWork.Create(a);
            if (this.Animation != null)
                a.AddAnimation(this.Animation);
        }

        public virtual void Update(GameObject actor, TargetArgs target)
        {
            if (this.State == States.Finished) // TODO: maybe check for failed state too?
            {
                Stop(actor);
                return;
            }
            var instr = new AIInstruction(target, this);

            if (this.IsCancelled(actor, target))
            {
                Stop(actor);
                this.State = States.Failed;
                actor.Net.Map.EventOccured(Message.Types.InteractionFailed, actor, instr , this.CancelState);
                //AILog.TryWrite(actor, "Failed: " + instr.ToString());
                AILog.TryWrite(actor, "Failed: " + this.GetCompletedText(actor, target) + " (cancelled)");

                return;
            }

            if (!this.Conditions.Evaluate(actor, target))
            {
                ScriptTaskCondition failed = this.Conditions.GetFailedCondition(actor, target);
                //this.State = States.Finished;
                this.State = States.Failed;
                actor.Net.Map.EventOccured(Message.Types.InteractionFailed, actor, instr, failed);
                //AILog.TryWrite(actor, "Failed: " + instr.ToString());
                AILog.TryWrite(actor, "Failed: " + this.GetCompletedText(actor, target) + string.Format("({0})", failed.ToString()));

                Stop(actor);
                return;
            }
            if (this.State == States.Unstarted)
                this.Start(actor, target);
            else if (this.State == States.Finished)
            {
                Stop(actor);
                actor.Net.Map.EventOccured(Message.Types.InteractionSuccessful, actor, instr);
                //AILog.TryWrite(actor, "Success: " + instr.ToString());
                AILog.TryWrite(actor, "Success: " + this.GetCompletedText(actor, target));

                return;
            }

            //if (this.State == States.Unstarted)
            //{
            //    // should i instead start the animation in the work component of the entity, when the interaction is started
            //    if (this.Animation != null)
            //        actor.AddAnimation(this.Animation);
            //}
            this.State = States.Running;
            if(this.RunningType == RunningTypes.Continuous)
            {
                this.Perform(actor, target);// Callback(actor, target);
                if (this.State == States.Finished)
                {
                    Stop(actor);
                    actor.Net.Map.EventOccured(Message.Types.InteractionSuccessful, actor, instr);
                    //AILog.TryWrite(actor, "Success: " + instr.ToString());
                    AILog.TryWrite(actor, "Success: " + this.GetCompletedText(actor, target));

                }
                return;
            }
            this.CurrentTick--;
            //if (this.Time.TotalMilliseconds <= 0)
            if (this.CurrentTick <= 0)
            {
                //actor.Net.ToConsole();
                Finish(actor, target);

                //AILog.TryWrite(actor, "Success: " + this.GetCompletedText(actor, target)); // do that here before calling perform() because the target might get disposed

                Stop(actor);
                this.Perform(actor, target);
                //actor.Net.Map.EventOccured(Message.Types.InteractionSuccessful, actor, instr);
                //AILog.TryWrite(actor, "Success: " + this.GetCompletedText(actor, target));

            }
        }



        protected virtual void Stop(GameObject actor)
        {
            //actor.Body.FadeOutAnimationAndRemove(this.Animation);
            this.Animation.FadeOutAndRemove();
        }

        protected virtual void Contact(GameObject actor, TargetArgs target)
        {
        }

        //public bool Condition(GameObject actor)
        //{
        //    foreach (var item in this.Conditions)
        //        if (!item.Condition(actor))
        //            return false;
        //    return true;
        //}

        //public virtual ScriptTaskCondition AvailabilityCondition(GameObject actor, TargetArgs target)
        //{
        //    return this.Conditions.Evaluate(actor, target);
        //}
        public virtual bool AvailabilityCondition(GameObject actor, TargetArgs target)
        {
            return this.Conditions.GetFailedCondition(actor, target) == null;
            //return this.Conditions.Evaluate(actor, target);

            //return true;
        }

        public void GetTooltip(UI.Control tooltip)
        {
            var panel = new PanelLabeled("Interact") { AutoSize = true, Location = tooltip.Controls.BottomLeft };
            panel.Controls.Add(new Label(this.Name + (this.Length > 0 ? TimeSpan.FromMilliseconds(this.Length).TotalSeconds.ToString(" #0.##s")  : "")) { Location = panel.Controls.BottomLeft }); //this.Length.ToString("#0.##s")
            this.Conditions.GetTooltip(panel);
            tooltip.Controls.Add(panel);
        }

        public float Percentage
        {
            //get { return (float)(1 - this.Time.TotalMilliseconds / this.Length); }
            get { return (float)(1 - this.CurrentTick / this.Length); }
        }
        public virtual void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        {
            if (this.RunningType == RunningTypes.Continuous)
                return;
            if (this.Length <= Engine.TicksPerSecond)
                return;
            var global = parent.Global;

            //Rectangle bounds = camera.GetScreenBounds(global, parent["Sprite"].GetProperty<Sprite>("Sprite").GetBounds());
            var bounds = camera.GetScreenBounds(global, parent.GetSprite().GetBounds());
            var scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
            var barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
            var textLoc = new Vector2(barLoc.X, scrLoc.Y);

            InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, this.Percentage);
            //UIManager.DrawStringOutlined(sb, this.Verb + (this.Time / Engine.TicksPerSecond).ToString(" #0.##s"), textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
            UIManager.DrawStringOutlined(sb, this.Verb, textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);

        }
        public virtual void DrawUI(SpriteBatch sb, Camera camera, GameObject parent, TargetArgs target)
        {
            this.DrawUI(sb, camera, parent);
        }
        public abstract object Clone();
        //{
        //    return new Interaction(this.Name, this.Seconds, this.Callback, this.Conditions, this.Skill);
        //}

        public void AIInit(GameObject agent, TargetArgs target, AIState state)
        {
            var job = new AIJob();
            //state.Job = job;
            state.StartJob(job);
            foreach (var cond in this.Conditions.GetConditions())
                cond.AIInit(agent, target, state);
        }

        //public AIJob GetJob(GameObject agent, TargetArgs target, AI.AIState state)
        //{
        //    AIJob job = new AIJob();
        //    var conditions = this.Conditions.GetConditions();
        //    foreach(var cond in conditions)
        //    {
        //        //if (cond is RangeCheck)
        //        //    continue;
          
        //        // if condition evaluates, continue, otherwise try to get instruction and if it's null, return
        //        if (cond.Condition(agent, target))// == null)
        //            continue;
        //        //var instruction = cond.AIGetPreviousStep(agent, target, state);
        //        AIInstruction instruction;
        //        if (cond.AITrySolve(agent, target, state, out instruction))
        //            continue;

        //        // if the condition didn't evaluate and it did output null, it didn't find a solution so return null
        //        if (instruction == null)
        //            return null;
        //        //job.Instructions.Add(instruction);
        //        //job.Instructions.Enqueue(instruction);
        //        job.AddStep(instruction);

        //    }
        //    //job.Instructions.Add(new AIInstruction(target, this));
        //    //job.Instructions.Enqueue(new AIInstruction(target, this));
        //    job.AddStep(new AIInstruction(target, this));
        //    return job;
        //}

        public bool IsValid(GameObject actor, TargetArgs target)
        {
            return (this.Conditions.Evaluate(actor, target) && !this.IsCancelled(actor, target));
        }

        public virtual string GetCompletedText(GameObject actor, TargetArgs target)
        {
            return this.Name + ": " + target.ToString();
        }
        public void Write(BinaryWriter w)
        {
            w.Write(this.CurrentTick);
            w.Write((int)this.State);
            this.WriteExtra(w);
        }
        public void Read(BinaryReader r)
        {
            this.CurrentTick = r.ReadSingle();
            this.State = (States)r.ReadInt32();
            this.ReadExtra(r);
        }
        protected virtual void WriteExtra(BinaryWriter w) { }
        protected virtual void ReadExtra(BinaryReader r) { }

        public SaveTag SaveAs(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            //var list = this.Save();
            //if (list == null || !list.Any())
            //    return null;
            //foreach (var t in list)
            //    tag.Add(t);
            tag.Add(this.GetType().FullName.Save("Name"));
            tag.Add(((int)this.State).Save("State"));
            //tag.Add(this.Name.Save("Name"));
            tag.Add(this.CurrentTick.Save("Progress"));
            this.AddSaveData(tag);
            return tag;
        }
        //public virtual List<SaveTag> Save()
        //{
        //    var list = new List<SaveTag>();
        //    return list;
        //}
        protected virtual void AddSaveData(SaveTag tag) { }
        public virtual void LoadData(SaveTag tag)
        {
        }
        //static public T Load<T>(SaveTag tag) where T : Interaction
        //{
        //    //return Factory[type.FullName](tag) as T;
        //    var inter = Factory[typeof(T).FullName]() as T;
        //    inter.LoadData(tag);
        //    return inter;
        //}
        static public Interaction Load(SaveTag tag)
        {
            var name = (string)tag["Name"].Value;
            //var inter = Factory[name]();
            var inter = Activator.CreateInstance(Type.GetType(name)) as Interaction;
            tag.TryGetTagValue<int>("State", t => inter.State = (States)t);
            tag.TryGetTagValue<float>("Progress", out inter.CurrentTick);
            inter.LoadData(tag);
            return inter;
        }
        internal virtual void Synced(IMap map)
        {
        }

        internal virtual void InitAction(GameObject actor, TargetArgs target)
        {
            if (this.Length == 0)
            {
                this.Perform(actor, target);
                this.Finish(actor, target);
            }
        }
        internal virtual void FinishAction(GameObject actor, TargetArgs target)
        {
        }
        public void Finish(GameObject actor, TargetArgs target)
        {
            this.State = States.Finished;
            actor.Net.Map.EventOccured(Message.Types.InteractionSuccessful, actor, this);
            //AILog.TryWrite(actor, "Success: " + this.GetCompletedText(actor, target)); // do that here before calling perform() because the target might get disposed
        }
        internal virtual void AfterLoad(GameObject actor, TargetArgs target)
        {
            this.Animation.Entity = actor;
        }
        public bool HasFinished { get { return this.State == States.Finished; } }
    }
}
