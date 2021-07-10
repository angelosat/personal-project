using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    [Obsolete]
    class ToolZoningPositions : ControlTool
    {
        Sprite GridSprite = Sprite.BlockFaceHighlights[Vector3.UnitZ];
        Vector3 Begin, End;
        int Width, Height;
        bool Enabled;
        bool Valid;
        bool Removing;
        protected Action<Vector3, int, int, bool> Add, Remove;
        public Func<Vector3, bool> IsValid;
        protected Func<List<Vector3>> GetZones = () => new List<Vector3>();

        public ToolZoningPositions(Action<Vector3, int, int, bool> callback)
            : this(callback, () => new List<Vector3>())
        {
        }
        public ToolZoningPositions(Action<Vector3, int, int, bool> callback, Func<List<Vector3>> zones)
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
                if (IsValid != null)
                    if (IsValid(pos))
                        continue;

                if (!Engine.Map.IsSolid(pos))
                    return false;
                if (Engine.Map.IsSolid(pos + Vector3.UnitZ))
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

            var existingPositions = this.GetZones();
            if (existingPositions.Contains(pos))
                this.Removing = true;
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
            if (this.Target.Face != Vector3.UnitZ)
                return Messages.Default;
            if (!this.Check(this.Width, this.Height))
                return Messages.Default;
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            var rect = new Rectangle(x, y, this.Width, this.Height);

            var begin = new Vector3(x, y, this.Begin.Z);
            var end = new Vector3(x + this.Width - 1, y + this.Height - 1, this.Begin.Z);
            this.Add(begin, this.Width, this.Height, IsRemoving());
           
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
            if (!this.Enabled)
                return;
            UIManager.DrawStringOutlined(sb, string.Format("{0} x {1}", this.Width, this.Height), UIManager.Mouse, Vector2.UnitY);
        }

        private bool IsRemoving()
        {
            return this.Removing || InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey);
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, MapBase map, Camera camera)
        {
            var allPositions = new HashSet<Vector3>(this.GetZones());
            var selectedPositions = new HashSet<Vector3>();
            if (this.Enabled)
            {
                var x = Math.Min(this.Begin.X, this.End.X);
                var y = Math.Min(this.Begin.Y, this.End.Y);
                var xx = Math.Max(this.Begin.X, this.End.X);
                var yy = Math.Max(this.Begin.Y, this.End.Y);
                var a = new Vector3(x, y, this.Begin.Z);
                var b = new Vector3(xx, yy, this.Begin.Z);
                BoundingBox box = new BoundingBox(a, b);
                foreach (var p in box.GetBox())
                {
                    allPositions.Add(p);
                    selectedPositions.Add(p + Vector3.UnitZ);
                }
            }
            Game1.Instance.GraphicsDevice.Textures[0] = Sprite.Atlas.Texture;
            Game1.Instance.GraphicsDevice.Textures[1] = Sprite.Atlas.DepthTexture;
            foreach (var pos in allPositions)
                this.DrawGridCell(sb, camera, Color.Yellow, pos + Vector3.UnitZ, selectedPositions);
        }
       
        private void DrawGridCell(MySpriteBatch sb, Camera cam, Color col, Vector3 global, HashSet<Vector3> selected)
        {
            var bounds = cam.GetScreenBounds(global, Block.Bounds);
            var pos = new Vector2(bounds.X, bounds.Y);
            var depth = global.GetDrawDepth(Engine.Map, cam);
          
            if (Enabled)
                if (selected.Contains(global))
                    col = IsRemoving() ? Color.Red : Color.Lime;
           
            sb.Draw(Sprite.Atlas.Texture, pos, GridSprite.AtlasToken.Rectangle, 0, Vector2.Zero, cam.Zoom, col, SpriteEffects.None, depth);
        }
    }
}
