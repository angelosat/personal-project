using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.PlayerControl
{
    class TileGrid
    {
        int Z = 64;
        Sprite GridSprite = Sprite.BlockFaceHighlights[Vector3.UnitZ];
        TargetArgs Mouseover;
        TargetArgs NextMouseover;

        void Draw(MySpriteBatch sb, Camera cam, Color color)
        {
            //this.Mouseover = null;
            this.Mouseover = this.NextMouseover;
            this.NextMouseover = null;
            foreach (var chunk in Engine.Map.GetActiveChunks().Values)
            {
                for (int i = 0; i < Chunk.Size; i++)
                {
                    for (int j = 0; j < Chunk.Size; j++)
                    {
                        Vector3 global = new Vector3(i, j, Z).ToGlobal(chunk);
                        //Camera cam = ScreenManager.CurrentScreen.Camera;
                        var bounds = cam.GetScreenBounds(global, Block.Bounds);
                        var pos = new Vector2(bounds.X, bounds.Y);
                        var gd = Game1.Instance.GraphicsDevice;
                        var depth = global.GetDrawDepth(Engine.Map, cam);

                        sb.Draw(Sprite.Atlas.Texture, pos, GridSprite.AtlasToken.Rectangle, 0, Vector2.Zero, cam.Zoom, color, SpriteEffects.None, depth);

                        if (this.Mouseover != null)
                            if (this.Mouseover.Global == global)
                                sb.Draw(Sprite.Atlas.Texture, pos, GridSprite.AtlasToken.Rectangle, 0, Vector2.Zero, cam.Zoom, Color.White, SpriteEffects.None, depth);

                        this.HitTest(global, bounds, cam);


                        //gd.SamplerStates[0] = SamplerState.PointClamp;
                        //gd.SamplerStates[1] = SamplerState.PointClamp;
                    }
                }
            }
        }
        bool HitTest(Rectangle bounds)
        {
            var mouse = UIManager.MouseRect;
            if (!bounds.Intersects(mouse))
                return false;
            return true;
        }
        bool HitTest(Vector3 global, Rectangle bounds, Camera cam)
        {
            var mouse = UIManager.MouseRect;

            if (!this.GridSprite.HitTest(cam, bounds))
                return false;

            if (this.NextMouseover == null)
            {
                this.NextMouseover = new TargetArgs(Engine.Map, global);
                return true;
            }
            if (global.GetDrawDepth(Engine.Map, cam) > this.NextMouseover.Global.GetDrawDepth(Engine.Map, cam))
            {

                this.NextMouseover.Global = global;
                return true;
            }

            return false;
        }
    }
}
