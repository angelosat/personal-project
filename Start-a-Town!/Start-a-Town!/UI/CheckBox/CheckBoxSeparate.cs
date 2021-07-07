using System;

namespace Start_a_Town_.UI
{
    public class CheckBoxSeparate : GroupBox, ILabelable
    {
        public CheckBoxIcon TickBox;
        public Label Label;
        public Func<string> TextFunc { get => this.Label.TextFunc; set => this.Label.TextFunc = value; }
    }
}
