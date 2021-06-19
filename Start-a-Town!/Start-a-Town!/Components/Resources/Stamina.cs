using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Resources
{
    class Stamina : ResourceDef
    {
        public Stamina():base("Stamina")
        {
            //this.Name = "Stamina";
        }
        public override string Format
        {
            get
            {
                return "##0.00";
            }
        }

        public override ResourceDef.ResourceTypes ID
        {
            get { return ResourceDef.ResourceTypes.Stamina; }
        }

        public override void Add(float add, Resource resource)
        {
            //if (add < 0)
            //    this.Rec.Value = this.Rec.Max;
            if (add < 0)
                resource.Rec.Value = 0;// resource.Rec.Max;
            base.Add(add, resource);
        }

        public override string Description
        {
            get { return "Required for sprinting and hauling heavy objects"; }
        }
        //public override string GetLabel(Resource values)
        //{
        //    //var t  = this.GetThreshold(values);
        //    //return t.Name;
        //    var val = values.Percentage;
        //    var thresh1 = .75f;
        //    var thresh2 = .5f;
        //    var thresh3 = .25f;

        //    if (val < thresh3)
        //        return "Out of breath";
        //    else if (val < thresh2)
        //        return "Exhausted";
        //    else if (val < thresh1)
        //        return "Tired";
        //    else
        //        return "Energetic";
        //}
        public float TickRate = Engine.TicksPerSecond / 2f; // 2 ticks a second
        public float Timer = 0;
        public float RegenerationRate = 1;

        public override bool HandleMessage(Resource resource, GameObject parent, ObjectEventArgs e = null)
        {
            switch (e.Type)
            {
                case Message.Types.Jumped:
                    resource.Rec.Value = resource.Rec.Max;
                    return true;

                default:
                    return base.HandleMessage(resource, parent, e);
            }
        }

        public override void Tick(GameObject parent, Resource values)
        {
            //if (values.Rec.Value > 0)
            //{
            //    values.Rec.Value--;
            //    return;
            //}
            base.Tick(parent, values);
            if (values.Rec.Value < values.Rec.Max)
            {
                values.Rec.Value++;
                return;
            }
            this.Add(this.GetRegenRate(values), values);
            
            //if (this.Rec.Value > 0)
            //{
            //    this.Rec.Value--;
            //    return;
            //}

            //this.Add(this.GetRate());
        }
        float GetRegenRate(Resource values)
        {
            //float missing = this.Max - this.Value;
            float rate = (1 + (float)Math.Pow(values.Percentage, 2)) / TickRate;
            //float rate = 1f + (float)Math.Pow(this.Percentage, 2);
            return rate;
        }

        public override Color GetBarColor(Resource resource)
        {
            return Color.Yellow;
        }
        public override Control GetControl(Resource res)
        {
            var box = new GroupBox();
            //var bar = new Bar() { Color = Color.Yellow, Object = res, TextFunc = () => this.GetLabel(res) };
            var bar = base.GetControl(res);
            var bar_StaminaRec = new Bar() { Object = res.Rec, Location = bar.BottomLeft, Height = 2 };
            box.AddControls(bar, bar_StaminaRec);
            return box;
        }
    }
}
