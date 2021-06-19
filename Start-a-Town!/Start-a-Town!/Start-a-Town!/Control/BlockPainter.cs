using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.PlayerControl
{
    class BlockPainter : ControlTool
    {
        Block Block { get; set; }
        byte State { get; set; }
        int Variation { get; set; }
     //   Vector3 Face { get; set; }
        Vector3 Plane { get; set; }
        int Slice { get; set; }
        bool Painting { get; set; }
        Vector3 LastGlobal { get; set; }
        Random Random { get; set; }
        int Orientation;
        Vector3 LastPainted = new Vector3(float.MinValue);
        System.Windows.Forms.Keys KeyReplace = System.Windows.Forms.Keys.ShiftKey;
        System.Windows.Forms.Keys KeyRemove = System.Windows.Forms.Keys.ControlKey;

        public BlockPainter (Block block, byte state)
        {
            this.State = state;
            this.Random = new Random();
            this.Block = block;
            this.Variation = new Random().Next(this.Block.Variations.Count);
        }

        // TODO: move this to mouse left up
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;

            if (this.Target.IsNull())
                return Messages.Default;
            this.LastGlobal = this.Target.Global + this.Target.Face;
            this.Painting = true;
        //    this.Face = this.Target.Face;
            this.Plane = this.Target.Face * (this.LastGlobal);
            this.Slice = (int)this.Plane.Length();

            //var global = this.Target.Global + (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey) ? Vector3.Zero : this.Target.Face);
            //Block block = InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey) ? Block.Air : this.Block;
            //Net.Client.PlayerSetBlock(global, block.Type);
            //this.Variation = this.Random.Next(block.Variations.Count);
            this.Paint();
            return ControlTool.Messages.Default;

            //return base.MouseLeftPressed(e);
        }
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;
            this.LastPainted = new Vector3(float.MinValue);
            this.Painting = false;
            return base.MouseLeftUp(e);
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return ControlTool.Messages.Remove;
        }
        public override ControlTool.Messages MouseRightUp(HandledMouseEventArgs e)
        {
            this.LastPainted = new Vector3(float.MinValue);
            return base.MouseRightUp(e);
        }
        public override void Update()
        {
            base.Update();
            //TargetOld = Controller.Instance.Mouseover.Object as GameObject;
            //Face = InputState.IsKeyDown(Keys.RShiftKey) ? Vector3.Forward : Controller.Instance.Mouseover.Face;

            //Face = InputState.IsKeyDown(Keys.ShiftKey) ? Vector3.Zero : Controller.Instance.Mouseover.Face;
            //Precise = Controller.Instance.Mouseover.Precise;

            //this.Target = Controller.Instance.Mouseover.Target;
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
            bool isDelete = InputState.IsKeyDown(KeyRemove);//System.Windows.Forms.Keys.ControlKey);
            bool isReplace = InputState.IsKeyDown(KeyReplace);//System.Windows.Forms.Keys.ShiftKey);
            var global = this.Target.Global + ((isDelete || isReplace) ? Vector3.Zero : this.Target.Face);
            Block block = isDelete ? Block.Air : this.Block;
            byte state = isDelete ? (byte)0 : this.State;

            if (global != this.LastPainted)
                Net.Client.PlayerSetBlock(global, block.Type, state, this.Variation, this.Orientation);
            this.LastPainted = global;

            this.Variation = this.Random.Next(block.Variations.Count);
        }
        public override void HandleKeyUp(KeyEventArgs e)
        {
            if (e.Handled)
                return;
            switch (e.KeyCode)
            {
                case Keys.OemOpenBrackets:
                    this.Orientation = (this.Orientation + 1) % 4;
                    break;

                case Keys.OemCloseBrackets:
                    this.Orientation -= 1;
                    if (this.Orientation < 0)
                        this.Orientation = 3;
                    break;
                    
                default:
                    break;
            }
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera cam)
        {
            base.DrawBeforeWorld(sb, map, cam);
            if (this.Painting)
                return;
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            {
                Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
                return;
            }
            if (this.Target == null)
                return;

            var atlastoken = this.Block.Variations.First();
            var global = this.Target.FaceGlobal;
            Game1.Instance.GraphicsDevice.Textures[0] = atlastoken.Atlas.Texture;
            Game1.Instance.GraphicsDevice.Textures[1] = atlastoken.Atlas.DepthTexture;
            //Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            //fx.CurrentTechnique = fx.Techniques["Combined"];
            //fx.CurrentTechnique.Passes["Pass1"].Apply();
            this.Block.DrawPreview(sb, map, global, cam, this.State, this.Orientation);
            sb.Flush();

            //var atlastoken = this.Block.Variations.First();
            //var global = this.Target.FaceGlobal;
            //var pos = cam.GetScreenPosition(global);
            //var depth = global.GetDrawDepth(Engine.Map, cam);
            //Game1.Instance.GraphicsDevice.Textures[0] = atlastoken.Atlas.Texture;
            //Game1.Instance.GraphicsDevice.Textures[1] = atlastoken.Atlas.DepthTexture;
            //Cell cell = new Cell();
            //cell.Variation = (byte)this.Variation;
            //cell.Block = this.Block;
            //cell.BlockData = this.State;
            //this.Block.Draw(sb, pos - Block.OriginCenter * cam.Zoom, Color.White, Vector4.One, Color.White * 0.5f, cam.Zoom, depth, cell);
            //sb.Flush();
        }
                    //tool.LeftClick = (target, face) =>
                    //{
                    //    var global = tool.TargetOld.Global + (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey) ? Vector3.Zero : tool.Face);
                    //    Block block = InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey) ? Block.Air : bl;
                    //    Net.Client.PlayerSetBlock(global, block.Type);
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

        internal override void DrawUI(SpriteBatch sb, Camera camera)
        {
            base.DrawUI(sb, camera);

            if (InputState.IsKeyDown(KeyReplace))
                Icon.Replace.Draw(sb, UI.UIManager.Mouse + new Vector2(16, 0));
            if (InputState.IsKeyDown(KeyRemove))
                Icon.Cross.Draw(sb, UI.UIManager.Mouse + new Vector2(16, 0));
        }
    }
}
