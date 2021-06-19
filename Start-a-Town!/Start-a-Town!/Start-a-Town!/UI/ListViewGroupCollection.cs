using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.UI
{
    public class ListViewGroupCollection
    {
        List<ListViewGroup> Collection;
        public ListViewGroupCollection()
        {
            Collection = new List<ListViewGroup>();
        }

        public void Add(ListViewGroup item)
        {
            Collection.Add(item);
        }

        public List<ListViewGroup>.Enumerator GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        public void Clear()
        {
            Collection.Clear();
        }
    }
}
