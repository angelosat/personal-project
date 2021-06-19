using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class FpsCounter : Label
    {
        public FpsCounter()
            : base("F")//ps: ###")
        {
         //   this.Width = (int)UIManager.Font.MeasureString("Fps: ###").X;
         //   this.Height = Label.DefaultHeight;
        }

        public override void Update()
        {
            this.Text = "Fps: " + GlobalVars.Fps.ToString();
           // this.Location = new Vector2(UIManager.Width / 2 - this.Width / 2, 0);
            base.Update();
        }
    }
}
