using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Editor
{
    class GridTool : ControlTool
    {
        int Z = 64;
        Sprite GridSprite = Sprite.BlockFaceHighlights[Vector3.UnitZ];
        TargetArgs Mouseover;
        TargetArgs NextMouseover;
        internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera camera)
        {
            this.DrawGrid(sb, camera);
            base.DrawBeforeWorld(sb, map, camera);
        }

        void DrawGrid(MySpriteBatch sb, Camera camera)
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
                        //Camera camera = ScreenManager.CurrentScreen.Camera;

                        var bounds = camera.GetScreenBounds(global, Block.Bounds);
                        var pos = new Vector2(bounds.X, bounds.Y);// cam.GetScreenBounds(global);
                        var gd = Game1.Instance.GraphicsDevice;
                        var depth = global.GetDrawDepth(Engine.Map, camera);
                        Color color;

                        //if (this.HitTest(bounds))
                        //    color = Color.Red;
                        //else color = Color.White;
                        sb.Draw(Sprite.Atlas.Texture, pos, GridSprite.AtlasToken.Rectangle, 0, Vector2.Zero, camera.Zoom, Color.White, SpriteEffects.None, depth);

                        
                        //if (this.Mouseover.Global == global)
                        //    color = Color.Red;
                        //else color = Color.White;

                        //sb.Draw(Sprite.Atlas.Texture, pos, GridSprite.AtlasToken.Rectangle, 0, Block.OriginCenter, cam.Zoom, color, SpriteEffects.None, depth);
                        if(this.Mouseover != null)
                            if (this.Mouseover.Global == global)
                        sb.Draw(Sprite.Atlas.Texture, pos, GridSprite.AtlasToken.Rectangle, 0, Vector2.Zero, camera.Zoom, Color.Red, SpriteEffects.None, depth);

                        this.HitTest(global, bounds, camera);
                      

                        gd.SamplerStates[0] = camera.Zoom >= 1 ? SamplerState.PointClamp : SamplerState.AnisotropicClamp;
                        gd.SamplerStates[1] = camera.Zoom >= 1 ? SamplerState.PointClamp : SamplerState.AnisotropicClamp;
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
            //if (!bounds.Intersects(mouse))
            //    return false;
            if (!this.GridSprite.HitTest(cam, bounds))
                return false;

            if(this.NextMouseover == null)
            {
                this.NextMouseover = new TargetArgs(Engine.Map, global);
                return true;
            }
            if (global.GetDrawDepth(Engine.Map, cam) > this.NextMouseover.Global.GetDrawDepth(Engine.Map, cam))
            {
                //if (!this.GridSprite.HitTest(cam, bounds))
                //    return false;
                this.NextMouseover.Global = global;
                return true;
            }

            return false;
        }
    }
}
