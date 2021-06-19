using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class Slider : Control
    {
        public static int DefaultHeight = UIManager.DefaultTrackBarThumbSprite.Height;
        protected float _Value;
        public float Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                OnValueChanged();
                //    ThumbPosition = (int)((Value / (float)Max) * (ThumbMax));
                this.Invalidate();
             //   Paint();
            }
        }
        public float Min, Max, Step;//, Value;
        public int TickFrequency { get; set; }

        public override void OnPaint(SpriteBatch sb)
        {
            DrawSprite(sb, Vector2.Zero);
        }

        public event EventHandler<EventArgs> ValueChanged;
        protected void OnValueChanged()
        {
            if (ValueChanged != null)
                ValueChanged(this, EventArgs.Empty);
        }

        public Slider(Vector2 location, int width, float min = 0, float max = 1, float step = 0.1f, float value = 0)
            : base(location)
        {
            Background = UIManager.DefaultTrackBarSprite;
            Height = DefaultHeight;
            Width = width;
            Alpha = Color.Lerp(Color.White, Color.Transparent, 0.5f);

            Min = min;
            Max = max;
            Step = step;
            Value = value;
            //Paint();
        }

        void DrawSprite(SpriteBatch sb, Vector2 position)
        {
            
            sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle((int)position.X, (int)position.Y, Height / 2, Height), new Rectangle(0, 0, UIManager.DefaultTrackBarSprite.Height / 2, UIManager.DefaultTrackBarSprite.Height), Color.White);
            sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle((int)position.X + Height / 2, (int)position.Y, Width - Height, Height), new Rectangle(UIManager.DefaultTrackBarSprite.Height / 2, 0, 1, UIManager.DefaultTrackBarSprite.Height), Color.White);
            sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle((int)position.X + Width - Height / 2, (int)position.Y, Height / 2, Height), new Rectangle(UIManager.DefaultTrackBarSprite.Height / 2 + 1, 0, UIManager.DefaultTrackBarSprite.Height / 2, UIManager.DefaultTrackBarSprite.Height), Color.White);
            sb.Draw(UIManager.DefaultTrackBarThumbSprite, 
                position - new Vector2(UIManager.DefaultTrackBarThumbSprite.Width / 2, 0) + new Vector2(Offset + (Width-2*Offset) * (Value - Min) / (Max - Min), 0), 
                Color.White);
        }

        //public override void Draw(SpriteBatch sb)
        //{
        //   // sb.Draw(Background, ScreenLocation, Alpha);
        //    DrawSprite(sb, ScreenLocation);
        //    base.Draw(sb);
        //}

        bool Moving;
        float Offset = UIManager.DefaultTrackBarSprite.Width / 4;// + UIManager.DefaultTrackBarSprite.Width / 2;
        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            float mouseX = (Controller.Instance.msCurrent.X / UIManager.Scale - ScreenLocation.X);
            if (mouseX < Offset || mouseX > Width - Offset)
                return;
            Moving = true;
            base.OnMouseLeftPress(e);
        }
        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            Moving = false;
        }

        protected override void OnMouseScroll(System.Windows.Forms.HandledMouseEventArgs e)
        {
          //  Value = Math.Max(Min, Math.Min(Max, Value + (e.CurrentMouseState.ScrollWheelValue > e.LastMouseState.ScrollWheelValue ? Step : -Step)));
        }

        public override void Update()
        {
            base.Update();
            if (!Moving)
                return;

            float mouseX = (Controller.Instance.msCurrent.X / UIManager.Scale - ScreenLocation.X);
            //if (mouseX < Offset || mouseX > Width - Offset)
            //    return;
            float mousePerc = MathHelper.Clamp(mouseX / (float)Width, 0, 1);

           // Console.WriteLine(mousePerc);
            Value = (float)Math.Round((Min + mousePerc * (Max - Min)) / Step) * Step;
            //this.HoverText = Value.ToString("##0.##");
        }

        public override string HoverText
        {
            get
            {
                return (!Name.IsNull() ? Name + ": " : "" ) + Value.ToString("##0.##");
            }
            set
            {
                base.HoverText = value;
            }
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
