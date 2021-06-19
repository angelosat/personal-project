using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Modules.Construction
{
    class ToolDrawingSinglePreview : ToolDrawing
    {
        public override ToolDrawing.Modes Mode
        {
            get { return Modes.Single; }
        }
        Func<Block> BlockGetter;
        public override string Name
        {
            get { return "Single"; }
        }
        public ToolDrawingSinglePreview()
        {

        }
        public ToolDrawingSinglePreview(Action<Args> callback)
            : base(callback)
        {

        }
        public ToolDrawingSinglePreview(Action<Args> callback, Func<Block> blockGetter)
            : this(callback)
        {
            this.BlockGetter = blockGetter;
            this.Block = blockGetter();
        }
        //public ToolDrawingSinglePreview(Action<Args> callback, Block block)
        //    : this(callback)
        //{
        //    this.Block = block;
        //}
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.Enabled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            //var args = new Args(Modes.Single, this.Begin, this.Begin, InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey), this.Orientation);
            //this.Callback(args);
            this.Send(Modes.Single, this.Begin, this.Begin, this.Orientation);
            this.Enabled = false;
            return Messages.Default;
        }
        //public override void Update()
        //{
        //    base.Update();
        //}
        protected override void DrawGrid(MySpriteBatch sb, IMap map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;
            cam.DrawGridCell(sb, this.Valid ? Color.Lime : Color.Red, this.Begin);
        }
        static public List<Vector3> GetPositions(Vector3 a, Vector3 b)
        {
            return new List<Vector3>() { a };
        }
        internal override void DrawAfterWorld(MySpriteBatch sb, IMap map)
        {
            //base.DrawAfterWorld(sb, map, cam);
            var cam = map.Camera;
            if (this.Target == null && !this.Enabled)
                return;

            var atlastoken = this.Block.GetDefault();// this.Block.Variations.First();
            var global = this.Enabled ? this.Begin : this.Target.FaceGlobal;
            atlastoken.Atlas.Begin(sb);
            this.Block.DrawPreview(sb, map, global, cam, this.State, this.Variation, this.Orientation);
            sb.Flush();

            // show operation position of workstation 
            cam.DrawGridCells(sb, Color.White *.5f, new Vector3[] { global + Block.GetFrontSide(this.Orientation) });
        }
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, IMap map, Camera camera, Net.PlayerData player)
        {
            this.DrawAfterWorld(sb, map);
        }
        public override void HandleKeyPress(KeyPressEventArgs e)
        {
            if (e.Handled)
                return;
            switch (e.KeyChar)
            {
                //case '[':
                case 'e':
                    this.Orientation = (this.Orientation + 1) % 4;
                    e.Handled = true;
                    break;

                //case ']':
                case 'q':
                    this.Orientation -= 1;
                    if (this.Orientation < 0)
                        this.Orientation = 3;
                    e.Handled = true;
                    break;

                default:
                    break;
            }
        }
        protected override void WriteData(System.IO.BinaryWriter w)
        {
            w.Write((int)this.Block.Type);
        }
        protected override void ReadData(System.IO.BinaryReader r)
        {
            this.Block = Block.Registry[(Block.Types)r.ReadInt32()];
        }
    }
}
