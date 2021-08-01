using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.AI;
using Start_a_Town_.Animations;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public abstract class Interaction : ICloneable
    {
        public bool IsFinished => this.State == States.Finished;
        public static readonly float DefaultRange = (float)Math.Sqrt(2);

        readonly static Dictionary<string, Func<Interaction>> Factory = new();
        
        static public void AddInteraction<T>(Func<Interaction> factory)
        {
            Factory[typeof(T).FullName] = factory;
        }
        static public void AddInteraction<T>() where T : Interaction, new()
        {
            Factory[typeof(T).FullName] = () => new T();
        }
        
        internal virtual void OnToolContact(Actor parent, TargetArgs target)
        {
        }

        public static void Initialize()
        {
            
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
       
        public States State { get; protected set; } = States.Unstarted;

        public RunningTypes RunningType = RunningTypes.Once;

        public string Name { get; set; }
        public string Verb { get; set; }
        public Action<GameObject, TargetArgs> Callback { get; set; }
        
        public float Length { get; set; }
        public float CurrentTick;
        public ToolAbilityDef Skill { get; set; }
        public float Seconds { get; set; }
        public Animation Animation = new(AnimationDef.Work);
        internal Actor Actor;
        internal TargetArgs Target;

        // TODO: i need a method that returns satisfaction score based on ai entity's state
        static readonly Dictionary<Need.Types, float> _NeedSatisfaction = new();
        public virtual Dictionary<Need.Types, float> NeedSatisfaction 
        {
            get { return _NeedSatisfaction; }
        }

        public Interaction()
        {
            this.Callback = (a, t) => { };
        }
        public Interaction(string name, Action<GameObject, TargetArgs> callback) : this(name, 0, callback, null) { }
        public Interaction(string name, float seconds, Action<GameObject, TargetArgs> callback) : this(name, seconds, callback, null) { }
        public Interaction(string name, float seconds, Action<GameObject, TargetArgs> callback, ToolAbilityDef skill)
        {
            this.Name = name;
            this.Callback = callback;
            this.Seconds = seconds;
            this.CurrentTick = this.Length = seconds * Engine.TicksPerSecond;
            this.Skill = skill;
        }

        protected Interaction(string name, float seconds = 0) 
            :this()
        {
            this.Name = name;
            this.Seconds = seconds;
            this.CurrentTick = this.Length = seconds * Engine.TicksPerSecond;
        }
        
        public virtual void Interrupt(Actor parent, bool success)
        {
            if(!success)
                parent.Net.EventOccured(Message.Types.InteractionInterrupted, parent, this);
            this.State = States.Finished;
            this.Animation?.FadeOutAndRemove();

        }

        public virtual void Perform(Actor a, TargetArgs t)
        {
            this.Callback(a, t);
        }

        public virtual void Start(Actor a, TargetArgs t)
        {
            if (this.Animation != null)
                a.AddAnimation(this.Animation);
        }

        internal bool Evaluate(Actor a, TargetArgs t)
        {
            return true;
        }

        public virtual void Update(Actor actor, TargetArgs target)
        {
            if (this.State == States.Finished) // TODO: maybe check for failed state too?
            {
                Stop(actor);
                return;
            }

            if (this.State == States.Unstarted)
                this.Start(actor, target);
            else if (this.State == States.Finished)
            {
                Stop(actor);
                AILog.TryWrite(actor, "Success: " + this.GetCompletedText(actor, target));
                return;
            }

            this.State = States.Running;
            if(this.RunningType == RunningTypes.Continuous)
            {
                this.Perform(actor, target);
                if (this.State == States.Finished)
                {
                    Stop(actor);
                    AILog.TryWrite(actor, "Success: " + this.GetCompletedText(actor, target));
                }
                return;
            }
            this.CurrentTick--;
            if (this.CurrentTick <= 0)
            {
                Finish(actor, target);
                Stop(actor);
                this.Perform(actor, target);
            }
        }

        protected virtual void Stop(GameObject actor)
        {
            this.Animation.FadeOutAndRemove();
        }
        
        public void GetTooltip(Control tooltip)
        {
            var panel = new PanelLabeled("Interact") { AutoSize = true, Location = tooltip.Controls.BottomLeft };
            panel.Controls.Add(new Label(this.Name + (this.Length > 0 ? TimeSpan.FromMilliseconds(this.Length).TotalSeconds.ToString(" #0.##s")  : "")) { Location = panel.Controls.BottomLeft }); //this.Length.ToString("#0.##s")
            tooltip.Controls.Add(panel);
        }

        public float Percentage
        {
            get { return (float)(1 - this.CurrentTick / this.Length); }
        }
        public virtual void DrawUI(SpriteBatch sb, Camera camera, GameObject parent)
        {
            if (this.RunningType == RunningTypes.Continuous)
                return;
            if (this.Length <= Engine.TicksPerSecond)
                return;
            var global = parent.Global;

            var bounds = camera.GetScreenBounds(global, parent.GetSprite().GetBounds());
            var scrLoc = new Vector2(bounds.X + bounds.Width / 2f, bounds.Y);//
            var barLoc = scrLoc - new Vector2(InteractionBar.DefaultWidth / 2, InteractionBar.DefaultHeight / 2);
            var textLoc = new Vector2(barLoc.X, scrLoc.Y);

            InteractionBar.Draw(sb, barLoc, InteractionBar.DefaultWidth, this.Percentage);
            UIManager.DrawStringOutlined(sb, this.Verb, textLoc, HorizontalAlignment.Left, VerticalAlignment.Center, 0.5f);
        }
        public virtual void DrawUI(SpriteBatch sb, Camera camera, GameObject parent, TargetArgs target)
        {
            this.DrawUI(sb, camera, parent);
        }
        public abstract object Clone();
        
        public virtual string GetCompletedText(Actor actor, TargetArgs target)
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
            tag.Add(this.GetType().FullName.Save("Name"));
            tag.Add(((int)this.State).Save("State"));
            tag.Add(this.CurrentTick.Save("Progress"));
            this.AddSaveData(tag);
            return tag;
        }
        
        protected virtual void AddSaveData(SaveTag tag) { }
        public virtual void LoadData(SaveTag tag)
        {
        }
        
        static public Interaction Load(SaveTag tag)
        {
            var name = (string)tag["Name"].Value;
            var inter = Activator.CreateInstance(Type.GetType(name)) as Interaction;
            tag.TryGetTagValue<int>("State", t => inter.State = (States)t);
            tag.TryGetTagValue<float>("Progress", out inter.CurrentTick);
            inter.LoadData(tag);
            return inter;
        }
        internal virtual void Synced(MapBase map)
        {
        }

        internal virtual void InitAction(Actor actor, TargetArgs target)
        {
            if (this.Length == 0)
            {
                this.Perform(actor, target);
                this.Finish(actor, target);
            }
        }
        internal virtual void FinishAction(Actor actor, TargetArgs target)
        {
        }
        public void Finish(Actor actor, TargetArgs target)
        {
            this.State = States.Finished;
        }
        internal virtual void AfterLoad(Actor actor, TargetArgs target)
        {
            this.Animation.Entity = actor;
        }
        public bool HasFinished { get { return this.State == States.Finished; } }
    }
}
