using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.UI
{
    public class ListBoxObservable<TObject, TControl> : ScrollableBoxNew, IListSearchable<TObject>
        where TControl : Control, new()
        where TObject : class
    {
        const int Spacing = 1;

        public TObject SelectedItem { get { return (SelectedControl == null ? default : SelectedControl.Tag as TObject); } }
        TControl SelectedControl;

        public void SelectItem(TObject obj)
        {
            this.SelectedControl = this.Client.Controls.FirstOrDefault(i => i.Tag == obj) as TControl;
        }

        public List<TControl> Items
        {
            get
            {
                return this.Client.Controls.Cast<TControl>().ToList();
            }
        }

        public Action<TObject> ItemChangedFunc = (item) => { };
        public ObservableCollection<TObject> List;// = new List<TObject>();
        Func<TObject, TControl> ControlFactory;
        public ListBoxObservable(int width, int height, Func<TObject, TControl> controlFactory, ScrollModes mode = ScrollModes.Vertical)
            : base(new Rectangle(0, 0, width, height), mode)
        {
            this.ControlFactory = controlFactory;
        }
        public ListBoxObservable(int width, int height, ObservableCollection<TObject> collection, Func<TObject, TControl> controlFactory, ScrollModes mode = ScrollModes.Vertical)
            : base(new Rectangle(0, 0, width, height), mode)
        {
            //this.List = collection;
            this.ControlFactory = controlFactory;
            //this.List.CollectionChanged += List_CollectionChanged;
            this.Bind(collection);
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

        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged(object control)
        {
            ItemChangedFunc((control as TControl).Tag as TObject);
            if (SelectedItemChanged != null)
                SelectedItemChanged(control, EventArgs.Empty);
        }
        ListBoxObservable<TObject, TControl> Clear()
        {
            this.Client.Controls.Clear();
            return this;
        }

        //public ListBoxObservable<TObject, TControl> Build(IEnumerable<TObject> list, Func<TObject, string> nameFunc, Func<TObject, string> hoverTextFunc = null)
        //{
        //    this.List = list;
        //    Func<TObject, string> htv = hoverTextFunc ?? (foo => "");
        //    this.Client.Controls.Clear();
        //    //TControl btn;
        //    foreach (var obj in list)
        //    {
        //        var btn = new TControl()
        //        {
        //            Location = Client.Controls.Count > 0 ? Client.Controls.Last().BottomLeft : Vector2.Zero,
        //          //  Width = this.Width,// Client.Width,
        //            Tag = obj,
        //            Name = nameFunc(obj),
        //            HoverText = htv(obj),
        //            Active = true
        //        };

        //        this.Client.Controls.Add(btn);
        //    }
        //    Remeasure();
        //    Client.ClientLocation = Vector2.Zero;
        //   // SelectedItem = default(TObject);
        //    SelectedControl = null;
        //    return this;
        //}
        //public ListBoxObservable<TObject, TControl> Build(IEnumerable<TObject> list, Action<TObject, TControl> onControlInit)
        //{
        //    this.List = list;
        ////    Func<TObject, string> htv = hoverTextFunc ?? (foo => "");
        //    this.Client.Controls.Clear();
        //    foreach (var obj in list)
        //    {
        //        AddItem(obj, onControlInit);
        //    }
        //    //Remeasure();
        //    Client.ClientLocation = Vector2.Zero;
        //    // SelectedItem = default(TObject);
        //    SelectedControl = null;
        //    return this;
        //}
        //void AddItem(TObject obj, Action<TObject, TControl> onControlInit)
        //{
        //    TControl btn = new TControl()
        //    {
        //        Location = Client.Controls.BottomLeft,
        //        Width = Client.Width,
        //        Tag = obj,
        //        //Name = nameFunc(obj),
        //        TooltipFunc = (tt) => { if (obj is ITooltippable) (obj as ITooltippable).GetTooltipInfo(tt); },
        //        Active = true
        //    };
        //    onControlInit(obj, btn as TControl);
        //    this.Client.Controls.Add(btn);
        //}
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
            if (items == null)
                return;
            foreach (var i in items)
                this.RemoveItem(i);
        }
        void RemoveItem(TObject item)
        {
            //var prev = 0;
            //foreach (var r in this.Client.Controls.Where(c => !c.Tag.Equals(item)))
            //{
            //    r.Location.Y = prev;
            //    prev = r.Bottom;
            //}
            //this.Client.Controls.Remove(this.Client.Controls.First(c => c.Tag.Equals(item)));
            if (item == null)
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


        TControl GetItem(TObject obj)
        {
            return this.Items.FirstOrDefault(c => c.Tag == obj);
        }
        [Obsolete]
        public ListBoxObservable<TObject, TControl> Build(Dictionary<string, List<TObject>> list, Func<TObject, string> nameFunc, Action<string, TControl> onCategoryInit, Action<TObject, TControl> onControlInit)
        {
            this.Client.Controls.Clear();
            foreach (var obj in list)
            {
                var inner = new GroupBox();// new List<TControl>();
                foreach(var item in obj.Value)
                {
                    TControl btninner = new TControl();
                    btninner.Tag = item;
                    //btninner.Name = btninner.Text;
                    btninner.TooltipFunc = (tt) => { if (item is ITooltippable) (item as ITooltippable).GetTooltipInfo(tt); };
                    //btninner.Text = nameFunc(item);
                    btninner.Active = true;
                    btninner.Location = inner.Controls.BottomLeft;// +new Vector2(16, 0);
                    //btninner.LeftClickAction = () => Toggle(btninner);
                    inner.Controls.Add(btninner);
                }

                TControl btn = new TControl()
                {
                    Location = Client.Controls.Count > 0 ? Client.Controls.Last().BottomLeft : Vector2.Zero,
                    Tag = inner,
                    //Text = obj.Key,
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

        //void btn_Click(TControl ctrl)
        //{
        //    //TControl ctrl = sender as TControl;
        //    //    this.SelectedItem = ctrl.Tag as TObject;
        //    if (!this.SelectedControl.IsNull())
        //        this.SelectedControl.BackgroundColor = Color.Transparent;
        //    this.SelectedControl = ctrl;
        //    ctrl.BackgroundColor = Color.White * 0.5f;
        //    OnSelectedItemChanged(ctrl);
        //}

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
        }

        public void Filter(Func<TObject, bool> filter)
        {
            this.Client.Controls.Clear();
            var validControls = this.Items.Where(c => filter(c.Tag as TObject)).ToArray();
            this.Client.AddControlsBottomLeft(validControls);
            this.UpdateClientSize();
        }
    }

    //public class ListBoxObservableNew<TObject, TControl> : ScrollableBoxNew, IListSearchable<TObject>
    //   where TControl : Control, new()
    //{
    //    const int Spacing = 1;


    //    public List<TControl> Items
    //    {
    //        get
    //        {
    //            return this.Client.Controls.Cast<TControl>().ToList();
    //        }
    //    }

    //    public Action<TObject> ItemChangedFunc = (item) => { };
    //    public ObservableCollection<TObject> List;// = new List<TObject>();
    //    Func<TObject, TControl> ControlFactory;
    //    public ListBoxObservableNew(int width, int height, Func<TObject, TControl> controlFactory, ScrollModes mode = ScrollModes.Vertical)
    //        : base(new Rectangle(0, 0, width, height), mode)
    //    {
    //        this.ControlFactory = controlFactory;
    //    }
    //    public ListBoxObservableNew(int width, int height, ObservableCollection<TObject> collection, Func<TObject, TControl> controlFactory, ScrollModes mode = ScrollModes.Vertical)
    //        : base(new Rectangle(0, 0, width, height), mode)
    //    {
    //        //this.List = collection;
    //        this.ControlFactory = controlFactory;
    //        //this.List.CollectionChanged += List_CollectionChanged;
    //        this.Bind(collection);
    //    }
    //    public ListBoxObservableNew<TObject, TControl> Bind(ObservableCollection<TObject> collection)
    //    {
    //        if (this.List != null)
    //            this.List.CollectionChanged -= List_CollectionChanged;
    //        this.List = collection;
    //        this.List.CollectionChanged += List_CollectionChanged;
    //        this.Clear();
    //        this.AddItems(collection);

    //        return this;
    //    }
    //    private void List_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    //    {
    //        this.AddItems(e.NewItems?.Cast<TObject>());
    //        this.RemoveItems(e.OldItems?.Cast<TObject>());
    //    }

    //    ListBoxObservableNew<TObject, TControl> Clear()
    //    {
    //        this.Client.Controls.Clear();
    //        return this;
    //    }

        
    //    void AddItems(IEnumerable<TObject> items)
    //    {
    //        if (items == null)
    //            return;
    //        foreach (var i in items)
    //            this.AddItem(i);
    //    }

    //    TControl AddItem(TObject item)
    //    {
    //        var control = this.ControlFactory(item);
    //        this.Client.AddControlsBottomLeft(control);
    //        control.Location.Y += Spacing;
    //        this.UpdateClientSize();

    //        return control;
    //    }
    //    void RemoveItems(IEnumerable<TObject> items)
    //    {
    //        if (items == null)
    //            return;
    //        foreach (var i in items)
    //            this.RemoveItem(i);
    //    }
    //    void RemoveItem(TObject item)
    //    {
    //        //var prev = 0;
    //        //foreach (var r in this.Client.Controls.Where(c => !c.Tag.Equals(item)))
    //        //{
    //        //    r.Location.Y = prev;
    //        //    prev = r.Bottom;
    //        //}
    //        //this.Client.Controls.Remove(this.Client.Controls.First(c => c.Tag.Equals(item)));
    //        if (item == null)
    //            return;
    //        var listControls = this.Client.Controls;
    //        var removedItemIndex = listControls.FindIndex(c => c.Tag == item);
    //        var prevY = listControls[removedItemIndex].Location.Y;
    //        for (int i = removedItemIndex + 1; i < listControls.Count; i++)
    //        {
    //            var r = listControls[i];
    //            r.Location.Y = prevY;
    //            prevY = r.Bottom + Spacing;
    //        }
    //        listControls.RemoveAt(removedItemIndex);
    //        this.UpdateClientSize();
    //    }


    //    TControl GetItem(TObject obj)
    //    {
    //        return this.Items.FirstOrDefault(c => c.Tag == obj);
    //    }
        
    //    void Toggle(TControl btn)
    //    {
    //        var inner = btn.Tag as GroupBox;
    //        if (this.Client.Controls.Contains(inner))
    //            this.Collapse(btn);
    //        else
    //            this.Expand(btn);
    //    }
    //    private void Expand(TControl btn)
    //    {
    //        var inner = btn.Tag as GroupBox;
    //        inner.Location = btn.BottomLeft + new Vector2(16, 0);
    //        var btnIndex = this.Client.Controls.FindIndex(c => c == btn);
    //        for (int i = btnIndex + 1; i < this.Client.Controls.Count; i++)
    //        {
    //            var control = this.Client.Controls[i];
    //            control.Location.Y += inner.Height;
    //        }
    //        //this.Client.Controls.Add(inner);
    //        this.Add(inner);
    //        //Remeasure();
    //    }



    //    private void Collapse(TControl btn)
    //    {
    //        var inner = btn.Tag as GroupBox;
    //        this.Remove(inner);

    //        var btnIndex = this.Client.Controls.FindIndex(c => c == btn);
    //        for (int i = btnIndex + 1; i < this.Client.Controls.Count; i++)
    //        {
    //            var control = this.Client.Controls[i];
    //            control.Location.Y -= inner.Height;
    //        }
    //        Remeasure();
    //    }

    //    public override void Remeasure()
    //    {
    //        base.Remeasure();
    //        // TODO: lol
    //        //int oldH = Client.Height;
    //        //Client.ClientSize = new Rectangle(0, 0, Client.Width, Client.ClientSize.Height);
    //        //Client.Height = oldH;
    //        foreach (var btn in Client.Controls)
    //        {
    //            //btn.Width = Client.Width;
    //            btn.Width = Math.Max(btn.Width, Client.Width);
    //        }
    //    }
    //    public void Sort<T>(Func<TObject, T> selector, IComparer<T> comparer)
    //    {
    //        //var strings = this.Client.Controls.Select(c => selector(c.Tag as TObject)).ToList();
    //        //strings.Sort()
    //        //var ordered = strings.OrderBy(s => s, comparer).ToList();
    //        var ordered = this.Client.Controls.OrderBy(c => selector(c.Tag as TObject), comparer);
    //        var prev = 0;
    //        foreach (var r in ordered)
    //        {
    //            r.Location.Y = prev;
    //            prev = r.Bottom;
    //        }
    //    }

    //    //void btn_Click(TControl ctrl)
    //    //{
    //    //    //TControl ctrl = sender as TControl;
    //    //    //    this.SelectedItem = ctrl.Tag as TObject;
    //    //    if (!this.SelectedControl.IsNull())
    //    //        this.SelectedControl.BackgroundColor = Color.Transparent;
    //    //    this.SelectedControl = ctrl;
    //    //    ctrl.BackgroundColor = Color.White * 0.5f;
    //    //    OnSelectedItemChanged(ctrl);
    //    //}

    //    public override void Draw(SpriteBatch sb)
    //    {
    //        base.Draw(sb);
    //    }

    //    public void Filter(Func<TObject, bool> filter)
    //    {
    //        this.Client.Controls.Clear();
    //        var validControls = this.Items.Where(c => filter(c.Tag as TObject)).ToArray();
    //        this.Client.AddControlsBottomLeft(validControls);
    //        this.UpdateClientSize();
    //    }
    //}
}

