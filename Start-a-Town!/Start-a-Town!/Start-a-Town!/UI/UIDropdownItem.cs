using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class UIDropdownItem : Button
    {
        public UIDropdownItem(int id, Vector2 Location, int width, string label)
            : base(Location, width, label)
        {
        }

        public override bool HitTest()
        {
            ListBox list = Parent as ListBox;
            if (list.HitTest())
                return Bounds.Intersects(new Rectangle(Controller.Instance.msCurrent.X, Controller.Instance.msCurrent.Y + (Parent as ListBox).BoxY, 1, 1));
            return false;
        }

        public override void Draw(SpriteBatch sb)//, RenderTarget2D box)
        {
            //sb.Draw(UIManager.defaultButtonSprite, ScreenLocation, UIButton.Left, tint, 0, new Vector2(0), 1, sprFx, Depth);
            //sb.Draw(UIManager.defaultButtonSprite, new Rectangle(X + 4, Y, Width - 8, 23), UIButton.Center, tint, 0, new Vector2(0), sprFx, Depth);
            //sb.Draw(UIManager.defaultButtonSprite, new Vector2(X + Width - 4, Y), UIButton.Right, tint, 0, new Vector2(0), 1, sprFx, Depth);

            //sb.Draw(labelSprite, ScreenLocation + new Vector2(Width / 2, Convert.ToInt32(isPressed) + Height / 2), null, Color.White, 0, new Vector2((labelSprite.Width) / 2, (labelSprite.Height) / 2), 1, SpriteEffects.None, Depth);
            
            // draw to rendertarget Parent.ItemsBox

            sb.Draw(UIManager.defaultButtonSprite, Location, Button.SpriteLeft, Alpha, 0, new Vector2(0), 1, SprFx, Depth);
            sb.Draw(UIManager.defaultButtonSprite, new Rectangle((int)Location.X + 4, (int)Location.Y, Width - 8, 23), Button.SpriteCenter, Alpha, 0, new Vector2(0), SprFx, Depth);
            sb.Draw(UIManager.defaultButtonSprite, new Vector2(Location.X + Width - 4, Location.Y), Button.SpriteRight, Alpha, 0, new Vector2(0), 1, SprFx, Depth);
            //sb.Draw(UIManager.defaultButtonSprite, Location, Color.White);

            sb.Draw(TextSprite, Location + new Vector2(Width / 2, Convert.ToInt32(isPressed) + Height / 2), null, Color.White, 0, new Vector2((TextSprite.Width) / 2, (TextSprite.Height) / 2), 1, SpriteEffects.None, Depth);
        }
    }
}
