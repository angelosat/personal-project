using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class UICloseButton : IconButton
    {
        public UICloseButton()
            : base()
        {
            this.BackgroundTexture = UIManager.Icon16Background;
            this.Icon = new Icon(UIManager.Icons16x16, 0, 16);
            HoverText = "Close";
        }

        public override void OnPaint(SpriteBatch sb)
        {
            base.OnPaint(sb);
        }

        void Parent_SizeChanged(object sender, EventArgs e)
        {
            Location = new Vector2(Parent.Width - 16 - UIManager.BorderPx - Parent.ClientLocation.X, UIManager.BorderPx - Parent.ClientLocation.Y);
        }

        protected override void OnParentChanged()
        {
            if (Parent != null)
                Parent.SizeChanged -= Parent_SizeChanged;
            Parent.SizeChanged+=new EventHandler<EventArgs>(Parent_SizeChanged);
            base.OnParentChanged();
        }

        public override void Dispose()
        {
            Parent.SizeChanged -= Parent_SizeChanged;
            base.Dispose();
        }
    }
}
