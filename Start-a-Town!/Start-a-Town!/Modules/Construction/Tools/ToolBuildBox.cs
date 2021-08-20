using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class ToolBuildBox : ToolBuildWithHeight
    {
        public ToolBuildBox()
        {

        }
        public ToolBuildBox(Action<Args> callback)
            : base(callback)
        {
        }
        
        protected override void DrawGrid(MySpriteBatch sb, MapBase map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;
            var end = this.End + IntVec3.UnitZ * this.Height;

            var box = this.Begin.GetBox(end);

            cam.DrawCellHighlights(sb, Block.BlockBlueprint, box, color);
        }
        public override IEnumerable<IntVec3> GetPositions()
        {
            foreach (var i in GetPositions(this.Begin, this.TopCorner))
                yield return i;
        }
        static public List<IntVec3> GetPositions(IntVec3 a, IntVec3 b)
        {
            VectorHelper.GetMinMaxVector3(a, b, out IntVec3 min, out IntVec3 max);
            var dx = max.X - min.X;
            var dy = max.Y - min.Y;
            var dz = max.Z - min.Z;
            if (dx <= 1 || dy <= 1 || dz <= 1)
                return min.GetBox(max);
            else
                return min.GetBox(max).Except((min + IntVec3.One).GetBox(max - IntVec3.One)).ToList();
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
    }
}
