using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.UI
{
    public class ScrollEventArgs : EventArgs
    {
        public int NewValue;
        public int OldValue;
        public int Type;

        public ScrollEventArgs(int newvalue, int oldvalue, int type)
        {
            NewValue = newvalue;
            OldValue = oldvalue;
            Type = type;
        }
    }
}
