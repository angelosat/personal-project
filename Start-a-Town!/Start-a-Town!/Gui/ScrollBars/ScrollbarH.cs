using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class ScrollbarH : Control
    {
        static public int Height = 16;
        PictureBox Thumb, Btn_Right, Btn_Left;
        GroupBox Area;
        float Step = 3*Label.DefaultHeight;
        int ThumbOffset;
        bool ThumbMoving;

        public override int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;
                Btn_Right.Location.X = value - 16;
            }
        }

        public ScrollbarH(Vector2 location, int width, object tag = null)
            : base(location)
        {
            this.BackgroundColor = Color.Black * 0.5f;
            Tag = tag;
            AutoSize = true;
            Height = 16;
            Btn_Left = new PictureBox(Vector2.Zero, UIManager.DefaultHScrollbarSprite, new Rectangle(0, 0, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);
            Btn_Right = new PictureBox(new Vector2(width - 16, 0), UIManager.DefaultHScrollbarSprite, new Rectangle(32, 0, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);
            Thumb = new PictureBox(new Vector2(16, 0), UIManager.DefaultHScrollbarSprite, new Rectangle(16, 0, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);

            Btn_Left.MouseLeftPress += new EventHandler<HandledMouseEventArgs>(Left_Click);
            Btn_Right.MouseLeftPress += new EventHandler<HandledMouseEventArgs>(Right_Click);
            Thumb.MouseLeftPress += new EventHandler<HandledMouseEventArgs>(Thumb_Click);
            Thumb.MouseScroll += new EventHandler<HandledMouseEventArgs>(Thumb_MouseScroll);

            this.Area = new GroupBox() { Size = new Rectangle(0, 0, width - 32, Height), Location = this.Btn_Left.TopRight };
            this.Area.MouseLeftPress += Area_MouseLeftPress;
            Controls.Add(Btn_Left, Btn_Right, this.Area,Thumb);
        }

        void Area_MouseLeftPress(object sender, HandledMouseEventArgs e)
        {
            Control scr = Tag as Control;
            if (Tag is null)
                return;

            if (UIManager.Mouse.X < this.Thumb.ScreenLocation.X)
            {
                scr.ClientLocation.X += scr.Size.Width;
                scr.ClientLocation.X = Math.Min(0, scr.ClientLocation.X);
                e.Handled = true;
            }
            else if (UIManager.Mouse.X > this.Thumb.ScreenLocation.X + this.Thumb.Width)
            {
                scr.ClientLocation.X -= scr.Size.Width;
                scr.ClientLocation.X = Math.Max(scr.Size.Width - scr.ClientSize.Width, scr.ClientLocation.X);
                e.Handled = true;
            }
        }

        public override void HandleLButtonUp(HandledMouseEventArgs e)
        {
            ThumbMoving = false;
        }

        public override void Update()
        {
            Control scr = Tag as Control;
            if (scr is not null)
            {
                float percentage = scr.Size.Width / (float)scr.ClientSize.Width;
                this.Thumb.Size = new Rectangle(0, 0, (int)((Size.Width - 32) * percentage), 16);
                float pos = -scr.ClientLocation.X / scr.ClientSize.Width;

                Thumb.Width = (int)((Size.Width - 32) * percentage);
                if (ThumbMoving)
                {
                    Thumb.Location.X = 16 + Math.Max(0, Math.Min(Size.Width - 32 - Thumb.Width, (UIManager.Mouse.X - ThumbOffset)));
                    scr.ClientLocation.X = -(scr.ClientSize.Width * (Thumb.Location.X - 16) / (float)(Size.Width - 32));
                }
                else
                {
                    Thumb.Location.X = 16 + ((pos) * (Size.Width - 32));
                }
            }
           base.Update();
        }

        void Thumb_Click(object sender, EventArgs e)
        {
            if (this.Tag is not null)
            {
                ThumbMoving = true;
                ThumbOffset = (int)(16 + UIManager.Mouse.X - Thumb.Location.X);
            }
        }

        void Right_Click(object sender, EventArgs e)
        {
            Control scr = Tag as Control;
            if (Tag is null)
                return;

            scr.ClientLocation.X -= Step;
            scr.ClientLocation.X = Math.Max(scr.Size.Width - scr.ClientSize.Width, scr.ClientLocation.X);
        }

        void Left_Click(object sender, EventArgs e)
        {
            Control scr = Tag as Control;
            if (Tag is null)
                return;

            scr.ClientLocation.X += Step;
            scr.ClientLocation.X = Math.Min(0, scr.ClientLocation.X);
        }

        void Thumb_MouseScroll(object sender, HandledMouseEventArgs e)
        {
        }
    }
}
