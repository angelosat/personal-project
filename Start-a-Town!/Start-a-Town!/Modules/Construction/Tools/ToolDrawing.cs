using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.UI;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Modules.Construction
{
    public abstract class ToolDrawing : ToolManagement, INamed// ControlTool// DefaultTool// ControlTool
    {
        public enum Modes { Single, Line, Wall, Enclosure, BoxFilled, BoxHollow, Box, Roof, Pyramid }
        public class Args
        {
            public Vector3 Begin, End;
            public bool Removing, Replacing, Cheat;
            public Modes Mode;
            public int Orientation;
            public Args(Modes mode, Vector3 begin, Vector3 end, bool modkey, bool cheat, bool replacing = false, int orientation = 0)
            {
                this.Mode = mode;
                this.Begin = begin;
                this.End = end;
                this.Removing = modkey;
                this.Replacing = replacing;
                this.Orientation = orientation;
                this.Cheat = cheat;
            }
            public void Write(BinaryWriter w)
            {
                w.Write((byte)this.Mode);
                w.Write(this.Begin);
                w.Write(this.End);
                w.Write(this.Removing);
                w.Write(this.Replacing);
                w.Write(this.Orientation);
                w.Write(this.Cheat);
            }
            public Args(BinaryReader r)
            {
                this.Mode = (Modes)r.ReadByte();
                this.Begin = r.ReadVector3();
                this.End = r.ReadVector3();
                this.Removing = r.ReadBoolean();
                this.Replacing = r.ReadBoolean();
                this.Orientation = r.ReadInt32();
                this.Cheat = r.ReadBoolean();
            }
        }
        new readonly Icon Icon = new(UIManager.Icons32, 12, 32);
        public abstract Modes Mode { get; }
        public abstract string Name { get; }
        //Action<BlockConstruction.ProductMaterialPair, TargetArgs, int, bool, bool> Callback;
        private Action<Args> Callback;
        protected bool Valid, Enabled;
        protected Vector3 Begin, End, Axis;

        public Block Block;// { get { return this.blockbette} }
        public byte State;
        public int Variation, Orientation;

        int 
            Length, 
            Height;
        public ToolDrawing()
        {

        }
        public ToolDrawing(Action<Args> callback)
            //:base(ScreenManager.CurrentScreen.Camera)
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
            var targetposition = this.Target.FaceGlobal; // the construction designation block is non-solid, so we don't want to exclude non-solid in selecting target block face
            this.Valid = true;
            //foreach(var p in this.Item.Block.GetParts(targetposition, this.Orientation))
            //    this.Valid &= Client.Instance.Map.IsEmpty(p.Key);
        }
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Enabled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            if (this.Target.Type != TargetType.Position)
                return Messages.Default;
            this.Replacing = IsReplacing();// InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey);
            var pos = this.Replacing ? this.Target.Global : this.Target.FaceGlobal;
            //if (this.GetZones().Contains(pos))
            //    this.Removing = true;
            this.Begin = pos;
            this.End = this.Begin;
            this.Length = 1;
            this.Height = 0;
            this.Enabled = true;
            //this.Valid = this.Check(this.Width, this.Height);
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
        //public override ControlTool.Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
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
            //return Messages.Default;

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

        private void SetHeight()
        {
            var endscreenposition = ScreenManager.CurrentScreen.Camera.GetScreenPosition(this.End);
            var currentposition = UIManager.Mouse;
            var length = Math.Max(0, endscreenposition.Y - currentposition.Y) / ScreenManager.CurrentScreen.Camera.Zoom;
            var lengthinblocks = (int)(length / Block.BlockHeight);
            this.Height = Math.Min(Net.Client.Instance.Map.GetMaxHeight() - 1, lengthinblocks);
        }

        
        internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera camera)
        {
            

            //foreach (var g in this.GetZones())
            //    this.DrawGridCell(sb, camera, Color.Yellow, g);

            //base.DrawBeforeWorld(sb, map, camera);
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
                //this.DrawGrid(sb, map, cam, Color.White);
        }
        public override void DrawBlockMouseover(MySpriteBatch sb, IMap map, Camera camera)
        {
            Sprite.Atlas.Begin();
            if (!this.Enabled)
            {
                if (this.Target != null)
                    camera.DrawGridBlock(sb, Color.Yellow, this.GetMouseover());// this.Target.FaceGlobal);
                return;
            }
            //this.DrawGrid(sb, camera);
        }
        //internal override void DrawAfterWorld(MySpriteBatch sb, IMap map, Camera cam)
        //{
        //    Game1.Instance.GraphicsDevice.Textures[0] = Sprite.Atlas.Texture;
        //    Game1.Instance.GraphicsDevice.Textures[1] = Sprite.Atlas.DepthTexture;
        //    this.DrawGrid(sb, cam);
        //    base.DrawAfterWorld(sb, map, cam);
        //}
        //protected virtual void DrawAction(MySpriteBatch sb, Camera cam){}
        protected virtual void DrawGrid(MySpriteBatch sb, IMap map, Camera cam, Color color)
        {
            if (!this.Enabled)
                return;
            //this.DrawAction(sb, cam);
            //return;

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
                        //DrawGridCell(sb, cam, col, global);
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
            //if (this.Target == null)
            //    return;

            var atlastoken = this.Block.GetDefault();
            //var global = this.Target.FaceGlobal;
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
            //var dimensionsText = this.GetDimensionSize().Aggregate((a, b) => a + " x " + b);
            var txt = string.Join(" x ", this.GetDimensionSize());
            //UIManager.DrawStringOutlined(sb, GetPositions().Count.ToString(), UIManager.Mouse - new Vector2(0, 32), Vector2.Zero);
            UIManager.DrawStringOutlined(sb, txt, UIManager.Mouse - new Vector2(0, Label.DefaultHeight), Vector2.Zero);
            
            return;
            //if (IsDesignating())
            //    Icon.Replace.Draw(sb);
            //else if(IsRemoving())
            //    Icon.Cross.Draw(sb);
        }
        public virtual IEnumerable<string> GetDimensionSize()
        {
            yield return (Math.Abs(this.End.X - this.Begin.X) + 1).ToString();
            yield return (Math.Abs(this.End.Y - this.Begin.Y) + 1).ToString();
        }
        private static bool IsDesignating()
        {
            return InputState.IsKeyDown(System.Windows.Forms.Keys.ShiftKey);
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
            switch(mode)
            {
                case Modes.Single:
                    return ToolDrawingSingle.GetPositions(a,b);
        
                case Modes.Line:
                    return ToolDrawingLine.GetPositions(a,b);
        
                case Modes.Enclosure:
                    return ToolDrawingEnclosure.GetPositions(a, b);

                case Modes.Box:
                    return ToolDrawingBox.GetPositions(a, b);

                case Modes.Wall:
                    return ToolDrawingWall.GetPositions(a, b);

                case Modes.Pyramid:
                    return ToolDrawingPyramid.GetPositions(a, b).ToList();

                case Modes.Roof:
                    return ToolDrawingRoof.GetPositions(a, b).ToList();

                case Modes.BoxFilled:
                    return ToolDrawingBoxFilled.GetPositions(a, b);

                default:
                    return new List<Vector3>();
            }
        }
        static public List<Vector3> GetPositions(Args a)
        {
            return GetPositions(a.Mode, a.Begin, a.End);
            //switch (a.Mode)
            //{
            //    case Modes.Single:
            //        return ToolDrawingSingle.GetPositions(a.Begin, a.End);

            //    case Modes.Line:
            //        return ToolDrawingLine.GetPositions(a.Begin, a.End);

            //    case Modes.Enclosure:
            //        return ToolDrawingEnclosure.GetPositions(a.Begin, a.End);

            //    default:
            //        return new List<Vector3>();
            //}
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
            return false;// InputState.IsKeyDown(System.Windows.Forms.Keys.LMenu);
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
