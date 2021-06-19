﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.GameModes;
using Start_a_Town_.Net;

namespace Start_a_Town_.PlayerControl
{
    class BuildingPlacer : ControlTool
    {
        //public GameObject Building { get; set; }
        public Reaction.Product.ProductMaterialPair Building { get; set; }
        public BuildingPlacer(Reaction.Product.ProductMaterialPair building)//GameObject building)
        {
            this.Building = building;
        }
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Target == null)
                return Messages.Default;
            if (this.Target.Type != TargetType.Position)
                return Messages.Default;
            Client.PlayerBuild(this.Building, this.Target.Global + this.Target.Face);
            return Messages.Remove;
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera cam)
        {
            base.DrawBeforeWorld(sb, map, cam);

            if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            {
                Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
                return;
            }
            //if (this.TargetOld.IsNull())
            //    return;
            //if (!this.TargetOld.Exists)
            //    return;
            if (this.Target == null)
                return;
            var sprite = this.Building.Product.Body.Sprite;
            var atlastoken = sprite.AtlasToken;
            //var global = this.TargetOld.Global + this.Face;
            var global = this.Target.FaceGlobal;
            var pos = cam.GetScreenPosition(global);
            var depth = global.GetDrawDepth(Engine.Map, cam);
            Game1.Instance.GraphicsDevice.Textures[0] = atlastoken.Atlas.Texture;
            //this.Block.Draw(sb, pos - Block.OriginCenter * cam.Zoom, Color.White, Vector4.One, Color.White * 0.5f, cam.Zoom, depth, cell);
            sprite.Draw(sb, pos, Color.White * 0.5f, 0, sprite.OriginGround, cam.Zoom, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, depth);
            sb.Flush();
        }
    }
}
