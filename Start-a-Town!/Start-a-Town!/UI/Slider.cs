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
        public Action<float> ValueChangedAction = v => { };
        Action _ValueChangedFunc = () => { };
        public virtual Action ValueChangedFunc
        {
            get { return _ValueChangedFunc; }
            set
            {
                _ValueChangedFunc = value;
            }
        }

        PictureBox Thumb;
        public static int DefaultHeight = UIManager.DefaultTrackBarThumbSprite.Height;
        protected float _Value;
        public float Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                ValueChangedFunc();
                OnValueChanged();
                this.ValueChangedAction(value);

                //    ThumbPosition = (int)((Value / (float)Max) * (ThumbMax));
                this.Thumb.Location.X = Offset + (Width - 2 * Offset) * (Value - Min) / (Max - Min) - UIManager.DefaultTrackBarThumbSprite.Width / 2;
              //  this.Thumb.HoverText = !Name.IsNull() ? string.Format(Name, Value) : Value.ToString("##0.##");
              //  this.Invalidate();
            }
        }

        public override int Height
        {
            get
            {
                return DefaultHeight;
            }
            set
            {
               // base.Height = value;
            }
        }

        public override string Name
        {
            get
            {
                return base.Name;
            }
            set
            {
                base.Name = value;
                //this.Thumb.Name = value;
                //this.Thumb.HoverText = !Name.IsNull() ? string.Format(Name, Value) : Value.ToString("##0.##");
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
            
            BackgroundTexture = UIManager.DefaultTrackBarSprite;
         //   Height = DefaultHeight;
            Width = width;
            Alpha = Color.Lerp(Color.White, Color.Transparent, 0.5f);

            this.Thumb = new PictureBox(new Vector2(Offset + (Width - 2 * Offset) * (Value - Min) / (Max - Min), 0), UIManager.DefaultTrackBarThumbSprite, null) { MouseThrough = true };
            this.Thumb.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(Thumb_MouseLeftPress);
            this.Name = "{0}";
            Min = min;
            Max = max;
            Step = step;
            Value = value;
            HoverFunc = () => Value.ToString("##0.##");
            
            Controls.Add(Thumb);
            //Paint();
        }

        void Thumb_MouseLeftPress(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.Moving = true;
        }

        void DrawSprite(SpriteBatch sb, Vector2 position)
        {
            
            sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle((int)position.X, (int)position.Y, Height / 2, Height), new Rectangle(0, 0, UIManager.DefaultTrackBarSprite.Height / 2, UIManager.DefaultTrackBarSprite.Height), Color.White);
            sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle((int)position.X + Height / 2, (int)position.Y, Width - Height, Height), new Rectangle(UIManager.DefaultTrackBarSprite.Height / 2, 0, 1, UIManager.DefaultTrackBarSprite.Height), Color.White);
            sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle((int)position.X + Width - Height / 2, (int)position.Y, Height / 2, Height), new Rectangle(UIManager.DefaultTrackBarSprite.Height / 2 + 1, 0, UIManager.DefaultTrackBarSprite.Height / 2, UIManager.DefaultTrackBarSprite.Height), Color.White);
            //sb.Draw(UIManager.DefaultTrackBarThumbSprite, 
            //    position - new Vector2(UIManager.DefaultTrackBarThumbSprite.Width / 2, 0) + new Vector2(Offset + (Width-2*Offset) * (Value - Min) / (Max - Min), 0), 
            //    Color.White);
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

        //public override string HoverText
        //{
        //    get
        //    {
        //      //  return (!Name.IsNull() ? Name + ": " : "" ) + Value.ToString("##0.##");
        //        return !Name.IsNull() ? string.Format(Name, Value) : Value.ToString("##0.##");
        //    }
        //    set
        //    {
        ////        base.HoverText = value;
        //    }
        //}

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

        public override void Dispose()
        {
            this.Thumb.MouseLeftPress -= Thumb_MouseLeftPress;
        }
    }
}
