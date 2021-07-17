using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class ScrollbarVNew : GroupBox
    {
        static public int DefaultWidth = 16;
        PictureBox Thumb;
        IconButton Up, Down;
        GroupBox Area;
        int ThumbClickOrigin, MaxValue, ThumbRange;
        bool ThumbMoving;
        Action<int> ThumbValueChangeFunc;
        int SmallStep = 1, LargeStep;
        Func<int> ValueFunc;
       
        public ScrollbarVNew(int areaheight, int maxValue, int step, int largeStep, int thumbRange, Func<int> valueFunc, Func<float> percentageFunc, Func<int> positionFunc, Action<int> thumbValueChangeFunc)
        {
            this.ThumbValueChangeFunc = thumbValueChangeFunc;
            this.MaxValue = maxValue;
            this.ThumbRange = thumbRange;
            this.ValueFunc = valueFunc;
            this.BackgroundColor = Color.Black * 0.5f;
            this.SmallStep = step;
            this.LargeStep = largeStep;
            areaheight = Math.Max(areaheight, maxValue + UIManager.Icon16Background.Height);
            var height = areaheight + 2 * UIManager.Icon16Background.Height;
            AutoSize = true;
            Width = DefaultWidth;
            this.Up = new IconButton(Icon.ArrowUp) { BackgroundTexture = UIManager.Icon16Background, LeftClickAction = StepUp };
            Thumb = new PictureBox(Vector2.Zero, UIManager.DefaultVScrollbarSprite, new Rectangle(0, 16, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);
            this.Area = new GroupBox() { MouseThrough = false, AutoSize = false, Size = new Rectangle(0, 0, Width, areaheight), Location = this.Up.BottomLeft };
            this.Area.AddControls(this.Thumb);
            this.Down = new IconButton(Icon.ArrowDown) { BackgroundTexture = UIManager.Icon16Background, Location = this.Area.BottomLeft, LeftClickAction = StepDown };
            this.AddControls(Up, Down, Area);
        }

        void StepUp()
        {
            this.ThumbValueChangeFunc(this.ValueFunc() - SmallStep);
        }
        void StepDown()
        {
            this.ThumbValueChangeFunc(this.ValueFunc() + SmallStep);
        }
        public override void HandleLButtonUp(HandledMouseEventArgs e)
        {
            ThumbMoving = false;
            base.HandleLButtonUp(e);
        }
        public override void HandleLButtonDown(HandledMouseEventArgs e)
        {
            if (e.Handled)
                return;
            if (this.WindowManager.ActiveControl == this.Thumb)
            {
                this.ThumbClickOrigin = (int)(UIManager.Mouse.Y - Thumb.Location.Y);
                this.ThumbMoving = true;
                e.Handled = true;
            }
            else if (this.Area.IsTopMost)
            {
                e.Handled = true;
                if(UIManager.Mouse.Y < this.Thumb.ScreenLocation.Y)
                {
                    this.ThumbValueChangeFunc(this.ValueFunc() - LargeStep);
                }
                else
                {
                    this.ThumbValueChangeFunc(this.ValueFunc() + LargeStep);
                }
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
            float percentage = this.ThumbRange / (float)this.MaxValue;
            this.ResizeThumb(16);
            var currentval = this.ValueFunc();
            float pos = (this.Area.Height - this.Thumb.Height) * currentval / (float)this.MaxValue;
            if (ThumbMoving)
            {
                Thumb.Location.Y = Math.Max(0, Math.Min(Size.Height - 32 - Thumb.Height, (UIManager.Mouse.Y - ThumbClickOrigin) / UIManager.Scale));
                var val = this.MaxValue * (Thumb.Location.Y / (float)(this.Area.Height - this.Thumb.Height));
                this.ThumbValueChangeFunc((int)val);
            }
            else
                Thumb.Location.Y = pos;
            base.Update();
        }
        void ResizeThumb(int height)
        {
            if (this.Thumb.Height == height)
                return;
            var newSize = new Rectangle(0, 0, 16, height);
            this.Thumb.Size = newSize;
            this.Thumb.Invalidate();
        }

        public void Reset()
        {
            Thumb.Location = Vector2.Zero;
        }
        void Thumb_Click(object sender, HandledMouseEventArgs e)
        {
            Control scr = Tag as Control;
            if (Tag == null)
                return;
            ThumbMoving = true;
            ThumbClickOrigin = (int)(16 + UIManager.Mouse.Y - Thumb.Location.Y);
            e.Handled = true;
        }

        void Down_Click(object sender, HandledMouseEventArgs e)
        {
            Control scr = Tag as Control;
            if (Tag == null)
                return;
            scr.ClientLocation.Y -= SmallStep;
            scr.ClientLocation.Y = Math.Max(scr.Size.Height - scr.ClientSize.Height, scr.ClientLocation.Y);
            e.Handled = true;
        }

        void Up_Click(object sender, HandledMouseEventArgs e)
        {
            Control scr = Tag as Control;
            if (Tag == null)
                return;

            scr.ClientLocation.Y += SmallStep;
            scr.ClientLocation.Y = Math.Min(0, scr.ClientLocation.Y);
            e.Handled = true;
        }
    }
}
