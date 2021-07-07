using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UI;

namespace Start_a_Town_.UI
{
    public abstract class Control : Element, IDisposable, ITooltippable, IKeyEventHandler, IBounded, IBoundedCollection
    {
        /// <summary>
        /// Gets the location of the control relative to the parent window.
        /// </summary>
        /// <returns></returns>
        public virtual Vector2 GetLocation()
        {
            return Location + (Parent != null ? Parent.ClientLocation + Parent.GetLocation() : Vector2.Zero);
        }
        public virtual Vector2 ScreenLocation
        {
            get
            {
                return LocationFunc() + Location
               - this.Dimensions * this.Anchor
                  + (Parent != null ? Parent.ScreenLocation + Parent.ClientLocation : Vector2.Zero);
            }
        }
        public int X
        {
            get { return (int)ScreenLocation.X; }
        }
        public int Y
        {
            get { return (int)ScreenLocation.Y; }
        }
        Control _Parent;
        public Control Parent
        {
            get { return _Parent; }
            set
            {
                _Parent = value;
                OnParentChanged();
            }
        }

        internal bool ContainsMouse()
        {
            return this.BoundsScreen.Intersects(UIManager.MouseRect);
        }

        public virtual void Refresh()
        {
            var ctrls = this.Controls.ToArray();
            this.ClearControls();
            this.AddControls(ctrls.ToArray());
        }
        public Control SnapToScreenCenter()
        {
            this.Location = new Vector2(UIManager.Width, UIManager.Height) / 2;
            this.Anchor = Vector2.One / 2f;
            return this;
        }
        internal Control MoveToScreenCenter()
        {
            this.Location = new Vector2(UIManager.Width - this.Width, UIManager.Height - this.Height) / 2;
            return this;
        }
        internal Control SetAnchor(Vector2 anchor)
        {
            this.Anchor = anchor;
            return this;
        }

        public Control AnchorToScreenCenter()
        {
            this.LocationFunc = () => UIManager.Center;
            this.Anchor = Vector2.One / 2f;
            return this;
        }
        public Control CenterLeftScreen()
        {
            this.Location = new Vector2(0, (UIManager.Height - this.Height) / 2);
            return this;
        }
        public Vector2 LeftCenterScreen
        { get { return new Vector2(0, (UIManager.Height - Height) / 2); } }
        public Vector2 RightCenterScreen
        { get { return new Vector2(UIManager.Width - this.Width, (UIManager.Height - Height) / 2); } }
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
        public virtual bool MouseThrough
        {
            get { return _MouseThrough; }
            set { _MouseThrough = value; }
        }
        public bool IsMouseThrough
        {
            get { return MouseThrough; }
        }
        LayerTypes _Layer = LayerTypes.Windows;
        public LayerTypes Layer { get { return (this.Parent != null ? Parent.Layer : _Layer); } set { _Layer = value; } }
        public DrawMode DrawMode = DrawMode.Normal;
        public event EventHandler<DrawItemEventArgs> DrawItem;
        public void OnDrawItem(DrawItemEventArgs e)
        {
            DrawItem?.Invoke(this, e);
        }

        public event EventHandler<TooltipArgs> DrawTooltip;
        protected virtual void OnDrawTooltip(TooltipArgs e)
        {
            DrawTooltip?.Invoke(this, e);
        }
        Object _Tag;

        public virtual Object Tag { get { return _Tag; } set { _Tag = value; OnTagChanged(); } }
        public event EventHandler<EventArgs> GotFocus, MouseEnter, MouseLeave, LostFocus, TooltipChanged, TagChanged;
        public event EventHandler<KeyPressEventArgs2> KeyPress, KeyRelease;
        UIManager _WindowManager;
        public UIManager WindowManager { get { return _WindowManager ?? ScreenManager.CurrentScreen.WindowManager; } set { _WindowManager = value; } }
        public bool AllowDrop, ClipToBounds = true;
        public Action LostFocusAction = () => { };
        public Action ControlsChangedAction = () => { };
        public Action OnUpdate = () => { };
        public Action<SpriteBatch, Rectangle> OnDrawAction = (sb, bounds) => { };
        string _Name;
        public string NameFormat;
        public virtual Func<string> NameFunc { get; set; }
        public virtual string Name { get { return NameFunc == null ? _Name : NameFunc(); } set { _Name = value; } }
        public UIManager ui;
        public Dictionary<Components.Message.Types, Action<Control, GameEvent>> GameEventHandlers = new Dictionary<Components.Message.Types, Action<Control, GameEvent>>();
        public int ID;
        public float t = 0, dt = -0.05f;
        public Color Alpha, Blend;
        Color _BackgroundColor = Color.Transparent;
        public Func<Color> BackgroundColorFunc;
        public virtual Color BackgroundColor
        {
            get
            {
                if (BackgroundColorFunc == null)
                    return _BackgroundColor;
                else
                    return BackgroundColorFunc();
            }
            set { _BackgroundColor = value; this.Invalidate(); }
        }

        public override Vector2 Dimensions { get => base.Dimensions; set { base.Dimensions = value; OnSizeChanged(); } }
        float _Opacity = 1;
        public virtual float Opacity
        {
            get
            {
                return this._Opacity;
            }
            set { _Opacity = value; }
        }
        public virtual RenderTarget2D Texture { get; set; }
        public bool Focused { get { return Controller.Instance.MouseoverBlock.Object == this; } } //return WindowManager.ActiveControl == this; } }
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
            this.Controls.AlignVertically();
        }
        public virtual void AlignLeftToRight()
        {
            this.Controls.AlignHorizontally();
        }
        public Control SetControls(params Control[] controls)
        {
            foreach (Control ctrl in controls)
                Controls.Add(ctrl);
            return this;
        }

        protected bool Valid = false;
        public bool IsValidated { get { return this.Valid; } }
        public virtual Control Invalidate(bool invalidateChildren = false)
        {

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
        }

        public virtual void Reposition(float ratio = 1)
        {
            Location = Location / ratio - (new Vector2(Size.Width - Size.Width / ratio, Size.Height - Size.Height / ratio) * Anchor);
        }

        public virtual void Reposition(Vector2 ratio)
        {
            Location = Location * ratio - (new Vector2(this.Size.Width - this.Size.Width * ratio.X, this.Size.Height - this.Size.Height * ratio.Y) * Anchor);
        }

        public void AlignVertically(HorizontalAlignment alignment = HorizontalAlignment.Left)
        {
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
        public Vector2 ClientLocation = Vector2.Zero;

        internal Control SnapToMouse()
        {
            this.SetLocation(UIManager.Mouse);
            return this;
        }
        public Control SetTag(object tag)
        {
            this.Tag = tag;
            return this;
        }

        Color _Tint = Color.White;
        public Func<Color> TintFunc;
        public virtual Color Tint
        {
            get
            {
                return this.TintFunc?.Invoke() ?? this._Tint;
            }
            set
            {
                this._Tint = value;
                this.Invalidate();
            }
        }

        Color _Color = Color.White * .5f;
        public Func<Color> ColorFunc;
        public virtual Color Color
        {
            get
            {
                if (ColorFunc == null)
                    return _Color;
                else
                    return ColorFunc();
            }
            set
            {
                _Color = value;
                this.Invalidate();
            }
        }
        public virtual void Validate(bool cascade = false)
        {
            PreparingPaint();

            if (this.Width <= 0 ||
                this.Height <= 0)
                return;

            var gd = Game1.Instance.GraphicsDevice;
            this.Texture = this.CreateTexture(gd);
            var sb = new SpriteBatch(gd);
            gd.SetRenderTarget(Texture);
            gd.Clear(this.BackgroundColor);

            sb.Begin();
            OnPaint(sb);
            sb.End();

            sb.Begin();
            OnAfterPaint(sb);
            sb.End();

            this.Valid = true;

            if (cascade)
                foreach (var c in this.Controls)
                    c.Validate(cascade);
        }
        public virtual void PreparingPaint() { }
        public virtual void OnPaint(SpriteBatch sb) { }
        public virtual void OnAfterPaint(SpriteBatch sb) { }
        public virtual RenderTarget2D CreateTexture(GraphicsDevice gd) { return new RenderTarget2D(Game1.Instance.GraphicsDevice, this.Width, this.Height); }
        Texture2D _BackgroundTexture;

        public Action<TargetArgs> OnSelectedTargetChangedAction;
        internal virtual void OnSelectedTargetChanged(TargetArgs target)
        {
            OnSelectedTargetChangedAction?.Invoke(target);
            foreach (var c in this.Controls.ToArray())
                c.OnSelectedTargetChanged(target);
        }
        public Control SetOnSelectedTargetChangedAction(Action<Control, TargetArgs> action)
        {
            this.OnSelectedTargetChangedAction = (t) => action(this, t);
            return this;
        }
        public Control SetOnSelectedTargetChangedAction(Action<TargetArgs> action)
        {
            this.OnSelectedTargetChangedAction = action;
            return this;
        }
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

        internal bool isPressed = false, Active = true;
        public event UIEvent LeftClick, RightClick,
            MouseUp, MouseWheelDown, MouseWheelUp,
            TopMostControlChanged;
        public event EventHandler<System.Windows.Forms.HandledMouseEventArgs> MouseDown, MouseLeftUp, MouseLeftDown, MouseRightPress, MouseRightUp, MouseScroll, MouseMUp, MouseMDown;
        public event EventHandler<System.Windows.Forms.HandledMouseEventArgs> MouseWheel, MouseMove, MouseLeftPress;

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
        public Window Window => this.GetWindow();
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
            Location = location;
        }
        public Control(Vector2 location, Vector2 size)
            : this()
        {
            Width = (int)size.X;
            Height = (int)size.Y;
            Location = location;
        }
        public Control()
        {
            Location = new Vector2(0);
        }

        #region Raise Events
        protected virtual void OnTagChanged()
        {
            TagChanged?.Invoke(this, EventArgs.Empty);
        }
        protected virtual void OnTooltipChanged()
        {
            TooltipChanged?.Invoke(this, new EventArgs());
        }
        protected virtual void OnMouseWheelUp()
        {
            MouseWheelUp?.Invoke(this, new EventArgs());
        }
        protected virtual void OnMouseWheelDown()
        {
            MouseWheelDown?.Invoke(this, new EventArgs());
        }
        protected virtual void OnMouseMove()
        {

        }
        protected virtual void OnMouseMove(System.Windows.Forms.HandledMouseEventArgs e)
        {
            MouseMove?.Invoke(this, e);
        }
        protected virtual void OnMouseDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            MouseDown?.Invoke(this, e);
        }
        protected virtual void OnMouseUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            MouseUp?.Invoke(this, e);
        }
        protected virtual void OnTopMostControlChanged()
        {
            TopMostControlChanged?.Invoke(this, new UIEventArgs(this));
        }
        protected virtual void OnLeftClick()
        {
            LeftClick?.Invoke(this, new UIEventArgs(this));
        }
        protected virtual void OnRightClick()
        {
            RightClick?.Invoke(this, new UIEventArgs(this));
        }
        protected virtual void OnKeyPress(KeyPressEventArgs2 e)
        {
            KeyPress?.Invoke(this, e);
        }
        protected virtual void OnKeyRelease(KeyPressEventArgs2 e)
        {
            KeyRelease?.Invoke(this, e);
        }
        #endregion

        public bool IsTopMost
        {
            get { return WindowManager.ActiveControl == this; }
        }

        protected virtual void OnGotFocus()
        {
            GotFocus?.Invoke(this, EventArgs.Empty);

        }
        public virtual void OnLostFocus()
        {
            this.LostFocusAction();
            if (LostFocus != null)
                LostFocus(this, EventArgs.Empty);

        }
        Action MouseEnterAction = () => { }, MouseLeaveAction = () => { };
        public Action MouseLBActionOld = () => { };
        public Action MouseRBAction;
        public Action MouseLBAction = () => { };

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
            this.MouseLBActionOld();
            Root.BringToFront();
            if (MouseLeftPress != null)
                MouseLeftPress(this, e);
        }
        protected virtual void OnMouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            MouseLeftUp?.Invoke(this, e);
        }
        protected virtual void OnMouseRightPress(System.Windows.Forms.HandledMouseEventArgs e)
        {
            MouseRightPress?.Invoke(this, e);
        }
        protected virtual void OnMouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            MouseRightUp?.Invoke(this, e);
        }
        protected virtual void OnMouseLeftDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            MouseLeftDown?.Invoke(this, e);
        }
        protected virtual void OnMouseMDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            MouseMDown?.Invoke(this, e);
        }
        protected virtual void OnMouseMUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            MouseMUp?.Invoke(this, e);
        }
        protected virtual void OnMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            MouseWheel?.Invoke(this, e);
        }
        protected virtual void OnMouseScroll(System.Windows.Forms.HandledMouseEventArgs e)
        {
            MouseScroll?.Invoke(this, e);
        }
        protected virtual void OnLButtonDblClk(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.OnMouseLeftPress(e);
        }
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
        {
            get
            {
                return this.TopLeft + new Vector2(Width / 2, Height);
            }
        }

        public Vector2 BottomLeft
        {
            get { return this.TopLeft + Vector2.UnitY * this.Height; }
        }
        public Vector2 BottomRight
        { get { return this.TopLeft + new Vector2(this.Width, this.Height); } }
        public Vector2 TopLeft
        { get { return this.Location + this.LocationFunc() - this.Dimensions * this.Anchor; } }
        public Vector2 TopRight
        { get { return TopLeft + Vector2.UnitX * this.Width; } }
        public Vector2 CenterRight
        { get { return new Vector2(Right, Top + Height / 2); } }

        public Rectangle ClientRectangle
        {
            get { return new Rectangle((int)ClientLocation.X, (int)ClientLocation.Y, ClientSize.Width, ClientSize.Height); }
            set
            {
                ClientLocation.X = value.X; ClientLocation.Y = value.Y;
                ClientSize = new Rectangle(0, 0, value.Width, value.Height);
            }
        }

        public virtual bool HitTest(Rectangle viewport)
        {
            return ((!this.IsMouseThrough) && Rectangle.Intersect(viewport, this.BoundsScreen).Intersects(new Rectangle((int)(Controller.Instance.msCurrent.X / UIManager.Scale), (int)(Controller.Instance.msCurrent.Y / UIManager.Scale), 1, 1)));
        }
        public virtual bool HitTest()
        {
            return ((!this.IsMouseThrough) && BoundsScreen.Intersects(new Rectangle((int)(Controller.Instance.msCurrent.X / UIManager.Scale), (int)(Controller.Instance.msCurrent.Y / UIManager.Scale), 1, 1)));
        }

        public Vector2 ScreenClientLocation
        {
            get { return new Vector2(X + ClientLocation.X, Y + ClientLocation.Y); }
        }
        public Rectangle ScreenClientRectangle
        {
            get { return new Rectangle(X + (int)ClientLocation.X, Y + (int)ClientLocation.Y, ClientSize.Width, ClientSize.Height); }
        }
        public virtual Rectangle BoundsScreen
        {
            get { return new Rectangle((int)ScreenLocation.X, (int)ScreenLocation.Y, Width, Height); }
        }
        public virtual Rectangle BoundsLocal
        {
            get { return new Rectangle((int)Location.X, (int)Location.Y, Width, Height); }
        }

        protected Rectangle _ClientSize;
        public virtual Rectangle ClientSize
        {
            get
            {
                return _ClientSize;
            }
            set
            {
                _ClientSize = value;

                if (AutoSize)
                {
                    Width = ClientSize.Width + (_BackgroundStyle != null ? 2 * _BackgroundStyle.Border : 0);
                    Height = ClientSize.Height + (_BackgroundStyle != null ? 2 * _BackgroundStyle.Border : 0);
                }
                this.Invalidate();
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
            if (!exclude.Contains(this))
                this.Opacity = value;
            if (children)
                foreach (Control control in Controls)
                    control.SetOpacity(value, children, exclude);
        }
        public virtual Control SetMousethrough(bool value, bool children = false)
        {
            this.MouseThrough = value;
            if (children)
                foreach (Control control in Controls)
                    control.SetMousethrough(value, children);
            return this;
        }

        public virtual Vector2 ClientDimensions
        {
            get
            { return new Vector2(ClientSize.Width, ClientSize.Height); }
            set
            {
                ClientSize = new Rectangle(0, 0, (int)value.X, (int)value.Y);
            }
        }
        public virtual Rectangle Size
        {
            get
            { return new Rectangle(0, 0, Width, Height); }
            set
            {
                var oldheight = this.Height;
                Width = value.Width;
                Height = value.Height;
                var border = this.BackgroundStyle != null ? this.BackgroundStyle.Border : 0;
                this._ClientSize.Width = Width - 2 * border;
                this._ClientSize.Height = Height - 2 * border;
                OnSizeChanged();
            }
        }
        internal void ConformToControls()
        {
            this.Conform(this.Controls.ToArray());
        }
        public virtual void Conform(IEnumerable<Control> controls)
        {
            this.Conform(controls.ToArray());
        }
        public virtual void Conform(params Control[] controls)
        {
            var rects = new Queue<Control>(controls);
            var union = new Rectangle(0, 0, 0, 0);
            while (rects.Count > 0)
                union = Rectangle.Union(union, rects.Dequeue().BoundsLocal);
            this.ClientSize = union;
        }
        public event EventHandler<EventArgs> SizeChanged;
        protected virtual void OnSizeChanged()
        {
            SizeChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual bool AutoSize { get; set; }
        public event EventHandler<EventArgs> ControlAdded, ControlRemoved;

        internal virtual void OnControlAdded(Control control)
        {
            if (this.AutoSize)
            {
                this.ClientSize = PreferredClientSize;
                ResizeToClientSize();
            }
            if (Parent != null)
                Parent.OnControlAdded(control);

            this.ControlsChangedAction();
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
            {
                ClientSize = PreferredClientSize;

                ResizeToClientSize();
            }
            if (Parent != null)
                Parent.OnControlRemoved(control);
            this.ControlsChangedAction();
        }

        public Vector2 Origin = new Vector2(0);
        public virtual void DrawHighlight(SpriteBatch sb, float alpha = 0.5f)
        {
            sb.Draw(UIManager.Highlight, new Rectangle((int)(ScreenLocation.X - Origin.X), (int)(ScreenLocation.Y - Origin.Y), BoundsScreen.Width, BoundsScreen.Height), null, Color.Lerp(Color.Transparent, Color.White, alpha), 0, Vector2.Zero, SpriteEffects.None, Depth);
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
                    width = Math.Max(width, (int)control.TopLeft.X + control.Width - (int)control.Origin.X);
                    height = Math.Max(height, (int)control.TopLeft.Y + control.Height - (int)control.Origin.Y);
                }
                return new Rectangle(0, 0, width, height);
            }
        }

        public virtual void OnBeforeDraw(SpriteBatch sb, Rectangle viewport) { }
        public virtual void OnAfterDraw(SpriteBatch sb, Rectangle viewport) { }
        public virtual void OnHitTestPass()
        {
            Controller.Instance.MouseoverBlockNext.Object = this;
        }
        public virtual void Draw(SpriteBatch sb, Rectangle viewport)
        {
            OnDrawItem(new DrawItemEventArgs(sb, BoundsScreen));
            OnBeforeDraw(sb, viewport);
            Color c = Tint;
            if (Texture != null)
            {
                Rectangle final, source;
                // TODO: this is no use because if i add something outside the window's client area and the window has autosize, it expands the client area screwing up hittesting
                if (!ClipToBounds)
                {
                    sb.Draw(this.Texture, this.BoundsScreen, null, c * Opacity, Rotation, this.Origin, SpriteEffects.None, 0);
                }
                else
                {
                    this.BoundsScreen.Clip(Texture.Bounds, viewport, out final, out source);
                    sb.Draw(this.Texture, final, source, c * Opacity, Rotation, this.Origin, SpriteEffects.None, 0);
                    OnDrawAction(sb, final);
                }

            }
            OnAfterDraw(sb, viewport);

            foreach (Control control in this.Controls)
                control.Draw(sb, Rectangle.Intersect(control.BoundsScreen, viewport));
        }

        public virtual void Draw(SpriteBatch sb)
        {
            // TODO: maybe put that somewhere else?
            if (HitTest())
                Controller.Instance.MouseoverBlockNext.Object = this;


            foreach (Control control in Controls)
                control.Draw(sb);
            OnDrawItem(new DrawItemEventArgs(sb, BoundsScreen));
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
                if (this.HoverFunc == null)
                    return _HoverText;
                else
                    return HoverFunc();
            }
            set { _HoverText = value; }
        }

        public Action<Tooltip> TooltipFunc;
        public bool CustomTooltip;
        public virtual void GetTooltipInfo(Tooltip tooltip)
        {
            if (TooltipFunc != null)
            {
                TooltipFunc(tooltip);
                return;
            }
            if (this.HoverText.Length > 0)
                tooltip.Controls.Add(new Label(HoverText, HoverFormat) { TextFunc = this.HoverFunc });

            if (CustomTooltip)
            {
                List<GroupBox> tt = new List<GroupBox>();
                OnDrawTooltip(new TooltipArgs(tooltip));
            }
        }
        public override void Update()
        {
            this.OnUpdate();

            base.Update();

            List<Control> copy = this.Controls.ToList();
            foreach (Control control in copy)
                control.Update();

            if (!Valid)
                this.Validate(); // i put this here after calling base.update because if the size of the control was changed then for one frame it was drawn stretched
        }

        public virtual void ClearControls()
        {
            this.Controls.Clear();
        }
        public virtual Control AddControlsVertically(params Control[] controls)
        {
            return this.AddControlsVertically(0, controls);
        }
        public virtual Control AddControlsVertically(int spacing, params Control[] controls)
        {
            var y = 0;
            foreach (var ctrl in controls)
            {
                ctrl.Location = new Vector2(0, y);
                this.Controls.Add(ctrl);
                y += ctrl.Height + spacing;
            }
            return this;
        }
        public virtual Control AddControlsHorizontally(params Control[] controls)
        {
            return this.AddControlsHorizontally(0, controls);
        }
        public virtual Control AddControlsHorizontally(int spacing, params Control[] controls)
        {
            var x = 0;
            foreach (var ctrl in controls)
            {
                ctrl.Location = new Vector2(x, 0);
                this.Controls.Add(ctrl);
                x += ctrl.Width + spacing;
            }
            return this;
        }
        public virtual Control AddControlsSmart(params Control[] controls)
        {
            foreach (var ctrl in controls)
            {
                var rect = this.FindBestUncoveredRectangle(ctrl.Size.Width, ctrl.Size.Height);
                ctrl.Location = new Vector2(rect.Left, rect.Top);
                this.Controls.Add(ctrl);
            }
            return this;
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
        public virtual Control AddControlsTopRight(params Control[] controls)
        {
            foreach (var c in controls)
            {
                c.Location = this.Controls.TopRight;
                this.Controls.Add(c);
            }
            return this;
        }
        public virtual void AddControlsTopRight(int spacing, params Control[] controls)
        {
            foreach (var c in controls)
            {
                c.Location = this.Controls.TopRight + spacing * Vector2.One;
                this.Controls.Add(c);
            }
        }
        public virtual void RemoveControls(params Control[] controls)
        {
            foreach (var ctrl in controls)
                this.Controls.Remove(ctrl);
        }
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
        }
        public virtual void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.MouseLBAction?.Invoke();

            foreach (var c in this.Controls.ToList())
                c.HandleLButtonDown(e);
            if (WindowManager.ActiveControl == this)
            {
                this.Select();
                OnMouseLeftPress(e);
                return;
            }
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
            OnMouseLeftUp(e);
        }
        public virtual void HandleMiddleUp(System.Windows.Forms.HandledMouseEventArgs e)
        {

            if (this.HasChildren)
            {
                List<Control> controls = Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                    c.HandleMiddleUp(e);
            }
            OnMouseMUp(e);
        }
        public virtual void HandleMiddleDown(System.Windows.Forms.HandledMouseEventArgs e)
        {

            if (this.HasChildren)
            {
                List<Control> controls = Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                    c.HandleMiddleDown(e);
            }
            OnMouseMDown(e);
        }
        public virtual void HandleRButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.MouseRBAction?.Invoke();
            foreach (var c in this.Controls.ToList())
                c.HandleRButtonDown(e);
            if (WindowManager.ActiveControl == this)
            {
                OnMouseRightPress(e);
                return;
            }
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
        public virtual void HandleLButtonDoubleClick(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.HasChildren)
            {
                List<Control> controls = Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                    c.HandleLButtonDoubleClick(e);
            }
            if (WindowManager.ActiveControl == this)
            {
                this.Select();
                OnLButtonDblClk(e);
                return;
            }
        }
        public virtual Control Toggle(Vector2 loc)
        {
            this.SetLocation(loc);
            this.Toggle();
            return this;
        }
        public virtual bool Toggle()
        {
            return this.Toggle(this.Layer);
        }
        public virtual bool Toggle(LayerTypes layer)
        {
            this.Layer = layer;

            if (Show(layer))
                return true;
            else
                return !Hide();
        }
        public virtual bool ToggleSmart()
        {
            if (!this.IsOpen)
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
            if (Parent == null)
            {
                if (this.WindowManager.Layers[Layer].Remove(this))
                    this.WindowManager.Layers[Layer].Add(this);
                return;
            }
            if (Parent.Controls.Remove(this))
                Parent.Controls.Add(this);
            Parent.BringToFront();
        }
        public object DataSource;
        Action<object> GetDataAction = data => { };
        public Control GetData(object dataSource)
        {
            this.DataSource = dataSource;
            this.GetDataAction?.Invoke(dataSource);
            return this;
        }
        public Control GetData(object dataSource, bool cascade)
        {
            this.DataSource = dataSource;
            this.GetDataAction?.Invoke(dataSource);
            if (cascade)
                foreach (var c in this.Controls)
                    c.GetData(dataSource, cascade);
            return this;
        }
        public Control SetGetDataAction(Action<object> action)
        {
            this.GetDataAction = action;
            return this;
        }
        public Action ShowAction = () => { };

        public virtual bool Show()
        {
            return this.Show(this.Layer);
        }
        public virtual bool Show(LayerTypes layer)
        {
            this.Layer = layer;
            this.OnShow();

            ShowAction();
            if (!WindowManager.Layers[this.Layer].Contains(this))
            {
                if (!WindowManager.ControlsInMemory.Contains(this))
                    WindowManager.ControlsInMemory.Add(this);
                WindowManager[this.Layer].Add(this);
                this.ConformToScreen();
                return true;
            }
            this.BringToFront();
            return false;
        }
        public Action OnShowAction = () => { };
        protected virtual void OnShow()
        {
            this.OnShowAction();
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

        public virtual Rectangle Bounds => this.BoundsLocal;
        public virtual Rectangle ContainerSize => this.BoundsLocal;
        public IBounded[] Children => this.Controls.ToArray();

        protected virtual void OnHidden()
        {
            HideAction();
            foreach (var ch in this.Controls)
                ch.OnHidden();
        }
        public virtual bool Hide()
        {
            if (WindowManager.Remove(this))
            {
                if (DialogBlock != null)
                {
                    DialogBlock = null;
                }
                OnHidden();
                return true;
            }

            return false;
        }
        public virtual bool HideOld()
        {
            if (WindowManager[Layer].Contains(this))
            {
                WindowManager[Layer].Remove(this);
                return true;
            }
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

        public virtual void DrawOnCamera(SpriteBatch sb, Camera camera)
        {
            foreach (var ch in this.Controls)
                ch.DrawOnCamera(sb, camera);
        }
        public virtual void DrawWorld(MySpriteBatch sb, Camera camera)
        {
            foreach (var ch in this.Controls)
                ch.DrawWorld(sb, camera);
        }

        public virtual Window ToWindow(string name = "", bool closable = true, bool movable = true)
        {
            if (this is Window)
                return this as Window;
            Window window = new Window()
            {
                Title = name,
                Movable = movable,
                AutoSize = true,
                Closable = closable
            };
            window.Client.Controls.Add(this);
            return window;
        }
        public Action<GameEvent> OnGameEventAction = (e) => { };
        readonly Dictionary<Components.Message.Types, Action<GameEvent>> Listeners = new();
        public void ListenTo(Components.Message.Types msgType, Action<object[]> action)
        {
            this.Listeners.Add(msgType, e => action(e.Parameters));
        }
        internal virtual void OnGameEvent(GameEvent e)
        {
            this.OnGameEventAction(e);
            this.Listeners.TryGetValue(e.Type, a => a(e));
            foreach (var child in this.Controls)
                child.OnGameEvent(e);
        }
        internal Control AnchorTo(Vector2 location, Vector2 anchor)
        {
            this.Location = location;
            this.Anchor = anchor;
            return this;
        }
        public void SmartPosition()
        {
            if (this.IsOpen)
                return;
            var rect = this.WindowManager.FindBestUncoveredRectangle(new Vector2(this.BoundsScreen.Width, this.BoundsScreen.Height));
            this.Location = new Vector2(rect.X, rect.Y);
        }

        internal virtual void OnWindowHidden(Window window)
        {
            foreach (var c in this.Controls)
                c.OnWindowHidden(window);
        }

        public virtual void Update(Rectangle rectangle)
        {
            if (HitTest(rectangle))
                this.OnHitTestPass();
            foreach (var c in this.Controls)
                c.Update(Rectangle.Intersect(rectangle, this.BoundsScreen));
        }

        public virtual Control FindChild(Func<Control, bool> predicate)
        {
            foreach (var chi in this.Controls)
                if (predicate(chi))
                    return chi;
            foreach (var chi in this.Controls)
            {
                var found = chi.FindChild(predicate);
                if (found != null)
                    return found;
            }
            return null;
        }

        protected DialogBlock DialogBlock;
        public virtual bool ShowDialog()
        {
            WindowManager.Layers[LayerTypes.Dialog].Remove(DialogBlock.Instance);
            WindowManager.Layers[LayerTypes.Dialog].Add(DialogBlock.Instance);
            this.Layer = LayerTypes.Dialog;

            SnapToScreenCenter();

            return this.Show();
        }

        internal virtual void OnControlResized(ButtonBase buttonBase)
        {

        }
        public Control SetGameEventAction(Action<GameEvent> onGameEventAction)
        {
            this.OnGameEventAction = onGameEventAction;
            return this;
        }
    }
}
