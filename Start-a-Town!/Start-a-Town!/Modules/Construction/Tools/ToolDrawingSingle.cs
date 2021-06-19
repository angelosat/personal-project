using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Modules.Construction
{
    class ToolDrawingSingle : ToolDrawing
    {
        public override string Name
        {
            get { return "Single"; }
        }
        public override Modes Mode { get { return Modes.Single; } }
        public ToolDrawingSingle()
        {

        }
        public ToolDrawingSingle(Action<Args> callback)
            : base(callback)
        {

        }
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.Enabled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            this.Send(Modes.Single, this.Begin, this.Begin, this.Orientation);
            this.Enabled = false;
            return Messages.Default;
        }
        
        public override void Update()
        {
            base.Update();
        }
        
        protected override void DrawGrid(MySpriteBatch sb, IMap map, Camera cam, Color color)
        {
            cam.DrawGridBlock(sb, this.Valid ? Color.Lime : Color.Red, this.Begin);
        }
        public override List<Vector3> GetPositions()
        {
            return new List<Vector3>() { this.Begin };
        }
        static public List<Vector3> GetPositions(Vector3 a, Vector3 b)
        {
            return new List<Vector3>() { a };
        }
    }
}
