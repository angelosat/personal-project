using System;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.PlayerControl
{
    class BlockPainter : ToolManagement
    {
        Block Block;
        byte State;
        int Variation;
        bool Painting;
        readonly Random Random;
        int Orientation;
        Vector3 LastPainted = new Vector3(float.MinValue);
        Keys KeyReplace = Keys.ShiftKey;
        Keys KeyRemove = Keys.ControlKey;
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
            this.Painting = true;
            this.Paint();
            return ControlTool.Messages.Default;
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
        
        void Paint()
        {
            bool isDelete = InputState.IsKeyDown(KeyRemove);
            bool isReplace = InputState.IsKeyDown(KeyReplace);
            var global = this.Target.Global + ((isDelete || isReplace) ? Vector3.Zero : this.Target.Face);
            var block = isDelete ? BlockDefOf.Air : this.Block;
            byte state = isDelete ? (byte)0 : this.State;

            if (global != this.LastPainted)
                PacketPlayerSetBlock.Send(Client.Instance, Client.Instance.GetPlayer(), global, block, state, this.Variation, this.Orientation);
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
        
        internal override void DrawAfterWorld(MySpriteBatch sb, MapBase map)
        {
            var cam = map.Camera;
            base.DrawAfterWorld(sb, map);
            if (this.Painting)
                return;
            if (InputState.IsKeyDown(Keys.ControlKey))
                return;
            if (this.Target is null)
                return;

            var atlastoken = this.Block.GetDefault();
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

        internal override void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, Net.PlayerData player)
        {
            var targetArgs = player.Target;
            if (targetArgs.Type != TargetType.Position)
                return;
            var atlastoken = this.Block.GetDefault();
            var global = targetArgs.FaceGlobal;
            atlastoken.Atlas.Begin(sb);
            this.Block.DrawPreview(sb, map, global, camera, this.State, this.Variation, this.Orientation);
            sb.Flush();
        }
        protected override void WriteData(System.IO.BinaryWriter w)
        {
            w.Write(this.Block.Hash);
            w.Write(this.State);
        }
        protected override void ReadData(System.IO.BinaryReader r)
        {
            this.Block = Block.Registry[r.ReadInt32()];
            this.State = r.ReadByte();
        }
    }
}
