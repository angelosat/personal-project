using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Start_a_Town_.UI
{
    public abstract class ButtonBase : Control
    {
        //public bool IsToggled { get; private set; }
        bool IsToggled;
        public Func<bool> IsToggledFunc = () => false; 
        SpriteFont _Font = UIManager.Font;
        public SpriteFont Font
        {
            get { return _Font; }
            set
            {
                _Font = value;
                OnTextChanged();
            }
        }
        //public SpriteEffects SprFx;
        //SpriteEffects _SprFx;
        public virtual SpriteEffects SprFx
        {
            get
            {
                return IsPressed ? SpriteEffects.FlipVertically : SpriteEffects.None;
            }
        }

        public Action
            //LeftDownAction = () => { },
            //LeftUpAction = () => { },
            LeftClickAction = () => { },
            RightClickAction = () => { };
        public Action<ButtonBase>
            LeftClickActionNew = bb => { }
        ,
            RightClickActionNew = bb => { };

        public Func<Color> TextColorFunc;
        Color _TextColor = UIManager.DefaultTextColor;
        public virtual Color TextColor
        {
            get
            {
                if (TextColorFunc == null)
                    return _TextColor;
                else
                    return TextColorFunc();
            }
            set { this._TextColor = value; }
        }

        public virtual void PerformLeftClick()
        {
            this.Pressed = false;
            //LeftClickAction();
            OnLeftClick();
        }

        public virtual void PerformRightClick()
        {
            this.Pressed = false;

            RightClickAction();
            OnRightClick();
        }
        
        public string TextFormat;
        Func<string> _TextFunc;// = () => "";
        public virtual Func<string> TextFunc
        {
            get { return _TextFunc; }
            set
            {
                _TextFunc = value;
                if (value != null)
                    this.Text = value();
            }
        }
        public virtual ButtonBase SetText(string text)
        {
            this.Text = text;
            return this;
        }
        protected string _Text = "";
        public virtual string Text
        {
            get
            {
                //if (this.TextFunc == null)
                return _Text;
                //else
                //return TextFunc();
            }
            set
            {
                if (value != _Text)
                {
                    _Text = value;
                    OnTextChanged();
                }
            }
        }
        string LastText = "";
        public override void Update()
        {
             
            var newToggled = IsToggledFunc();
            if (this.IsToggled != newToggled)
            {
                this.IsToggled = newToggled;

                this.Invalidate();
            }
            //this.IsToggled = newToggled;

           
            //if (this.Text != this.LastText)
            //    this.Invalidate();

            var nextText = this.TextFunc?.Invoke() ?? this.Text;
            if (nextText != this.LastText)
            {
                this.Text = nextText;
                this.Invalidate();
            }

            base.Update();
            //if (this.TextFunc != null)
            //{
            //    var newText = this.TextFunc();
            //    if (newText != this.Text)
            //    {
            //        this.Text = newText;
            //        this.Invalidate();
            //    }
            //}

            this.LastText = this.Text;
        }
        public Color Fill = Color.White, TextOutline = Color.Black;

        protected virtual void OnTextChanged()
        {
            this.Invalidate();
            string txt = this.Text;
            if (txt == null)
            {
                txt = "";
            }
            if (TextFormat != null)
                return;
            //if (!AutoSize)
            //    return;
            Vector2 textsize = this.Font.MeasureString(txt);
            //Width = (int)textsize.X + 2;
            var maxw = (int)textsize.X + 2;
            var oldw = this.Width;
            //Width = maxw;
            //this.Width = Math.Max((int)textsize.X + 2, Width);

            if (AutoSize)
                this.Width = maxw;
            else
                this.Width = Math.Max(maxw, Width);

            var lineCount = string.IsNullOrEmpty(this.Text) ? 1 : this.Text.Split('\n').Length;
            var oldh = this.Height;
            Height = (int)textsize.Y;// + 2 * lineCount; WARNING commented this out because measurestring returns height = 17 while font linespacing = 15
            if (this.Height != oldh || this.Width != oldw)
                if (this.Parent != null)
                    this.Parent.OnControlResized(this);

        }

        //new public bool AutoSize = false;

        protected bool LeftPressed, RightPressed;
        public virtual bool IsPressed
        {
            get { return LeftPressed || RightPressed || IsToggled; }
        }
        bool _Pressed;
        public virtual bool Pressed
        {
            get { return _Pressed; }
            set
            {
                this._Pressed = value;
                //SprFx = _Pressed ? SpriteEffects.FlipVertically : SpriteEffects.None;
                this.Invalidate();
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
                base.Width = value;
                this.Invalidate();
            }
        }

        public override int Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                base.Height = value;
                this.Invalidate();
            }
        }

        public ButtonBase(Vector2 location) : base(location) { }
        public ButtonBase() : base() { }
        public virtual Control SetLeftClickAction(Action<ButtonBase> action)
        {
            this.LeftClickAction = () => action(this);
            return this;
        }
        
        public override void OnLostFocus()
        {
            //if (Togglable)
            //    return;
            Pressed = false;
            Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            base.OnLostFocus();
        }


        public override void OnMouseEnter()
        {
            base.OnMouseEnter();
            if (Active)
                this.Invalidate();
        }
        public override void OnMouseLeave()
        {
            //Pressed = false;
            //Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            //if (!Togglable)
            //{
            Pressed = false;
            RightPressed = false;
            //}
            base.OnMouseLeave();
            if (Active)
                this.Invalidate();
        }

        protected override void OnMouseRightPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return;
            e.Handled = true;
            this.RightPressed = true;
            this.Pressed = true;
            //   e.Handled = true;
            base.OnMouseRightPress(e);
        }
        protected override void OnMouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.RightPressed)
            {
                this.Invalidate();
                e.Handled = true;
                RightPressed = false;
                this.Pressed = false;
                OnRightClick();
                base.OnMouseRightUp(e);
            }
        }
        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return;
            if (!MouseHover)
                return;
            if (!this.Active)
                return;
            //if (Togglable && !Pressed)
            //{
            //    foreach (var control in this.Parent.Controls)
            //    {
            //        var btn = control as ButtonBaseTogglable;
            //        if (btn.Togglable)
            //            btn.Pressed = false;
            //    }
            //    Pressed = true;
            //    ToggleAction();
            //}
            //else
            this.LeftPressed = true;
            Pressed = true;
            //Pressed = Togglable ? !Pressed : true;
            e.Handled = true;

            base.OnMouseLeftPress(e);
        }
        protected override void OnMouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
        
            //if (this.Pressed && this.Active)
            if (this.LeftPressed && this.Active)
                {
                this.LeftPressed = false;
                    Pressed = false;
                if (!e.Handled)
                    OnLeftClick();
                e.Handled = true;
                base.OnMouseLeftUp(e);
            }
            //base.OnMouseLeftUp(e);
        }


        protected override void OnLeftClick()
        {
            //   if (!LeftClickAction.IsNull())
            LeftClickAction();
            this.LeftClickActionNew(this);
            base.OnLeftClick();
        }

        protected override void OnRightClick()
        {
            //  if (!RightClickAction.IsNull())
            RightClickAction();
            this.RightClickActionNew(this);
            base.OnRightClick();
        }

        public override string ToString()
        {
            return "ButtonBase: " + Text;
        }

        public virtual void DrawSprite(SpriteBatch sb, Rectangle destRect, Rectangle? sourceRect, Color color, float opacity) { }
        public virtual void DrawSprite(SpriteBatch sb, Rectangle destRect, Rectangle? sourceRect, Rectangle viewport, Color color, float opacity, SpriteEffects sprFx) { }
        public virtual void DrawSprite(SpriteBatch sb, Rectangle destRect, Rectangle? sourceRect, Color color, float opacity, SpriteEffects sprFx) { }

        public virtual void DrawText(SpriteBatch sb, Vector2 position, Rectangle? sourceRect, Color color, float opacity) { }
        public virtual void DrawText(SpriteBatch sb, Rectangle screenRect, Rectangle? sourceRect, Color color, float opacity) { }
        public virtual void DrawText(SpriteBatch sb, Rectangle screenRect, Rectangle? sourceRect, Rectangle viewport, Color color, float opacity) { }

    }
}
