using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class Anchor 
    {
        static public Vector2 Center
        {
            get { return new Vector2(0.5f); }
        }
        static public Vector2 TopLeft
        {
            get { return Vector2.Zero; }
        }
    }
    class ControlEventArgs : EventArgs
    {
        public Control Control;
        public ControlEventArgs(Control control)
        {
            this.Control = control;
        }
    }
    public abstract class Control : Element, IDisposable, ITooltippable, IKeyEventHandler
    {
        //public Vector2 CenterScreen { get { return new Vector2((UIManager.Width - Width) / 2, (UIManager.Height - Height) / 2); } }
        //public Vector2 BottomLeftScreen
        //{ get { return new Vector2(0, UIManager.Height - Height); } }
        //public Vector2 BottomRightScreen
        //{ get { return new Vector2(UIManager.Width - Width, UIManager.Height - Height); } }
        //public Vector2 BottomCenterScreen
        //{ get { return new Vector2((UIManager.Width - Width) / 2, UIManager.Height - Height); } }
        //public Vector2 TopRightScreen
        //{ get { return new Vector2(UIManager.Width - Width, 0); } }

        //public Vector2 CenterScreen { get { return new Vector2((UIManager.Width - Width) / 2, (UIManager.Height - Height) / 2); } }
        public Vector2 CenterScreen { get { return new Vector2(UIManager.Width / 2, UIManager.Height / 2); } }
        public Vector2 BottomLeftScreen
        { get { return new Vector2(0, UIManager.Height - Height); } }
        public Vector2 BottomRightScreen
        { get { return new Vector2(UIManager.Width - Width, UIManager.Height - Height); } }
        public Vector2 BottomCenterScreen
        { get { return new Vector2((UIManager.Width - Width) / 2, UIManager.Height - Height); } }
        public Vector2 TopRightScreen
        { get { return new Vector2(UIManager.Width - Width, 0); } }

       

        public virtual void ConformToScreen()
        {
            this.Location.X = Math.Max(0, Math.Min(UIManager.Width - Width, this.Location.X));
            this.Location.Y = Math.Max(0, Math.Min(UIManager.Height - Height, this.Location.Y));
        }

        public float Rotation = 0;
        bool _MouseThrough = false;
        public bool MouseThrough
        {
         //   get { return (Parent != null ? Parent.MouseThrough : _MouseThrough); }
            get { return _MouseThrough; }
            set { _MouseThrough = value; }
        }
        public bool IsMouseThrough
        {
            //get { return this.Parent == null ? MouseThrough : Parent.IsMouseThrough; }
            get { return MouseThrough; }
        }
        LayerTypes _Layer = LayerTypes.Main;
        public LayerTypes Layer { get { return (this.Parent != null ? Parent.Layer : _Layer); } set { _Layer = value; } }
        public DrawMode DrawMode = DrawMode.Normal;
        public event EventHandler<DrawItemEventArgs> DrawItem;
        public void OnDrawItem(DrawItemEventArgs e)
        {
            if (DrawItem != null)
                DrawItem(this, e);
        }

        public event EventHandler<TooltipArgs> DrawTooltip;
        protected virtual void OnDrawTooltip(TooltipArgs e)//TooltipArgs e)
        {
            if (DrawTooltip != null)
                DrawTooltip(this, e);
        }
        Object _Tag;
       // public virtual Control SetMouseThrough(bool toggle) { this.MouseThrough = toggle; return this; }
       // public virtual Control SetTag(object tag) { this.Tag = tag; return this; }
        public virtual Object Tag { get { return _Tag; } set { _Tag = value; OnTagChanged(); } }
        public event EventHandler<EventArgs> GotFocus, MouseEnter, MouseLeave, LostFocus, TooltipChanged, TagChanged;
        public event EventHandler<KeyPressEventArgs2> KeyPress, KeyRelease;
        UIManager _WindowManager;
        public UIManager WindowManager { get { return _WindowManager ?? ScreenManager.CurrentScreen.WindowManager; } set { _WindowManager = value; } }
       // public UIManager WindowManager
        public bool AllowDrop, ClipToBounds = true;

        
        //public virtual string Name { get { return _Name; } set { _Name = value; } }
        //Func<string> _NameFunc;
        //public Func<string?> NameFunc = new Func<string> (() =>
        //{
        //    if (_NameFunc.IsNull())
        //        return null;
        //    else
        //        return _NameFunc();
        //});

        //public Action<Control> OnUpdate = (ctrl) => { };
        public Action OnUpdate = () => { };
        string _Name;
        public string NameFormat;
        public virtual Func<string> NameFunc { get; set; }
        public virtual string Name { get { return NameFunc.IsNull() ? _Name : NameFunc(); } set { _Name = value; } }
        public UIManager ui;
        
        public int ID;
        public SpriteEffects SprFx;
        public float t = 0, dt = -0.05f;
        public Color Alpha, Blend;
        Color _BackgroundColor = Color.Transparent;
        public Func<Color> BackgroundColorFunc;
        public virtual Color BackgroundColor
        {
            get
            {
                if (BackgroundColorFunc.IsNull())
                    return _BackgroundColor;
                else
                    return BackgroundColorFunc();
            }
            set { _BackgroundColor = value; this.Invalidate(); }
        }
        Vector2 _Anchor = Vector2.Zero;
        public Vector2 Anchor// = Vector2.Zero;
        {
            get { return _Anchor; }
            set
            {
                _Anchor = value;
                Location = Location -Dimensions * value;
            }
        }
        float _Opacity = 1;
        //public Func<float> OpacityFunc;
        public virtual float Opacity
        {
            get
            {
                //if (this.OpacityFunc != null)
                //    return this.OpacityFunc();
                return this._Opacity;
            }
            set { _Opacity = value; }
        }//Invalidate(); } }
        public virtual RenderTarget2D Texture { get; set; }
       // protected bool TopMost;
        public bool Focused { get { return Controller.Instance.Mouseover.Object == this; } } //return WindowManager.ActiveControl == this; } }
        ControlCollection _Controls;
        public ControlCollection Controls
        {
            get
            {
                if (_Controls == null)
                    _Controls = new ControlCollection(this);
                return _Controls;
            }
        }
        public virtual void AlignTopToBottom()
        {
            this.Controls.AlignTopToBottom();
        }
        public Control SetControls(params Control[] controls)
        {
            foreach (Control ctrl in controls)
                Controls.Add(ctrl);
            return this;
        }

        protected bool Valid = false;
        public virtual Control Invalidate(bool invalidateChildren = false)
        {
            //this.Texture = null;
            this.Valid = false;
            if (invalidateChildren)
                foreach (Control ctrl in Controls)
                    ctrl.Invalidate(invalidateChildren);
            return this;
        }

        public virtual void Reposition(UIScaleEventArgs e = null)
        {
            float ratio = e.NewScale / e.OldScale;
            Location = Location / ratio - (new Vector2(Size.Width - Size.Width / ratio, Size.Height - Size.Height / ratio) * Anchor);
            // Location = (Location + (new Vector2(Size.Width, Size.Height) * Anchor)) / ratio;// ;
        }

        public virtual void Reposition(float ratio = 1)
        {
            Location = Location / ratio - (new Vector2(Size.Width - Size.Width / ratio, Size.Height - Size.Height / ratio) * Anchor);
        }

        public virtual void Reposition(Vector2 ratio)
        {
            Location = Location * ratio - (new Vector2(this.Size.Width - this.Size.Width * ratio.X, this.Size.Height - this.Size.Height * ratio.Y) * Anchor);
            //foreach (var control in Controls)
            //    control.Reposition(ratio);
        }


        public void AlignVertically(HorizontalAlignment alignment = HorizontalAlignment.Left)
        {
            //Vector2 last = Vector2.Zero;
            //foreach (var current in this.Controls)
            //{
            //    current.Location = last;
            //    last = current.BottomLeft;
            //}

            switch (alignment)
            {
                case HorizontalAlignment.Left:
                    Vector2 last = Vector2.Zero;
                    foreach (var current in this.Controls)
                    {
                        current.Location = last;
                        last = current.BottomLeft;
                    }
                    break;

                case HorizontalAlignment.Center:
                    //last = current.BottomCenter - new Vector2(current.Width / 2, 0);
                    int maxw = 0, maxx = 0;
                    foreach (var current in this.Controls)
                    {
                        maxw = Math.Max(maxw, current.Width);
                        maxx = Math.Max(maxx, (int)current.Location.X);
                    }
                    last = new Vector2(maxw / 2, 0);
                    foreach (var current in this.Controls)
                    {
                        current.Location = last - new Vector2(current.Width / 2, 0);
                        last = current.BottomCenter;
                    }
                    break;

                case HorizontalAlignment.Right:
                    //last = current.BottomRight - new Vector2(current.Width, 0);
                    maxw = 0;
                    maxx = 0;
                    foreach (var current in this.Controls)
                    {
                        maxw = Math.Max(maxw, current.Width);
                        maxx = Math.Max(maxx, (int)current.Location.X);
                    }
                    last = new Vector2(maxw, 0);
                    foreach (var current in this.Controls)
                    {
                        current.Location = last - new Vector2(current.Width, 0);
                        last = current.BottomRight;
                    }
                    break;

                default:
                    break;
            }

            ReAddChildren();
        }
        public void AlignHorizontally()
        {
            Vector2 last = Vector2.Zero;
            foreach (var current in this.Controls)
            {
                current.Location = last;
                last = current.TopRight;
            }
            ReAddChildren();
        }

        private void ReAddChildren()
        {
            var temp = this.Controls.ToList();
            this.Controls.Clear();
            foreach (var c in temp)
                this.Controls.Add(c);
        }
        

        public bool HasChildren
        {
            get
            {
                if (Controls != null)
                    return Controls.Count > 0;
                return false;
            }
        }

        //Vector2 _ClientLocation = Vector2.Zero;
        public Vector2 ClientLocation = Vector2.Zero;
        //{
        //    get { return _ClientLocation; }
        //    set
        //    {
        //        _ClientLocation = value;

        //    }
        //}
        Color _Tint = Color.White;
        public Func<Color> TintFunc;
        public virtual Color Tint
        {
            get
            {
                if (TintFunc.IsNull())
                    return _Tint;
                else
                    return TintFunc();
            }
            //set { _Color = value; this.Invalidate(); }    
            set { _Tint = value; this.Invalidate(); }
        }

        Color _Color = Color.White;
        public Func<Color> ColorFunc;
        public virtual Color Color
        {
            get
            {
                if (ColorFunc.IsNull())
                    return _Color;
                else
                    return ColorFunc();
            }
            set { _Color = value; this.Invalidate(); }
        }
        public virtual void Validate()//bool cascade = false)
        {
            PreparingPaint();

            if (this.Width <= 0 || 
                this.Height <= 0)
                return;

            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            //if (Texture.IsNull())
                Texture = CreateTexture(gd);
            SpriteBatch sb = new SpriteBatch(gd);
            gd.SetRenderTarget(Texture);
            gd.Clear(this.BackgroundColor);

            sb.Begin();//SpriteSortMode.FrontToBack, BlendState.AlphaBlend);
            //sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);//SpriteSortMode.FrontToBack, BlendState.AlphaBlend);

            OnPaint(sb);
            sb.End();

            sb.Begin();//SpriteSortMode.FrontToBack, BlendState.AlphaBlend);
            //sb.Begin(SpriteSortMode.Deferred, null, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);//SpriteSortMode.FrontToBack, BlendState.AlphaBlend);
            OnAfterPaint(sb);//.GraphicsDevice);
            sb.End();

            Valid = true;
        }
        public virtual void PreparingPaint() { }
        public virtual void OnPaint(SpriteBatch sb) { }
        public virtual void OnAfterPaint(SpriteBatch sb) { }
        public virtual RenderTarget2D CreateTexture(GraphicsDevice gd) { return new RenderTarget2D(Game1.Instance.GraphicsDevice, this.Width, this.Height); }
        Texture2D _BackgroundTexture;
        public virtual Texture2D BackgroundTexture
        {
            get { return _BackgroundTexture; }
            set
            {
                this._BackgroundTexture = value;
                Width = _BackgroundTexture.Width;
                Height = _BackgroundTexture.Height;
            }
        }
        protected BackgroundStyle _BackgroundStyle;
        public virtual BackgroundStyle BackgroundStyle
        {
            get { return _BackgroundStyle; }
            set
            {
                _BackgroundStyle = value;
                ClientLocation = new Vector2(_BackgroundStyle.Border);
                Width = ClientSize.Width + 2 * _BackgroundStyle.Border;
                Height = ClientSize.Height + _BackgroundStyle.Border * 2;
            }
        }

        //}

        internal bool isPressed = false, Active = true;
        public event UIEvent LeftClick, RightClick,
            MouseUp, MouseWheelDown, MouseWheelUp,
            TopMostControlChanged;
        public event EventHandler<System.Windows.Forms.HandledMouseEventArgs> MouseDown, MouseLeftUp, MouseLeftDown, MouseRightPress, MouseRightUp, MouseScroll;
        public event EventHandler<System.Windows.Forms.HandledMouseEventArgs> MouseWheel, MouseMove, MouseLeftPress;

        protected virtual void OnMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (MouseWheel != null)
                MouseWheel(this, e);
        }
        protected virtual void OnMouseScroll(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (MouseScroll != null)
                MouseScroll(this, e);
        }
        public Control TopLevelControl
        {
            get
            {
                Control parent = this;
                while (parent.Parent != null)
                    parent = parent.Parent;
                return parent;
            }
        }

        public Window GetWindow()
        {
            Control window = this;
            while (window != null)
            {
                if (window is Window)
                    return window as Window;
                window = window.Parent;
            }
            return null;
        }
        public Control Root
        {
            get
            {
                Control parent = this;
                while (parent.Parent != null)
                    parent = parent.Parent;
                return parent;
            }
        }

        public virtual void Initialize()
        {

        }

        public Control(Vector2 location)
            : this()
        {
            //Name = "<undefined>";
            Location = location;

           // Initialize();
        }
        public Control(Vector2 location, Vector2 size)
            : this()
        {
            Width = (int)size.X;
            Height = (int)size.Y;

            //Name = "<undefined>";
            Location = location;

          //  Initialize();
        }
        public Control()
        {
           // Name = "<undefined>";
            Location = new Vector2(0);
            //this.AutoSize = true;
            //Initialize();
        }

        #region Raise Events
        protected virtual void OnTagChanged()
        {
            if (TagChanged != null)
                TagChanged(this, EventArgs.Empty);
        }
        protected virtual void OnTooltipChanged()
        {
            if (TooltipChanged != null)
                TooltipChanged(this, new EventArgs());
        }
        protected virtual void OnMouseWheelUp()
        {
            if (MouseWheelUp != null)
                MouseWheelUp(this, new EventArgs());
        }
        protected virtual void OnMouseWheelDown()
        {
            if (MouseWheelDown != null)
                MouseWheelDown(this, new EventArgs());
        }
        protected virtual void OnMouseMove()
        {

        }
        protected virtual void OnMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (MouseMove != null)
                MouseMove(this, e);
        }
        protected virtual void OnMouseDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (MouseDown != null)
                MouseDown(this, e);
        }
        protected virtual void OnMouseUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (MouseUp != null)
                MouseUp(this, e);
        }
        protected virtual void OnTopMostControlChanged()
        {
            if (TopMostControlChanged != null)
                TopMostControlChanged(this, new UIEventArgs(this));
        }
        protected virtual void OnLeftClick()
        {
            if (LeftClick != null)
                LeftClick(this, new UIEventArgs(this));
        }
        protected virtual void OnRightClick()
        {
            if (RightClick != null)
                RightClick(this, new UIEventArgs(this));
        }
        protected virtual void OnKeyPress(KeyPressEventArgs2 e)
        {
            if (KeyPress != null)
                KeyPress(this, e);
        }
        protected virtual void OnKeyRelease(KeyPressEventArgs2 e)
        {
            if (KeyRelease != null)
                KeyRelease(this, e);
        }
        #endregion



        public bool IsTopMost
        {
            get { return WindowManager.ActiveControl == this; }
        }

        protected virtual void OnGotFocus()
        {
            if (GotFocus != null)
                GotFocus(this, EventArgs.Empty);

        }
        public virtual void OnLostFocus()
        {


            if (LostFocus != null)
                LostFocus(this, EventArgs.Empty);

        }
        Action MouseEnterAction = () => { }, MouseLeaveAction = () => { };
        public virtual void OnMouseEnter()
        {
            MouseEnterAction();
            MouseHover = true;
            if (MouseEnter != null)
                MouseEnter(this, EventArgs.Empty);
        }
        public virtual void OnMouseLeave()
        {
            MouseLeaveAction();
            MouseHover = false;
            if (MouseLeave != null)
                MouseLeave(this, EventArgs.Empty);
        }
        protected virtual void OnMouseLeftPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //this.WindowManager.BringToFront(GetWindow());
            Root.BringToFront();
            if (MouseLeftPress != null)
                MouseLeftPress(this, e);
        }
        protected virtual void OnMouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (MouseLeftUp != null)
                MouseLeftUp(this, e);
        }
        protected virtual void OnMouseRightPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (MouseRightPress != null)
                MouseRightPress(this, e);
        }
        protected virtual void OnMouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (MouseRightUp != null)
                MouseRightUp(this, e);
        }
        protected virtual void OnMouseLeftDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (MouseLeftDown != null)
                MouseLeftDown(this, e);
        }

        //public virtual void DoDragDrop(Object data, DragDropEffects effects) { }



        public virtual void Select()
        {
            if (WindowManager.FocusedControl != null)
                if (WindowManager.FocusedControl != this)
                    WindowManager.FocusedControl.Unselect();
            WindowManager.FocusedControl = this;
            OnGotFocus();
        }

        public virtual void Unselect()
        {
            OnLostFocus();
        }



        public virtual void Dispose()
        {

            if (HasChildren)
                foreach (Control control in Controls)
                    control.Dispose();
        }
        public int Top
        {
            get { return (int)Location.Y; }
            set { Location.Y = value; }
        }
        public int Left
        { get { return (int)Location.X; } }

        public virtual int Bottom
        { get { return (int)Location.Y + Height; } }
        public int Right
        { get { return (int)Location.X + Width; } }
        public Vector2 Center
        { get { return new Vector2(Width / 2, Height / 2); } }
        public Vector2 BottomCenter
        { get { return new Vector2(Left + Width / 2, Bottom); } }
        public Vector2 BottomLeft
        {
            get { return new Vector2(Left, Bottom); }
           // set { Location.X = value.X; Location.Y = value.Y - Height; }
        }
        public Vector2 BottomRight
        { get { return new Vector2(Right, Bottom); } }
        public Vector2 TopRight
        { get { return new Vector2(Right, Top); } }
        public Vector2 CenterRight
        { get { return new Vector2(Right, Top + Height / 2); } }

        public Rectangle ClientRectangle
        {
            get { return new Rectangle((int)ClientLocation.X, (int)ClientLocation.Y, ClientSize.Width, ClientSize.Height); }
            set
            {
                ClientLocation.X = value.X; ClientLocation.Y = value.Y;
              //  ClientSize = new Rectangle((int)ClientLocation.X, (int)ClientLocation.Y, value.Width, value.Height);
                ClientSize = new Rectangle(0, 0, value.Width, value.Height);
            }
        }

        static public bool HitTest(Rectangle rect, Rectangle? viewport = null)
        {
            Rectangle final = Rectangle.Intersect(rect, viewport ?? rect);
            return (final.Intersects(new Rectangle((int)(Controller.Instance.msCurrent.X / UIManager.Scale), (int)(Controller.Instance.msCurrent.Y / UIManager.Scale), 1, 1)));
        }
        public virtual bool HitTest(Rectangle viewport)
        {
            return ((!this.IsMouseThrough) && Rectangle.Intersect(viewport, this.ScreenBounds).Intersects(new Rectangle((int)(Controller.Instance.msCurrent.X / UIManager.Scale), (int)(Controller.Instance.msCurrent.Y / UIManager.Scale), 1, 1)));
        }
        public virtual bool HitTest()
        {
            //return ((!this.MouseThrough) && Bounds.Intersects(new Rectangle((int)(Controller.Instance.msCurrent.X / UIManager.Scale), (int)(Controller.Instance.msCurrent.Y / UIManager.Scale), 1, 1)));
            return ((!this.IsMouseThrough) && ScreenBounds.Intersects(new Rectangle((int)(Controller.Instance.msCurrent.X / UIManager.Scale), (int)(Controller.Instance.msCurrent.Y / UIManager.Scale), 1, 1)));
        }
        public virtual bool HitTest(System.Windows.Forms.HandledMouseEventArgs e)//MouseState ms)
        {
            //return ((!MouseThrough) && Bounds.Intersects(new Rectangle((int)(ms.X / UIManager.Scale),(int)( ms.Y / UIManager.Scale), 1, 1)));
            return ((!this.IsMouseThrough) && ScreenBounds.Intersects(new Rectangle((int)(e.X / UIManager.Scale), (int)(e.Y / UIManager.Scale), 1, 1)));
        }
        public virtual bool HitTest(System.Windows.Forms.HandledMouseEventArgs e, Rectangle viewport)
        {
            //return ((!MouseThrough) && Bounds.Intersects(new Rectangle((int)(ms.X / UIManager.Scale),(int)( ms.Y / UIManager.Scale), 1, 1)));
            return ((!this.IsMouseThrough) && Rectangle.Intersect(viewport, this.ScreenBounds).Intersects(new Rectangle((int)(e.X / UIManager.Scale), (int)(e.Y / UIManager.Scale), 1, 1)));
        }
        public virtual bool HitTest(Vector2 ms)
        {
           // Vector2 ms = new Vector2(ms2.X, ms2.Y);
            //return ((!MouseThrough) && Bounds.Intersects(new Rectangle((int)(ms.X / UIManager.Scale),(int)( ms.Y / UIManager.Scale), 1, 1)));
            return ((!this.IsMouseThrough) && ScreenBounds.Intersects(new Rectangle((int)(ms.X / UIManager.Scale), (int)(ms.Y / UIManager.Scale), 1, 1)));
        }
        //public virtual bool HitTest(Rectangle box)
        //{
        //    return (box.Intersects(new Rectangle(Controller.Instance.msCurrent.X, Controller.Instance.msCurrent.Y, 1, 1)));
        //}
        public Vector2 ScreenClientLocation
        {
            get { return new Vector2(X + ClientLocation.X, Y + ClientLocation.Y); }
        }
        public Rectangle ScreenClientRectangle
        {
            get { return new Rectangle(X + (int)ClientLocation.X, Y + (int)ClientLocation.Y, ClientSize.Width, ClientSize.Height); }
        }
        public virtual Rectangle ScreenBounds
        {
            get { return new Rectangle((int)ScreenLocation.X, (int)ScreenLocation.Y, Width, Height); }
        }
        public virtual Rectangle LocalBounds
        {
            get { return new Rectangle((int)Location.X, (int)Location.Y, Width, Height); }
        }

        public virtual int Width { get; set; }
        public virtual int Height { get; set; }

        protected Rectangle _ClientSize;
        public virtual Rectangle ClientSize
        {
            get
            {
                return _ClientSize;
               // return new Rectangle((int)ClientLocation.X, (int)ClientLocation.Y, 
            }
            set
            {
                _ClientSize = value;
                //ClientLocation.X = value.X;
                //ClientLocation.Y = value.Y;

                //if (AutoSize)
                //{
                    Width = ClientSize.Width + (_BackgroundStyle != null ? 2 * _BackgroundStyle.Border : 0);
                    Height = ClientSize.Height + (_BackgroundStyle != null ? 2 * _BackgroundStyle.Border : 0);
                //}
                OnClientSizeChanged();
                OnSizeChanged();
            }
        }
        protected virtual void OnClientSizeChanged() { }

        public void SetClipToBounds(bool value, bool children = false, params Control[] exclude)
        {
            this.ClipToBounds = value;
            if (children)
                foreach (Control control in Controls.Except(exclude))
                    control.SetClipToBounds(value, children, exclude);
        }
        public virtual Control SetLocation(Vector2 value)
        {
            this.Location = value;
            return this;
        }
        public virtual void SetOpacity(float value, bool children = false, params Control[] exclude)
        {
            //this.Opacity = value;
            //if (children)
            //foreach (Control control in Controls.Except(exclude))
            //    control.SetOpacity(value, children, exclude);

            if (!exclude.Contains(this))
                this.Opacity = value;
            if (children)
                foreach (Control control in Controls)
                    control.SetOpacity(value, children, exclude);
        }
        public Control SetMousethrough(bool value, bool children = false)
        {
            this.MouseThrough = value;
            if (children)
                foreach (Control control in Controls)
                    control.SetMousethrough(value, children);
            return this;
        }
        public virtual Vector2 Dimensions
        {
            get
            { return new Vector2(Width, Height); }
            set
            {
                Width = (int)value.X;
                Height = (int)value.Y;
                OnSizeChanged();
            }
        }
        public virtual Vector2 ClientDimensions
        {
            get
            { return new Vector2(ClientSize.Width, ClientSize.Height); }
            set
            {
                ClientSize = new Rectangle(0, 0, (int)value.X, (int)value.Y);
          //      OnSizeChanged();
            }
        }
        public virtual Rectangle Size
        {
            get
            { return new Rectangle(0, 0, Width, Height); }
            set
            {
                Width = value.Width;
                Height = value.Height;
                var border = this.BackgroundStyle != null ? this.BackgroundStyle.Border : UIManager.BorderPx;
                this._ClientSize.Width = Width - 2 * border;
                this._ClientSize.Height = Height - 2 * border;
                //if (this.BackgroundStyle == null)
                //{
                    //this._ClientSize.Width = Width - 2 * UIManager.BorderPx;
                    //this._ClientSize.Height = Height - 2 * UIManager.BorderPx;
                //}
                //else
                //{
                //    this._ClientSize.Width = Width - 2 * this.BackgroundStyle.Border;
                //    this._ClientSize.Height = Height - 2 * this.BackgroundStyle.Border;
                //}
                OnSizeChanged();
            }
        }
        public virtual void Conform(params Control[] controls)
        {
            var rects = new Queue<Control>(controls);
            //if (rects.Count == 0)
            //    return;
            var union = new Rectangle(0,0,0,0);
            while (rects.Count > 0)
                union = Rectangle.Union(union, rects.Dequeue().LocalBounds);
            this.ClientSize = union;
            return;


            Rectangle biggest = new Rectangle(0, 0, 0, 0);
            (new List<Control>(controls)).ForEach(
                t =>
                {
                    biggest.Width = Math.Max(biggest.Width, t.Size.Width);
                    biggest.Height = Math.Max(biggest.Height, t.Size.Height);
                });
            this.ClientSize = biggest;
        }
        public event EventHandler<EventArgs> SizeChanged;
        protected virtual void OnSizeChanged()
        {
            // i commented this out because i couldn't place controls above and left of the parent
            //Location.X = MathHelper.Clamp(Location.X, 0, Game1.Instance.graphics.PreferredBackBufferWidth - Width);
            //Location.Y = MathHelper.Clamp(Location.Y, 0, Game1.Instance.graphics.PreferredBackBufferHeight - Height);

            if (SizeChanged != null)
                SizeChanged(this, EventArgs.Empty);
        }
        
        public virtual bool AutoSize { get; set; }
        public event EventHandler<EventArgs> ControlAdded, ControlRemoved;

        internal virtual void OnControlAdded(Control control)
        {
            if (this.AutoSize)
                this.ClientSize = PreferredClientSize;
                //Size = PreferredSize;

            ResizeToClientSize();

            if (ControlAdded != null)
                ControlAdded(this, new ControlEventArgs(control));
            if(!Parent.IsNull())
                Parent.OnControlAdded(control);
        }

        public void ResizeToClientSize()
        {
            if (this.BackgroundStyle != null)
            {
                ClientLocation = new Vector2(this.BackgroundStyle.Border);
                this.Width = this.ClientSize.Width + 2 * this.BackgroundStyle.Border;
                this.Height = this.ClientSize.Height + this.BackgroundStyle.Border * 2;
            }
        }
        internal virtual void OnControlRemoved(Control control)
        {
            if (AutoSize)
                ClientSize = PreferredClientSize;

            ResizeToClientSize();

                //Size = PreferredSize;
            if (ControlRemoved != null)
                ControlRemoved(this, new ControlEventArgs(control));
            if (!Parent.IsNull()) Parent.OnControlRemoved(control);
        }



        public Vector2 Origin = new Vector2(0);
        public virtual void DrawHighlight(SpriteBatch sb, float alpha = 0.5f)
        {
            sb.Draw(UIManager.Highlight, new Rectangle((int)(ScreenLocation.X - Origin.X), (int)(ScreenLocation.Y - Origin.Y), ScreenBounds.Width, ScreenBounds.Height), null, Color.Lerp(Color.Transparent, Color.White, alpha), 0, Vector2.Zero, SpriteEffects.None, Depth);
        }
        public virtual void DrawHighlight(SpriteBatch sb, Rectangle destinationRect, float alpha = 0.5f)
        {
            sb.Draw(UIManager.Highlight, destinationRect, null, Color.Lerp(Color.Transparent, Color.White, alpha), 0, Vector2.Zero, SpriteEffects.None, Depth);
        }
        public virtual void DrawHighlight(SpriteBatch sb, Rectangle destinationRect, Color color, float depth = 0)
        {
            sb.Draw(UIManager.Highlight, destinationRect, null, color, 0, Vector2.Zero, SpriteEffects.None, depth);
        }
        public virtual void DrawShade(SpriteBatch sb, float alpha = 0.5f)
        {
            sb.Draw(UIManager.Shade, new Rectangle((int)ScreenLocation.X, (int)ScreenLocation.Y, Width, Height), null, Color.Lerp(Color.Transparent, Color.White, alpha), 0, Origin, SpriteEffects.None, 0);//Depth);
        }

        public virtual Rectangle PreferredClientSize
        {
            get
            {
                int width = 0, height = 0;
                foreach (Control control in Controls)
                {
                    //width = Math.Max(width, Math.Abs( (int)control.Location.X + control.Width - (int)control.Origin.X));
                    //height = Math.Max(height, Math.Abs((int)control.Location.Y + control.Height - (int)control.Origin.Y));
                    width = Math.Max(width, (int)control.Location.X + control.Width - (int)control.Origin.X);
                    height = Math.Max(height, (int)control.Location.Y + control.Height - (int)control.Origin.Y);


                }
                return new Rectangle(0, 0, width, height);
            }
        }

        public virtual void OnBeforeDraw(SpriteBatch sb, Rectangle viewport) { }
        public virtual void OnAfterDraw(SpriteBatch sb, Rectangle viewport) { }
        public virtual void OnHitTestPass() {
            Controller.Instance.MouseoverNext.Object = this;
        }
        public virtual void Draw(SpriteBatch sb, Rectangle viewport)
        {
            // TODO: maybe put that somewhere else?
            //if (HitTest(viewport))
            //    Controller.Instance.MouseoverNext.Object = this;
            OnDrawItem(new DrawItemEventArgs(sb, ScreenBounds));
            OnBeforeDraw(sb, viewport); 
            //this.Bounds.DrawHighlight(sb);
            Color c = Tint;// Color.White;//Color;
            if (!Texture.IsNull())
            {
                Rectangle final, source;
               // this.Bounds.DrawHighlight(sb, new Vector2(Origin.X / Width, Origin.Y / Height), Rotation, 0.5f);

                // TODO: this is no use because if i add something outside the window's client area and the window has autosize, it expands the client area screwing up hittesting
                if (!ClipToBounds)
                {
                    //          this.Bounds.DrawHighlight(sb);
                    //sb.Draw(this.Texture, this.Bounds, null, c * Opacity);
                    sb.Draw(this.Texture, this.ScreenBounds, null, c * Opacity, Rotation, this.Origin, SpriteEffects.None, 0);
                }
                else
                {
                    this.ScreenBounds.Clip(Texture.Bounds, viewport, out final, out source);
                    sb.Draw(this.Texture, final, source, c * Opacity, Rotation, this.Origin, SpriteEffects.None, 0);
                }

            }
            OnAfterDraw(sb, viewport);
            if (this.HitTest(viewport))
                OnHitTestPass();
            

            foreach (Control control in this.Controls.ToList())
                control.Draw(sb, Rectangle.Intersect(control.ScreenBounds, viewport));
        }

        public virtual void Draw(SpriteBatch sb)//, ref IMouseoverable mouseover)
        {
            // TODO: maybe put that somewhere else?
            if (HitTest())
                Controller.Instance.MouseoverNext.Object = this;
                //WindowManager.ActiveControl = this;

            //if (MouseHover)
            //    Controller.Instance.MouseoverNext.Object = this;
            
            foreach (Control control in Controls)
                control.Draw(sb);
            //else
            OnDrawItem(new DrawItemEventArgs(sb, ScreenBounds));
        }

        public override string ToString()
        {
            return this.Name ?? base.ToString();
        }

       

        public GroupBox TooltipHoverText
        {
            get
            {
                GroupBox tooltip = new GroupBox();
                Label label = new Label(HoverText);
                tooltip.Controls.Add(label);
                return tooltip;
            }
        }
        string _HoverText = "";
        public Func<string> HoverFunc;
        public string HoverFormat;
        public virtual string HoverText
        {
            get
            {
                //return this.HoverFunc.IsNull() ? _HoverText : HoverFunc();
                if (this.HoverFunc.IsNull())
                    return _HoverText;
                else
                    return HoverFunc();
            }
            set { _HoverText = value; }
        }

        public event EventHandler<TooltipEventArgs> TooltipDraw;
        void OnTooltipDraw(TooltipEventArgs e)
        {
            if (TooltipDraw != null)
                TooltipDraw(this, e);
        }

        public Action<Tooltip> TooltipFunc;
        public bool CustomTooltip;
        public virtual void GetTooltipInfo(Tooltip tooltip)//List<GroupBox> TooltipGroups
        {
            if (!TooltipFunc.IsNull())
            {
                TooltipFunc(tooltip);
                return;
            }
            if (this.HoverText.Length > 0)
                //tooltip.Controls.Add(new Label(HoverText, HoverFormat));// { TextFunc = this.HoverFunc });//new Label(HoverText));
                tooltip.Controls.Add(new Label(HoverText, HoverFormat){ TextFunc = this.HoverFunc });//new Label(HoverText));

            // get
            // {
            if (CustomTooltip)
            {
                List<GroupBox> tt = new List<GroupBox>();
                OnDrawTooltip(new TooltipArgs(tooltip));//new TooltipArgs(this, tt));
                //return tt;
            }
            // return null;

            // }
        }
        public override void Update()
        {
            this.OnUpdate();
          //  MouseHover = false;
            if (!Valid)
            //{
                this.Validate();
            //    Valid = true;
            //}

            base.Update();
            //ControlCollection copy = Controls.Copy();
            List<Control> copy = this.Controls.ToList();
            foreach (Control control in copy)
                control.Update();
        }

        public virtual Control AddControls(params Control[] controls)
        {
            foreach (var ctrl in controls)
                this.Controls.Add(ctrl);
            return this;
        }
        public virtual void AddControlsBottomLeft(params Control[] controls)
        {
            foreach (var c in controls)
            {
                c.Location = this.Controls.BottomLeft;
                this.Controls.Add(c);
            }
        }
        //public virtual void HandleInput(InputState input)
        //{
        //  //  if (input.Handled)
        //  //      return;
        //    if (MouseThrough)
        //        return;

        //    if (this.HasChildren)
        //    {
        //        List<Control> controls = Controls.ToList();
        //        controls.Reverse();
        //        foreach (Control c in controls)
        //            c.HandleInput(input);
        //    }

        //    //if (input.Handled)
        //    //    return;

        //    //foreach (Control c in Controls.ToList())
        //    //    c.HandleInput(input);

        //    //if (input.Handled)
        //    //    return;
        //    MouseHover = false;
        ////    MouseHover = true;
            

        //        if(HitTest(input.CurrentMouseState))
        //    {
        ////        input.Handled = true;

        //        if (WindowManager.ActiveControl != null)
        //            return;

                
        //            WindowManager.ActiveControl = this;
        //            MouseHover = true;
                
        //        //else 
        //        //if (WindowManager.ActiveControl != this)
        //        //{
        //        //    if (WindowManager.ActiveControl != null)
        //        //    {
        //        //        WindowManager.ActiveControl.Unselect();
        //        //        WindowManager.ActiveControl.OnMouseLeave();
        //        //    }
        //        //    OnGotFocus();
        //        //    WindowManager.ActiveControl = this;
        //        //}

        //        if (!HitTest(input.LastMouseState))
        //        {
        //            if (WindowManager.ActiveControl == this)
        //            {
        //                OnMouseEnter();
        //                MouseHover = true;
        //            }
        //        }
        //        else if (!(input.CurrentMouseState.X / UIManager.Scale == input.LastMouseState.X / UIManager.Scale && input.CurrentMouseState.Y / UIManager.Scale == input.LastMouseState.Y / UIManager.Scale))
        //        {
        //        //    input.Handled = true;
        //            OnMouseMove(input);
        //        }

        //    //    if (input.Handled)
        //     //       return;

        //        Keys[] keys = input.CurrentKeyboardState.GetPressedKeys();
        //        if (keys.Length > 0)
        //            foreach (Keys key in keys)
        //                if (input.IsKeyPressed(key))
        //                    OnKeyPress(new KeyPressEventArgs2(key, input));

        //        keys = input.LastKeyboardState.GetPressedKeys();
        //        if (keys.Length > 0)
        //            foreach (Keys key in keys)
        //                if (input.IsKeyReleased(key))
        //                    OnKeyRelease(new KeyPressEventArgs2(key, input));

        //        if (input.CurrentMouseState.LeftButton == ButtonState.Pressed)
        //            if (input.LastMouseState.LeftButton == ButtonState.Released)
        //            {
        //        //        input.Handled = true;
        //                Select();
        //                OnMouseLeftPress(input);
        //            }

        //        if (input.LastMouseState.LeftButton == ButtonState.Pressed)
        //            if (input.CurrentMouseState.LeftButton == ButtonState.Released)
        //            {

        //              //  input.Handled = true;
        //                OnMouseLeftUp(input);
        //            }

        //        if (input.LastMouseState.RightButton == ButtonState.Pressed)
        //            if (input.CurrentMouseState.RightButton == ButtonState.Released)
        //            {
        //             //   input.Handled = true;
        //                OnMouseRightUp(input);
        //            }

        //        if (input.CurrentMouseState.RightButton == ButtonState.Pressed)
        //            if (input.LastMouseState.RightButton == ButtonState.Released)
        //            {
        //        //        input.Handled = true;

        //                OnMouseRightPress(input);
        //            }

        //        if (input.CurrentMouseState.ScrollWheelValue != input.LastMouseState.ScrollWheelValue)
        //            {
        //                OnMouseScroll(input);
        //            }
        //    }
        //    else if (HitTest(input.LastMouseState))
        //    {
        //        OnMouseLeave();
        //        MouseHover = false;
        //    }
        //}

        public virtual void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            foreach (Control control in Controls.ToList())
                control.HandleKeyUp(e);
        }
        public virtual void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            foreach (Control control in Controls.ToList())
                control.HandleKeyDown(e);
        }
        public virtual void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            foreach (Control control in Controls.ToList())
                control.HandleKeyPress(e);
        }
        public virtual void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e, Rectangle viewport)
        {
            if (this.HasChildren)
            {
                List<Control> controls = Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                    c.HandleMouseMove(e, viewport);
            }
            

            //// Control active = Controller.Instance.MouseoverNext.Object as Control; //WindowManager.ActiveControl

            //if (HitTest(e, viewport))
            //{

            //    if (Controller.Instance.MouseoverNext.Object != this)
            //    {
            //        //     Console.WriteLine(Controller.Instance.MouseoverNext.Object);
            //        return;
            //    }
            //    //   Controller.Instance.MouseoverNext.Object = this;

            //    if (!MouseHover)
            //    {
            //        //if (Controller.Instance.MouseoverNext.Object == this)
            //        //{
            //        OnMouseEnter();
            //        MouseHover = true;
            //        //}
            //    }
            //    else
            //        OnMouseMove(e);
            //}
            //else
            //{
            //    if (MouseHover)
            //        OnMouseLeave();
            //    MouseHover = false;
            //}

        }
        public virtual void HandleMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.HasChildren)
            {
                List<Control> controls = Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                    c.HandleMouseMove(e);
            }

            //if (HitTest(e))
            //{
            //    if (Controller.Instance.MouseoverNext.Object != this)
            //        return;

            //    if (!MouseHover)
            //    {
            //        OnMouseEnter();
            //        MouseHover = true;
            //    }
            //    else
            //        OnMouseMove(e);
            //}
            //else
            //{
            //    if (MouseHover)
            //        OnMouseLeave();
            //    MouseHover = false;
            //}

        }
        public virtual void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (WindowManager.ActiveControl == this)
            {
                this.Select();
                OnMouseLeftPress(e);
                return;
            }
            foreach (var c in this.Controls.ToList())
                c.HandleLButtonDown(e);

            //this.Select();
            //OnMouseLeftPress(e);
        }
        public virtual void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {

            if (this.HasChildren)
            {
                List<Control> controls = Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                    c.HandleLButtonUp(e);
            }
            //if (WindowManager.ActiveControl != this)
            //    return;

            //if (!MouseHover)
            //    return;

          //  Select();
            OnMouseLeftUp(e);
        }
        public virtual void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            OnMouseRightPress(e);
        }
        public virtual void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.HasChildren)
            {
                List<Control> controls = Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                    c.HandleRButtonUp(e);
            }
            OnMouseRightUp(e);
        }
        public virtual void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.HasChildren)
            {
                List<Control> controls = Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                    c.HandleMouseWheel(e);
            }
            OnMouseWheel(e);
        }

        public virtual bool Toggle()
        {
            //if (WindowManager.Controls.Contains(this))
            //{
            //    WindowManager.Controls.Remove(this);
            //    return false;
            //}
            //else
            //{
            //    if (!WindowManager.ControlsInMemory.Contains(this))
            //        WindowManager.ControlsInMemory.Add(this);
            //    WindowManager.Controls.Add(this);
            //}
            //return true;

            if (Show())
                return true;
            else
                return !Hide();
        }
        public virtual bool ToggleSmart()
        {
            if(!this.IsOpen)
                this.SmartPosition();

            if (Show())
            {
                return true;
            }
            else
                return !Hide();
        }
        public virtual void BringToFront()
        {
            if (Parent.IsNull())
            {
                if (this.WindowManager.Layers[Layer].Remove(this))
                    this.WindowManager.Layers[Layer].Add(this);
                return;
            }
            if (Parent.Controls.Remove(this))
                Parent.Controls.Add(this);
            Parent.BringToFront();
        }

        public Action ShowAction = () => { };
        public virtual bool Show(params object[] p)
        {
            this.OnShow();
            ShowAction();
            //Invalidate(true);
            if (!WindowManager.Layers[this.Layer].Contains(this))
            {
                if (!WindowManager.ControlsInMemory.Contains(this))
                    WindowManager.ControlsInMemory.Add(this);
                WindowManager[this.Layer].Add(this);
                return true;
            }
          //  WindowManager.BringToFront(this);
            this.BringToFront();
            return false;
        }
        protected virtual void OnShow()
        {
            foreach (var c in this.Controls)
                c.OnShow();
        }
        public bool IsOpen
        {
            get
            {
                return WindowManager[Layer].Contains(this);
            }
        }

        public virtual bool Show(UIManager manager)
        {
            if (!manager[Layer].Contains(this))
            {
                if (!manager.ControlsInMemory.Contains(this))
                    manager.ControlsInMemory.Add(this);
                manager[Layer].Add(this);
                return true;
            }

            return false;
        }
        Action _HideAction = () => { };
        public virtual Action HideAction
        { get { return _HideAction; } set { _HideAction = value; } }
        public event EventHandler<EventArgs> Hidden;
        protected virtual void OnHidden()
        {
            HideAction();
            if (Hidden != null)
                Hidden(this, EventArgs.Empty);
        }
        public virtual bool Hide()
        {
            if (WindowManager[Layer].Contains(this))
            {
                WindowManager[Layer].Remove(this);
                return true;
            }
            // if (WindowManager.Windows.Contains(this))
            //     WindowManager.Windows.Remove(this);
            return false;
        }

        public virtual bool Remove()
        {
            Dispose();
            Hide();
            if (WindowManager.ControlsInMemory.Contains(this))
                WindowManager.ControlsInMemory.Remove(this);
            return false;
        }

        public virtual void DrawOnCamera(SpriteBatch sb, Camera camera) { }
        public virtual void DrawWorld(MySpriteBatch sb, Camera camera)
        {

        }

        public virtual Window ToWindow(string name = "")
        {
            if (this is Window)
                return this as Window;
            Window window = new Window() { Title = name, Movable = true, AutoSize = true, Location = UIManager.Mouse };//, TintFunc = ()=>Color.Black };
            //if(name.Length==0)
            //    window.Controls.Remove(window.labe
            window.Client.Controls.Add(this);
            return window;
        }
        public Action<GameEvent> OnGameEventAction = (e) => { };
        internal virtual void OnGameEvent(GameEvent e)
        {
            this.OnGameEventAction(e);
            foreach (var child in this.Controls)
                child.OnGameEvent(e);
        }
        //public Window ToWindow(string name)
        //{
        //    Window win = new Window();
        //    win.Title = name;
        //    win.AutoSize = true;
        //    win.Client.AddControls(this);
        //    return win;
        //}

        public void SmartPosition()
        {
            if (this.IsOpen)
                return;
            var rect = this.WindowManager.FindBestUncoveredRectangle(new Vector2(this.ScreenBounds.Width, this.ScreenBounds.Height));
            this.Location = new Vector2(rect.X, rect.Y);
        }

    }
}
