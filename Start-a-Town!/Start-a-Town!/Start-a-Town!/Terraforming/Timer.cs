using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class Timer
    {
        float t;
        public event EventHandler<EventArgs> TimerFinished;
        protected void OnTimerFinished()
        {
            if (TimerFinished != null)
                TimerFinished(this, EventArgs.Empty);
        }

        public Timer(float tstart)
        {
            t = tstart;
        }

        public void Update()
        {
            t -= UIFpsCounter.deltaTime;
            if (t <= 0)
            {
                OnTimerFinished();
            }
        }
    }
}
