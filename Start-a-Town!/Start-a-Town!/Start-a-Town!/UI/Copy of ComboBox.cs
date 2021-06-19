using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class ComboBox<T> : ButtonBase //Control//, IScrollable
    {
        VScrollbar Scroll;
        ScrollableList List;
        Panel Panel_list;
        public T SelectedItem;
        public Func<T, string> TextFunction;
        public List<T> ItemList;

        public event EventHandler<EventArgs> SelectedItemChanged;

        bool _DroppedDown;
        public bool DroppedDown
        {
            get { return _DroppedDown; }
            set
            {
                _DroppedDown = value;
                SprFx = _DroppedDown ? SpriteEffects.FlipVertically : SpriteEffects.None;
            }
        }


        public enum States { CBST_CLOSED, CBST_OPEN};
        int state = (int)States.CBST_CLOSED;
        string DefaultText;
        public int GetState() { return state; }
        public void SetState(States state) { this.state = (int)state; }
        public event UIEvent DropDown, DropDownClosed, TextChanged;

        //protected Texture2D TextSprite;
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
  
        protected void OnSelectedItemChanged()
        {
            if (SelectedItemChanged != null)
                SelectedItemChanged(this, EventArgs.Empty);
        }

        private Rectangle
            unpressedBox = new Rectangle(0, 0, 27, 23),
            pressedBox = new Rectangle(0, 23, 27, 23),
            left = new Rectangle(0, 0, 4, 23),
            center = new Rectangle(4, 0, 1, 23),
            right = new Rectangle(5, 0, 22, 23);

        //public ComboBox(Control parent, Vector2 Location, int width, string label, List<string> options)
        //    : base(parent, Location)
        //{
        //    Controls = new List<Control>();
        //    Width = width;
        //    Height = UIManager.DefaultButtonHeight;
        //    Update();
        //    Values = options;
        //    Text = options[0];
        //}

        protected override void OnGotFocus()
        {
            Alpha = Color.White;
            base.OnGotFocus();
        }
        public override void OnLostFocus()
        {
            //Pressed = false;
          //  Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            base.OnLostFocus();
        }

        public void Close()
        {
            if (!DroppedDown)
                return;
            DroppedDown = false;
            Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            Parent.Controls.Remove(Panel_list);
        }

        public void Open()
        {
            if (DroppedDown)
                return;
            DroppedDown = true;
            Parent.Controls.Add(Panel_list);
        }

        public new void Toggle()
        {
            if (DroppedDown)
                Close();
            else
                Open();
        }

        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //DroppedDown = !DroppedDown;
            

            //if (DroppedDown)
            //    Parent.Controls.Add(DropDownPanel);
            //else
            //    Parent.Controls.Remove(DropDownPanel);
            Toggle();
            base.OnMouseLeftPress(e);
        }

        public ComboBox(Vector2 location, int width, string defaultText, List<T> items, Func<T, string> func)
            : base(location)
        {
            Width = width;
            Height = UIManager.DefaultButtonHeight;
            this.ItemList = items;
            TextFunction = func;
            DefaultText = defaultText;
            Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            Build();
        }

        public void Build()
        {
            Panel_list = new Panel(new Vector2(0, Bottom), new Vector2(Width, Math.Min(ItemList.Count * Button.DefaultHeight, 200))); //Game1.ScreenSize.Y - (int)ScreenLocation.Y - Height)));
            //DropDownPanel.AutoSize = true;
            //DropDownPanel.Color = Color.Black;
            List = new ScrollableList(Vector2.Zero, Panel_list.ClientSize.Width, Panel_list.ClientSize.Height);// (int)Math.Min(ItemList.Count * Button.DefaultHeight, 200)); //DropDownPanel.Size); //DropDownPanel.ClientSize);
            List.ControlAdded += new EventHandler<EventArgs>(List_ControlAdded);
            List.ControlRemoved += new EventHandler<EventArgs>(List_ControlRemoved);
            Scroll = new VScrollbar(new Vector2(Panel_list.ClientSize.Width - VScrollbar.Width, 0), Panel_list.ClientSize.Height);
            Scroll.Tag = List;
            foreach (T obj in ItemList)
            {
                //Label label = new Label(new Vector2(0, DropDownBox.Controls.Count * Label.DefaultHeight), TextFunction(obj));
                Button label = new Button(new Vector2(0, List.Controls.Count * Button.DefaultHeight), Panel_list.ClientSize.Width, TextFunction(obj));
                //   label.Color = Color.Black;
                label.Tag = obj;
                label.Click += new UIEvent(label_Click);
                List.Add(label);
            }
            Panel_list.Controls.Add(List);
            
            if (!ItemList.Contains(SelectedItem))
                Text = DefaultText;
        }

        void List_ControlRemoved(object sender, EventArgs e)
        {
            ScrollableList list = sender as ScrollableList;
            if (list.ClientSize.Height <= ClientSize.Height)
            {
                list.Size = new Rectangle(0, 0, Panel_list.ClientSize.Width, list.Size.Height);
                Controls.Remove(Scroll);
                foreach (ButtonBase btn in List.Controls)
                    btn.Width = Panel_list.ClientSize.Width;
            }
        }

        void List_ControlAdded(object sender, EventArgs e)
        {
            ScrollableList list = sender as ScrollableList;
            if (list.ClientSize.Height > ClientSize.Height)
            {
                list.Size = new Rectangle(0, 0, Panel_list.ClientSize.Width - 16, list.Size.Height);
                Panel_list.Controls.Add(Scroll);
                foreach (ButtonBase btn in List.Controls)
                    btn.Width = Panel_list.ClientSize.Width - VScrollbar.Width;
            }
        }

        void label_Click(object sender, EventArgs e)
        {
            if (!Panel_list.ScreenClientRectangle.Intersects(new Rectangle((int)UIManager.Mouse.X, (int)UIManager.Mouse.Y, 1, 1)))
                return;
            ButtonBase label = sender as ButtonBase;
            SelectedItem = (T)label.Tag;
            List.SelectedItem = SelectedItem;
            //Text = label.Text;
            Text = TextFunction(SelectedItem);
            OnSelectedItemChanged();
        }

        //void DropDownBox_Click(object sender, EventArgs e)
        //{
        //    DroppedDown = false;
        //    //OnDropDownClosed();
        //    base.OnClick();
        //}

        //void DropDownBox_SelectedValueChanged(object sender, EventArgs e)
        //{
        //    SelectedItem = (sender as ListBox).SelectedValue;
        //}

        public override void Update()
        {
            //t += (MouseHover ? dt : -dt) * UIFpsCounter.deltaTime;
            //Alpha = Color.Lerp(Color.Transparent, Color.White, Math.Max(0.5f, Math.Min(1, t)));
            base.Update();
            //if (List != null)
            //    List.Update();
            foreach (Control control in Controls)
                control.Update();
        }
        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(UIManager.DefaultDropdownSprite, ScreenLocation, left, Alpha, 0, new Vector2(0), 1, SprFx, Depth);
            sb.Draw(UIManager.DefaultDropdownSprite, new Rectangle(X + left.Width, Y, Width - (left.Width + right.Width), 23), center, Alpha, 0, new Vector2(0), SprFx, Depth);
            sb.Draw(UIManager.DefaultDropdownSprite, ScreenLocation + new Vector2(Width - right.Width, 0), right, Alpha, 0, new Vector2(0), 1, SprFx, Depth);

            //if(TextSprite!=null)
            //sb.Draw(TextSprite, ScreenLocation + new Vector2(0, Convert.ToInt32(isPressed) + Height / 2), null, Color.White, 0, new Vector2(-UIManager.BorderPx, (TextSprite.Height) / 2), 1, SpriteEffects.None, Depth);

            base.Draw(sb);
        }

        //protected void OnTextChanged()
        //{
        //    TextSprite = UIManager.DrawTextOutlined(Text);
        //    if (TextChanged != null)
        //        TextChanged(this, new UIEventArgs(this));
        //}
        //public override void Paint()
        //{
        //    TextSprite = UIManager.DrawTextOutlined(Text);
        //    base.Paint();
        //}
    }
}
