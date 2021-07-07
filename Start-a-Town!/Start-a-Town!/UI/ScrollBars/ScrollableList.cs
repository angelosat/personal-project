using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class ListItemEventArgs<T> : EventArgs
    {
        public T Item;
        public ListItemEventArgs(T item)
        {
            this.Item = item;
        }
    }
}
