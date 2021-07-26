using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Start_a_Town_.UI
{
    public class TableObservable<TObject> : GroupBox, IListSearchable<TObject>
        where TObject : class
    {
        const int Spacing = 1;
        readonly List<Column> Columns = new();
        public Action<TObject> ItemChangedFunc = (item) => { };
        public ObservableCollection<TObject> List;

        TableObservable<TObject> Clear()
        {
            this.Controls.Clear();
            return this;
        }

        public TableObservable<TObject> Bind(ObservableCollection<TObject> collection)
        {
            if (collection == this.List)
                return this;
            if (this.List != null)
                this.List.CollectionChanged -= this.List_CollectionChanged;
            this.List = collection;
            this.List.CollectionChanged += this.List_CollectionChanged;
            this.Clear();
            this.AddItems(collection);
            return this;
        }

        public TableObservable<TObject> AddColumn(string label, int width, Func<TObject, Control> controlGetter)
        {
            this.Columns.Add(new(label, width, controlGetter));
            return this;
        }

        private void List_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.AddItems(e.NewItems?.Cast<TObject>());
            this.RemoveItems(e.OldItems?.Cast<TObject>());
        }

        void AddItems(IEnumerable<TObject> items)
        {
            if (items == null)
                return;
            foreach (var i in items)
                this.AddItem(i);
        }

        Control AddItem(TObject item)
        {
            var row = new GroupBox();
            int currentX = 0;
            foreach (var col in this.Columns)
            {
                row.AddControls(col.ControlGetter(item).SetLocation(currentX, 0)
                                                       .SetTag(item));
                currentX = col.Width;
            }
            this.AddControlsBottomLeft(row);
            row.Location.Y += Spacing;
            return row;
        }
        void RemoveItems(IEnumerable<TObject> items)
        {
            if (items is null)
                return;
            foreach (var i in items)
                this.RemoveItem(i);
        }
        void RemoveItem(TObject item)
        {
            if (item is null)
                return;
            var listControls = this.Controls;
            var removedItemIndex = listControls.FindIndex(c => c.Tag == item);
            var prevY = listControls[removedItemIndex].Location.Y;
            for (int i = removedItemIndex + 1; i < listControls.Count; i++)
            {
                var r = listControls[i];
                r.Location.Y = prevY;
                prevY = r.Bottom + Spacing;
            }
            listControls.RemoveAt(removedItemIndex);
        }

        public void Filter(Func<TObject, bool> filter)
        {
            this.Controls.Clear();
            var validControls = this.Controls.Where(c => filter(c.Tag as TObject)).ToArray();
            this.AddControlsBottomLeft(validControls);
        }

        class Column
        {
            public string Label;
            public int Width;
            public Func<TObject, Control> ControlGetter;
            public Column(string label, int width, Func<TObject, Control> controlGetter)
            {
                this.Label = label;
                this.Width = width;
                this.ControlGetter = controlGetter;
            }
        }
    }
}

