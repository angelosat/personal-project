﻿using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class GroupBox : Control
    {
        public GroupBox() { MouseThrough = true; AutoSize = true; }
        public GroupBox(int width, int height)
        {
            this.AutoSize = false;
            this.Size = new Rectangle(0, 0, width, height);
        }

        internal override void OnControlResized(ButtonBase buttonBase)
        {
            this.ClientSize = PreferredClientSize;
        }
        public virtual GroupBox AddControlsLineWrap(IEnumerable<ButtonBase> labels, int width = int.MaxValue)
        {
            var lastControl = this.Controls.LastOrDefault();
            var currentX = lastControl?.Right ?? 0;
            var currentY = lastControl?.Top ?? 0;
            foreach (var l in labels)
            {
                if (currentX + l.Width > width)
                {
                    currentX = 0;
                    currentY += l.Height;
                }
                l.Location = new IntVec2(currentX, currentY);
                currentX += l.Width;
                this.AddControls(l);
            }
            if (width != int.MaxValue)
                this.Width = width;
            return this;
        }

        internal void CenterControlsAlignmentVertically()
        {
            var maxh = this.Controls.Max(c => c.Height);
            foreach (var c in this.Controls)
                c.Location.Y = (maxh - c.Height) / 2;
        }
    }
}