using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.UI
{
    public class Icon
    {
        //static public readonly Icon X = new Icon(UIManager.Icons16x16, 0, 16);
        //static public readonly Icon ArrowUp = new Icon(UIManager.UpArrow, 0, 16);
        //static public readonly Icon ArrowDown = new Icon(UIManager.DownArrow, 0, 16);


        static public readonly Icon X = new Icon(UIManager.IconX);
        static public readonly Icon ArrowUp = new Icon(UIManager.ArrowUp);
        static public readonly Icon ArrowDown = new Icon(UIManager.ArrowDown);
        static public readonly Icon ArrowLeft = new Icon(UIManager.ArrowLeft);
        static public readonly Icon ArrowRight = new Icon(UIManager.ArrowRight);
        static public readonly Icon Cursor = new Icon(UIManager.MousePointer);
        static public readonly Icon CursorGrayscale = new Icon(UIManager.MousePointerGrayscale);
        static public readonly Icon Construction = new Icon(UIManager.SpriteConstruction);

        static public readonly Icon Cross = new Icon(UI.UIManager.Icons16x16, 0, 16);
        static public readonly Icon Replace = new Icon(UI.UIManager.Icons16x16, 3, 16);
        static public readonly Icon Dice = new Icon(UIManager.Icons16x16, 1, 16);
        //static public readonly Icon Construction = new Icon(UIManager.Icons32, 12, 32);

        public Color Tint = Color.White;
        public Texture2D SpriteSheet;
        public Rectangle SourceRect;
        //public AtlasWithDepth.Node.Token AtlasToken { get; set; }
        public IAtlasNodeToken AtlasToken { get; set; }
        //public Icon(Atlas.Node.Token token)
        //{
        //    SpriteSheet = token.Atlas.Texture;
        //    SourceRect = token.Rectangle;
        //}
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
            sprite = sprite ?? Sprite.Default;
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
            if (this.AtlasToken == null)
                sb.Draw(SpriteSheet, loc, sourceRect.HasValue ? Rectangle.Intersect(this.SourceRect, new Rectangle(this.SourceRect.X + sourceRect.Value.X, this.SourceRect.Y + sourceRect.Value.Y, sourceRect.Value.Width, sourceRect.Value.Height)) : this.SourceRect, Color.White);
            else
                //sb.Draw(Sprite.Atlas.Texture, loc, this.AtlasToken.Rectangle, Color.White);
                sb.Draw(this.AtlasToken.Atlas.Texture, loc, this.AtlasToken.Rectangle, Color.White);
        }
        public void Draw(SpriteBatch sb, Vector2 loc, Vector2 originPercentage)
        {
            if (this.AtlasToken == null)
                sb.Draw(SpriteSheet, new Vector2((int)loc.X, (int)loc.Y), SourceRect, Color.White, 0, new Vector2(SourceRect.Width * originPercentage.X, SourceRect.Height * originPercentage.Y), 1, SpriteEffects.None, 0);
            else
                //        sb.Draw(Sprite.Atlas.Texture, loc, this.AtlasToken.Rectangle, Color.White, 0, new Vector2(SourceRect.Width * originPercentage.X, SourceRect.Height * originPercentage.Y), 1, SpriteEffects.None, 0);
                sb.Draw(this.AtlasToken.Atlas.Texture, loc.Floor(), this.AtlasToken.Rectangle, Color.White, 0, new Vector2(SourceRect.Width * originPercentage.X, SourceRect.Height * originPercentage.Y), 1, SpriteEffects.None, 0);
        }

        public void Draw(SpriteBatch sb, Vector2 loc, float scale = 1, float alpha = 1)
        {
            ////    if (this.AtlasToken == null)
            ////        sb.Draw(SpriteSheet, loc, SourceRect, Color.White);
            ////    else
            ////        //sb.Draw(this.AtlasToken.Texture, loc, this.AtlasToken.Rectangle, Color.White);
            ////        sb.Draw(this.AtlasToken.Atlas.Texture, loc, this.AtlasToken.Rectangle, Color.White);
            //if (this.AtlasToken == null)
            //    sb.Draw(SpriteSheet, loc, SourceRect, Color.White * alpha, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            //else
            //    //sb.Draw(this.AtlasToken.Texture, loc, this.AtlasToken.Rectangle, Color.White);
            //    sb.Draw(this.AtlasToken.Atlas.Texture, loc, this.AtlasToken.Rectangle, Color.White * alpha, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            this.Draw(sb, loc, Color.White * alpha, scale);
        }
        public void Draw(SpriteBatch sb, Vector2 loc, Color color, float scale = 1)
        {
            if (this.AtlasToken == null)
                sb.Draw(SpriteSheet, loc, SourceRect, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
            else
                //sb.Draw(this.AtlasToken.Texture, loc, this.AtlasToken.Rectangle, Color.White);
                sb.Draw(this.AtlasToken.Atlas.Texture, loc, this.AtlasToken.Rectangle, color, 0, Vector2.Zero, scale, SpriteEffects.None, 0);
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

        public void DrawAboveEntity(SpriteBatch sb, Camera camera, GameObject entity, float scale = .5f)
        {
            var bounds = entity.GetSprite().GetBounds();
            //var bounds = entity.GetBounds(camera);
            var offset = IMap.IconOffset;
            scale *= camera.Zoom;
            var pos = camera.GetScreenPosition(entity.Global) - new Vector2(this.SourceRect.Width, this.SourceRect.Height) * scale / 2; ;// -new Vector2(UI.Icon.Cross.SourceRect.Width / 2, rect.Height * camera.Zoom);
            //pos.Y -= camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, Block.Bounds, Vector2.Zero).W;
            pos.Y -= bounds.Height * camera.Zoom;
            pos.Y += offset * this.SourceRect.Height / 4 * scale;
            this.Draw(sb, pos, scale, alpha: .5f);
        }
        public void DrawAboveEntity(SpriteBatch sb, Camera camera, Vector3 global, float scale = .5f)
        {
            var bounds = Block.Bounds;
            var offset = IMap.IconOffset;
            scale *= camera.Zoom;
            var pos = camera.GetScreenPosition(global) - new Vector2(this.SourceRect.Width, this.SourceRect.Height) * scale / 2; ;// -new Vector2(UI.Icon.Cross.SourceRect.Width / 2, rect.Height * camera.Zoom);
            //pos.Y -= camera.GetScreenBoundsVector4(global.X, global.Y, global.Z, Block.Bounds, Vector2.Zero).W;
            pos.Y -= bounds.Height * camera.Zoom;
            pos.Y += offset * this.SourceRect.Height / 4 * scale;
            this.Draw(sb, pos, scale, alpha: .5f);
        }
    }
}
