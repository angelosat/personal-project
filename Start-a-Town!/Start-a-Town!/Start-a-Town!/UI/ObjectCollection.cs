using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;

namespace Start_a_Town_.UI
{
    public class ObjectCollection
    {
        Control Owner;
        List<Object> Collection;

        public Object this[int i] { get { return Collection[i]; } }

        public ObjectCollection(Control owner)
        {
            Owner = owner;
            Collection = new List<object>();
        }

        public ObjectCollection(Control owner, List<object> collection)
        {
            Owner = owner;
            Collection = collection;
        }

        public void Clear()
        {
            Collection.Clear();
        }

        public void Add(object obj)
        {
            Collection.Add(obj);
        }

        public List<object>.Enumerator GetEnumerator()
        {
            return Collection.GetEnumerator();
        }

        public int IndexOf(Object item)
        {
            return Collection.IndexOf(item);
        }

        public int Count
        { get { return Collection.Count; } }
    }
}
