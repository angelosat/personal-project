using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    class ToolZoningPositionsNew : ToolManagement
    {
        protected Vector3 Begin, End;
        int Width, Height;
        bool Enabled;
        bool Removing;
        protected Action<Vector3, int, int, bool> Add, Remove;
        public Func<Vector3, bool> IsValid;
        protected Func<List<ZoneNew>> GetZones = () => new List<ZoneNew>();
        public override bool TargetOnlyBlocks => true;
        protected Town Town;
        
        public ToolZoningPositionsNew(Action<Vector3, int, int, bool> callback, Func<List<ZoneNew>> zones)
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

            this.End = new Vector3(this.Target.Global.XY(), this.Begin.Z);
            var w = (int)Math.Abs(this.Target.Global.X - this.Begin.X) + 1;
            var h = (int)Math.Abs(this.Target.Global.Y - this.Begin.Y) + 1;
            this.Width = w;
            this.Height = h;
        }
        private bool IsValidShape(int w, int h)
        {
            if (w < 1)
                return false;
            if (h < 1)
                return false;
            return true;
            if (this.IsRemoving())
                return true;
            var positions = this.GetPositions(w, h);
            foreach (var pos in positions)
            {
                if (IsValid != null)
                    if (IsValid(pos))
                        continue;
                if (!ZoneNew.IsPositionValid(this.Town.Map, pos))
                    return false;
            }
            return true;
        }
       
        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Enabled)
                return Messages.Default;
            if(this.Target == null)
                return Messages.Default;
            if(this.Target.Type != TargetType.Position)
                return Messages.Default;
            if (this.Target.Face != Vector3.UnitZ)
                return Messages.Default;
            var pos = this.Target.Global;// +this.Target.Face; //DO I WANT the zone to contain the solid blocks under the empty space? or the empty space itself???

            this.Begin = pos;
            this.End = this.Begin;
            this.Width = this.Height = 1;
            this.Enabled = true;
            Sync();
            return Messages.Default;
        }

        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (!this.Enabled)
                return Messages.Default;
            if (this.Target == null)
                return Messages.Default;
            if (this.Target.Type != TargetType.Position)
                return Messages.Default;
            if (this.Target.Face != Vector3.UnitZ)
                return Messages.Default;
            if (!this.IsValidShape(this.Width, this.Height))
                return Messages.Default;
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            var rect = new Rectangle(x, y, this.Width, this.Height);

            var begin = new Vector3(x, y, this.Begin.Z);
            var end = new Vector3(x + this.Width - 1, y + this.Height - 1, this.Begin.Z);
            this.Add(begin, this.Width, this.Height, IsRemoving());
            
            this.Removing = false;
            this.Enabled = false;
            Sync();
            return Messages.Default;
        }

        public override ControlTool.Messages MouseRightDown(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Enabled)
            {
                this.Enabled = false;
                Sync();
                return Messages.Default;
            }
            else
                return Messages.Remove;
        }
        
        public override void UpdateRemote(TargetArgs target)
        {
            if (target.Type == TargetType.Position)
                this.End = new Vector3(target.Global.XY(), this.Begin.Z);
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
            if (!this.Enabled)
                return;
            UIManager.DrawStringOutlined(sb, $"{this.Width} x {this.Height}", UIManager.Mouse, Vector2.UnitY);
        }

        private bool IsRemoving()
        {
            return this.Removing || InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey);
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera camera)
        {
            DrawGrid(sb, map, camera, this.IsRemoving() ? Color.Red : Color.Lime);
        }

        private void DrawGrid(MySpriteBatch sb, IMap map, Camera camera, Color color)
        {
            if (!this.Enabled)
                return;
            var validpositions = GetPositions(map, this.Begin, this.End);
            camera.DrawGridCells(sb, color *.5f, validpositions);
        }

        private IEnumerable<Vector3> GetPositions(IMap map, Vector3 a, Vector3 b)
        {
            var validpositions = a.GetBox(b).Where(v => ZoneNew.IsPositionValid(map, v)).Select(v => v.Above());
            return validpositions;
        }
       
        protected override void WriteData(System.IO.BinaryWriter w)
        {
            w.Write(this.Enabled);
            w.Write(this.Begin);
        }
        protected override void ReadData(System.IO.BinaryReader r)
        {
            this.Enabled = r.ReadBoolean();
            this.Begin = r.ReadVector3();
        }
        internal override void DrawAfterWorldRemote(MySpriteBatch sb, IMap map, Camera camera, Net.PlayerData player)
        {
            DrawGrid(sb, map, camera, Color.Red);
        }
    }
}
