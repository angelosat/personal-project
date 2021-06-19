using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;

namespace Start_a_Town_.UI
{
    class ToolstripItemCollection
    {
        Control Owner;
        List<ToolstripItem> Collection;

        public Object this[int i] { get { return Collection[i]; } }

        public ToolstripItemCollection(Control owner)
        {
            Owner = owner;
            Collection = new List<ToolstripItem>();
        }

        public ToolstripItemCollection(Control owner, List<ToolstripItem> collection)
        {
            Owner = owner;
            Collection = collection;
        }

        public void Add(ToolstripItem item)
        {
            Collection.Add(item);
        }

        public List<ToolstripItem>.Enumerator GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        public int IndexOf(ToolstripItem item)
        {
            return Collection.IndexOf(item);
        }
    }
}
