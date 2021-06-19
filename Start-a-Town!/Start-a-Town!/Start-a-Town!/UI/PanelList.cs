using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class PanelList<ObjectType> : Panel
    {
       // HorizontalAlignment HorizontalAlignment;
        public ScrollableList<ObjectType> List;
        public ObjectType SelectedItem { get { return List.SelectedItem; } set { List.SelectedItem = value; } }
     //   Func<ObjectType, string> TextFunction, HoverTextSelector;
        VScrollbar Scroll;
        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }
        public PanelList(Vector2 position, Vector2 size, Func<ObjectType, string> textFunc, HorizontalAlignment hAlign = HorizontalAlignment.Center)
            : base(position, size)
        {
            List = new ScrollableList<ObjectType>(Vector2.Zero, ClientSize, textFunc, hAlign: hAlign);
            List.ControlAdded += new EventHandler<EventArgs>(List_ControlAdded);
            List.ControlRemoved += new EventHandler<EventArgs>(List_ControlRemoved);
            List.SelectedItemChanged += new EventHandler<EventArgs>(List_SelectedItemChanged);
            Scroll = new VScrollbar(new Vector2(ClientSize.Width - VScrollbar.Width, 0), ClientSize.Height, List);
      //      this.TextFunction = textFunc;

            Controls.Add(List);
        }

        private void Remeasure()
        {
            int height = 0;
            foreach (Control control in List.Controls)
                height += control.Height;
            List.ClientSize = new Rectangle(0, 0, List.Size.Width, height);
        }

        //public void Build(IEnumerable<ObjectType> list, Func<ObjectType, string> textSelector, Func<ObjectType, string> hoverTextSelector)
        //{
        //    Build(list, textSelector);
        //    this.HoverTextSelector = hoverTextSelector;
        //}
        //public void Build(IEnumerable<ObjectType> list, Func<ObjectType, string> textSelector, Func<ObjectType, string> hoverTextSelector = null)
        //{
        //    Build(list);
        //    this.TextFunction = textSelector;
        //    this.HoverTextSelector = hoverTextSelector;
        //}
        public void Build(IEnumerable<ObjectType> list)
        {
            Controls.Remove(List);
            List.Build(list);
            Controls.Add(List);
            List.ClientLocation = Vector2.Zero;
            SelectedItem = default(ObjectType);
        }

        public void Build()
        {
            Build(new List<ObjectType>());
        }

        void List_SelectedItemChanged(object sender, EventArgs e)
        {
            SelectedItem = List.SelectedItem;
            OnSelectedItemChanged();
        }

        void List_ControlRemoved(object sender, EventArgs e)
        {
            Remeasure();
            if (List.ClientSize.Height <= ClientSize.Height)
            {
                List.Size = new Rectangle(0, 0, ClientSize.Width, List.Size.Height);
                Controls.Remove(Scroll);
                foreach (ButtonBase btn in List.Controls)
                    btn.Width = ClientSize.Width;
            }
        }


        void List_ControlAdded(object sender, EventArgs e)
        {
            Remeasure();
            if (List.ClientSize.Height > ClientSize.Height)
            {
                List.Size = new Rectangle(0, 0, ClientSize.Width - VScrollbar.Width, List.Size.Height);
                Controls.Add(Scroll);
                foreach (ButtonBase btn in List.Controls)
                    btn.Width = ClientSize.Width - VScrollbar.Width;
            }
        }

        public override void Dispose()
        {
            List.ControlAdded -= List_ControlAdded;
            List.ControlRemoved -= List_ControlRemoved;
            List.SelectedItemChanged -= List_SelectedItemChanged;
            base.Dispose();
        }

    }

    class PanelList<ObjectT, ButtonType> : Panel where ButtonType : ButtonBase, new() where ObjectT:class
    {
        public ScrollableList<ObjectT, ButtonType> List;
        public ObjectT SelectedItem;
        Func<ObjectT, string> TextFunction;
        VScrollbar Scroll;
        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }
        public event EventHandler<ListItemEventArgs<ObjectT>> ItemRightClick;
        void OnItemRightClick(ObjectT item)
        {
            if (ItemRightClick != null)
                ItemRightClick(this, new ListItemEventArgs<ObjectT>(item));
        }

        public PanelList(Vector2 position, Rectangle size, Func<ObjectT, string> textFunc) : this(position, new Vector2(size.Width, size.Height), textFunc) { }
        public PanelList(Vector2 position, Vector2 size, Func<ObjectT, string> textFunc)
            : base(position, size)
        {
            List = new ScrollableList<ObjectT, ButtonType>(Vector2.Zero, ClientSize, textFunc);
            List.ControlAdded += new EventHandler<EventArgs>(List_ControlAdded);
            List.ControlRemoved += new EventHandler<EventArgs>(List_ControlRemoved);
            List.SelectedItemChanged += new EventHandler<EventArgs>(List_SelectedItemChanged);
            List.ItemRightClick += new EventHandler<ListItemEventArgs<ObjectT>>(List_ItemRightClick);
            Scroll = new VScrollbar(new Vector2(ClientSize.Width - VScrollbar.Width, 0), ClientSize.Height, List);
            this.TextFunction = textFunc;

            Controls.Add(List);
        }

        public void Build(IEnumerable<ObjectT> list)
        {
            Controls.Remove(List);
            List.Build(list);
            Controls.Add(List);
            this.Scroll.Reset();
            this.List.ClientLocation.Y = 0;
        }

        void List_SelectedItemChanged(object sender, EventArgs e)
        {
            SelectedItem = List.SelectedItem;
            OnSelectedItemChanged();
        }

        void List_ItemRightClick(object sender, ListItemEventArgs<ObjectT> e)
        {
            OnItemRightClick(e.Item);
        }

        void List_ControlRemoved(object sender, EventArgs e)
        {
            //int height = 0;
            //foreach (Control control in Controls)
            //    height += control.Height;
            //List.ClientSize = new Rectangle(0, 0, List.Size.Width, height);
            Remeasure();
            if (List.ClientSize.Height <= ClientSize.Height)
            {
                List.Size = new Rectangle(0, 0, ClientSize.Width, List.Size.Height);
                Controls.Remove(Scroll);
                foreach (ButtonType btn in List.Controls)
                    btn.Width = ClientSize.Width;
            }
        }



        void List_ControlAdded(object sender, EventArgs e)
        {
            Remeasure();
            if (List.ClientSize.Height > ClientSize.Height)
            {
                List.Size = new Rectangle(0, 0, ClientSize.Width - VScrollbar.Width, List.Size.Height);
                Controls.Add(Scroll);
                foreach (ButtonType btn in List.Controls)
                    btn.Width = ClientSize.Width - VScrollbar.Width;
            }
        }

        //private void Remeasure()
        //{
        //    int height = 0;
        //    foreach (Control control in List.Controls)
        //        height += control.Height;
        //    List.ClientSize = new Rectangle(0, 0, List.Size.Width, height);
        //}

        private void Remeasure()
        {
            if (this.Height < List.PreferredClientSize.Height)
            {
                Controls.Add(Scroll);
                List.Width = ClientSize.Width - 16;
            }
            else
            {
                Controls.Remove(Scroll);
                List.Width = ClientSize.Width;
            }
            Scroll.Height = List.Height;
        }

        public override void Dispose()
        {
            List.ControlAdded -= List_ControlAdded;
            List.ControlRemoved -= List_ControlRemoved;
            List.SelectedItemChanged -= List_SelectedItemChanged;
            List.ItemRightClick -= List_ItemRightClick;
            base.Dispose();
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            this.List.ScreenBounds.DrawHighlight(sb);
            base.Draw(sb, this.ScreenBounds);
        }
    }
}
