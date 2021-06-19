using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.GameModes;
using Start_a_Town_.Towns.Housing;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_.Towns
{
    class HouseTool : ControlTool
    {
        Sprite GridSprite = Sprite.BlockFaceHighlights[Vector3.UnitZ];
        Vector3 Begin, End;
        int Width, Depth;
        bool Enabled;
        bool Valid;
        Town Town;
        Action<Vector3> CreateCallback;
        Action<Vector3> RemoveCallback;

        public HouseTool()
        {

        }
        public HouseTool(Town town)
        {
            this.Town = town;
        }
        public HouseTool(Action<Vector3> addcallback, Action<Vector3> removecallback)
        {
            this.CreateCallback = addcallback;
            this.RemoveCallback = removecallback;
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
            if (w != this.Width || h != this.Depth)
                this.Valid = this.Check(w, h);

            this.Width = w;
            this.Depth = h;
        }

        internal void DeleteHouseAt(Vector3 pos)
        {
            //foreach (var item in this.Town.Stockpiles.Values.ToList())
            //{
            //    var box = new BoundingBox(item.Global, item.End);
            //    if (box.Contains(pos) == ContainmentType.Contains)
            //    {
            //        //this.Stockpiles.Remove(item.ID);
            //        // send delete stockpile packet
            //        new TownsPacketHandler()
            //            .Send(new PacketDeleteStockpile(item.ID));
            //    }
            //}
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
                //if (Engine.Map.IsSolid(pos))
                //    return false;
                //if (!Engine.Map.IsSolid(pos - Vector3.UnitZ))
                //    return false;
                var maxZ = Client.Instance.Map.GetMaxHeight();
                int z = (int)pos.Z;
                while (z < maxZ && !Client.Instance.Map.GetCell(new Vector3(pos.X, pos.Y, z)).Opaque)
                    z++;
                if (z >= maxZ - 1)
                    return false;
            }
            return true;
        }
        static int DetectHeight(IMap map, Vector3 start, Vector3 end)
        {
            //var w = (int)(end.X - start.X);
            //var h = (int)(end.Y - start.Y);
            //var positions = GetPositions(start, end);
            //foreach (var pos in positions)
            //{
                var maxZ = map.GetMaxHeight();
                int z = (int)start.Z;
            //    while (z < maxZ && !map.GetCell(new Vector3(pos.X, pos.Y, z)).Opaque)
            //        z++;
            //    return z;
            //}
                var cell = map.GetCell(new Vector3(start.X, start.Y, z));
                while (z < maxZ && !cell.Opaque)
                {
                    z++;
                    cell = map.GetCell(new Vector3(start.X, start.Y, z));
                };
                var h = z - (int)start.Z;
                return h;
        }

        public override ControlTool.Messages MouseLeftPressed(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;
            
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
                this.DeleteHouseAt(pos);
                return Messages.Default;
            }
            this.Begin = pos;
            //this.Enabled = true;
            return Messages.Default;
        }

        public override ControlTool.Messages MouseLeftUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            if (e.Handled)
                return Messages.Default;
            
            if (this.Target == null)
                return Messages.Default;
            if (this.Target.Type != TargetType.Position)
                return Messages.Default;

            if (!this.Enabled)
            {
                if (InputState.IsKeyDown(System.Windows.Forms.Keys.LControlKey))
                {
                    this.RemoveCallback(this.Target.FaceGlobal);
                    return Messages.Default;
                }

                this.CreateCallback(this.Target.FaceGlobal);
                return Messages.Default;

                //var shouse = FloodFill(Server.Instance.Map, this.Target.FaceGlobal);
                //var chouse = FloodFill(Client.Instance.Map, this.Target.FaceGlobal);
                ////var hhouse = new House(new Town(Engine.Map), enterior);
                //Client.Instance.Map.GetTown().AddHouse(chouse);
                //Server.Instance.Map.GetTown().AddHouse(shouse);
                ////Create(hhouse);
                //return Messages.Default;

                //this.Enabled = true;
                //return Messages.Default;
            }

            //if (this.Target.Face != Vector3.UnitZ)
            //    return Messages.Default;
            //if (!this.Valid)
            //    return Messages.Default;
            //int x = (int)Math.Min(this.Begin.X, this.End.X);
            //int y = (int)Math.Min(this.Begin.Y, this.End.Y);

            //// find house height
            //var height = DetectHeight(Client.Instance.Map, this.Begin, this.End);
            //var house = new House(new Town(Engine.Map), new Vector3(x, y, this.Begin.Z), this.Width, this.Depth, height);
            //Create(house);
            //this.Enabled = false;
            return Messages.Remove;
        }

        static House FloodFill(IMap map, Vector3 global)
        {
            HashSet<Vector3> enterior = new HashSet<Vector3>(); // add global to enterior straight away?
            HashSet<Vector3> walls = new HashSet<Vector3>(); // add global to enterior straight away?
            Queue<Vector3> tocheck = new Queue<Vector3>();
            HashSet<Vector3> handled = new HashSet<Vector3>() { global };
            tocheck.Enqueue(global);
            while (tocheck.Count > 0)
            {
                var current = tocheck.Dequeue();
                var block = map.GetBlock(current);
                if (block == null)
                    "kala".ToConsole();
                if (block.Opaque) // check if block is air instead of opacity? put a field in each block specifying wether it can be considered a valid house wall?
                {
                    walls.Add(current);
                    continue;
                }
                else
                {
                    var heightmap = map.GetHeightmapValue(current);
                    if (current.Z > heightmap)
                        continue;
                    enterior.Add(current);
                }
                foreach (var n in current.GetNeighborsDiag())//.GetNeighbors())
                    if (!handled.Contains(n))
                    {
                        handled.Add(n);
                        tocheck.Enqueue(n);
                    }
            }

            var sortedwalls = walls.ToList();
            sortedwalls.Sort((w1, w2) =>
            {
                if (w1.Z < w2.Z)
                    return -1;
                if (w1.Z > w2.Z)
                    return 1;
                return 0;
            });

            var house = new House(map.GetTown(), enterior, sortedwalls);
            return house;
        }

        static void Create(House house)
        {
            // TODO fix networking properly
            Client.Instance.Map.GetTown().AddHouse(house);
            Server.Instance.Map.GetTown().AddHouse(house);
        }
        
        public override ControlTool.Messages MouseRightUp(System.Windows.Forms.HandledMouseEventArgs e)
        {
            return Messages.Remove;
        }
        internal override void DrawBeforeWorld(MySpriteBatch sb, IMap map, Camera camera)
        {
            this.DrawGrid(sb, camera);
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
                for (int j = y; j < y + this.Depth; j++)
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

        List<Vector3> GetPositions()
        {
            List<Vector3> list = new List<Vector3>();
            int x = (int)Math.Min(this.Begin.X, this.End.X);
            int y = (int)Math.Min(this.Begin.Y, this.End.Y);
            for (int i = x; i < x + this.Width; i++)
                for (int j = y; j < y + this.Depth; j++)
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
        //static List<Vector3> GetPositions(Vector3 start, Vector3 end)
        //{
        //    List<Vector3> list = new List<Vector3>();
        //    int x = (int)Math.Min(start.X, end.X);
        //    int y = (int)Math.Min(start.Y, end.Y);
        //    for (int i = x; i < x + w; i++)
        //        for (int j = y; j < y + h; j++)
        //            list.Add(new Vector3(i, j, start.Z));
        //    return list;
        //}
        new Icon Icon = new Icon(UI.UIManager.Icons32, 12, 32);
        internal override void DrawUI(SpriteBatch sb, Camera camera)
        {
            base.DrawUI(sb, camera); 
            
            Icon.Draw(sb, UI.UIManager.Mouse);
            if(InputState.IsKeyDown(System.Windows.Forms.Keys.ControlKey))
            {    
                var icondelete = new Icon(UI.UIManager.Icons16x16, 0, 16);
                icondelete.Draw(sb, UI.UIManager.Mouse + new Vector2(Icon.SourceRect.Width / 2, 0));
            }
            //sb.Draw(Icon.SpriteSheet, UI.UIManager.Mouse, Icon.SourceRect, Color.White, 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }
    }
}
