﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
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
            return this.Location + (this.Parent != null ? this.Parent.ClientLocation + this.Parent.GetLocation() : Vector2.Zero);
        }
        public virtual Vector2 ScreenLocation => this.LocationFunc() + this.Location
               - this.Dimensions * this.Anchor
                  + (this.Parent != null ? this.Parent.ScreenLocation + this.Parent.ClientLocation : Vector2.Zero);
        public int X => (int)this.ScreenLocation.X;
        public int Y => (int)this.ScreenLocation.Y;

        Control _Parent;
        public Control Parent
        {
            get => this._Parent;
            set
            {
                this._Parent = value;
                this.OnParentChanged();
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
        /// <summary>
        /// this is wrong when the control's height is larger than half the screen's height
        /// </summary>
        /// <returns></returns>
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
            this.Anchor = new(.5f);
            return this;
        }
        public Control CenterLeftScreen()
        {
            this.Location = new(0, (UIManager.Height - this.Height) / 2);
            return this;
        }
        public Vector2 LeftCenterScreen => new(0, (UIManager.Height - this.Height) / 2);
        public Vector2 RightCenterScreen => new(UIManager.Width - this.Width, (UIManager.Height - this.Height) / 2);
        public Vector2 BottomLeftScreen => new(0, UIManager.Height - this.Height);
        public Vector2 BottomRightScreen => new(UIManager.Width - this.Width, UIManager.Height - this.Height);
        public Vector2 BottomCenterScreen => new((UIManager.Width - this.Width) / 2, UIManager.Height - this.Height);
        public Vector2 TopRightScreen => new(UIManager.Width - this.Width, 0);

        public virtual void ConformToScreen()
        {
            this.Location.X = Math.Max(0, Math.Min(UIManager.Width - this.Width, this.Location.X));
            this.Location.Y = Math.Max(0, Math.Min(UIManager.Height - this.Height, this.Location.Y));
        }

        public float Rotation = 0;
        bool _mouseThrough = false;
        public virtual bool MouseThrough
        {
            get => this._mouseThrough;
            set => this._mouseThrough = value;
        }
        public bool IsMouseThrough => this.MouseThrough;
        public GuiLayer Layer = UIManager.LayerWindows;

        public DrawMode DrawMode = DrawMode.Normal;
        public Vector2 ClientLocation = Vector2.Zero;

        object _tag;
        public virtual object Tag { get => this._tag; set { this._tag = value; this.OnTagChanged(); } }
        UIManager _windowManager;
        public UIManager WindowManager { get => this._windowManager ?? ScreenManager.CurrentScreen.WindowManager; set => this._windowManager = value; }
        //IWindowManager _windowManager;
        //public IWindowManager WindowManager { get => this._windowManager; set => this._windowManager = value; }

        public bool AllowDrop, ClipToBounds = true;
        public Action LostFocusAction = () => { };
        public Action ControlsChangedAction = () => { };
        public Action OnUpdate = () => { };
        public Action<SpriteBatch, Rectangle> OnDrawAction = (sb, bounds) => { };
        string _name;
        public string NameFormat;
        public virtual Func<string> NameFunc { get; set; }
        public virtual string Name { get => this.NameFunc?.Invoke() ?? this._name; set => this._name = value; }
        public Dictionary<Components.Message.Types, Action<Control, GameEvent>> GameEventHandlers = new();
        public int ID;
        public float t = 0, dt = -0.05f;
        public Color Alpha, Blend;
        Color _backgroundColor = Color.Transparent;
        public Func<Color> BackgroundColorFunc;
        public virtual Color BackgroundColor
        {
            get => this.BackgroundColorFunc?.Invoke() ?? this._backgroundColor;
            set
            {
                this._backgroundColor = value;
                this.Invalidate();
            }
        }

        public override Vector2 Dimensions { get => base.Dimensions; set { base.Dimensions = value; this.OnSizeChanged(); } }
        float _opacity = 1;
        public virtual float Opacity
        {
            get => this._opacity;
            set => this._opacity = value;
        }
        public virtual RenderTarget2D Texture { get; set; }
        public bool Focused => Controller.Instance.MouseoverBlock.Object == this;  //return WindowManager.ActiveControl == this; } }
        ControlCollection _controls;
        public ControlCollection Controls => this._controls ??= new ControlCollection(this);
        
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
                this.Controls.Add(ctrl);
            return this;
        }

        protected bool Valid = false;
        public bool IsValidated => this.Valid;
        public virtual Control Invalidate(bool invalidateChildren = false)
        {

            this.Valid = false;
            if (invalidateChildren)
                foreach (Control ctrl in this.Controls)
                    ctrl.Invalidate(invalidateChildren);
            return this;
        }

        public virtual void Reposition(UIScaleEventArgs e = null)
        {
            if (this.LocationFunc is not null)
                return;
            float ratio = e.NewScale / e.OldScale;
            this.Location = this.Location / ratio - (new Vector2(this.Size.Width - this.Size.Width / ratio, this.Size.Height - this.Size.Height / ratio) * this.Anchor);
        }

        public virtual void Reposition(float ratio = 1)
        {
            this.Location = this.Location / ratio - (new Vector2(this.Size.Width - this.Size.Width / ratio, this.Size.Height - this.Size.Height / ratio) * this.Anchor);
        }

        public virtual void Reposition(Vector2 ratio)
        {
            this.Location = this.Location * ratio - (new Vector2(this.Size.Width - this.Size.Width * ratio.X, this.Size.Height - this.Size.Height * ratio.Y) * this.Anchor);
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

            this.ReAddChildren();
        }
        public void AlignHorizontally()
        {
            Vector2 last = Vector2.Zero;
            foreach (var current in this.Controls)
            {
                current.Location = last;
                last = current.TopRight;
            }
            this.ReAddChildren();
        }

        private void ReAddChildren()
        {
            var temp = this.Controls.ToList();
            this.Controls.Clear();
            foreach (var c in temp)
                this.Controls.Add(c);
        }

        public bool HasChildren => this.Controls.Any();

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

        Color _tint = Color.White;
        public Func<Color> TintFunc;
        public virtual Color Tint
        {
            get => this.TintFunc?.Invoke() ?? this._tint;
            set
            {
                this._tint = value;
                this.Invalidate();
            }
        }

        Color _color = Color.White * .5f;
        public Func<Color> ColorFunc;
        public virtual Color Color
        {
            get => this.ColorFunc?.Invoke() ?? this._color;
            set
            {
                this._color = value;
                this.Invalidate();
            }
        }
        public virtual void Validate(bool cascade = false)
        {
            this.PreparingPaint();

            if (this.Width <= 0 ||
                this.Height <= 0)
            {
                return;
            }

            var gd = Game1.Instance.GraphicsDevice;
            this.Texture = this.CreateTexture(gd);
            var sb = new SpriteBatch(gd);
            gd.SetRenderTarget(this.Texture);
            gd.Clear(this.BackgroundColor);

            sb.Begin();
            this.OnPaint(sb);
            sb.End();

            sb.Begin();
            this.OnAfterPaint(sb);
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
            this.OnSelectedTargetChangedAction?.Invoke(target);
            foreach (var c in this.Controls.ToArray())
            {
                c.OnSelectedTargetChanged(target);
            }
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
            get => this._BackgroundTexture;
            set
            {
                this._BackgroundTexture = value;
                this.Width = this._BackgroundTexture.Width;
                this.Height = this._BackgroundTexture.Height;
            }
        }
        protected BackgroundStyle _BackgroundStyle;
        public virtual BackgroundStyle BackgroundStyle
        {
            get => this._BackgroundStyle;
            set
            {
                this._BackgroundStyle = value;
                this.ClientLocation = new Vector2(this._BackgroundStyle.Border);
                this.Width = this.ClientSize.Width + 2 * this._BackgroundStyle.Border;
                this.Height = this.ClientSize.Height + this._BackgroundStyle.Border * 2;
            }
        }

        internal bool isPressed = false, Active = true;
       
        [Obsolete]
        public event EventHandler<HandledMouseEventArgs> MouseScroll, MouseLeftPress;

        public Control TopLevelControl
        {
            get
            {
                Control parent = this;
                while (parent.Parent is not null)
                    parent = parent.Parent;

                return parent;
            }
        }
        public Window Window => this.GetWindow();
        public Window GetWindow()
        {
            Control window = this;
            while (window is not null)
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
                while (parent.Parent is not null)
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
            this.Location = location;
        }
        public Control(Vector2 location, Vector2 size)
            : this()
        {
            this.Width = (int)size.X;
            this.Height = (int)size.Y;
            this.Location = location;
        }
        public Control()
        {
            this.Location = new Vector2(0);
        }

        #region Raise Events
        protected virtual void OnTagChanged()
        {
        }
        protected virtual void OnTooltipChanged()
        {
        }
        protected virtual void OnMouseWheelUp()
        {
        }
        protected virtual void OnMouseWheelDown()
        {
        }
        protected virtual void OnMouseMove()
        {

        }
        protected virtual void OnMouseMove(HandledMouseEventArgs e)
        {
        }
        protected virtual void OnMouseDown(HandledMouseEventArgs e)
        {
        }
        protected virtual void OnMouseUp(HandledMouseEventArgs e)
        {
        }
        protected virtual void OnTopMostControlChanged()
        {
        }
        protected virtual void OnLeftClick()
        {
        }
        protected virtual void OnRightClick()
        {
        }
        protected virtual void OnKeyPress(KeyPressEventArgs2 e)
        {
        }
        protected virtual void OnKeyRelease(KeyPressEventArgs2 e)
        {
        }
        #endregion

        public bool IsTopMost => this.WindowManager.ActiveControl == this;

        protected virtual void OnGotFocus()
        {
        }
        public virtual void OnLostFocus()
        {
            this.LostFocusAction();
        }

        readonly Action MouseEnterAction = () => { }, MouseLeaveAction = () => { };
        public Action MouseLBActionOld = () => { };
        public Action MouseRBAction;
        public Action MouseLBAction = () => { };

        public virtual void OnMouseEnter()
        {
            this.MouseEnterAction();
            this.MouseHover = true;
        }
        public virtual void OnMouseLeave()
        {
            this.MouseLeaveAction();
            this.MouseHover = false;
        }
        protected virtual void OnMouseLeftPress(HandledMouseEventArgs e)
        {
            this.MouseLBActionOld();
            this.Root.BringToFront();
            this.MouseLeftPress?.Invoke(this, e);
        }
        protected virtual void OnMouseLeftUp(HandledMouseEventArgs e)
        {
        }
        protected virtual void OnMouseRightPress(HandledMouseEventArgs e)
        {
        }
        protected virtual void OnMouseRightUp(HandledMouseEventArgs e)
        {
        }
        protected virtual void OnMouseLeftDown(HandledMouseEventArgs e)
        {
        }
        protected virtual void OnMouseMDown(HandledMouseEventArgs e)
        {
        }
        protected virtual void OnMouseMUp(HandledMouseEventArgs e)
        {
        }
        protected virtual void OnMouseWheel(HandledMouseEventArgs e)
        {
        }
        protected virtual void OnMouseScroll(HandledMouseEventArgs e)
        {
            MouseScroll?.Invoke(this, e);
        }
        protected virtual void OnLButtonDblClk(HandledMouseEventArgs e)
        {
            this.OnMouseLeftPress(e);
        }
        public virtual void Select()
        {
            if (this.WindowManager.FocusedControl != null)
                if (this.WindowManager.FocusedControl != this)
                    this.WindowManager.FocusedControl.Unselect();

            this.WindowManager.FocusedControl = this;
            this.OnGotFocus();
        }
        public override void Unselect()
        {
            this.OnLostFocus();
        }

        public virtual void Dispose()
        {

            if (this.HasChildren)
                foreach (Control control in this.Controls)
                    control.Dispose();
        }
        public int Top
        {
            get => (int)this.Location.Y;
            set => this.Location.Y = value;
        }
        public int Left => (int)this.Location.X;

        public virtual int Bottom => (int)this.Location.Y + this.Height;
        public int Right => (int)this.Location.X + this.Width;
        public Vector2 Center => new(this.Width / 2, this.Height / 2);
        public Vector2 BottomCenter => this.TopLeft + new Vector2(this.Width / 2, this.Height);

        public Vector2 BottomLeft => this.TopLeft + Vector2.UnitY * this.Height;
        public Vector2 BottomRight => this.TopLeft + new Vector2(this.Width, this.Height);
        public Vector2 TopLeft => this.Location + this.LocationFunc() - this.Dimensions * this.Anchor;
        public Vector2 TopRight => this.TopLeft + Vector2.UnitX * this.Width;
        public Vector2 CenterRight => new(this.Right, this.Top + this.Height / 2);

        public Rectangle ClientRectangle
        {
            get => new Rectangle((int)this.ClientLocation.X, (int)this.ClientLocation.Y, this.ClientSize.Width, this.ClientSize.Height);
            set
            {
                this.ClientLocation.X = value.X; this.ClientLocation.Y = value.Y;
                this.ClientSize = new Rectangle(0, 0, value.Width, value.Height);
            }
        }

        public virtual bool HitTest(Rectangle viewport)
        {
            return (!this.IsMouseThrough) && Rectangle.Intersect(viewport, this.BoundsScreen).Intersects(new Rectangle((int)(Controller.Instance.msCurrent.X / UIManager.Scale), (int)(Controller.Instance.msCurrent.Y / UIManager.Scale), 1, 1));
        }

        public virtual bool HitTest()
        {
            return (!this.IsMouseThrough) && this.BoundsScreen.Intersects(new Rectangle((int)(Controller.Instance.msCurrent.X / UIManager.Scale), (int)(Controller.Instance.msCurrent.Y / UIManager.Scale), 1, 1));
        }

        public Vector2 ScreenClientLocation => new(this.X + this.ClientLocation.X, this.Y + this.ClientLocation.Y);
        public Rectangle ScreenClientRectangle => new(this.X + (int)this.ClientLocation.X, this.Y + (int)this.ClientLocation.Y, this.ClientSize.Width, this.ClientSize.Height);
        public virtual Rectangle BoundsScreen => new((int)this.ScreenLocation.X, (int)this.ScreenLocation.Y, this.Width, this.Height);
        public virtual Rectangle BoundsLocal => new((int)this.Location.X, (int)this.Location.Y, this.Width, this.Height);

        protected Rectangle _ClientSize;
        public virtual Rectangle ClientSize
        {
            get => this._ClientSize;
            set
            {
                this._ClientSize = value;

                if (this.AutoSize)
                {
                    this.Width = this.ClientSize.Width + (this._BackgroundStyle != null ? 2 * this._BackgroundStyle.Border : 0);
                    this.Height = this.ClientSize.Height + (this._BackgroundStyle != null ? 2 * this._BackgroundStyle.Border : 0);
                }
                this.Invalidate();
                this.OnClientSizeChanged();
                this.OnSizeChanged();
            }
        }
        protected virtual void OnClientSizeChanged() { }

        public void SetClipToBounds(bool value, bool children = false, params Control[] exclude)
        {
            this.ClipToBounds = value;
            if (children)
                foreach (Control control in this.Controls.Except(exclude))
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
                foreach (Control control in this.Controls)
                    control.SetOpacity(value, children, exclude);
        }
        public virtual Control SetMousethrough(bool value, bool children = false)
        {
            this.MouseThrough = value;
            if (children)
                foreach (Control control in this.Controls)
                    control.SetMousethrough(value, children);

            return this;
        }

        public virtual Vector2 ClientDimensions
        {
            get => new Vector2(this.ClientSize.Width, this.ClientSize.Height);
            set => this.ClientSize = new Rectangle(0, 0, (int)value.X, (int)value.Y);
        }
        public virtual Rectangle Size
        {
            get => new Rectangle(0, 0, this.Width, this.Height);
            set
            {
                var oldheight = this.Height;
                this.Width = value.Width;
                this.Height = value.Height;
                var border = this.BackgroundStyle != null ? this.BackgroundStyle.Border : 0;
                this._ClientSize.Width = this.Width - 2 * border;
                this._ClientSize.Height = this.Height - 2 * border;
                this.OnSizeChanged();
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
            {
                union = Rectangle.Union(union, rects.Dequeue().BoundsLocal);
            }

            this.ClientSize = union;
        }
        public event EventHandler<EventArgs> SizeChanged;
        protected virtual void OnSizeChanged()
        {
            SizeChanged?.Invoke(this, EventArgs.Empty);
        }

        public virtual bool AutoSize { get; set; }

        internal virtual void OnControlAdded(Control control)
        {
            if (this.AutoSize)
            {
                this.ClientSize = this.PreferredClientSize;
                this.ConformToClientSize();
            }
            this.Parent?.OnControlAdded(control);
            this.ControlsChangedAction();
        }

        public void ConformToClientSize()
        {
            if (this.BackgroundStyle != null)
            {
                this.ClientLocation = new Vector2(this.BackgroundStyle.Border);
                this.Width = this.ClientSize.Width + 2 * this.BackgroundStyle.Border;
                this.Height = this.ClientSize.Height + this.BackgroundStyle.Border * 2;
            }
        }
        internal virtual void OnControlRemoved(Control control)
        {
            if (this.AutoSize)
            {
                this.ClientSize = this.PreferredClientSize;
                this.ConformToClientSize();
            }
            this.Parent?.OnControlRemoved(control);
            this.ControlsChangedAction();
        }

        public Vector2 Origin = new Vector2(0);
        public virtual void DrawHighlight(SpriteBatch sb, float alpha = 0.5f)
        {
            sb.Draw(UIManager.Highlight, new Rectangle((int)(this.ScreenLocation.X - this.Origin.X), (int)(this.ScreenLocation.Y - this.Origin.Y), this.BoundsScreen.Width, this.BoundsScreen.Height), null, Color.Lerp(Color.Transparent, Color.White, alpha), 0, Vector2.Zero, SpriteEffects.None, this.Depth);
        }
        public virtual void DrawHighlight(SpriteBatch sb, Rectangle destinationRect, float alpha = 0.5f)
        {
            sb.Draw(UIManager.Highlight, destinationRect, null, Color.Lerp(Color.Transparent, Color.White, alpha), 0, Vector2.Zero, SpriteEffects.None, this.Depth);
        }
        public virtual void DrawHighlight(SpriteBatch sb, Rectangle destinationRect, Color color, float depth = 0)
        {
            sb.Draw(UIManager.Highlight, destinationRect, null, color, 0, Vector2.Zero, SpriteEffects.None, depth);
        }
        public virtual void DrawShade(SpriteBatch sb, float alpha = 0.5f)
        {
            sb.Draw(UIManager.Shade, new Rectangle((int)this.ScreenLocation.X, (int)this.ScreenLocation.Y, this.Width, this.Height), null, Color.Lerp(Color.Transparent, Color.White, alpha), 0, this.Origin, SpriteEffects.None, 0);//Depth);
        }

        public virtual Rectangle PreferredClientSize
        {
            get
            {
                int width = 0, height = 0;
                foreach (Control control in this.Controls)
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
            this.OnBeforeDraw(sb, viewport);
            Color c = this.Tint;
            if (this.Texture != null)
            {
                // TODO: this is no use because if i add something outside the window's client area and the window has autosize, it expands the client area screwing up hittesting
                if (!this.ClipToBounds)
                    sb.Draw(this.Texture, this.BoundsScreen, null, c * this.Opacity, this.Rotation, this.Origin, SpriteEffects.None, 0);
                else
                {
                    this.BoundsScreen.Clip(this.Texture.Bounds, viewport, out Rectangle final, out Rectangle source);
                    sb.Draw(this.Texture, final, source, c * this.Opacity, this.Rotation, this.Origin, SpriteEffects.None, 0);
                    this.OnDrawAction(sb, final);
                }

            }
            this.OnAfterDraw(sb, viewport);

            foreach (Control control in this.Controls)
            {
                control.Draw(sb, Rectangle.Intersect(control.BoundsScreen, viewport));
            }
        }

        public virtual void Draw(SpriteBatch sb)
        {
            // TODO: maybe put that somewhere else?
            if (this.HitTest())
            {
                Controller.Instance.MouseoverBlockNext.Object = this;
            }

            foreach (Control control in this.Controls)
            {
                control.Draw(sb);
            }
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
                Label label = new Label(this.HoverText);
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
                {
                    return this._HoverText;
                }
                else
                {
                    return this.HoverFunc();
                }
            }
            set => this._HoverText = value;
        }

        public Action<Control> TooltipFunc;
        public bool CustomTooltip;
        public virtual void GetTooltipInfo(Control tooltip)
        {
            if (this.TooltipFunc != null)
            {
                this.TooltipFunc(tooltip);
                return;
            }
            if (this.HoverText.Length > 0)
                tooltip.Controls.Add(new Label(this.HoverText, this.HoverFormat) { TextFunc = this.HoverFunc });
        }
        public override void Update()
        {
            this.OnUpdate();

            base.Update();

            List<Control> copy = this.Controls.ToList();
            foreach (Control control in copy)
            {
                control.Update();
            }

            if (!this.Valid)
            {
                this.Validate(); // i put this here after calling base.update because if the size of the control was changed then for one frame it was drawn stretched
            }
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
        public virtual Control AddControlsVertically(int spacing, HorizontalAlignment horAlignment, params Control[] controls)
        {
            var y = 0;
            var maxWidth = controls.Max(c => c.Width);
            foreach (var ctrl in controls)
            {
                switch (horAlignment)
                {
                    case HorizontalAlignment.Left:
                        ctrl.Location = new Vector2(0, y);
                        break;
                    case HorizontalAlignment.Center:
                        ctrl.Location = new Vector2(maxWidth / 2, y);
                        ctrl.Anchor = new(.5f, 0);
                        break;
                    case HorizontalAlignment.Right:
                        ctrl.Location = new Vector2(maxWidth, y);
                        ctrl.Anchor = new(1, 0);
                        break;
                    default:
                        throw new Exception();
                }
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
            {
                this.Controls.Add(ctrl);
            }

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
            {
                this.Controls.Remove(ctrl);
            }
        }
        public virtual void HandleKeyUp(KeyEventArgs e)
        {
            foreach (Control control in this.Controls.ToList())
            {
                control.HandleKeyUp(e);
            }
        }
        public virtual void HandleKeyDown(KeyEventArgs e)
        {
            foreach (Control control in this.Controls.ToList())
            {
                control.HandleKeyDown(e);
            }
        }
        public virtual void HandleKeyPress(KeyPressEventArgs e)
        {
            foreach (Control control in this.Controls.ToList())
            {
                control.HandleKeyPress(e);
            }
        }
        public virtual void HandleMouseMove(HandledMouseEventArgs e, Rectangle viewport)
        {
            if (this.HasChildren)
            {
                List<Control> controls = this.Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                {
                    c.HandleMouseMove(e, viewport);
                }
            }
        }
        public virtual void HandleMouseMove(HandledMouseEventArgs e)
        {
            if (this.HasChildren)
            {
                List<Control> controls = this.Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                    c.HandleMouseMove(e);
            }
        }
        public virtual void HandleLButtonDown(HandledMouseEventArgs e)
        {
            this.MouseLBAction?.Invoke();

            foreach (var c in this.Controls.ToList())
                c.HandleLButtonDown(e);

            if (this.WindowManager.ActiveControl == this)
            {
                this.Select();
                this.OnMouseLeftPress(e);
                return;
            }
        }
        public virtual void HandleLButtonUp(HandledMouseEventArgs e)
        {
            if (this.HasChildren)
            {
                List<Control> controls = this.Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                    c.HandleLButtonUp(e);
            }
            this.OnMouseLeftUp(e);
        }
        public virtual void HandleMiddleUp(HandledMouseEventArgs e)
        {
            if (this.HasChildren)
            {
                List<Control> controls = this.Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                    c.HandleMiddleUp(e);
            }
            this.OnMouseMUp(e);
        }
        public virtual void HandleMiddleDown(HandledMouseEventArgs e)
        {
            if (this.HasChildren)
            {
                List<Control> controls = this.Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                    c.HandleMiddleDown(e);
            }
            this.OnMouseMDown(e);
        }
        public virtual void HandleRButtonDown(HandledMouseEventArgs e)
        {
            this.MouseRBAction?.Invoke();
            foreach (var c in this.Controls.ToList())
                c.HandleRButtonDown(e);

            if (this.WindowManager.ActiveControl == this)
            {
                this.OnMouseRightPress(e);
                return;
            }
        }
        public virtual void HandleRButtonUp(HandledMouseEventArgs e)
        {
            if (this.HasChildren)
            {
                List<Control> controls = this.Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                    c.HandleRButtonUp(e);
            }
            this.OnMouseRightUp(e);
        }
        public virtual void HandleMouseWheel(HandledMouseEventArgs e)
        {
            if (this.HasChildren)
            {
                List<Control> controls = this.Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                    c.HandleMouseWheel(e);
            }
            this.OnMouseWheel(e);
        }
        public virtual void HandleLButtonDoubleClick(HandledMouseEventArgs e)
        {
            if (this.HasChildren)
            {
                List<Control> controls = this.Controls.ToList();
                controls.Reverse();
                foreach (Control c in controls)
                    c.HandleLButtonDoubleClick(e);
            }
            if (this.WindowManager.ActiveControl == this)
            {
                this.Select();
                this.OnLButtonDblClk(e);
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
        public virtual bool Toggle(GuiLayer layer)
        {
            this.Layer = layer;

            if (this.Show(layer))
                return true;
            else
                return !this.Hide();
        }
        public virtual bool ToggleSmart()
        {
            if (!this.IsOpen)
                this.SmartPosition();

            if (this.Show())
                return true;
            else
                return !this.Hide();
        }
        public virtual void BringToFront()
        {
            if (this.Parent == null)
            {
                if (this.WindowManager.Remove(this))
                    this.WindowManager.Add(this);
                return;
            }
            if (this.Parent.Controls.Remove(this))
                this.Parent.Controls.Add(this);

            this.Parent.BringToFront();
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
        public virtual bool Show(GuiLayer layer)
        {
            this.Layer = layer;
            this.OnShow();

            this.ShowAction();
            if (!this.WindowManager.Contains(this))
            {
                this.WindowManager.Add(this);
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
        public bool IsOpen => this.WindowManager.Contains(this);

        public virtual bool Show(UIManager manager)
        {
            if (!manager.Contains(this))
            {
                manager.Add(this);
                return true;
            }

            return false;
        }
        Action _hideAction = () => { };
        public virtual Action HideAction
        { get => this._hideAction; set => this._hideAction = value; }

        public virtual Rectangle Bounds => this.BoundsLocal;
        public virtual Rectangle ContainerSize => this.BoundsLocal;
        public IBounded[] Children => this.Controls.ToArray();

        protected virtual void OnHidden()
        {
            this.HideAction();
            foreach (var ch in this.Controls)
                ch.OnHidden();
        }
        public virtual bool Hide()
        {
            if (this.WindowManager.Remove(this))
            {
                this.OnHidden();
                return true;
            }

            return false;
        }
      
        public virtual bool Remove()
        {
            this.Dispose();
            this.Hide();
            
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
            if (this.HitTest(rectangle))
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
                if (found is not null)
                    return found;
            }
            return null;
        }

        public virtual bool ShowDialog()
        {
            this.Layer = UIManager.LayerDialog;
            this.MoveToScreenCenter();
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
