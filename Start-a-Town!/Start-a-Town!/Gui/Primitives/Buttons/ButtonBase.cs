using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace Start_a_Town_.UI
{
    public abstract class ButtonBase : Control
    {
        bool IsToggled;
        public Func<bool> IsToggledFunc = () => false;
        SpriteFont _font = UIManager.Font;
        public SpriteFont Font
        {
            get => this._font;
            set
            {
                this._font = value;
                this.OnTextChanged();
            }
        }
        public virtual SpriteEffects SprFx => this.IsPressed ? SpriteEffects.FlipVertically : SpriteEffects.None;

        public Action
            LeftClickAction = () => { },
            RightClickAction = () => { };
        public Action<ButtonBase>
            LeftClickActionNew = bb => { },
            RightClickActionNew = bb => { };

        public Func<Color> TextColorFunc;
        Color _TextColor = UIManager.DefaultTextColor;
        public virtual Color TextColor
        {
            get
            {
                if (this.TextColorFunc == null)
                    return this._TextColor;
                else
                    return this.TextColorFunc();
            }
            set => this._TextColor = value;
        }

        public virtual void PerformLeftClick()
        {
            this.Pressed = false;
            this.OnLeftClick();
        }

        public virtual void PerformRightClick()
        {
            this.Pressed = false;
            this.RightClickAction();
            this.OnRightClick();
        }

        public string TextFormat;
        Func<string> _textFunc;
        public virtual Func<string> TextFunc
        {
            get => this._textFunc;
            set
            {
                this._textFunc = value;
                if (value is not null)
                    this.Text = value();
            }
        }
        internal ButtonBase SetTextFunc(Func<string> textFunc)
        {
            this.TextFunc = textFunc;
            return this;
        }
        public virtual ButtonBase SetText(string text)
        {
            this.Text = text;
            return this;
        }
        protected string _text = "";
        public virtual string Text
        {
            get => this._text;
            set
            {
                if (value != this._text)
                {
                    this._text = value;
                    this.OnTextChanged();
                }
            }
        }
        string LastText = "";
        public override void Update()
        {
            var newToggled = this.IsToggledFunc();
            if (this.IsToggled != newToggled)
            {
                this.IsToggled = newToggled;
                this.Invalidate();
            }

            var nextText = this.TextFunc?.Invoke() ?? this.Text;
            if (nextText != this.LastText)
            {
                this.Text = nextText;
                this.Invalidate();
            }

            base.Update();

            this.LastText = this.Text;
        }
        public Color Fill = Color.White, TextOutline = Color.Black;

        protected virtual void OnTextChanged()
        {
            this.Invalidate();
            string txt = this.Text;
            if (txt == null)
                txt = "";
            if (this.TextFormat != null)
                return;

            var textsize = this.Font.MeasureString(txt);
            var maxw = (int)textsize.X + 2;
            var oldw = this.Width;

            if (this.AutoSize)
                this.Width = maxw;
            else
                this.Width = Math.Max(maxw, this.Width);

            var lineCount = string.IsNullOrEmpty(this.Text) ? 1 : this.Text.Split('\n').Length;
            var oldh = this.Height;
            this.Height = (int)textsize.Y;// + 2 * lineCount; WARNING commented this out because measurestring returns height = 17 while font linespacing = 15
            if (this.Height != oldh || this.Width != oldw)
                if (this.Parent is not null)
                    this.Parent.OnControlResized(this);
        }

        protected bool LeftPressed, RightPressed;
        public virtual bool IsPressed => this.LeftPressed || this.RightPressed || this.IsToggled;
        bool _Pressed;
        public virtual bool Pressed
        {
            get => this._Pressed;
            set
            {
                this._Pressed = value;
                this.Invalidate();
            }
        }

        public override int Width
        {
            get => base.Width;
            set
            {
                base.Width = value;
                this.Invalidate();
            }
        }

        public override int Height
        {
            get => base.Height;
            set
            {
                base.Height = value;
                this.Invalidate();
            }
        }

        public ButtonBase(Vector2 location) : base(location) { }
        public ButtonBase() : base() { }
        public virtual Control SetLeftClickAction(Action action)
        {
            this.LeftClickAction = action;
            return this;
        }
        public virtual Control SetLeftClickAction(Action<ButtonBase> action)
        {
            this.LeftClickAction = () => action(this);
            return this;
        }

        public override void OnLostFocus()
        {
            this.Pressed = false;
            this.Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            base.OnLostFocus();
        }


        public override void OnMouseEnter()
        {
            base.OnMouseEnter();
            if (this.Active)
                this.Invalidate();
        }
        public override void OnMouseLeave()
        {
            this.Pressed = false;
            this.RightPressed = false;
            base.OnMouseLeave();
            if (this.Active)
                this.Invalidate();
        }

        protected override void OnMouseRightPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return;

            e.Handled = true;
            this.RightPressed = true;
            this.Pressed = true;
            base.OnMouseRightPress(e);
        }
        protected override void OnMouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.RightPressed)
            {
                this.Invalidate();
                e.Handled = true;
                this.RightPressed = false;
                this.Pressed = false;
                this.OnRightClick();
                base.OnMouseRightUp(e);
            }
        }
        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return;

            if (!this.MouseHover)
                return;

            if (!this.Active)
                return;

            this.LeftPressed = true;
            this.Pressed = true;
            e.Handled = true;
            base.OnMouseLeftPress(e);
        }
        protected override void OnMouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.LeftPressed && this.Active)
            {
                this.LeftPressed = false;
                this.Pressed = false;
                if (!e.Handled)
                    this.OnLeftClick();

                e.Handled = true;
                base.OnMouseLeftUp(e);
            }
        }

        protected override void OnLeftClick()
        {
            this.LeftClickAction();
            this.LeftClickActionNew(this);
            base.OnLeftClick();
        }

        protected override void OnRightClick()
        {
            this.RightClickAction();
            this.RightClickActionNew(this);
            base.OnRightClick();
        }

        public void Toggle()
        {
            if (!this.Active)
                return;
            if (!this.LeftPressed)
            {
                this.LeftPressed = true;
                this.Pressed = true;
            }
            else if (this.LeftPressed)
            {
                this.LeftPressed = false;
                this.Pressed = false;
                this.OnLeftClick();
            }
        }

        public override string ToString()
        {
            return "ButtonBase: " + this.Text;
        }

        public virtual void DrawSprite(SpriteBatch sb, Rectangle destRect, Rectangle? sourceRect, Color color, float opacity) { }
        public virtual void DrawSprite(SpriteBatch sb, Rectangle destRect, Rectangle? sourceRect, Color color, float opacity, SpriteEffects sprFx) { }

        public virtual void DrawText(SpriteBatch sb, Vector2 position, Rectangle? sourceRect, Color color, float opacity) { }
    }
}
