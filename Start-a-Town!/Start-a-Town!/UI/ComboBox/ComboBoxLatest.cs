using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    [Obsolete]
    class ComboBoxLatest<T> : ButtonBase
        where T : class, INamed
    {
        public enum OpenOrientation { Below, Above }
        public OpenOrientation Orientation = OpenOrientation.Below;
        public bool Open = false;
        Control Panel;
        public Action<T> ItemChangedFunction = item => { };

        public ComboBoxLatest(T[] items, int width, int maxVisibleItems, Action<ComboBoxLatest<T>, T> callback, Func<ComboBoxLatest<T>, T> selectedGetter)
        {
            this.TextFunc = () => selectedGetter(this).Name;
            this.Width = width;
            this.Height = UIManager.DefaultButtonHeight;
            var list = new ListBoxNew<T, Label>(width, maxVisibleItems * Label.DefaultHeight, maxVisibleItems < items.Length ? ScrollableBoxNew.ScrollModes.Vertical : ScrollableBoxNew.ScrollModes.None);
            list.AddItems(items, i => new Label() { TextFunc = () => i.Name, LeftClickAction = () => { callback(this, i); this.ToggleOpen(); } });
            this.Panel = list.ToPanel();
        }
        protected override void OnTextChanged()
        {
            this.Invalidate();
        }
        public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.Panel.HitTest() && this.Open)
            {
                this.ToggleOpen();
            }
            else
            base.HandleLButtonDown(e);

        }
        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            ToggleOpen();
        }

        protected override void OnHidden()
        {
            this.Panel.Hide();
            base.OnHidden();
        }

        private void ToggleOpen()
        {
            switch(this.Orientation)
            {
                case OpenOrientation.Below:
                    this.Panel.Location = this.ScreenLocation + new Vector2(-Panel.BackgroundStyle.Border, this.Height);
                    this.Panel.ConformToScreen();
                    break;

                case OpenOrientation.Above:
                    this.Panel.Location = this.ScreenLocation - new Vector2(0, this.Panel.Height);
                    this.Panel.ConformToScreen();
                    break;
            }
            if (!Open)
            {
                this.Panel.Layer = this.Layer;
                this.Panel.Layer = this.Layer;
                this.Panel.Show();
            }
            else
                this.Panel.Hide();
            this.Panel.BringToFront();
            Open = !Open;
            this.Validate();

        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }

        public override void OnPaint(SpriteBatch sb)
        {
            float a = (MouseHover && Active) ? 1 : (Active ? 0.5f : 0.1f);
            SpriteEffects spr = this.Open ? SpriteEffects.FlipVertically : SpriteEffects.None;
            sb.Draw(UIManager.DefaultDropdownSprite, Vector2.Zero, left, Color * a, 0, new Vector2(0), 1, spr, Depth);
            sb.Draw(UIManager.DefaultDropdownSprite, new Rectangle(left.Width, 0, Width - (left.Width + right.Width), 23), center, Color * a, 0, new Vector2(0), spr, Depth);
            sb.Draw(UIManager.DefaultDropdownSprite, new Vector2(Width - right.Width, 0), right, Color * a, 0, new Vector2(0), 1, spr, Depth);
            UIManager.DrawStringOutlined(sb, this.Text, new Vector2(this.Width / 2 - 8, this.Height / 2 + (Open ? 1 : 0)), new Vector2(.5f));

        }

        static Rectangle
            left = new Rectangle(0, 0, 4, 23),
            center = new Rectangle(4, 0, 1, 23),
            right = new Rectangle(5, 0, 22, 23);
    }
}
