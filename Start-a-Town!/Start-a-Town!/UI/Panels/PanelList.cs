using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class PanelList<ObjectT, ButtonType> : Panel where ButtonType : ButtonBase, new() where ObjectT:class
    {
        public ScrollableList<ObjectT, ButtonType> List;
        public ObjectT SelectedItem;
        ScrollbarV Scroll;
        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            SelectedItemChanged?.Invoke(this, EventArgs.Empty);
        }
        public event EventHandler<ListItemEventArgs<ObjectT>> ItemRightClick;
        void OnItemRightClick(ObjectT item)
        {
            ItemRightClick?.Invoke(this, new ListItemEventArgs<ObjectT>(item));
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
                List.Size = new Rectangle(0, 0, ClientSize.Width - ScrollbarV.Width, List.Size.Height);
                Controls.Add(Scroll);
                foreach (ButtonType btn in List.Controls)
                    btn.Width = ClientSize.Width - ScrollbarV.Width;
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
            this.List.BoundsScreen.DrawHighlight(sb);
            base.Draw(sb, this.BoundsScreen);
        }
    }
}
