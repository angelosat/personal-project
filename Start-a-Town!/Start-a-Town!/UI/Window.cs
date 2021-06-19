using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Start_a_Town_.UI
{
    public class Window : Control, ICloseable//, IDisposable
    {
        public override string ToString()
        {
            return "Window: " + this.title;
        }
        private string title = "<undefined>";
        private Rectangle contentArea = new Rectangle();
        //private Rectangle icon;
       // private Texture2D TitleSprite;
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
                        //Size = PreferredClientSize;
                        ClientSize = PreferredClientSize;
                }
            }
        }

        //Control _ActiveControl;
        //public Control ActiveControl
        //{
        //    get { return _ActiveControl; }
        //    set
        //    {
        //        Control old = _ActiveControl;
        //        _ActiveControl = null;

        //    }
        //}

        public override void PreparingPaint()
        {
            //TitleSprite = UIManager.DrawTextOutlined(title);
            //base.Paint();
        }

        public override void OnPaint(SpriteBatch sb)
        {
            DrawOuterPadding(sb, Vector2.Zero);
           // sb.Draw(TitleSprite, TitleLocation, null, Color.White, 0, new Vector2(0), 1, SpriteEffects.None, 0.1f);
        }
        public override Control AddControls(params Control[] controls)
        {
            foreach (var ctrl in controls)
                this.Client.Controls.Add(ctrl);
            return this;
        }
        //protected UICloseButton CloseButton;
        public Label Label_Title;
        public GroupBox Client;
        protected UICloseButton CloseButton;
      //  protected Control CancelButton;
        protected override void OnClientSizeChanged()
        {
            Size = new Rectangle(0, 0, ClientSize.Width + 2 * UIManager.BorderPx, ClientSize.Height + 2 * UIManager.BorderPx + (int)UIManager.Font.MeasureString(Title).Y);
            base.OnClientSizeChanged();
        }
        //public event EventHandler<EventArgs> Closed;
        protected virtual void OnClosed()
        {
            //if (Closed != null)
            //    Closed(this, EventArgs.Empty);
        }
        
        public virtual bool Close()
        {
            return this.Hide();
           //if (WindowManager.RemoveWindow(this))
           // {
           //     //Instance = null;
           //     if (DialogBlock != null)
           //     {
           //         WindowManager.Layers[LayerTypes.Dialog].Remove(DialogBlock);
           //         DialogBlock = null;
           //         //Controller.InputHandlers.Remove(InputBlock);
           //         InputBlock = null;
           //     }
           //    // Dispose();
           //     OnClosed();

           //     if (Previous != null)
           //         Previous.Show(WindowManager);

           //     return true;
           // }
            
           // return false;
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
            //WindowManager.KeyPress -= WindowManager_KeyPress;
         //   Game1.Instance.graphics.DeviceReset -= graphics_DeviceReset;
            base.Dispose();
        }

        //public Vector2 CenterScreen
        //{ get { return new Vector2((Game1.Instance.graphics.PreferredBackBufferWidth - Width) / 2, (Game1.Instance.graphics.PreferredBackBufferHeight - Height) / 2); } }
        
        public Vector2 CenterMouseOnControl(Control control)
        {
            Location = Vector2.Zero;
       //     return UIManager.Mouse - ClientLocation - control.GetScreenLocation() - new Vector2(control.Width, control.Height) / 2;
            Vector2 controlLoc = control.GetLocation();
            return UIManager.Mouse - controlLoc - new Vector2(control.Width, control.Height) / 2;
        }
        //public override void Dispose()
        //{
        //    Controller.EscapeDown -= Controller_EscapeDown;
        //    base.Dispose();
        //}
        public Window(Control child, bool closable = true) : this("", child, closable)
        {
        }
        public Window(string name, Control child, bool closable = true) : this()
        {
            //this.Label_Title.Text = name;
            this.Title = name; 
            this.Movable = true;
            this.AutoSize = true;
            //this.Location = UIManager.Mouse; 
            this.Closable = closable;
            this.Client.AddControls(child);
        }
        public Window()
        {

            //TitleSprite = UIManager.DrawTextOutlined(title);
            this.Color = UIManager.Tint;
            ClientLocation = new Vector2(UIManager.BorderPx, UIManager.BorderPx);// + (int)UIManager.Font.MeasureString(Title).Y);
           // ClientSize = new Rectangle(0, 0, Width - (int)ClientLocation.X - 2 * UIManager.BorderPx, Height - (int)ClientLocation.Y - 2 * UIManager.BorderPx);
            //Client = new GroupBox() { Name = "Window client area", MouseThrough = true, Size = ClientSize, Location = new Vector2(0, (int)UIManager.Font.MeasureString(Title).Y) };
            this.Client = new GroupBox() { Name = "Window client area", MouseThrough = true, Size = ClientSize, Location = new Vector2(0, Label.DefaultHeight) };
            CloseButton = new UICloseButton();
            CloseButton.Location = new Vector2(Width - 16 - UIManager.BorderPx - ClientLocation.X, UIManager.BorderPx - ClientLocation.Y);
            CloseButton.LeftClick+=new UIEvent(close_button_Click);

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
            //Close();
        }
 
        public override void Update()
        {

          //  Opacity += 0.1f * GlobalVars.DeltaTime;
           // Opacity = Math.Min(Opacity, 1);
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
            //List<Control> controls = Controls.ToList();
            //foreach (Control control in controls)
            //    control.Update();
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
            //this.ScreenBounds.DrawHighlight(sb);
            base.Draw(sb, viewport);
            //this.Bounds.DrawHighlight(sb);
            //(new Rectangle((int)Location.X, (int)Location.Y, Width, Height)).DrawHighlight(sb, 0.5f);
           // (new Rectangle((int)ScreenClientLocation.X, (int)ScreenClientLocation.Y, ClientSize.Width, ClientSize.Height)).DrawHighlight(sb, 0.5f);
        }
        //public override void Draw(SpriteBatch sb)
        //{
        //    //DrawOuterPadding(sb, ScreenLocation);
        //    //sb.Draw(TitleSprite, ScreenLocation + TitleLocation, null, Color.Lerp(Color.Transparent, Color.White, Opacity), 0, new Vector2(0), 1, SpriteEffects.None, 0.1f);

        //    //this.Bounds.DrawHighlight(sb);
        //    base.Draw(sb, UIManager.Bounds);
        //}
        private void DrawOuterPadding(SpriteBatch sb, Vector2 loc)
        {
            Color color = this.Color;// UIManager.Tint;// Color.Lerp(Color.Transparent, UIManager.uiTint, Opacity);
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
        private void DrawInnderPadding(SpriteBatch sb)
        { 
            //topleft, topright, bottomleft, bottomright
            Color tint = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            sb.Draw(UIManager.SlotSprite, new Vector2(contentArea.X, contentArea.Y), InnerPadding.TopLeft, tint, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.SlotSprite, new Vector2(contentArea.X + contentArea.Width - 19, contentArea.Y), InnerPadding.TopRight, tint, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.SlotSprite, new Vector2(contentArea.X, contentArea.Y + contentArea.Height - 19), InnerPadding.BottomLeft, tint, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.SlotSprite, new Vector2(contentArea.X + contentArea.Width - 19, contentArea.Y - 19 + contentArea.Height), InnerPadding.BottomRight, tint, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

            //top, left, right, bottom
            sb.Draw(UIManager.SlotSprite, new Rectangle(contentArea.X + 19, contentArea.Y, contentArea.Width - 38, 19), InnerPadding.Top, tint, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.SlotSprite, new Rectangle(contentArea.X, contentArea.Y + 19, 19, contentArea.Height - 38), InnerPadding.Left, tint, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.SlotSprite, new Rectangle(contentArea.X + contentArea.Width - 19, contentArea.Y + 19, 19, contentArea.Height - 38), InnerPadding.Right, tint, 0, new Vector2(0, 0), SpriteEffects.None, 0.001f);
            sb.Draw(UIManager.SlotSprite, new Rectangle(contentArea.X + 19, contentArea.Y + contentArea.Height - 19, contentArea.Width - 38, 19), InnerPadding.Bottom, tint, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

            //center
            sb.Draw(UIManager.SlotSprite, new Rectangle(contentArea.X + 19, contentArea.Y + 19, contentArea.Width - 38, contentArea.Height - 38), InnerPadding.Center, tint, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        }
        private void DrawInnderPadding2(SpriteBatch sb)
        {
            //topleft, topright, bottomleft, bottomright
            Color tint = Color.Lerp(Color.Transparent, Color.White, 0.5f);
            sb.Draw(UIManager.SlotSprite, new Vector2(ScreenClientBounds.X, ScreenClientBounds.Y), InnerPadding.TopLeft, tint, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.SlotSprite, new Vector2(ScreenClientBounds.X + ScreenClientBounds.Width - 19, ScreenClientBounds.Y), InnerPadding.TopRight, tint, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.SlotSprite, new Vector2(ScreenClientBounds.X, ScreenClientBounds.Y + ScreenClientBounds.Height - 19), InnerPadding.BottomLeft, tint, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.SlotSprite, new Vector2(ScreenClientBounds.X + ScreenClientBounds.Width - 19, ScreenClientBounds.Y - 19 + ScreenClientBounds.Height), InnerPadding.BottomRight, tint, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

            //top, left, right, bottom
            sb.Draw(UIManager.SlotSprite, new Rectangle(ScreenClientBounds.X + 19, ScreenClientBounds.Y, ScreenClientBounds.Width - 38, 19), InnerPadding.Top, tint, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.SlotSprite, new Rectangle(ScreenClientBounds.X, ScreenClientBounds.Y + 19, 19, ScreenClientBounds.Height - 38), InnerPadding.Left, tint, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
            sb.Draw(UIManager.SlotSprite, new Rectangle(ScreenClientBounds.X + ScreenClientBounds.Width - 19, ScreenClientBounds.Y + 19, 19, ScreenClientBounds.Height - 38), InnerPadding.Right, tint, 0, new Vector2(0, 0), SpriteEffects.None, 0.001f);
            sb.Draw(UIManager.SlotSprite, new Rectangle(ScreenClientBounds.X + 19, ScreenClientBounds.Y + ScreenClientBounds.Height - 19, ScreenClientBounds.Width - 38, 19), InnerPadding.Bottom, tint, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

            //center
            sb.Draw(UIManager.SlotSprite, new Rectangle(ScreenClientBounds.X + 19, ScreenClientBounds.Y + 19, ScreenClientBounds.Width - 38, ScreenClientBounds.Height - 38), InnerPadding.Center, tint, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
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
            //Width = control.Width + 2 * UIManager.BorderPx;
            //Height = control.Height + 2 * UIManager.BorderPx + (int)UIManager.Font.MeasureString(Title).Y;
            ////Size.Width = Width;
            ////Size.Height = Height;
            //OnSizeChanged();
            Size = new Rectangle(0, 0, control.Width + 2 * UIManager.BorderPx, control.Height + 2 * UIManager.BorderPx + (int)UIManager.Font.MeasureString(Title).Y);
        }
        public void OnTitleChanged()
        {
            //TitleSprite = UIManager.DrawTextOutlined(title);
            //Width = Math.Max(Width, TitleSprite.Width);
            if (Title.Length == 0)
            {
                this.Controls.Remove(Label_Title);
                this.Client.Location = Vector2.Zero;// new Vector2(UIManager.BorderPx);
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

        //protected override void OnMouseMove(InputState e)
        //{
        //    if (IsDragged)
        //    {
        //        //ScreenLocation = Controller.Location - MouseOffset;
        //        //Location.X = Math.Max(0, Math.Min(UIManager.Width - Width, UIManager.Mouse.X - (int)MouseOffset.X / UIManager.Scale));
        //        //Location.Y = Math.Max(0, Math.Min(UIManager.Height - Height, UIManager.Mouse.Y - (int)MouseOffset.Y / UIManager.Scale));
        //        //Location.X = Controller.X - (int)MouseOffset.X;
        //        //Location.Y = Controller.Y - (int)MouseOffset.Y;

           
                
        //            Location.X = Math.Max(0, Math.Min(UIManager.Width - Width, UIManager.Mouse.X - (int)MouseOffset.X));
        //            Location.Y = Math.Max(0, Math.Min(UIManager.Height - Height, UIManager.Mouse.Y - (int)MouseOffset.Y));
                
        //    }
        //    base.OnMouseMove(e);
        //}


        

        //public void CenterToScreen()
        //{
        //    this.Location = this.CenterScreen - this.Center;
        //}

        public virtual bool ToggleDialog()
        {
            if (this.ShowDialog())
                return true;
            else
                return !Hide();
        }
        public override bool Show()
        {
            // i did this to prevent close button getting behind a control in case of no title label
            //this.CloseButton.Location = new Vector2(Math.Max(Width - 16 - UIManager.BorderPx - ClientLocation.X, this.Controls.TopRight.X), this.CloseButton.Location.Y);
            //ClientSize = PreferredSize;
            this.ConformToScreen();
            //this.CenterScreen();
            return base.Show();
        }
        public override bool Hide()
        {
            if (WindowManager.RemoveWindow(this))
            {
                //Instance = null;
                if (DialogBlock != null)
                {
                  //  WindowManager["Dialog"].Remove(DialogBlock);
                    DialogBlock = null;
                    //InputBlock = null;
                }
                OnHidden();

                if (Previous != null)
                    Previous.Show(WindowManager);

                this.Client.OnWindowHidden(this);

                return true;
            }

            return false;
        }

        protected override void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //if (Controller.Instance.Mouseover.Object != this)
            //    return;
            e.Handled = true;
            StartDragging();
            base.OnMouseLeftPress(e);
        }

        private void StartDragging()
        {
            IsDragged = Movable;
            //MouseOffset = Controller.Instance.MouseLocation - ScreenLocation;
            MouseOffset = UIManager.Mouse - ScreenLocation;
        }

        protected override void OnMouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            StopDragging();
            base.OnMouseLeftUp(e);
        }

        private void StopDragging()
        {
            IsDragged = false;
        }

        public override Rectangle PreferredClientSize
        {
            get
            {
                int width = 0;// Label_Title.Width + CloseButton.Width
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
                //Controls.Add(CloseButton);
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
