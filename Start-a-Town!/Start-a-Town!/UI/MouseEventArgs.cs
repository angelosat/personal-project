using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace Start_a_Town_.UI
{
    public class MouseEventArgs : System.Windows.Forms.MouseEventArgs
    {
        public bool Handled { get; set; }
        public MouseEventArgs(System.Windows.Forms.MouseButtons buttons, int clicks, int x, int y, int delta)
            : base(buttons, clicks, x, y, delta)
        {
            this.Handled = false;
        }
    }
}
