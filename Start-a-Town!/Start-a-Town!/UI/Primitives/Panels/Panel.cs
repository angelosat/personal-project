using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class Panel : Control
    {
        public override void OnPaint(SpriteBatch sb)
        {
            Color tint = Color;
            BackgroundStyle.Draw(sb, this.Size, tint);
        }
        public override Rectangle ContainerSize => this.ClientSize;
       
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
            ClientLocation = new Vector2(UIManager.BorderPx);
            BackgroundStyle = DefaultStyle;
            Color = Color.Black;
        }
        public Panel(Vector2 location, Vector2 size)
            : base(location, size)
        {
            ClientLocation = new Vector2(UIManager.BorderPx);
            ClientSize = new Rectangle(0, 0, Width - 2 * UIManager.BorderPx, Height - 2 * UIManager.BorderPx);
            BackgroundStyle = DefaultStyle;
            Color = Color.Black;
        }
        public Panel(Rectangle rect)
            : base(new Vector2(rect.X, rect.Y), new Vector2(rect.Width, rect.Height))
        {
            ClientLocation = new Vector2(UIManager.BorderPx);
            ClientSize = new Rectangle(0, 0, Width - 2 * UIManager.BorderPx, Height - 2 * UIManager.BorderPx);
            BackgroundStyle = DefaultStyle;
            this.Width = rect.Width;
            this.Height = rect.Height;
            Color = Color.Black;
        }
        public Panel()
        {
            ClientLocation = new Vector2(UIManager.BorderPx);
            BackgroundStyle = DefaultStyle;
            Color = Color.Black;
        }
        public Panel(int x, int y, int w, int h) : this(new Rectangle(x, y, w, h)) { }
      
        public static readonly BackgroundStyle DefaultStyle = BackgroundStyle.Window;
        static public int GetClientWidth(int totalWidth)
        {
            return totalWidth - UIManager.BorderPx - UIManager.BorderPx; 
        }
        static public int GetClientHeight(int totalHeight)
        {
            return totalHeight - UIManager.BorderPx - UIManager.BorderPx;
        }

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
            }
        }
        public Panel SetClientDimensions(Rectangle clientSize)
        {
            this.ClientSize = clientSize;
            this.Width = ClientSize.Width + 2 * BackgroundStyle.Border;
            this.Height = ClientSize.Height + 2 * BackgroundStyle.Border;
            return this;
        }
        public Panel SetClientDimensions(Vector2 clientDimensions)
        {
            this.ClientSize = new Rectangle(0, 0, (int)clientDimensions.X, (int)clientDimensions.Y);
            this.Width = ClientSize.Width + 2 * BackgroundStyle.Border;
            this.Height = ClientSize.Height + 2 * BackgroundStyle.Border;
            return this;
        }
        public Panel SetClientDimensions(int w, int h)
        {
            this.ClientSize = new Rectangle(0, 0, w, h);
            this.Width = ClientSize.Width + 2 * BackgroundStyle.Border;
            this.Height = ClientSize.Height + 2 * BackgroundStyle.Border;
            return this;
        }
    }
}
