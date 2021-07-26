using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class ListBoxObservable<TObject, TControl> : ScrollableBoxNew, IListSearchable<TObject>
        where TControl : Control, new()
        where TObject : class
    {
        const int Spacing = 1;

        public TObject SelectedItem => SelectedControl == null ? default : SelectedControl.Tag as TObject; 
        TControl SelectedControl;

        public void SelectItem(TObject obj)
        {
            this.SelectedControl = this.Client.Controls.FirstOrDefault(i => i.Tag == obj) as TControl;
        }

        public List<TControl> Items => this.Client.Controls.Cast<TControl>().ToList();

        public Action<TObject> ItemChangedFunc = (item) => { };
        public ObservableCollection<TObject> List;
        Func<TObject, TControl> ControlFactory;
        ListBoxObservable<TObject, TControl> Clear()
        {
            this.Client.Controls.Clear();
            return this;
        }
        public ListBoxObservable(int width, int height, Func<TObject, TControl> controlFactory, ScrollModes mode = ScrollModes.Vertical)
            : base(new Rectangle(0, 0, width, height), mode)
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
        [Obsolete]
        public ListBoxObservable<TObject, TControl> Build(Dictionary<string, List<TObject>> list, Func<TObject, string> nameFunc, Action<string, TControl> onCategoryInit, Action<TObject, TControl> onControlInit)
        {
            this.Client.Controls.Clear();
            foreach (var obj in list)
            {
                var inner = new GroupBox();
                foreach (var item in obj.Value)
                {
                    TControl btninner = new TControl();
                    btninner.Tag = item;
                    btninner.TooltipFunc = (tt) => { if (item is ITooltippable) (item as ITooltippable).GetTooltipInfo(tt); };
                    btninner.Active = true;
                    btninner.Location = inner.Controls.BottomLeft;
                    inner.Controls.Add(btninner);
                }

                TControl btn = new TControl()
                {
                    Location = Client.Controls.Count > 0 ? Client.Controls.Last().BottomLeft : Vector2.Zero,
                    Tag = inner,
                    Name = obj.Key,
                    TooltipFunc = (tt) => { if (obj is ITooltippable) (obj as ITooltippable).GetTooltipInfo(tt); },
                    Active = true
                };

                this.Client.Controls.Add(btn);
            }
            Remeasure();
            Client.ClientLocation = Vector2.Zero;
            SelectedControl = null;
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
            this.Client.AddControlsBottomLeft(control);
            control.Location.Y += Spacing;
            this.UpdateClientSize();

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
            var listControls = this.Client.Controls;
            var removedItemIndex = listControls.FindIndex(c => c.Tag == item);
            var prevY = listControls[removedItemIndex].Location.Y;
            for (int i = removedItemIndex + 1; i < listControls.Count; i++)
            {
                var r = listControls[i];
                r.Location.Y = prevY ;
                prevY = r.Bottom + Spacing;
            }
            listControls.RemoveAt(removedItemIndex);
            this.UpdateClientSize();
        }

        public override void Remeasure()
        {
            base.Remeasure();
            foreach (var btn in Client.Controls)
            {
                btn.Width = Math.Max(btn.Width, Client.Width);
            }
        }
       
        public void Filter(Func<TObject, bool> filter)
        {
            this.Client.Controls.Clear();
            var validControls = this.Items.Where(c => filter(c.Tag as TObject)).ToArray();
            this.Client.AddControlsBottomLeft(validControls);
            this.UpdateClientSize();
        }
    }
}

