using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    [Obsolete]
    class ComboBox<T> : ButtonBase
        where T : class
    {
        public enum OpenOrientation { Below, Above }
        public OpenOrientation Orientation = OpenOrientation.Below;
        public bool Open = false;
        public T SelectedItem;
        ComboBoxPanel<T> Panel;
        public Func<T, string> TextSelector;
        public ListBox<T, Button> List { get { return this.Panel.List; } }
        public Action<T> ItemChangedFunction = item => { };
        public ComboBox(IEnumerable<T> list, int w, int h , Func<T, string> textFunc, Action<T, Button> ctrlFunc)
        {
            this.Panel = new ComboBoxPanel<T>(list, w, h, textFunc, ctrlFunc, list_SelectedItemChanged);
            this.Width = this.Panel.List.Width;
            this.Height = UIManager.DefaultButtonHeight;
        }
        public ComboBox(ListBox<T, Button> list, Func<T, string> textSelector)
        {
            this.Panel = new ComboBoxPanel<T>(list, list_SelectedItemChanged);
            if (TextSelector == null)
                throw new ArgumentNullException();
            this.TextSelector = textSelector;
            this.Width = list.Width;
            this.Height = UIManager.DefaultButtonHeight;
        }

        void list_SelectedItemChanged(T item)
        {
            SelectedItem = item;
            if(this.TextSelector != null)
                this.Text = TextSelector(SelectedItem);
            ToggleOpen();
            this.Invalidate();
            this.ItemChangedFunction(SelectedItem);
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
            this.Invalidate();
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

        }

        public override void OnPaint(SpriteBatch sb)
        {
            float a = (MouseHover && Active) ? 1 : (Active ? 0.5f : 0.1f);
            SpriteEffects spr = this.Open ? SpriteEffects.FlipVertically : SpriteEffects.None;
            sb.Draw(UIManager.DefaultDropdownSprite, Vector2.Zero, left, Color * a, 0, new Vector2(0), 1, spr, Depth);
            sb.Draw(UIManager.DefaultDropdownSprite, new Rectangle(left.Width, 0, Width - (left.Width + right.Width), 23), center, Color * a, 0, new Vector2(0), spr, Depth);
            sb.Draw(UIManager.DefaultDropdownSprite, new Vector2(Width - right.Width, 0), right, Color * a, 0, new Vector2(0), 1, spr, Depth);
            UIManager.DrawStringOutlined(sb, this.Text, new Vector2(this.Width / 2 - 8, Open ? 1 : this.Height/2), new Vector2(0.5f, .5f));
        }

        static Rectangle
            left = new(0, 0, 4, 23),
            center = new(4, 0, 1, 23),
            right = new(5, 0, 22, 23);
    }
}
