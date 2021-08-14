using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Modules.Construction
{
    class ToolDrawingSingle : ToolBlockBuild
    {
        public override string Name { get; } = "Single";
        public override Modes Mode { get; } = Modes.Single; 
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
        
        protected override void DrawGrid(MySpriteBatch sb, MapBase map, Camera cam, Color color)
        {
            cam.DrawGridBlock(sb, this.Valid ? Color.Lime : Color.Red, this.Begin);
        }
        public override IEnumerable<IntVec3> GetPositions()
        {
            yield return this.Begin;
            //return new List<IntVec3>() { this.Begin };
        }
        static public List<IntVec3> GetPositions(IntVec3 a, IntVec3 b)
        {
            return new List<IntVec3>() { a };
        }
    }
}
