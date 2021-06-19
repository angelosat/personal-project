using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class ButtonTogglable : Button
    {
        bool Toggled;
        public Func<bool> IsToggled;

        public ButtonTogglable(SpriteFont font, string text, int width = 0)
            : base(font, text, width)
        {
        }
        public ButtonTogglable(string text, int width = 0)
            : base(text, width)
        {
        }
        public override bool IsPressed
        {
            get
            {
                return Toggled;
            }
        }
        public override void Update()
        {
            var newToggled = IsToggled();
            if(this.Toggled != newToggled)
            {
                this.Invalidate();
            }
            this.Toggled = newToggled;
            base.Update();
        }
        public override Vector2 GetLocation()
        {
            return base.GetLocation();
        }
        public override void OnPaint(SpriteBatch sb)
        {
            Draw(sb, this, Vector2.Zero);
        }
        static public void Draw(SpriteBatch sb, ButtonTogglable button, Vector2 location)
        {
            DrawSprite(sb, button, new Rectangle((int)location.X, (int)location.Y, button.Width, button.Height),
                button.TexBackgroundColor,//button.Active ? button.Color:Color.Gray,
                1,//(button.MouseHover && button.Active) ? 1 : (button.Active ? 0.5f : 0.1f), 
                button.SprFx);
            //UIManager.DrawStringOutlined(sb, button.Text, new Vector2(1 + button.Width / 2, 1 + (button.Pressed ? 1 : 0)), new Vector2(0.5f, 0), button.TextColor, button.TextOutline, button.Font);
            var halign = button.HAlign;
            var actualwidth = button.Width - Button.SpriteLeft.Width - Button.SpriteRight.Width;
            //var pos = new Vector2(1 + Button.SpriteLeft.Width + actualwidth * halign, 1 + (button.Pressed ? 1 : 0));
            var pos = new Vector2(Button.SpriteLeft.Width + actualwidth * halign, 1 + (button.IsPressed ? 1 : 0));
            var origin = new Vector2(halign, 0);
            UIManager.DrawStringOutlined(sb, button.Text, pos, origin, button.TextColor, button.TextOutline, button.Font);
        }
        static public void DrawSprite(SpriteBatch sb, ButtonTogglable button, Rectangle destRect, Color color, float opacity, SpriteEffects sprFx)
        {
            SpriteEffects fx = button.IsPressed ? SpriteEffects.FlipVertically : SpriteEffects.None;
            Color c = Color.Lerp(Color.Transparent, color, opacity);
            sb.Draw(UIManager.defaultButtonSprite, new Vector2(destRect.X, destRect.Y), Button.SpriteLeft, c, 0, Vector2.Zero, 1, fx, 0);
            sb.Draw(UIManager.defaultButtonSprite,
                new Rectangle(
                    destRect.X + Button.SpriteLeft.Width,
                    destRect.Y,
                    destRect.Width - Button.SpriteLeft.Width - Button.SpriteRight.Width,
                    SpriteCenter.Height),
                Button.SpriteCenter, c, 0, Vector2.Zero, fx, 0);
            sb.Draw(UIManager.defaultButtonSprite, new Vector2(destRect.X + destRect.Width - Button.SpriteRight.Width, destRect.Y), Button.SpriteRight, c, 0, Vector2.Zero, 1, fx, 0);
        }
    }
}
