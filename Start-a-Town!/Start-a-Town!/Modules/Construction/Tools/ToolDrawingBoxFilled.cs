using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Modules.Construction
{
    class ToolDrawingBoxFilled : ToolDrawingWithHeight
    {
        public override string Name => "Box Filled";
        public ToolDrawingBoxFilled()
        {

        }
        public ToolDrawingBoxFilled(Action<Args> callback)
            : base(callback)
        {
        }
        
        protected override void DrawGrid(MySpriteBatch sb, MapBase map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;
            var end = this.End + Vector3.UnitZ * this.Height;

            var box = this.Begin.GetBox(end);

            cam.DrawGridBlocks(sb, Block.BlockBlueprint, box, color);
        }
        public override List<Vector3> GetPositions()
        {
            return GetPositions(this.Begin, this.TopCorner);
        }
        static public List<Vector3> GetPositions(Vector3 a, Vector3 b)
        {
            var box = a.GetBox(b);
            return box;
        }
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, MapBase map, Camera camera, Net.PlayerData player)
        {
            this.DrawGrid(sb, map, camera, Color.Red);
        }
        protected override void WriteData(System.IO.BinaryWriter w)
        {
            base.WriteData(w);
            w.Write(this.SettingHeight);
            w.Write(this.Height);
        }
        protected override void ReadData(System.IO.BinaryReader r)
        {
            base.ReadData(r);
            this.SettingHeight = r.ReadBoolean();
            this.Height = r.ReadInt32();
        }

        public override ToolDrawing.Modes Mode
        {
            get { return Modes.BoxFilled; }
        }
    }
}
