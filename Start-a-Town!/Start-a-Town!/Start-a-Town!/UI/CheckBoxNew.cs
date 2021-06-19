using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class CheckBoxNew : ButtonBase
    {
        static Rectangle
            UnCheckedRegion = new Rectangle(0, 0, 23, 23),
            CheckedRegion = new Rectangle(0, 23, 23, 23);

        Rectangle Region { get { return this.Value ? CheckedRegion : UnCheckedRegion; } }

        public override void OnPaint(SpriteBatch sb)
        {
            sb.Draw(BackgroundTexture, new Vector2(0, (Pressed && Active) ? 1 : 0), Region, UIManager.DefaultTextColor * ((MouseHover && Active) ? 1 : 0.5f));
            UIManager.DrawStringOutlined(sb, this.Text, new Vector2(25, Height / 2), new Vector2(0, 0.5f));
        }
        //public Action<bool> ValueChangedFunction = value => { };
        bool _Value;
        public bool Value
        {
            get { return _Value; }
            set
            {
                _Value = value;
                //this.ValueChangedFunction(value);
                this.Invalidate();
            }
        }

        public CheckBoxNew():this("")
        {
            //BackgroundTexture = UIManager.TextureTickBox;
        }
        public CheckBoxNew(string text, bool check = false)
            : this(text, Vector2.Zero, check)
        { }
        public CheckBoxNew(string text, Vector2 location, bool check = false)
           : base(location)
        {
            BackgroundTexture = UIManager.TextureTickBox;
            Text = text;
            Height = 23;
            Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            this.Value = check;
        }

        protected override void OnTextChanged()
        {
            base.OnTextChanged();
            if(!string.IsNullOrWhiteSpace(this.Text))
            this.Width += BackgroundTexture.Width + 5;
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }

        public CheckBoxNew SetChecked(bool condition) { Value = condition; return this; }

        protected override void OnLeftClick()
        {
            // change state only if clicked within the actual checkmark box, otherwise just select
            Rectangle bounds = this.ScreenBounds;
                //Checked = (!Checked && Active); //this checkbox doesnt change its value itself
            base.OnLeftClick();
        }

    }
}
