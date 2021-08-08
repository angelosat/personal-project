using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class ButtonNew : ButtonBase
    {
        public float HAlign = .5f;
      
        public override int Height
        {
            get
            {
                return this.BackgroundStyle.Left.Height;
            }
            set
            {
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
                    base.Width = Math.Max((int)this.Font.MeasureString(Text).X + this.BackgroundStyle.Left.Width + this.BackgroundStyle.Right.Width + 2, value);
                else
                    base.Width = Math.Max(base.Width, value);
            }
        }
        public SpriteEffects SpriteEffects => SpriteEffects.None;
        public override Vector2 ScreenLocation => base.ScreenLocation + Vector2.UnitY * (this.Pressed ? 1 : 0);

        public Color IdleColor = Color.White * 0.1f;
        Color _TexBackgroundColor;
        public Func<Color> TexBackgroundColorFunc;
        public virtual Color TexBackgroundColor
        {
            get
            {
                if (TexBackgroundColorFunc == null)
                    return _TexBackgroundColor;
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
            base.OnTextChanged();
        }

        public ButtonNew() : base() {
            TexBackgroundColorFunc = () =>
            {
                return DefaultBackgroundColorFunc();
            };
            this.BackgroundStyle = UI.BackgroundStyle.LargeButton;
            this.Height = this.BackgroundStyle.Left.Height;
            Text = "";
            this.AutoSize = true;
        }

        public Color DefaultBackgroundColorFunc()
        {
            if (this.IsPressed)
                return this.Color;
            return (this.MouseHover && this.Active) ? this.Color : IdleColor;
        }
        public ButtonNew(int width) : this("", width) 
        {
            this.AutoSize = false;
            this.Width = width;
        }
       
        public ButtonNew(string text, int width = 0)
            : this()
        {
            Text = text;
            this.Width = Math.Max(this.Width, width);
        }
        public ButtonNew(Vector2 location) : this() { this.Location = location; Text = ""; Height = UIManager.DefaultButtonHeight; }
        public override void Initialize()
        {
            base.Initialize();
            Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
        }
        public override void OnPaint(SpriteBatch sb)
        {
            ButtonNew.Draw(sb, this, Vector2.Zero, (MouseHover || IsPressed) ? 1 : 0.5f);
        }
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
                    Rectangle.Intersect(Button.SpriteCenter, sourceRect.Value).Height), 
                Rectangle.Intersect(Button.SpriteCenter, sourceRect.Value), c, 0, Vector2.Zero, fx, Depth);
            sb.Draw(UIManager.defaultButtonSprite, new Vector2(destRect.X + destRect.Width - Button.SpriteRight.Width, destRect.Y), Rectangle.Intersect(Button.SpriteRight, sourceRect.Value), c, 0, Vector2.Zero, 1, fx, Depth);
        }
       
        static public void Draw(SpriteBatch sb, ButtonNew button, Vector2 location, float opacity = 1)
        {
            button.BackgroundStyle.Draw(sb, button.Size, button.TexBackgroundColor, button.SpriteEffects);
            var halign = button.HAlign;
            var actualwidth = button.Width - Button.SpriteLeft.Width - Button.SpriteRight.Width;
            var pos = new Vector2(Button.SpriteLeft.Width + actualwidth * halign, Button.SpriteCenter.Height / 2 + (button.Pressed ? 1 : 0));
            var origin = new Vector2(halign, .5f);
            UIManager.DrawStringOutlined(sb, button.Text, pos, origin, button.TextColor, button.TextOutline, button.Font);
        }
        
        public override void DrawText(SpriteBatch sb, Vector2 position, Rectangle? sourceRect, Color color, float opacity)
        {
            UIManager.DrawStringOutlined(sb, this.Text, position, this.Anchor);
        }

        static public ButtonNew CreateBig(Action leftClickAction, int width, Texture2D graphic, Func<string> textTop, Func<string> textBottom = null)
        {
            var btn = new ButtonNew(width)
            {
                AutoSize = false,
                BackgroundStyle = BackgroundStyle.LargeButton,
                LeftClickAction = leftClickAction
            };
            btn.Width = width;
            var padding = btn.BackgroundStyle.Left.Width;
            var picbox = new PictureBox(graphic) { MouseThrough = true, Location = new Vector2(padding, btn.Height / 2), Anchor = new Vector2(0, .5f) };//) { DrawAction = () => block.PaintIcon(Block.Width, Block.Height, variant.Data) });
            var label = new Label() { TextFunc = textTop, Location = picbox.TopRight + Vector2.UnitX * padding, MouseThrough = true };

            btn.AddControls(picbox, label);
            if(textBottom != null)
                btn.AddControls( new Label() { TextFunc = textBottom, Location = picbox.BottomRight + Vector2.UnitX * padding, Anchor = Vector2.UnitY, MouseThrough = true });

            return btn;
        }
        static public ButtonNew CreateBig(Action leftClickAction, int width, Func<string> textTop, Func<string> textBottom = null)
        {
            var btn = new ButtonNew(width)
            {
                AutoSize = false,
                BackgroundStyle = BackgroundStyle.LargeButton,
                LeftClickAction = leftClickAction
            };
            btn.Width = width;
            var padding = btn.BackgroundStyle.Left.Width;
            var label = new Label() { TextFunc = textTop, Location = Vector2.One * padding, MouseThrough = true };

            btn.AddControls(label);
            if (textBottom != null)
                btn.AddControls(new Label() { TextFunc = textBottom, Location = new Vector2(padding, btn.Height - padding), Anchor = Vector2.UnitY, MouseThrough = true });

            return btn;
        }
    }
}
