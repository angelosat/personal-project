using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    public class Icon
    {
        public Color Tint = Color.White;
        public Texture2D SpriteSheet;
        public Rectangle SourceRect;
        //public AtlasWithDepth.Node.Token AtlasToken { get; set; }
        public IAtlasNodeToken AtlasToken { get; set; }
        public Icon(Texture2D spritesheet, uint index, int size)
        {
            SpriteSheet = spritesheet;
            // Index = index;
            SourceRect = new Rectangle(((int)index % (SpriteSheet.Width / size)) * size, ((int)index / (SpriteSheet.Width / size)) * size, size, size);
        }
        public Icon(string assetName)
        {
            this.AtlasToken = Sprite.Atlas.Load(assetName);
        }
        public Icon(Texture2D spritesheet, Rectangle source)
        {
            this.SpriteSheet = spritesheet;
            this.SourceRect = source;
        }
        public Icon(Sprite sprite)
        {
            if (sprite.AtlasToken != null)
            {
                this.AtlasToken = sprite.AtlasToken;
                this.SpriteSheet = Sprite.Atlas.Texture;
                this.SourceRect = sprite.AtlasToken.Rectangle;
            }
            else
            {
                this.SpriteSheet = sprite.Texture;
                this.SourceRect = sprite.SourceRects[0][0];
            }
            this.Tint = sprite.Tint;
        }
        public Icon(IAtlasNodeToken token)
        {
            this.AtlasToken = token;
            this.SpriteSheet = token.Atlas.Texture;
            this.SourceRect = token.Rectangle;
        }
        public override string ToString()
        {
            return "Sprite sheet: " + SpriteSheet.Name;// +
               // "\nIcon index: " + Index;
        }


        public void Draw(SpriteBatch sb, Vector2 loc, Rectangle? sourceRect)
        {
            if (this.AtlasToken.IsNull())
                sb.Draw(SpriteSheet, loc, sourceRect.HasValue ? Rectangle.Intersect(this.SourceRect, new Rectangle(this.SourceRect.X + sourceRect.Value.X, this.SourceRect.Y + sourceRect.Value.Y, sourceRect.Value.Width, sourceRect.Value.Height)) : this.SourceRect, Color.White);
            else
                //sb.Draw(Sprite.Atlas.Texture, loc, this.AtlasToken.Rectangle, Color.White);
                sb.Draw(this.AtlasToken.Atlas.Texture, loc, this.AtlasToken.Rectangle, Color.White);
        }
        public void Draw(SpriteBatch sb, Vector2 loc, Vector2 originPercentage)
        {
            if (this.AtlasToken.IsNull())
                sb.Draw(SpriteSheet, new Vector2((int)loc.X, (int)loc.Y), SourceRect, Color.White, 0, new Vector2(SourceRect.Width * originPercentage.X, SourceRect.Height * originPercentage.Y), 1, SpriteEffects.None, 0);
            else
                //        sb.Draw(Sprite.Atlas.Texture, loc, this.AtlasToken.Rectangle, Color.White, 0, new Vector2(SourceRect.Width * originPercentage.X, SourceRect.Height * originPercentage.Y), 1, SpriteEffects.None, 0);
                sb.Draw(this.AtlasToken.Atlas.Texture, loc, this.AtlasToken.Rectangle, Color.White, 0, new Vector2(SourceRect.Width * originPercentage.X, SourceRect.Height * originPercentage.Y), 1, SpriteEffects.None, 0);
        }
        public void Draw(SpriteBatch sb, Vector2 loc)
        {
            if (this.AtlasToken.IsNull())
                sb.Draw(SpriteSheet, loc, SourceRect, Color.White);
            else
                //sb.Draw(this.AtlasToken.Texture, loc, this.AtlasToken.Rectangle, Color.White);
                sb.Draw(this.AtlasToken.Atlas.Texture, loc, this.AtlasToken.Rectangle, Color.White);
        }
        /// <summary>
        /// Draws near mouse
        /// </summary>
        /// <param name="sb"></param>
        public void Draw(SpriteBatch sb)
        {
            this.Draw(sb, UI.UIManager.Mouse + new Vector2(16, 0));
        }
        //public void Draw(SpriteBatch sb, Vector2 position, Color color)
        //{
        //    sb.Draw(SpriteSheet, position, SourceRect, color);
        //}


        static public readonly Icon Cross = new Icon(UI.UIManager.Icons16x16, 0, 16);
        static public readonly Icon Replace = new Icon(UI.UIManager.Icons16x16, 3, 16);
    }
}
