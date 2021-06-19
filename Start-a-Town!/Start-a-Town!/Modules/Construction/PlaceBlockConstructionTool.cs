using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Net;

namespace Start_a_Town_.Modules.Construction
{
    class PlaceBlockConstructionTool : DefaultTool// ControlTool
    {
        Action<BlockRecipe.ProductMaterialPair, TargetArgs, int, bool, bool> Callback;
        BlockRecipe.ProductMaterialPair Item;
        Cell CurrentCell;
        bool Valid;
        int Orientation;

        public PlaceBlockConstructionTool(BlockRecipe.ProductMaterialPair item, Action<BlockRecipe.ProductMaterialPair, TargetArgs, int, bool, bool> callback)
        {
            this.Callback = callback;
            this.Item = item;
            //this.MouseMove = false;
        }

        protected override void OnTargetChanged()
        {
            this.CurrentCell = Client.Instance.Map.GetCell(this.Target.Global);
            //var targetposition = this.CurrentCell.IsSolid() ? this.Target.FaceGlobal : this.Target.Global; 
            CheckValidity();
        }

        private void CheckValidity()
        {
            var targetposition = this.Target.FaceGlobal; // the construction designation block is non-solid, so we don't want to exclude non-solid in selecting target block face
            this.Valid = true;
            foreach(var p in this.Item.Block.GetParts(targetposition, this.Orientation))
                this.Valid &= Client.Instance.Map.IsEmpty(p.Key);
            //this.Valid = Client.Instance.Map.IsEmpty(targetposition);
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Default;
        }
       
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            var target = this.Target;
            if (target == null)
                return ControlTool.Messages.Default;
            if (IsRemoving())
            {
                //var entity = target.Object;
                //if (entity == null)
                //    return ControlTool.Messages.Default;
                //if (entity.HasComponent<Components.ConstructionComponent>())
                //{
                //    Client.RemoveObject(entity);
                //    return ControlTool.Messages.Default;
                //}
                //return ControlTool.Messages.Default;
            }
            var cell = this.CurrentCell;// Client.Instance.Map.GetCell(target.Global);
            //var targetposition = this.CurrentCell.IsSolid() ? this.Target.FaceGlobal : this.Target.Global; 
            //var targetposition = this.Target.FaceGlobal; // the construction designation block is non-solid, so we don't want to exclude non-solid in selecting target block face
            //this.Valid = Client.Instance.Map.IsEmpty(targetposition);
            CheckValidity();
            if (this.Valid)
                this.Callback(this.Item, this.Target, this.Orientation, IsDesignating(), IsRemoving());
            else
                Client.Instance.Log.Write("Invalid build location!");
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
        internal override void DrawAfterWorld(MySpriteBatch sb, IMap map)
        {
            var cam = map.Camera;
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            {
                Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
                return;
            }
            if (this.Target == null)
                return;
            base.DrawAfterWorld(sb, map);

            //var atlastoken = this.Item.Block.Variations.First();

            var global = this.Target.FaceGlobal;
            var pos = cam.GetScreenPosition(global);
            var depth = global.GetDrawDepth(Engine.Map, cam);
            var gd = Game1.Instance.GraphicsDevice;

            gd.Textures[0] = Block.Atlas.Texture;
            gd.Textures[1] = Block.Atlas.DepthTexture;

            Cell cell = new Cell();
            cell.BlockData = this.Item.Data;
            cell.SetBlockType(this.Item.Block.Type);
            //var color = this.Valid ? Color.Lime : Color.Red;
            var color = this.Valid ? Color.White : Color.Red;

            //this.Item.Block.Draw(cam, global, color, Vector4.One, Color.Transparent, color * 0.5f, depth, this.Variation);// cell);
            this.Item.Block.DrawPreview(sb, map, global, cam, color *.5f, this.Item.Data, 0, this.Orientation);

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
      
        const char RotateL = '[';
        const char RotateR = ']';
        public override void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            switch(e.KeyChar)
            {
                case RotateL:
                    this.Orientation--;
                    if (this.Orientation < 0)
                        this.Orientation += 4;// this.Item.Block.Variations.Count;
                    break;

                case RotateR:
                    this.Orientation++;
                    this.Orientation %= 4;//this.Item.Block.Variations.Count;
                    break;

                default:
                    break;
            }
        }
        internal override void GetContextActions(ContextArgs args)
        {
            args.Actions.Add(new ContextAction(RotateL + ", " + RotateR + ": Rotate", null));
            args.Actions.Add(new ContextAction("Place construction", null) { Shortcut = PlayerInput.LButton });
            args.Actions.Add(new ContextAction("Cancel", null) { Shortcut = PlayerInput.RButton });
        }
    }
}
