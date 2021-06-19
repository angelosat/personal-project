using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.GameModes;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    class ToolDesignate : ControlTool
    {
        Sprite GridSprite = Sprite.BlockFaceHighlights[Vector3.UnitZ];
        Vector3 Begin, End;
        int Width, Height;
        bool Enabled;
        bool Valid;
        //Action<Rectangle> Callback;
        Action<Vector3, int, int> Add;//, Remove;
        //Predicate<Vector3> IsValid;

        //Town Town;
        //public ToolDesignate(Town town)
        //{
        //    this.Town = town;
        //}
        public ToolDesignate(Action<Vector3, int, int> callback)
        {
            this.Add = callback;
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

        //private void Create(Stockpile stockpile)
        //{
        //    if (stockpile == null)
        //        return;
        //    new TownsPacketHandler()
        //        .Send(new PacketCreateStockpile(Player.Actor.InstanceID, stockpile));
        //}

        //internal void DeleteStockpileAt(Vector3 pos)
        //{
        //    foreach (var item in this.Town.Stockpiles.Values.ToList())
        //    {
        //        var box = new BoundingBox(item.Begin, item.End);
        //        if (box.Contains(pos) == ContainmentType.Contains)
        //        {
        //            new TownsPacketHandler()
        //                .Send(new PacketDeleteStockpile(item.ID));
        //        }
        //    }
        //}

        private bool Check(int w, int h)
        {
            if (w < 2)
                return false;
            if (h < 2)
                return false;
            var positions = this.GetPositions(w, h);
            foreach (var pos in positions)
            {
                if (Engine.Map.IsSolid(pos))
                    return false;
                if (!Engine.Map.IsSolid(pos - Vector3.UnitZ))
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
            var pos = this.Target.Global + this.Target.Face;
            if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            {
                //DeleteStockpileAt(pos);
                return Messages.Default;
            }
            this.Begin = pos;
            this.Enabled = true;
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
            if (!this.Valid)
                return Messages.Default;
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            //var stockpile = new Stockpile(new Town(Engine.Map), new Vector3(x, y, this.Begin.Z), this.Width, this.Height);
            //this.Create(stockpile);
            var rect = new Rectangle(x, y, this.Width, this.Height);

            var begin = new Vector3(x, y, this.Begin.Z - 1);
            var end = new Vector3(x + this.Width - 1, y + this.Height - 1, this.Begin.Z - 1);
            this.Add(begin, this.Width, this.Height);
            //for (int i = 0; i < this.Width; i++)
            //{
            //    var xx = x + i;
            //    for (int j = 0; j < this.Height; j++)
            //    {
            //        var yy = y + j;
            //        this.Add(new Vector3(xx, yy, this.Begin.Z - 1));
            //    }
            //}
            this.Enabled = false;
            return Messages.Remove;
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
        internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera camera)
        {
            this.DrawGrid(sb, camera);
            base.DrawBeforeWorld(sb, map, camera);
        }
        void DrawGrid(MySpriteBatch sb, Camera cam)
        {
            if (!this.Enabled)
                return;
            var col = this.Valid ? Color.Lime : Color.Red;
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            for (int i = x; i < x + this.Width; i++)
                for (int j = y; j < y + this.Height; j++)
                {
                    Vector3 global = new Vector3(i, j, this.Begin.Z);
                      
                    var bounds = cam.GetScreenBounds(global, Block.Bounds);
                    var pos = new Vector2(bounds.X, bounds.Y);
                    var depth = global.GetDrawDepth(Engine.Map, cam);
                    sb.Draw(Sprite.Atlas.Texture, pos, GridSprite.AtlasToken.Rectangle, 0, Vector2.Zero, cam.Zoom, col, SpriteEffects.None, depth);
                }
        }

        List<Vector3> GetPositions()
        {
            List<Vector3> list = new List<Vector3>();
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            for (int i = x; i < x + this.Width; i++)
                for (int j = y; j < y + this.Height; j++)
                    list.Add(new Vector3(i, j, this.Begin.Z));
            return list;
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
            if(InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            {
                var icondelete = Icon.Cross;
                icondelete.Draw(sb, UI.UIManager.Mouse + new Vector2(Icon.SourceRect.Width / 2, 0));
            }
        }
    }
}
