using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    class ToolDesignatePositions : ToolManagement
    {
        enum ValidityType { Invalid, Valid, Ignore }
        IntVec3 Begin, End;
        int Width, Height;
        bool Enabled;
        bool Valid;
        bool Removing;

        readonly Action<IntVec3, IntVec3, bool> Add;
        Func<List<IntVec3>> GetZones;
        public Func<IntVec3, bool> ValidityCheck;
        IntVec3 Plane;

        public ToolDesignatePositions(Action<IntVec3, IntVec3, bool> callback)
            : this(callback, () => new List<IntVec3>())
        {
        }
        public ToolDesignatePositions(Action<IntVec3, IntVec3, bool> callback, Func<List<IntVec3>> zones)
        {
            this.Add = callback;
            this.GetZones = zones;
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

            this.End = (IntVec3)this.Target.Global * (IntVec3.One - this.Plane) + this.Begin * this.Plane;

            var w = (int)Math.Abs(this.Target.Global.X - this.Begin.X) + 1;
            var h = (int)Math.Abs(this.Target.Global.Y - this.Begin.Y) + 1;
            if (w != this.Width || h != this.Height)
                this.Valid = this.Check(w, h);
            this.Width = w;
            this.Height = h;
        }

        private bool Check(int w, int h)
        {
            if (w < 1)//2)
                return false;
            if (h < 1)//2)
                return false;
            var positions = this.GetPositions(w, h);
            foreach (var pos in positions)
            {
                if (ValidityCheck != null)
                {
                    if (!ValidityCheck(pos))
                        return false;
                    else
                        continue;
                }
                
                if (Engine.Map.IsSolid(pos))
                    return false;
                if (!Engine.Map.IsSolid(pos - Vector3.UnitZ))
                    return false;
            }
            return true;
        }
        public bool RestrictZ;
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Enabled)
                return Messages.Default;
            if(this.Target == null)
                return Messages.Default;
            if(this.Target.Type != TargetType.Position)
                return Messages.Default;
            var pos = this.Target.Global;
            
            if (this.GetZones().Contains(pos))
                this.Removing = true;
            this.Plane = RestrictZ ? Vector3.UnitZ : this.Target.Face;
            this.Begin = pos;
            this.End = this.Begin;
            this.Width = this.Height = 1;
            this.Enabled = true;
            this.Valid = this.Check(this.Width, this.Height);
            return Messages.Default;
        }

        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Target == null)
                return Messages.Default;
            if (this.Target.Type != TargetType.Position)
                return Messages.Default;
            if (!this.Check(this.Width, this.Height))
                return Messages.Default;
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            int z = (int)Math.Min(this.Begin.Z, this.End.Z);

            int xx = (int)(this.Begin.X + this.End.X - x);
            int yy = (int)(this.Begin.Y + this.End.Y - y);
            int zz = (int)(this.Begin.Z + this.End.Z - z);

            var rect = new Rectangle(x, y, this.Width, this.Height);

            var begin = new Vector3(x, y, z);
            var end = new Vector3(xx, yy, zz);

            this.Add(begin, end, IsRemoving());

            this.Removing = false;
            this.Enabled = false;
            return Messages.Default;
        }

        public override ControlTool.Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Enabled)
            {
                this.Enabled = false;
                return Messages.Default;
            }
            else
                return Messages.Remove;
        }
        
        List<Vector3> GetPositions(int w, int h)
        {
            List<Vector3> list = new List<Vector3>();
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            for (int i = x; i < x + w; i++)
                for (int j = y; j < y + h; j++)
                    list.Add(new Vector3(i, j, this.Begin.Z));
            return list;
        }
        Icon Icon = new Icon(UI.UIManager.Icons32, 12, 32);
        internal override void DrawUI(SpriteBatch sb, Camera camera)
        {
            base.DrawUI(sb, camera); 
            
            Icon.Draw(sb, UI.UIManager.Mouse);
            if (this.IsRemoving())
            {
                var icondelete = Icon.Cross;
                icondelete.Draw(sb, UI.UIManager.Mouse + new Vector2(Icon.SourceRect.Width / 2, 0));
            }
        }

        private bool IsRemoving()
        {
            return this.Removing || InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey);
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera camera)
        {
            this.DrawGrid(sb, camera);
            camera.DrawGridCells(sb, Color.Yellow, this.GetZones());
            base.DrawBeforeWorld(sb, map, camera);
        }

        void DrawGrid(MySpriteBatch sb, Camera cam)
        {
            if (!this.Enabled)
                return;
            var col = this.Valid ? Color.Lime : Color.Red;
            cam.DrawGridBlocks(sb, this.Begin.GetBox(this.End), col);
        }
    }
}
