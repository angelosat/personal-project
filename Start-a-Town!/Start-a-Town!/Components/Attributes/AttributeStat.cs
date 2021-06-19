using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Start_a_Town_.Components;
using System.IO;

namespace Start_a_Town_
{
    class AttributeStat : ISaveable, ISerializable
    {
        public class ValueModifier
        {
            float _Value;
            readonly Func<float> ValueGetter;
            public ValueModifier(float value)
            {
                this._Value = value;
                this.ValueGetter = () => this._Value;
            }
            public ValueModifier(Func<float> valueGetter)
            {
                this.ValueGetter = valueGetter;
            }
            public float GetValue()
            {
                return this.ValueGetter();
            }
        }

        public float Tick = Engine.TicksPerSecond / 0.5f; //1 tick per 2 seconds
        public float Timer = 0;
        public float RegenerationRate = 1;
        public Progress Rec = new(0, Engine.TicksPerSecond, Engine.TicksPerSecond);
        public float DecayRate = -0.5f;
        public float GainRate = 0;
        public List<ValueModifier> Modifiers = new();
        //public List<AttributeProgressAwarderDef> Awarders = new List<AttributeProgressAwarderDef>();
        public ProgressLeveledExp Progress;// = new ProgressLeveledExp(5);
        public AttributeDef Def;

        public int Level { get { return this.Progress.Level; } set { this.Progress.SetLevel(value); } }
        //public Progress Progress = new Progress() { Min = 0, Max = 100, Value = 0 };
        public int Min = 10;
        const int BaseXpToLevel = 100;//5; //placeholder
        public AttributeStat(AttributeDef def, int value = 10)
        {
            this.Def = def;
            this.Progress = new ProgressLeveledExp(BaseXpToLevel, value);
            //this.Value = value;
        }
        public AttributeStat()
        {
            this.Progress = new ProgressLeveledExp(BaseXpToLevel, 10);
        }
        public void Update(GameObject parent)
        {
            this.Def.TryAward(parent, this);
            return;
            //if (this.Timer > 0)
            //{
            //    this.Timer--;
            //    return;
            //}
            //float gain = 0;
            //if (this.Modifiers.Count > 1)
            //    throw new Exception();
            //foreach (var v in from mod in this.Modifiers select mod.GetValue())
            //    gain += v;
            //if (gain != 0)
            //    this.AddToProgress(parent, gain);//-0.5f);
            //this.Timer = this.Tick;
            //if (this.Rec.Value > 0)
            //{
            //    this.Rec.Value--;
            //    return;
            //}
        }

        //public object Clone()
        //{
        //    //return new AttributeDef(this.ID, this.Name, this.Value) { Description = this.Description };
        //    return new AttributeStat(this.Def, this.Value);
        //}

        //static public AttributeDef Create(Types id, int value)
        //{
        //    AttributeDef a = GetAttribute(id).Clone() as AttributeDef;
        //    a.Value = value;
        //    return a;
        //}

        public override string ToString()
        {
            return this.Def.Name + ": " + this.Level;
        }

        internal void AddToProgress(GameObject parent, float p)
        {
            this.Progress.Value += p;
            parent.Net.EventOccured(Message.Types.AttributeProgressChanged, this);
            if (p > 0)
                this.Rec.Value = this.Rec.Max;
            //if (this.Progress.Percentage < 1)
            //    return;
            if (this.Progress.Percentage == 1)
            {
                //this.Value++;
                this.Progress.Value++;
                this.Progress.Value = 0;
                parent.Net.EventOccured(Message.Types.AttributeChanged, this);
            }
            else if (this.Progress.Percentage == 0)
            {
                if (this.Level == this.Min)
                    return;
                //this.Value--;
                this.Progress.Value--;
                this.Progress.Value = this.Progress.Max;
                parent.Net.EventOccured(Message.Types.AttributeChanged, this);
            }
        }

        public Control GetControl()
        {
            var label = new Label()
            {
                TextFunc = () => string.Format("{0}: {1}", this.Def.Name, this.Level),
                //HoverText = this.Description,
                TooltipFunc = (t) =>
                {
                    t.AddControlsBottomLeft(
                        new Label(this.Def.Description),
                        this.Progress.GetControl());
                        //new Label() { TextFunc = () => string.Format("Current Value: {0}", this.Value) },
                        //new Label() { TextFunc = () => string.Format("Progress: {0} / {1}", this.Progress.Value, this.Progress.Max) });
                }
            };
            return label;
        }
        public AttributeStat Clone()
        {
            return new AttributeStat(this.Def, this.Level);
        }
        public SaveTag Save(string name = "")
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);
            //this.Def.Name.Save(tag, "Def");
            tag.Add(this.Progress.Save("Progress"));
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            //this.Def = global::Start_a_Town_.Def.GetDef<AttributeDef>(tag.GetValue<string>("Def"));
            tag.TryGetTag("Progress", this.Progress.Load);
            return this;
        }

        public void Write(BinaryWriter w)
        {
            //w.Write(this.Progress.Level);
            //w.Write(this.Progress.Value);
            this.Progress.Write(w);
        }

        public ISerializable Read(BinaryReader r)
        {
            this.Progress.Read(r);
            return this;
        }
    }
}
