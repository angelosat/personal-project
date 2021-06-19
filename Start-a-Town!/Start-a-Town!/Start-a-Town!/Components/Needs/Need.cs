using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.AI;
using Start_a_Town_.Components.AI;

namespace Start_a_Town_.Components.Needs
{
    public abstract class Need : ICloneable, IProgressBar
    {
        public class Factory
        {
            
            static Dictionary<Need.Types, Need> _Registry;
            public static Dictionary<Need.Types, Need> Registry
            {
                get
                {
                    if (_Registry.IsNull())
                        Initialize();
                    return _Registry;
                }
            }
            static void Initialize()
            {
                _Registry = new Dictionary<Need.Types, Need>();
                Register(
                    new NeedHunger(),
                    new NeedCuriosity()
                    //new Need(Types.Hunger, "Hunger", 100f),
                    //new Need(Types.Water, "Water", 100f),
                    //new Need(Types.Sleep, "Sleep", 100f),
                    //new Need(Types.Achievement, "Achievement", 100f),
                    //new Need(Types.Work, "Work", 100f),
                    //new Need(Types.Brains, "Brains", 100f)
                    );
            }
            static public void Register(params Need[] needs)
            {
                foreach (var need in needs)
                    _Registry.Add(need.ID, need);
            }
            static public Need Create(Need.Types id, float value = 100f, float decay = .1f, float tolerance = 50f)
            {
                return (Registry[id].Clone() as Need).Initialize(value, decay, tolerance);
            }
        }

        //static public readonly Need Hunger = new NeedHunger();

        public enum Types { Hunger, Water, Sleep, Achievement, Work, Brains, Curiosity }
        static string Format = "##0";//.00";
        public virtual Types ID { get; set; }
        public virtual string Name { get; set; }
        float _Value;
        public virtual float Value
        {
            get
            {
                return this._Value;
            }
            set
            {
                this._Value = Math.Max(0, Math.Min(100, value));
            }
        }
        public virtual float Min { get; set; }
        public virtual float Max { get; set; }
        public virtual float Percentage { get { return this.Value / this.Max; } }
        public virtual float Decay { get; set; }
        public virtual float Tolerance { get; set; }
        public override string ToString()
        {
            return Name + " " + Value.ToString();
        }
        public Need()
        {
            this.Min = 0;
            this.Max = 100;
        }
        public Need(Types id, string name, float value, float decay = .5f, float tolerance = 50f):this()
        {
            this.ID = id;
            this.Name = name;
            this.Value = value;
            this.Decay = decay;
            this.Tolerance = tolerance;
        }
        public Need Initialize(float value, float decay = .5f, float tolerance = 50f)
        {
            this.Value = value;
            this.Decay = decay;
            this.Tolerance = tolerance;
            return this;
        }
        //public virtual object Clone()
        //{
        //    return new Need(this.ID, this.Name, this.Value, this.Decay, this.Tolerance);
        //}
        public abstract object Clone();
        public void Update(GameObject parent)
        {
            // TODO: is exponential decay better? maybe have both exp and linear and choose between them for each need?
            float d = this.Decay * (1 + 5 * (1 - Value / 100f) * (1 - Value / 100f));
            d = this.Decay;
            float newValue = Value - d;// Math.Max(0, Value - d);
            //if (Value >= 50 && newValue < 50)
            //{
            //    //parent.PostMessage(Message.Types.Think, parent, "Need low: " + this.Name, "I need to satisfy my: " + this.Name + " soon");
            //    parent.PostMessage(Message.Types.Need, parent, this);
            //}
        //   Value = newValue;
            SetValue(newValue, parent);
        }

        public virtual List<AIJob> FindPotentialJobs(GameObject entity, Dictionary<GameObject, List<Interactions.Interaction>> allInteractions) { return null; }
        //{
        //    List<AIJob> jobs = new List<AIJob>();
        //    var state = AIState.GetState(entity);
        //    var memory = state.Knowledge.Objects;
        //    //var lowest = memory.Values.Min(m => m.Score);
        //    var ordered = memory.Values.Where(o => o.Object.Exists).OrderBy(m => Vector3.DistanceSquared(entity.Global, m.Object.Global));
        //    foreach (var m in ordered)
        //    {
        //        var obj = m.Object;
        //        var interactions = obj.GetInteractionsList();
        //        var examine = interactions.FirstOrDefault(i => //i.NeedSatisfaction.ContainsKey(this.ID));
        //            {
        //                float value;
        //                if (i.NeedSatisfaction.TryGetValue(this.ID, out value))
        //                    return value <= this.Max - this.Value;
        //                return false;
        //            });
        //        if (examine == null)
        //            continue;
        //        //return new AIInstruction(new TargetArgs(obj), examine);// examine;
        //        var job = new AIJob();
        //        job.AddStep(new AIInstruction(new TargetArgs(obj), examine));// examine;
        //        jobs.Add(job);
        //    }
        //    return jobs;// GetScores(entity, jobs);// jobs;
        //}

        [Obsolete]
        public void SetValue(float newVal, GameObject parent)
        {
            float oldVal = Value;
            if (oldVal >= Tolerance && newVal < Tolerance)
            {
                //throw new NotImplementedException();
                //parent.PostMessage(Message.Types.Need, parent, this);
            }
            this.Value = Math.Max(0, Math.Min(100, newVal));
        }

        public Bar ToBar(GameObject parent)
        {
            Bar bar = new Bar()
            {
                ColorFunc = () => Color.Lerp(Color.Red, Color.Lime, this.Value / 100f),
                Object = this,
                //Tag = this,
                //PercFunc = () => this.Value / 100f,
                NameFunc = () => this.Name,
                HoverFunc = () => Name + ": " + Value.ToString(Format).PadLeft(Format.Length),
                HoverFormat = this.Name + ": " + Format,

            };
            bar.LeftClickAction = () =>
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                {
                   // this.Value = 100 * (UIManager.Mouse.X - bar.ScreenLocation.X) / (float)bar.Width;
                    this.SetValue(100 * (UIManager.Mouse.X - bar.ScreenLocation.X) / (float)bar.Width, parent);
                    return;
                }
            };
            return bar;
        }

        public Panel GetUI(GameObject entity)
        {
            var panel = new Panel() { AutoSize = true, BackgroundStyle = BackgroundStyle.TickBox };
            panel.Controls.Add(this.ToBar(entity));
            return panel;
        }
    }
}
