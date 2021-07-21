using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class ListBoxNewNoBtnBase<TObject, TControl> : ScrollableBoxNew, IListSearchable<TObject>
        where TControl : Control, new()
        where TObject : class
    {
        public const int Spacing = 1;

        public TObject SelectedItem { get { return SelectedControl == null ? default : SelectedControl.Tag as TObject; } }
        TControl SelectedControl;

        public void SelectItem(TObject obj)
        {
            this.SelectedControl = this.Client.Controls.FirstOrDefault(i => i.Tag == obj) as TControl;
        }

        public List<TControl> Items => this.Client.Controls.Cast<TControl>().ToList();
           
        readonly Func<TObject, TControl> ControlFactory;

        public Action<TObject> ItemChangedFunc = item => { };
        public IEnumerable<TObject> List = new List<TObject>();

        public ListBoxNewNoBtnBase(int width, int height, ScrollModes mode = ScrollModes.Vertical) : base(new Rectangle(0,0,width,height), mode) 
        {
        }
        
        public ListBoxNewNoBtnBase(int width, int height, Func<TObject, TControl> controlFactory, ScrollModes mode = ScrollModes.Vertical) : this(width, height, mode)
        {
            this.ControlFactory = controlFactory;
        }

        public ListBoxNewNoBtnBase<TObject, TControl> Clear()
        {
            this.Client.Controls.Clear();
            return this;
        }

        public void AddItems(IEnumerable<TObject> items)
        {
            foreach (var i in items)
                this.AddItem(i);
            if (this.Client.Controls.LastOrDefault() is Control last)
                this.EnsureLocationVisible(last.Bottom);
        }
        public void AddItem(TObject obj, Action<TObject, TControl> onControlInit)
        {
            var btn = new TControl()
            {
                Location = Client.Controls.BottomLeft,
                Width = Client.Width,
                Tag = obj,
                TooltipFunc = (tt) => { if (obj is ITooltippable) (obj as ITooltippable).GetTooltipInfo(tt); },
                Active = true
            };
            onControlInit(obj, btn);
            this.Client.Controls.Add(btn);
        }
        internal TControl AddItem(TObject item)
        {
            var control = this.ControlFactory(item);
            this.Client.AddControlsBottomLeft(control);
            control.Location.Y += Spacing;
            this.UpdateClientSize();
            return control;
        }

        public void RemoveItem(TObject item)
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

