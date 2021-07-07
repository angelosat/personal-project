using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    [Obsolete]
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
                this.Thumb.Location.X = Offset + (Width - 2 * Offset) * (Value - Min) / (Max - Min) - UIManager.DefaultTrackBarThumbSprite.Width / 2;
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
            }
        }

        public float Min, Max, Step;
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
            this.Name = "{0}";
            Min = min;
            Max = max;
            Step = step;
            Value = value;
            HoverFunc = () => Value.ToString("##0.##");
            
            Controls.Add(Thumb);
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
        }

        bool Moving;
        float Offset = UIManager.DefaultTrackBarSprite.Width / 4;
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

        public override void Update()
        {
            base.Update();
            if (!Moving)
                return;

            float mouseX = (Controller.Instance.msCurrent.X / UIManager.Scale - ScreenLocation.X);
            float mousePerc = MathHelper.Clamp(mouseX / (float)Width, 0, 1);
            Value = (float)Math.Round((Min + mousePerc * (Max - Min)) / Step) * Step;
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
