using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

namespace Start_a_Town_.Modules.Construction
{
    public abstract partial class ToolDrawing : ToolManagement, INamed
    {
        public enum Modes { Single, Line, Wall, Enclosure, BoxFilled, BoxHollow, Box, Roof, Pyramid }
        new readonly Icon Icon = new(UIManager.Icons32, 12, 32);
        public abstract Modes Mode { get; }
        public abstract string Name { get; }
        private Action<Args> Callback;
        protected bool Valid, Enabled;
        protected Vector3 Begin, End, Axis;

        public Block Block;
        public byte State;
        public int Variation, Orientation;

        int 
            Height;
        public ToolDrawing()
        {

        }
        public ToolDrawing(Action<Args> callback)
        {
            this.Callback = callback;
        }
        public override void Update()
        {
            base.Update();
            if(this.Target.Type == TargetType.Entity)
                this.Target = Controller.Instance.MouseoverBlock.TargetCell;
        }
        private void CheckValidity()
        {
            this.Valid = true;
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Enabled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            if (this.Target.Type != TargetType.Position)
                return Messages.Default;
            this.Replacing = IsReplacing();
            var pos = this.Replacing ? this.Target.Global : this.Target.FaceGlobal;
            this.Begin = pos;
            this.End = this.Begin;
            this.Height = 0;
            this.Enabled = true;
            Sync();
            return Messages.Default;
        }

        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            var target = this.Target;
            if (target == null)
                return ControlTool.Messages.Default;
            if (IsRemoving())
            {

            }
            CheckValidity();

            return ControlTool.Messages.Default;
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Enabled)
            {
                this.Enabled = false;
                this.Replacing = false;
                Sync();
                return Messages.Default;
            }
            else
                return Messages.Remove;
        }
        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            if (e.KeyValue == 17) //control
            {
                ToolManager.SetTool(new ToolDrawingErase(this));
            }
            base.HandleKeyDown(e);
        }
        public override void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        {
            if (e.Handled)
                return;
            switch (e.KeyChar)
            {
                //case '[':
                case 'e':
                    RotateClockwise();
                    e.Handled = true;
                    break;

                //case ']':
                case 'q':
                    RotateAntiClockwise();
                    e.Handled = true;
                    break;

                default:
                    break;
            }
        }

        private void RotateAntiClockwise()
        {
            this.Orientation -= 1;
            if (this.Orientation < 0)
                this.Orientation = 3;
        }

        private void RotateClockwise()
        {
            this.Orientation = (this.Orientation + 1) % 4;
        }
        Vector3 GetMouseover()
        {
            return IsReplacing() ? this.Target.Global : this.Target.FaceGlobal;
        }

        internal override void DrawAfterWorld(MySpriteBatch sb, IMap map)
        {
            var cam = map.Camera;
            if (!this.Enabled)
            {
                this.DrawBlockMouseover(sb, map, cam);
                return;
            }
            else
                this.DrawBlockPreviews(sb, map, cam);
        }
        public override void DrawBlockMouseover(MySpriteBatch sb, IMap map, Camera camera)
        {
            Sprite.Atlas.Begin();
            if (!this.Enabled)
            {
                if (this.Target != null)
                    camera.DrawGridBlock(sb, Color.Yellow, this.GetMouseover());
                return;
            }
        }
    
        protected virtual void DrawGrid(MySpriteBatch sb, IMap map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;

            var end = this.End + Vector3.UnitZ * this.Height;
            var col = this.Valid ? Color.Lime : Color.Red;
            int x = (int)Math.Min(this.Begin.X, end.X);
            int y = (int)Math.Min(this.Begin.Y, end.Y);
            int z = (int)Math.Min(this.Begin.Z, end.Z);

            int dx = (int)Math.Abs(this.Begin.X - end.X);
            int dy = (int)Math.Abs(this.Begin.Y - end.Y);
            int dz = (int)Math.Abs(this.Begin.Z - end.Z);

            var minBegin = new Vector3(x, y, z);
            for (int i = 0; i <= dx; i++)
            {
                for (int j = 0; j <= dy; j++)
                {
                    for (int k = 0; k <= dz; k++)
                    {
                        Vector3 global = minBegin + new Vector3(i, j, k);
                        cam.DrawGridCell(sb, col, global);
                    }
                }
            }
        }
        protected virtual List<Vector3> GetCells()
        {
            var list = new List<Vector3>();
            return list;

        }
        protected void DrawBlockPreviews(MySpriteBatch sb, IMap map, Camera cam)
        {
            var atlastoken = this.Block.GetDefault();
            atlastoken.Atlas.Begin(sb);
            foreach(var pos in this.GetPositions())
                this.Block.DrawPreview(sb, map, pos, cam, this.State, this.Variation, this.Orientation);
            sb.Flush();
        }

        internal override void DrawUI(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Camera camera)
        {
            base.DrawUI(sb, camera);
            this.Icon.Draw(sb, UIManager.Mouse);
            if (this.Replacing || IsReplacing())
                Icon.Replace.Draw(sb);

            if (!this.Enabled)
                return;
            var txt = string.Join(" x ", this.GetDimensionSize());
            UIManager.DrawStringOutlined(sb, txt, UIManager.Mouse - new Vector2(0, Label.DefaultHeight), Vector2.Zero);
        }
        public virtual IEnumerable<string> GetDimensionSize()
        {
            yield return (Math.Abs(this.End.X - this.Begin.X) + 1).ToString();
            yield return (Math.Abs(this.End.Y - this.Begin.Y) + 1).ToString();
        }
        
        private static bool IsRemoving()
        {
            return InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey);
        }
        protected bool Replacing;
        private static bool IsReplacing()
        {
            return InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey);
        }

        public virtual List<Vector3> GetPositions() { return new List<Vector3>(); }
        static public List<Vector3> GetPositions(Modes mode, Vector3 a, Vector3 b)
        {
            return mode switch
            {
                Modes.Single => ToolDrawingSingle.GetPositions(a, b),
                Modes.Line => ToolDrawingLine.GetPositions(a, b),
                Modes.Enclosure => ToolDrawingEnclosure.GetPositions(a, b),
                Modes.Box => ToolDrawingBox.GetPositions(a, b),
                Modes.Wall => ToolDrawingWall.GetPositions(a, b),
                Modes.Pyramid => ToolDrawingPyramid.GetPositions(a, b).ToList(),
                Modes.Roof => ToolDrawingRoof.GetPositions(a, b).ToList(),
                Modes.BoxFilled => ToolDrawingBoxFilled.GetPositions(a, b),
                _ => new List<Vector3>(),
            };
        }
        static public List<Vector3> GetPositions(Args a)
        {
            return GetPositions(a.Mode, a.Begin, a.End);
        }
        public Args Send(Modes mode, Vector3 start, Vector3 end, int orientation)// = 0)
        {
            var a = new Args(mode, start, end, IsRemoving(), IsGodMode(), this.Replacing, orientation);
            this.Callback(a);
            this.Enabled = false;
            this.Replacing = false;
            return a;
        }

        private static bool IsGodMode()
        {
            return false;
        }
        protected override void WriteData(BinaryWriter w)
        {
            w.Write(this.Enabled);
            w.Write(this.Begin);
        }
        protected override void ReadData(BinaryReader r)
        {
            this.Enabled = r.ReadBoolean();
            this.Begin = r.ReadVector3();
        }
    }
}
