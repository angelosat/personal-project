using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.UI
{   
    class MenuStrip : GroupBox
    {
        public class MenuSripItemCollection : List<MenuStripItem>
        {
            MenuStrip Parent;
            public MenuSripItemCollection(MenuStrip parent)
            {
                this.Parent = parent;
            }
            public void Add(params MenuStripItem[] items)
            {
                foreach (var item in items)
                {
                    base.Add(item);
                    Parent.Controls.Add(item);
             //       Parent.OnItemAdded(item);
                }
            }
        }

        public MenuSripItemCollection Items;

        public bool IsOpen;
        public void Open()
        {
            IsOpen = true;
        }
        public void Activate(MenuStripItem item)
        {
            //if (!IsOpen)
            //    return;
            var items = this.Items.Except(new MenuStripItem[] { item }).ToList();
            foreach (var other in items)
            {
                //Control control = other.Tag as Control;
                // if (!other.Dropdown.IsNull())
                if (other.Dropdown.Controls.Count > 0)
                    other.Dropdown.Hide();
            }
            // if (!item.Dropdown.IsNull())
            if (item.Dropdown.Controls.Count > 0)
                item.Dropdown.Show();
        }

        public MenuStrip()
        {
            this.Items = new MenuSripItemCollection(this);
        }

        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            IsOpen = false;
            foreach (var item in this.Items)
            {
                if (item.Dropdown.Controls.Count > 0)//IsNull())
                    item.Dropdown.Hide();
            }
            base.HandleLButtonUp(e);
        }
    }
}
