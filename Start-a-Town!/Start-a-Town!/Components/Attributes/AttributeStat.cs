using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_
{
    public class AttributeStat : Inspectable, ISaveable, ISerializable
    {
        public class ValueModifier
        {
            readonly float _Value;
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

        public float Tick = Ticks.TicksPerSecond / 0.5f; //1 tick per 2 seconds
        public float Timer = 0;
        public float RegenerationRate = 1;
        public Progress Rec = new(0, Ticks.TicksPerSecond, Ticks.TicksPerSecond);
        public float DecayRate = -0.5f;
        public float GainRate = 0;
        public List<ValueModifier> Modifiers = new();
        public ProgressLeveledExp Progress;
        public AttributeDef Def;

        public int Level { get => this.Progress.Level; set => this.Progress.SetLevel(value); }
        public int Min = 10;
        const int BaseXpToLevel = 100;//5; //placeholder
        public AttributeStat(AttributeDef def, int value = 10)
        {
            this.Def = def;
            this.Progress = new ProgressLeveledExp(BaseXpToLevel, value);
        }
        public AttributeStat()
        {
            this.Progress = new ProgressLeveledExp(BaseXpToLevel, 10);
        }
        public void Update(GameObject parent)
        {
            this.Def.Worker.Tick(parent, this);
            //this.Def.Tick(parent, this);
        }

        public override string ToString()
        {
            return this.Def.Name + ": " + this.Level;
        }

        internal void AddToProgress(GameObject parent, float p)
        {
            this.Progress.Value += p;
            if (p > 0)
                this.Rec.Value = this.Rec.Max;
            if (this.Progress.Percentage == 1)
            {
                this.Progress.Value++;
                this.Progress.Value = 0;
            }
            else if (this.Progress.Percentage == 0)
            {
                if (this.Level == this.Min)
                    return;
                this.Progress.Value--;
                this.Progress.Value = this.Progress.Max;
            }
        }

        public Control GetControl()
        {
            var label = new Label()
            {
                TextFunc = () => string.Format("{0}: {1}", this.Def.Name, this.Level),
                TooltipFunc = (t) =>
                {
                    t.AddControlsBottomLeft(
                        new Label(this.Def.Description),
                        this.Progress.GetControl());
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
            tag.Add(this.Progress.Save("Progress"));
            return tag;
        }

        public ISaveable Load(SaveTag tag)
        {
            tag.TryGetTag("Progress", this.Progress.Load);
            return this;
        }

        public void Write(BinaryWriter w)
        {
            this.Progress.Write(w);
        }

        public ISerializable Read(BinaryReader r)
        {
            this.Progress.Read(r);
            return this;
        }
    }
}
