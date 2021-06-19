using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Modules.Construction
{
    class ToolDrawingBoxFilled : ToolDrawingWithHeight
    {
        public override string Name
        {
            get { return "Box Filled"; }
        }
        //protected bool SettingHeight;
        //protected int Height;
        public ToolDrawingBoxFilled()
        {

        }
        public ToolDrawingBoxFilled(Action<Args> callback)
            : base(callback)
        {

        }
        
        
        protected override void DrawGrid(MySpriteBatch sb, IMap map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;
            var end = this.End + Vector3.UnitZ * this.Height;

            var box = this.Begin.GetBox(end);

            //foreach (var vec in box)
            //    cam.DrawGridBlock(sb, this.Valid ? Color.Lime : Color.Red, vec);

            cam.DrawGridBlocks(sb, Block.BlockBlueprint, box, color);
        }
        public override List<Vector3> GetPositions()
        {
            return GetPositions(this.Begin, this.TopCorner);
        }
        static public List<Vector3> GetPositions(Vector3 a, Vector3 b)
        {
            var box = a.GetBox(b);
            return box;
        }
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, IMap map, Camera camera, Net.PlayerData player)
        {
            this.DrawGrid(sb, map, camera, Color.Red);
        }
        protected override void WriteData(System.IO.BinaryWriter w)
        {
            base.WriteData(w);
            //w.Write(this.End);
            w.Write(this.SettingHeight);
            w.Write(this.Height);
        }
        protected override void ReadData(System.IO.BinaryReader r)
        {
            base.ReadData(r);
            //this.End = r.ReadVector3();
            this.SettingHeight = r.ReadBoolean();
            this.Height = r.ReadInt32();
        }

        public override ToolDrawing.Modes Mode
        {
            get { return Modes.BoxFilled; }
        }
    }
}
