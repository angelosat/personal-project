using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_
{
    public class Timer3 : ProgressOld
    {
        public event EventHandler<EventArgs> Tick;

        protected bool AlarmOff;
        protected float _Min, _Max, _StepValue, _Value;
        //public float Min
        //{
        //    get { return _Min; }
        //    set { _Min = value; }
        //}
        //public float Max
        //{
        //    get { return _Max; }
        //    set { _Max = value; }
        //}
        //public float Percentage
        //{
        //    get { return Value / (Max - Min); }
        //    set { Value = Min + value * (Max - Min); }
        //}
        //public float StepValue
        //{
        //    get { return _StepValue; }
        //    set { _StepValue = value; }
        //}
        //public float Value
        //{
        //    get { return _Value; }
        //    set
        //    {
        //        _Value = value;
        //        if (_Value > Max)
        //            if (!AlarmOff)
        //                OnAlarm();
        //    }
        //}

        public void Update()
        {
            Value += 1;//GlobalVars.DeltaTime;
            if (Value >= Max)
                OnTick();
        }

        void OnTick()
        {
            AlarmOff = true;
            if (Tick != null)
                Tick(this, EventArgs.Empty);
        }

        public void AlarmReset()
        {
            AlarmOff = false;
        }
        public void Reset()
        {
            //Value = Min;
            Value -= Range;
            //Console.WriteLine("alarm reset: " + Value.ToString());
            AlarmReset();
        }

        public Timer3(TimerArgs a)
            : base(a.Min, a.Max, a.Value)
        {
            //Min = a.Min;
            //Max = a.Max;
            StepValue = a.StepValue;
            //Value = a.Value;
        }
    }
}
