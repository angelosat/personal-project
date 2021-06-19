using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Modules.Construction
{
    public class ToolPlaceWall : DefaultTool// ControlTool
    {
        public enum Modes { Wall, Single }
        public Modes Mode;
        public class Args
        {
            public Vector3 Begin, End;
            public bool ModifierKey;
            public Args(Vector3 begin, Vector3 end, bool modkey)
            {
                this.Begin = begin;
                this.End = end;
                this.ModifierKey = modkey;
            }
            public void Write(BinaryWriter w)
            {
                w.Write(this.Begin);
                w.Write(this.End);
                w.Write(this.ModifierKey);
            }
            public Args(BinaryReader r)
            {
                this.Begin = r.ReadVector3();
                this.End = r.ReadVector3();
                this.ModifierKey = r.ReadBoolean();
            }
        }
        //Action<BlockConstruction.ProductMaterialPair, TargetArgs, int, bool, bool> Callback;
        Action<Args> Callback;
        bool Enabled, Valid, SettingHeight;
        Vector3 Begin, End, Axis;
        int
            Length,
            Height;

        public ToolPlaceWall(Action<Args> callback)
        {
            this.Callback = callback;
        }

        private void CheckValidity()
        {
            var targetposition = this.Target.FaceGlobal; // the construction designation block is non-solid, so we don't want to exclude non-solid in selecting target block face
            this.Valid = true;
            //foreach(var p in this.Item.Block.GetParts(targetposition, this.Orientation))
            //    this.Valid &= Client.Instance.Map.IsEmpty(p.Key);
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if(this.SettingHeight)
            {
                //this.Callback(this.Begin, this.End + Vector3.UnitZ * this.Height);
                var args = new Args(this.Begin, this.End + Vector3.UnitZ * this.Height, InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey));
                this.Callback(args);
                this.SettingHeight = false;
                this.Enabled = false;
                return Messages.Default;
            }
            if (this.Enabled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            if (this.Target.Type != TargetType.Position)
                return Messages.Default;
            var pos = this.Target.FaceGlobal;
            //if (this.GetZones().Contains(pos))
            //    this.Removing = true;
            this.Begin = pos;
            this.End = this.Begin;
            this.Length = 1;
            this.Height = 0;
            this.Enabled = true;
            //this.Valid = this.Check(this.Width, this.Height);
            return Messages.Default;
        }

        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            var target = this.Target;
            if (target == null)
                return ControlTool.Messages.Default;
            if (IsRemoving())
            {

            }
            //var cell = this.CurrentCell;
            CheckValidity();
            //if (this.Valid)
            //    this.Callback(this.Item, this.Target, this.Orientation, IsDesignating(), IsRemoving());
            //else
            //    Client.Console.Write("Invalid build location!");
            
            if(this.Mode == Modes.Single)
            {
                var args = new Args(this.Begin, this.End + Vector3.UnitZ * this.Height, InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey));
                this.Callback(args);
                this.Enabled = false;
                return ControlTool.Messages.Default;
            }
            //this.Enabled = false;
            if (this.SettingHeight)
            {
                this.Enabled = false;
                this.SettingHeight = false;
            }
            else
                this.SettingHeight = this.Enabled;
            return ControlTool.Messages.Default;
        }

        public override void Update()
        {
            base.Update();
            if (!Enabled)
                return;
            if (this.Target == null)
                return;
            if (this.Target.Type != TargetType.Position)
                return;
            if(this.SettingHeight)
            {
                this.SetHeight();
                return;
            }
            if (this.Mode == Modes.Single)
                return;
            var end = this.Target.Global;// *(Vector3.One - this.Plane) + this.Begin * this.Plane;
            var dx = end.X - this.Begin.X;
            var adx = Math.Abs(dx);
            var dy = end.Y - this.Begin.Y;
            var ady = Math.Abs(dy);
            if (adx > ady)
                this.Axis = Vector3.UnitX + Vector3.UnitZ;
            else if (ady > adx)
                this.Axis = Vector3.UnitY + Vector3.UnitZ;

            //this.End = end * this.Axis;
            this.End = this.Begin + new Vector3(dx * this.Axis.X, dy * this.Axis.Y, 0);

            //var w = (int)Math.Abs(this.Target.Global.X - this.Begin.X) + 1;
            //var h = (int)Math.Abs(this.Target.Global.Y - this.Begin.Y) + 1;
            //if (w != this.Width || h != this.Height)
            //    this.Valid = this.Check(w, h);
            //this.Width = w;
            //this.Height = h;
        }

        private void SetHeight()
        {
            var endscreenposition = ScreenManager.CurrentScreen.Camera.GetScreenPosition(this.End);
            var currentposition = UIManager.Mouse;
            var length = Math.Max(0, endscreenposition.Y - currentposition.Y) / ScreenManager.CurrentScreen.Camera.Zoom;
            var lengthinblocks = (int)(length / Block.BlockHeight);
            this.Height = lengthinblocks;
        }

        public override ControlTool.Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera camera)
        {
            Game1.Instance.GraphicsDevice.Textures[0] = Sprite.Atlas.Texture;
            Game1.Instance.GraphicsDevice.Textures[1] = Sprite.Atlas.DepthTexture;
            this.DrawGrid(sb, camera);

            //foreach (var g in this.GetZones())
            //    this.DrawGridCell(sb, camera, Color.Yellow, g);

            base.DrawBeforeWorld(sb, map, camera);
        }
        void DrawGrid(MySpriteBatch sb, Camera cam)
        {
            if (!this.Enabled)
                return;
            var end = this.End + Vector3.UnitZ * this.Height;
            var col = this.Valid ? Color.Lime : Color.Red;
            int x = (int)Math.Min(this.Begin.X, end.X);
            int y = (int)Math.Min(this.Begin.Y, end.Y);
            int z = (int)Math.Min(this.Begin.Z, end.Z);

            int dx = (int)Math.Abs(this.Begin.X - end.X);
            int dy = (int)Math.Abs(this.Begin.Y - end.Y);
            int dz = (int)Math.Abs(this.Begin.Z - end.Z);

            var minBegin = new Vector3(x, y, z);
            for (int i = 0; i <= dx; i++)
            {
                for (int j = 0; j <= dy; j++)
                {
                    for (int k = 0; k <= dz; k++)
                    {
                        Vector3 global = minBegin + new Vector3(i, j, k);

                        var bounds = cam.GetScreenBounds(global, Block.Bounds);
                        var pos = new Vector2(bounds.X, bounds.Y);
                        var depth = global.GetDrawDepth(Engine.Map, cam);
                        sb.Draw(Sprite.Atlas.Texture, pos, Sprite.BlockHighlight.AtlasToken.Rectangle, 0, Vector2.Zero, cam.Zoom, col * .5f, SpriteEffects.None, depth);
                    }
                }
            }
        }
        //internal override void DrawAfterWorld(MySpriteBatch sb, GameModes.IMap map, Camera cam)
        //{
        //    if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
        //    {
        //        Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
        //        return;
        //    }
        //    if (this.Target == null)
        //        return;
        //    base.DrawAfterWorld(sb, map, cam);


        //    var global = this.Target.FaceGlobal;
        //    var pos = cam.GetScreenPosition(global);
        //    var depth = global.GetDrawDepth(Engine.Map, cam);
        //    var gd = Game1.Instance.GraphicsDevice;

        //    gd.Textures[0] = Block.Atlas.Texture;
        //    gd.Textures[1] = Block.Atlas.DepthTexture;

        //    Cell cell = new Cell();
        //    cell.BlockData = this.Item.Data;
        //    cell.SetBlockType(this.Item.Block.Type);
        //    var color = this.Valid ? Color.White : Color.Red;

        //    //this.Item.Block.DrawPreview(sb, map, global, cam, color *.5f, this.Item.Data, 0, this.Orientation);

        //    sb.Flush();
        //}

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
      
        //const char RotateL = '[';
        //const char RotateR = ']';

        //public override void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        //{
        //    switch(e.KeyChar)
        //    {
        //        case RotateL:
        //            this.Orientation--;
        //            if (this.Orientation < 0)
        //                this.Orientation += 4;// this.Item.Block.Variations.Count;
        //            break;

        //        case RotateR:
        //            this.Orientation++;
        //            this.Orientation %= 4;//this.Item.Block.Variations.Count;
        //            break;

        //        default:
        //            break;
        //    }
        //}
        //internal override void GetContextActions(ContextArgs args)
        //{
        //    args.Actions.Add(new ContextAction(RotateL + ", " + RotateR + ": Rotate", null));
        //    args.Actions.Add(new ContextAction("Place construction", null) { Shortcut = PlayerInput.LButton });
        //    args.Actions.Add(new ContextAction("Cancel", null) { Shortcut = PlayerInput.RButton });
        //}
       
    }
}
