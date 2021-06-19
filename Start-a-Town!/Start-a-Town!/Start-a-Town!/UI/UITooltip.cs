using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class UITooltip
    {
        public string Text;
        private Texture2D textSprite;
        public static Vector2 Location = new Vector2(10);
        public Rectangle Bbox;
        public float Depth = 0.001f;
        public static int maxWidth = 100;

        public int X
        {
            get { return (int)Bbox.X; }
            set { Bbox.X = value; }
        }
        public int Y
        {
            get { return (int)Bbox.Y; }
            set { Bbox.Y = value; }
        }
        public int W
        {
            get { return (int)Bbox.Width; }
            set { Bbox.Width = value; }
        }
        public int H
        {
            get { return (int)Bbox.Height; }
            set { Bbox.Height = value; }
        }

        public UITooltip(string text)
        {            
            //UIManager.Tooltip = this;

            Text = UIManager.WrapText(text, maxWidth);
            textSprite = UIManager.DrawTextOutlined(Text, Color.Black, Color.White);

            X = (int)(Controller.Instance.msCurrent.X + Location.X);
            Y = (int)(Controller.Instance.msCurrent.Y + Location.Y);
            W = textSprite.Width + 2 * UIManager.BorderPx;
            H = textSprite.Height + 2 * UIManager.BorderPx;
        }

        public void Update()
        {
            X = (int)(Controller.Instance.msCurrent.X + Location.X);
            Y = (int)(Controller.Instance.msCurrent.Y + Location.Y);
        }
        public void Draw(SpriteBatch sb)
        {
            // corners: tl, tr, bl, br
            sb.Draw(UIManager.frameSprite, new Vector2(X, Y), new Rectangle(0, 0, 11, 11), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, Depth);
            sb.Draw(UIManager.frameSprite, new Vector2(X - 11 + W, Y), new Rectangle(12, 0, 11, 11), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, Depth);
            sb.Draw(UIManager.frameSprite, new Vector2(X, Y - 11 + H), new Rectangle(0, 12, 11, 11), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, Depth);
            sb.Draw(UIManager.frameSprite, new Vector2(X - 11 + W, Y - 11 + H), new Rectangle(12, 12, 11, 11), Color.White, 0, new Vector2(0, 0), 1, SpriteEffects.None, Depth);

            //top, left, right, bottom
            sb.Draw(UIManager.frameSprite, new Rectangle(X + 11, Y, W - 22, 11), new Rectangle(12, 0, 1, 11), Color.White, 0, new Vector2(0, 0), SpriteEffects.None, Depth);
            sb.Draw(UIManager.frameSprite, new Rectangle(X, Y + 11, 11, H - 22), new Rectangle(0, 12, 11, 1), Color.White, 0, new Vector2(0, 0), SpriteEffects.None, Depth);
            sb.Draw(UIManager.frameSprite, new Rectangle(X + W - 11, Y + 11, 11, H - 22), new Rectangle(12, 11, 11, 1), Color.White, 0, new Vector2(0, 0), SpriteEffects.None, Depth);
            sb.Draw(UIManager.frameSprite, new Rectangle(X + 11, Y - 11 + H, W - 22, 11), new Rectangle(11, 12, 1, 11), Color.White, 0, new Vector2(0, 0), SpriteEffects.None, Depth);

            //center
            sb.Draw(UIManager.frameSprite, new Rectangle(X + 11, Y + 11, W - 22, H - 22), new Rectangle(11, 11, 1, 1), Color.White, 0, new Vector2(0, 0), SpriteEffects.None, Depth);

            //sb.DrawString(UIManager.Font, Text, new Vector2(X, Y) + Location, Color.Black);
            sb.Draw(textSprite, new Vector2(X + UIManager.BorderPx, Y + UIManager.BorderPx) + Location, null, Color.White, 0, Location, 1, SpriteEffects.None, Depth / 2f);
        }
    }
}
