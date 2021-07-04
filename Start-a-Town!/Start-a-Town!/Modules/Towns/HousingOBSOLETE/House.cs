using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Towns;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Towns.Housing
{
    public class House : IEnterior
    {
        static public bool HideRoof = true, HideWalls = true, HideCeiling = true;

        public int ID;
        public Town Town;
        public string Name = "";//untitled";
        BoundingBox? _Box;
        public BoundingBox Box
        {
            set { _Box = value; }
            get
            {
                if (_Box == null)
                    this.FindMinBox();
                return _Box.Value;
            }
        }
        public HashSet<Vector3> Enterior = new HashSet<Vector3>();
        public List<Vector3> Walls = new List<Vector3>();
        public List<Vector3> EastWalls = new List<Vector3>();
        public List<Vector3> SouthWalls = new List<Vector3>();
        public List<Vector3> WestWalls = new List<Vector3>();
        public List<Vector3> NorthWalls = new List<Vector3>();
        public List<Vector3> EnteriorWalls = new List<Vector3>();
        public List<Vector3> Roof = new List<Vector3>();
        public List<Vector3> Floor = new List<Vector3>();
        public List<Vector3> Ceiling = new List<Vector3>();
        //MySpriteBatch WallsMesh, EastWallsMesh, SouthWallsMesh, WestWallsMesh, NorthWallsMesh, FloorsMesh, RoofMesh, CeilingMesh, EnteriorMesh;
        Canvas WallsMesh, EastWallsMesh, SouthWallsMesh, WestWallsMesh, NorthWallsMesh, FloorsMesh, RoofMesh, CeilingMesh, EnteriorMesh;

        //IMap GetMap()
        //{
        //    return this.topwn
        //}
        bool Valid;
        public void Invalidate()
        {
            this.Valid = false;
        }
        public void Validate()
        {
            this.Valid = true;
            this.EastWallsMesh = new Canvas(Game1.Instance.GraphicsDevice);
            this.WestWallsMesh = new Canvas(Game1.Instance.GraphicsDevice);
            this.SouthWallsMesh = new Canvas(Game1.Instance.GraphicsDevice);
            this.NorthWallsMesh = new Canvas(Game1.Instance.GraphicsDevice);
            this.FloorsMesh = new Canvas(Game1.Instance.GraphicsDevice);
            this.RoofMesh = new Canvas(Game1.Instance.GraphicsDevice);
            this.CeilingMesh = new Canvas(Game1.Instance.GraphicsDevice);
            this.EnteriorMesh = new Canvas(Game1.Instance.GraphicsDevice);
            this.WallsMesh = new Canvas(Game1.Instance.GraphicsDevice);

            foreach (var wall in this.Walls)
                ScreenManager.CurrentScreen.Camera.DrawCell(this.WallsMesh, this.Town.Map, wall);
            foreach (var wall in this.EastWalls)
                ScreenManager.CurrentScreen.Camera.DrawCell(this.EastWallsMesh, this.Town.Map, wall);
            foreach (var wall in this.WestWalls)
                ScreenManager.CurrentScreen.Camera.DrawCell(this.WestWallsMesh, this.Town.Map, wall);
            foreach (var wall in this.SouthWalls)
                ScreenManager.CurrentScreen.Camera.DrawCell(this.SouthWallsMesh, this.Town.Map, wall); 
            foreach (var wall in this.NorthWalls)
                ScreenManager.CurrentScreen.Camera.DrawCell(this.NorthWallsMesh, this.Town.Map, wall);
            foreach (var wall in this.Floor)
                ScreenManager.CurrentScreen.Camera.DrawCell(this.FloorsMesh, this.Town.Map, wall);
            foreach(var w in this.Roof)
                ScreenManager.CurrentScreen.Camera.DrawCell(this.RoofMesh, this.Town.Map, w);
            foreach (var w in this.Ceiling)
                ScreenManager.CurrentScreen.Camera.DrawCell(this.CeilingMesh, this.Town.Map, w);     
            foreach(var w in this.Enterior)
                ScreenManager.CurrentScreen.Camera.DrawCell(this.EnteriorMesh, this.Town.Map, w);
        }

        public House(Town town, Vector3 global, int width, int depth, int height)
        {
            this.Town = town;
            this.Box = new BoundingBox(global, global + new Vector3(width, depth, height) - Vector3.One);
        }
        public House(Town town, HashSet<Vector3> enterior, List<Vector3> walls)
        {
            this.Town = town;
            this.Enterior = enterior;
            this.Walls = walls;


            DistinguishWalls();
        }
        public House(Town town, BinaryReader r)
        {
            this.Town = town;
            this.Read(r);
        }
        public House(Town town, SaveTag save)
        {
            this.Town = town;
            this.Load(save);
        }
        void FindMinBox()
        {
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
            foreach (var vec in this.Walls)
            {
                min = Vector3.Min(min, vec);
                max = Vector3.Max(max, vec);
            }
            this.Box = new BoundingBox(min, max);
        }
        private void DistinguishWalls()
        {
            var map = this.Town.Map;
            Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue), max = new Vector3(float.MinValue, float.MinValue, float.MinValue);

            this.NorthWalls.Clear();
            this.SouthWalls.Clear();
            this.EastWalls.Clear();
            this.WestWalls.Clear();
            this.Roof.Clear();

            foreach (var vec in this.Walls)
            {
                min = Vector3.Min(min, vec);
                max = Vector3.Max(max, vec);

                CategorizeWall(map, vec);

                //var xx = vec + Vector3.UnitX;
                //var yy = vec + Vector3.UnitY;
                //var heightxx = town.Map.GetHeightmapValue(xx);
                //var heightyy = town.Map.GetHeightmapValue(yy);
                //if (xx.Z > heightxx || yy.Z > heightyy)
                //    this.FrontWalls.Add(vec);
                //var vecheight = town.Map.GetHeightmapValue(vec);
                //if (vec.Z == vecheight)
                //    this.Roof.Add(vec);
            }

            foreach (var vec in this.Enterior)
            {
                if (map.GetBlock(vec) != BlockDefOf.Air)
                    this.EnteriorWalls.Add(vec);
            }

            this.Box = new BoundingBox(min, max);
        }
        
        private void CategorizeWall(IMap map, Vector3 vec)
        {
            var east = vec + Vector3.UnitX;
            var south = vec + Vector3.UnitY;
            var west = vec - Vector3.UnitX;
            var north = vec - Vector3.UnitY;

            var heighteast = map.GetHeightmapValue(east);
            var heightsouth = map.GetHeightmapValue(south);
            var heightwest = map.GetHeightmapValue(west);
            var heightnorth = map.GetHeightmapValue(north);

            if (east.Z > heighteast)
                this.EastWalls.Add(vec);
            if (south.Z > heightsouth)
                this.SouthWalls.Add(vec);
            if (west.Z > heightwest)
                this.WestWalls.Add(vec);
            if (north.Z > heightnorth)
                this.NorthWalls.Add(vec);

            //if (east.Z < heighteast &&
            //    south.Z < heightsouth &&
            //    west.Z < heightwest &&
            //    north.Z < heightnorth)


            // TODO: distinguish floors and enterior non-air blocks 

            var vecheight = map.GetHeightmapValue(vec);
            if (vec.Z == vecheight)
                this.Roof.Add(vec);

            //if (vec.Z == 68)
            //    "asdasd".ToConsole();
            if (this.Town.Map.GetBlock(vec).Opaque)
                if (this.Enterior.Contains(vec - Vector3.UnitZ))
                    this.Ceiling.Add(vec);
            //var above = map.GetBlock(vec + Vector3.UnitZ);
            //if (above == Block.Air)
            //    this.Floor.Add(vec);
            if (this.Enterior.Contains(vec + Vector3.UnitZ))
                this.Floor.Add(vec);
        }

		/// <summary>
		/// TODO: cache positions
		/// </summary>
		/// <returns></returns>
        //public Dictionary<Vector3, Cell> GetVisibleBlocks()
        //{
        //    var list = new Dictionary<Vector3, Cell>();//List<Cell>();
        //    int x = (int)this.Box.Min.X - 1;
        //    int y = (int)this.Box.Min.Y - 1;
        //    int z = (int)this.Box.Min.Z - 1;
        //    int xx = (int)this.Box.Max.X +1;
        //    int yy = (int)this.Box.Max.Y +1;
        //    int zz = (int)this.Box.Max.Z +1;
        //    //int w = xx - x;
        //    //int d = yy - y;
        //    //int h = zz - z;

        //    // add base
        //    int xxx = xx+1;
        //    int yyy = yy+1;
        //    for (int i = x; i < xxx; i++)
        //        for (int j = y; j < yyy; j++)
        //        {
        //            var pos = new Vector3(i, j, this.Box.Min.Z - 1);
        //            var cell = this.Town.Map.GetCell(pos);
        //            //if (cell.IsDrawable())
        //            //this.Town.Map.GetChunk(pos).InvalidateCell(cell);
        //                list.Add(pos, cell);
        //        }

        //    // add enterior blocks and far outer walls
        //    for (int i = x; i < xx; i++)
        //        for (int j = y; j < yy; j++)
        //        {
        //            for (int k = z; k < zz; k++)
        //            {
        //                var pos = new Vector3(i, j, k);
        //                var cell = this.Town.Map.GetCell(pos);
        //                if (cell.IsDrawable())
        //                    //list.Add(pos, cell);
        //                    list[pos] = cell;

        //                //list.Add(cell);
        //            }
        //        }

        //    // add any doors
        //    //for (int i = x; i < xx; i++)
        //    //{
        //    //    var pos = new Vector3(i, this.Box.Max.Y + 1, z + 1);
        //    //    var cell = this.Town.Map.GetCell(pos);
        //    //    if(cell.Block == Block.Door)
        //    //    {
        //    //        foreach(Vector3 g in Components.BlockDoor.GetChildren(this.Town.Map, pos))
        //    //            list.Add(g, this.Town.Map.GetCell(g));
        //    //    }
        //    //}
        //    //for (int i = y; i < yy; i++)
        //    //{
        //    //    var pos = new Vector3(this.Box.Max.X + 1, i, z + 1);
        //    //    var cell = this.Town.Map.GetCell(pos);
        //    //    if (cell.Block == Block.Door)
        //    //    {
        //    //        foreach (Vector3 g in Components.BlockDoor.GetChildren(this.Town.Map, pos))
        //    //            list.Add(g, this.Town.Map.GetCell(g));
        //    //    }
        //    //}
        //    return list;
        //}
        public Dictionary<Vector3, Cell> GetVisibleBlocks(Camera cam)
        {
            var list = new Dictionary<Vector3, Cell>();//List<Cell>();

            //foreach(var vec in this.Floor.Concat(this.SouthWalls).Concat(this.EastWalls).Concat(EnteriorWalls))
            //    list.Add(vec, this.Town.Map.GetCell(vec));
            //return list;

            // TODO: instead of excluding walls each frame, precompute enterior non-air blocks and floors so i draw only them?
            List<Vector3> exclude = new List<Vector3>();
            if (HideWalls)
            {
                List<Vector3> tohide1 = null, tohide2 = null;
                switch ((int)cam.Rotation)
                {
                    case 0:
                        tohide1 = this.SouthWalls;
                        tohide2 = this.EastWalls;
                        break;
                    case 3:
                        tohide1 = this.SouthWalls;
                        tohide2 = this.WestWalls;
                        break;
                    case 2:
                        tohide1 = this.NorthWalls;
                        tohide2 = this.WestWalls;
                        break;
                    case 1:
                        tohide1 = this.NorthWalls;
                        tohide2 = this.EastWalls;
                        break;
                    default:
                        break;
                }
                exclude = exclude.Concat(tohide1).Concat(tohide2).ToList();
            }
      
            if (HideRoof)
                exclude = exclude.Concat(this.Roof).ToList();

            if (HideCeiling)
                exclude = exclude.Concat(this.Ceiling).ToList();

            //foreach (var vec in this.Walls.Except(tohide1).Except(tohide2).Except(this.Roof))
            //    list.Add(vec, this.Town.Map.GetCell(vec));
            foreach (var vec in this.Walls.Except(exclude))
                list.Add(vec, this.Town.Map.GetCell(vec));
            return list;

            /*
            //int x = (int)this.Box.Min.X - 1;
            //int y = (int)this.Box.Min.Y - 1;
            //int z = (int)this.Box.Min.Z - 1;
            //int xx = (int)this.Box.Max.X + 1;
            //int yy = (int)this.Box.Max.Y + 1;
            //int zz = (int)this.Box.Max.Z + 1;
            //int w = xx - x;
            //int d = yy - y;
            //int h = zz - z;

            //// add base
            //int xxx = xx + 1;
            //int yyy = yy + 1;
            //for (int i = x; i < xxx; i++)
            //    for (int j = y; j < yyy; j++)
            //    {
            //        var pos = new Vector3(i, j, this.Box.Min.Z - 1);
            //        var cell = this.Town.Map.GetCell(pos);
            //        list.Add(pos, cell);
            //    }

            //// add enterior blocks and backdrop walls
            //w = (int)(this.Box.Max.X - this.Box.Min.X) + 1;
            //d = (int)(this.Box.Max.Y - this.Box.Min.Y) + 1;
            //h = (int)(this.Box.Max.Z - this.Box.Min.Z) + 1;
            //for (int i = 0; i < w + 2; i++)
            //    for (int j = 0; j < d + 2; j++)
            //        for (int k = 0; k < h + 1; k++)
            //        {
            //            var pos = this.Box.Min - Vector3.One + new Vector3(i, j, k);
            //            var cell = this.Town.Map.GetCell(pos);
            //            if (cell.IsDrawable())
            //                list[pos] = cell;
            //        }

            //// add roof
            ////for (int i = 0; i < w + 2; i++)
            ////    for (int j = 0; j < d + 2; j++)
            ////        {
            ////            var pos = this.Box.Min - Vector3.One + new Vector3(i, j, h + 1);
            ////            var cell = this.Town.Map.GetCell(pos);
            ////            if (cell.IsDrawable())
            ////                list[pos] = cell;
            ////        }

            //// front walls cutaway
            //for (int i = (int)this.Box.Min.X; i < (int)this.Box.Max.X + 2; i++)
            //{
            //    Vector3 pos = Vector3.Zero;
            //    switch ((int)cam.Rotation)
            //    {
            //        case 0:
            //        case 3:
            //            pos = new Vector3(i, this.Box.Max.Y + 1, 0); //this.Box.Min - Vector3.One + 
            //            break;
            //        case 1:
            //        case 2:
            //            //pos = new Vector3(this.Box.Max.X + 1, i, 0);
            //            pos = new Vector3(i, this.Box.Min.Y - 1, 0);
            //            break;
            //        default:
            //            break;
            //    }
            //    for (int k = (int)this.Box.Min.Z; k < (int)this.Box.Max.Z + 1; k++)
            //    {
            //        pos.Z = k;
            //        list.Remove(pos);
            //    }
            //}

            //for (int i = (int)this.Box.Min.Y; i < (int)this.Box.Max.Y + 2; i++)
            //{
            //    Vector3 pos = Vector3.Zero;
            //    switch ((int)cam.Rotation)
            //    {
            //        case 0:
            //        case 1:
            //            pos = new Vector3(this.Box.Max.X + 1, i, 0);
            //            break;
            //        case 2:
            //        case 3:
            //            pos = new Vector3(this.Box.Min.X - 1, i, 0);
            //            break;

            //        default:
            //            break;
            //    }
            //    for (int k = (int)this.Box.Min.Z; k < (int)this.Box.Max.Z + 1; k++)
            //    {
            //        pos.Z = k;
            //        list.Remove(pos);
            //    }
            //}

            // add any doors
            //for (int i = x; i < xx; i++)
            //{
            //    var pos = new Vector3(i, this.Box.Max.Y + 1, z + 1);
            //    var cell = this.Town.Map.GetCell(pos);
            //    if(cell.Block == Block.Door)
            //    {
            //        foreach(Vector3 g in Components.BlockDoor.GetChildren(this.Town.Map, pos))
            //            list.Add(g, this.Town.Map.GetCell(g));
            //    }
            //}
            //for (int i = y; i < yy; i++)
            //{
            //    var pos = new Vector3(this.Box.Max.X + 1, i, z + 1);
            //    var cell = this.Town.Map.GetCell(pos);
            //    if (cell.Block == Block.Door)
            //    {
            //        foreach (Vector3 g in Components.BlockDoor.GetChildren(this.Town.Map, pos))
            //            list.Add(g, this.Town.Map.GetCell(g));
            //    }
            //}
            //return list;
             */
        }

		public bool Contains(Vector3 global)
        {
            return this.Enterior.Contains(global.RoundXY());
            //return this.Box.Contains(global.Round()) != ContainmentType.Disjoint;
        }

        internal void InvalidateBlock(Vector3 global)
        {
            var block = this.Town.Map.GetBlock(global);
            if(block == BlockDefOf.Air)
            {
                this.Walls.Remove(global);
                //this.Enterior.Add(global); 
                this.FloodFill(global);
            }
            else
            {
                this.Walls.Add(global);
                this.Enterior.Remove(global);
            }
        }

        public void Draw(Camera cam, Effect effect)
        {
            if (!this.Valid)
                this.Validate();
            //float x, y;
            //Coords.Iso(cam, this.MapCoords.X * Chunk.Size, this.MapCoords.Y * Chunk.Size, 0, out x, out y);
            //int rotx, roty;
            //Coords.Rotate(cam, this.MapCoords.X, this.MapCoords.Y, out rotx, out roty);
            var world = Matrix.CreateTranslation(Vector3.Zero);//new Vector3(x, y, ((rotx + roty) * Chunk.Size)));
            effect.Parameters["World"].SetValue(world);
            EffectParameter effectMaxDrawZ = effect.Parameters["MaxDrawLevel"];
            EffectParameter effectHideWalls = effect.Parameters["HideWalls"];
            effectMaxDrawZ.SetValue(int.MaxValue);
            effectHideWalls.SetValue(Engine.HideWalls);
            effect.CurrentTechnique.Passes["Pass1"].Apply();
            this.FloorsMesh.Opaque.Draw();
            this.EastWallsMesh.Opaque.Draw();
            this.WestWallsMesh.Opaque.Draw();
            this.SouthWallsMesh.Opaque.Draw();
            this.NorthWallsMesh.Opaque.Draw();
            //this.RoofMesh.Draw();
            //this.EnteriorMesh.Draw();
            //this.CeilingMesh.Draw();
            //this.WallsMesh.Draw();
        }

        void SortWalls()
        {
            SortWalls(this.Walls);
        }
        static void SortWalls(List<Vector3> walls)
        {
            walls.Sort((w1, w2) =>
            {
                if (w1.Z < w2.Z)
                    return -1;
                if (w1.Z > w2.Z)
                    return 1;
                return 0;
            });
        }

        /// <summary>
        /// returs false if it reaches an outside location
        /// </summary>
        /// <param name="global"></param>
        /// <returns></returns>
        public void FloodFill(Vector3 global)
        {
            var map = this.Town.Map;
            var block = map.GetBlock(global);
            if (block != BlockDefOf.Air)
                return;
            Queue<Vector3> tocheck = new Queue<Vector3>();
            HashSet<Vector3> handled = new HashSet<Vector3>() { global };
            tocheck.Enqueue(global);
            while (tocheck.Count > 0)
            {
                var current = tocheck.Dequeue();
                if (!this.Enterior.Contains(current))
                    this.Enterior.Add(current);
                foreach (var n in current.GetNeighbors())//.GetNeighbors())
                {
                    var nblock = map.GetBlock(n);
                    if (nblock == null)
                        continue;
                    if (nblock == BlockDefOf.Air)
                    {
                        var heightmap = map.GetHeightmapValue(n);
                        if (current.Z > heightmap)
                            continue;
                        if (!handled.Contains(n))
                        {
                            handled.Add(n);
                            tocheck.Enqueue(n);
                        }
                    }
                    else
                    {
                        if (!this.Walls.Contains(n))
                        {
                            this.Walls.Add(n);
                            this.CategorizeWall(map, n);
                        }
                        if (!nblock.Opaque)
                        {
                            if (!handled.Contains(n))
                            {
                                handled.Add(n);
                                tocheck.Enqueue(n);
                            }
                        }
                    }
                }
            }
        }
        static public House FloodFill(IMap map, Vector3 global)
        {
            var block = map.GetBlock(global);
            if (block != BlockDefOf.Air)
                return null;
            HashSet<Vector3> enterior = new HashSet<Vector3>(); // add global to enterior straight away?
            HashSet<Vector3> walls = new HashSet<Vector3>(); // add global to enterior straight away?
            Queue<Vector3> tocheck = new Queue<Vector3>();
            HashSet<Vector3> handled = new HashSet<Vector3>() { global };
            tocheck.Enqueue(global);
            while (tocheck.Count > 0)
            {
                var current = tocheck.Dequeue();
               
                //var block = map.GetBlock(current);
                //if (block != Block.Air)
                //    continue;
                enterior.Add(current);

                //foreach (var n in current.GetNeighbors())//.GetNeighbors())
                foreach (var n in current.GetNeighborsDiag())
                {
                    var nblock = map.GetBlock(n);
                    if (nblock == BlockDefOf.Chest)
                        "asdasd".ToConsole();
                    if (nblock == BlockDefOf.Air)
                    {
                        var heightmap = map.GetHeightmapValue(n);
                        if (current.Z > heightmap)
                            continue;
                        if (!handled.Contains(n))
                        {
                            handled.Add(n);
                            tocheck.Enqueue(n);
                        }
                    }
                    else
                    {
                        walls.Add(n);
                        if (!nblock.Opaque)
                        {

                            if (!handled.Contains(n))
                            {
                                handled.Add(n);
                                tocheck.Enqueue(n);
                            }
                        }
                    }
                }

                ////if (block.Opaque) // check if block is air instead of opacity? mark each block specifying wether it can be considered a valid house wall?
                //if (block == Block.Air)
                //{    
                //    //var heightmap = map.GetHeightmapValue(current);
                //    //if (current.Z > heightmap)
                //    //    continue;
                //    enterior.Add(current);
                //}
                //else
                //{
                //    walls.Add(current);
                //    //if (block.Opaque)
                //    //    continue;
                //}
                //foreach (var n in current.GetNeighborsDiag())//.GetNeighbors())
                //    if (!handled.Contains(n))
                //    {
                //        handled.Add(n);
                //        tocheck.Enqueue(n);
                //    }
            }
            if (enterior.Count == 0)
                return null;

            var sortedwalls = walls.ToList();
            SortWalls(sortedwalls);

            var house = new House(map.Town, enterior, sortedwalls);
            return house;
        }

        static public House FloodFillOld(IMap map, Vector3 global)
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
                //if (block.Opaque) // check if block is air instead of opacity? mark each block specifying wether it can be considered a valid house wall?
                if (block != BlockDefOf.Air)
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
            if (enterior.Count == 0)
                return null;

            var sortedwalls = walls.ToList();
            SortWalls(sortedwalls);

            var house = new House(map.GetTown(), enterior, sortedwalls);
            return house;
        }

        public void Write(BinaryWriter w)
        {
            w.Write(this.Name);

            w.Write(this.Enterior.Count);
            foreach (var vec in this.Enterior)
                w.Write(vec);
            
            w.Write(this.Walls.Count);
            foreach (var vec in this.Walls)
                w.Write(vec);

            w.Write(this.NorthWalls.Count);
            foreach (var vec in this.NorthWalls)
                w.Write(vec);

            w.Write(this.SouthWalls.Count);
            foreach (var vec in this.SouthWalls)
                w.Write(vec);

            w.Write(this.EastWalls.Count);
            foreach (var vec in this.EastWalls)
                w.Write(vec);

            w.Write(this.WestWalls.Count);
            foreach (var vec in this.WestWalls)
                w.Write(vec);

            w.Write(this.Roof.Count);
            foreach (var vec in this.Roof)
                w.Write(vec);

            w.Write(this.Ceiling.Count);
            foreach (var vec in this.Ceiling)
                w.Write(vec);

            w.Write(this.Floor);
        }
        public void Read(BinaryReader r)
        {
            this.Name = r.ReadString();

            this.Enterior.Clear();
            this.Walls.Clear();
            this.NorthWalls.Clear();
            this.SouthWalls.Clear();
            this.EastWalls.Clear();
            this.WestWalls.Clear();
            this.Roof.Clear();
            this.Ceiling.Clear();

            var count = 0;
            count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                this.Enterior.Add(r.ReadVector3());
            count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                this.Walls.Add(r.ReadVector3());
            count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                this.NorthWalls.Add(r.ReadVector3());
            count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                this.SouthWalls.Add(r.ReadVector3());
            count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                this.EastWalls.Add(r.ReadVector3());
            count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                this.WestWalls.Add(r.ReadVector3());
            count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                this.Roof.Add(r.ReadVector3());
            count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                this.Ceiling.Add(r.ReadVector3());
            this.Floor = r.ReadListVector3();
        }
        
        public SaveTag Save(string name)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, name);

            var enterior = new SaveTag(SaveTag.Types.List, "Enterior", SaveTag.Types.Vector3);
            foreach (var vec in this.Enterior)
                enterior.Add(new SaveTag(SaveTag.Types.Vector3, "", vec));

            var walls = new SaveTag(SaveTag.Types.List, "Walls", SaveTag.Types.Vector3);
            foreach (var vec in this.Walls)
                walls.Add(new SaveTag(SaveTag.Types.Vector3, "", vec));

            var north = new SaveTag(SaveTag.Types.List, "NorthWalls", SaveTag.Types.Vector3);
            foreach (var vec in this.NorthWalls)
                north.Add(new SaveTag(SaveTag.Types.Vector3, "", vec));

            var south = new SaveTag(SaveTag.Types.List, "SouthWalls", SaveTag.Types.Vector3);
            foreach (var vec in this.SouthWalls)
                south.Add(new SaveTag(SaveTag.Types.Vector3, "", vec));

            var east = new SaveTag(SaveTag.Types.List, "EastWalls", SaveTag.Types.Vector3);
            foreach (var vec in this.EastWalls)
                east.Add(new SaveTag(SaveTag.Types.Vector3, "", vec));

            var west = new SaveTag(SaveTag.Types.List, "WestWalls", SaveTag.Types.Vector3);
            foreach (var vec in this.WestWalls)
                west.Add(new SaveTag(SaveTag.Types.Vector3, "", vec));

            var roof = new SaveTag(SaveTag.Types.List, "Roof", SaveTag.Types.Vector3);
            foreach (var vec in this.Roof)
                roof.Add(new SaveTag(SaveTag.Types.Vector3, "", vec));

            var ceiling = new SaveTag(SaveTag.Types.List, "Ceiling", SaveTag.Types.Vector3);
            foreach (var vec in this.Ceiling)
                ceiling.Add(new SaveTag(SaveTag.Types.Vector3, "", vec));

            tag.Add(enterior);
            tag.Add(walls);
            tag.Add(north);
            tag.Add(south);
            tag.Add(east);
            tag.Add(west);
            tag.Add(roof);
            tag.Add(ceiling);

            tag.Add(new SaveTag(SaveTag.Types.String, "Name", this.Name));

            return tag;
        }
        public void Load(SaveTag tag)
        {
            List<SaveTag> list;// = tag["Enterior"].Value as List<SaveTag>;
            if (tag.TryGetTagValue("Enterior", out list))
                foreach (var t in list)
                    this.Enterior.Add((Vector3)t.Value);

            if (tag.TryGetTagValue("Walls", out list))
                foreach (var t in list)
                    this.Walls.Add((Vector3)t.Value);

            if (tag.TryGetTagValue("NorthWalls", out list))
                foreach (var t in list)
                    this.NorthWalls.Add((Vector3)t.Value);
            if (tag.TryGetTagValue("SouthWalls", out list))
                foreach (var t in list)
                    this.SouthWalls.Add((Vector3)t.Value);
            if (tag.TryGetTagValue("EastWalls", out list))
                foreach (var t in list)
                    this.EastWalls.Add((Vector3)t.Value);
            if (tag.TryGetTagValue("WestWalls", out list))
                foreach (var t in list)
                    this.WestWalls.Add((Vector3)t.Value);

            if (tag.TryGetTagValue("Roof", out list))
                foreach (var t in list)
                    this.Roof.Add((Vector3)t.Value);

            if (tag.TryGetTagValue("Ceiling", out list))
                foreach (var t in list)
                    this.Ceiling.Add((Vector3)t.Value);


            tag.TryGetTagValue<string>("Name", v => this.Name = v);
        }
    }
}

namespace Start_a_Town_
{
    interface IEnterior
    {
        Dictionary<Vector3, Cell> GetVisibleBlocks(Camera cam);
        bool Contains(Vector3 global);
    }
}
