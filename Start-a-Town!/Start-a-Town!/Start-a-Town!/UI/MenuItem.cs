using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class MenuItem : Button
    {
        protected Vector2 Origin = new Vector2(-UIManager.BorderPx, 0);
        public MenuItem(Menu parent, Vector2 Location, string label, TextAlignment halign)
            : base(parent, Location, label)
        {
            switch (halign)
            {
                case TextAlignment.Center:
                    Origin.X += Width / 2;
                    break;
                case TextAlignment.Right:
                    Origin.X += Width;
                    break;
                default:
                    break;
            }
        }
        public MenuItem(Menu parent, Vector2 Location, string label)
            : base(parent, Location, label)
        { }

        public override void Update()
        {
            Width = Parent.ClientSize.Width;
            base.Update();
            //Console.WriteLine(MouseHover.ToString());
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(UIManager.defaultButtonSprite, ScreenLocation, Button.SpriteLeft, Alpha, 0, new Vector2(0), 1, SprFx, Depth);
            sb.Draw(UIManager.defaultButtonSprite, new Rectangle(X + 4, Y, Width - 8, 23), Button.SpriteCenter, Alpha, 0, new Vector2(0), SprFx, Depth);
            sb.Draw(UIManager.defaultButtonSprite, new Vector2(X + Width - 4, Y), Button.SpriteRight, Alpha, 0, new Vector2(0), 1, SprFx, Depth);

            sb.Draw(TextSprite, ScreenLocation, null, Color.White, 0, Origin, 1, SpriteEffects.None, Depth);
        }
    }
}
