﻿using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    class ToolBuildWall : ToolBuildWithHeight
    {
        public ToolBuildWall()
        {

        }
        public ToolBuildWall(Action<Args> callback)
            : base(callback)
        {
        }

        protected override IntVec3 GetBottomCorner()
        {
            return GetBottomCorner(this.Begin, this.Target.Global);
        }
        private static IntVec3 GetBottomCorner(IntVec3 a, IntVec3 b)
        {
            var end = b;
            var dx = end.X - a.X;
            var adx = Math.Abs(dx);
            var dy = end.Y - a.Y;
            var ady = Math.Abs(dy);
            IntVec3 axis;
            if (adx > ady)
                axis = IntVec3.UnitX + IntVec3.UnitZ;
            else
                axis = IntVec3.UnitY + IntVec3.UnitZ;
            return a + new IntVec3(dx * axis.X, dy * axis.Y, 0);
        }

        static IntVec3 GetEnd(IntVec3 begin, IntVec3 target)
        {
            var end = target;
            var dx = end.X - begin.X;
            var adx = Math.Abs(dx);
            var dy = end.Y - begin.Y;
            var ady = Math.Abs(dy);
            IntVec3 axis;
            if (adx > ady)
                axis = IntVec3.UnitX + IntVec3.UnitZ;
            else
                axis = IntVec3.UnitY + IntVec3.UnitZ;

            return begin + new IntVec3(dx * axis.X, dy * axis.Y, 0);
        }

        public override IEnumerable<IntVec3> GetPositions()
        {
            foreach (var i in this.Begin.GetBox(this.TopCorner))
                yield return i;
        }
        protected override IEnumerable<IntVec3> GetPositionsNew(IntVec3 a, IntVec3 b)
        {
            var end = b + IntVec3.UnitZ * this.Height;
            var box = a.GetBox(end);
            return box;
        }

        static public List<IntVec3> GetPositions(IntVec3 a, IntVec3 b)
        {
            IntVec3 axis;
            var end = b;
            var dx = end.X - a.X;
            var adx = Math.Abs(dx);
            var dy = end.Y - a.Y;
            var ady = Math.Abs(dy);
            var dz = end.Z - a.Z;
            if (adx > ady)
                axis = IntVec3.UnitX + IntVec3.UnitZ;
            else
                axis = IntVec3.UnitY + IntVec3.UnitZ;

            var bb = a + new IntVec3(dx * axis.X, dy * axis.Y, dz);
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
