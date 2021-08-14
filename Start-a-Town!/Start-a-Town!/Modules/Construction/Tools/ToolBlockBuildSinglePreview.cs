using System;
using System.Collections.Generic;
using System.Windows.Forms;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Modules.Construction
{
    class ToolDrawingSinglePreview : ToolBlockBuild
    {
        public override string Name { get; } = "Single";
        public override Modes Mode { get; } = Modes.Single; 
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
            this.Block = blockGetter();
        }
        
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.Enabled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            this.Send(Modes.Single, this.Begin, this.Begin, this.Orientation);
            this.Enabled = false;
            return Messages.Default;
        }
       
        protected override void DrawGrid(MySpriteBatch sb, MapBase map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;
            cam.DrawGridCell(sb, this.Valid ? Color.Lime : Color.Red, this.Begin);
        }
        static public List<Vector3> GetPositions(Vector3 a, Vector3 b)
        {
            return new List<Vector3>() { a };
        }
        internal override void DrawAfterWorld(MySpriteBatch sb, MapBase map)
        {
            var cam = map.Camera;
            if (this.Target == null && !this.Enabled)
                return;

            var atlastoken = this.Block.GetDefault();
            var global = this.Enabled ? this.Begin : (IntVec3)this.Target.FaceGlobal;
            atlastoken.Atlas.Begin(sb);
            this.Block.DrawPreview(sb, map, global, cam, this.State, this.Material, this.Variation, this.Orientation);
            sb.Flush();

            // show operation position of workstation 
            cam.DrawGridCells(sb, Color.White *.5f, new IntVec3[] { global + Cell.GetFront(this.Orientation) });
        }
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, Net.PlayerData player)
        {
            this.DrawAfterWorld(sb, map);
        }

        protected override void WriteData(System.IO.BinaryWriter w)
        {
            w.Write(this.Block);
        }
        protected override void ReadData(System.IO.BinaryReader r)
        {
            this.Block = r.ReadBlock();
        }
        internal override void RotateAntiClockwise()
        {
            this.Orientation -= 1;
            if (this.Orientation < 0)
                this.Orientation = 3;
        }

        internal override void RotateClockwise()
        {
            this.Orientation = (this.Orientation + 1) % 4;
        }
    }
}
