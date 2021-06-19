using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.UI
{
    public class ListBoxNew<TObject, TControl> : ScrollableBoxNew, IListSearchable<TObject>
        where TControl : ButtonBase, new()
        where TObject : class
    {
        public Func<TObject, string> ItemNameFunc;
        public Action<TObject, TControl> OnControlInit;
        Func<TObject, TControl> ControlGetter;
        public TObject SelectedItem { get { return (SelectedControl == null ? default(TObject) : SelectedControl.Tag as TObject); } }
        TControl SelectedControl;

        public void SelectItem(TObject obj)
        {
            this.SelectedControl = this.Client.Controls.FirstOrDefault(i => i.Tag == obj) as TControl;
            this.btn_Click(this.SelectedControl);
        }
        public void SelectItem(int index)
        {
            this.SelectedControl = this.Client.Controls[index] as TControl;
            this.btn_Click(this.SelectedControl);
        }
        public List<TControl> Items = new List<TControl>();
        //{
        //    get
        //    {
        //        return this.Client.Controls.Cast<TControl>().ToList();
        //    }
        //}

        public Action<TObject> ItemChangedFunc = (item) => { };
        public IEnumerable<TObject> List = new List<TObject>();
        public ListBoxNew()
            : this(0, 0) 
        {
            this.AutoSize = true;
        }
        public ListBoxNew(int width, int height, ScrollableBoxNew.ScrollModes mode = ScrollModes.Vertical) : base(new Rectangle(0,0,width,height), mode) 
        {
        }//MouseThrough = true; }
        public ListBoxNew(int width, int height, Func<TObject, TControl> controlGetter, ScrollableBoxNew.ScrollModes mode = ScrollModes.Vertical) : this(width, height, mode)
        {
            this.ControlGetter = controlGetter;
        }//MouseThrough = true; }
        public ListBoxNew(int width, int height, Func<TObject, ListBoxNew<TObject,TControl>, TControl> controlGetter, ScrollableBoxNew.ScrollModes mode = ScrollModes.Vertical) : this(width, height, mode)
        {
            this.ControlGetter = i => controlGetter(i, this);
        }//MouseThrough = true; }
        public ListBoxNew(Rectangle bounds) : base(bounds)
        {
        }//MouseThrough = true; }

        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged(object control)
        {
            ItemChangedFunc((control as TControl).Tag as TObject);
            if (SelectedItemChanged != null)
                SelectedItemChanged(control, EventArgs.Empty);
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
            //TControl btn;
            foreach (var obj in list)
            {
                var btn = new TControl()
                {
                    Location = Client.Controls.Count > 0 ? Client.Controls.Last().BottomLeft : Vector2.Zero,
                  //  Width = this.Width,// Client.Width,
                    Tag = obj,
                    Text = nameFunc(obj),
                    Name = nameFunc(obj),
                    HoverText = htv(obj),
                    Active = true
                };
                //btn.LeftClick += new UIEvent(btn_Click);
                var action = btn.LeftClickAction;
                btn.LeftClickAction = () =>
                {
                    action();
                    btn_Click(btn);
                };
                this.Client.Controls.Add(btn);
            }
            Remeasure();
            Client.ClientLocation = Vector2.Zero;
           // SelectedItem = default(TObject);
            SelectedControl = null;
            return this;
        }
        public ListBoxNew<TObject, TControl> Build(IEnumerable<TObject> list, Func<TObject, string> nameFunc, Action<TObject, TControl> onControlInit)
        {
            foreach(var item in list)
            {
                this.AddItem(item, nameFunc, onControlInit);
            }
            return this;

            //this.List = list;
            //this.ItemNameFunc = nameFunc;
            //this.OnControlInit = onControlInit;
            //this.Client.Controls.Clear();
            //foreach (var obj in list)
            //{
            //    AddItem(obj, nameFunc, onControlInit);
            //}
            //Remeasure(); // why did i have this commented out?
            //Client.ClientLocation = Vector2.Zero;
            //SelectedControl = null;
            //this.UpdateClientSize();

            //return this;
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
                btn_Click(btn);
            };
            this.Items.Add(btn);
            //this.Client.Controls.Add(btn);
            this.Client.AddControlsBottomLeft(btn);
            this.UpdateClientSize();
            //this.Client.ClientSize = this.Client.PreferredClientSize;
            //this.UpdateScrollbars();
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
            // i'm testing adding a null option in the beginning of some lists
            //if (obj == null)
            //    return this;
            var btn = controlGetter(obj);
            btn.AutoSize = false; // latest addition
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
                btn_Click(btn);
            };
            this.Items.Add(btn);
            //this.Client.Controls.Add(btn);
            if (this.CurrentFilter(obj))
            {
                this.Client.AddControlsBottomLeft(btn);
                this.UpdateClientSize();
            }
            //this.Client.ClientSize = this.Client.PreferredClientSize;
            //this.UpdateScrollbars();
            return this;
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
        public void RemoveItems(IEnumerable<TObject> items)
        {
            foreach (var i in items)
                this.RemoveItem(i);
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

            //foreach (var r in this.Client.Controls.Where(c => c.Tag != null && !c.Tag.Equals(item)))
            //{
            //    r.Location.Y = prev;
            //    prev = r.Bottom;
            //}
            //this.Client.Controls.Remove(this.Client.Controls.First(c => c.Tag != null && c.Tag.Equals(item)));
            //this.UpdateClientSize();
        }
        [Obsolete]
        public ListBoxNew<TObject, TControl> Build(Dictionary<string, List<TObject>> list, Func<TObject, string> nameFunc, Action<string, TControl> onCategoryInit, Action<TObject, TControl> onControlInit)
        {
            this.Client.Controls.Clear();
            foreach (var obj in list)
            {
                var inner = new GroupBox();// new List<TControl>();
                foreach(var item in obj.Value)
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
            //this.Client.Controls.Add(inner);
            this.Add(inner);
            //Remeasure();
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
            // TODO: lol
            //int oldH = Client.Height;
            //Client.ClientSize = new Rectangle(0, 0, Client.Width, Client.ClientSize.Height);
            //Client.Height = oldH;
            foreach (var btn in Client.Controls)
            {
                //btn.Width = Client.Width;
                btn.Width = Math.Max(btn.Width, Client.Width);
            }
        }
        public void Sort<T>(Func<TObject, T> selector, IComparer<T> comparer)
        {
            //var strings = this.Client.Controls.Select(c => selector(c.Tag as TObject)).ToList();
            //strings.Sort()
            //var ordered = strings.OrderBy(s => s, comparer).ToList();
            var ordered = this.Client.Controls.OrderBy(c=>selector(c.Tag as TObject), comparer);
            var prev = 0;
            foreach (var r in ordered)
            {
                r.Location.Y = prev;
                prev = r.Bottom;
            }
        }

        //void btn_Click(object sender, EventArgs e)
        //{
        //    TControl ctrl = sender as TControl;
        ////    this.SelectedItem = ctrl.Tag as TObject;
        //    if (!this.SelectedControl.IsNull())
        //        this.SelectedControl.BackgroundColor = Color.Transparent;
        //    this.SelectedControl = ctrl;
        //    ctrl.BackgroundColor = Color.White * 0.5f;
        //    OnSelectedItemChanged(sender);
        //}
        void btn_Click(TControl ctrl)
        {
            //TControl ctrl = sender as TControl;
            //    this.SelectedItem = ctrl.Tag as TObject;
            if (this.SelectedControl != null)
                this.SelectedControl.BackgroundColor = Color.Transparent;
            this.SelectedControl = ctrl;
            ctrl.BackgroundColor = Color.White * 0.5f;
            OnSelectedItemChanged(ctrl);
        }
        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }
        Func<TObject, bool> CurrentFilter = i => true;

        public void Filter(Func<TObject, bool> filter)
        {
            //var validControls = this.Client.Controls.Where(c => filter(c.Tag as TObject)).ToArray();
            //var validControlsHash = new HashSet<Control>(validControls);
            //this.Client.Controls.RemoveAll(c => !validControlsHash.Contains(c));
            //this.Client.AddControlsBottomLeft(validControls);
            this.Client.Controls.Clear();
            var validControls = this.Items.Where(c => filter(c.Tag as TObject)).ToArray();
            this.Client.AddControlsBottomLeft(validControls);
            this.UpdateClientSize();
            this.CurrentFilter = filter;
        }
    }
}

