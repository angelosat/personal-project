using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using UI;

namespace Start_a_Town_.UI
{
    public class Panel : Control
    {
        public override void OnPaint(SpriteBatch sb)
        {
            Color tint = Color;// Color.Lerp(Color.Transparent, Color, Opacity);
            BackgroundStyle.Draw(sb, this.Size, tint);
        }
        public override Rectangle ContainerSize => this.ClientSize;
        //public override Color Color
        //{
        //    get
        //    {
        //        return Color.Black;
        //    }
        //    set
        //    {
        //        base.Color = value;
        //    }
        //}

        //public override void HandleLButtonDown(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    base.HandleLButtonDown(e);
        //    if(HitTest())
        //    e.Handled = true;
        //}
        public Rectangle GetClientSize()
        {
            return new Rectangle(0, 0, Width - 2 * UIManager.BorderPx, Height - 2 * UIManager.BorderPx);
        }
        public override Vector2 Dimensions
        {
            get
            {
                return base.Dimensions;
            }
            set
            {
                base.Dimensions = value;
                this.ClientSize = new Rectangle(0, 0, Width - 2 * UIManager.BorderPx, Height - 2 * UIManager.BorderPx);
            }
        }

        public Panel(Vector2 location)
            : base(location)
        {
            //Controls = new List<Control>();
            ClientLocation = new Vector2(UIManager.BorderPx);
            BackgroundStyle = DefaultStyle;
            //AutoSize = true;
            Color = Color.Black;
        }
        public Panel(Vector2 location, Vector2 size)
            : base(location, size)
        {
            //Controls = new List<Control>();
            ClientLocation = new Vector2(UIManager.BorderPx);
            ClientSize = new Rectangle(0, 0, Width - 2 * UIManager.BorderPx, Height - 2 * UIManager.BorderPx);
            BackgroundStyle = DefaultStyle;
            //AutoSize = true;
            Color = Color.Black;
        }
        public Panel(Rectangle rect)
            : base(new Vector2(rect.X, rect.Y), new Vector2(rect.Width, rect.Height))
        {
            //Controls = new List<Control>();
            ClientLocation = new Vector2(UIManager.BorderPx);
            ClientSize = new Rectangle(0, 0, Width - 2 * UIManager.BorderPx, Height - 2 * UIManager.BorderPx);
            BackgroundStyle = DefaultStyle;
            //AutoSize = true;
            this.Width = rect.Width;
            this.Height = rect.Height;
            Color = Color.Black;
        }
        public Panel()
        {
            //Controls = new List<Control>();
            ClientLocation = new Vector2(UIManager.BorderPx);
            //BackgroundStyle = BackgroundStyle.Panel;
            BackgroundStyle = DefaultStyle;

            Color = Color.Black;
            //AutoSize = true;
        }
        public Panel(int x, int y, int w, int h) : this(new Rectangle(x, y, w, h)) { }
      
        public static readonly BackgroundStyle DefaultStyle = BackgroundStyle.Window; //Panel
        static public int GetClientWidth(int totalWidth)
        {
            return totalWidth - UIManager.BorderPx - UIManager.BorderPx; 
        }
        static public int GetClientHeight(int totalHeight)
        {
            return totalHeight - UIManager.BorderPx - UIManager.BorderPx;
        }
        //public Vector2 Center
        //{
        //    get { return new Vector2(ClientSize.Width / 2, ClientSize.Height / 2); }
        //}

        //public override void Update()
        //{
        //    base.Update();

        //    foreach (Control control in this.Controls)
        //        control.Update();
        //}

        //public override void Draw(SpriteBatch sb, Rectangle viewport)
        //{
        //    base.Draw(sb);//, viewport);
        //}

        //public override void Draw(SpriteBatch sb)
        //{
        //  //  if (DrawItem != null)
        //   //     OnDrawItem(new DrawItemEventArgs(sb, Bounds));
            
        //        Color tint = Color.Lerp(Color.Transparent, Color, Opacity);//0.75f);
        //        BackgroundStyle.Draw(sb, Bounds, tint);
        //        //if (Name.Length > 0)
        //        //    UIManager.DrawStringOutlined(sb, this.Name, new Vector2(Bounds.X, Bounds.Y));

        //    //sb.Draw(UIManager.containerSprite, new Vector2(X, Y), InnerPadding.TopLeft, tint, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
        //    //sb.Draw(UIManager.containerSprite, new Vector2(X + Width - 19, Y), InnerPadding.TopRight, tint, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
        //    //sb.Draw(UIManager.containerSprite, new Vector2(X, Y + Height - 19), InnerPadding.BottomLeft, tint, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);
        //    //sb.Draw(UIManager.containerSprite, new Vector2(X + Width - 19, Y - 19 + Height), InnerPadding.BottomRight, tint, 0, new Vector2(0, 0), 1, SpriteEffects.None, 0.01f);

        //    ////top, left, right, bottom
        //    //sb.Draw(UIManager.containerSprite, new Rectangle(X + 19, Y, Width - 38, 19), InnerPadding.Top, tint, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        //    //sb.Draw(UIManager.containerSprite, new Rectangle(X, Y + 19, 19, Height - 38), InnerPadding.Left, tint, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);
        //    //sb.Draw(UIManager.containerSprite, new Rectangle(X + Width - 19, Y + 19, 19, Height - 38), InnerPadding.Right, tint, 0, new Vector2(0, 0), SpriteEffects.None, 0.001f);
        //    //sb.Draw(UIManager.containerSprite, new Rectangle(X + 19, Y + Height - 19, Width - 38, 19), InnerPadding.Bottom, tint, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

        //    ////center
        //    //sb.Draw(UIManager.containerSprite, new Rectangle(X + 19, Y + 19, Width - 38, Height - 38), InnerPadding.Center, tint, 0, new Vector2(0, 0), SpriteEffects.None, 0.01f);

        //    //foreach (Control control in Controls)
        //    //    control.Draw(sb);
        //    //    this.ScreenClientRectangle.DrawHighlight(sb);
        //        if (DrawMode == UI.DrawMode.Normal)
        //            base.Draw(sb, this.ScreenClientRectangle);
        //        else
        //            OnDrawItem(new DrawItemEventArgs(sb, Bounds));
        //}

        public static Vector2 GetClientSize(Vector2 boundsSize)
        {
            return boundsSize - 2 * new Vector2(UIManager.BorderPx);
        }
        public static int GetClientLength(int totalLength)
        {
            return totalLength - 2 * UIManager.BorderPx;
        }

        public override Rectangle ClientSize
        {
            get
            {
                return base.ClientSize;
            }
            set
            {
                base.ClientSize = value;

                // does this cause problems?
                //Width = ClientSize.Width + 2 * BackgroundStyle.Border;// UIManager.BorderPx;
                //Height = ClientSize.Height + 2 * BackgroundStyle.Border;//UIManager.BorderPx;
            }
        }
        public Panel SetClientDimensions(Rectangle clientSize)
        {
            this.ClientSize = clientSize;
            this.Width = ClientSize.Width + 2 * BackgroundStyle.Border;// UIManager.BorderPx;
            this.Height = ClientSize.Height + 2 * BackgroundStyle.Border;//UIManager.BorderPx;
            return this;
        }
        public Panel SetClientDimensions(Vector2 clientDimensions)
        {
            this.ClientSize = new Rectangle(0, 0, (int)clientDimensions.X, (int)clientDimensions.Y);
            this.Width = ClientSize.Width + 2 * BackgroundStyle.Border;// UIManager.BorderPx;
            this.Height = ClientSize.Height + 2 * BackgroundStyle.Border;//UIManager.BorderPx;
            return this;
        }
        public Panel SetClientDimensions(int w, int h)
        {
            this.ClientSize = new Rectangle(0, 0, w, h);
            this.Width = ClientSize.Width + 2 * BackgroundStyle.Border;// UIManager.BorderPx;
            this.Height = ClientSize.Height + 2 * BackgroundStyle.Border;//UIManager.BorderPx;
            return this;
        }
    }
}
