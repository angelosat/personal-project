﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Modules.Construction;

namespace Start_a_Town_
{
    class ToolDrawingPyramid : ToolDrawingWithHeight
    {
        public ToolDrawingPyramid()
        {

        }
        public ToolDrawingPyramid(Action<Args> callback)
            : base(callback)
        {

        }
        public override string Name
        {
            get { return "Pyramid"; }
        }
        public override Modes Mode { get { return Modes.Pyramid; } }
        public override List<Vector3> GetPositions()
        {
            return GetPositions(this.Begin, this.TopCorner).ToList();
        }

        static public IEnumerable<Vector3> GetPositions(Vector3 begin, Vector3 end)
        {
            //List<Vector3> list = new List<Vector3>();
            Vector3 min, max;
            VectorHelper.GetMinMaxVector3(begin, end, out min, out max);
            //var dx = max.X - min.X;
            //var dy = max.Y - min.Y;
            //var shortestside = Math.Min(dx, dy);
            //var maxh = shortestside / 2;

            var h = Math.Min(GetMaxHeight(min, max), max.Z - min.Z) + 1;
            for (int i = 0; i < h; i++)
            {
                var a = new Vector3(min.X + i, min.Y + i, begin.Z + i);
                var b = new Vector3(max.X - i, max.Y - i, begin.Z + i);
                foreach (var pos in a.GetBox(b))
                    yield return pos;
                //list.AddRange(a.GetBox(b));
            }
            //return list;
        }
        protected static int GetMaxHeight(Vector3 min, Vector3 max)
        {
            var dx = max.X - min.X;
            var dy = max.Y - min.Y;
            var shortestside = Math.Min(dx, dy);
            return (int)(shortestside / 2);
        }
        protected static float GetShortestSideInt(Vector3 min, Vector3 max)
        {
            var dx = max.X - min.X;
            var dy = max.Y - min.Y;
            return Math.Min(dx, dy);
        }
        protected static Vector3 GetAxis(Vector3 min, Vector3 max)
        {
            var dx = max.X - min.X;
            var dy = max.Y - min.Y;
            if (dx < dy)
                return Vector3.UnitY;
            else return Vector3.UnitX;
        }
    }
}
