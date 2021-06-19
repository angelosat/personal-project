using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class SuperDataView : Control
    {
        public event EventHandler SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }

        public enum Views { List };
        public Object SelectedItem;

        public Views View = Views.List;

        //public class SuperDataViewItem : Control
        //{
        //    string Name;
        //    int IconIndex;
        //}

        public void Add(string name, Object tag)
        {
            Control item = null;
            switch (View)
            {
                case Views.List:
                    item = new Label(new Vector2(0, Controls.Count * Label.DefaultHeight), name);
                    break;
                default:
                    break;
            }
            if (item == null)
                throw (new Exception("Something went horribly wrong."));
            item.Tag = tag;
            Controls.Add(item);
            item.LeftClick += new UIEvent(item_Click);
        }

        void item_Click(object sender, EventArgs e)
        {
            SelectedItem = ((Control)sender).Tag;
            OnSelectedItemChanged();
        }

        public SuperDataView()
        {
            AutoSize = true;
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            foreach (Control control in Controls)
            {
                if (WindowManager.ActiveControl == control)
                {
                    control.DrawHighlight(sb, 0.2f);
                    //HoverIndex = i;
                }
                control.Draw(sb);
            }
        }

        public void Clear()
        {
            foreach (Control item in Controls)
            {
                item.LeftClick -= item_Click;
            }
            Controls.Clear();
        }
    }
}
