using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

namespace Start_a_Town_.UI
{
    class ScrollbarVNew : GroupBox
    {
        static public int DefaultWidth = 16;
        PictureBox Thumb;//, Up, Down;
        IconButton Up, Down;
        GroupBox Area;
        //float Step = 3*Label.DefaultHeight;
        int ThumbClickOrigin, MaxValue, ThumbRange;
        bool ThumbMoving;
        Func<float> ThumbSizePercentageFunc;
        Func<int> ThumbPositionFunc;
        Action<int> ThumbValueChangeFunc;
        int SmallStep = 1, LargeStep;
        Func<int> ValueFunc;
        //public override int Height
        //{
        //    get
        //    {
        //        return base.Height;
        //    }
        //    set
        //    {
        //        base.Height = value;
        //        Down.Location.Y = value - 16;
        //    }
        //}

        public ScrollbarVNew(int areaheight, int maxValue, int step, int largeStep, int thumbRange, Func<int> valueFunc, Func<float> percentageFunc, Func<int> positionFunc, Action<int> thumbValueChangeFunc)
        {
            this.ThumbSizePercentageFunc = percentageFunc;
            this.ThumbPositionFunc = positionFunc;
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
            Width = DefaultWidth;// 16;
            //Up = new PictureBox(Vector2.Zero, UIManager.DefaultVScrollbarSprite, new Rectangle(0, 0, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);
            //Down = new PictureBox(new Vector2(0, height - 16), UIManager.DefaultVScrollbarSprite, new Rectangle(0, 32, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);
            this.Up = new IconButton(Icon.ArrowUp) { BackgroundTexture = UIManager.Icon16Background, LeftClickAction = StepUp };

            //Thumb = new PictureBox(new Vector2(0, 16), UIManager.DefaultVScrollbarSprite, new Rectangle(0, 16, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);
            Thumb = new PictureBox(Vector2.Zero, UIManager.DefaultVScrollbarSprite, new Rectangle(0, 16, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top)
            {
                //LeftDownAction = ThumbClick,
                //LeftUpAction = ThumbClick
            };

            //Up.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(Up_Click);
            //Down.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(Down_Click);
            //Thumb.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(Thumb_Click);
            //Thumb.MouseScroll += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(Thumb_MouseScroll);

            this.Area = new GroupBox() { MouseThrough = false, AutoSize = false, Size = new Rectangle(0, 0, Width, areaheight), Location = this.Up.BottomLeft };
            this.Area.AddControls(this.Thumb);
            this.Down = new IconButton(Icon.ArrowDown) { BackgroundTexture = UIManager.Icon16Background, Location = this.Area.BottomLeft, LeftClickAction = StepDown };

            //this.Area.MouseLeftPress += Area_MouseLeftPress;
            this.AddControls(Up, Down, Area);
        }

        void StepUp()
        {
            //this.Value = Math.Max(0, this.Value - this.Step);
            this.ThumbValueChangeFunc(this.ValueFunc() - SmallStep);

        }
        void StepDown()
        {
            //this.Value = Math.Min(Value + Step, MaxValue - ThumbRange);
            this.ThumbValueChangeFunc(this.ValueFunc() + SmallStep);
        }

        //void Area_MouseLeftPress(object sender, HandledMouseEventArgs e)
        //{
        //    //float mousey = UIManager.Mouse.Y - this.Area.ScreenClientLocation.Y;
        //    //float perc = mousey / this.Area.Height;
        //    Control scr = Tag as Control;
        //    if (Tag == null)
        //        return;

        //    if (UIManager.Mouse.Y < this.Thumb.ScreenLocation.Y)
        //    {
        //        scr.ClientLocation.Y += scr.Size.Height;
        //        scr.ClientLocation.Y = Math.Min(0, scr.ClientLocation.Y);
        //        e.Handled = true;
        //    }
        //    else if (UIManager.Mouse.Y > this.Thumb.ScreenLocation.Y + this.Thumb.Height)
        //    {
        //        scr.ClientLocation.Y -= scr.Size.Height;
        //        scr.ClientLocation.Y = Math.Max(scr.Size.Height - scr.ClientSize.Height, scr.ClientLocation.Y);
        //        e.Handled = true;
        //    }
        //}

        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
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
            //if (this.WindowManager.ActiveControl!=null)
            //this.WindowManager.ActiveControl.ToConsole();
            float percentage = this.ThumbRange / (float)this.MaxValue;// this.ThumbSizePercentageFunc();// scr.Size.Height / (float)scr.ClientSize.Height;

            //Thumb.Size = new Rectangle(0, 0, 16, 16);// (int)((Size.Height - 32) * percentage));
            this.ResizeThumb(16);
            var currentval = this.ValueFunc();
            float pos = (this.Area.Height - this.Thumb.Height) * currentval / (float)this.MaxValue;// this.ThumbPositionFunc();// -scr.ClientLocation.Y / scr.ClientSize.Height;

            //Thumb.Height = (int)((Size.Height - 32) * percentage);
            if (ThumbMoving)
            {
                //Thumb.Location.Y = 16 + Math.Max(0, Math.Min(Size.Height - 32 - Thumb.Height, (Controller.Instance.msCurrent.Y - ThumbOffset) / UIManager.Scale));
                Thumb.Location.Y = Math.Max(0, Math.Min(Size.Height - 32 - Thumb.Height, (UIManager.Mouse.Y - ThumbClickOrigin) / UIManager.Scale));
                var val = this.MaxValue * (Thumb.Location.Y / (float)(this.Area.Height - this.Thumb.Height));
                this.ThumbValueChangeFunc((int)val);
            }
            else
                Thumb.Location.Y = pos;
                //Thumb.Location.Y = 16 + ((pos) * (Size.Height - 32));
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
        //void ThumbClick()
        //{
        //    this.ThumbMoving = !this.ThumbMoving;
        //}
        void Thumb_Click(object sender, HandledMouseEventArgs e)
        {
            Control scr = Tag as Control;
            if (Tag == null)
                return;
            ThumbMoving = true;
            //ThumbOffset = Controller.Instance.msCurrent.Y - ((int)ScreenLocation.Y + 16) - (int)Thumb.Location.Y;

            //ThumbOffset = (int)(Controller.Instance.msCurrent.Y - Thumb.Location.Y + 16);
            ThumbClickOrigin = (int)(16 + UIManager.Mouse.Y - Thumb.Location.Y);
           // ThumbOffset = (int)(UIManager.Mouse.Y - ScreenLocation.Y + 16);
            e.Handled = true;
        }

        void Down_Click(object sender, HandledMouseEventArgs e)
        {
            Control scr = Tag as Control;
            if (Tag == null)
                return;

            scr.ClientLocation.Y -= SmallStep;
            scr.ClientLocation.Y = Math.Max(scr.Size.Height - scr.ClientSize.Height, scr.ClientLocation.Y);

            //  Console.WriteLine(scr.ClientLocation.Y);
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
         //   Console.WriteLine(scr.ClientLocation.Y);
        }

        protected override void OnMouseScroll(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //if (e.CurrentMouseState.ScrollWheelValue > e.LastMouseState.ScrollWheelValue)
            //    Up_Click(this, EventArgs.Empty);
            //else
            //    Down_Click(this, EventArgs.Empty);
            e.Handled = true;
        }

        void Thumb_MouseScroll(object sender, System.Windows.Forms.HandledMouseEventArgs e)
        {
            //OnMouseScroll(e);
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
        
    }
}
