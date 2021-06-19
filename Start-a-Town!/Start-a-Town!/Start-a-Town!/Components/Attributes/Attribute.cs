using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components
{
    class Attribute : ICloneable
    {
        public class ValueModifier
        {
            float _Value;
            Func<float> ValueGetter;
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

        public static readonly Attribute Strength = new Attribute(Types.Strength, "Strength");
        public static readonly Attribute Intelligence = new Attribute(Types.Intelligence, "Intelligence");
        public static readonly Attribute Dexterity = new Attribute(Types.Dexterity, "Dexterity");

        static List<Attribute> _Registry;
        public static List<Attribute> Registry
        {
            get
            {
                if (_Registry.IsNull())
                {
                    _Registry = new List<Attribute>()
                    {
                        Strength, Intelligence, Dexterity
                        //new Attribute(Types.Strength, "Strength"),
                        //new Attribute(Types.Intelligence, "Intelligence"),
                        //new Attribute(Types.Dexterity, "Dexterity"),
                    };
                }
                return _Registry;
            }
        }

        static public Attribute GetAttribute(Types id)
        {
            return Registry.ToDictionary(foo => foo.ID, foo => foo)[id];
        }

        public enum Types { Strength, Intelligence, Dexterity }
        public Types ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Value { get; set; }
        public Progress Progress = new Progress() { Min = 0, Max = 100, Value = 0 };
        public int Min = 10;

        //int Tick, TickRate, TickRateMax;
        //List<ValueModifier> Modifiers = new List<ValueModifier>();
        public float Tick = Engine.TargetFps / 0.5f; //1 tick per 2 seconds
        public float Timer = 0;
        public float RegenerationRate = 1;
        //public Progress Rec = new Progress(0, Engine.TargetFps * 5, Engine.TargetFps * 5);
        public Progress Rec = new Progress(0, Engine.TargetFps, Engine.TargetFps);
        public float DecayRate = -0.5f;
        public float GainRate = 0;
        public List<ValueModifier> Modifiers = new List<ValueModifier>();

        public Attribute(Types id, string name, int value = 0)
        {
            this.ID = id;
            this.Name = name;
            this.Value = value;
            this.Description = "";
            //this.Modifiers.Add(new ValueModifier("Decay"))
        }
        public void Update(GameObject parent)
        {
            if (this.Timer > 0)
            {
                this.Timer--;
                return;
            }
            //this.AddToProgress(parent, this.GainRate);//-0.5f);
            float gain = 0;
            if (this.Modifiers.Count > 1)
                throw new Exception();
            foreach (var v in from mod in this.Modifiers select mod.GetValue())
                gain += v;
            this.AddToProgress(parent, gain);//-0.5f);
            this.Timer = this.Tick;
            if (this.Rec.Value > 0)
            {
                this.Rec.Value--;
                return;
            }
            this.AddToProgress(parent, this.DecayRate);//-0.5f);
        }

        public object Clone()
        {
            return new Attribute(this.ID, this.Name, this.Value) { Description = this.Description };
        }

        static public Attribute Create(Types id, int value)
        {
            Attribute a = GetAttribute(id).Clone() as Attribute;
            a.Value = value;
            return a;
        }

        public override string ToString()
        {
            return this.Name + ": " + this.Value;
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
                this.Value++;
                this.Progress.Value = 0;
                parent.Net.EventOccured(Message.Types.AttributeChanged, this);
            }
            else if (this.Progress.Percentage == 0)
            {
                if (this.Value == this.Min)
                    return;
                this.Value--;
                this.Progress.Value = this.Progress.Max;
                parent.Net.EventOccured(Message.Types.AttributeChanged, this);
            }
        }
    }
}
