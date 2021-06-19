using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components
{
    class GraphicsComponent : Component
    {
        Texture2D Sprite;
        byte Style, Rotation;
        Rectangle[] SourceRects;
        Rectangle SourceRect
        { get { return SourceRects[Rotation * 4 + Style]; } }
        Vector2 Origin;

        public GraphicsComponent(Texture2D sprite, Rectangle[] sourcerects, Vector2 origin, byte style = 0, byte rotation = 0)
        {
            Sprite = sprite;
            SourceRects = sourcerects;
            Origin = origin;
            Style = style;
            Rotation = rotation;
        }

        public void Draw(Camera camera, IEntity entity, Chunk chunk, Cell cell)
        {
            Vector3 global = entity.GetComponent<MovementComponent>("Position").Position.Global;
            Rectangle screenBounds;
            if (camera.CullingCheck(global.X, global.Y, global.Z, Sprite.Bounds, out screenBounds))
                camera.SpriteBatch.Draw(Sprite, screenBounds, SourceRect, Color.White, 0, Origin, SpriteEffects.None, 0f);
                //camera.SpriteBatch.Draw(Sprite, screenBounds, SourceRect, Color.White, 0, Origin, SpriteEffects.None, (global.X + global.Y + global.Z) / (float)camera.DepthFront);
        }
    }
}
