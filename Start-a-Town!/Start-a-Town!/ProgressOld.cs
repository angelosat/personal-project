using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    [Obsolete]
    public class ProgressOld
    {
        public event EventHandler<EventArgs> ValueChanged;

        public string Name { get; protected set; }
        public float Percentage
        {
            get
            {
                return (Value - Min) / (float)(Max - Min);
            }
            set
            {
                Value = Min + value * (Max - Min);
            }
        }
        public float Min { get; set; }
        public float Max { get; set; }
        public float StepValue { get; set; }
        public float Range { get; set; }
        float _Value;
        public float Value
        {
            get { return _Value; }
            set
            {
                float valueold = _Value;
                //_Value = Math.Min(Math.Max(Min, value), Max);
                _Value = value;
                if (valueold != _Value)
                    OnValueChanged();
            }
        }
        void OnValueChanged()
        {
            if (ValueChanged != null)
                ValueChanged(this, EventArgs.Empty);
        }

        public ProgressOld(string label, float min, float max, float value)
        {
            Name = label;
            Min = min;
            Max = max;
            Value = value;
        }

        public ProgressOld(float min, float max, float value)
        {
            Min = min;
            Max = max;
            Value = value;
            Range = Max - Min;
        }

        public bool Finished
        { get { return Value >= Max; } }

        //Bar Bar;
        //public void Show()
        //{
        //    //TOTO
        //    //disconnect this from here
        //    Bar = new Bar(new Vector2(200));
        //    Bar.Track(this);
        //    Bar.Show();
        //}

        //public void Hide()
        //{
        //    Bar.Stop();
        //    Bar.Hide();
        //}

        //public bool Update()
        //{
        //    Value += Start_a_Town_.UI.UIFpsCounter.deltaTime;
        //}

    }
}
