using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class Window : Control
    {
        public override string ToString()
        {
            return "Window: " + this.title;
        }
        private string title = "<undefined>";
        private Rectangle contentArea = new Rectangle();
        bool isResizable = false;
        public bool FocusLast = false, isMouseOverCurrent = false, isMouseOverPrevious = false;
        public Window Previous;
        public bool Movable, IsDragged;
        protected Rectangle ScreenClientBounds = new Rectangle();
        public event InputEvent FocusChanged;
        protected bool _Closable = true;
        public bool Closable
        {
            get { return _Closable; }
            set 
            {
                bool oldClosable = Closable;
                _Closable = value;
                if(_Closable != oldClosable)
                {
                    if (_Closable)
                        Controls.Add(CloseButton);
                    else
                        Controls.Remove(CloseButton);
                    if(AutoSize)
                        ClientSize = PreferredClientSize;
                }
            }
        }

        public override void PreparingPaint()
        {
        }

        public override void OnPaint(SpriteBatch sb)
        {
            DrawOuterPadding(sb, Vector2.Zero);
        }
        public override Control AddControls(params Control[] controls)
        {
            foreach (var ctrl in controls)
                this.Client.Controls.Add(ctrl);
            return this;
        }
        public Label Label_Title;
        public GroupBox Client;
        protected UICloseButton CloseButton;
        protected override void OnClientSizeChanged()
        {
            Size = new Rectangle(0, 0, ClientSize.Width + 2 * UIManager.BorderPx, ClientSize.Height + 2 * UIManager.BorderPx + (int)UIManager.Font.MeasureString(Title).Y);
            base.OnClientSizeChanged();
        }
        protected virtual void OnClosed()
        {
        }
        
        public virtual bool Close()
        {
            return this.Hide();
        }

        /// <summary>
        /// Closes the previous window and opens the current one, opens the previous window when current one closes 
        /// </summary>
        /// <param name="previous"></param>
        /// <returns></returns>
        internal virtual Window ShowFrom(Window previous)
        {
            this.Previous = previous;
            previous.Hide();
            this.Show();
            return this;
        }
        public override void Dispose()
        {
            base.Dispose();
        }

        public Vector2 CenterMouseOnControl(Control control)
        {
            Location = Vector2.Zero;
            Vector2 controlLoc = control.GetLocation();
            return UIManager.Mouse - controlLoc - new Vector2(control.Width, control.Height) / 2;
        }
       
        public Window(Control child, bool closable = true) : this("", child, closable)
        {
        }
        public Window(string name, Control child, bool closable = true) : this()
        {
            this.Title = name; 
            this.Movable = true;
            this.AutoSize = true;
            this.Closable = closable;
            this.Client.AddControls(child);
        }
        public Window()
        {
            this.Color = UIManager.Tint;
            ClientLocation = new Vector2(UIManager.BorderPx, UIManager.BorderPx);// + (int)UIManager.Font.MeasureString(Title).Y);
            this.Client = new GroupBox() { Name = "Window client area", MouseThrough = true, Size = ClientSize, Location = new Vector2(0, Label.DefaultHeight) };
            CloseButton = new UICloseButton();
            CloseButton.Location = new Vector2(Width - 16 - UIManager.BorderPx - ClientLocation.X, UIManager.BorderPx - ClientLocation.Y);
            CloseButton.LeftClick += new UIEvent(close_button_Click);
            this.Label_Title = new Label() { Font = UIManager.FontBold, MouseThrough = true};//false, Active = true };//true };
            Label_Title.MouseLBActionOld = StartDragging;
            AutoSize = true;
            Controls.Add(Label_Title, CloseButton, Client);
            AutoSize = false;
            Select();
        }

        void close_button_Click(Object sender, EventArgs e)
        {
            this.Hide();
        }
 
        public override void Update()
        {
            base.Update();

            if (this.TitleFunc != null)
            {
                var nowTitle = this.TitleFunc();
                if (this.LastTitle != nowTitle)
                {
                    this.OnTitleChanged();
                }
                this.LastTitle = nowTitle;
            }
            Drag();
            ScreenClientBounds = new Rectangle((int)ScreenClientLocation.X - UIManager.BorderPx, (int)ScreenClientLocation.Y - UIManager.BorderPx, ClientSize.Width + 2 * UIManager.BorderPx, ClientSize.Height + 2 * UIManager.BorderPx);
            if (Focused != FocusLast)
                if (FocusChanged != null)
                    FocusChanged();
            FocusLast = Focused;
        }

        private void Drag()
        {
            if (IsDragged)
            {
                Location.X = Math.Max(0, Math.Min(UIManager.Width - Width, UIManager.Mouse.X - (int)MouseOffset.X));
                Location.Y = Math.Max(0, Math.Min(UIManager.Height - Height, UIManager.Mouse.Y - (int)MouseOffset.Y));
            }
        }
        public override bool MouseThrough
        {
            get
            {
                return base.MouseThrough;
            }
            set
            {
                base.MouseThrough = value;
                this.Client.MouseThrough = value;
            }
        }
        static protected Vector2 TitleLocation = new Vector2(UIManager.BorderPx * 2, UIManager.BorderPx);
        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
        
        private void DrawOuterPadding(SpriteBatch sb, Vector2 loc)
        {
            Color color = this.Color;
            sb.Draw(UIManager.frameSprite, loc, OuterPadding.TopLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.frameSprite, loc + new Vector2( - 11 + Width, 0), OuterPadding.TopRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.frameSprite, loc + new Vector2(0,  - 11 + Height), OuterPadding.BottomLeft, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.frameSprite, loc + new Vector2( - 11 + Width,  - 11 + Height), OuterPadding.BottomRight, color, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

            //top, left, right, bottom
            sb.Draw(UIManager.frameSprite, new Rectangle((int)loc.X + 11, (int)loc.Y, Width - 22, 11), OuterPadding.Top, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.frameSprite, new Rectangle((int)loc.X, (int)loc.Y + 11, 11, Height - 22), OuterPadding.Left, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.frameSprite, new Rectangle((int)loc.X + Width - 11, (int)loc.Y + 11, 11, Height - 22), OuterPadding.Right, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.frameSprite, new Rectangle((int)loc.X + 11, (int)loc.Y - 11 + Height, Width - 22, 11), OuterPadding.Bottom, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

            //center
            sb.Draw(UIManager.frameSprite, new Rectangle((int)loc.X + 11, (int)loc.Y + 11, Width - 22, Height - 22), OuterPadding.Center, color, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        }

        internal Window SetTitle(string text)
        {
            this.Title = text;
            return this;
        }

        string LastTitle;
        public Func<string> TitleFunc;
        public string Title
        {
            get { return this.TitleFunc != null ? this.TitleFunc() : title; }
            set
            {
                title = value;
                OnTitleChanged();
            }
        }
        public bool IsResizable
        { get { return isResizable; } }
       
        public void SizeToControl(Control control)
        {
            Size = new Rectangle(0, 0, control.Width + 2 * UIManager.BorderPx, control.Height + 2 * UIManager.BorderPx + (int)UIManager.Font.MeasureString(Title).Y);
        }
        public void OnTitleChanged()
        {
            if (Title.Length == 0)
            {
                this.Controls.Remove(Label_Title);
                this.Client.Location = Vector2.Zero;
            }
            else
            {
                this.Label_Title.Text = Title;
                if (!this.Controls.Contains(this.Label_Title))
                {
                    this.Controls.Add(this.Label_Title);
                    this.Client.Location = this.Label_Title.BottomLeft;
                }

            }
            this.Controls.Remove(this.CloseButton);
            ClientSize = PreferredClientSize;
            if (this._Closable)
            {
                this.Controls.Add(this.CloseButton);
                this.CloseButton.Location.X = Math.Max(this.CloseButton.Location.X, this.Label_Title.Right);
            }

        }

        public override Vector2 Dimensions
        {
            get
            {
                return base.Dimensions;
            }
            set
            {
                base.Dimensions = value;

                Client.Dimensions = new Vector2(Width, Height) - new Vector2(UIManager.BorderPx * 2) - new Vector2(0, (int)UIManager.Font.MeasureString(Title).Y);
            }
        }

        public virtual bool ToggleDialog()
        {
            if (this.ShowDialog())
                return true;
            else
                return !Hide();
        }
        public override bool Show()
        {
            this.ConformToScreen();
            return base.Show();
        }
        public override bool Hide()
        {
            if (WindowManager.RemoveWindow(this))
            {
                if (DialogBlock is not null)
                {
                    DialogBlock = null;
                }
                OnHidden();

                Previous?.Show(WindowManager);

                this.Client.OnWindowHidden(this);

                return true;
            }

            return false;
        }

        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            e.Handled = true;
            StartDragging();
            base.OnMouseLeftPress(e);
        }

        private void StartDragging()
        {
            IsDragged = Movable;
            MouseOffset = UIManager.Mouse - ScreenLocation;
        }

        public override Rectangle PreferredClientSize
        {
            get
            {
                int width = 0;
                foreach (Control control in this.Controls)
                    width = Math.Max(width, (int)control.Location.X + control.Width - (int)control.Origin.X);
                int height = 0;
                foreach (Control control in this.Client.Controls)
                {
                    if (control != CloseButton)
                    {
                        width = Math.Max(width, (int)control.Location.X + control.Width - (int)control.Origin.X);
                        height = Math.Max(height, (int)control.Location.Y + control.Height - (int)control.Origin.Y);
                    }
                }
                CloseButton.Location = new Vector2(Width - 16 - UIManager.BorderPx - ClientLocation.X, UIManager.BorderPx - ClientLocation.Y);
                return new Rectangle(0, 0, width, height);
            }
        }
        public void AlignToMouse(Vector2 loc)
        {
            Location = UIManager.Mouse - ClientLocation - loc;
        }

        internal Window Transparent()
        {
            this.SetOpacity(0);
            this.SetMousethrough(true);
            this.Label_Title.MouseThrough = false;
            this.Label_Title.Active = true;
            return this;
        }
    }
}
