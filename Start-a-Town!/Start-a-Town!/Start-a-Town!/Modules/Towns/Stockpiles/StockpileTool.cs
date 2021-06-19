using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Towns.Stockpiles
{
    class StockpileTool : DefaultTool
    {
        Sprite GridSprite = Sprite.BlockFaceHighlights[Vector3.UnitZ];
        Vector3 Begin, End;
        int Width, Height;
        bool Enabled;
        bool Valid;
        Action<Stockpile> Callback;
        Town Town;
        public StockpileTool(Town town)
        {
            this.Town = town;
        }
        public StockpileTool(Action<Stockpile> callback)
        {
            this.Callback = callback;
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

            //this.Width = (int)Math.Abs(this.Target.Global.X - this.Begin.X + 1);
            //this.Height = (int)Math.Abs(this.Target.Global.Y - this.Begin.Y + 1);
            this.End = new Vector3(this.Target.Global.XY(), this.Begin.Z);
            var w = (int)Math.Abs(this.Target.Global.X - this.Begin.X) + 1;
            var h = (int)Math.Abs(this.Target.Global.Y - this.Begin.Y) + 1;
            if (w != this.Width || h != this.Height)
                this.Valid = this.Check(w, h);
                //this.Invalidate();
            this.Width = w;
            this.Height = h;
            
        }

        private void Create(Stockpile stockpile)
        {
            if (stockpile == null)
                return;
            //new TownsPacketHandler()
            //    .Send(new PacketCreateStockpile(Player.Actor.Network.ID, stockpile));
        }

        internal void DeleteStockpileAt(Vector3 pos)
        {
            foreach (var item in this.Town.Stockpiles.Values.ToList())
            {
                if(item.Positions.Contains(pos))
                    new TownsPacketHandler()
                        .Send(new PacketDeleteStockpile(item.ID));
            //    var box = new BoundingBox(item.Begin, item.End);
            //    if (box.Contains(pos) == ContainmentType.Contains)
            //    {
            //        new TownsPacketHandler()
            //            .Send(new PacketDeleteStockpile(item.ID));
            //    }
            }
        }

        private bool Check(int w, int h)
        {
            if (w < 2)
                return false;
            if (h < 2)
                return false;
            var positions = this.GetPositions(w, h);
            foreach (var pos in positions)
            {
                //if (pos.IsSolid(Engine.Map))
                if (Engine.Map.IsSolid(pos))
                    return false;
                //if (!(pos - Vector3.UnitZ).IsSolid(Engine.Map))
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
                DeleteStockpileAt(pos);
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
            //if (!this.IsValid())
            //this.Valid = this.Validate();
            if (!this.Valid)
                return Messages.Default;
                //this.Enabled = false;
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            var begin = new Vector3(x, y, this.Begin.Z);
            var stockpile = new Stockpile(begin, this.Width, this.Height);
            //var town = new Town(Engine.Map);
            //town.AddStockpile(stockpile);
            //this.Callback(stockpile);
            //this.Create(stockpile);
            PacketCreateStockpile.Send(Player.Actor.Network.ID, 0, begin, this.Width, this.Height);

            this.Enabled = false;
            return Messages.Default;// Remove;
        }

        public override ControlTool.Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            //this.Callback(null);
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
            //Game1.Instance.GraphicsDevice.Textures[0] = Block.Atlas.Texture;
            Game1.Instance.GraphicsDevice.Textures[0] = Sprite.Atlas.Texture;
            Game1.Instance.GraphicsDevice.Textures[1] = Sprite.Atlas.DepthTexture;
            this.DrawGrid(sb, camera);
            var stockpiles = map.GetTown().Stockpiles;
            foreach (var s in stockpiles.Values)
                foreach(var pos in s.Positions)
                    this.DrawGridCell(sb, camera, Color.Yellow, pos);
                //this.DrawGrid(sb, camera, s.Begin, s.Width, s.Height, Color.Yellow);
            base.DrawBeforeWorld(sb, map, camera);
        }
        void DrawGrid(MySpriteBatch sb, Camera cam)
        {
            //this.Mouseover = this.NextMouseover;
            //this.NextMouseover = null;
            //foreach (var chunk in Engine.Map.ActiveChunks.Values)
            //{
            if (!this.Enabled)
                return;
            //var col = this.Invalidate() ? Color.Lime : Color.Red;
            var col = this.Valid ? Color.Lime : Color.Red;
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            //for (int i = 0; i < this.Width; i++)
            //    for (int j = 0; j < this.Height; j++)
            for (int i = x; i < x + this.Width; i++)
                for (int j = y; j < y + this.Height; j++)
                {
                    Vector3 global = new Vector3(i, j, this.Begin.Z);
                      
                    var bounds = cam.GetScreenBounds(global, Block.Bounds);
                    var pos = new Vector2(bounds.X, bounds.Y);
                    //var gd = Game1.Instance.GraphicsDevice;
                    var depth = global.GetDrawDepth(Engine.Map, cam);
                    //Color color;

                    sb.Draw(Sprite.Atlas.Texture, pos, GridSprite.AtlasToken.Rectangle, 0, Vector2.Zero, cam.Zoom, col, SpriteEffects.None, depth);

                    //if (this.Mouseover != null)
                    //    if (this.Mouseover.Global == global)
                    //        sb.Draw(Sprite.Atlas.Texture, pos, GridSprite.AtlasToken.Rectangle, 0, Vector2.Zero, cam.Zoom, Color.Red, SpriteEffects.None, depth);

                    //this.HitTest(global, bounds, cam);


                    //gd.SamplerStates[0] = cam.Zoom >= 1 ? SamplerState.PointClamp : SamplerState.AnisotropicClamp;
                    //gd.SamplerStates[1] = cam.Zoom >= 1 ? SamplerState.PointClamp : SamplerState.AnisotropicClamp;
                }
        //}
        }
        void DrawGrid(MySpriteBatch sb, Camera cam, Vector3 global, int w, int h, Color c)
        {
            this.DrawGrid(sb, cam, (int)global.X, (int)global.Y, (int)global.Z, w, h, c);
        }
        void DrawGrid(MySpriteBatch sb, Camera cam, int x, int y, int z, int w, int h, Color col)
        {
            for (int i = x; i < x + w; i++)
                for (int j = y; j < y + h; j++)
                {
                    Vector3 global = new Vector3(i, j, z);

                    DrawGridCell(sb, cam, col, global);
                }
        }

        private void DrawGridCell(MySpriteBatch sb, Camera cam, Color col, Vector3 global)
        {
            var bounds = cam.GetScreenBounds(global, Block.Bounds);
            var pos = new Vector2(bounds.X, bounds.Y);
            var depth = global.GetDrawDepth(Engine.Map, cam);

            sb.Draw(Sprite.Atlas.Texture, pos, GridSprite.AtlasToken.Rectangle, 0, Vector2.Zero, cam.Zoom, col, SpriteEffects.None, depth);
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
                var icondelete = Icon.Cross;// new Icon(UI.UIManager.Icons16x16, 0, 16);
                icondelete.Draw(sb, UI.UIManager.Mouse + new Vector2(Icon.SourceRect.Width / 2, 0));
            }
            //sb.Draw(Icon.SpriteSheet, UI.UIManager.Mouse, Icon.SourceRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }
    }
}
