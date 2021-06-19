﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class PanelLabeledNew : Panel
    {
        public Label Label;
        public GroupBox Client;
        public PanelLabeledNew(string label)
        {
            this.Name = label;
            this.AutoSize = true;
            this.Label = new Label(label) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            this.Client = new GroupBox() { AutoSize = true, Location = this.Label.BottomLeft };
            this.AddControls(this.Label, this.Client);
        }
        public PanelLabeledNew(Func<string> label)
        {
            this.AutoSize = true;
            this.Label = new Label(label) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            this.Client = new GroupBox() { AutoSize = true, Location = this.Label.BottomLeft };
            this.AddControls(this.Label, this.Client);
        }
        public PanelLabeledNew(string label, int width, int height)
            : base(Vector2.Zero, new Vector2(width, height))
        {
            this.Label = new Label(label) { TextColorFunc = () => Color.Goldenrod, Font = UIManager.FontBold };
            this.Client = new GroupBox() { AutoSize = true, Location = this.Label.BottomLeft };
            this.AddControls(this.Label, this.Client);
        }
        protected PanelLabeledNew() { }
    }
}
