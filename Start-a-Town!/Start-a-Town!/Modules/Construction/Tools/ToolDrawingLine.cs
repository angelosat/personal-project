using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Modules.Construction
{
    class ToolDrawingLine : ToolDrawing
    {
        public override string Name
        {
            get { return "Line"; }
        }
        public override Modes Mode { get { return Modes.Line; } }
        public ToolDrawingLine()
        {

        }
        public ToolDrawingLine(Action<Args> callback)
            : base(callback)
        {

        }
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.Enabled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            this.Send(Modes.Line, this.Begin, this.End, this.Orientation);
            this.Enabled = false;
            Sync();
            return Messages.Default;
        }
        public override void Update()
        {
            base.Update();
            if (!Enabled)
                return;
            if (this.Target == null)
                return;
            if (this.Target.Type != TargetType.Position)
                return;
            //var end = this.Target.Global;
            //var dx = end.X - this.Begin.X;
            //var adx = Math.Abs(dx);
            //var dy = end.Y - this.Begin.Y;
            //var ady = Math.Abs(dy);
            //if (adx > ady)
            //    this.Axis = Vector3.UnitX + Vector3.UnitZ;
            //else
            //    this.Axis = Vector3.UnitY + Vector3.UnitZ;
            //this.End = this.Begin + new Vector3(dx * this.Axis.X, dy * this.Axis.Y, 0);
            this.End = GetEnd(this.Begin, this.Target.Global);
        }
        static public Vector3 GetEnd(Vector3 begin, Vector3 end)
        {
            var dx = end.X - begin.X;
            var adx = Math.Abs(dx);
            var dy = end.Y - begin.Y;
            var ady = Math.Abs(dy);
            var axis = Vector3.Zero;
            if (adx > ady)
                axis = Vector3.UnitX + Vector3.UnitZ;
            else
                axis = Vector3.UnitY + Vector3.UnitZ;

            return begin + new Vector3(dx * axis.X, dy * axis.Y, 0);
        }
        protected override void DrawGrid(MySpriteBatch sb, IMap map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;
            var box = this.Begin.GetBox(this.End)
                .Where(vec => this.Replacing ? map.GetBlock(vec) != BlockDefOf.Air : map.GetBlock(vec) == BlockDefOf.Air);
            cam.DrawGridBlocks(sb, Block.BlockBlueprint, box, color);

            //foreach (var vec in box)
            //    cam.DrawGridBlock(sb, Block.BlockBlueprint, Color.White, vec);
        }
        public override List<Vector3> GetPositions()
        {
            return this.Begin.GetBox(this.End);
        }
        static public List<Vector3> GetPositions(Vector3 a, Vector3 b)
        {
            Vector3 axis;
            var end = b;
            var dx = end.X - a.X;
            var adx = Math.Abs(dx);
            var dy = end.Y - a.Y;
            var ady = Math.Abs(dy);
            if (adx > ady)
                axis = Vector3.UnitX + Vector3.UnitZ;
            else// if (ady > adx)
                axis = Vector3.UnitY + Vector3.UnitZ;

            var bb = a + new Vector3(dx * axis.X, dy * axis.Y, 0);
            var box = a.GetBox(bb);
            return box;
        }
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, IMap map, Camera camera, Net.PlayerData player)
        {
            return;
            if (!this.Enabled)
                return;
            var targetArgs = player.Target;
            this.End = targetArgs.Type != TargetType.Null ? GetEnd(this.Begin, targetArgs.Global) : this.End;
            //var map = Net.Client.Instance.Map;// targetArgs.Map;
            var box = this.Begin.GetBox(this.End)
                .Where(vec => this.Replacing ? map.GetBlock(vec) != BlockDefOf.Air : map.GetBlock(vec) == BlockDefOf.Air);

            //foreach (var vec in box)
            //    camera.DrawGridBlock(sb, Block.BlockBlueprint, Color.Red, vec);
            camera.DrawGridBlocks(sb, Block.BlockBlueprint, box, Color.Red);

            //var end = GetEnd(this.Begin, targetArgs.Global);
            //var map = Client.Instance.Map;// targetArgs.Map;
            //var box = this.Begin.GetBox(end)
            //    .Where(vec => this.Replacing ? map.GetBlock(vec) != Block.Air : map.GetBlock(vec) == Block.Air);

            ////foreach (var vec in box)
            ////    camera.DrawGridBlock(sb, Block.BlockBlueprint, Color.Red, vec);
            //camera.DrawGridBlocks(sb, Block.BlockBlueprint, box, Color.Red);

        }
    }
}
