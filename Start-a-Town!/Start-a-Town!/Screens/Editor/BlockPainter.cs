using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Editor
{
    class BlockPainter : ControlTool
    {
        //Map Map { get; set; }
        Block Block { get; set; }
        byte State { get; set; }
        int Variation { get; set; }
        Vector3 Plane { get; set; }
        int Slice { get; set; }
        bool Painting { get; set; }
        Vector3 LastGlobal { get; set; }
        Random Random { get; set; }
        public BlockPainter (Block block, byte state)
        {
            //this.Map = map;
            this.State = state;
            this.Random = new Random();
            this.Block = block;
            this.Variation = new Random().Next(this.Block.Variations.Count);
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Target == null)
                return Messages.Default;
            this.LastGlobal = this.Target.Global + this.Target.Face;
            this.Painting = true;
        //    this.Face = this.Target.Face;
            this.Plane = this.Target.Face * (this.LastGlobal);
            this.Slice = (int)this.Plane.Length();

            //var global = this.Target.Global + (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey) ? Vector3.Zero : this.Target.Face);
            //Block block = InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey) ? Block.Air : this.Block;
            //Client.PlayerSetBlock(global, block.Type);
            //this.Variation = this.Random.Next(block.Variations.Count);
            this.Paint();
            return ControlTool.Messages.Default;

            //return base.MouseLeftPressed(e);
        }
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.Painting = false;
            return base.MouseLeftUp(e);
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return ControlTool.Messages.Remove;
        }
        public override void Update()
        {
            //TargetOld = Controller.Instance.Mouseover.Object as GameObject;

            //Face = InputState.IsKeyDown(Keys.RShiftKey) ? Vector3.Forward : Controller.Instance.Mouseover.Face;

            //Face = InputState.IsKeyDown(Keys.ShiftKey) ? Vector3.Zero : Controller.Instance.Mouseover.Face;
            //Precise = Controller.Instance.Mouseover.Precise;

            this.Target = Controller.Instance.MouseoverBlock.Target;
            if (this.Target == null)
                return;
            //base.Update();
            if (!this.Painting)
                return;
            
            
            Vector3 nextGlobal = this.Target.Global + this.Target.Face;
            if (this.LastGlobal == nextGlobal)
                return;
            Vector3 normal = this.Plane;
            normal.Normalize();
            if (this.Target.Face != normal)
                return;
    
            if (normal * nextGlobal - this.Plane != Vector3.Zero)
                return;

            this.Paint();
        }
        void Paint()
        {
            var global = this.Target.Global + ((InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey) || InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey)) ? Vector3.Zero : this.Target.Face);
            Block block = InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey) ? BlockDefOf.Air : this.Block;
            byte state = InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey) ? (byte)0 : this.State;
            //Client.PlayerSetBlock(global, block.Type, state);
            //Engine.Map.SetCell(global, block.Type, state, this.Variation);
            Engine.Map.SetBlock(global, block.Type, state, this.Variation);

            this.Variation = this.Random.Next(block.Variations.Count);
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera cam)
        {
            base.DrawBeforeWorld(sb, map, cam);
            if (this.Painting)
                return;
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            {
                //Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
                return;
            }
            //if (this.TargetOld.IsNull())
            //    return;
            //if (!this.TargetOld.Exists)
            //    return;
            if (this.Target == null)
                return;
            var atlastoken = this.Block.Variations.First();
            //var global = this.TargetOld.Global + this.Face;
            var global = this.Target.FaceGlobal;

            var pos = cam.GetScreenPosition(global);
            var depth = global.GetDrawDepth(Engine.Map, cam);
            //Sprite sprite = Sprite.Default;
            //sprite.Draw(sb, pos, Color.White * 0.5f, 0, sprite.Origin, cam.Zoom, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, global.GetDrawDepth(Engine.Map, cam));
            Game1.Instance.GraphicsDevice.Textures[0] = atlastoken.Atlas.Texture;
            Game1.Instance.GraphicsDevice.Textures[1] = atlastoken.Atlas.DepthTexture;
            //sb.Draw(atlastoken.Atlas.Texture, pos, atlastoken.Rectangle, 0, Block.OriginCenter, cam.Zoom, Color.White * 0.5f, SpriteEffects.None, depth);
            var cell = new Cell
            {
                Variation = (byte)this.Variation,// new Random().Next(bl.Variations.Count);
                                                 //cell.Type = this.Block.Type;
                Block = this.Block,
                BlockData = this.State
            };
            this.Block.Draw(sb, pos - Block.OriginCenter * cam.Zoom, Color.White, Vector4.One, Color.White * 0.5f, cam.Zoom, depth, cell);
            sb.Flush();
        }
                    //tool.LeftClick = (target, face) =>
                    //{
                    //    var global = tool.TargetOld.Global + (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey) ? Vector3.Zero : tool.Face);
                    //    Block block = InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey) ? Block.Air : bl;
                    //    Client.PlayerSetBlock(global, block.Type);
                    //    vari = new Random().Next(bl.Variations.Count);
                    //    return ControlTool.Messages.Default;
                    //};
                    //tool.DrawActionMy = (sb, cam) =>
                    //{
                    //    if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
                    //    {
                    //        Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
                    //        return;
                    //    }
                    //    if (tool.TargetOld.IsNull())
                    //        return;
                    //    if (!tool.TargetOld.Exists)
                    //        return;
                    //    var atlastoken = bl.Variations.First();
                    //    var global = tool.TargetOld.Global + tool.Face;
                    //    var pos = cam.GetScreenBounds(global);
                    //    var depth = global.GetDrawDepth(Engine.Map, cam);
                    //    //Sprite sprite = Sprite.Default;
                    //    //sprite.Draw(sb, pos, Color.White * 0.5f, 0, sprite.Origin, cam.Zoom, Microsoft.Xna.Framework.Graphics.SpriteEffects.None, global.GetDrawDepth(Engine.Map, cam));
                    //    Game1.Instance.GraphicsDevice.Textures[0] = atlastoken.Atlas.Texture;
                    //    //sb.Draw(atlastoken.Atlas.Texture, pos, atlastoken.Rectangle, 0, Block.OriginCenter, cam.Zoom, Color.White * 0.5f, SpriteEffects.None, depth);
                    //    Cell cell = new Cell();
                    //    cell.Variation = vari;// new Random().Next(bl.Variations.Count);
                    //    cell.Type = bl.Type;
                    //    bl.Draw(sb, pos - Block.OriginCenter * cam.Zoom, Color.White, Vector4.One, Color.White * 0.5f, cam.Zoom, depth, cell);
                    //    sb.Flush();
                    //};
    }
}
