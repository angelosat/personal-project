using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.Components
{
    public class MouseMapComponent : SpriteComponent
    {
        public MouseMapComponent(Sprite sprite) : base(sprite) { }

        public new void Draw(
            SpriteBatch sb,
            Camera camera,
            Controller controller,
            Player player,
            Map map,
            Chunk chunk,
            Cell cell,
            Rectangle bounds,
            //SpriteComponent sprite
            GameObject obj,
            float depth
            )
        {

            //Console.WriteLine("ASDASD");
            Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
            byte sunlight = chunk.GetSunlight(cell.LocalCoords);
            Color color = Color.Lerp(Color.Black, Color.White, (sunlight + 1) / 16.0f);
            //Color color = Color.Lerp(Color.Black, Color.White, (cell.Skylight + 1) / 16.0f);//cell.Skylight / 15.0f); //Color.White;


            MovementComponent posComp;
            if (obj.TryGetComponent<MovementComponent>("Position", out posComp))
            {
                Position pos = posComp.GetProperty<Position>("Position");
                if (camera.CullingCheck(pos.Global.X, pos.Global.Y, pos.Global.Z, Sprite.GetBounds(), out bounds))
                    screenLoc = new Vector2(bounds.X, bounds.Y);
            }



            // TODO: fix this crap
            //int orientation = (Orientation + (int)camera.Rotation) % Sprite.SourceRect[Variation].Length;
            //camera.SpriteBatch.Draw(Sprite.Texture, screenLoc, Sprite.SourceRect[Variation][orientation], color, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, depth);


            // TODO: checking for speed is slow
            sb.Draw(Sprite.Texture, screenLoc, Sprite.SourceRect[Variation][Orientation], color, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, depth);

            Color face;
            if (HitTest(bounds, camera, out face))
            {
                    controller.MouseoverNext.Object = obj;
            }


        }

        //protected override bool HitTest(Rectangle bounds, Camera camera)
        //{
        //    if (bounds.Intersects(Controller.Instance.MouseRect))
        //    {
        //        int xx = (int)((Controller.Instance.msCurrent.X - bounds.X) / (float)camera.Zoom);
        //        int yy = (int)((Controller.Instance.msCurrent.Y - bounds.Y) / (float)camera.Zoom);

        //        Color color = Sprite.MouseMap[yy * Sprite.SourceRect[0][0].Width + xx];
        //        if (color.A > 0)
        //        {

        //            return true;
        //        }
        //    }
        //    return false;
        //}
    }
}
