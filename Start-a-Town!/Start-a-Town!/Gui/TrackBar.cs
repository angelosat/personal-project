using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class TrackBar : Control
    {
        public static int DefaultHeight = UIManager.DefaultTrackBarThumbSprite.Height;
        protected int _Value = 0;
        public int Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                OnValueChanged();
                ThumbPosition = (int)((Value / (float)Maximum) * (ThumbMax));
                Validate();
            }
        }
        public int Minimum { get; set; }
        protected int _Maximum = 1;
        public int Maximum
        { get { return _Maximum; } set { _Maximum = value; Validate(); } }
        public int TickFrequency { get; set; }
        protected int _LargeChange = 1;
        public int LargeChange
        {
            get { return _LargeChange; }
            set { _LargeChange = value; Validate(); }
        }

        int ThumbPosition = 0, 
            ThumbMax;

        public event EventHandler<EventArgs> ValueChanged;
        protected void OnValueChanged()
        {
            if (ValueChanged != null)
                ValueChanged(this, EventArgs.Empty);
        }

        public TrackBar(Vector2 location, int width)
            : base(location)
        {
            BackgroundTexture = UIManager.DefaultTrackBarSprite;
            Height = DefaultHeight;
            Width = width;
            Alpha = Color.Lerp(Color.White, Color.Transparent, 0.5f);
            ThumbMax = Width - 16;
            Validate();
        }

        public override void Validate(bool cascade = false)
        {
            SpriteBatch sb = Game1.Instance.spriteBatch;
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            BackgroundTexture = new RenderTarget2D(gfx, Width, Height);

            gfx.SetRenderTarget(BackgroundTexture as RenderTarget2D);
            gfx.Clear(Color.Transparent);
            sb.Begin();

            sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle(0, 0, Height / 2, Height), new Rectangle(0, 0, UIManager.DefaultTrackBarSprite.Height / 2, UIManager.DefaultTrackBarSprite.Height), Color.White);
            sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle(Height / 2, 0, Width - Height, Height), new Rectangle(UIManager.DefaultTrackBarSprite.Height / 2, 0, 1, UIManager.DefaultTrackBarSprite.Height), Color.White);
            sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle(Width - Height / 2, 0, Height / 2, Height), new Rectangle(UIManager.DefaultTrackBarSprite.Height / 2 + 1, 0, UIManager.DefaultTrackBarSprite.Height / 2, UIManager.DefaultTrackBarSprite.Height), Color.White);
            sb.Draw(UIManager.DefaultTrackBarThumbSprite, new Vector2(ThumbPosition, 0), Color.White);

            for (int i = 0; i <= Maximum; i+=LargeChange)
            {
                sb.Draw(UIManager.Highlight, new Rectangle(7 + i * ((Width - 16) / Maximum), Height - 5, 3, 5), Color.Black);
                sb.Draw(UIManager.Highlight, new Rectangle(8 + i * ((Width - 16) / Maximum), Height - 4, 1, 3), Color.White);
            }

            sb.End();
            gfx.SetRenderTarget(null);
            base.Validate(cascade);
        }

        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(BackgroundTexture, ScreenLocation, Alpha);
            base.Draw(sb);
        }

        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (Controller.Instance.msCurrent.X < ScreenLocation.X + ThumbPosition)
                Value = Math.Max(Minimum, Value - LargeChange);
            else if (Controller.Instance.msCurrent.X > ScreenLocation.X + ThumbPosition + 16)
                Value = Math.Min(Maximum, Value + LargeChange);
            
            base.OnMouseLeftPress(e);
        }

        protected override void OnGotFocus()
        {
            Alpha = Color.White;
            base.OnGotFocus();
        }
        public override void OnLostFocus()
        {
            Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            base.OnLostFocus();
        }
    }
}
