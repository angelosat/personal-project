using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class PictureBox : ButtonBase// Control
    {
        public Action<PictureBox> Animate = (box) => { };
        
     //   public Vector2 Origin = Vector2.Zero;
        //public Func<Texture2D> SpriteGetter;
        //public Action<Vector2, Rectangle> SpriteRenderer;
        public event EventHandler<EventArgs> SpriteChanged;

        Action<RenderTarget2D> _Renderer;

        public Action<RenderTarget2D> Renderer
        {
            get { return _Renderer; }
            set
            {
                this._Renderer = value;
                this.Sprite = new RenderTarget2D(Game1.Instance.GraphicsDevice, this.ScreenBounds.Width, this.ScreenBounds.Height);
            }
        }

        protected Texture2D _Sprite;
        public Texture2D Sprite
        {
            get { return _Sprite; }
            set
            {
                _Sprite = value;
                Width = SourceRect.Width;
                Height = SourceRect.Height;
                this.Invalidate();
                OnSpriteChanged();
            }
        }
        //protected RenderTarget2D _Sprite;
        //public RenderTarget2D Sprite
        //{
        //    get { return _Sprite; }
        //    set
        //    {
        //        _Sprite = value;
        //        Width = SourceRect.Width;
        //        Height = SourceRect.Height;
        //        this.Invalidate();
        //        OnSpriteChanged();
        //    }
        //}
        Rectangle? _SourceRect;
        public Rectangle SourceRect
        {
            get { return _SourceRect.GetValueOrDefault(this.Sprite.IsNull() ? new Rectangle(0,0,0,0) : this.Sprite.Bounds); }
            set
            {
                _SourceRect = value;
                Width = SourceRect.Width;
                Height = SourceRect.Height;
                this.Invalidate();
                switch (Alignment)
                {
                    case HorizontalAlignment.Center:
                        //PictureOrigin = new Vector2(Width / 2, (Height - SourceRect.Height) / 2);
                        PictureOrigin = new Vector2(SourceRect.Width / 2, SourceRect.Height / 2);
                        break;
                    case HorizontalAlignment.Right:
                        PictureOrigin.X = SourceRect.Width;
                        break;
                    default:
                        break;
                }
            }
        }

        public override void Validate()
        {
            if (this.DrawAction == null)
            {
                //if (this.SpriteGetter != null)
                //    this.Sprite = this.SpriteGetter();
                if (Renderer != null)
                    this.Renderer(this.Sprite as RenderTarget2D);

                base.Validate();
                return;
            }
            else
            {
                this.DrawAction();
                this.Valid = true;
            }
        }
        public override void OnPaint(SpriteBatch sb)
        {
            
            //sb.Draw(this.Sprite, Vector2.Zero, SourceRect, Color.White);//Color);
            sb.Draw(this.Sprite, Vector2.Zero, null, Color.White);//Color);

            //sb.Draw(this.Sprite, Vector2.Zero, new Rectangle(0,0,300,300), Color.White);//Color);

            //this.DrawAction(sb);
           // sb.Draw(this.Sprite, Vector2.Zero, SourceRect, Color, 0, Origin, SpriteEffects.None, 0);
        }

        public override void Update()
        {
            //this.Invalidate();//TOO SLOW TO REPAINT EVERY FRAME
            this.Animate(this);
            base.Update();
        }

        public Action DrawAction;// = () => { };
        public Vector2 PictureOrigin = new Vector2(0);
        protected void OnSpriteChanged()
        {
            if (SpriteChanged != null)
                SpriteChanged(this, EventArgs.Empty);
        }
        public override RenderTarget2D CreateTexture(GraphicsDevice gd)
        {
            return new RenderTarget2D(Game1.Instance.GraphicsDevice, SourceRect.Width, SourceRect.Height);
        }
        public HorizontalAlignment Alignment;
        public PictureBox() { }
        public PictureBox(Vector2 size, Action<RenderTarget2D> renderer, HorizontalAlignment halign = HorizontalAlignment.Left, VerticalAlignment valign = VerticalAlignment.Top)
        {
            //Sprite = sprite;
            SourceRect = new Rectangle(0, 0, (int)size.X, (int)size.Y);// sprite.Bounds;
            Width = SourceRect.Width;
            Height = SourceRect.Height;
            this.Renderer = renderer;

            Alignment = halign;
            switch (halign)
            {
                case HorizontalAlignment.Center:
                    //PictureOrigin = new Vector2(Width / 2, (Height - SourceRect.Height) / 2);
                    PictureOrigin = new Vector2(SourceRect.Width / 2, SourceRect.Height / 2);
                    break;
                case HorizontalAlignment.Right:
                    PictureOrigin.X = SourceRect.Width;
                    break;
                default:
                    break;
            }
        }
        public PictureBox(Vector2 location, Texture2D sprite, Rectangle? sourcerect, HorizontalAlignment halign = HorizontalAlignment.Left, VerticalAlignment valign = VerticalAlignment.Top)
            : base(location)
        {
            Sprite = sprite;
            SourceRect = sourcerect.HasValue ? sourcerect.Value : sprite.Bounds;
            Width = SourceRect.Width;
            Height = SourceRect.Height;

            Alignment = halign;
            switch (halign)
            {
                case HorizontalAlignment.Center:
                    //PictureOrigin = new Vector2(Width / 2, (Height - SourceRect.Height) / 2);
                    PictureOrigin = new Vector2(SourceRect.Width / 2, SourceRect.Height / 2);
                    break;
                case HorizontalAlignment.Right:
                    PictureOrigin.X = SourceRect.Width;
                    break;
                default:
                    break;
            }
        }


        public override void Draw(SpriteBatch sb)
        {
            if (this.DrawMode == UI.DrawMode.Normal)
            {
                //sb.Draw(Texture, ScreenLocation, SourceRect, Color.Lerp(Color.Transparent, Color.White, Opacity), this.Rotation, PictureOrigin, 1, SpriteEffects.None, 0);
                //if (this.SpriteRenderer != null)
                //    this.SpriteRenderer();
                //else
                sb.Draw(this.Sprite, ScreenBounds, Color.White);
            }
          //  else
          //      OnDrawItem(new DrawItemEventArgs(sb, Bounds));
            
            base.Draw(sb);
            if (Active && MouseHover)
                sb.Draw(Texture, ScreenBounds, SourceRect, new Color(1, 1, 1, 0.5f), this.Rotation, PictureOrigin, SpriteEffects.None, 0);
        }


        //static public PictureBox LoadingBox
        //{
        //    get
        //    {
        //        int w = UIManager.DefaultLoadingSprite.Width, h = UIManager.DefaultLoadingSprite.Height;
        //        int ww = (int)Math.Sqrt(2 * w * w);
        //        int hh = (int)Math.Sqrt(2 * h * h);
        //        return new PictureBox()
        //        {
        //            Sprite = UIManager.DefaultLoadingSprite,
        //            Width = ww,
        //            Height = hh,
        //            Origin = new Vector2(w, h) * 0.5f,// new Vector2(0.5f),
        //            Color = Color.LightSeaGreen,
        //            Animate = (box) =>
        //            {
        //                box.Rotation = (float)(4 * Math.PI * DateTime.Now.Millisecond / 2000f);
        //            }
        //        };
        //    }
        //}

        public override void Draw(SpriteBatch sb, Rectangle viewport)
        {
            //sb.Draw(UIManager.Highlight, this.Bounds, Color.Blue * .5f);
            base.Draw(sb, viewport);
        }
    }

}
