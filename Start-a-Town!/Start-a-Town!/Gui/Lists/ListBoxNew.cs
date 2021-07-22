using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class ListBoxNew<TObject, TControl> : ScrollableBoxNew, IListSearchable<TObject>
        where TControl : ButtonBase, new()
        where TObject : class
    {
        public Func<TObject, string> ItemNameFunc;
        public Action<TObject, TControl> OnControlInit;
        Func<TObject, TControl> ControlGetter;
        public TObject SelectedItem { get { return SelectedControl == null ? default : SelectedControl.Tag as TObject; } }
        TControl SelectedControl;
        Func<TObject, bool> CurrentFilter = i => true;

        public List<TControl> Items = new List<TControl>();
        public event EventHandler<EventArgs> SelectedItemChanged;

        public Action<TObject> ItemChangedFunc = (item) => { };
        public IEnumerable<TObject> List = new List<TObject>();
        public ListBoxNew()
            : this(0, 0) 
        {
            this.AutoSize = true;
        }
        public ListBoxNew(int width, int height, ScrollModes mode = ScrollModes.Vertical) : base(new Rectangle(0,0,width,height), mode) 
        {
        }
        public ListBoxNew(int width, int height, Func<TObject, TControl> controlGetter, ScrollModes mode = ScrollModes.Vertical) : this(width, height, mode)
        {
            this.ControlGetter = controlGetter;
        }
        public ListBoxNew(int width, int height, Func<TObject, ListBoxNew<TObject,TControl>, TControl> controlGetter, ScrollModes mode = ScrollModes.Vertical) : this(width, height, mode)
        {
            this.ControlGetter = i => controlGetter(i, this);
        }
        public ListBoxNew(Rectangle bounds) : base(bounds)
        {
        }
        public ListBoxNew<TObject, TControl> Clear()
        {
            this.Client.Controls.Clear();
            this.Items.Clear();
            return this;
        }
        public ListBoxNew<TObject, TControl> Build(IEnumerable<TObject> list, Func<TObject, string> nameFunc, Func<TObject, string> hoverTextFunc = null)
        {
            this.List = list;
            Func<TObject, string> htv = hoverTextFunc ?? (foo => "");
            this.Client.Controls.Clear();
            foreach (var obj in list)
            {
                var btn = new TControl()
                {
                    Location = Client.Controls.Count > 0 ? Client.Controls.Last().BottomLeft : Vector2.Zero,
                    Tag = obj,
                    Text = nameFunc(obj),
                    Name = nameFunc(obj),
                    HoverText = htv(obj),
                    Active = true
                };
                var action = btn.LeftClickAction;
                btn.LeftClickAction = () =>
                {
                    action();
                    Btn_Click(btn);
                };
                this.Client.Controls.Add(btn);
            }
            Remeasure();
            Client.ClientLocation = Vector2.Zero;
            SelectedControl = null;
            return this;
        }
        public ListBoxNew<TObject, TControl> Build(IEnumerable<TObject> list, Func<TObject, string> nameFunc, Action<TObject, TControl> onControlInit)
        {
            foreach (var item in list)
            {
                this.AddItem(item, nameFunc, onControlInit);
            }
            return this;
        }
       
        
        /// <summary>
        /// obsolete, use AddItems instead
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Obsolete]
        public ListBoxNew<TObject, TControl> AddItem(TObject obj)
        {
            this.AddItem(obj, this.ItemNameFunc, this.OnControlInit);
            return this;
        }
        [Obsolete]
        public ListBoxNew<TObject, TControl> AddItem(TObject obj, Func<TObject, string> nameFunc, Action<TObject, TControl> onControlInit)
        {
            if (obj == null)
                return this;
            TControl btn = new TControl()
            {
                AutoSize = false, // latest addition
                Location = Client.Controls.BottomLeft,
                Tag = obj,
                Text = nameFunc(obj),
                Name = nameFunc(obj),
                TooltipFunc = (tt) => { if (obj is ITooltippable) (obj as ITooltippable).GetTooltipInfo(tt); },
                Width = Client.Width,
                Active = true
            };
            onControlInit(obj, btn);
            var action = btn.LeftClickAction;
            btn.LeftClickAction = () =>
            {
                action();
                Btn_Click(btn);
            };
            this.Items.Add(btn);
            this.Client.AddControlsBottomLeft(btn);
            this.UpdateClientSize();
            return this;
        }
        public ListBoxNew<TObject, TControl> AddItem(TObject obj, string label, Action callback)
        {
            if (obj == null)
                return this;
            var btn = new TControl()
            {
                AutoSize = false, // latest addition
                Location = Client.Controls.BottomLeft,
                Tag = obj,
                Text = label,
                Name = label,
                TooltipFunc = (tt) => { if (obj is ITooltippable) (obj as ITooltippable).GetTooltipInfo(tt); },
                Width = Client.Width,
                Active = true
            };
            btn.LeftClickAction = () =>
            {
                callback();
                Btn_Click(btn);
            };
            this.Items.Add(btn);
            this.Client.AddControlsBottomLeft(btn);
            this.UpdateClientSize();
            return this;
        }

        /// <summary>
        /// Adds a control that doesn't use the lists default name getter and control initializer
        /// </summary>
        /// <param name="btn"></param>
        /// <returns></returns>
        public ListBoxNew<TObject, TControl> AddItem(TControl btn)
        {
            this.Items.Add(btn);
            this.Client.AddControlsBottomLeft(btn);
            this.UpdateClientSize();
            return this;
        }
        public ListBoxNew<TObject, TControl> AddItems(IEnumerable<TObject> objs, Func<TObject, TControl> controlGetter)
        {
            this.ControlGetter = controlGetter;
            foreach (var obj in objs)
                this.AddItem(obj, controlGetter);
            return this;
        }
        public ListBoxNew<TObject, TControl> AddItems(params TObject[] objs)
        {
            foreach (var obj in objs)
                this.AddItem(obj, this.ControlGetter);
            return this;
        }
        public ListBoxNew<TObject, TControl> AddItems(IEnumerable<TObject> objs)
        {
            foreach (var obj in objs)
                this.AddItem(obj, this.ControlGetter);
            return this;
        }
        public ListBoxNew<TObject, TControl> AddItems(IEnumerable<TObject> objs, Action<TObject> callback)
        {
            foreach (var obj in objs)
                this.AddItem(obj, r =>
                {
                    var c = this.ControlGetter(r);
                    c.LeftClickAction = () => callback(r);
                    return c;
                });
            return this;
        }
        public ListBoxNew<TObject, TControl> AddItem(TObject obj, Func<TObject, TControl> controlGetter)
        {
            var btn = controlGetter(obj);
            btn.AutoSize = false;
            btn.Location = Client.Controls.BottomLeft;
            btn.Tag = obj;
            btn.Name = btn.Text;
            btn.TooltipFunc = (tt) => { if (obj is ITooltippable) (obj as ITooltippable).GetTooltipInfo(tt); };
            btn.Width = Client.Width;
            btn.Active = true;
            var action = btn.LeftClickAction;
            btn.LeftClickAction = () =>
            {
                action();
                Btn_Click(btn);
            };
            this.Items.Add(btn);
            if (this.CurrentFilter(obj))
            {
                this.Client.AddControlsBottomLeft(btn);
                this.UpdateClientSize();
            }
            return this;
        }
        [Obsolete]
        public ListBoxNew<TObject, TControl> Build(Dictionary<string, List<TObject>> list, Func<TObject, string> nameFunc, Action<string, TControl> onCategoryInit, Action<TObject, TControl> onControlInit)
        {
            this.Client.Controls.Clear();
            foreach (var obj in list)
            {
                var inner = new GroupBox();// new List<TControl>();
                foreach (var item in obj.Value)
                {
                    TControl btninner = new TControl();
                    btninner.Tag = item;
                    btninner.Name = btninner.Text;
                    btninner.TooltipFunc = (tt) => { if (item is ITooltippable) (item as ITooltippable).GetTooltipInfo(tt); };
                    btninner.Text = nameFunc(item);
                    btninner.Active = true;
                    btninner.Location = inner.Controls.BottomLeft;// +new Vector2(16, 0);
                    btninner.LeftClickAction = () => Toggle(btninner);
                    inner.Controls.Add(btninner);
                }

                TControl btn = new TControl()
                {
                    Location = Client.Controls.Count > 0 ? Client.Controls.Last().BottomLeft : Vector2.Zero,
                    Tag = inner,
                    Text = obj.Key,
                    Name = obj.Key,
                    TooltipFunc = (tt) => { if (obj is ITooltippable) (obj as ITooltippable).GetTooltipInfo(tt); },
                    Active = true
                };
                //onControlInit(obj, btn as TControl);
                var action = btn.LeftClickAction;
                btn.LeftClickAction = () =>
                {
                    this.Toggle(btn);
                };
                this.Client.Controls.Add(btn);
            }
            Remeasure();
            Client.ClientLocation = Vector2.Zero;
            SelectedControl = null;
            return this;
        }

        public void SelectItem(int index)
        {
            this.SelectedControl = this.Client.Controls[index] as TControl;
            this.Btn_Click(this.SelectedControl);
        }
        void OnSelectedItemChanged(object control)
        {
            ItemChangedFunc((control as TControl).Tag as TObject);
            SelectedItemChanged?.Invoke(control, EventArgs.Empty);
        }
        public void RemoveItems(Func<TObject, bool> condition)
        {
            foreach(var ctrl in this.Client.Controls.ToList())
            {
                var item = ctrl.Tag as TObject;
                if (condition(item))
                    this.RemoveItem(item);
            }
        }
        public void RemoveItems(params TObject[] items)
        {
            foreach (var i in items)
                this.RemoveItem(i);
        }
        public void RemoveItem(TObject item)
        {
            if (item == null)
                return;
            var listControls = this.Client.Controls;
            var removedItemIndex = listControls.FindIndex(c => c.Tag == item);
            var prevY = listControls[removedItemIndex].Location.Y;
            for (int i = removedItemIndex + 1; i < listControls.Count; i++)
            {
                var r = listControls[i];
                r.Location.Y = prevY;
                prevY = r.Bottom;
            }
            listControls.RemoveAt(removedItemIndex);
            this.UpdateClientSize();
        }
       
        void Toggle(TControl btn)
        {
            var inner = btn.Tag as GroupBox;
            if (this.Client.Controls.Contains(inner))
                this.Collapse(btn);
            else
                this.Expand(btn);
        }
        private void Expand(TControl btn)
        {
            var inner = btn.Tag as GroupBox;
            inner.Location = btn.BottomLeft + new Vector2(16,0);
            var btnIndex = this.Client.Controls.FindIndex(c => c == btn);
            for (int i = btnIndex + 1; i < this.Client.Controls.Count; i++)
            {
                var control = this.Client.Controls[i];
                control.Location.Y += inner.Height;
            }
            this.Add(inner);
        }
        private void Collapse(TControl btn)
        {
            var inner = btn.Tag as GroupBox;
            this.Remove(inner);

            var btnIndex = this.Client.Controls.FindIndex(c => c == btn);
            for (int i = btnIndex + 1; i < this.Client.Controls.Count; i++)
            {
                var control = this.Client.Controls[i];
                control.Location.Y -= inner.Height;
            }
            Remeasure();
        }

        public override void Remeasure()
        {
            base.Remeasure();
            foreach (var btn in Client.Controls)
            {
                btn.Width = Math.Max(btn.Width, Client.Width);
            }
        }
        
        void Btn_Click(TControl ctrl)
        {
            if (this.SelectedControl != null)
                this.SelectedControl.BackgroundColor = Color.Transparent;
            this.SelectedControl = ctrl;
            ctrl.BackgroundColor = Color.White * 0.5f;
            OnSelectedItemChanged(ctrl);
        }
        
        public void Filter(Func<TObject, bool> filter)
        {
            this.Client.Controls.Clear();
            var validControls = this.Items.Where(c => filter(c.Tag as TObject)).ToArray();
            this.Client.AddControlsBottomLeft(validControls);
            this.UpdateClientSize();
            this.CurrentFilter = filter;
        }
    }
}

