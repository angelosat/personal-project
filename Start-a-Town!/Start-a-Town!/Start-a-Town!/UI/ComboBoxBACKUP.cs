using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class ComboBox : ListControl//, IScrollable
    {
        ListBox DropDownBox;
        Panel DropDownPanel;
        
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

        //protected int _SelectedIndex;
        public override int SelectedIndex
        {
            get
            {
                return _SelectedIndex;
            }
            set
            {
                _SelectedIndex = value;
                SelectedValue = Items[_SelectedIndex];
            }
        }

        public enum States { CBST_CLOSED, CBST_OPEN};
        int state = (int)States.CBST_CLOSED;
        public int GetState() { return state; }
        public void SetState(States state) { this.state = (int)state; }
        public event UIEvent DropDown, DropDownClosed, TextChanged;

        protected Texture2D TextSprite;
        protected string _Text;
        public string Text
        {
            get { return _Text; }
            set
            {
                _Text = value;
                OnTextChanged();
            }
        }
  
        protected override void OnSelectedValueChanged()
        {
            if (DisplayMember != "")
                Text = (string)SelectedValue.GetType().GetProperty(DisplayMember).GetValue(SelectedValue, null);
            else
            {
                if (DisplayMemberFunc != null)
                    Text = DisplayMemberFunc(SelectedValue);
                else
                    Text = SelectedValue.ToString();
            }
            base.OnSelectedValueChanged();
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
            Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            base.OnLostFocus();
        }

        protected override void OnMouseLeftPress(InputState e)
        {
            DroppedDown = !DroppedDown;
            if (DroppedDown)
                Build();
            else
                OnDropDownClosed();
            base.OnMouseLeftPress(e);
        }

        public ComboBox(Vector2 location, int width, string label = "")
            : base(location)
        {
            //Controls = new List<Control>();
            Width = width;
            Height = UIManager.DefaultButtonHeight;
            //Update();
            Items = new ObjectCollection(this);
            //Controls = new List<Control>();
            Text = label;
            Alpha = Color.Lerp(Color.Transparent, Color.White, 0.5f);
        }

        public override void Build()
        {
            DropDownPanel = new Panel(new Vector2(0, Bottom), new Vector2(Width, Game1.Instance.graphics.PreferredBackBufferHeight - (int)ScreenLocation.Y - Height));
            DropDownBox = new ListBox(Vector2.Zero, DropDownPanel.Size); //DropDownPanel.ClientSize);
            DropDownBox.DisplayMember = DisplayMember;
            DropDownBox.ValueMember = ValueMember;
            DropDownBox.DisplayMemberFunc = DisplayMemberFunc; 
            int maxwidth = Width, height = 0;
            foreach (object obj in Items)
            {
                if (DisplayMember != "")
                    maxwidth = Math.Max(maxwidth, (int)UIManager.Font.MeasureString((string)obj.GetType().GetProperty(DisplayMember).GetValue(obj, null)).X);
                else
                {
                    if(DisplayMemberFunc !=null)
                        maxwidth = Math.Max(maxwidth, (int)UIManager.Font.MeasureString(DisplayMemberFunc(obj)).X);
                    else
                    maxwidth = Math.Max(maxwidth, (int)UIManager.Font.MeasureString(obj.ToString()).X);
                }
                height += UI.Label.DefaultHeight;
                DropDownBox.Items.Add(obj);
            }
            //DropDownBox.Width = Math.Max(Width, maxwidth);
            DropDownBox.Width = DropDownPanel.ClientSize.Width;
            DropDownBox.Height = Math.Min(height, Game1.Instance.graphics.PreferredBackBufferHeight - (int)ScreenLocation.Y - Height);
            DropDownBox.Build();
            DropDownBox.Click += new UIEvent(DropDownBox_Click);

            //DropDownPanel.AutoSize = true;
            DropDownPanel.Controls.Add(DropDownBox);
            DropDownPanel.Opacity = 1;
            //FindWindow().Controls.Add(DropDownPanel);
            Parent.Controls.Add(DropDownPanel);
            DropDownPanel.ClientSize = new Rectangle(0, 0, DropDownPanel.ClientSize.Width, height);
            DropDownBox.Size = DropDownPanel.ClientSize;
            DropDownBox.SelectedIndex = Items.IndexOf(SelectedValue);
            SelectedIndex = DropDownBox.SelectedIndex;
            DropDownBox.SelectedValueChanged += new EventHandler<EventArgs>(DropDownBox_SelectedValueChanged);
        }

        void DropDownBox_Click(object sender, EventArgs e)
        {
            DroppedDown = false;
            OnDropDownClosed();
            base.OnClick();
        }

        void DropDownBox_SelectedValueChanged(object sender, EventArgs e)
        {
            SelectedValue = (sender as ListBox).SelectedValue;
        }

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

            //if (List != null)
            //    List.Draw(sb);
            //sb.Draw(TextSprite, ScreenLocation + new Vector2(Width / 2, Convert.ToInt32(isPressed) + Height / 2), null, Color.White, 0, new Vector2((TextSprite.Width) / 2, (TextSprite.Height) / 2), 1, SpriteEffects.None, Depth);
            sb.Draw(TextSprite, ScreenLocation + new Vector2(0, Convert.ToInt32(isPressed) + Height / 2), null, Color.White, 0, new Vector2(-UIManager.BorderPx, (TextSprite.Height) / 2), 1, SpriteEffects.None, Depth);
            //foreach (Control control in Controls)
            //    control.Draw(sb);
            base.Draw(sb);
        }

        //protected override void Parent_MouseLeftRelease(object sender, EventArgs e)
        //{

        //}
        //protected override void Parent_MouseLeftPress(object sender, EventArgs e)
        //{
        //    if (!DroppedDown)
        //    {
        //        if (IsTopMost)
        //            DroppedDown = true;
        //    }
        //    else
        //    {
        //        if (!List.HitTest())
        //            DroppedDown = false;
        //    }
        //}


        //void List_SelectedValueChanged(Object sender, EventArgs e)
        //{
        //    //SelectedValue = List.SelectedValue;
        //    Text = Values[SelectedValue];
        //    DroppedDown = false;
        //}

        protected void OnDropDown()
        {
            if (DropDown != null)
                DropDown(this, new UIEventArgs(this));
        }
        protected void OnDropDownClosed()
        {
            DropDownBox.Click -= DropDownBox_Click;
            DropDownBox.SelectedValueChanged -= DropDownBox_SelectedValueChanged;
            foreach (Control control in Controls)
                control.Dispose();
            //Controls.Clear();
            Parent.Controls.Remove(DropDownPanel);
            if (DropDownClosed != null)
                DropDownClosed(this, new UIEventArgs(this));
        }
        protected void OnTextChanged()
        {
            TextSprite = UIManager.DrawTextOutlined(Text);
            if (TextChanged != null)
                TextChanged(this, new UIEventArgs(this));
        }
        public override void Paint()
        {
            TextSprite = UIManager.DrawTextOutlined(Text);
            base.Paint();
        }
    }
}
