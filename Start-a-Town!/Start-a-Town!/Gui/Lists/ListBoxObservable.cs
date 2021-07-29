using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace Start_a_Town_.UI
{
    public class ListBoxObservable<TObject, TControl> : GroupBox, IListSearchable<TObject>
        where TControl : Control, new()
        where TObject : class
    {
        const int Spacing = 1;
        static readonly Func<TObject, bool> DefaultFilter = i => true;
        Func<TObject, bool> CurrentFilter = DefaultFilter;
        public TObject SelectedItem => SelectedControl == null ? default : SelectedControl.Tag as TObject;
        TControl SelectedControl;

        public void SelectItem(TObject obj)
        {
            this.SelectedControl = this.Controls.FirstOrDefault(i => i.Tag == obj) as TControl;
        }

        /// <summary>
        /// A list containing all item controls, not just the currently displayed ones. Used for filtering.
        /// </summary>
        List<TControl> Items = new();// => this.Controls.Cast<TControl>().ToList();

        public Action<TObject> ItemChangedFunc = (item) => { };
        public ObservableCollection<TObject> List;
        Func<TObject, TControl> ControlFactory;
        ListBoxObservable<TObject, TControl> Clear()
        {
            this.Controls.Clear();
            return this;
        }
        public ListBoxObservable(Func<TObject, TControl> controlFactory)
        {
            this.ControlFactory = controlFactory;
        }
        public ListBoxObservable<TObject, TControl> Bind(ObservableCollection<TObject> collection)
        {
            if (collection == this.List)
                return this;
            if (this.List != null)
                this.List.CollectionChanged -= List_CollectionChanged;
            this.List = collection;
            this.List.CollectionChanged += List_CollectionChanged;
            this.Clear();
            this.AddItems(collection);
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

        TControl AddItem(TObject item)
        {
            var control = this.ControlFactory(item);
            control.Tag = item;
            this.Items.Add(control);
            if (this.Controls.Any())
            {
                this.AddControlsBottomLeft(control);
                control.Location.Y += Spacing;
            }
            else
                this.AddControls(control);
            return control;
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
            this.Items.Remove(this.Items.First(c => c.Tag == item));
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
            this.CurrentFilter = filter ?? DefaultFilter;
            this.Controls.Clear();
            var validControls = this.Items.Where(c => this.CurrentFilter(c.Tag as TObject)).ToArray();
            this.AddControlsVertically(Spacing, validControls);
        }
    }
}

