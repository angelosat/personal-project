using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.Graphics;

namespace Start_a_Town_
{
    class ToolSelect : ControlTool
    {
        protected Vector3 Begin, End;
        AtlasDepthNormals.Node.Token SelectionGraphic = Block.BlockHighlight;
        Action<Vector3, Vector3> SelectAction = (a, b) => { };
        public ToolSelect()
        {

        }
        public ToolSelect(TargetArgs target)
        {
            this.Begin = target.Type == TargetType.Position ? this.GetBeginFromTarget(target.Global) : target.Object.Global.SnapToBlock();
            this.End = this.Begin;
        }
        protected virtual Vector3 GetBeginFromTarget(Vector3 a)
        {
            return a.Above();
        }
        protected virtual Vector3 GetEndFromTarget(Vector3 a)
        {
            return new Vector3(a.XY(), this.Begin.Z);
        }
        protected virtual void Select()
        {
            this.SelectAction(this.Begin, this.End);
            UISelectedInfo.Refresh(Client.Instance.Map, this.Begin.GetBoundingBox(this.End));
        }
        public override void Update()
        {
            //base.Update();
            //var cam = ScreenManager.CurrentScreen.Camera;
            var cam = Engine.Map.Camera;

            cam.MousePicking(Rooms.Ingame.DrawServer ? Server.Instance.Map : Client.Instance.Map);
            this.UpdateTarget();

            //if (this.Target == null)
            //    return;
            //if (this.Target.Type != TargetType.Position)
            //    return;
            //this.End = this.GetEndFromTarget(this.Target.Global);

            if (Controller.TargetCell != null)
                this.End = this.GetEndFromTarget(Controller.TargetCell.Global);
        }

        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Target == null)
                return Messages.Default;
            //if (this.Target.Type != TargetType.Position)
            //    return Messages.Default;
            this.Select();
            return Messages.Remove;
        }

        internal override void DrawAfterWorld(MySpriteBatch sb, IMap map)
        {
            var camera = map.Camera;
            camera.DrawGridBlocks(sb, this.Begin.GetBox(this.End), Color.White);
        }
    }
}
