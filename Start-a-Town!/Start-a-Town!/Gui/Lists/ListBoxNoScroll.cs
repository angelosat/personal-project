using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_.UI
{
    public class ListBoxNoScroll<TObject> : GroupBox, IListSearchable<TObject>
    {
        public int Spacing = 1;

        public TObject SelectedItem { get { return SelectedControl == null ? default : (TObject)SelectedControl.Tag; } }
        Control SelectedControl;

        public void SelectItem(TObject obj)
        {
            this.SelectedControl = this.Controls.FirstOrDefault(i => i.Tag.Equals(obj));
        }

        public List<Control> Items => this.Controls;

        readonly Func<TObject, Control> ControlFactory;

        public Action<TObject> ItemChangedFunc = item => { };
        public IEnumerable<TObject> List = new List<TObject>();
        public ListBoxNoScroll(Func<TObject, Control> controlFactory, int spacing = 1)
        {
            this.ControlFactory = controlFactory;
            this.Spacing = spacing;
        }

        public ListBoxNoScroll<TObject> Clear()
        {
            this.Controls.Clear();
            return this;
        }

        public ListBoxNoScroll<TObject> AddItems(IEnumerable<TObject> items)
        {
            foreach (var i in items)
                this.AddItem(i);
            return this;
        }
        public ListBoxNoScroll<TObject> AddItems(params TObject[] items)
        {
            foreach (var i in items)
                this.AddItem(i);
            return this;
        }
        
        Control AddItem(TObject item)
        {
            var control = this.ControlFactory(item);
            control.Tag = item;
            this.AddControlsBottomLeft(control);
            control.Location.Y += Spacing;
            return control;
        }
        public void RemoveItems(params TObject[] items)
        {
            foreach (var i in items)
                this.RemoveItem(i);
        }
        public void RemoveItems(IEnumerable<TObject> items)
        {
            foreach (var i in items)
                this.RemoveItem(i);
        }
        internal void RemoveWhere(Func<TObject, bool> filter)
        {
            this.RemoveItems(this.Items.Select(c => (TObject)c.Tag).Where(filter));
        }
        void RemoveItem(TObject item)
        {
            if (item is null)
                return;
            var listControls = this.Controls;
            var removedItemIndex = listControls.FindIndex(c => c.Tag.Equals(item));
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
            var validControls = this.Items.Where(c => filter((TObject)c.Tag)).ToArray();
            this.AddControlsBottomLeft(validControls);
        }

    }

    public class ListBoxNoScroll<TObject, TControl> : GroupBox, IListSearchable<TObject>
      where TControl : Control, new()
    {
        public int Spacing = 1;

        public TObject SelectedItem { get { return SelectedControl == null ? default : (TObject)SelectedControl.Tag; } }
        TControl SelectedControl;

        public void SelectItem(TObject obj)
        {
            this.SelectedControl = this.Controls.FirstOrDefault(i => i.Tag.Equals(obj)) as TControl;
        }

        public List<TControl> Items => this.Controls.Cast<TControl>().ToList();

        readonly Func<TObject, TControl> ControlFactory;

        public Action<TObject> ItemChangedFunc = item => { };
        public IEnumerable<TObject> List = new List<TObject>();
        public ListBoxNoScroll(Func<TObject, TControl> controlFactory, int spacing = 1)
        {
            this.ControlFactory = controlFactory;
            this.Spacing = spacing;
        }

        public ListBoxNoScroll<TObject, TControl> Clear()
        {
            this.Controls.Clear();
            return this;
        }

        public ListBoxNoScroll<TObject, TControl> AddItems(IEnumerable<TObject> items)
        {
            foreach (var i in items)
                this.AddItem(i);
            return this;
        }
        public ListBoxNoScroll<TObject, TControl> AddItems(params TObject[] items)
        {
            foreach (var i in items)
                this.AddItem(i);
            return this;
        }
        public ListBoxNoScroll<TObject, TControl> AddItem(TObject obj, Action<TObject, TControl> onControlInit)
        {
            var btn = new TControl()
            {
                Location = this.Controls.BottomLeft,
                Tag = obj,
                TooltipFunc = (tt) => { if (obj is ITooltippable) (obj as ITooltippable).GetTooltipInfo(tt); },
                Active = true
            };
            onControlInit(obj, btn);
            this.Controls.Add(btn);
            return this;
        }
        TControl AddItem(TObject item)
        {
            var control = this.ControlFactory(item);
            control.Tag = item;
            this.AddControlsBottomLeft(control);
            control.Location.Y += Spacing;
            return control;
        }
        public void RemoveItems(params TObject[] items)
        {
            foreach (var i in items)
                this.RemoveItem(i);
        }
        public void RemoveItems(IEnumerable<TObject> items)
        {
            foreach (var i in items)
                this.RemoveItem(i);
        }
        internal void RemoveWhere(Func<TObject, bool> filter)
        {
            this.RemoveItems(this.Items.Select(c => (TObject)c.Tag).Where(filter));
        }
        void RemoveItem(TObject item)
        {
            if (item is null)
                return;
            var listControls = this.Controls;
            var removedItemIndex = listControls.FindIndex(c => c.Tag.Equals(item));
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
            var validControls = this.Items.Where(c => filter((TObject)c.Tag)).ToArray();
            this.AddControlsBottomLeft(validControls);
        }

    }
}

