using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class Button : ButtonBase
    {
        public float HAlign = .5f;
        public static Rectangle
            SpriteLeft = new(0, 0, 4, 23),
            SpriteCenter = new(4, 0, 1, 23),
            SpriteRight = new(5, 0, 4, 23);
        public static int DefaultHeight = 23;
        public override int Padding { get => SpriteLeft.Width; }
        public override int Height
        {
            get
            {
                return DefaultHeight;
            }
            set
            {
                base.Height = value;
            }
        }
        public override int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                if (this.AutoSize)
                    base.Width = Math.Max((int)this.Font.MeasureString(Text).X + SpriteLeft.Width + SpriteRight.Width + 2, value);
                else
                    base.Width = value;
            }
        }

        //public static int MeasureWidth(int )

        public Color IdleColor = Color.White * 0.1f; //.5f
        Color _TexBackgroundColor;// = Color.Transparent;
        public Func<Color> TexBackgroundColorFunc;
        public virtual Color TexBackgroundColor
        {
            get
            {
                if (TexBackgroundColorFunc == null)
                    return _TexBackgroundColor;
                    //return this.IsPressed ? _TexBackgroundColor : _TexBackgroundColor * .2f;
                else
                    return TexBackgroundColorFunc();
            }
            set { 
                _TexBackgroundColor = value; 
                this.Invalidate(); 
            }
        }
        public override void Update()
        {
            //t += dt;
            if (this.TexBackgroundColorFunc != null)
            {
                var newCol = this.TexBackgroundColorFunc();
                if (newCol != this._TexBackgroundColor)
                {
                    this._TexBackgroundColor = newCol;
                    this.Invalidate();
                }
            }
            base.Update();
        }

        protected override void OnTextChanged()
        {
            this.Validate();
            return;
            base.OnTextChanged();
        }
        public override void Validate(bool cascade = false)
        {
            this.Text = this.TextFunc?.Invoke() ?? this.Text;
            base.Validate(cascade);
        }
        static public int GetWidth(Microsoft.Xna.Framework.Graphics.SpriteFont font, string txt)
        {
            
            var textw = (int)font.MeasureString(txt).X;
            var w = textw + SpriteLeft.Width + SpriteRight.Width + 2;// Math.Max((int)textw + 2, Width);
            return w;
        }

        public Button() : base() {
            TexBackgroundColorFunc = () =>
            {
                //Color c = this.Active ? this.Color : Color.Gray;// IdleColor;
                //float a = (this.MouseHover && this.Active) ? 1 : (this.Active ? 0.5f : 0.1f);
                //return c * a;

                return DefaultBackgroundColorFunc();

            };
            Height = UIManager.DefaultButtonHeight;
            Text = "";
            this.AutoSize = true;
        }//Opacity = 0.5f; }

        public Microsoft.Xna.Framework.Color DefaultBackgroundColorFunc()
        {
            if (this.IsPressed)
                return this.Color;
            return (this.MouseHover && this.Active) ? this.Color : IdleColor;
            

            //return (this.MouseHover && this.Active) ? this.Color : IdleColor;
        }
        public Button(int width) : this("", width) 
        { 
            this.Width = width;
            if (width > 0)
                this.AutoSize = false;
        }
        public Button(SpriteFont font, string text, int width = 0)
            : this()
        {
            this.Font = font;
            Text = text;
            //this.Width =  Math.Max(this.Width + SpriteLeft.Width + SpriteRight.Width + 2, width);
            this.Width = Math.Max(this.Width, width);

        }
        public Button(string text, Action action, int width = 0) :this(text, width)
        {
            this.LeftClickAction = action;
        }
        public Button(Func<string> text, Action action, int width = 0) : this(width)
        {
            this.TextFunc = text;
            this.LeftClickAction = action;
        }
        public Button(string text, int width = 0)
            : this()
        {
            Text = text;
            //this.Width =  Math.Max(this.Width + SpriteLeft.Width + SpriteRight.Width + 2, width);
            if (width > 0)
                this.AutoSize = false;
            this.Width = Math.Max(this.Width, width);
            
        }
        public Button(Vector2 location) : this() { this.Location = location; Text = ""; Height = UIManager.DefaultButtonHeight; }//Opacity = 0.5f; }
        public Button(Vector2 Location, int width, String label = "")
            : this(Location)
        {
            Text = label;

            //this.Width = width == 0 ? this.Width + 2 * UIManager.BorderPx : width;
            this.Width = Math.Max(this.Width + SpriteLeft.Width + SpriteRight.Width + 2, width);
            this.Height = UIManager.DefaultButtonHeight;

        }
        public Button(Vector2 Location, String label = "")
            : this(Location)
        {
            Text = label;

            this.Width += 2 * UIManager.BorderPx;
            this.Height = UIManager.DefaultButtonHeight;
        }
        public override void Initialize()
        {
            base.Initialize();
            Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);

        }

        public override void OnPaint(SpriteBatch sb)
        {
            Button.Draw(sb, this, Vector2.Zero, (MouseHover || IsPressed) ? 1 : 0.5f);
        }

        //protected override void OnIconChanged()
        //{
        //    Width = 45;
        //    Height = 45;
        //    base.OnIconChanged();
        //}

       
        public override void DrawSprite(SpriteBatch sb, Rectangle destRect, Rectangle? sourceRect, Color color, float opacity)
        {
            DrawSprite(sb, destRect, sourceRect, color, opacity, SprFx);
        }
        public override void DrawSprite(SpriteBatch sb, Rectangle destRect, Rectangle? sourceRect, Color color, float opacity, SpriteEffects sprFx)
        {
            SpriteEffects fx = Active ? sprFx : SpriteEffects.FlipVertically;
            Color c = Color.Lerp(Color.Transparent, color, opacity);
            sb.Draw(UIManager.defaultButtonSprite, new Vector2(destRect.X, destRect.Y), Rectangle.Intersect(Button.SpriteLeft, sourceRect.Value), c, 0, Vector2.Zero, 1, fx, Depth);
            sb.Draw(UIManager.defaultButtonSprite,
                new Rectangle(
                    destRect.X + Button.SpriteLeft.Width, 
                    destRect.Y, 
                    destRect.Width - Button.SpriteLeft.Width - Button.SpriteRight.Width, 
                    Rectangle.Intersect(Button.SpriteCenter, sourceRect.Value).Height), //UIManager.defaultButtonSprite.Height), 
                Rectangle.Intersect(Button.SpriteCenter, sourceRect.Value), c, 0, Vector2.Zero, fx, Depth);
            sb.Draw(UIManager.defaultButtonSprite, new Vector2(destRect.X + destRect.Width - Button.SpriteRight.Width, destRect.Y), Rectangle.Intersect(Button.SpriteRight, sourceRect.Value), c, 0, Vector2.Zero, 1, fx, Depth);
          //  sb.Draw(UIManager.defaultButtonSprite, new Rectangle(X + 4, Y, Width - 8, 23), Button.SpriteCenter, c, 0, new Vector2(0), SprFx, Depth);
          //  sb.Draw(UIManager.defaultButtonSprite, new Vector2(position.X + Width - 4, position.Y), Button.SpriteRight, c, 0, new Vector2(0), 1, SprFx, Depth);
        }
        public override void DrawSprite(SpriteBatch sb, Rectangle destRect, Rectangle? sourceRect, Rectangle viewport, Color color, float opacity, SpriteEffects sprFx)
        {
            SpriteEffects fx = Active ? sprFx : SpriteEffects.FlipVertically;
            Color c = Color.Lerp(Color.Transparent, color, opacity);

            Rectangle lb, ls, mb, ms, rb, rs;
            (new Rectangle(destRect.X, destRect.Y, Button.SpriteLeft.Width, Button.SpriteLeft.Height)).Clip(Button.SpriteLeft, viewport, out lb, out ls);
            (new Rectangle(destRect.X + Button.SpriteLeft.Width, destRect.Y, destRect.Width - Button.SpriteLeft.Width - Button.SpriteRight.Width, Button.SpriteCenter.Height)).Clip(Button.SpriteCenter, viewport, out mb, out ms);
            (new Rectangle(destRect.X + destRect.Width - Button.SpriteRight.Width, destRect.Y, Button.SpriteRight.Width, Button.SpriteRight.Height)).Clip(Button.SpriteRight, viewport, out rb, out rs);

            sb.Draw(UIManager.defaultButtonSprite, lb, ls, c, 0, Vector2.Zero, fx, Depth);
            sb.Draw(UIManager.defaultButtonSprite, mb, ms, c, 0, Vector2.Zero, fx, Depth);
            sb.Draw(UIManager.defaultButtonSprite, rb, rs, c, 0, Vector2.Zero, fx, Depth);
        }

        static public void Draw(SpriteBatch sb, Button button, Vector2 location, float opacity = 1)
        {
            DrawSprite(sb, button, new Rectangle((int)location.X, (int)location.Y, button.Width, button.Height),
                button.TexBackgroundColor,//button.Active ? button.Color:Color.Gray,
                opacity,//1,//(button.MouseHover && button.Active) ? 1 : (button.Active ? 0.5f : 0.1f), 
                button.SprFx);
            var halign = button.HAlign;
            var actualwidth = button.Width - Button.SpriteLeft.Width - Button.SpriteRight.Width;

            //var pos = new Vector2(Button.SpriteLeft.Width + actualwidth * halign, Button.SpriteCenter.Height / 2 + (button.Pressed ? 1 : 0));
            var pos = new Vector2(Button.SpriteLeft.Width + actualwidth * halign, Button.SpriteCenter.Height / 2 + (button.IsPressed ? 1 : 0));

            var origin = new Vector2(halign, .5f);
            UIManager.DrawStringOutlined(sb, button.Text, pos, origin, button.TextColor, button.TextOutline, button.Font);

        }

        static public void DrawSprite(SpriteBatch sb, Button button, Rectangle destRect, Color color, float opacity, SpriteEffects sprFx)
        {
            SpriteEffects fx = button.Active ? sprFx : SpriteEffects.FlipVertically;
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
        static public void DrawSprite(SpriteBatch sb, Vector2 location, int width, Color color, SpriteEffects sprFx)
        {
            sb.Draw(UIManager.defaultButtonSprite, location,  Button.SpriteLeft, color, 0, Vector2.Zero, 1, sprFx, 0);
            sb.Draw(UIManager.defaultButtonSprite,
                new Rectangle(
                    (int)location.X + Button.SpriteLeft.Width,
                    (int)location.Y,
                    width - Button.SpriteLeft.Width - Button.SpriteRight.Width,
                    SpriteCenter.Height),
                Button.SpriteCenter, color, 0, Vector2.Zero, sprFx, 0);
            sb.Draw(UIManager.defaultButtonSprite, new Vector2(location.X + width - Button.SpriteRight.Width, location.Y), Button.SpriteRight, color, 0, Vector2.Zero, 1, sprFx, 0);
        }
        public override void DrawText(SpriteBatch sb, Vector2 position, Rectangle? sourceRect, Color color, float opacity)
        {
            UIManager.DrawStringOutlined(sb, this.Text, position, this.Anchor);
            //if (!sourceRect.HasValue)
            //    sourceRect = TextSprite.Bounds;
            //sb.Draw(TextSprite, position + new Vector2((Width - TextSprite.Width) / 2, (Height - TextSprite.Height) / 2 + (Pressed ? 1 : 0)), Rectangle.Intersect(TextSprite.Bounds, sourceRect.Value), color * opacity, 0, Vector2.Zero, 1, SpriteEffects.None, Depth);
        }


        internal static int GetMaxWidth(IEnumerable<string> enumerable)
        {
            int max = 0;
            foreach (var item in enumerable)
            {
                var w = GetWidth(UIManager.Font, item);
                if (w > max)
                    max = w;
            }
            return max;
        }
    }
}
