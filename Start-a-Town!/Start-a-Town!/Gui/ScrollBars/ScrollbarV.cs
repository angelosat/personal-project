﻿using Microsoft.Xna.Framework;
using System;
using System.Windows.Forms;

namespace Start_a_Town_.UI
{
    class ScrollbarV : Control
    {
        public const int DefaultWidth = 16;
        readonly PictureBox Thumb, Up, Down;
        readonly GroupBox Area;
        readonly float Step = 3 * Label.DefaultHeight;
        int ThumbOffset;
        bool ThumbMoving;

        public override int Height
        {
            get => base.Height;
            set
            {
                base.Height = value;
                this.Down.Location.Y = value - 16;
            }
        }

        public ScrollbarV(Vector2 location, int height, object tag = null)
            : base(location)
        {
            this.BackgroundColor = Color.Black * 0.5f;
            this.Tag = tag;
            this.AutoSize = true;
            this.Up = new PictureBox(Vector2.Zero, UIManager.DefaultVScrollbarSprite, new Rectangle(0, 0, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);
            this.Down = new PictureBox(new Vector2(0, height - 16), UIManager.DefaultVScrollbarSprite, new Rectangle(0, 32, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);
            this.Thumb = new PictureBox(new Vector2(0, 16), UIManager.DefaultVScrollbarSprite, new Rectangle(0, 16, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);

            this.Up.MouseLeftPress += new EventHandler<HandledMouseEventArgs>(this.Up_Click);
            this.Down.MouseLeftPress += new EventHandler<HandledMouseEventArgs>(this.Down_Click);
            this.Thumb.MouseLeftPress += new EventHandler<HandledMouseEventArgs>(this.Thumb_Click);

            this.Area = new GroupBox() { Size = new Rectangle(0, 0, DefaultWidth, height - 32), Location = this.Up.BottomLeft };
            this.Area.MouseLeftPress += this.Area_MouseLeftPress;
            this.Controls.Add(this.Up, this.Down, this.Area, this.Thumb);
        }

        void Area_MouseLeftPress(object sender, HandledMouseEventArgs e)
        {
            Control scr = this.Tag as Control;
            if (this.Tag == null)
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

        public override void HandleLButtonUp(HandledMouseEventArgs e)
        {
            this.ThumbMoving = false;
        }

        public override void Update()
        {
            if (this.Tag is Control scr)
            {
                var w = DefaultWidth;
                var ww = w + w;

                float percentage = scr.Size.Height / (float)scr.ClientSize.Height;

                this.Thumb.Size = new Rectangle(0, 0, w, (int)((this.Size.Height - ww) * percentage));
                float pos = -scr.ClientLocation.Y / scr.ClientSize.Height;

                this.Thumb.Height = (int)((this.Size.Height - ww) * percentage);
                if (this.ThumbMoving)
                {
                    this.Thumb.Location.Y = w + Math.Max(0, Math.Min(this.Size.Height - ww - this.Thumb.Height, (Controller.Instance.msCurrent.Y - this.ThumbOffset) / UIManager.Scale));
                    scr.ClientLocation.Y = -(scr.ClientSize.Height * (this.Thumb.Location.Y - w) / (this.Size.Height - ww));
                }
                else
                {
                    this.Thumb.Location.Y = w + ((pos) * (this.Size.Height - ww));
                }
            }
            base.Update();
        }

        public void Reset()
        {
            this.Thumb.Location = Vector2.Zero;
        }

        void Thumb_Click(object sender, HandledMouseEventArgs e)
        {
            if (this.Tag is null)
                return;
            this.ThumbMoving = true;
            this.ThumbOffset = (int)(16 + UIManager.Mouse.Y - this.Thumb.Location.Y);
            e.Handled = true;
        }

        void Down_Click(object sender, HandledMouseEventArgs e)
        {
            if (this.Tag is not Control scr)
                return;

            scr.ClientLocation.Y -= this.Step;
            scr.ClientLocation.Y = Math.Max(scr.Size.Height - scr.ClientSize.Height, scr.ClientLocation.Y);

            e.Handled = true;
        }

        void Up_Click(object sender, HandledMouseEventArgs e)
        {
            if (this.Tag is not Control scr)
                return;

            scr.ClientLocation.Y += this.Step;
            scr.ClientLocation.Y = Math.Min(0, scr.ClientLocation.Y);
            e.Handled = true;
        }
    }
}
