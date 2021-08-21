using Start_a_Town_.UI;
using System;

namespace Start_a_Town_.Terraforming.Mutators
{
    public class MutatorProperty
    {
        public string Name;
        public float Min;
        public float Max;
        float _Value;
        public float Value
        {
            get { return _Value; }
            set
            {
                _Value = Math.Max(0, Math.Min(MapBase.MaxHeight, value));
            }
        }
        public float Step { get; set; }
        public MutatorProperty(string name, float value, float min, float max, float step = 1)
        {
            if (step <= 0)
                throw new ArgumentException();
            this.Name = name;
            this.Min = min;
            this.Max = max;
            this.Value = value;
            this.Step = step;
        }
        public Control GetGui()
        {
            return SliderNew.CreateWithLabelNew(this.Name, () => this.Value, v => this.Value = v, 100, this.Min, this.Max, this.Step, "##0%");
        }
    }
}
