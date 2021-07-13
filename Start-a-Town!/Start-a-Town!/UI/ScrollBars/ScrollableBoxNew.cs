using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class ScrollableBoxNew : Control
    {
        [Flags]
        public enum ScrollModes { None, Vertical, Horizontal, Both}

        readonly ScrollbarV VScroll;
        readonly ScrollbarH HScroll;
        public Control Client;

        public override void SetOpacity(float value, bool children, params Control[] exclude) 
        {
            base.SetOpacity(value, children, exclude);
            this.VScroll.SetOpacity(value, true);
            this.HScroll.SetOpacity(value, true);
        }
        public override void OnPaint(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.OnPaint(sb);
        }

        public override bool AutoSize
        {
            get
            {
                return base.AutoSize;
            }
            set
            {
                base.AutoSize = value;
                this.Client.AutoSize = value;
            }
        }

        public ScrollableBoxNew(Rectangle viewportBounds, ScrollModes mode = ScrollModes.Both)
            : base(new Vector2(viewportBounds.X, viewportBounds.Y), new Vector2(viewportBounds.Width, viewportBounds.Height))
        {
            var modeFactor = new IntVec2((mode & ScrollModes.Vertical) == ScrollModes.Vertical ? 1 : 0, (mode & ScrollModes.Horizontal) == ScrollModes.Horizontal ? 1 : 0);
            var buttonSize = 16;
            this.Client = new GroupBox() { Size = new Rectangle(0, 0, viewportBounds.Width - buttonSize*modeFactor.X, viewportBounds.Height - buttonSize * modeFactor.Y) };
            this.Client.AutoSize = false;
            this.VScroll = new ScrollbarV(new Vector2(this.Client.Width, 0), this.Client.Height, Client);
            this.HScroll = new ScrollbarH(new Vector2(0, this.Client.Height), this.Client.Width, Client);
            this.Size = new Rectangle(viewportBounds.X, viewportBounds.Y, viewportBounds.Width, viewportBounds.Height);
            Controls.Add(Client);
        }

        public virtual void Add(params Control[] controls)
        {
            foreach (var ctrl in controls)
                Client.Controls.Add(ctrl);
            Remeasure();
        }
        public void Remove(Control control)
        {
            Client.Controls.Remove(control);
            Remeasure();
        }

        public virtual void Remeasure()
        {
            var clientDesiredSize = Client.PreferredClientSize;
            var prefw = clientDesiredSize.Width;
            if (this.Width < prefw)
            {
                Controls.Add(HScroll);
                Client.Height = Height - 16;
            }
            else
            {
                Controls.Remove(HScroll);
                Client.Height = Height;
            }

            var prefh = clientDesiredSize.Height;
            if (this.Height < prefh)
            {
                Controls.Remove(VScroll);
                Controls.Add(VScroll);
                Client.Width = Width - 16;
            }
            else
            {
                Controls.Remove(VScroll);
                Client.Width = Width;
            }
            HScroll.Width = Client.Width;
            VScroll.Height = Client.Height;
        }
        public virtual void UpdateScrollbars()
        {
            var prefsize = Client.PreferredClientSize;
            var prefw = prefsize.Width;
            if (this.Client.Width < prefw)
            {
                if(!this.Controls.Contains(HScroll))
                    Controls.Add(HScroll);
            }
            else
            {
                Controls.Remove(HScroll);
            }

            var prefh = prefsize.Height;
            if (this.Client.Height < prefh)
            {
                if(!this.Controls.Contains(VScroll))
                    Controls.Add(VScroll);
            }
            else
            {
                Controls.Remove(VScroll);
            }
            HScroll.Width = Client.Width;
            VScroll.Height = Client.Height;
        }

        public override void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.HitTest())
                return;

            int step = UIManager.LineHeight;
            Client.ClientLocation.Y = Math.Min(0, Math.Max(Client.Height - Client.ClientSize.Height, Client.ClientLocation.Y + step * e.Delta));
            e.Handled = true;
        }

        public override void AlignTopToBottom()
        {
            this.Client.Controls.AlignVertically();
            this.Client.ClientSize = this.Client.PreferredClientSize;
            this.Client.ConformToClientSize();
            this.Remeasure();
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }

        public void UpdateClientSize()
        {
            this.Client.ClientSize = this.Client.PreferredClientSize;
            this.EnsureClientWithinBounds(); // should i do this here?
            this.UpdateScrollbars(); // should i do this here?
        }

        protected void EnsureLocationVisible(float lowestVisiblePoint)
        {
            this.Client.ClientLocation.Y = Math.Min(this.Client.ClientLocation.Y, -lowestVisiblePoint + this.Client.Size.Height);
        }
        protected void EnsureLocationVisible(int lowestVisiblePoint)
        {
            this.Client.ClientLocation.Y = Math.Min(this.Client.ClientLocation.Y, -lowestVisiblePoint + this.Client.Size.Height);
        }
        protected void EnsureClientWithinBounds()
        {
            this.Client.ClientLocation.Y = Math.Max(this.Client.ClientLocation.Y, Math.Min(0, this.Client.Size.Height - this.Client.ClientSize.Height));
        }
    }
}
