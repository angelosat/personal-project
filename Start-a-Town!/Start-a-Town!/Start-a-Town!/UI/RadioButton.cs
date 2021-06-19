using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class RadioButton : CheckBox//ButtonBase//Control
    {
        //static Rectangle
        //    UnCheckedRegion = new Rectangle(0, 0, 23, 23),
        //    CheckedRegion = new Rectangle(0, 23, 23, 23);

        //Rectangle Region = UnCheckedRegion;

        public event EventHandler<EventArgs> CheckedChanged;
        public RadioButton(string text, bool check = false) : base(text, check) { }
        public RadioButton(string text, Vector2 location, bool check = false) : base(text, location, check) { }
        //Texture2D TextSprite;

        //protected string _Text;
        //public string Text
        //{
        //    get { return _Text; }
        //    set
        //    {
        //        _Text = value;
        //        OnTextChanged();
        //    }
        //}
        //private void OnTextChanged()
        //{
        //    TextSprite = UIManager.DrawTextOutlined(Text);
        //}

        //public override void Paint()
        //{
        //    TextSprite = UIManager.DrawTextOutlined(Text);
        //    base.Paint();
        //}

        //bool _Checked;
        //public bool Checked
        //{
        //    get { return _Checked; }
        //    set
        //    {
        //        bool oldCheck = _Checked;
        //        _Checked = value;
        //        if (oldCheck != _Checked)
        //        {
        //            Region = Checked ? CheckedRegion : UnCheckedRegion;
        //            //OnCheckedChanged();
        //        }
        //    }
        //}
        public void OnCheckedChanged()
        {
            if (CheckedChanged != null)
                CheckedChanged(this, EventArgs.Empty);
        }

        //public virtual RadioButton SetChecked(bool condition) { Checked = condition; return this; }
        //public RadioButton SetTag(object tag) { this.Tag = tag; return this; }
        //public RadioButton(string text, Vector2 location)
        //    : base(location)
        //{
        //    Background = Game1.Instance.Content.Load<Texture2D>("Graphics/Gui/CheckBox");
        //    Text = text;
        //    Height = 23;
        //    Width = Background.Width + TextSprite.Width + 5;
        //    Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
        //}

        ////public override void Update()
        ////{
        ////    t += (MouseHover ? dt : -dt) * UIFpsCounter.deltaTime;
        ////    Alpha = Color.Lerp(Color.Transparent, Color.White, Math.Max(0.5f, Math.Min(1, t)));
        ////    base.Update();
        ////}

        //public override void Draw(SpriteBatch sb)
        //{
        //    //DrawHighlight(sb);

        //    sb.Draw(Background, ScreenLocation, Region, Alpha);
        //    sb.Draw(TextSprite, ScreenLocation + new Vector2(25, Height / 2), null, Color.White, 0, new Vector2(0, TextSprite.Height / 2), 1, SpriteEffects.None, 1f);
        //    base.Draw(sb);
        //}

        protected override void OnLeftClick()
        {
          //  Checked = true;// !Checked;
            if (Checked)
                return;
            Rectangle bounds = this.ScreenBounds;
            // change state only if clicked within the actual checkmark box, otherwise just select???
            //if (!Rectangle.Intersect(bounds, new Rectangle(bounds.X, bounds.Y, 23, 23)).Intersects(UIManager.MouseRect))
            //    return;
            base.OnLeftClick(); 
            OnCheckedChanged();
            if (Checked)
            {
                if (Parent.Controls != null)
                    foreach (Control control in Parent.Controls.Where(foo => foo is RadioButton))
                        if (control != this)
                            (control as RadioButton).Checked = false;
            }

            //base.OnMouseLeftPress(e);
        }

        //protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    Checked = true;// !Checked;
        //    OnCheckedChanged();
        //    if (Checked)
        //    {
        //        if (Parent.Controls != null)
        //            foreach (Control control in Parent.Controls.Where(foo => foo is RadioButton))
        //                if (control != this)
        //                    (control as RadioButton).Checked = false;
        //    }
            
        //    base.OnMouseLeftPress(e);
        //}
    }
}
