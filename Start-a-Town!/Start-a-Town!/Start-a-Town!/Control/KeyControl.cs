﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.PlayerControl
{
    class KeyControl
    {
        static float Threshold = Engine.TargetFps / 2;
        float Timer { get; set; }
        Action PressCallback { get; set; }
        Action HoldCallBack { get; set; }
        bool Active { get; set; }
        bool Pressed { get; set; }
        public KeyControl(Action press)
        {
            this.Pressed = false;
            this.Active = false;
            this.Timer = 0;
            this.PressCallback = press;
            this.HoldCallBack = () => { };
        }
        public KeyControl(Action press, Action hold)
        {
            this.Pressed = false;
            this.Active = false;
            this.Timer = 0;
            this.PressCallback = press;
            this.HoldCallBack = hold;
        }
        public void Update()
        {
            if (!this.Active)
                return;

            this.Timer++;
            if (this.Timer >= Threshold)
            {
                HoldCallBack();
                this.Active = false;
            }
        }
        public void Down()
        {
            if (this.Pressed)
                return;
            this.Pressed = true;
            this.Active = true;
            this.Timer = 0;
        }
        public void Up()
        {
            this.Pressed = false;
            if (!this.Active)
                return;
            this.Active = false;
            PressCallback();
        }
    }
}
