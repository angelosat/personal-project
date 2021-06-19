using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.UI
{
    class TimerControl : Control
    {
        float Interval = 1, Value = 0;
        public event EventHandler Tick;
        void OnTick()
        {
            if (Tick != null)
                Tick(this, EventArgs.Empty);
        }
        public bool Enabled;

        public void Update()
        {
            Value += Global.DeltaTime;
            if (Value >= Interval)
            {
                OnTick();
                Value -= Interval;
            }
        }
    }
}
