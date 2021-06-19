using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Rooms;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.PlayerControl
{
    public class StructureTool :ControlTool
    {
        Reaction.Product.ProductMaterialPair Product { get; set; }
        public StructureTool(Reaction.Product.ProductMaterialPair product)
        {
            this.Product = product;
        }
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Target.IsNull())
                return ControlTool.Messages.Default;
            if(this.Target.Type == TargetType.Entity)
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                {
                    if (this.Target.Object.HasComponent<Components.StructureComponent>())
                    {
                        Net.Client.RemoveObject(this.Target.Object);
                        return ControlTool.Messages.Default;
                    }
                    return ControlTool.Messages.Default;
                }
            Net.Client.PlayerBuild(Product, this.Target.Global + this.Target.Face + (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : this.Target.Precise));
            return ControlTool.Messages.Default;
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return ControlTool.Messages.Remove;
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera cam)
        {
            base.DrawBeforeWorld(sb, map, cam);
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            {
                Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
                return;
            }
            if (this.Target.IsNull())
                return;
            if (this.Target.Type != TargetType.Position)
                return;

            var sprite = this.Product.Product.GetSprite();
            var global = this.Target.Global + this.Target.Face + (InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey) ? Vector3.Zero : this.Target.Precise);
            var pos = cam.GetScreenPosition(global);
            sprite.Draw(sb, pos, Color.White * 0.5f, 0, sprite.Origin, cam.Zoom, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, global.GetDrawDepth(Engine.Map, cam));
        }
    }
}
