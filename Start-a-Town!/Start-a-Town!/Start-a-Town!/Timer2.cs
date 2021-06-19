﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class Timer2
    {
        public event EventHandler<EventArgs> Tick;

        public float Interval;// = 1;
        protected float _Value = 0;
        //protected float Value = 0;

        public Timer2(float interval)
        {
            this.Interval = interval;
        }

        public float Value
        {
            get { return _Value; }
            protected set { _Value = value; }
        }

        public float Percentage
        {
            get { return Value / (float)Interval; }
        }


        public void Update(GameTime gt)
        {
            Value -= 1;//GlobalVars.DeltaTime;
            if (Value <= 0)//>= Interval)
            {
                OnTick();
                //Value -= Interval;
                Value = Interval;
            }
        }

        void OnTick()
        {
            if (Tick != null)
                Tick(this, EventArgs.Empty);
        }

        public void Start()
        {
            ScreenManager.CurrentScreen.Timers.Add(this);
            this.Value = Interval;
        }

        public void Stop()
        {
            ScreenManager.CurrentScreen.Timers.Remove(this);
        }

        public void Restart()
        {
            Stop();
            Start();
        }
    }
}
