﻿using System;
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
    class ToolZoning : ControlTool
    {
        enum States { Inactive, Adding, Removing, Selecting }
        States State;
        Sprite GridSprite = Sprite.BlockFaceHighlights[Vector3.UnitZ];
        Vector3 Begin, End;
        int Width, Height;
        bool Enabled;
        bool Valid;

        //Action<Rectangle> Callback;
        readonly Action<Vector3, int, int, bool> Add;//, Remove;
        public Func<Vector3, bool> IsValid;
        Func<List<Zone>> GetZones = () => new List<Zone>();

        Town Town;
        public ToolZoning(Town town)
        {
            this.Town = town;
        }
        public ToolZoning(Action<Vector3, int, int, bool> callback)
            : this(callback, () => new List<Zone>())
        {
            //this.Add = callback;
        }
        public ToolZoning(Action<Vector3, int, int, bool> callback, Func<List<Zone>> zones)
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

            if (this.State == States.Selecting)
            {
                //this.End = this.Begin;
                this.Width = this.Height = 1;
                this.Valid = true;
                return;
            }

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
                if (IsValid != null)
                    if (IsValid(pos))
                        continue;

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

            //if (InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            //{
            //    //DeleteStockpileAt(pos);
            //    //return Messages.Default;
            //    this.State = States.Selecting;
            //}
            //else
            foreach (var z in this.GetZones())
                if (z.Contains(pos))
                    this.State = States.Selecting;

           
            this.Begin = this.End = pos;
            this.Width = this.Height = 1; 
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

            
            //if (IsRemoving()) // if we are removing then we only need one position, the current mouse position (end)
            //{
            //    this.Add(this.End, 1, 1, true);
            //}
            //else
            //{

            if(this.State == States.Selecting)
                if(this.Begin != this.End)
                {
                    this.Enabled = false;
                    this.State = States.Inactive;
                    return Messages.Default;
                }
           
                int x = (int)Math.Min(this.Begin.X, this.End.X);
                int y = (int)Math.Min(this.Begin.Y, this.End.Y);

                var rect = new Rectangle(x, y, this.Width, this.Height);

                var begin = new Vector3(x, y, this.Begin.Z);
                var end = new Vector3(x + this.Width - 1, y + this.Height - 1, this.Begin.Z);
                this.Add(begin, this.Width, this.Height, IsRemoving());
            //}
                this.State = States.Inactive;
            this.Enabled = false;
            return Messages.Default;
        }

        public override ControlTool.Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (this.Enabled)
            {
                this.Enabled = false;
                this.State = States.Inactive;
                return Messages.Default;
            }
            else
                return Messages.Remove;
        }
        //internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera camera)
        //{
        //    this.DrawGrid(sb, camera);
        //    base.DrawBeforeWorld(sb, map, camera);
        //}
        void DrawGrid(MySpriteBatch sb, Camera cam)
        {
            if (!this.Enabled)
                return;
            if (this.State == States.Selecting)
            {
                DrawGridCell(sb, cam, Color.White, this.Begin);
                return;
            }
            var col = this.Valid ? Color.Lime : Color.Red;
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            for (int i = x; i < x + this.Width; i++)
                for (int j = y; j < y + this.Height; j++)
                {
                    Vector3 global = new Vector3(i, j, this.Begin.Z);

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
            if (IsRemoving())
            {
                var icondelete = Icon.Cross;
                icondelete.Draw(sb, UI.UIManager.Mouse + new Vector2(Icon.SourceRect.Width / 2, 0));
            }
        }

        private static bool IsRemoving()
        {
            return InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey);
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera camera)
        {
            Game1.Instance.GraphicsDevice.Textures[0] = Sprite.Atlas.Texture;
            Game1.Instance.GraphicsDevice.Textures[1] = Sprite.Atlas.DepthTexture;
            this.DrawGrid(sb, camera);
            //var stockpiles = map.GetTown().Stockpiles;
            //foreach (var s in stockpiles.Values)
            //    this.DrawGrid(sb, camera, s.Global, s.Width, s.Height, Color.Yellow);
            foreach(var z in this.GetZones())
                this.DrawGrid(sb, camera, z.Begin, z.Width, z.Height, Color.Yellow);

            base.DrawBeforeWorld(sb, map, camera);
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

                    var bounds = cam.GetScreenBounds(global, Block.Bounds);
                    var pos = new Vector2(bounds.X, bounds.Y);
                    var depth = global.GetDrawDepth(Engine.Map, cam);

                    sb.Draw(Sprite.Atlas.Texture, pos, GridSprite.AtlasToken.Rectangle, 0, Vector2.Zero, cam.Zoom, col, SpriteEffects.None, depth);
                }
        }
    }
}
