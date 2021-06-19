using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    public class DialogBlock : Control
    {
        static DialogBlock _Instance;
        static public DialogBlock Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new DialogBlock();
                return _Instance;
            }
        }
        DialogBlock() { }
        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            e.Handled = true;
            return; 
        }
        protected override void OnMouseLeftDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            e.Handled = true;
            return; 
        }
        protected override void OnMouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            e.Handled = true;
            return; 
        }
        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            e.Handled = true;
        }
        public override Rectangle BoundsScreen
        {
            get
            {
                return UIManager.Bounds;// new Rectangle(0, 0, Game1.Instance.graphics.PreferredBackBufferWidth, Game1.Instance.graphics.PreferredBackBufferHeight);
            }
        }

        //protected override void WindowManager_MouseLeftPress()
        //{
        //    //base.WindowManager_MouseLeftPress();
        //}

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            sb.Draw(WindowManager.DimScreen, viewport, Color.Lerp(Color.White, Color.Transparent, 0.5f));
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.Draw(sb);
            sb.Draw(WindowManager.DimScreen, Game1.Instance.GraphicsDevice.Viewport.Bounds, Color.Lerp(Color.White, Color.Transparent, 0.5f));
        }
    }
}
