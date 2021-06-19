using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Modules.Construction
{
    abstract class ToolDrawingWithHeight : ToolDrawing
    {
        protected bool SettingHeight;
        protected int Height;
        protected Vector3 TopCorner;

        
        public ToolDrawingWithHeight()
        {

        }
        public ToolDrawingWithHeight(Action<Args> callback)
            : base(callback)
        {

        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //base.MouseLeftPressed(e);
            //this.TopCorner = this.Begin;
            //return Messages.Default;
            if (this.SettingHeight && this.Enabled)
            {
                this.Send(this.Mode, this.Begin, this.TopCorner, this.Orientation);// this.End + Vector3.UnitZ * this.Height);//
                this.SettingHeight = false;
                this.Enabled = false;
                return Messages.Default;
            }
            if (!this.Enabled)
            {
                this.SettingHeight = false;
                base.MouseLeftPressed(e);
                this.TopCorner = this.Begin;
                this.Height = 0;

                return Messages.Default;
            }
            this.Height = 0;
            return Messages.Default;

            //return base.MouseLeftPressed(e);
        }
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.Enabled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            if (this.SettingHeight)
            {
                this.Enabled = false;
                this.SettingHeight = false;
            }
            else
            {
                this.SettingHeight = this.Enabled;
                this.TopCorner = this.Begin;
                this.Height = 0;
            }
            Sync();
            return Messages.Default;
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            this.SettingHeight = false;
            this.Height = 0;
            return base.MouseRightDown(e);
        }
        public sealed override void Update()
        {
            base.Update();
            if (this.SettingHeight)
            {
                this.SetHeight();
                return;
            }
            if (!Enabled)
                return;
            if (this.Target == null)
                return;
            if (this.Target.Type != TargetType.Position)
                return;
            this.OnUpdate();
        }
        protected override void OnUpdate()
        {
            //base.OnUpdate();
            if (this.SettingHeight)
            {
                //this.Height = Math.Min(Net.Client.Instance.Map.GetMaxHeight() - 1, lengthinblocks);

                //this.TopCorner = this.End + Vector3.UnitZ * Math.Min(Net.Client.Instance.Map.GetMaxHeight() - 1, this.Height - this.End.Z);
                this.TopCorner = new Vector3(this.End.X, this.End.Y, Math.Min(this.End.Z + this.Height, Net.Client.Instance.Map.GetMaxHeight() - 1));

                return;
            }
            else
            {
                this.End = GetBottomCorner(); //this.Target.Global;
                this.TopCorner = this.End;
            }
        }
        protected virtual Vector3 GetBottomCorner()
        {
            //return this.Target.FaceGlobal;
            var g = this.Target.FaceGlobal;
            return new Vector3(g.X, g.Y, this.Begin.Z);
        }
        private void SetHeight()
        {
            //var endscreenposition = ScreenManager.CurrentScreen.Camera.GetScreenPosition(this.End);
            //var currentposition = UIManager.Mouse;
            //var length = Math.Max(0, endscreenposition.Y - currentposition.Y) / ScreenManager.CurrentScreen.Camera.Zoom;
            //var lengthinblocks = (int)(length / Block.BlockHeight);
            //this.Height = lengthinblocks;
            //if (this.Height < 0)
            //    throw new Exception();
            this.Height = GetHeight(this.End, UIManager.Mouse);
        }
        static protected int GetHeight(Vector3 end, Vector2 mousePointer)
        {
            var endscreenposition = ScreenManager.CurrentScreen.Camera.GetScreenPosition(end);
            var length = (endscreenposition.Y - mousePointer.Y) / ScreenManager.CurrentScreen.Camera.Zoom;
            var lengthinblocks = (int)(length / Block.BlockHeight);
            var height = lengthinblocks;
            //if (height < 0)
            //    throw new Exception();
            return height;

            //var endscreenposition = ScreenManager.CurrentScreen.Camera.GetScreenPosition(end);
            //var length = Math.Max(0, endscreenposition.Y - mousePointer.Y) / ScreenManager.CurrentScreen.Camera.Zoom;
            //var lengthinblocks = (int)(length / Block.BlockHeight);
            //var height = lengthinblocks;
            //if (height < 0)
            //    throw new Exception();
            //return height;
        }

        protected override void DrawGrid(MySpriteBatch sb, IMap map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;
            var positions = this.GetPositionsNew(this.Begin, this.End)
                .Where(vec => this.Replacing ? map.GetBlock(vec) != BlockDefOf.Air : map.GetBlock(vec) == BlockDefOf.Air);
            cam.DrawGridBlocks(sb, Block.BlockBlueprint, positions, color);
        }
        protected virtual IEnumerable<Vector3> GetPositionsNew(Vector3 a, Vector3 b) { yield break; }
        protected override void WriteData(System.IO.BinaryWriter w)
        {
            base.WriteData(w);
            w.Write(this.End);
            w.Write(this.SettingHeight);
            w.Write(this.Height);
        }
        protected override void ReadData(System.IO.BinaryReader r)
        {
            base.ReadData(r);
            this.End = r.ReadVector3();
            this.SettingHeight = r.ReadBoolean();
            this.Height = r.ReadInt32();
        }
        public override IEnumerable<string> GetDimensionSize()
        {
            foreach(var t in base.GetDimensionSize())
                yield return t;
            yield return (Math.Abs(this.TopCorner.Z - this.Begin.Z) + 1).ToString();
        }
    }
}
