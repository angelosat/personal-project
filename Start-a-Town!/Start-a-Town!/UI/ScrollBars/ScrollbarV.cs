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
    class ScrollbarV : Control
    {
        static public int Width = 16;
        PictureBox Thumb, Up, Down;
        //IconButton Up, Down;
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

       

        public ScrollbarV() : base() { }
        public ScrollbarV(Vector2 location) : base(location) { }
        public ScrollbarV(Vector2 location, int height, object tag = null)
            : base(location)
        {
            //Height = height;
            this.BackgroundColor = Color.Black * 0.5f;
            Tag = tag;
            AutoSize = true;
            Width = 16;
            Up = new PictureBox(Vector2.Zero, UIManager.DefaultVScrollbarSprite, new Rectangle(0, 0, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);
            Down = new PictureBox(new Vector2(0, height - 16), UIManager.DefaultVScrollbarSprite, new Rectangle(0, 32, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);
            Thumb = new PictureBox(new Vector2(0, 16), UIManager.DefaultVScrollbarSprite, new Rectangle(0, 16, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);

            //this.Up = new IconButton() { BackgroundTexture = UIManager.Icon16Background, Icon = Icon.ArrowUp };
            //this.Down = new IconButton() { BackgroundTexture = UIManager.Icon16Background, Icon = Icon.ArrowDown };
            //Thumb = new PictureBox(Vector2.Zero, UIManager.DefaultVScrollbarSprite, new Rectangle(0, 16, 16, 16), HorizontalAlignment.Left, VerticalAlignment.Top);

            Up.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(Up_Click);
            Down.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(Down_Click);
            Thumb.MouseLeftPress += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(Thumb_Click);
            //Thumb.DrawMode = UI.DrawMode.OwnerDrawFixed;
            //Thumb.DrawItem += new EventHandler<DrawItemEventArgs>(Thumb_DrawItem);
            Thumb.MouseScroll += new EventHandler<System.Windows.Forms.HandledMouseEventArgs>(Thumb_MouseScroll);

            this.Area = new GroupBox() { Size = new Rectangle(0, 0, Width, height - 32), Location = this.Up.BottomLeft };
            this.Area.MouseLeftPress += Area_MouseLeftPress;
            Controls.Add(Up, Down, Area, Thumb);
        }

        void Area_MouseLeftPress(object sender, HandledMouseEventArgs e)
        {
            //float mousey = UIManager.Mouse.Y - this.Area.ScreenClientLocation.Y;
            //float perc = mousey / this.Area.Height;
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


        //public override void HandleLButtonDown(HandledMouseEventArgs e)
        //{
        //    base.HandleLButtonDown(e);
        //}

        public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            ThumbMoving = false;
        }
        //void Thumb_DrawItem(object sender, DrawItemEventArgs e)
        //{
        //    Control scr = Tag as Control;
        //    if (Tag == null)
        //        return;
        //    float percentage = scr.Size.Height / (float)scr.ClientSize.Height;
        //    Rectangle finalRect = new Rectangle(e.Bounds.X, e.Bounds.Y, 16, (int)((Size.Height - 32) * percentage));
        //    e.SpriteBatch.Draw(Thumb.Texture, finalRect, Thumb.SourceRect, Color.White);
        //    //e.SpriteBatch.Draw(Thumb.Texture, new Rectangle(e.Bounds.X, e.Bounds.Y, , Thumb.SourceRect, Color.White);
        //}

        public override void Update()
        {
            Control scr = Tag as Control;
            //   Location.Y = -Parent.ClientLocation.Y;
            if (scr != null)
            {
                float percentage = scr.Size.Height / (float)scr.ClientSize.Height;

                Thumb.Size = new Rectangle(0, 0, 16, (int)((Size.Height - 32) * percentage));
                float pos = -scr.ClientLocation.Y / scr.ClientSize.Height;

                Thumb.Height = (int)((Size.Height - 32) * percentage);
                if (ThumbMoving)
                {
                    //Thumb.Location.Y = 16 + Math.Max(0,Math.Min(Size.Height - 32 - Thumb.Height ,Controller.Instance.msCurrent.Y - ThumbOffset)); 




                    Thumb.Location.Y = 16 + Math.Max(0, Math.Min(Size.Height - 32 - Thumb.Height, (Controller.Instance.msCurrent.Y - ThumbOffset) / UIManager.Scale));
                    //Thumb.Location.Y = 16 + Math.Max(0, Math.Min(Size.Height - 32 - Thumb.Height, (UIManager.Mouse.Y - ThumbOffset) ));// / UIManager.Scale));
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
            Control scr = Tag as Control;
            if (Tag == null)
                return;
            ThumbMoving = true;
            //ThumbOffset = Controller.Instance.msCurrent.Y - ((int)ScreenLocation.Y + 16) - (int)Thumb.Location.Y;

            //ThumbOffset = (int)(Controller.Instance.msCurrent.Y - Thumb.Location.Y + 16);
            ThumbOffset = (int)(16 + UIManager.Mouse.Y - Thumb.Location.Y);
           // ThumbOffset = (int)(UIManager.Mouse.Y - ScreenLocation.Y + 16);
            e.Handled = true;
        }

        void Down_Click(object sender, HandledMouseEventArgs e)
        {
            Control scr = Tag as Control;
            if (Tag == null)
                return;

            scr.ClientLocation.Y -= Step;
            scr.ClientLocation.Y = Math.Max(scr.Size.Height - scr.ClientSize.Height, scr.ClientLocation.Y);

            //  Console.WriteLine(scr.ClientLocation.Y);
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

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
    }
}
