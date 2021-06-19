using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    static class ComboBox
    {
        public static ComboBox<T> Create<T>(List<T> items, int width, int maxVisibleItems) where T : class, INamed
        {
            throw new NotImplementedException();
        }
    }

    class ComboBox<T> : ButtonBase
        where T : class
    {
        public enum OpenOrientation { Below, Above }
        public OpenOrientation Orientation = OpenOrientation.Below;
        //Panel Panel;
        //public ListBox<T, Button> List;
        public bool Open = false;
        public T SelectedItem;
        ComboBoxPanel<T> Panel;
        public Func<T, string> TextSelector;
        public ListBox<T, Button> List { get { return this.Panel.List; } }
        public Action<T> ItemChangedFunction = item => { };
        //public event EventHandler<EventArgs> SelectedItemChanged;
        //void OnSelectedItemChanged()
        //{
        //    if (SelectedItemChanged != null)
        //        SelectedItemChanged(this, EventArgs.Empty);
        //}
        public ComboBox(IEnumerable<T> list, int w, int h , Func<T, string> textFunc, Action<T, Button> ctrlFunc)
        {
            this.Panel = new ComboBoxPanel<T>(list, w, h, textFunc, ctrlFunc, list_SelectedItemChanged);
            this.Width = this.Panel.List.Width;
            this.Height = UIManager.DefaultButtonHeight;
        }
        //public ComboBox(IEnumerable<T> list, int width)
        //{
        //    this.Width = width;
        //}
        //public ComboBox(IEnumerable<T> list, int w, int h, Func<T, string> textFunc, Action<T, Button> ctrlFunc)
        //{
        //    this.List = new ListBox<T, Button>(w, h);
        //    this.List.Build(list, textFunc, ctrlFunc);
        //    this.TextSelector = textFunc;
        //    this.List.ItemChangedFunc = list_SelectedItemChanged;
        //    this.List.BackgroundColor = Color.Black;
        //    this.Width = this.List.Width;
        //    this.Height = UIManager.DefaultButtonHeight;
        //    //Panel.Parent = this;
        //    //Panel.Location =  this.BottomLeft - Panel.ClientLocation;
        //    //  Panel.Location.X -= 10;
        //    this.Panel = new Panel()
        //    {
        //        AutoSize = true
        //    };
        //    this.Panel.Controls.Add(this.List);
        //}
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
            //this.OnSelectedItemChanged();
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
                //this.Panel.Hide();
            }
            else
            base.HandleLButtonDown(e);

        }
        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            ToggleOpen();
            this.Invalidate();
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
            switch(this.Orientation)
            {
                case OpenOrientation.Below:
                    this.Panel.Location = this.ScreenLocation + new Vector2(-Panel.BackgroundStyle.Border, this.Height);
                    //this.Panel.LocationFunc = ()=>this.ScreenLocation + new Vector2(-Panel.BackgroundStyle.Border, this.Height);
                    this.Panel.ConformToScreen();
                    break;

                case OpenOrientation.Above:
                    this.Panel.Location = this.ScreenLocation - new Vector2(0, this.Panel.Height);
                    //this.Panel.LocationFunc = () => this.ScreenLocation - new Vector2(0, this.Panel.Height);
                    this.Panel.ConformToScreen();
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
            //UIManager.DrawStringOutlined(sb, this.Text, new Vector2(this.Width / 2 - 8, Open ? 1 : 0), new Vector2(0.5f, 0));
            UIManager.DrawStringOutlined(sb, this.Text, new Vector2(this.Width / 2 - 8, Open ? 1 : this.Height/2), new Vector2(0.5f, .5f));

        }

        static Rectangle
            unpressedBox = new(0, 0, 27, 23),
            pressedBox = new(0, 23, 27, 23),
            left = new(0, 0, 4, 23),
            center = new(4, 0, 1, 23),
            right = new(5, 0, 22, 23);
    }
}
