using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Modules.Construction;

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
        public override List<Vector3> GetPositions()
        {
            return GetPositions(this.Begin, this.TopCorner).ToList();
        }

        static public new IEnumerable<Vector3> GetPositions(Vector3 begin, Vector3 end)
        {
            VectorHelper.GetMinMaxVector3(begin, end, out var min, out var max);
            var maxh = GetMaxHeight(min, max);
            var axis = GetAxis(min, max);
            var h = Math.Min(maxh, max.Z - min.Z) + 1;
            for (int i = 0; i < h; i++)
            {
                var a = new Vector3(min.X + i * axis.Y, min.Y + i * axis.X, begin.Z + i);
                var b = new Vector3(max.X - i * axis.Y, max.Y - i * axis.X, begin.Z + i);
                foreach (var pos in a.GetBox(b))
                    yield return pos;
            }
        }
    }
}
