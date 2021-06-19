using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.PlayerControl
{
    class HaulingTool : DefaultTool
    {
        public override void Update()
        {
            base.Update();
        }
        internal override void DrawAfterWorld(MySpriteBatch sb, IMap map)
        {
            var cam = map.Camera;
            base.DrawAfterWorld(sb, map);
            var haul = PersonalInventoryComponent.GetHauling(PlayerOld.Actor).Object;
            if (haul != null)
            {
                if (this.Target == null)
                    return;
                var global = this.Target.Global + this.Target.Face + (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : this.Target.Precise);
                haul.DrawPreview(sb, cam, global);
                //var body = haul.Body;
                //var pos = cam.GetScreenPositionFloat(global);
                //pos += body.OriginGroundOffset * cam.Zoom;
                ////body.DrawTree(this.Entity, sb, pos, Color.White, Color.White, Color.White, Color.Transparent, body.RestingFrame.Offset, 0, cam.Zoom, SpriteEffects.None, 0.5f, global.GetDrawDepth(Engine.Map, cam));
                //// TODO: fix difference between tint and material in this drawtree method
                //var tint = Color.White * .5f;// Color.Transparent;
                //body.DrawTree(haul, sb, pos, Color.White, Color.White, tint, Color.Transparent, 0, cam.Zoom, 0, SpriteEffects.None, 0.5f, global.GetDrawDepth(Engine.Map, cam));
            }
        }
    }
}
