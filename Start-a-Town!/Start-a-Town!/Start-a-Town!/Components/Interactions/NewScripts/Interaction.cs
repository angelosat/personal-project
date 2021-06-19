using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Graphics;
using Start_a_Town_.AI;
using Start_a_Town_.Graphics.Animations;

namespace Start_a_Town_.Components.Interactions
{
    public abstract class Interaction : ICloneable
    {
        public override string ToString()
        {
            return "Interaction: " + this.Name;
        }

        public enum States { Unstarted, Running, Finished, Failed }
        public enum RunningTypes { Once, Continuous }

        public RangeCheck RangeCheckCached
        {
            get
            {
                return this.Conditions.GetAllChildren().FirstOrDefault(c => c is RangeCheck) as RangeCheck;
            }
        }
        public bool InRange(GameObject a, TargetArgs t)
        {
            return this.RangeCheckCached.Condition(a, t);
        }
        //public bool RangeCheck(Vector3 a, Vector3 t)
        //{
        //    return this.RangeCheckCached.Condition(a, t);
        //}
        //public AIGoalState CancelState;
        public virtual ScriptTaskCondition CancelState { get; set; }

        public bool IsCancelled(GameObject a, TargetArgs t)
        {
            if (t.Type == TargetType.Entity)
                if (AI.AIState.IsItemReserved(t.Object))
                {
                    // TODO: make the reserved collection a dictionary containing the actor who reserved the item
                    //"WARNING! "
                    //return true;
                }
            //return this.CancelState == null ? false : !this.CancelState.IsMet(a, t);
            return this.CancelState == null ? false : !this.CancelState.Condition(a, t);
        }

        public States State = States.Unstarted;
        public RunningTypes RunningType = RunningTypes.Once;
        public string Name { get; set; }
        public string Verb { get; set; }
        public Action<GameObject, TargetArgs> Callback { get; set; }
        //public Func<ScriptTaskCondition> Condition { get; set; }
        //public ScriptTaskCondition Condition { get; set; }

        // TODO: use an static readonly empty condition collection to return in the virtual method
        public virtual TaskConditions Conditions { get { return null; } }//set; }
        public float Length { get; set; }
        //public TimeSpan Time { get; set; }
        public float Time { get; set; }
        public Skill Skill { get; set; }
        public float Seconds { get; set; }
        //AnimationCollection Animation = AnimationCollection.Working;
        protected AnimationCollection Animation = new AnimationWork();// AnimationCollection.Working;

        // TODO: i need a method that returns satisfaction score based on ai entity's state
        //public HashSet<Needs.Need.Types> NeedSatisfaction = new HashSet<Needs.Need.Types>();
        static readonly Dictionary<Needs.Need.Types, float> _NeedSatisfaction = new Dictionary<Needs.Need.Types, float>();
        public virtual Dictionary<Needs.Need.Types, float> NeedSatisfaction //= new Dictionary<Needs.Need.Types, float>();
        {
            get { return _NeedSatisfaction; }
        }
        //protected AnimationCollection Ani;

        public Interaction()
        {
            this.Callback = (a, t) => { };
            //this.Conditions = new TaskConditions();
        }
        //public TargetArgs Target { get; set; }
        public Interaction(string name, Action<GameObject, TargetArgs> callback) : this(name, 0, callback, new TaskConditions(), null) { }
        public Interaction(string name, float seconds, Action<GameObject, TargetArgs> callback) : this(name, seconds, callback, new TaskConditions(), null) { }
        public Interaction(string name, float seconds, Action<GameObject, TargetArgs> callback, Skill skill) : this(name, seconds, callback, new TaskConditions(), skill) { }
        public Interaction(string name, float seconds, Action<GameObject, TargetArgs> callback, TaskConditions conditions) : this(name, seconds, callback, conditions, null) { }
        public Interaction(string name, float seconds, Action<GameObject, TargetArgs> callback, TaskConditions conditions, Skill skill)
        {
            this.Name = name;
            this.Callback = callback;
            this.Seconds = seconds;
            //this.Length = seconds * 1000;
            //this.Time = TimeSpan.FromMilliseconds(this.Length);
            this.Time = this.Length = seconds * Engine.TargetFps;
            //this.Conditions = conditions;
            this.Skill = skill;
        }

        public void SetSeconds(float seconds)
        {
            this.Time = this.Length = seconds * Engine.TargetFps;
        }

        protected Interaction(string name, float seconds = 0) //: this(name, seconds, new TaskConditions(), null)
            :this()
        {
            this.Name = name;
            this.Seconds = seconds;
            this.Time = this.Length = seconds * Engine.TargetFps;
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

        public virtual void Interrupt(GameObject parent)
        {
            parent.Net.EventOccured(Message.Types.Interrupt, parent, this);
            this.State = States.Finished;
            //parent.Body.FadeOut(this.Animation);
            parent.Body.FadeOutAnimation(this.Animation);

        }

        public virtual void Perform(GameObject a, TargetArgs t)
        {
            this.Callback(a, t);
        }

        public virtual void Start(GameObject a, TargetArgs t)
        { }
        
        public virtual void Update(GameObject actor, TargetArgs target)
        {
            var instr = new AIInstruction(target, this);
            if (this.State == States.Finished) // TODO: maybe check for failed state too?
            {
                Stop(actor);
                return;
            }

            if (this.IsCancelled(actor, target))
            {
                Stop(actor);
                this.State = States.Failed;
                actor.Net.Map.EventOccured(Message.Types.InteractionFailed, actor, instr , this.CancelState);
                AI.AILog.TryWrite(actor, "Failed: " + instr.ToString());
                return;
            }

            //if (this.State == States.Finished)
            //{
            //    actor.Body.FadeOut(this.Animation);
            //    return;
            //}
            //if !condition then finish

            //var failed = this.Conditions.Evaluate(actor, target);
            //if (failed != null)

            //if(!(actor.Net is Net.Client))  // workaround until i handle interaction packets on time with server clock timestamps
            if (!this.Conditions.Evaluate(actor, target))
            {
                //actor.Net.EventOccured(Message.Types.InteractionFailed, actor, failed);
                ScriptTaskCondition failed = this.Conditions.GetFailedCondition(actor, target);
                //this.State = States.Finished;
                this.State = States.Failed;
                actor.Net.Map.EventOccured(Message.Types.InteractionFailed, actor, instr, failed);
                AI.AILog.TryWrite(actor, "Failed: " + instr.ToString());

                Stop(actor);
                return;
            }
            if (this.State == States.Unstarted)
                this.Start(actor, target);
            else if (this.State == States.Finished)
            {
                Stop(actor);
                actor.Net.Map.EventOccured(Message.Types.InteractionSuccessful, actor, instr);
                AI.AILog.TryWrite(actor, "Success: " + instr.ToString());

                return;
            }

           // this.Time = this.Time.Add(TimeSpan.FromMilliseconds(-Engine.Tick));
            //this.Time--;
            if (this.State == States.Unstarted)
            {
                //actor.Body.Start(this.Animation);
                actor.Body.AddAnimation(this.Animation);
                //actor.Body.CrossFade(this.Animation, true, 10);
                //this.Animation.Contact = () => Contact(actor, target);
            }
                this.State = States.Running;
            if(this.RunningType == RunningTypes.Continuous)
            {
                this.Perform(actor, target);// Callback(actor, target);
                if (this.State == States.Finished)
                {
                    Stop(actor);
                    actor.Net.Map.EventOccured(Message.Types.InteractionSuccessful, actor, instr);
                    AI.AILog.TryWrite(actor, "Success: " + instr.ToString());

                }
                return;
            }
            this.Time--;
            //if (this.Time.TotalMilliseconds <= 0)
            if (this.Time <= 0)
            {
                //actor.Net.ToConsole();
                this.State = States.Finished;
                Stop(actor);
                this.Perform(actor, target);// Callback(actor, target);
                actor.Net.Map.EventOccured(Message.Types.InteractionSuccessful, actor, instr);
                AI.AILog.TryWrite(actor, "Success: " + instr.ToString());

            }
        }

        protected virtual void Stop(GameObject actor)
        {
            actor.Body.FadeOutAnimation(this.Animation);
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
            return this.Conditions.Evaluate(actor, target);

            return true;
        }

        public void GetTooltip(UI.Control tooltip)
        {
            PanelLabeled panel = new PanelLabeled("Interact") { AutoSize = true, Location = tooltip.Controls.BottomLeft };
            panel.Controls.Add(new Label(this.Name + (this.Length > 0 ? TimeSpan.FromMilliseconds(this.Length).TotalSeconds.ToString(" #0.##s")  : "")) { Location = panel.Controls.BottomLeft }); //this.Length.ToString("#0.##s")
            this.Conditions.GetTooltip(panel);
            tooltip.Controls.Add(panel);
        }

        public float Percentage
        {
            //get { return (float)(1 - this.Time.TotalMilliseconds / this.Length); }
            get { return (float)(1 - this.Time / this.Length); }
        }
        public void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        {
            if (this.RunningType == RunningTypes.Continuous)
                return;

            Vector3 global = parent.Global;

            //Rectangle bounds = camera.GetScreenBounds(global, parent["Sprite"].GetProperty<Sprite>("Sprite").GetBounds());
            Rectangle bounds = camera.GetScreenBounds(global, parent.GetSprite().GetBounds());
            Vector2 scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
            Vector2 barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
            Vector2 textLoc = new Vector2(barLoc.X, scrLoc.Y);
            InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, this.Percentage);
            //UIManager.DrawStringOutlined(sb, this.Name + this.Time.TotalSeconds.ToString(" #0.##s"), textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
            UIManager.DrawStringOutlined(sb, this.Verb + (this.Time / Engine.TargetFps).ToString(" #0.##s"), textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);

        }

        public abstract object Clone();
        //{
        //    return new Interaction(this.Name, this.Seconds, this.Callback, this.Conditions, this.Skill);
        //}

        public void AIInit(GameObject agent, TargetArgs target, AI.AIState state)
        {
            AIJob job = new AIJob();
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
    }
}
