using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class SliderNew : Control
    {
        Panel PanelValue;
        Label LabelValue;

        public Action<float> ValueSelectAction = v => { };
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
                this.Thumb.Location.X = (int)(Border + (Width - 2 * Border) * (Value - Min) / (Max - Min) - UIManager.DefaultTrackBarThumbSprite.Width / 2);
            }
        }
        public float NewValue;

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

        public SliderNew(Vector2 location, int width, float min = 0, float max = 1, float step = 0.1f, float value = 0)
            : base(location)
        {
            BackgroundTexture = UIManager.DefaultTrackBarSprite;
            Width = width;
            Alpha = Color.Lerp(Color.White, Color.Transparent, 0.5f);

            this.Thumb = new PictureBox(new Vector2(Border + (Width - 2 * Border) * (Value - Min) / (Max - Min), 0), UIManager.DefaultTrackBarThumbSprite, null) { MouseThrough = true };
            this.Thumb.ClipToBounds = false;
            this.Name = "{0}";
            Min = min;
            Max = max;
            Step = step;
            Value = value;
            HoverFunc = () => Value.ToString("##0.##");

            this.PanelValue = new Panel() { Location = this.TopRight };
            this.PanelValue.ClientDimensions = UIManager.Font.MeasureString(this.Min.ToString());
            this.LabelValue = new Label();
            this.PanelValue.AddControls(this.LabelValue);
            this.PanelValue.SetMousethrough(true, true);

            Controls.Add(Thumb);
        }

        void DrawSprite(SpriteBatch sb, Vector2 position)
        {
            sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle((int)position.X, (int)position.Y, Height / 2, Height), new Rectangle(0, 0, UIManager.DefaultTrackBarSprite.Height / 2, UIManager.DefaultTrackBarSprite.Height), Color.White);
            sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle((int)position.X + Height / 2, (int)position.Y, Width - Height, Height), new Rectangle(UIManager.DefaultTrackBarSprite.Height / 2, 0, 1, UIManager.DefaultTrackBarSprite.Height), Color.White);
            sb.Draw(UIManager.DefaultTrackBarSprite, new Rectangle((int)position.X + Width - Height / 2, (int)position.Y, Height / 2, Height), new Rectangle(UIManager.DefaultTrackBarSprite.Height / 2 + 1, 0, UIManager.DefaultTrackBarSprite.Height / 2, UIManager.DefaultTrackBarSprite.Height), Color.White);
        }

        bool SelectingValue;
        float Border = UIManager.DefaultTrackBarSprite.Width / 4;
        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            SelectingValue = true;
            base.OnMouseLeftPress(e);
        }
        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!SelectingValue)
                return;

            SelectingValue = false;

            if (this.NewValue != this.Value)
                this.ValueSelectAction(this.NewValue);
        }

        public override void Update()
        {
            base.Update();
            if (!SelectingValue)
                return;
            this.LabelValue.Text = this.NewValue.ToString();
            float mouseX = (Controller.Instance.msCurrent.X / UIManager.Scale - (ScreenLocation.X + this.Border));
            float mousePerc = MathHelper.Clamp(mouseX / (float)(Width - 2 * this.Border), 0, 1);

            var newValue = ((Min + mousePerc * (Max - Min)) / Step);
            this.NewValue = (float)Math.Round(newValue) * Step;
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

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
            if (!this.SelectingValue)
                return;
            var loc = new Vector2((int)(Border + (Width - 2 * Border) * (this.NewValue - Min) / (Max - Min) - UIManager.DefaultTrackBarThumbSprite.Width / 2), 0);

            sb.Draw(UIManager.DefaultTrackBarThumbSprite,
                this.ScreenLocation + loc,
                Color.White * .5f);
            UIManager.DrawStringOutlined(sb, this.NewValue.ToString("##0%"), this.ScreenLocation + new Vector2(this.Width, 0), new Vector2(1));
        }
    }
}
