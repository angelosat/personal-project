using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.AI;
using Start_a_Town_.Components.AI;
using Start_a_Town_.AI.Behaviors;

namespace Start_a_Town_//.Components.Needs
{
    public abstract class Need : IProgressBar, ISerializable, ISaveable //ICloneable,
    {
        internal void AddMod(NeedLetDef needLetDef, float value, float rate)
        {
            if (this.Mods.Any(n => n.Def == needLetDef))
                throw new Exception();
            var needLet = new NeedLet(needLetDef, value, rate);
            this.Mods.Add(needLet);
        }
        internal void RemoveMod(NeedLetDef def)
        {
            this.Mods.RemoveAll(n => n.Def == def);
        }

        //static public readonly Need Hunger = new NeedHunger();
        public NeedDef NeedDef;
        public enum Types { Hunger, Water, Sleep, Achievement, Work, Brains, Curiosity, Social, Energy }
        const string Format = "P0";// "##0%";//.00";
        public virtual Types ID { get; set; }
        public virtual string Name { get { return this.NeedDef.Name; } set { } }
        public float DecayDelay, DecayDelayMax = 3;
        float _Value;
        public double LastTick;
        public virtual float Value
        {
            get
            {
                return this._Value + this.Mods.Sum(m => m.ValueMod);
            }
            set
            {
                this._Value = MathHelper.Clamp(value, 0, 100);
            }
        }
        public Actor Parent;
        public float Min = 0f;
        public float Max = 100f;
        public virtual float Percentage { get { return this.Value / this.Max; } }
        public float Decay;
        public float Mod;// { get; set; }
        readonly List<NeedLet> Mods = new();
        public virtual float Tolerance { get; set; }
        public virtual float Threshold { get { return this.NeedDef.BaseThreshold; } }// 50; } }
        public virtual bool IsBelowThreshold { get { return this.Value < this.Threshold; } }
        public override string ToString()
        {
            //var txt = Name + ": " + Value.ToString(Format).PadLeft(Format.Length);// Name + " " + Value.ToString();
            var txt = $"{Name}: {this.Percentage:P0}";// Name + " " + Value.ToString();

            foreach (var needlet in Mods)
                txt += $"\n{needlet}";
            return txt;
        }
        //public Need()
        //{
        //    //this.Def = def;
        //    this.Value = def.BaseValue;// this.Max;
        //}
        //public Need()
        //{
        //    this.Min = 0;
        //    this.Max = 100;
        //}
        public Need(Actor parent)
        {
            this.Parent = parent;
            this._Value = this.Max;
        }
        //public abstract object Clone();
        //public object Clone()
        //{
        //    return Activator.CreateInstance(this.Def.Type, this.Def);
        //}
        //public virtual object Clone() { return new Need(this.Def); }
        public virtual void TickLong(GameObject parent) { }
        public virtual void Tick(GameObject parent)
        {
            this.LastTick = parent.NetNew.CurrentTick;
            float newValue;
            //var mod = this.Mod;
            var mod = this.Mods.Sum(d => d.RateMod);
            if (mod != 0)
            {
                newValue = this._Value + mod;
                this.DecayDelay = this.DecayDelayMax;
            }
            else
            {
                if (this.DecayDelay > 0)
                {
                    this.DecayDelay--;
                    return;
                }
                else
                {
                    // TODO: is exponential decay better? maybe have both exp and linear and choose between them for each need?
                    var p = 1 - Value / 100f;
                    float d = this.Decay * (1 + 5 * p * p);
                    d = this.Decay;
                    d = this.NeedDef.BaseDecayRate;
                    newValue = this._Value - d;// Math.Max(0, Value - d);
                }
            }
           
            //if (Value >= 50 && newValue < 50)
            //{
            //    //parent.PostMessage(Message.Types.Think, parent, "Need low: " + this.Name, "I need to satisfy my: " + this.Name + " soon");
            //    parent.PostMessage(Message.Types.Need, parent, this);
            //}
        //   Value = newValue;

            SetValue(newValue, parent);
        }

        public virtual AITask GetTask(GameObject parent) { return null; }
        
        public virtual TaskGiver TaskGiver { get { return this.NeedDef.TaskGiver; } }


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
            if (this.Value > oldVal)
                this.DecayDelay = DecayDelayMax;
            //if(parent.Net is Server)
            //    PacketNeedModify.Send(parent.Net as Server, parent.InstanceID, (int)this.ID, newVal - oldVal);
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
                HoverFunc = () => this.ToString(),// Name + ": " + Value.ToString(Format).PadLeft(Format.Length),
                HoverFormat = this.Name + ": " + Format,

            };
            bar.LeftClickAction = () =>
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                {
                    //this.SetValue(100 * (UIManager.Mouse.X - bar.ScreenLocation.X) / (float)bar.Width, parent);
                    "todo: request need change from server".ToConsole();
                    return;
                }
            };
            return bar;
        }

        public Panel GetUI(GameObject entity)
        {
            var panel = new Panel() { AutoSize = true, BackgroundStyle = BackgroundStyle.TickBox};
            panel.Controls.Add(this.ToBar(entity));
            return panel;
        }

        public void Write(System.IO.BinaryWriter w)
        {
            this.NeedDef.Write(w);
            w.Write(this.Value);
            w.Write(this.Mod);
            w.Write(this.DecayDelay);
            this.Mods.Write(w);
        }
        public ISerializable Read(System.IO.BinaryReader r)
        {
            this.NeedDef = r.ReadDef<NeedDef>();
            this.Value = r.ReadSingle();
            this.Mod = r.ReadSingle();
            this.DecayDelay = r.ReadSingle();
            this.Mods.Read(r);
            return this;
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            //var tag = new SaveTag(SaveTag.Types.Compound, this.NeedDef.Name);
            this.NeedDef.Save(tag, "Def");
            tag.Add(this.Value.Save("Value"));
            tag.Add(this.Mod.Save("Mod"));
            tag.Add(this.DecayDelay.Save("DecayTimer"));
            tag.Add(this.Mods.SaveNewBEST("Mods"));
            return tag;
        }
        public ISaveable Load(SaveTag tag)
        {
            //this.NeedDef = Def.GetDef<NeedDef>(tag.Name);
            //this.NeedDef = tag.LoadDef<NeedDef>("Def");
            tag.TryGetTagValue<string>("Def", v => this.NeedDef = Def.GetDef<NeedDef>(v));
            tag.TryGetTagValue<float>("Value", out this._Value);
            tag.TryGetTagValue<float>("Mod", out this.Mod);
            tag.TryGetTagValue<float>("DecayTimer", out this.DecayDelay);
            //tag.TryGetTag("Mods", t=>)
            this.Mods.TryLoadMutable(tag, "Mods");
            return this;
        }
        //public abstract Behavior GetSatisfyBehavior();

    }
}
