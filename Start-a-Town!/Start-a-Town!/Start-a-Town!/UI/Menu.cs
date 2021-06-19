using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public abstract class Menu : Control
    {
        protected List<MenuItem> Items = new List<MenuItem>();

        public Menu()
        { }
        public Menu(Control parent)
            : base(parent)
        { }
        public Menu(Control parent, Vector2 location)
            : base(parent, location)
        { }
        public Menu(Control parent, Vector2 location, Vector2 size)
            : base(parent, location, size)
        { }
        //public override void Initialize()
        //{
        //    //Controls = new List<Control>();
        //    base.Initialize();
        //}
        public virtual void Add(MenuItem item)
        {
            Controls.Add(item);
            Items.Add(item);
        }

        public virtual void Show()
        {
            WindowManager.Windows.Add(this);
        }
        public virtual void Hide()
        {
            WindowManager.Windows.Remove(this);
        }

        public override void Update()
        {
            base.Update();
            foreach (Control c in Controls)
                c.Update();
        }

        public override Rectangle Size
        {
            get
            {
                return base.Size;
            }
            set
            {
                //base.Size = value;
                ClientSize = value;
                base.Size = new Rectangle(0, 0, value.Width + 2 * UIManager.BorderPx, value.Height + 2 * UIManager.BorderPx);
            }
        }

        public int MaxItemWidth
        {
            get
            {
                int width = 0;
                foreach (MenuItem item in Items)
                    width = Math.Max(width, item.Width);
                return width;
            }
        }
        public Rectangle PreferredSize
        {
            get
            {
                int h = 0;
                foreach (MenuItem item in Items)
                    h += item.Height;
                return new Rectangle(0, 0, MaxItemWidth, h);
            }
        }

        public override void Draw(SpriteBatch sb)
        {
            foreach (Control c in Controls)
                c.Draw(sb);
        }
    }
}
