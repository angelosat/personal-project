using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class Label : ButtonBase //control
    {
        static public int DefaultHeight = UIManager.LineHeight + 2;
        HorizontalAlignment _Halign;
        public HorizontalAlignment Halign
        {
            get { return _Halign; }
            set
            {
                _Halign = value;
                //if(!string.IsNullOrWhiteSpace(this.Text))
                //OnTextChanged();
            }
        }
        public VerticalAlignment Valign;
        
        public Color TextBackground = Color.Transparent;

        public override string ToString()
        {
            // return base.ToString() + " " + Text;
            return "Label: " + this.Text ?? this.Name;
        }

        public override int Width
        {
            get
            {
                return base.Width;
            }
            set
            {
                base.Width = value;
                //this.Texture = null; // this line causes flickering whenever text changes
                if (value > 0)
                    this.Invalidate();
            }
        }


        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
           // this.Bounds.DrawHighlight(sb, 0.5f);
            //new Rectangle(this.Bounds.X, this.Bounds.Y, this.TextSprite.Width, this.TextSprite.Height).DrawHighlight(sb, 0.5f);

        }

        public Color ActiveColor = Color.Lime;
        public override void OnPaint(SpriteBatch sb)
        {
            //if (Active && MouseHover)
            //{
            //    Button.DrawSprite(sb, Vector2.Zero, Width, Color.White * 0.5f, Pressed ? SpriteEffects.FlipVertically : SpriteEffects.None); 
            //   // (Btn_Blocks.MouseHover && Btn_Blocks.Active) ? Color.White * 0.5f : Color.Transparent;
            //}
            //System.Diagnostics.Debug.Assert(this.Text != "");
            UIManager.DrawStringOutlined(sb, this.Text, new Vector2(1, 1) + Anchor * Dimensions + ((Active && Pressed) ? Vector2.UnitY : Vector2.Zero), Anchor, (MouseHover && Active) ? ActiveColor : TextColor, TextOutline, Font);
            //UIManager.DrawStringOutlined(sb, this.Text, new Vector2(1, 1), Vector2.Zero, (MouseHover && Active) ? ActiveColor : TextColor, Outline, Font);
       }

        public event UIEvent TextChanged;
        protected override void OnTextChanged()
        {
            //this.Invalidate();
            base.OnTextChanged();
            //TextSprite = UIManager.DrawTextOutlined(UIManager.WrapText(Text, Width));
           //this.TextSprite = UIManager.DrawTextOutlined(Text, Color);
           //// TextSprite = UIManager.DrawTextOutlined(Text, Fill, Outline, FontStyle);//, HorizontalAlignment);
           // //Vector2 textsize = UIManager.Font.MeasureString(this.Text);
           // Width = TextSprite.Width +2;
           // Height = TextSprite.Height+2;
            //Texture = new RenderTarget2D(Game1.Instance.GraphicsDevice, this.Width, this.Height);
            switch (Halign)
            {
                case HorizontalAlignment.Left:
                    Origin.X = 0;
                    break;
                case HorizontalAlignment.Center:
                    Origin.X = Width / 2;
                    break;
                case HorizontalAlignment.Right:
                    Origin.X = Width;
                    break;
                default:
                    break;
            }
            switch (Valign)
            {
                case VerticalAlignment.Top:
                    Origin.Y = 0;
                    break;
                case VerticalAlignment.Center:
                    Origin.Y = Height / 2;
                    break;
                case VerticalAlignment.Bottom:
                    Origin.Y = Height;
                    break;
                default:
                    break;
            }
            //Location.X -= Origin.X;
            //Location.Y -= Origin.Y;

            if (TextChanged != null)
                TextChanged(this, new UIEventArgs(this));
        }
      //  public new bool Active = false;
        public Label(int width) : base() { 
            Text = "";
            this.Width = width;
            this.Height = Label.DefaultHeight;
            Active = false;
            //Location.X = size.X;
            //Location.Y = size.Y;
         //   Texture = new RenderTarget2D(Game1.Instance.GraphicsDevice, this.Width, this.Height);
        }
        public Label() : this(Vector2.Zero, "") { }
        public Label(string text) : this(Vector2.Zero, text) { }
        //public Label(Vector2 location, string text = "")
        //    : base(location)
        //{
        //    Text = text;
        //    Width = TextSprite.Width;
        //    Height = TextSprite.Height;
        //    //Location.X -= Origin.X;
        //}
        public Label(Vector2 location, string text = "", Color? fill = null, Color? outline = null, SpriteFont font = null)// System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular)
            : base(location)
        {
         //   Color = c.HasValue ? c.Value : Color.White;
            if (fill.HasValue)
                this.Fill = fill.Value;
            if (outline.HasValue)
                this.TextOutline = outline.Value;
            //  this.FontStyle = style;
            Text = text;
            this.Font = font ?? UIManager.Font;
            
            //Width = TextSprite.Width;
            //Height = TextSprite.Height;
            Active = false;
           // Texture = new RenderTarget2D(Game1.Instance.GraphicsDevice, this.Width, this.Height);
            //Location.X -= Origin.X;
        }
        public Label(string text = "", Color? fill = null, Color? outline = null, SpriteFont font = null)// System.Drawing.FontStyle style = System.Drawing.FontStyle.Regular)
            : base()
        {
            if (fill.HasValue)
                this.Fill = fill.Value;
            if (outline.HasValue)
                this.TextOutline = outline.Value;
            Text = text;
            this.Font = font ?? UIManager.Font;
            Active = false;
        }
        public Label(Vector2 location, string text, HorizontalAlignment halign)
            : base(location)
        {
            this.Halign = halign;
            Text = text;
            //Width = TextSprite.Width;
            //Height = TextSprite.Height;
            
            //switch (halign)
            //{
            //    case TextAlignment.Left:
            //        Origin.X = 0;
            //        break;
            //    case TextAlignment.Center:
            //        Origin.X = Width/2;
            //        break;
            //    case TextAlignment.Right:
            //        Origin.X = Width;
            //        break;
            //    default:
            //        break;
            //}
            ////Console.WriteLine(Location.ToString());
            //Location.X -= Origin.X;
            ////Console.WriteLine(Location.ToString());
        }
        public Label(Vector2 location, string text, HorizontalAlignment halign, VerticalAlignment valign)
            : base(location)
        {
            Halign = halign;
            Valign = valign;
            Text = text;

            //Width = TextSprite.Width;
            //Height = TextSprite.Height;
            switch (halign)
            {
                case HorizontalAlignment.Left:
                    Origin.X = 0;
                    break;
                case HorizontalAlignment.Center:
                    Origin.X = Width / 2;
                    break;
                case HorizontalAlignment.Right:
                    Origin.X = Width;
                    break;
                default:
                    break;
            }
            switch (valign)
            {
                case VerticalAlignment.Top:
                    Origin.Y = 0;
                    break;
                case VerticalAlignment.Center:
                    Origin.Y = Height / 2;
                    break;
                case VerticalAlignment.Bottom:
                    Origin.Y = Height;
                    break;
                default:
                    break;
            }
            //Location.X -= Origin.X;
            //Location.Y -= Origin.Y;
            Location = Location - Origin;
        }

        public Label(string text, string format)
            : this(text)
        {
            this.Text = text;
            this.TextFormat = format ?? text;
            this.Dimensions = UIManager.Font.MeasureString(TextFormat) + new Vector2(2);
        }

        //public override void DrawHighlight(SpriteBatch sb)
        //{
        //    //sb.Draw(UIManager.Highlight, new Rectangle((int)ScreenLocation.X, (int)ScreenLocation.Y, Width, Height), Color.White);
        //    sb.Draw(UIManager.Highlight, new Rectangle((int)ScreenLocation.X - (int)Origin.X, (int)ScreenLocation.Y - (int)Origin.Y, TextSprite.Width, TextSprite.Height), null, Color.White, 0, Vector2.Zero, SpriteEffects.None, Depth);
        //}

        //public override void Draw(SpriteBatch sb, Rectangle viewport)
        //{
        //    base.Draw(sb, viewport);
        //    Rectangle final, source;
        //    this.Bounds.Clip(Texture.Bounds, viewport, out final, out source);
        //    sb.Draw(this.Texture, final, source, Color.White * Opacity);
        //}

        public override void Draw(SpriteBatch sb)
        {
            base.Draw(sb);
            DrawText(sb, ScreenLocation, null, MouseHover && Active ? Color.Lime : Color.White, Opacity); //Color.White
        }
        static public void DrawText(SpriteBatch sb, Texture2D textSprite, Vector2 position, int width, Color color, float opacity, HorizontalAlignment hAlign)
        {
            Color c = color * opacity;
            // TODO: fix the origin so it's not always at the center
            Vector2 offset = new Vector2(1, 1), origin = Vector2.Zero;
            switch (hAlign)
            {
                case HorizontalAlignment.Left:
                    offset = new Vector2(1, 1);
                    origin = Vector2.Zero;
                    break;
                case HorizontalAlignment.Center:
                    offset = new Vector2((float)Math.Floor(width * 0.5), 1);
                    origin = new Vector2(textSprite.Width / 2, 0);
                    break;
                case HorizontalAlignment.Right:
                    offset = new Vector2(width - 2, 1);
                    origin = new Vector2(textSprite.Width, 0);
                    break;
                default:
                    break;
            }

            //sb.Draw(TextSprite, position +  new Vector2((Width - TextSprite.Width) / 2, (Height - TextSprite.Height) / 2 + ((Pressed && Active) ? 1 : 0)), Rectangle.Intersect(TextSprite.Bounds, sourceRect.Value), c, 0, Vector2.Zero, 1, SpriteEffects.None, Depth);
            sb.Draw(textSprite, position + offset, null, c, 0, origin, 1, SpriteEffects.None, 0);
        }
        static public void DrawText(SpriteBatch sb, Texture2D textSprite, Vector2 position, Rectangle? sourceRect, int width, Color color, float opacity, HorizontalAlignment hAlign)
        {
            Color c = color * opacity;
            // sb.Draw(TextSprite, position, sourceRect, c, 0, Origin, 1, SpriteEffects.None, Depth); 
            if (!sourceRect.HasValue)
                sourceRect = textSprite.Bounds;
            // TODO: fix the origin so it's not always at the center

            Vector2 offset = new Vector2(1, 1), origin = Vector2.Zero;
            switch (hAlign)
            {
                case HorizontalAlignment.Left:
                    offset = new Vector2(1, 1);
                    origin = Vector2.Zero;
                    break;
                case HorizontalAlignment.Center:
                    offset = new Vector2((float)Math.Floor(width * 0.5), 1);
                    origin = new Vector2(textSprite.Width / 2, 0);
                    break;
                case HorizontalAlignment.Right:
                    offset = new Vector2(width - 2, 1);
                    origin = new Vector2(textSprite.Width, 0);
                    break;
                default:
                    break;
            }

            //sb.Draw(TextSprite, position +  new Vector2((Width - TextSprite.Width) / 2, (Height - TextSprite.Height) / 2 + ((Pressed && Active) ? 1 : 0)), Rectangle.Intersect(TextSprite.Bounds, sourceRect.Value), c, 0, Vector2.Zero, 1, SpriteEffects.None, Depth);
            sb.Draw(textSprite, position + offset, Rectangle.Intersect(textSprite.Bounds, sourceRect.Value), c, 0, origin, 1, SpriteEffects.None, 0);
        }
        public override void DrawText(SpriteBatch sb, Vector2 position, Rectangle? sourceRect, Color color, float opacity)
        {
            //Color c = color * opacity;
 
            //if (!sourceRect.HasValue)
            //    sourceRect = TextSprite.Bounds;
            //// TODO: fix the origin so it's not always at the center

            //Vector2 offset = new Vector2(1, 1), origin = Vector2.Zero;
            //switch (Halign)
            //{
            //    case HorizontalAlignment.Left:
            //        offset = new Vector2(1, 1);
            //        origin = Vector2.Zero;
            //        break;
            //    case HorizontalAlignment.Center:
            //        offset = new Vector2((float)Math.Floor(Width * 0.5), 1);
            //        origin = new Vector2(TextSprite.Width / 2, 0);
            //        break;
            //    case HorizontalAlignment.Right:
            //        offset = new Vector2(Width - 2, 1);
            //        origin = new Vector2(TextSprite.Width, 0);
            //        break;
            //    default:
            //        break;
            //}

            //sb.Draw(TextSprite, position + offset, Rectangle.Intersect(TextSprite.Bounds, sourceRect.Value), c, 0, origin, 1, SpriteEffects.None, Depth);

        }

        public override void OnMouseEnter()
        {
            base.OnMouseEnter();
            if (Active) this.Invalidate();
        }
        public override void OnMouseLeave()
        {
            base.OnMouseLeave();
            if (Active) this.Invalidate();
        }
    }
}
