using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class CheckBox : ButtonBase
    {
        static public readonly Rectangle
            UnCheckedRegion = new(0, 0, 23, 23),
            CheckedRegion = new(0, 23, 23, 23);

        Rectangle Region { get { return this.Checked ? CheckedRegion : UnCheckedRegion; } }

        public override void Update()
        {
            if (this.ValueFunction != null)
            {
                var newvalue = this.ValueFunction();
                if (this.LastValue != newvalue)
                    this.Checked = newvalue;
                this.LastValue = newvalue;
            }
            base.Update();
        }

        public override void OnPaint(SpriteBatch sb)
        {
            sb.Draw(BackgroundTexture, new Vector2(0, (Pressed && Active) ? 1 : 0), Region, UIManager.DefaultTextColor * ((MouseHover && Active) ? 1 : 0.5f));
            UIManager.DrawStringOutlined(sb, this.Text, new Vector2(25, Height / 2), new Vector2(0, 0.5f));
        }
        bool LastValue;
        public Func<bool> ValueFunction;
        public Action<bool> ValueChangedFunction = value => { };
        bool _Checked;
        public bool Checked
        {
            get { return _Checked; }
            set
            {
                _Checked = value;
                this.ValueChangedFunction(value);
                this.Invalidate();
            }
        }

        public CheckBox()
        {
            BackgroundTexture = UIManager.TextureTickBox;
        }
        public CheckBox(string text, bool check = false)
            : this(text, Vector2.Zero, check)
        { }
        public CheckBox(string text, Vector2 location, bool check = false)
           : base(location)
        {
            BackgroundTexture = UIManager.TextureTickBox;
            Text = text;
            Height = 23;
            Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            this.Checked = check;
        }

        protected override void OnTextChanged()
        {
            base.OnTextChanged();
            Width += BackgroundTexture.Width + 5;
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            //this.BoundsScreen.DrawHighlight(sb);
            base.Draw(sb, viewport);
        }

        public CheckBox SetChecked(bool condition) { Checked = condition; return this; }

        protected override void OnLeftClick()
        {
            // change state only if clicked within the actual checkmark box, otherwise just select
            Rectangle bounds = this.BoundsScreen;
            //if (Rectangle.Intersect(bounds, new Rectangle(bounds.X, bounds.Y, 23, 23)).Intersects(UIManager.MouseRect))
                Checked = (!Checked && Active);
            base.OnLeftClick();
        }
    }
}
