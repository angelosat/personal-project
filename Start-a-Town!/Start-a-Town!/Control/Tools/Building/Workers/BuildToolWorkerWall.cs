using System;
using System.Collections.Generic;

namespace Start_a_Town_
{
    class BuildToolWorkerWall : BuildToolWorker
    {
        public override IEnumerable<IntVec3> GetPositions(IntVec3 a, IntVec3 b)
        {
            return a.GetBox(b);
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
    }
}
