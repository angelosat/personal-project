using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Modules.Construction
{
    class ToolDrawingWall : ToolDrawingWithHeight
    {
        public override string Name => "Wall"; 
        public override Modes Mode { get { return Modes.Wall; } }
        public ToolDrawingWall()
        {

        }
        public ToolDrawingWall(Action<Args> callback)
            : base(callback)
        {
        }
        
        protected override Vector3 GetBottomCorner()
        {
            return GetBottomCorner(this.Begin, this.Target.Global);
        }
        private static Vector3 GetBottomCorner(Vector3 a, Vector3 b)
        {
            var end = b;
            var dx = end.X - a.X;
            var adx = Math.Abs(dx);
            var dy = end.Y - a.Y;
            var ady = Math.Abs(dy);
            Vector3 axis;
            if (adx > ady)
                axis = Vector3.UnitX + Vector3.UnitZ;
            else
                axis = Vector3.UnitY + Vector3.UnitZ;
            return a + new Vector3(dx * axis.X, dy * axis.Y, 0);
        }

        static Vector3 GetEnd(Vector3 begin, Vector3 target)
        {
            var end = target;
            var dx = end.X - begin.X;
            var adx = Math.Abs(dx);
            var dy = end.Y - begin.Y;
            var ady = Math.Abs(dy);
            Vector3 axis;
            if (adx > ady)
                axis = Vector3.UnitX + Vector3.UnitZ;
            else
                axis = Vector3.UnitY + Vector3.UnitZ;

            return begin + new Vector3(dx * axis.X, dy * axis.Y, 0);
        }

        public override List<Vector3> GetPositions()
        {
            return this.Begin.GetBox(this.TopCorner);
        }
        protected override IEnumerable<Vector3> GetPositionsNew(Vector3 a, Vector3 b)
        {
            var end = b + Vector3.UnitZ * this.Height;
            var box = a.GetBox(end);
            return box;
        }
 
        static public List<Vector3> GetPositions(Vector3 a, Vector3 b)
        {
            Vector3 axis;
            var end = b;
            var dx = end.X - a.X;
            var adx = Math.Abs(dx);
            var dy = end.Y - a.Y;
            var ady = Math.Abs(dy);
            var dz = end.Z - a.Z;
            if (adx > ady)
                axis = Vector3.UnitX + Vector3.UnitZ;
            else
                axis = Vector3.UnitY + Vector3.UnitZ;

            var bb = a + new Vector3(dx * axis.X, dy * axis.Y, dz);
            var box = a.GetBox(bb);
            return box;
        }
       
        protected override void WriteData(System.IO.BinaryWriter w)
        {
            base.WriteData(w);
            w.Write(this.Axis);
        }
        protected override void ReadData(System.IO.BinaryReader r)
        {
            base.ReadData(r);
            this.Axis = r.ReadVector3();
        }
    }
}
