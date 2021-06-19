using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_
{
    public class DragDropArgs : EventArgs
    {
        public Object Data;
        public DragDropArgs(Object data)
        {
            Data = data;
        }
    }
}
