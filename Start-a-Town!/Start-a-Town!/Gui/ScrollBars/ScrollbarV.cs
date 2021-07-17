using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class ScrollbarV : Control
    {
        static public int Width = 16;
        PictureBox Thumb, Up, Down;
        GroupBox Area;
        float Step = 3*Label.DefaultHeight;
        int ThumbOffset;
        bool ThumbMoving;

        public override int Height
        {
            get
            {
                return base.Height;
            }
            set
            {
                base.Height = value;
                Down.Location.Y = value - 16;
            }
        }

        public ScrollbarV(Vector2 location, int height, object tag = null)
            : base(location)
        {
            this.BackgroundColor = Color.Black * 0.5f;
            Tag = tag;
            AutoSize = true;
            Width = 16;
            Up = new PictureBox(Vector2.Zero, UIManager.DefaultVScrollbarSprite, new Rectangle(0, 0, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);
            Down = new PictureBox(new Vector2(0, height - 16), UIManager.DefaultVScrollbarSprite, new Rectangle(0, 32, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);
            Thumb = new PictureBox(new Vector2(0, 16), UIManager.DefaultVScrollbarSprite, new Rectangle(0, 16, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);

            Up.MouseLeftPress += new EventHandler<HandledMouseEventArgs>(Up_Click);
            Down.MouseLeftPress += new EventHandler<HandledMouseEventArgs>(Down_Click);
            Thumb.MouseLeftPress += new EventHandler<HandledMouseEventArgs>(Thumb_Click);

            this.Area = new GroupBox() { Size = new Rectangle(0, 0, Width, height - 32), Location = this.Up.BottomLeft };
            this.Area.MouseLeftPress += Area_MouseLeftPress;
            Controls.Add(Up, Down, Area, Thumb);
        }

        void Area_MouseLeftPress(object sender, HandledMouseEventArgs e)
        {
            Control scr = Tag as Control;
            if (Tag == null)
                return;

            if (UIManager.Mouse.Y < this.Thumb.ScreenLocation.Y)
            {
                scr.ClientLocation.Y += scr.Size.Height;
                scr.ClientLocation.Y = Math.Min(0, scr.ClientLocation.Y);
                e.Handled = true;
            }
            else if (UIManager.Mouse.Y > this.Thumb.ScreenLocation.Y + this.Thumb.Height)
            {
                scr.ClientLocation.Y -= scr.Size.Height;
                scr.ClientLocation.Y = Math.Max(scr.Size.Height - scr.ClientSize.Height, scr.ClientLocation.Y);
                e.Handled = true;
            }
        }

        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            ThumbMoving = false;
        }
      
        public override void Update()
        {
            Control scr = Tag as Control;
            if (scr != null)
            {
                float percentage = scr.Size.Height / (float)scr.ClientSize.Height;

                Thumb.Size = new Rectangle(0, 0, 16, (int)((Size.Height - 32) * percentage));
                float pos = -scr.ClientLocation.Y / scr.ClientSize.Height;

                Thumb.Height = (int)((Size.Height - 32) * percentage);
                if (ThumbMoving)
                {
                    Thumb.Location.Y = 16 + Math.Max(0, Math.Min(Size.Height - 32 - Thumb.Height, (Controller.Instance.msCurrent.Y - ThumbOffset) / UIManager.Scale));
                    scr.ClientLocation.Y = -(scr.ClientSize.Height * (Thumb.Location.Y - 16) / (float)(Size.Height - 32));
                }
                else
                {
                    Thumb.Location.Y = 16 + ((pos) * (Size.Height - 32));
                }
            }
           base.Update();
        }

        public void Reset()
        {
            Thumb.Location = Vector2.Zero;
        }

        void Thumb_Click(object sender, HandledMouseEventArgs e)
        {
            if (Tag == null)
                return;
            ThumbMoving = true;
            ThumbOffset = (int)(16 + UIManager.Mouse.Y - Thumb.Location.Y);
            e.Handled = true;
        }

        void Down_Click(object sender, HandledMouseEventArgs e)
        {
            Control scr = Tag as Control;
            if (Tag == null)
                return;

            scr.ClientLocation.Y -= Step;
            scr.ClientLocation.Y = Math.Max(scr.Size.Height - scr.ClientSize.Height, scr.ClientLocation.Y);

            e.Handled = true;
        }

        void Up_Click(object sender, HandledMouseEventArgs e)
        {
            Control scr = Tag as Control;
            if (Tag == null)
                return;

            scr.ClientLocation.Y += Step;
            scr.ClientLocation.Y = Math.Min(0, scr.ClientLocation.Y);
            e.Handled = true;
        }

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
    }
}
