using Microsoft.Xna.Framework;
using System;
using System.Windows.Forms;

namespace Start_a_Town_.UI
{
    public class ScrollableBoxTest : GroupBox
    {
        private const int buttonSize = 16;
        readonly ScrollbarV VScroll;
        readonly ScrollbarH HScroll;
        public GroupBox Client;
        readonly GroupBox Container;
        public override Rectangle ClientSize => this.Client.ClientSize;
        readonly ScrollModes Mode;
        public override Control SetOpacity(float value, bool children, params Control[] exclude)
        {
            base.SetOpacity(value, children, exclude);
            this.VScroll.SetOpacity(value, true);
            this.HScroll.SetOpacity(value, true);
            return this;
        }
        public int SmallStep
        {
            set => this.VScroll.SmallStep = value;
        }
        public ScrollableBoxTest(GroupBox container, int width, int height, ScrollModes mode = ScrollModes.Both)
            : base(width, height)
        {
            this.Container = container;
            this.Mode = mode;
            var modeFactor = new IntVec2((mode & ScrollModes.Vertical) == ScrollModes.Vertical ? 1 : 0, (mode & ScrollModes.Horizontal) == ScrollModes.Horizontal ? 1 : 0);
            this.Client = new GroupBox(width - buttonSize * modeFactor.X, height - buttonSize * modeFactor.Y) { AutoSize = false };
            this.Client.AddControls(this.Container);
            this.VScroll = new ScrollbarV(this.Client, this.Container) { Location = new Vector2(this.Client.Width, 0) };//, this.Client.Height, this.Client);
            this.HScroll = new ScrollbarH(this.Client, this.Container) { Location = new Vector2(0, this.Client.Height) };//, this.Client.Height, this.Client);
            this.Controls.Add(this.Client);
            this.UpdateScrollbars();
        }

        internal override void OnControlResized(Control control)
        {
            base.OnControlResized(control);
            if (control == this.Client)
            {
                this.UpdateScrollbars();
                this.EnsureClientWithinBounds();
            }
        }

        protected virtual void UpdateScrollbars()
        {
            var containerSize = this.Container.Size;
            var containerW = containerSize.Width;
            if ((this.Mode & ScrollModes.Horizontal) == ScrollModes.Horizontal && this.Client.Width < containerW)
            {
                if (!this.Controls.Contains(this.HScroll))
                    this.Controls.Add(this.HScroll);
            }
            else
                this.Controls.Remove(this.HScroll);

            var containerH = containerSize.Height;
            if ((this.Mode & ScrollModes.Vertical) == ScrollModes.Vertical && this.Client.Height < containerH)
            {
                if (!this.Controls.Contains(this.VScroll))
                    this.Controls.Add(this.VScroll);
            }
            else
                this.Controls.Remove(this.VScroll);
            this.HScroll.Width = this.Client.Width;
            this.VScroll.Height = this.Client.Height;
        }
        public override void HandleMouseWheel(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.HitTest()) // why hittest again? i hittest during update. just check if has focus
                return;
            e.Handled = true;
            if (this.Container.Height <= this.Client.Height)
                /// if nothing to scroll, dont move the client container. added this after chat lines where moved from the bottom to the top when turning the mousewheel once,
                /// even when their height was smaller than the chat window
                return;
            int step = this.VScroll.SmallStep;
            this.Container.Location.Y = Math.Min(0, Math.Max(this.Client.Height - this.Container.Height, this.Container.Location.Y + step * e.Delta));
        }

        protected void EnsureClientWithinBounds()
        {
            this.Container.Location.Y = Math.Max(this.Container.Location.Y, Math.Min(0, this.Client.Size.Height - this.Container.Height));
        }
        
        class ScrollbarV : GroupBox
        {
            public const int DefaultWidth = 16;
            readonly PictureBox Thumb;
            readonly IconButton Up, Down;
            readonly GroupBox Area;
            private int ThumbClickOrigin;
            bool ThumbMoving;
            public int SmallStep = Label.DefaultHeight;// 1;
            GroupBox Container, Client;

            public ScrollbarV(GroupBox client, GroupBox container)
            {
                this.Container = container;
                this.Client = client;
                this.BackgroundColor = Color.Black * 0.5f;
                this.AutoSize = true;
                this.Width = DefaultWidth;
                var areaheight = client.Height - 2 * DefaultWidth;
                this.Up = new IconButton(Icon.ArrowUp) { BackgroundTexture = UIManager.Icon16Background, LeftClickAction = StepUp };
                this.Thumb = new PictureBox(Vector2.Zero, UIManager.DefaultVScrollbarSprite, new Rectangle(0, 16, 16, 16), Alignment.Horizontal.Left, Alignment.Vertical.Top);
                this.Area = new GroupBox() { MouseThrough = false, AutoSize = false, Size = new Rectangle(0, 0, this.Width, areaheight), Location = this.Up.BottomLeft };
                this.Area.AddControls(this.Thumb);
                this.Down = new IconButton(Icon.ArrowDown) { BackgroundTexture = UIManager.Icon16Background, Location = this.Area.BottomLeft, LeftClickAction = StepDown };
                this.AddControls(this.Up, this.Down, this.Area);
            }
            
            void StepUp()
            {
                //this.Container.Location.Y = Math.Min(0, this.Container.Location.Y + this.SmallStep);
                this.MoveContainer(this.Container.Location.Y + this.SmallStep);
            }
            void StepDown()
            {
                //this.Container.Location.Y = Math.Max(this.Client.Height - this.Container.Height, this.Container.Location.Y - this.SmallStep);
                this.MoveContainer(this.Container.Location.Y - this.SmallStep);
            }
            void MoveContainer(float newPos)
            {
                this.Container.Location.Y = Math.Min(0, Math.Max(this.Client.Height - this.Container.Height, newPos));
            }
            public override void HandleLButtonUp(HandledMouseEventArgs e)
            {
                this.ThumbMoving = false;
                base.HandleLButtonUp(e);
            }
            public override void HandleLButtonDown(HandledMouseEventArgs e)
            {
                if (e.Handled)
                    return;
                if (this.WindowManager.ActiveControl == this.Thumb)
                {
                    this.ThumbClickOrigin = (int)(UIManager.Mouse.Y - this.Thumb.Location.Y);
                    this.ThumbMoving = true;
                    e.Handled = true;
                }
                else if (this.Area.IsTopMost)
                {
                    e.Handled = true;
                    if (UIManager.Mouse.Y < this.Thumb.ScreenLocation.Y)
                        //this.Container.Location.Y -= this.LargeStep;
                        this.MoveContainer(this.Container.Location.Y + this.Client.Height);
                    else
                        //this.Container.Location.Y += this.LargeStep;
                        this.MoveContainer(this.Container.Location.Y - this.Client.Height);
                }
                else
                    base.HandleLButtonDown(e);
            }
            public override void HandleLButtonDoubleClick(HandledMouseEventArgs e)
            {
                this.HandleLButtonDown(e);
            }
            public override void Update()
            {
                float max = this.Container.Height - this.Client.Height;
                this.ResizeThumb();
                var thumbH = this.Thumb.Height;
                if (this.ThumbMoving)
                {
                    this.Thumb.Location.Y = Math.Max(0, Math.Min(this.Size.Height - 32 - thumbH, (UIManager.Mouse.Y - this.ThumbClickOrigin) / UIManager.Scale));
                    var val = max * (this.Thumb.Location.Y / (this.Area.Height - thumbH));
                    this.Container.Location.Y = -val;
                }
                else
                {
                    var currentval = Math.Min(0, Math.Max(this.Client.Height - this.Container.Height, this.Container.Location.Y));
                    float pos = (this.Area.Height - thumbH) * currentval / max;
                    this.Thumb.Location.Y = -pos;
                }
                base.Update();
            }
            
            void ResizeThumb()
            {
                float percentage = this.Client.Height / (float)this.Container.Height;
                var height = (int)(this.Area.Height * percentage);
                if (this.Thumb.Height == height)
                    return;
                var newSize = new Rectangle(0, 0, DefaultWidth, height);
                this.Thumb.Size = newSize;
                this.Thumb.Invalidate();
            }

            public void Reset()
            {
                this.Thumb.Location = Vector2.Zero;
            }
        }

        class ScrollbarH : GroupBox
        {
            public const int DefaultHeight = 16;
            readonly PictureBox Thumb;
            readonly IconButton BtnLeft, BtnRight;
            readonly GroupBox Area;
            private int ThumbClickOrigin;
            bool ThumbMoving;
            public int SmallStep = Label.DefaultHeight;// 1;
            readonly GroupBox Container, Client;

            public ScrollbarH(GroupBox client, GroupBox container)
            {
                this.Container = container;
                this.Client = client;
                this.BackgroundColor = Color.Black * 0.5f;
                this.AutoSize = true;
                this.Height = DefaultHeight;
                var arealength = client.Width - 2 * DefaultHeight;
                this.BtnLeft = new IconButton(Icon.ArrowUp) { BackgroundTexture = UIManager.Icon16Background, LeftClickAction = StepUp };
                this.Thumb = new PictureBox(Vector2.Zero, UIManager.DefaultVScrollbarSprite, new Rectangle(0, 16, 16, 16), Alignment.Horizontal.Left, Alignment.Vertical.Top);
                this.Area = new GroupBox() { MouseThrough = false, AutoSize = false, Size = new Rectangle(0, 0, arealength, this.Height), Location = this.BtnLeft.TopRight };
                this.Area.AddControls(this.Thumb);
                this.BtnRight = new IconButton(Icon.ArrowDown) { BackgroundTexture = UIManager.Icon16Background, Location = this.Area.TopRight, LeftClickAction = StepDown };
                this.AddControls(this.BtnLeft, this.BtnRight, this.Area);
            }

            void StepUp()
            {
                this.MoveContainer(this.Container.Location.X + this.SmallStep);
            }
            void StepDown()
            {
                this.MoveContainer(this.Container.Location.X - this.SmallStep);
            }
            void MoveContainer(float newPos)
            {
                this.Container.Location.X = Math.Min(0, Math.Max(this.Client.Width - this.Container.Width, newPos));
            }
            public override void HandleLButtonUp(HandledMouseEventArgs e)
            {
                this.ThumbMoving = false;
                base.HandleLButtonUp(e);
            }
            public override void HandleLButtonDown(HandledMouseEventArgs e)
            {
                if (e.Handled)
                    return;
                if (this.WindowManager.ActiveControl == this.Thumb)
                {
                    this.ThumbClickOrigin = (int)(UIManager.Mouse.X - this.Thumb.Location.X);
                    this.ThumbMoving = true;
                    e.Handled = true;
                }
                else if (this.Area.IsTopMost)
                {
                    e.Handled = true;
                    if (UIManager.Mouse.X < this.Thumb.ScreenLocation.X)
                        this.MoveContainer(this.Container.Location.X + this.Client.Width);
                    else
                        this.MoveContainer(this.Container.Location.X - this.Client.Width);
                }
                else
                    base.HandleLButtonDown(e);
            }
            public override void HandleLButtonDoubleClick(HandledMouseEventArgs e)
            {
                this.HandleLButtonDown(e);
            }
            public override void Update()
            {
                float max = this.Container.Height - this.Client.Height;
                this.ResizeThumb();
                var thumbH = this.Thumb.Width;
                if (this.ThumbMoving)
                {
                    this.Thumb.Location.X = Math.Max(0, Math.Min(this.Size.Width - 32 - thumbH, (UIManager.Mouse.X - this.ThumbClickOrigin) / UIManager.Scale));
                    var val = max * (this.Thumb.Location.X / (this.Area.Width - thumbH));
                    this.Container.Location.X = -val;
                }
                else
                {
                    var currentval = Math.Min(0, Math.Max(this.Client.Width - this.Container.Width, this.Container.Location.X));
                    float pos = (this.Area.Width - thumbH) * currentval / max;
                    this.Thumb.Location.X = -pos;
                }
                base.Update();
            }

            void ResizeThumb()
            {
                float percentage = this.Client.Height / (float)this.Container.Height;
                var w = (int)(this.Area.Width * percentage);
                if (this.Thumb.Width == w)
                    return;
                var newSize = new Rectangle(0, 0, DefaultHeight, w);
                this.Thumb.Size = newSize;
                this.Thumb.Invalidate();
            }

            public void Reset()
            {
                this.Thumb.Location = Vector2.Zero;
            }
        }
    }
}
