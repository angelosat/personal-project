using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
                //ThumbPosition = ThumbMin + (int)((Value / (float)Maximum) * (ThumbMax - ThumbMin));
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

        int ThumbPosition = 0, //ThumbMin = 8, 
            ThumbMax;

        //Texture2D _Sprite;
        //public override Texture2D Background
        //{
        //    get
        //    {
        //        if (_Sprite == null)
        //            Paint();
        //        return _Sprite;
        //    }
        //    set { _Sprite = value; }
        //}

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
            //Value = 0;
            ThumbMax = Width - 16;
            Validate();
        }

        public override void Validate()
        {
            SpriteBatch sb = Game1.Instance.spriteBatch;
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            BackgroundTexture = new RenderTarget2D(gfx, Width, Height);

            gfx.SetRenderTarget(BackgroundTexture as RenderTarget2D);
            gfx.Clear(Color.Transparent);
            sb.Begin();

            //sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle(0, Height / 4, Height / 2, Height / 2), new Rectangle(0, 0, Height / 2, Height), Alpha);
            //sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle(Height / 2, Height / 4, Width - Height + 1, Height / 2), new Rectangle(Height / 2, 0, 1, Height), Alpha);
            //sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle(Width - Height / 2, Height / 4, Height / 2, Height / 2), new Rectangle(Height / 2, 0, Height / 2, Height), Alpha);
            //sb.Draw(UIManager.DefaultTrackBarThumbSprite, new Vector2(ThumbPosition, Height / 8), Color.White);

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
            base.Validate();
        }

        public override void Draw(SpriteBatch sb)
        {
            //sb.Draw(Sprite, ScreenLocation, new Rectangle(0, 0, Height / 2, Height), Alpha);
            //sb.Draw(Sprite, new Rectangle((int)ScreenLocation.X + Height / 2, (int)ScreenLocation.Y, Width - Height + 1, Height), new Rectangle(Height / 2, 0, 1, Height), Alpha);
            //sb.Draw(Sprite, ScreenLocation + new Vector2(Width - Height / 2, 0), new Rectangle(Height / 2, 0, Height / 2, Height), Alpha);

            //sb.Draw(Sprite, new Rectangle((int)ScreenLocation.X, (int)ScreenLocation.Y + Height / 4, Height / 2, Height / 2), new Rectangle(0, 0, Height / 2, Height), Alpha);
            //sb.Draw(Sprite, new Rectangle((int)ScreenLocation.X + Height / 2, (int)ScreenLocation.Y + Height / 4, Width - Height + 1, Height / 2), new Rectangle(Height / 2, 0, 1, Height), Alpha);
            //sb.Draw(Sprite, new Rectangle((int)ScreenLocation.X + Width - Height / 2, (int)ScreenLocation.Y + Height / 4, Height / 2, Height / 2), new Rectangle(Height / 2, 0, Height / 2, Height), Alpha);
            //sb.Draw(UIManager.DefaultTrackBarThumbSprite, ScreenLocation + new Vector2(ThumbPosition, Height / 8), Color.White);
            //for (int i = 0; i <= Maximum; i++)
            //{
            //    sb.Draw(UIManager.Highlight, new Rectangle((int)ScreenLocation.X + 8 + i*((Width-16)/Maximum), (int)ScreenLocation.Y + Height - 3, 1, 3), Color.White);
            //}
            //DrawHighlight(sb);
            sb.Draw(BackgroundTexture, ScreenLocation, Alpha);
            base.Draw(sb);
        }

        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //if((new Rectangle((int)ScreenLocation.X + ThumbPosition, (int)ScreenLocation.Y, 16, 16).Intersects(new Rectangle(Controller.X, Controller.Y, 1, 1))))
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
