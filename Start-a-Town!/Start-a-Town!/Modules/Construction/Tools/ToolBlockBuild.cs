using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace Start_a_Town_.Modules.Construction
{
    public abstract partial class ToolBlockBuild : ToolManagement, INamed
    {
        public enum Modes { Single, Line, Wall, Enclosure, BoxFilled, BoxHollow, Box, Roof, Pyramid }
        new readonly Icon Icon = new(UIManager.Icons32, 12, 32);
        public abstract Modes Mode { get; }
        public abstract string Name { get; }
        private readonly Action<Args> Callback;
        protected bool Valid, Enabled;
        protected IntVec3 Begin, End, Axis;
        public Block Block;
        public MaterialDef Material;
        public byte State;
        public int Variation, Orientation;
        int Height;
        public override bool TargetOnlyBlocks => true;

        public ToolBlockBuild()
        {

        }
        public ToolBlockBuild(Action<Args> callback)
        {
            this.Callback = callback;
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
            this.Sync();
            return Messages.Default;
        }
        public override void HandleLButtonDoubleClick(System.Windows.Forms.HandledMouseEventArgs e)
        {
        }
        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            var target = this.Target;
            if (target == null)
                return ControlTool.Messages.Default;
            if (IsRemoving())
            {

            }
            this.CheckValidity();

            return ControlTool.Messages.Default;
        }
        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Enabled)
            {
                this.Enabled = false;
                this.Replacing = false;
                this.Sync();
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
        IntVec3 GetMouseover()
        {
            return IsReplacing() ? this.Target.Global : this.Target.FaceGlobal;
        }
        internal override void DrawAfterWorld(MySpriteBatch sb, MapBase map)
        {
            var cam = map.Camera;
            //if (!this.Enabled)
            //{
            //    this.DrawBlockMouseover(sb, map, cam);
            //    return;
            //}
            //else
            //    this.DrawBlockPreviews(sb, map, cam);
            this.DrawBlockMouseover(sb, map, cam);
            if (this.Enabled)
                this.DrawBlockPreviews(sb, map, cam);
        }
        //public override void DrawBlockMouseover(MySpriteBatch sb, MapBase map, Camera camera)
        //{
        //    Sprite.Atlas.Begin();
        //    if (this.Target is not null)
        //    //if (!this.Enabled && this.Target is not null)
        //    {
        //        var vec = this.GetMouseover();
        //        var col = map.IsValidBuildSpot(vec) ? Color.Yellow : Color.Red;
        //        camera.DrawGridBlock(sb, col, this.GetMouseover());
        //    }
        //}
        public override void DrawBlockMouseover(MySpriteBatch sb, MapBase map, Camera camera)
        {
            if (this.Target is not null)
            //if (!this.Enabled && this.Target is not null)
            {
                var vec = this.GetMouseover();
                var col = map.IsValidBuildSpot(vec) ? Color.Yellow : Color.Red;
                ToolManager.DrawBlockHighlight(sb, map, camera, this.GetMouseover(), col);
                //camera.DrawGridBlock(sb, Block.BlockHightlightBack, col, this.GetMouseover());
                //camera.DrawGridBlock(sb, Block.BlockHighlight, col, this.GetMouseover());
            }
        }
        protected virtual void DrawGrid(MySpriteBatch sb, MapBase map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;

            var end = this.End + IntVec3.UnitZ * this.Height;
            var col = this.Valid ? Color.Lime : Color.Red;
            int x = Math.Min(this.Begin.X, end.X);
            int y = Math.Min(this.Begin.Y, end.Y);
            int z = Math.Min(this.Begin.Z, end.Z);

            int dx = Math.Abs(this.Begin.X - end.X);
            int dy = Math.Abs(this.Begin.Y - end.Y);
            int dz = Math.Abs(this.Begin.Z - end.Z);

            var minBegin = new IntVec3(x, y, z);
            for (int i = 0; i <= dx; i++)
            {
                for (int j = 0; j <= dy; j++)
                {
                    for (int k = 0; k <= dz; k++)
                    {
                        IntVec3 global = minBegin + new IntVec3(i, j, k);
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
        void DrawBlockPreviews(MySpriteBatch sb, MapBase map, Camera cam)
        {
            var atlastoken = this.Block.GetDefault();
            atlastoken.Atlas.Begin(sb);
            foreach (var pos in this.GetPositions().Where(map.IsValidBuildSpot))
                this.Block.DrawPreview(sb, map, pos, cam, this.State, this.Material, this.Variation, this.Orientation);
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

        public virtual IEnumerable<IntVec3> GetPositions() { yield break; }
        public static List<IntVec3> GetPositions(Modes mode, IntVec3 a, IntVec3 b)
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
                _ => new List<IntVec3>(),
            };
        }
        public static List<IntVec3> GetPositions(Args a)
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
