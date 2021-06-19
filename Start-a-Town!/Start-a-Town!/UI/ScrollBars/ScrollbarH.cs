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

        //public HScrollbar() : base() { }
        //public HScrollbar(Vector2 location) : base(location) { }
        public ScrollbarH(int width) : this(Vector2.Zero, width) { }
        public ScrollbarH(Vector2 location, int width, object tag = null)
            : base(location)
        {
            //Height = height;
            this.BackgroundColor = Color.Black * 0.5f;
            Tag = tag;
            AutoSize = true;
            Height = 16;
            Btn_Left = new PictureBox(Vector2.Zero, UIManager.DefaultHScrollbarSprite, new Rectangle(0, 0, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);// { Rotation = (float)(Math.PI * 3 / 2f) };
            Btn_Right = new PictureBox(new Vector2(width - 16, 0), UIManager.DefaultHScrollbarSprite, new Rectangle(32, 0, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);// { Rotation = (float)(Math.PI * 3 / 2f) };
            Thumb = new PictureBox(new Vector2(16, 0), UIManager.DefaultHScrollbarSprite, new Rectangle(16, 0, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);// { Rotation = (float)(Math.PI * 3 / 2f) };

            Btn_Left.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(Left_Click);
            Btn_Right.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(Right_Click);
            Thumb.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(Thumb_Click);
            //Thumb.DrawMode = UI.DrawMode.OwnerDrawFixed;
            //Thumb.DrawItem += new EventHandler<DrawItemEventArgs>(Thumb_DrawItem);
            Thumb.MouseScroll += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(Thumb_MouseScroll);

            this.Area = new GroupBox() { Size = new Rectangle(0, 0, width - 32, Height), Location = this.Btn_Left.TopRight };
            this.Area.MouseLeftPress += Area_MouseLeftPress;
            Controls.Add(Btn_Left, Btn_Right, this.Area,Thumb);
        }

        void Area_MouseLeftPress(object sender, HandledMouseEventArgs e)
        {
            //float mousey = UIManager.Mouse.Y - this.Area.ScreenClientLocation.Y;
            //float perc = mousey / this.Area.Height;
            Control scr = Tag as Control;
            if (Tag == null)
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


        //protected override void OnMouseUp(InputState e)
        //{
        //    ThumbMoving = false;
        //}

        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            ThumbMoving = false;
        }
        void Thumb_DrawItem(object sender, DrawItemEventArgs e)
        {
            Control scr = Tag as Control;
            if (this.Tag == null)
                return;
            float percentage = scr.Size.Width / (float)scr.ClientSize.Width;
            //Rectangle finalRect = new Rectangle(e.Bounds.X, e.Bounds.Y, 16, (int)((Size.Height - 32) * percentage));
            Rectangle finalRect = new Rectangle(e.Bounds.X, e.Bounds.Y, (int)((Size.Width - 32) * percentage), 16);
            e.SpriteBatch.Draw(Thumb.Texture, finalRect, Thumb.SourceRect, Color.White);
        }

        public override void Update()
        {
            Control scr = Tag as Control;
            //   Location.Y = -Parent.ClientLocation.Y;
            if (scr != null)
            {
                float percentage = scr.Size.Width / (float)scr.ClientSize.Width;
                this.Thumb.Size = new Rectangle(0, 0, (int)((Size.Width - 32) * percentage), 16);
                float pos = -scr.ClientLocation.X / scr.ClientSize.Width;

                Thumb.Width = (int)((Size.Width - 32) * percentage);
                if (ThumbMoving)
                {
                    //Thumb.Location.Y = 16 + Math.Max(0,Math.Min(Size.Height - 32 - Thumb.Height ,Controller.Instance.msCurrent.Y - ThumbOffset)); 




                    //Thumb.Location.Y = 16 + Math.Max(0, Math.Min(Size.Height - 32 - Thumb.Height, (Controller.Instance.msCurrent.Y - ThumbOffset) / UIManager.Scale));
                    Thumb.Location.X = 16 + Math.Max(0, Math.Min(Size.Width - 32 - Thumb.Width, (UIManager.Mouse.X - ThumbOffset)));// / UIManager.Scale));
                    scr.ClientLocation.X = -(scr.ClientSize.Width * (Thumb.Location.X - 16) / (float)(Size.Width - 32));
                }
                else
                {
                    Thumb.Location.X = 16 + ((pos) * (Size.Width - 32));
                }
            }
           base.Update();
        }

        public void Reset()
        {
            Thumb.Location = Vector2.Zero;
        }

        void Thumb_Click(object sender, EventArgs e)
        {
            Control scr = Tag as Control;
            if (Tag == null)
                return;
            ThumbMoving = true;
            //ThumbOffset = Controller.Instance.msCurrent.Y - ((int)ScreenLocation.Y + 16) - (int)Thumb.Location.Y;

            //ThumbOffset = (int)(Controller.Instance.msCurrent.Y - Thumb.Location.Y + 16);
            ThumbOffset = (int)(16 + UIManager.Mouse.X - Thumb.Location.X);
           // ThumbOffset = (int)(UIManager.Mouse.Y - ScreenLocation.Y + 16);
        }

        void Right_Click(object sender, EventArgs e)
        {
            Control scr = Tag as Control;
            if (Tag == null)
                return;

            scr.ClientLocation.X -= Step;
            scr.ClientLocation.X = Math.Max(scr.Size.Width - scr.ClientSize.Width, scr.ClientLocation.X);
          //  Console.WriteLine(scr.ClientLocation.Y);
        }

        void Left_Click(object sender, EventArgs e)
        {
            Control scr = Tag as Control;
            if (Tag == null)
                return;

            scr.ClientLocation.X += Step;
            scr.ClientLocation.X = Math.Min(0, scr.ClientLocation.X);
         //   Console.WriteLine(scr.ClientLocation.Y);
        }

        //public override void HandleInput(InputState input)
        //{
        //    if (input.CurrentMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Released)
        //        if (input.LastMouseState.LeftButton == Microsoft.Xna.Framework.Input.ButtonState.Pressed)
        //            ThumbMoving = false;
        //    base.HandleInput(input);
        //}

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
    }
}
