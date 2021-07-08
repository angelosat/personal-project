using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.UI
{
    public class PictureBox : ButtonBase
    {
        public Action<PictureBox> Animate = (box) => { };
        public event EventHandler<EventArgs> SpriteChanged;

        Action<RenderTarget2D> _Renderer;

        public Action<RenderTarget2D> Renderer
        {
            get { return _Renderer; }
            set
            {
                this._Renderer = value;
                this.Sprite = new RenderTarget2D(Game1.Instance.GraphicsDevice, this.BoundsScreen.Width, this.BoundsScreen.Height);
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
      
        Rectangle? _SourceRect;
        public Rectangle SourceRect
        {
            get { return _SourceRect.GetValueOrDefault(this.Sprite is null ? new Rectangle(0,0,0,0) : this.Sprite.Bounds); }
            set
            {
                _SourceRect = value;
                Width = SourceRect.Width;
                Height = SourceRect.Height;
                this.Invalidate();
                switch (Alignment)
                {
                    case HorizontalAlignment.Center:
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

        public override void Validate(bool cascade = false)
        {
            if (this.DrawAction == null)
            {
                if (Renderer != null)
                    this.Renderer(this.Sprite as RenderTarget2D);

                base.Validate(cascade);
                return;
            }
            else
            {
                var gd = Game1.Instance.GraphicsDevice;
                Texture = CreateTexture(gd);
                gd.SetRenderTarget(this.Texture);
                gd.Clear(Color.Transparent);
                this.DrawAction();
                gd.SetRenderTarget(null);
                this.Valid = true;
            }
        }
        public override void OnPaint(SpriteBatch sb)
        {
            sb.Draw(this.Sprite, Vector2.Zero, this.SourceRect, Color.White);
        }

        public override void Update()
        {
            this.Animate(this);
            base.Update();
        }
        public Action DrawAction;
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
            SourceRect = new Rectangle(0, 0, (int)size.X, (int)size.Y);
            Width = SourceRect.Width;
            Height = SourceRect.Height;
            this.Renderer = renderer;

            Alignment = halign;
            switch (halign)
            {
                case HorizontalAlignment.Center:
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
                    PictureOrigin = new Vector2(SourceRect.Width / 2, SourceRect.Height / 2);
                    break;
                case HorizontalAlignment.Right:
                    PictureOrigin.X = SourceRect.Width;
                    break;
                default:
                    break;
            }
        }
        public PictureBox(Entity item, float scale):this(item.Body.RenderIcon(item, scale))
        {
        }
        [Obsolete]
        public PictureBox(Bone bone, float scale)
        {
            this.Width = (int)(32 * scale);
            this.Height = (int)(32 * scale);
            this.SourceRect = new Rectangle(0, 0, this.Width, this.Height);
            this.DrawAction = () => GameObject.DrawIcon(bone, this.Width, this.Height, scale);
        }
        public PictureBox(IAtlasNodeToken token):this(Vector2.Zero, token.Atlas.Texture, token.Rectangle)
        {
        }
        public PictureBox(Texture2D texture2D, Rectangle? rectangle = null, float scale = 1):this(Vector2.Zero, texture2D, rectangle)
        {
            this.Width = (int)((rectangle?.Width ?? texture2D.Width) * scale);
            this.Height = (int)((rectangle?.Height ?? texture2D.Height) * scale);
        }
       
        public override void Draw(SpriteBatch sb)
        {
            sb.Draw(this.Sprite, BoundsScreen, Color.White);

            base.Draw(sb);
            if (Active && MouseHover)
                sb.Draw(Texture, BoundsScreen, SourceRect, new Color(1, 1, 1, 0.5f), this.Rotation, PictureOrigin, SpriteEffects.None, 0);
        }

        internal void SetTexture(Atlas.Node.Token token)
        {
            this.SourceRect = token.Rectangle;
            this.Sprite = token.Atlas.Texture;
            this.Validate();
        }
    }
}
