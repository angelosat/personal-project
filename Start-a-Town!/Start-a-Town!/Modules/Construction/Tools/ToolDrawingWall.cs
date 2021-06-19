using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Modules.Construction
{
    class ToolDrawingWall : ToolDrawingWithHeight
    {
        public override string Name
        {
            get { return "Wall"; }
        }
        public override Modes Mode { get { return Modes.Wall; } }
        public ToolDrawingWall()
        {

        }
        public ToolDrawingWall(Action<Args> callback)
            : base(callback)
        {
            //this.Callback = callback;
        }
        
        //protected override void OnUpdate()
        //{
        //    //base.OnUpdate();
        //    if (this.SettingHeight)
        //    {
        //        this.TopCorner = this.End + Vector3.UnitZ * this.Height;
        //        return;
        //    }
        //    else
        //    {
        //        this.End = GetBottomCorner();
        //        this.TopCorner = this.End;
        //    }
        //}
        //public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    return base.MouseLeftPressed(e);
        //    this.TopCorner = this.Begin;
        //}
        protected override Vector3 GetBottomCorner()
        {
            return GetBottomCorner(this.Begin, this.Target.Global);
            //var end = this.Target.Global;// *(Vector3.One - this.Plane) + this.Begin * this.Plane;
            //var dx = end.X - this.Begin.X;
            //var adx = Math.Abs(dx);
            //var dy = end.Y - this.Begin.Y;
            //var ady = Math.Abs(dy);
            //if (adx > ady)
            //    this.Axis = Vector3.UnitX + Vector3.UnitZ;
            //else
            //    this.Axis = Vector3.UnitY + Vector3.UnitZ;
            //var actualend = this.Begin + new Vector3(dx * this.Axis.X, dy * this.Axis.Y, 0);
            //return actualend;
        }
        private static Vector3 GetBottomCorner(Vector3 a, Vector3 b)
        {
            var end = b;// *(Vector3.One - this.Plane) + this.Begin * this.Plane;
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
            var end = target;// *(Vector3.One - this.Plane) + this.Begin * this.Plane;
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
            //return GetPositionsNew(this.Begin, this.SettingHeight ? this.End + Vector3.UnitZ * this.Height : this.End).ToList();// + Vector3.UnitZ * this.Height).ToList();
            return this.Begin.GetBox(this.TopCorner);
            //return GetPositionsNew(this.Begin, this.End).ToList();// + Vector3.UnitZ * this.Height).ToList();
        }
        protected override IEnumerable<Vector3> GetPositionsNew(Vector3 a, Vector3 b)
        {
            //var end = this.End + Vector3.UnitZ * this.Height;
            //var box = this.Begin.GetBox(end);
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
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, IMap map, Camera camera, Net.PlayerData player)
        {
            return;
            //if (!this.Enabled)
            //    return;

            //Vector3 end;
            //var vector2 = player.GetMousePosition(camera);
            //if (this.SettingHeight)
            //{
            //    this.Height = ToolDrawingWithHeight.GetHeight(this.End, vector2);
            //    end = this.End;
            //}
            //else

            //    this.End = player.Target.Type != TargetType.Position ? this.End : GetEnd(this.Begin, player.Target.Global);
            //end = this.End;

            //var positions = this.GetPositionsNew(this.Begin, end)
            //    .Where(vec => this.Replacing ? map.GetBlock(vec) != Block.Air : map.GetBlock(vec) == Block.Air);
            //camera.DrawGridBlocks(sb, Block.BlockBlueprint, positions, Color.Red);
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
