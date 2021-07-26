using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class ContainerList : IList<GameObject>, ISerializable
    {
        //static ListBoxObservableNew<GameObject, GroupBox> _gui;
        //public Control Gui
        //{
        //    get
        //    {
        //        if (_gui is null)
        //        {
        //            _gui = new ListBoxObservableNew<GameObject, GroupBox>(o =>
        //               new GroupBox().AddControls(
        //                   new Label(() => o.Name),
        //                   IconButton.CreateSmall(Icon.Cross, delegate { }).SetLocation(new(128, 0))) as GroupBox);
        //            var box = new ScrollableBoxNewNew(256, 256).AddControls(_gui);
        //        }
        //        return _gui.Bind(this.Contents).Parent;
        //    }
        //}
        static TableObservable<GameObject> _gui;
        public Control Gui
        {
            get
            {
                if (_gui is null)
                {
                    _gui = new TableObservable<GameObject>()
                        .AddColumn("name", 128, o => new Label(() => o.Name, () => SelectionManager.Select(o)))
                        .AddColumn("drop", Icon.Cross.Width, o => IconButton.CreateSmall(Icon.Cross, delegate { }));
                    var box = new ScrollableBoxNewNew(256, 256).AddControls(_gui);
                }
                return _gui.Bind(this.Contents).Parent;
            }
        }
        readonly ObservableCollection<GameObject> Contents = new();

        public int Count => ((ICollection<GameObject>)this.Contents).Count;

        public bool IsReadOnly => ((ICollection<GameObject>)this.Contents).IsReadOnly;

        public GameObject this[int index] { get => ((IList<GameObject>)this.Contents)[index]; set => ((IList<GameObject>)this.Contents)[index] = value; }

        public int IndexOf(GameObject item)
        {
            return ((IList<GameObject>)this.Contents).IndexOf(item);
        }

        public void Insert(int index, GameObject item)
        {
            ((IList<GameObject>)this.Contents).Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            ((IList<GameObject>)this.Contents).RemoveAt(index);
        }

        public void Add(GameObject item)
        {
            ((ICollection<GameObject>)this.Contents).Add(item);
        }

        public void Clear()
        {
            ((ICollection<GameObject>)this.Contents).Clear();
        }

        public bool Contains(GameObject item)
        {
            return ((ICollection<GameObject>)this.Contents).Contains(item);
        }

        public void CopyTo(GameObject[] array, int arrayIndex)
        {
            ((ICollection<GameObject>)this.Contents).CopyTo(array, arrayIndex);
        }

        public bool Remove(GameObject item)
        {
            return ((ICollection<GameObject>)this.Contents).Remove(item);
        }

        public IEnumerator<GameObject> GetEnumerator()
        {
            return ((IEnumerable<GameObject>)this.Contents).GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((IEnumerable)this.Contents).GetEnumerator();
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Contents.Count);
            foreach (var o in this.Contents)
                o.Write(w);
        }

        public ISerializable Read(BinaryReader r)
        {
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                this.Contents.Add(GameObject.Create(r));
            return this;
        }

        internal void Instantiate(Action<GameObject> instantiator)
        {
            foreach (var o in this.Contents)
                instantiator(o);
        }
    }
}
