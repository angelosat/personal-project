using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Resources
{
    class Stamina : Resource
    {
        //public class Recovery : IProgressBar
        //{
        //    public float Min { get; set; }
        //    public float Max { get; set; }
        //    public float Value { get; set; }
        //    public float Percentage { get { return 1 - this.Value / this.Max; } }
        //    public Recovery()
        //    {
        //        this.Min = 0;
        //        this.Max = this.Value = Engine.TargetFps;
        //    }
        //}
        

        public override string ComponentName
        {
            get { return "Stamina"; }
        }

        public override string Name
        {
            get { return "Stamina"; }
        }

        public override string Format
        {
            get
            {
                return "##0.00";
            }
        }

        public override Resource.Types ID
        {
            get { return Resource.Types.Stamina; }
        }

        public override void Add(float add)
        {
            if (add < 0)
                //this.RecoverTimer = this.RecoverLength;
                this.Rec.Value = this.Rec.Max;
            base.Add(add);
        }

        public override string Description
        {
            get { return "Required for sprinting"; }
        }

        public float Tick = Engine.TargetFps / 2f; // 2 ticks a second
        public float Timer = 0;
        public float RegenerationRate = 1;
        //public float RecoverLength = Engine.TargetFps;
        //public float RecoverTimer = 0;
        public Recovery Rec = new Recovery();

        public override bool HandleMessage(GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Jumped:
                    //this.Add(-5);
                    this.Rec.Value = this.Rec.Max;
                    return true;

                default:
                    return base.HandleMessage(parent, e);
            }
        }

        public override void Update(Net.IObjectProvider net, GameObject parent, Chunk chunk = null)
        {
            //if (this.RecoverTimer > 0)
            //{
            //    this.RecoverTimer--;
            //    return;
            //}
            if (this.Rec.Value > 0)
            {
                this.Rec.Value--;
                return;
            }

            //this.Timer--;
            //if (this.Timer > 0)
            //    return;
            //this.Timer = this.Tick;

            // this.Value += GetRate();// RegenerationRate;
            this.Add(this.GetRate());
        }
        float GetRate()
        {
            //float missing = this.Max - this.Value;
            float rate = (1 + (float)Math.Pow(this.Percentage, 2)) / Tick;
            //float rate = 1f + (float)Math.Pow(this.Percentage, 2);
            return rate;
        }

        public override object Clone()
        {
            return new Stamina();
        }

        public override Control GetControl()
        {
            var box = new GroupBox();
            var bar = new Bar() { Color = Color.Yellow , Object = this};
            var bar_StaminaRec = new Bar() { Object = this.Rec, Location = bar.BottomLeft, Height = 2 };
            box.AddControls(bar, bar_StaminaRec);
            return box;
        }
    }
}
