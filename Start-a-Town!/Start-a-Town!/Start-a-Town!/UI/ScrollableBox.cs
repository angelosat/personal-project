using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class ScrollableBox : Control
    {
        VScrollbar VScroll;
        HScrollbar HScroll;
        public Control Client;

        public override void SetOpacity(float value, bool children, params Control[] exclude) //[System.Runtime.InteropServices.OptionalAttribute][System.Runtime.InteropServices.DefaultParameterValueAttribute(false)]
        {
            base.SetOpacity(value, children, exclude);
            this.VScroll.SetOpacity(value, true);
            this.HScroll.SetOpacity(value, true);
        }

        public override void OnPaint(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.OnPaint(sb);
        }



        public ScrollableBox(Rectangle bounds)
            : base(new Vector2(bounds.X, bounds.Y), new Vector2(bounds.Width, bounds.Height))
        {
            Client = new GroupBox() { Size = this.Size};//, MouseThrough = true };
            this.VScroll = new VScrollbar(new Vector2(bounds.Width - 16, 0), bounds.Height, Client);
            this.HScroll = new HScrollbar(new Vector2(0, bounds.Height - 16), bounds.Width, Client);

            Controls.Add(Client);
           // MouseThrough = true;
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

        //internal override void OnControlAdded(Control control)
        //{
        //    base.OnControlAdded(control);
        //    Remeasure();
        //}
        //internal override void OnControlRemoved(Control control)
        //{
        //    base.OnControlRemoved(control);
        //    Remeasure();
        //}

        public virtual void Remeasure()
        {
            var prefw = Client.PreferredClientSize.Width;
            if (this.Width < prefw)
            {
                Controls.Add(HScroll);
             //   Viewport.Height = Height - 16;
                Client.Height = Height - 16;
            }
            else
            {
                Controls.Remove(HScroll);
             //   Viewport.Height = Height;
                Client.Height = Height;
            }

            var prefh = Client.PreferredClientSize.Height;
            if (this.Height < prefh)
            {
                Controls.Add(VScroll);
             //   Viewport.Width = Width - 16;
                Client.Width = Width - 16;
            }
            else
            {
                Controls.Remove(VScroll);
             //   Viewport.Width = Width;
                Client.Width = Width;
            }
            HScroll.Width = Client.Width;
      //      HScroll.SetOpacity(this.Opacity, true);
            VScroll.Height = Client.Height;
         //   VScroll.SetOpacity(this.Opacity, true);
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
            this.Client.Controls.AlignTopToBottom();
            this.Client.ClientSize = this.Client.PreferredClientSize;
            this.Client.ResizeToClientSize();
            this.Remeasure();
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            //this.Client.ScreenClientRectangle.DrawHighlight(sb);
            base.Draw(sb, viewport);
        }
    }
}
