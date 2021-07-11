﻿using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class ToolDrawingRoof : ToolDrawingPyramid
    {
        public ToolDrawingRoof()
        {

        }
        public ToolDrawingRoof(Action<Args> callback)
            : base(callback)
        {

        }
        public override string Name => "Roof";
        public override Modes Mode => Modes.Roof;
        public override List<IntVec3> GetPositions()
        {
            return GetPositions(this.Begin, this.TopCorner).ToList();
        }

        static public new IEnumerable<IntVec3> GetPositions(IntVec3 begin, IntVec3 end)
        {
            VectorHelper.GetMinMaxVector3(begin, end, out var min, out var max);
            var maxh = GetMaxHeight(min, max);
            var axis = GetAxis(min, max);
            var h = Math.Min(maxh, max.Z - min.Z) + 1;
            for (int i = 0; i < h; i++)
            {
                var a = new IntVec3(min.X + i * axis.Y, min.Y + i * axis.X, begin.Z + i);
                var b = new IntVec3(max.X - i * axis.Y, max.Y - i * axis.X, begin.Z + i);
                foreach (var pos in a.GetBox(b))
                    yield return pos;
            }
        }
    }
}
