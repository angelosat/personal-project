using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    public class ContainerList : IList<GameObject>, ISerializable, ISaveable
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
                        .AddColumn("name", 96, o => new Label(() => o.Name, () => SelectionManager.Select(o)) { TooltipFunc = o.GetInventoryTooltip })
                        .AddColumn("weight", 32, o => new Label(() => o.TotalWeight.ToString("0.# kg")))
                        .AddColumn("drop", Icon.Cross.Width, o => IconButton.CreateSmall(Icon.Cross, delegate { }, "Drop"));
                    var box = new ScrollableBoxNewNew(256, 256).AddControls(_gui);
                }
                return _gui.Bind(this.Contents).Parent;
            }
        }
        readonly ObservableCollection<GameObject> Contents = new();
        public GameObject Parent;

        public int Count => ((ICollection<GameObject>)this.Contents).Count;

        public bool IsReadOnly => ((ICollection<GameObject>)this.Contents).IsReadOnly;

        public GameObject this[int index] { get => ((IList<GameObject>)this.Contents)[index]; set => ((IList<GameObject>)this.Contents)[index] = value; }

        public int IndexOf(GameObject item)
        {
            return ((IList<GameObject>)this.Contents).IndexOf(item);
        }

        public void Insert(int index, GameObject item)
        {
            if (item.Container == this)
                throw new Exception();
            ((IList<GameObject>)this.Contents).Insert(index, item);
            item.Container?.Remove(item);
            item.Container = this;
        }

        public void RemoveAt(int index)
        {
            var item = this[index];
            if (item.Container != this)
                throw new Exception();
            item.Container = null;
            ((IList<GameObject>)this.Contents).RemoveAt(index);
        }

        public void Add(GameObject item)
        {
            if (item.Container == this)
                throw new Exception();

            if(this.Contents.FirstOrDefault(i=>i.CanAbsorb(item)) is GameObject existing)
            {
                existing.StackSize += item.StackSize;
                item.Dispose();
                return;
            }

            ((ICollection<GameObject>)this.Contents).Add(item);
            item.Container?.Remove(item);
            item.Slot?.Clear();
            item.Container = this;
        }

        public void Clear()
        {
            foreach (var i in this.Contents)
                i.Container = null;
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
            if (item.Container != this)
                throw new Exception();
            item.Container = null;
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

        public SaveTag Save(string name = "")
        {
            var save = new SaveTag(SaveTag.Types.Compound, name);
            var listtag = new SaveTag(SaveTag.Types.List, "Contents", SaveTag.Types.Compound);
            foreach (var i in this.Contents)
                listtag.Add(i.Save());
            save.Add(listtag);
            return save;
        }

        public ISaveable Load(SaveTag tag)
        {
            var itemList = tag["Contents"].Value as List<SaveTag>;
            foreach (var itemTag in itemList)
                this.Add(GameObject.Load(itemTag));
            return this;
        }
    }
}
