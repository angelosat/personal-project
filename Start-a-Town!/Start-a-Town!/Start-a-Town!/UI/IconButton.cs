using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    public class IconButton : ButtonBase
    {
        protected virtual void OnIconChanged() { }
        protected Icon _Icon;
        public Icon Icon
        {
            get { return _Icon; }
            set
            {
                _Icon = value;
                OnIconChanged();
                this.Invalidate();
            }
        }

        public override int Width
        {
            get
            {
                if (this.BackgroundTexture.IsNull())
                    return Icon.SourceRect.Width;
                if (Icon.IsNull())
                    return this.BackgroundTexture.Width;
                return Math.Max(this.BackgroundTexture.Width, Icon.SourceRect.Width);
            }
            set
            {
             //   base.Width = value;
            }
        }
        public override int Height
        {
            get
            {
                if (this.BackgroundTexture.IsNull())
                    return Icon.SourceRect.Height;
                if (Icon.IsNull())
                    return this.BackgroundTexture.Height;
                return Math.Max(this.BackgroundTexture.Height, Icon.SourceRect.Height);
            }
            set
            {
              //  base.Height = value;
            }
        }

        public override void OnPaint(SpriteBatch sb)
        {
            if(!this.BackgroundTexture.IsNull())
            DrawSprite(sb, new Rectangle(0, 0, Width, Height), null, Color, MouseHover ? 1 : 0.5f);
            //    Rectangle sourcerect = new Rectangle(IconID * 16, 0, Width, Height);
            if (!Icon.IsNull())
                this.Icon.Draw(sb, this.Dimensions * 0.5f + new Vector2(0, (Pressed ? 1 : 0)), new Vector2(0.5f));
            // DrawIcon(sb, Vector2.Zero, sourcerect, Color.White, Opacity);
        }
        public IconButton(Texture2D backgroundText) { BackgroundTexture = backgroundText; }
        public IconButton() : this(Vector2.Zero) { }
        public IconButton(Vector2 location) : base(location) { BackgroundTexture = UIManager.DefaultIconButtonSprite; }

        public override void Validate()
        {
            base.Validate();
        }

        public override void DrawSprite(SpriteBatch sb, Rectangle destRect, Rectangle? sourceRect, Color color, float opacity)
        {
            //sb.Draw(UIManager.Icon16Background, destRect, sourceRect, color * opacity, 0, Vector2.Zero, Pressed?SpriteEffects.FlipVertically: SpriteEffects.None, Depth);
            sb.Draw(this.BackgroundTexture, destRect, sourceRect, color * opacity, 0, Vector2.Zero, Pressed ? SpriteEffects.FlipVertically : SpriteEffects.None, Depth);
        }
    }
}
