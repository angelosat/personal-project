using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class ComboBoxNew<T> : Button where T : INamed// SelectableItemList<INamed>
    {
        public enum OpenOrientation { Below, Above }
        public OpenOrientation Orientation = OpenOrientation.Below;
        public bool Open;
        SelectableItemList<T> UIItems;
        Panel Panel;
        //public ComboBoxNew(IEnumerable<T> items, int width, int maxItems)
        //{

        //}
        //public ComboBoxNew(IEnumerable<T> items, int maxItems)
        //{

        //}
        public ComboBoxNew(SelectableItemList<T> items, string defaultText, int width) 
        {
            this.AutoSize = false;
            this.UIItems = items;
            this.Panel = new Panel() { AutoSize = true }.AddControls(items) as Panel;
            items.SelectAction = (i) => { this.Text = i.Name; this.ToggleOpen(); };
            this.Text = defaultText;
            this.Width = width;
            this.Height = UIManager.DefaultButtonHeight;
        }
       
        public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.Panel.HitTest() && this.Open)
            {
                this.ToggleOpen();

                //this.Panel.Hide();
            }
            else
                base.HandleLButtonDown(e);

        }
        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            ToggleOpen();
            // this.Lis
            //    this.List.BringToFront();
            //base.OnMouseLeftPress(e);
        }
        protected override void OnHidden()
        {
            this.Panel.Hide();
            base.OnHidden();
        }

        private void ToggleOpen()
        {
            switch (this.Orientation)
            {
                case OpenOrientation.Below:
                    //this.Panel.Location = this.ScreenLocation + new Vector2(-Panel.BackgroundStyle.Border, this.Height);
                    //this.Panel.LocationFunc = () => this.ScreenLocation + new Vector2(-Panel.BackgroundStyle.Border, this.Height);
                    this.Panel.LocationFunc = () => this.ScreenLocation + new Vector2(0, this.Height);

                    break;

                case OpenOrientation.Above:
                    //this.Panel.Location = this.ScreenLocation - new Vector2(0, this.Panel.Height);
                    this.Panel.LocationFunc = () => this.ScreenLocation - new Vector2(0, this.Panel.Height);

                    break;
            }
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
            this.Invalidate();

        }
        public override void OnPaint(SpriteBatch sb)
        {
            float a = (MouseHover && Active) ? 1 : (Active ? 0.5f : 0.1f);

            SpriteEffects spr = this.Open ? SpriteEffects.FlipVertically : SpriteEffects.None;
            sb.Draw(UIManager.DefaultDropdownSprite, Vector2.Zero, left, Color * a, 0, new Vector2(0), 1, spr, Depth);
            sb.Draw(UIManager.DefaultDropdownSprite, new Rectangle(left.Width, 0, Width - (left.Width + right.Width), 23), center, Color * a, 0, new Vector2(0), spr, Depth);
            sb.Draw(UIManager.DefaultDropdownSprite, new Vector2(Width - right.Width, 0), right, Color * a, 0, new Vector2(0), 1, spr, Depth);
            UIManager.DrawStringOutlined(sb, this.Text, new Vector2(this.Width / 2 - 8, this.Height / 2 + (Open ? 1 : 0)), new Vector2(0.5f, .5f));

            //UIManager.DrawStringOutlined(sb, this.Text, new Vector2(this.Width / 2 - 8, Open ? 1 : 0), new Vector2(0.5f, 0));
        }
        static readonly Rectangle
            unpressedBox = new Rectangle(0, 0, 27, 23),
            pressedBox = new Rectangle(0, 23, 27, 23),
            left = new Rectangle(0, 0, 4, 23),
            center = new Rectangle(4, 0, 1, 23),
            right = new Rectangle(5, 0, 22, 23);

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            //this.BoundsScreen.DrawHighlight(sb);
            base.Draw(sb, viewport);
        }
    }
}
