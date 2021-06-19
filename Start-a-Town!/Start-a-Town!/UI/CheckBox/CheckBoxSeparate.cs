using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class CheckBoxSeparate : GroupBox, ILabelable
    {
        public CheckBoxIcon TickBox;
        public Label Label;
        public CheckBoxSeparate()
        {
            this.TickBox = new CheckBoxIcon();
            this.Label = new Label() { LocationFunc = () => this.TickBox.TopRight };
        }
        public CheckBoxSeparate(string label):this()
        {
            //this.TickBox = new CheckBoxIcon();
            //this.Label = new Label(label) { LocationFunc = () => this.TickBox.TopRight };
            this.Label.Text = label;
            this.AddControls(this.TickBox, this.Label);
        }

        public Func<string> TextFunc { get => this.Label.TextFunc; set => this.Label.TextFunc = value; }
    }
}
