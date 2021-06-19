using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Modules.Construction
{
    class ToolDrawingEnclosure : ToolDrawingBox
    {
        public override string Name
        {
            get { return "Enclosure"; }
        }
        public ToolDrawingEnclosure()
        {

        }
        public ToolDrawingEnclosure(Action<Args> callback)
            : base(callback)
        {

        }
        
        protected override void DrawGrid(MySpriteBatch sb, IMap map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;
            var end = this.End + Vector3.UnitZ * this.Height;

            var box = this.Begin.GetBox(end);
            if (Math.Abs(this.End.X - this.Begin.X) > 1 && Math.Abs(this.End.Y - this.Begin.Y) > 1)
            {
                //var boxInner = (this.Begin + new Vector3(1, 1, 0)).GetBox(end - new Vector3(1, 1, 0));
                VectorHelper.GetMinMaxVector3(this.Begin, end, out Vector3 a, out Vector3 b);
                var boxInner = (a + new Vector3(1, 1, 0)).GetBox(b - new Vector3(1, 1, 0));
                box = box.Except(boxInner).ToList();
            }
            //foreach (var vec in box)
            //    cam.DrawGridBlock(sb, this.Valid ? Color.Lime : Color.Red, vec);
            cam.DrawGridBlocks(sb, Block.BlockBlueprint, box, color);
        }
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, IMap map, Camera camera, Net.PlayerData player)
        {
            this.DrawGrid(sb, map, camera, Color.Red);
        }
        public override List<Vector3> GetPositions()
        {
            return GetPositions(this.Begin, this.TopCorner);
        }
        static new public List<Vector3> GetPositions(Vector3 a, Vector3 b)
        {
            var box = a.GetBox(b);
            if (Math.Abs(b.X - a.X) > 1 && Math.Abs(b.Y - a.Y) > 1)
            {
                VectorHelper.GetMinMaxVector3(a, b, out a, out b);
                var boxInner = (a + new Vector3(1, 1, 0)).GetBox(b - new Vector3(1, 1, 0));
                box = box.Except(boxInner).ToList();
            }
            return box;
        }

        public override ToolDrawing.Modes Mode
        {
            get { return Modes.Enclosure; }
        }
    }
}
