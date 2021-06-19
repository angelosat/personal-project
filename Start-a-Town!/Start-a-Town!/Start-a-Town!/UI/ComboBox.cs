using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class ComboBox<T> : ButtonBase
        where T : class
    {
        Panel Panel;
        public ListBox<T, Button> List;
        public bool Open = false;
        public T SelectedItem;

        public Func<T, string> TextSelector;

        public Action<T> ItemChangedFunction = item => { };
        public event EventHandler<EventArgs> SelectedItemChanged;
        void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }

        public ComboBox(ListBox<T, Button> list, Func<T, string> textSelector)
        {
            Panel = new Panel() { AutoSize = true };
            this.List = list;
            this.TextSelector = textSelector;
            //list.SelectedItemChanged += new EventHandler<EventArgs>(list_SelectedItemChanged);
            list.ItemChangedFunc = list_SelectedItemChanged;

           // list.ClipToBounds = false;
       //     list.SetClipToBounds(false, true);
            list.BackgroundColor =  Color.Black;
            this.Width = list.Width;
            this.Height = UIManager.DefaultButtonHeight;
            //Panel.Parent = this;
            Panel.Location =  this.BottomLeft - Panel.ClientLocation;
          //  Panel.Location.X -= 10;
            Panel.Controls.Add(list);
        }

        void list_SelectedItemChanged(object sender, EventArgs e)
        {
            SelectedItem = (sender as Button).Tag as T;
            this.Text = TextSelector(SelectedItem);
            ToggleOpen();
            this.Invalidate();
            this.ItemChangedFunction(SelectedItem);
            this.OnSelectedItemChanged();
        }
        void list_SelectedItemChanged(T item)
        {
            SelectedItem = item;
            this.Text = TextSelector(SelectedItem);
            ToggleOpen();
            this.Invalidate();
            this.ItemChangedFunction(SelectedItem);
            this.OnSelectedItemChanged();
        }

        protected override void OnTextChanged()
        {
            this.Invalidate();
        }

        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            ToggleOpen();
            this.Invalidate();
            // this.Lis
            //    this.List.BringToFront();
            //base.OnMouseLeftPress(e);
        }

        private void ToggleOpen()
        {
            this.Panel.Location = this.ScreenLocation + new Vector2(-Panel.BackgroundStyle.Border, this.Height);
            if (!Open)
            //this.Parent.Controls.Add(this.Panel);
            {
                this.Panel.Layer = this.Layer;
                this.Panel.Show();

            }
            else
                //this.Parent.Controls.Remove(this.Panel);
                this.Panel.Hide();
            this.Panel.BringToFront();
            Open = !Open;

        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
         //   this.Bounds.DrawHighlight(sb);
            base.Draw(sb, viewport);
        }

        public override void OnPaint(SpriteBatch sb)
        {
            float a = (MouseHover && Active) ? 1 : (Active ? 0.5f : 0.1f);
            
            SpriteEffects spr = this.Open ? SpriteEffects.FlipVertically : SpriteEffects.None;
            sb.Draw(UIManager.DefaultDropdownSprite, Vector2.Zero, left, Color * a, 0, new Vector2(0), 1, spr, Depth);
            sb.Draw(UIManager.DefaultDropdownSprite, new Rectangle(left.Width, 0, Width - (left.Width + right.Width), 23), center, Color * a, 0, new Vector2(0), spr, Depth);
            sb.Draw(UIManager.DefaultDropdownSprite, new Vector2(Width - right.Width, 0), right, Color * a, 0, new Vector2(0), 1, spr, Depth);
            UIManager.DrawStringOutlined(sb, this.Text, new Vector2(this.Width / 2 - 8, Open ? 1 : 0), new Vector2(0.5f, 0));
        }

        static Rectangle
            unpressedBox = new Rectangle(0, 0, 27, 23),
            pressedBox = new Rectangle(0, 23, 27, 23),
            left = new Rectangle(0, 0, 4, 23),
            center = new Rectangle(4, 0, 1, 23),
            right = new Rectangle(5, 0, 22, 23);
    }
}
