using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_
{
    public class TimerArgs
    {
        public float Min, Max, StepValue, Value;
        public TimerArgs(float min, float max, float stepvalue, float value)
        {
            Min = min;
            Max = max;
            StepValue = stepvalue;
            Value = value;
        }
    }
}
