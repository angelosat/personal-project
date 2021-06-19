using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_.Modules.Construction
{
    class PlaceBlockConstructionTool : DefaultTool// ControlTool
    {
        Action<BlockConstruction.ProductMaterialPair, TargetArgs, bool, bool> Callback;
        BlockConstruction.ProductMaterialPair Item;
        Cell CurrentCell;
        bool Valid;

        public PlaceBlockConstructionTool(BlockConstruction.ProductMaterialPair item, Action<BlockConstruction.ProductMaterialPair, TargetArgs, bool, bool> callback)
        {
            this.Callback = callback;
            this.Item = item;
            //this.MouseMove = false;
        }

        protected override void OnTargetChanged()
        {
            this.CurrentCell = Net.Client.Instance.Map.GetCell(this.Target.Global);
            //var targetposition = this.CurrentCell.IsSolid() ? this.Target.FaceGlobal : this.Target.Global; 
            var targetposition = this.Target.FaceGlobal; // the construction designation block is non-solid, so we don't want to exclude non-solid in selecting target block face
            this.Valid = Net.Client.Instance.Map.IsEmpty(targetposition);
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Default;
        }
       
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            var target = this.Target;
            if (target.IsNull())
                return ControlTool.Messages.Default;
            if (IsRemoving())
            {
                //var entity = target.Object;
                //if (entity == null)
                //    return ControlTool.Messages.Default;
                //if (entity.HasComponent<Components.ConstructionComponent>())
                //{
                //    Net.Client.RemoveObject(entity);
                //    return ControlTool.Messages.Default;
                //}
                //return ControlTool.Messages.Default;
            }
            var cell = this.CurrentCell;// Net.Client.Instance.Map.GetCell(target.Global);
            //var targetposition = this.CurrentCell.IsSolid() ? this.Target.FaceGlobal : this.Target.Global; 
            var targetposition = this.Target.FaceGlobal; // the construction designation block is non-solid, so we don't want to exclude non-solid in selecting target block face
            this.Valid = Net.Client.Instance.Map.IsEmpty(targetposition);
            if (this.Valid)
                this.Callback(this.Item, this.Target, IsDesignating(), IsRemoving());
            else
                Net.Client.Console.Write("Invalid build location!");
            //Instance.PlayerConstruct(item, targetposition);
            return ControlTool.Messages.Default;
        }

        private bool IsEmpty(Vector3 targetposition)
        {
            throw new NotImplementedException();
        }

        public override ControlTool.Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }

        //internal override void DrawBeforeWorld(MySpriteBatch sb, GameModes.IMap map, Camera cam)
        internal override void DrawAfterWorld(MySpriteBatch sb, GameModes.IMap map, Camera cam)
        {
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            {
                Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
                return;
            }
            //if (tool.TargetOld.IsNull())
            //    return;
            //if (!tool.TargetOld.Exists)
            //    return;
            if (this.Target == null)
                return;
            //base.DrawBeforeWorld(sb, map, cam);
            base.DrawAfterWorld(sb, map, cam);

            var atlastoken = this.Item.Product.Variations.First();

            var global = this.Target.FaceGlobal;
            //var global = this.CurrentCell.IsSolid() ? this.Target.FaceGlobal : this.Target.Global;
            var pos = cam.GetScreenPosition(global);
            var depth = global.GetDrawDepth(Engine.Map, cam);
            var gd = Game1.Instance.GraphicsDevice;
            //gd.Textures[0] = atlastoken.Atlas.Texture;
            //gd.Textures[0] = Block.Atlas.Texture;
            //gd.Textures[2] = Block.Atlas.NormalTexture;
            //gd.Textures[3] = Block.Atlas.DepthTexture;

            gd.Textures[0] = Block.Atlas.Texture;
            gd.Textures[1] = Block.Atlas.DepthTexture;

            Cell cell = new Cell();
            //cell.Variation = this.Variation;
            cell.BlockData = this.Item.Data;
            //cell.Type = item.Product.Type;
            cell.SetBlockType(this.Item.Product.Type);
            //this.Item.Product.Draw(sb, pos - Block.OriginCenter * cam.Zoom, Color.White, Vector4.One, Color.White, cam.Zoom, depth, cell);
            var color = this.Valid ? Color.Lime : Color.Red;
            this.Item.Product.Draw(cam, global, color, Vector4.One, Color.Transparent, color * 0.5f, depth, cell);

            //Game1.Instance.GraphicsDevice.Textures[0] = atlastoken.Atlas.Texture;
            //Game1.Instance.GraphicsDevice.Textures[1] = atlastoken.Atlas.DepthTexture;
            //this.Item.Product.DrawPreview(sb, map, global, cam, this.Item.Data, 0);
            sb.Flush();
        }

        internal override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        {
            base.DrawUI(sb, camera);
            if (IsDesignating())
                Icon.Replace.Draw(sb);
            else if(IsRemoving())
                Icon.Cross.Draw(sb);
        }

        private static bool IsDesignating()
        {
            return InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey);
        }
        private static bool IsRemoving()
        {
            return InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey);
        }
    }
}
