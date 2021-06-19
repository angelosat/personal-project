using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.UI
{
    public class ListBoxCollapsible<TObject, TControl> : ScrollableBoxNew
        where TControl : ButtonBase, new()
        where TObject : class, ILabeled
    {
        public Action<TObject[]> CallBack = a => { };
        int IndentWidth = 8;

        public ListBoxCollapsible()
           : this(0, 0)
        {
            this.AutoSize = true;
            //this.MouseThrough = true;
        }
        public ListBoxCollapsible(int width, int height) : base(new Rectangle(0, 0, width, height))
        {
            //this.MouseThrough = true;
            this.AutoSize = false;
        }
        public ListBoxCollapsible(Rectangle bounds) : base(bounds)
        {
            //this.MouseThrough = true;
        }

        private void ResolveNodes(IEnumerable<TObject> list)
        {
            this.ARoot.Resolve(list);
        }

        ListBoxCollapsibleNode<TObject, TControl> ARoot = new ListBoxCollapsibleNode<TObject, TControl>("root");


        public ListBoxCollapsible<TObject, TControl> AddNode(ListBoxCollapsibleNode<TObject, TControl> node)
        {
            this.ARoot.AddNode(node);
            //this.ARoot = node;
            return this;
        }
        
        public ListBoxCollapsible<TObject, TControl> SetCallback(Action<TObject[]> callback)
        {
            this.CallBack = callback;
            return this;
        }

        public ListBoxCollapsible<TObject, TControl> Build(Action<TObject, TControl> onLeafInit)//, Action<ListBoxCollapsibleNode<TObject, TControl>, TControl> onNodeInit)//IEnumerable<TObject> list, Func<TObject, string> nameFunc, Action<TObject, TControl> onControlInit)
        {
            this.Client.ClearControls();
            var queue = new Queue<ListBoxCollapsibleNode<TObject, TControl>>(this.ARoot.Children);
            void onLeafSelect(params TObject[] obj)
            {
                this.CallBack(obj);
            };
            while(queue.Any())
            {
                var node = queue.Dequeue();
                node.Build(onLeafInit, onLeafSelect);
                var container = new GroupBox();
                
                var checkbox = new TControl();
                var label = new Label(node.Name) { Active = true } ;
                checkbox.LeftClickAction = () =>
                      {
                          this.CallBack(node.CustomLeafs.ToArray());
                      };
                node.OnNodeControlInit(checkbox);

                label.LeftClickAction = () =>
                {
                    if (!node.Expanded)
                    {
                        node.Expanded = true;
                        var nodeindex = this.Client.Controls.IndexOf(container);
                        var indexToInsert = nodeindex + 1;
                        foreach (var lc in node.LeafControls)
                        {
                            this.Client.Controls.Insert(indexToInsert, lc);
                            lc.Location.X = node.GetDepth() * IndentWidth;
                        }
                        this.Client.AlignTopToBottom();
                        //this.Client.ClientSize = this.Client.PreferredClientSize;
                        this.UpdateClientSize();

                        var lowestLeaf = node.LeafControls.First();
                        this.EnsureControlVisible(lowestLeaf);
                    }
                    else
                    {
                        node.Expanded = false;
                        this.Client.Controls.RemoveAll(c => node.LeafControls.Contains(c));

                        this.Client.AlignTopToBottom();
                        this.UpdateClientSize();
                        //this.EnsureClientWithinBounds(); // do i need this here?
                    }
                    //this.Remeasure();
                    //this.UpdateScrollbars(); // do i need this here?
                };
                
                container.AddControlsHorizontally(checkbox, label);

                node.Control = container;// checkbox;

                foreach (var child in node.Children)
                    queue.Enqueue(child);
            }
            
            foreach(var child in this.ARoot.Children)
            {
                this.Client.AddControlsBottomLeft(child.Control);
            }

            return this;
           
        }

        private void EnsureControlVisible(Control control)
        {
            var lowestVisiblePoint = control.Location.Y + control.Height;
            EnsureLocationVisible(lowestVisiblePoint);
        }

        public void AddItem(TObject obj, Func<TObject, string> nameFunc, Action<TObject, TControl> onControlInit)
        {
            //TControl btn = new TControl()
            //{
            //    Location = Client.Controls.BottomLeft,
            //    Width = Client.Width,
            //    Tag = obj,
            //    Text = nameFunc(obj),
            //    Name = nameFunc(obj),
            //    TooltipFunc = (tt) => { if (obj is ITooltippable) (obj as ITooltippable).GetTooltipInfo(tt); },
            //    Active = true
            //};
            //onControlInit(obj, btn as TControl);
            //var action = btn.LeftClickAction;
            //btn.LeftClickAction = () =>
            //{
            //    action();
            //    btn_Click(btn);
            //};
            //this.Client.Controls.Add(btn);
        }

        public TObject SelectedItem { get { return SelectedControl == null ? default : SelectedControl.Tag as TObject; } }
        TControl SelectedControl;

        public void SelectItem(TObject obj)
        {
            //this.SelectedControl = this.Client.Controls.FirstOrDefault(i => i.Tag == obj) as TControl;
            //this.btn_Click(this.SelectedControl);
        }

        public Action<TObject> ItemChangedFunc = (item) => { };
        public IEnumerable<TObject> List = new List<TObject>();
       
        public void Clear()
        {
            this.Client.Controls.Clear();
        }


        private void Expand(TControl btn)
        {
            List<TObject> list = btn.Tag as List<TObject>;

        }
        public override void Remeasure()
        {
            base.Remeasure();
            // TODO: lol
            int oldH = Client.Height;
            Client.ClientSize = new Rectangle(0, 0, Client.Width, Client.ClientSize.Height);
            Client.Height = oldH;
            foreach (var btn in Client.Controls)
            {
                btn.Width = Client.Width;
              //  btn.Invalidate();
            }
        }

        
    }

    public class ListBoxCollapsible : ScrollableBoxNew
    {
        //public Action<TObject[]> CallBack = a => { };
        int IndentWidth = UIManager.ArrowRight.Rectangle.Width;

        public ListBoxCollapsible()
           : this(0, 0)
        {
            this.AutoSize = true;
            //this.MouseThrough = true;
        }
        public ListBoxCollapsible(int width, int height, ScrollModes mode = ScrollModes.Vertical) : base(new Rectangle(0, 0, width, height), mode)
        {
            //this.MouseThrough = true;
            this.AutoSize = false;
        }
        public ListBoxCollapsible(Rectangle bounds) : base(bounds)
        {
            //this.MouseThrough = true;
        }

        //private void ResolveNodes(IEnumerable<TObject> list)
        //{
        //    this.ARoot.Resolve(list);
        //}

        public ListBoxCollapsibleNode ARoot = new("root");


        public ListBoxCollapsible AddNode(ListBoxCollapsibleNode node)
        {
            this.ARoot.AddNode(node);
            //this.ARoot = node;
            return this;
        }

        //public ListBoxCollapsible<TObject, TControl> SetCallback(Action<TObject[]> callback)
        //{
        //    this.CallBack = callback;
        //    return this;
        //}

        //public ListBoxCollapsible Build(Func<object, Control> controlGetter)//, Action<ListBoxCollapsibleNode<TObject, TControl>, TControl> onNodeInit)//IEnumerable<TObject> list, Func<TObject, string> nameFunc, Action<TObject, TControl> onControlInit)
        //{
        //    this.Client.ClearControls();
        //    var queue = new Queue<ListBoxCollapsibleNode>(this.ARoot.Children);
        //    //void onLeafSelect(params TObject[] obj)
        //    //{
        //    //    this.CallBack(obj);
        //    //};
        //    while (queue.Any())
        //    {
        //        var node = queue.Dequeue();
        //        //node.Build(onLeafInit, onLeafSelect);
        //        var container = new GroupBox();
        //        //var checkbox = new TControl();
        //        //var arrow = new PictureBox(UIManager.ArrowRight);
        //        var label = new Label(node.Name) { Active = true };//, Location = arrow.TopRight };
        //        //checkbox.LeftClickAction = () =>
        //        //{
        //        //    this.CallBack(node.CustomLeafs.ToArray());
        //        //};
        //        //node.OnNodeControlInit(checkbox);

        //        label.LeftClickAction = () =>
        //        {
        //            if (!node.Expanded)
        //            {
        //                node.Expanded = true;
        //                var nodeindex = this.Client.Controls.IndexOf(container);
        //                var indexToInsert = nodeindex + 1;
        //                foreach (var lc in node.LeafControls)
        //                {
        //                    this.Client.Controls.Insert(indexToInsert, lc);
        //                    lc.Location.X = node.GetDepth() * IndentWidth;
        //                }
        //                this.Client.AlignTopToBottom();
        //                this.UpdateClientSize();

        //                var lowestLeaf = node.LeafControls.First();
        //                this.EnsureControlVisible(lowestLeaf);
        //            }
        //            else
        //            {
        //                node.Expanded = false;
        //                this.Client.Controls.RemoveAll(c => node.LeafControls.Contains(c));

        //                this.Client.AlignTopToBottom();
        //                this.UpdateClientSize();
        //            }
        //        };

        //        //container.AddControlsLeftToRight(checkbox, label);
        //        //container.AddControlsLeftToRight(arrow, label);
        //        node.Control = container;// checkbox;

        //        foreach (var child in node.Children)
        //            queue.Enqueue(child);
        //    }

        //    foreach (var child in this.ARoot.Children)
        //    {
        //        this.Client.AddControlsBottomLeft(child.Control);
        //    }

        //    return this;

        //}
        public ListBoxCollapsible Build()//, Action<ListBoxCollapsibleNode<TObject, TControl>, TControl> onNodeInit)//IEnumerable<TObject> list, Func<TObject, string> nameFunc, Action<TObject, TControl> onControlInit)
        {
            this.Client.ClearControls();
            var queue = new Queue<ListBoxCollapsibleNode>(this.ARoot.Children);
            //void onLeafSelect(params TObject[] obj)
            //{
            //    this.CallBack(obj);
            //};
            while (queue.Any())
            {
                var node = queue.Dequeue();
                //node.Build(onLeafInit, onLeafSelect);
                var container = new GroupBox();
                //var checkbox = new TControl();
                node.Arrow = new PictureBox(UIManager.ArrowRight) { LeftClickAction = expand };// MouseThrough = true };
                var label = new Label(node.Name) { Active = true };
                //var label = node.ControlGetter();// new Label(node.Name) { Active = true };
                var control = node.ControlGetter?.Invoke();
                if (control != null)
                    container.AddControlsHorizontally(node.Arrow, control, label);
                else
                    container.AddControlsHorizontally(node.Arrow, label);
                container.CenterControlsAlignmentVertically();
                container.Validate(true);

                if (node.Parent != null)
                    node.Parent.ChildControls.Add(container);
                //checkbox.LeftClickAction = () =>
                //{
                //    this.CallBack(node.CustomLeafs.ToArray());
                //};
                //node.OnNodeControlInit(checkbox);

                label.LeftClickAction = expand;
                void expand()
                {
                    if (!node.Expanded)
                    {
                        node.Expanded = true;
                        node.Arrow.SetTexture(UIManager.ArrowDown);
                        var nodeindex = this.Client.Controls.IndexOf(container);
                        var indexToInsert = nodeindex + 1;
                        var allControls = node.ChildControls.Concat(node.LeafControls);
                        foreach (var lc in allControls)
                        {
                            this.Client.Controls.Insert(indexToInsert, lc);
                            lc.Location.X = node.GetDepth() * IndentWidth;
                        }
                        this.Client.AlignTopToBottom();
                        this.UpdateClientSize();

                        var lowestLeaf = allControls.First();
                        this.EnsureControlVisible(lowestLeaf);
                    }
                    else
                    {
                        var descendants = new Queue<ListBoxCollapsibleNode>();
                        descendants.Enqueue(node);
                        while (descendants.Any())
                        {
                            var current = descendants.Dequeue();
                            current.Expanded = false;
                            current.Arrow.SetTexture(UIManager.ArrowRight);

                            //this.Client.Controls.RemoveAll(c => node.LeafControls.Contains(c) || node.ChildControls.Contains(c));
                            this.Client.Controls.RemoveAll(c => current.LeafControls.Contains(c) || current.ChildControls.Contains(c));
                            foreach(var child in current.Children)
                                descendants.Enqueue(child);
                        }
                        this.Client.AlignTopToBottom();
                        this.UpdateClientSize();
                    }
                };

                //container.AddControlsLeftToRight(checkbox, label);

                node.Control = container;// label;// checkbox;

                foreach (var child in node.Children)
                    queue.Enqueue(child);
            }

            foreach (var child in this.ARoot.Children)
            {
                this.Client.AddControlsBottomLeft(child.Control);
            }
            return this;

        }

        private void EnsureControlVisible(Control control)
        {
            var lowestVisiblePoint = control.Location.Y + control.Height;
            EnsureLocationVisible(lowestVisiblePoint);
        }

        //public void AddItem(object obj, Func<string> nameFunc, Action<TObject, TControl> onControlInit)
        //{
        //    //TControl btn = new TControl()
        //    //{
        //    //    Location = Client.Controls.BottomLeft,
        //    //    Width = Client.Width,
        //    //    Tag = obj,
        //    //    Text = nameFunc(obj),
        //    //    Name = nameFunc(obj),
        //    //    TooltipFunc = (tt) => { if (obj is ITooltippable) (obj as ITooltippable).GetTooltipInfo(tt); },
        //    //    Active = true
        //    //};
        //    //onControlInit(obj, btn as TControl);
        //    //var action = btn.LeftClickAction;
        //    //btn.LeftClickAction = () =>
        //    //{
        //    //    action();
        //    //    btn_Click(btn);
        //    //};
        //    //this.Client.Controls.Add(btn);
        //}

        public object SelectedItem { get { return SelectedControl == null ? default : SelectedControl.Tag; } }
        Control SelectedControl;

        public void SelectItem(object obj)
        {
            //this.SelectedControl = this.Client.Controls.FirstOrDefault(i => i.Tag == obj) as TControl;
            //this.btn_Click(this.SelectedControl);
        }

        //public Action<TObject> ItemChangedFunc = (item) => { };
        //public IEnumerable<TObject> List = new List<TObject>();

        public void Clear()
        {
            this.Client.Controls.Clear();
            this.ARoot.Clear();
        }


        private void Expand(Control btn)
        {
            List<object> list = btn.Tag as List<object>;

        }
        public override void Remeasure()
        {
            base.Remeasure();
            // TODO: lol
            int oldH = Client.Height;
            Client.ClientSize = new Rectangle(0, 0, Client.Width, Client.ClientSize.Height);
            Client.Height = oldH;
            foreach (var btn in Client.Controls)
            {
                btn.Width = Client.Width;
                //  btn.Invalidate();
            }
        }

        internal bool FindLeafIndex(Control c, out int i)
        {
            i = 0;
            foreach (var item in this.GetEnumerable())
            {
                if (c == item)
                    return true;
                i++;
            }
            return false;
        }
        internal Control GetLeafByIndex(int i)
        {
            var n = 0;
            var enumerator = this.GetEnumerable().GetEnumerator();
            do { enumerator.MoveNext(); } while (n++ != i);
            return enumerator.Current;
        }
        IEnumerable<Control> GetEnumerable()
        {
            var queue = new Queue<ListBoxCollapsibleNode>();
            queue.Enqueue(this.ARoot);
            while (queue.Any())
            {
                var current = queue.Dequeue();
                foreach (var leaf in current.Leafs)
                    yield return leaf;
                foreach (var child in current.Children)
                    queue.Enqueue(child);
            }
        }
    }
}

