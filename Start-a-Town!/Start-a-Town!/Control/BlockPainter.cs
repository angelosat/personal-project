﻿using System;
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
    class BlockPainter : ToolManagement// ControlTool
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
        public BlockPainter()
        {

        }
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
            this.LastPainted = new Vector3(float.MinValue); // lol wut?
            return base.MouseRightUp(e);
        }
        //public override void Update()
        //{
        //    base.Update();
        //    return;
        //    //TargetOld = Controller.Instance.Mouseover.Object as GameObject;
        //    //Face = InputState.IsKeyDown(Keys.RShiftKey) ? Vector3.Forward : Controller.Instance.Mouseover.Face;

        //    //Face = InputState.IsKeyDown(Keys.ShiftKey) ? Vector3.Zero : Controller.Instance.Mouseover.Face;
        //    //Precise = Controller.Instance.Mouseover.Precise;

        //    //this.Target = Controller.Instance.Mouseover.Target;
        //    if (this.Target == null)
        //        return;
        //    //base.Update();
        //    if (!this.Painting)
        //        return;
            
            
        //    Vector3 nextGlobal = this.Target.Global + this.Target.Face;
        //    if (this.LastGlobal == nextGlobal)
        //        return;
        //    Vector3 normal = this.Plane;
        //    normal.Normalize();
        //    if (this.Target.Face != normal)
        //        return;
    
        //    if (normal * nextGlobal - this.Plane != Vector3.Zero)
        //        return;

    
        //        this.Paint();
        //}
        void Paint()
        {
            bool isDelete = InputState.IsKeyDown(KeyRemove);//System.Windows.Forms.Keys.ControlKey);
            bool isReplace = InputState.IsKeyDown(KeyReplace);//System.Windows.Forms.Keys.ShiftKey);
            var global = this.Target.Global + ((isDelete || isReplace) ? Vector3.Zero : this.Target.Face);
            Block block = isDelete ? BlockDefOf.Air : this.Block;
            byte state = isDelete ? (byte)0 : this.State;

            if (global != this.LastPainted)
                Net.Client.PlayerSetBlock(global, block.Type, state, this.Variation, this.Orientation);
            this.LastPainted = global;

            this.Variation = this.Random.Next(block.Variations.Count);
        }
        
        public override void HandleKeyPress(KeyPressEventArgs e)
        {
            if (e.Handled)
                return;
            switch(e.KeyChar)
            {
                case 'e':// '[':
                    this.Orientation = (this.Orientation + 1) % 4;
                    break;

                case 'q':// ']':
                    this.Orientation -= 1;
                    if (this.Orientation < 0)
                        this.Orientation = 3;
                    break;

                default:
                    break;
            }
        }
        
        internal override void DrawAfterWorld(MySpriteBatch sb, IMap map)
        {
            var cam = map.Camera;
            base.DrawAfterWorld(sb, map);
            if (this.Painting)
                return;
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            {
                Vector2 loc = Controller.Instance.MouseLocation / UIManager.Scale;
                return;
            }
            if (this.Target == null)
                return;

            var atlastoken = this.Block.GetDefault();// this.Block.Variations.First();
            var global = this.Target.FaceGlobal;
            atlastoken.Atlas.Begin(sb);
            this.Block.DrawPreview(sb, map, global, cam, this.State, this.Variation, this.Orientation);
            sb.Flush();
        }
        public override Icon GetIcon()
        {
            if (InputState.IsKeyDown(KeyReplace))
                return Icon.Replace;
            if (InputState.IsKeyDown(KeyRemove))
                return Icon.Cross;
            return base.GetIcon();
        }     

        //internal override void DrawUI(SpriteBatch sb, Camera camera)
        //{
        //    base.DrawUI(sb, camera);

        //    if (InputState.IsKeyDown(KeyReplace))
        //        Icon.Replace.Draw(sb, UI.UIManager.Mouse + new Vector2(16, 0));
        //    if (InputState.IsKeyDown(KeyRemove))
        //        Icon.Cross.Draw(sb, UI.UIManager.Mouse + new Vector2(16, 0));
        //}
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, IMap map, Camera camera, Net.PlayerData player)
        {
            var targetArgs = player.Target;
            if (targetArgs.Type != TargetType.Position)
                return;
            var atlastoken = this.Block.GetDefault();// this.Block.Variations.First();
            var global = targetArgs.FaceGlobal;
            atlastoken.Atlas.Begin(sb);
            this.Block.DrawPreview(sb, map, global, camera, this.State, this.Variation, this.Orientation);
            sb.Flush();
        }
        protected override void WriteData(System.IO.BinaryWriter w)
        {
            w.Write((int)this.Block.Type);
            w.Write(this.State);
        }
        protected override void ReadData(System.IO.BinaryReader r)
        {
            this.Block = Block.Registry[(Block.Types)r.ReadInt32()];
            this.State = r.ReadByte();
        }
    }
}
