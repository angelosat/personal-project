using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class ListItemEventArgs<T> : EventArgs
    {
        public T Item;
        public ListItemEventArgs(T item)
        {
            this.Item = item;
        }
    }

    class ScrollableList : Control
    {
        // Panel Panel_Scroll;
        //Scrollbar Scroll;
        public int Step = 3 * Label.DefaultHeight;
        public object SelectedItem;
        int MaxHeight;
        public ScrollableList(Vector2 location, int width, int maxHeight)
            : base(location, new Vector2(width, maxHeight))
        {
            ClientSize = Size;
           // AutoSize = true;
            this.MaxHeight = maxHeight;
        }
        public ScrollableList(Vector2 location, Rectangle size)
            : base(location, new Vector2(size.Width, size.Height))
        {
            ClientSize = size;
          //  AutoSize = true;
            this.MaxHeight = size.Height;
        }

        //public override void Update()
        //{
        //    base.Update();
        //    Scroll.Update();
        //}

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            //base.Draw(sb);
            // Scroll.Draw(sb);
            //DrawHighlight(sb);
            foreach (ButtonBase label in Controls.FindAll(foo => foo is ButtonBase))
            {
                Rectangle finalRect;
                Rectangle labelRect = label.ScreenBounds;
                Rectangle panelRect = ScreenBounds;//Bounds;
                Rectangle.Intersect(ref labelRect, ref panelRect, out finalRect);
                //  panel.DrawHighlight(e.SpriteBatch, panel.ScreenClientRectangle, 0.5f);
                if (label.Tag != null && label.Tag == SelectedItem)
                    label.DrawHighlight(sb, finalRect, 0.5f);
                //e.SpriteBatch.Draw(label.TextSprite, finalRect, Color.Lerp(Color.Transparent, Color.White, label.Opacity));
                Rectangle source = new Rectangle(0, finalRect.Y - labelRect.Y, finalRect.Width, finalRect.Height);
                label.OnDrawItem(new DrawItemEventArgs(sb, finalRect));
                bool isSelected = (label.Active && label.MouseHover) || label.Tag == SelectedItem;
                Vector2 pos = new Vector2(finalRect.X, finalRect.Y);
                label.DrawSprite(sb, finalRect, source, Color.Black, isSelected ? 1 : 0.5f);
                //label.DrawText(sb, Color.Lerp(Color.Transparent, label.Active && label.MouseHover ? Color.YellowGreen : Color.White, label.Opacity));
                label.DrawText(sb, pos, source, isSelected ? Color.YellowGreen : Color.White, 1);//label.Tag == SelectedItem ? 1 : 0.5f);

               // sb.Draw(label.TextSprite, new Vector2(finalRect.X, finalRect.Y), source, Color.Lerp(Color.Transparent, label.Active && label.MouseHover ? Color.YellowGreen : Color.White, label.Opacity));
                //  panel.ScreenClientRectangle
            }
        }

        public void Add(ButtonBase label)
        {
            Controls.Add(label);
            label.Active = true;
            int height = 0;
            foreach (Control control in Controls)
                height += control.Height;
            ClientSize = new Rectangle(0, 0, Size.Width, height);
            //if (height > MaxHeight)
            //{
            //    Scroll.Location = new Vector2(Size.Width - 16, 0);
            //    Scroll.Height = Size.Height;
            //    Controls.Add(Scroll);
            //}
        }

        public void Remove(ButtonBase label)
        {
            Controls.Remove(label);
            int height = 0;
            foreach (Control control in Controls)
                height += control.Height;
            //if (Height <= MaxHeight)
            //    Controls.Remove(Scroll);
        }


        //void listEntry_DrawItem(object sender, DrawItemEventArgs e)
        //{
        //    ButtonBase label = sender as ButtonBase;
        //    if (label.Tag != null && label.Tag == SelectedItem)
        //        label.DrawHighlight(e.SpriteBatch, 0.5f);
        //}
    }

    class ScrollableList<T> : Control
    {
        HorizontalAlignment HorizontalAlignment;
        // Panel Panel_Scroll;
        //Scrollbar Scroll;
        public T SelectedItem;
        int MaxHeight;
        List<T> List;
        public int Count { get { return List.Count; } }
        public Func<T, string> TextFunc, HoverFunc;
        public event EventHandler<EventArgs> SelectedItemChanged;
        public event EventHandler<ListItemEventArgs<T>> ItemRightClick;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }
        void OnItemRightClick(T item)
        {
            if(ItemRightClick!=null)
                ItemRightClick(this, new ListItemEventArgs<T>(item));
        }
        public ScrollableList(Vector2 location, int width, int maxHeight, Func<T, string> textFunc, HorizontalAlignment hAlign = UI.HorizontalAlignment.Center)
            : base(location, new Vector2(width, maxHeight))
        {
            this.HorizontalAlignment = hAlign;
            ClientSize = Size;
            this.MaxHeight = maxHeight;
           // List = list;
            TextFunc = textFunc;
        }
        public ScrollableList(Vector2 location, Rectangle size, Func<T, string> textFunc, HorizontalAlignment hAlign = UI.HorizontalAlignment.Center)
            : base(location, new Vector2(size.Width, size.Height))
        {
            this.HorizontalAlignment = hAlign;
            ClientSize = size;
            this.MaxHeight = size.Height;
         //   List = list;
            TextFunc = textFunc;
        }

        public void Build(IEnumerable<T> list)
        {
            Dispose();
            List = list.ToList();
            //foreach (Control control in Controls)
            //{
            //    control.Click -= entry_Click;
            //}
            //if (List == null)
            //    return;
            foreach(T item in List)
            {
                Label entry = new Label(new Vector2(0, Controls.Count * Label.DefaultHeight), TextFunc(item), HorizontalAlignment);
                entry.HoverText = HoverFunc != null ? HoverFunc(item) : "";
                entry.Width = Width;
                entry.LeftClick += new UIEvent(entry_Click);
                entry.MouseRightUp += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(entry_MouseRightUp);
                entry.Tag = item;
                Controls.Add(entry);
            }
        }
        public override void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.HasChildren)
                if (Parent.ScreenClientRectangle.Intersects(new Rectangle((int)UIManager.Mouse.X, (int)UIManager.Mouse.Y, 1, 1)))
                {
                    List<Control> controls = Controls.ToList();
                    controls.Reverse();
                    foreach (Control c in controls)
                    {
                        //Console.WriteLine(c);
                        c.HandleMouseMove(e);
                    }
                }
          //  Console.WriteLine(Controller.Instance.MouseoverNext.Object);
            if (HitTest(e))
            {
                
                if (Controller.Instance.MouseoverNext.Object != this)
                    return;
                
              //  WindowManager.ActiveControl = this;

                if (!MouseHover)
                {
                    //if (WindowManager.ActiveControl == this)
                    //{
                        OnMouseEnter();
                        MouseHover = true;
                    //}
                }
                else
                    OnMouseMove(e);
            }
            else
            {
                if (MouseHover)
                    OnMouseLeave();
                MouseHover = false;
            }
            //Console.WriteLine(Controller.Instance.MouseoverNext.Object);
            //----------------

            //if (!HitTest(e))
            //{
            //    if (MouseHover)
            //        OnMouseLeave();
            //    MouseHover = false;
            //    return;
            //}

            //if (WindowManager.ActiveControl != null)
            //    return;

            //WindowManager.ActiveControl = this;

            //if (!MouseHover)
            //{
            //    if (WindowManager.ActiveControl == this)
            //    {
            //        OnMouseEnter();
            //        MouseHover = true;
            //    }
            //}
            //else
            //    OnMouseMove(e);
        }
       

        public override void Dispose()
        {
            foreach (Control ctrl in Controls)
            {
                ctrl.LeftClick -= entry_Click;
                ctrl.MouseRightUp -= entry_MouseRightUp;
            }
            Controls.Clear();
        }

        void entry_Click(object sender, EventArgs e)
        {
            //if (!ScreenClientRectangle.Intersects(new Rectangle((int)UIManager.Mouse.X, (int)UIManager.Mouse.Y, 1, 1)))
            //    return;
            SelectedItem = (T)(sender as Label).Tag;
            OnSelectedItemChanged();
        }
        void entry_MouseRightUp(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            throw new NotImplementedException();
        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            //base.Draw(sb);
            // Scroll.Draw(sb);
            //DrawHighlight(sb);
            foreach (ButtonBase label in Controls.FindAll(foo => foo is ButtonBase))
            {
                Rectangle finalRect;
                Rectangle labelRect = label.ScreenBounds;
                Rectangle panelRect = ScreenBounds;//Bounds;
                Rectangle.Intersect(ref labelRect, ref panelRect, out finalRect);
                //  panel.DrawHighlight(e.SpriteBatch, panel.ScreenClientRectangle, 0.5f);
                if (label.Tag != null)
                    if (label.Tag.Equals(SelectedItem))
                        label.DrawHighlight(sb, finalRect, 0.5f);
                //e.SpriteBatch.Draw(label.TextSprite, finalRect, Color.Lerp(Color.Transparent, Color.White, label.Opacity));
                Rectangle source = new Rectangle(0, finalRect.Y - labelRect.Y, finalRect.Width, finalRect.Height);
                label.OnDrawItem(new DrawItemEventArgs(sb, finalRect));
                bool isSelected = (label.Active && label.MouseHover) || label.Tag.Equals(SelectedItem);
                Vector2 pos = new Vector2(finalRect.X, finalRect.Y);
                label.DrawSprite(sb, finalRect, source, Color.Black, isSelected ? 1 : 0.5f);
                //label.DrawText(sb, Color.Lerp(Color.Transparent, label.Active && label.MouseHover ? Color.YellowGreen : Color.White, label.Opacity));
                label.DrawText(sb, pos, source, isSelected ? Color.Lime : Color.White, 1);//label.Tag == SelectedItem ? 1 : 0.5f); //yellowgreen
                //if (label.Active && label.MouseHover)
                if (HitTest(finalRect))
                    Controller.Instance.MouseoverNext.Object = label;
                // sb.Draw(label.TextSprite, new Vector2(finalRect.X, finalRect.Y), source, Color.Lerp(Color.Transparent, label.Active && label.MouseHover ? Color.YellowGreen : Color.White, label.Opacity));
                //  panel.ScreenClientRectangle
            }
        }


    }

    class ScrollableList<ItemT,ButtonType> : Control where ButtonType : ButtonBase, new() where ItemT:class
    {
        // Panel Panel_Scroll;
        //Scrollbar Scroll;
        public ItemT SelectedItem;
        int MaxHeight;
        List<ItemT> List;
        public Func<ItemT, string> TextFunc, HoverFunc;
        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }
        public event EventHandler<ListItemEventArgs<ItemT>> ItemRightClick;
        void OnItemRightClick(ItemT item)
        {
            if (ItemRightClick != null)
                ItemRightClick(this, new ListItemEventArgs<ItemT>(item));
        }
        
        


        public ScrollableList(Vector2 location, int width, int maxHeight, Func<ItemT, string> textFunc)
            : base(location, new Vector2(width, maxHeight))
        {
          //  ClientSize = new Rectangle(0, 0, Width, 0);//Size;
           // AutoSize = true;
            this.MaxHeight = maxHeight;
            // List = list;
            TextFunc = textFunc;
        }
        public ScrollableList(Vector2 location, Rectangle size, Func<ItemT, string> textFunc)
            : base(location, new Vector2(size.Width, size.Height))
        {
           // ClientSize = new Rectangle(0, 0, Width, 0);// size;
           // AutoSize = true;
            this.MaxHeight = size.Height;
            //   List = list;
            TextFunc = textFunc;
        }

        public void Build(IEnumerable<ItemT> list)
        {
            List = list.ToList();
            foreach (Control control in Controls.ToList())
            {
                control.LeftClick -= entry_Click;
                control.MouseWheel -= entry_MouseWheel;
                control.MouseRightUp -= entry_MouseRightUp;
                Controls.Remove(control);
            }
           // Controls.Clear();
            if (List == null)
                return;
            int h = 0;
            foreach (ItemT item in List)
            {
                ButtonType entry = new ButtonType();//new Vector2(0, Controls.Count * Label.DefaultHeight), TextFunc(item));
              //  entry.Location = new Vector2(0, h);
                entry.HoverText = HoverFunc != null ? HoverFunc(item) : "";
                entry.SetText(TextFunc(item)).SetLocation(new Vector2(0, h));
               // entry.SetLocation(new Vector2(0, h));
             //   entry.Text = TextFunc(item);
                entry.Width = Width;
                entry.LeftClick += new UIEvent(entry_Click);
               // entry.MouseLeftUp += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(entry_MouseLeftUp);
                //entry.MouseMove += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(entry_MouseMove);
                entry.MouseWheel += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(entry_MouseWheel);
                entry.MouseRightUp += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(entry_MouseRightUp);
               // entry.Tag = item;
                entry.Tag = item;//.SetTag(item);
                this.Controls.Add(entry);
                h += entry.Height;
            }
            //if (AutoSize)
            //    Size = ClientSize;
        }

        //void entry_MouseMove(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    if (!Parent.ScreenClientRectangle.Intersects(new Rectangle((int)UIManager.Mouse.X, (int)UIManager.Mouse.Y, 1, 1)))
        //        e.Handled = false;
        //}

        //void entry_MouseLeftUp(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    if (!Parent.ScreenClientRectangle.Intersects(new Rectangle((int)UIManager.Mouse.X, (int)UIManager.Mouse.Y, 1, 1)))
        //        e.Handled = false;
        //}

        public override void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.HasChildren)
                if (Parent.ScreenClientRectangle.Intersects(new Rectangle((int)UIManager.Mouse.X, (int)UIManager.Mouse.Y, 1, 1)))
                {
                    List<Control> controls = Controls.ToList();
                    controls.Reverse();
                    foreach (Control c in controls)
                        c.HandleMouseMove(e);
                }

            //if (HitTest(e))
            //{
            //    if (WindowManager.ActiveControl != null)
            //        return;

            //    WindowManager.ActiveControl = this;

            //    if (!MouseHover)
            //    {
            //        if (WindowManager.ActiveControl == this)
            //        {
            //            OnMouseEnter();
            //            MouseHover = true;
            //        }
            //    }
            //    else
            //        OnMouseMove(e);
            //}
            //else
            //{
            //    if (MouseHover)
            //        OnMouseLeave();
            //    MouseHover = false;
            //}

            //---------------------

            if (!HitTest(e))
            {
                if (MouseHover)
                    OnMouseLeave();
                MouseHover = false;
                return;
            }

            if (Controller.Instance.MouseoverNext.Object != this)
                return;

           // WindowManager.ActiveControl = this;

            if (!MouseHover)
            {
                //if (WindowManager.ActiveControl == this)
                //{
                    OnMouseEnter();
                    MouseHover = true;
                //}
            }
            else
                OnMouseMove(e);
        }

        public override void Dispose()
        {
            foreach (Control ctrl in Controls)
                ctrl.LeftClick -= entry_Click;
        }

        void entry_Click(object sender, EventArgs e)
        {
            //if (!Parent.ScreenClientRectangle.Intersects(new Rectangle((int)UIManager.Mouse.X, (int)UIManager.Mouse.Y, 1, 1)))
            //{
            //    return;
            //} 
            SelectedItem = (ItemT)(sender as ButtonType).Tag;
            OnSelectedItemChanged();
        }
        void entry_MouseRightUp(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            OnItemRightClick((sender as Control).Tag as ItemT);
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            //base.Draw(sb);
            // Scroll.Draw(sb);
          //  DrawHighlight(sb);
            foreach (ButtonType label in Controls.FindAll(foo => foo is ButtonType))
            {
                Rectangle finalRect;
                Rectangle labelRect = label.ScreenBounds;
                Rectangle panelRect = ScreenBounds;//Bounds;
                Rectangle.Intersect(ref labelRect, ref panelRect, out finalRect);
                //  panel.DrawHighlight(e.SpriteBatch, panel.ScreenClientRectangle, 0.5f);
                if (label.Tag != null)
                    if (label.Tag.Equals(SelectedItem))
                        label.DrawHighlight(sb, finalRect, 0.5f);
                //e.SpriteBatch.Draw(label.TextSprite, finalRect, Color.Lerp(Color.Transparent, Color.White, label.Opacity));
                Rectangle source = new Rectangle(0, finalRect.Y - labelRect.Y, finalRect.Width, finalRect.Height);
                label.OnDrawItem(new DrawItemEventArgs(sb, finalRect));
                bool isSelected = (label.Active && label.MouseHover) || label.Tag.Equals(SelectedItem);
                Vector2 pos = new Vector2(finalRect.X, finalRect.Y);
                label.DrawSprite(sb, finalRect, source, Color.Black, isSelected ? 1 : 0.5f);
             //   label.DrawSprite(sb, finalRect, source, Color.White, isSelected ? 1 : 0.5f);
                //if(isSelected)

                //Console.WriteLine(Controller.Instance.Mouseover);
                //label.DrawText(sb, Color.Lerp(Color.Transparent, label.Active && label.MouseHover ? Color.YellowGreen : Color.White, label.Opacity));
                label.DrawText(sb, pos, source, isSelected ? Color.Lime : Color.White, 1);//label.Tag == SelectedItem ? 1 : 0.5f); //yellowgreen
                //if (label.Active && label.MouseHover)
                if (HitTest(finalRect))
                    Controller.Instance.MouseoverNext.Object = label;
                //if (Control.HitTest(finalRect))
                //    Controller.Instance.MouseoverNext.Object = label;
                // sb.Draw(label.TextSprite, new Vector2(finalRect.X, finalRect.Y), source, Color.Lerp(Color.Transparent, label.Active && label.MouseHover ? Color.YellowGreen : Color.White, label.Opacity));
                //  panel.ScreenClientRectangle
            }
        }
        public int Step = 3 * Label.DefaultHeight;
        //public override void HandleInput(InputState input)
        //{
        //    if (UIManager.MouseRect.Intersects(Bounds))
        //    {
        //        if (input.CurrentMouseState.ScrollWheelValue != input.LastMouseState.ScrollWheelValue)
        //        {
        //            ClientLocation.Y = Math.Min(0, Math.Max(Height - ClientSize.Height, ClientLocation.Y + Step * (input.CurrentMouseState.ScrollWheelValue > input.LastMouseState.ScrollWheelValue ? 1 : -1)));
        //            input.Handled = true;
        //        }
        //        base.HandleInput(input);
        //    }
        //    else
        //        foreach (Control ctrl in Controls)
        //            // TODO: this is a workaround
        //            ctrl.MouseHover = false;
        //}
        //public override void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    if (UIManager.MouseRect.Intersects(Bounds))
        //    {
        //        if (e.CurrentMouseState.ScrollWheelValue != e.LastMouseState.ScrollWheelValue)
        //        {
        //            ClientLocation.Y = Math.Min(0, Math.Max(Height - ClientSize.Height, ClientLocation.Y + Step * (e.CurrentMouseState.ScrollWheelValue > e.LastMouseState.ScrollWheelValue ? 1 : -1)));
        //            e.Handled = true;
        //        }
        //        base.HandleMouseMove(e);
        //    }
        //    else
        //        foreach (Control ctrl in Controls)
        //            // TODO: this is a workaround
        //            ctrl.MouseHover = false;
        //}
        void entry_MouseWheel(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.HandleMouseWheel(e);
        }
        public override void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //e.Handled = true;
            ClientLocation.Y = Math.Min(0, Math.Max(Height - ClientSize.Height, ClientLocation.Y + Step * e.Delta));
        }

        //protected override void OnMouseWheelDown()
        //{
        // //   Scroll.PerformDown();
        //}

        //protected override void OnMouseWheelUp()
        //{
        // //   Scroll.PerformUp();
        //}

        //protected override void OnMouseScroll(InputState e)
        //{
        //    base.OnMouseScroll(e);
        //}
    }
}
