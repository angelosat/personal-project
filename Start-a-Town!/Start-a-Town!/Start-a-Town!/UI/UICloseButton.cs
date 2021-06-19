using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class UICloseButton : IconButton// ButtonBase// Control
    {
        public UICloseButton()
            : base()
        {
            //Location = new Vector2(Parent.Width - 16 - UIManager.BorderPx - Parent.ClientLocation.X, UIManager.BorderPx - Parent.ClientLocation.Y);
            this.BackgroundTexture = UIManager.Icon16Background;// Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/gui-x");
            this.Icon = new Icon(UIManager.Icons16x16, 0, 16);
            //Width = BackgroundTexture.Width;
            //Height = BackgroundTexture.Height;
            HoverText = "Close";
            //Parent.SizeChanged += new UIEvent(Parent_SizeChanged);
        }

        public override void OnPaint(SpriteBatch sb)
        {
            base.OnPaint(sb);
        }

        void Parent_SizeChanged(object sender, EventArgs e)
        {
            Location = new Vector2(Parent.Width - 16 - UIManager.BorderPx - Parent.ClientLocation.X, UIManager.BorderPx - Parent.ClientLocation.Y);
            //Console.WriteLine(Location.ToString());
        }

        //public override void Draw(SpriteBatch sb)
        //{
        //    //Console.WriteLine(Location.ToString());
        //    sb.Draw(Background, ScreenLocation, Color.White);
        //    if(Focused)
        //        sb.Draw(Background, ScreenLocation, new Color(255, 255, 255, 127));
        //}

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
