using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.Serialization;
using System.Diagnostics;
using Start_a_Town_.Components;
using Start_a_Town_.Terraforming;
using Start_a_Town_.Blocks;
using Start_a_Town_.GameModes;

namespace Start_a_Town_
{
    [Flags]
    public enum Edges { None = 0x0, West = 0x1, North = 0x2, East = 0x4, South = 0x8, All = 0xF }//, Top = 0x10, Bottom = 0x20 }
    [Flags]
    public enum VerticalEdges { None = 0x0, Top = 0x1, Bottom = 0x2, All = 0x3 }
    public class Chunk : Component// : IComparable<Chunk>
    {
        public override string ComponentName
        {
            get { return "Chunk"; }
        }

        public override object Clone()
        {        
            Chunk chunk;
            using(BinaryWriter w = new BinaryWriter(new MemoryStream()))
            {
                this.Write(w);
                w.BaseStream.Position = 0;
                using (BinaryReader r = new BinaryReader(w.BaseStream))
                    chunk = Chunk.Read(r);
            }
            chunk.Map = this.Map;
            return chunk;
        }

        #region Private Fields

        //List<BitVector32> LightArray;
        #endregion

        #region Initialization
        static BitVector32.Section _Skylight, _NextSkylight, _Blocklight, _NextBlocklight; //
        static public void Initialize()
        {
            _Skylight = BitVector32.CreateSection(15);
            _NextSkylight = BitVector32.CreateSection(15, _Skylight);
            _Blocklight = BitVector32.CreateSection(15, _NextSkylight);
            _NextBlocklight = BitVector32.CreateSection(15, _Blocklight);
        }
        public Chunk InitCells(Action<Cell> init)
        {
            int n = 0;
            for (int z = 0; z < Start_a_Town_.Map.MaxHeight; z++)
                for (int i = 0; i < Size; i++)
                    for (int j = 0; j < Size; j++)
                        //for (int z = 0; z < Map.MaxHeight; z++)
                    {
                        Cell cell = new Cell(i, j, z);
                        init(cell);
                        //CellGrid2.Add(cell);
                        CellGrid2[n++] = cell;
                    }
            return this;
        }
        public Dictionary<Vector3, double> InitCells(List<Terraformer> mutators)
        {
            var gradientCache = new Dictionary<Vector3, double>();
            var world = Map.GetWorld();
            int n = 0; ;
            //Gradient grad = new Gradient(this.World);
            var grad = new GradientLowRes(this.World, this);
            mutators.ForEach(m => m.SetWorld(this.World));
            var watch = Stopwatch.StartNew();

            for (int z = 0; z < Start_a_Town_.Map.MaxHeight; z++)
                for (int i = 0; i < Size; i++)
                    for (int j = 0; j < Size; j++)
                        //for (int z = 0; z < Map.MaxHeight; z++)
                    {
                        Cell cell = new Cell(i, j, z);
                        //double gradient = grad.GetGradient((int)Start.X + i, (int)Start.Y + j, z);
                        double gradient = grad.GetGradient(i, j, z);
                        gradientCache.Add(new Vector3(i, j, z), gradient);
                        foreach(var m in mutators)
                            m.Initialize(world, cell, (int)Start.X + i, (int)Start.Y + j, z, gradient);
                        this.CellGrid2[n++] = cell;
                    }
            //grad.timessampled.ToConsole();
            ("generated in " + watch.Elapsed.ToString()).ToConsole();
            watch.Stop();
            //global::Start_a_Town_.Terraforming.Mutators.Caves.Watch.Elapsed.ToConsole();
            //global::Start_a_Town_.Terraforming.Mutators.Caves.Watch.Reset();
            return gradientCache;
        }
        public Chunk InitCells()
        {
            int n = 0;
            for (int z = 0; z < Start_a_Town_.Map.MaxHeight; z++)
                for (int i = 0; i < Size; i++)
                    for (int j = 0; j < Size; j++)
                        //for (int z = 0; z < Map.MaxHeight; z++)
                    {
                        Cell cell = new Cell(i, j, z);
                        //CellGrid2.Add(cell);
                        this.CellGrid2[n++] = cell;
                    }
            return this;
        }
        //public Chunk FinalizeCells()
        public Chunk FinalizeCells(Net.RandomThreaded random)
        {
            for (int z = 0; z < Start_a_Town_.Map.MaxHeight; z++)
                for (int i = 0; i < Size; i++)
                    for (int j = 0; j < Size; j++)
                    {
                        Cell cell = this[i, j, z];
                        //Map.World.Mutators.ForEach(m => m.Finalize(random, Map.World, cell, (int)Start.X + i, (int)Start.Y + j, z));
                        foreach (var m in Map.GetWorld().GetMutators())
                            m.Finalize(random, Map.GetWorld(), cell, (int)Start.X + i, (int)Start.Y + j, z);
                    }
            return this;
        }
        #endregion

        public override string ToString()
        {
            string text =
                "Local: " + MapCoords.ToString() +
                  "\nGlobal: " + Start.ToString() +
                   "\nObjects: " + Objects.Count +
                "\nVisibleOutdoorCells: " + VisibleOutdoorCells.Count +
                "\nCells to validate: " + this.CellsToValidate.Count;
                //   "\nCells to light: " + CellsToLight.Count +
                //"\nCells to activate: " + CellsToActivate.Count
                
            //   "\nCells to update: " + CellsToUpdate.Count +


            text += "\nProperties:\n";
            foreach (KeyValuePair<string, object> prop in Properties)
                text += "   " + prop.Key + ": " + prop.Value.ToString() + "\n"; //"\t" + 

            text += "Objects: " + Objects.Count.ToString() + "\n";
            //foreach (GameObject obj in Objects)
            //    text += "   " + obj.Name + "\n";
            return text.Remove(text.Length - 1);
        }

        #region Public Fields
        //public int Width, Height;
        //public List<Cell> CellGrid2;// { get; private set; }
        public Cell[] CellGrid2;// { get; private set; }

        public void CopyFrom(Chunk chunk)
        {
            this.Objects = chunk.Objects;
            this.CellGrid2 = chunk.CellGrid2;
            this.HeightMap = chunk.HeightMap;
            this.Sunlight = chunk.Sunlight;
            this.BlockLight = chunk.BlockLight;
            this.VisibleOutdoorCells = chunk.VisibleOutdoorCells;
        }

        //    public SortedList<int, List<GameObject>> StaticObjects;
        public List<GameObject> Objects;
        Dictionary<int, GameObject> BlockObjects;
        public Dictionary<Vector3, BlockEntity> BlockEntities = new Dictionary<Vector3, BlockEntity>();
        public bool IsQueuedForLight;
        public static int Size = 16;//32;//16; //
        //public List<IEntity> Entities;
        public SortedList<int, Cell> VisibleIndoorCells;

        public SortedDictionary<int, Cell> VisibleOutdoorCells = new SortedDictionary<int, Cell>();

        public Vector2 Start, bottomRight;
        bool _Saved = false;
        public bool Saved
        {
            get { return _Saved; }
            //set
            //{
            //    _Saved = value;
            //}
        }
        public void Invalidate()
        {
            this._Saved = false;
            //("chunk: " + this.MapCoords.ToString() + " invalidated, rebuilding mesh").ToConsole();
                
        }
        public void Rebuild()
        {
            this.Valid = false;
        }

        public int X, Y;
        public int RectHeight;
        public IMap Map;
        public IWorld World { get { return this.Map.GetWorld(); } }
        public bool Valid;
        //public bool TopSliceChanged = true;
        //{
        //    get { return (bool)this["Valid"]; }
        //    set { this["Valid"] = (bool)value; }
        //}
        //public bool Invalid;

        //HashSet<Cell> CellsToValidate = new HashSet<Cell>();
        //ConcurrentDictionary<Cell, bool> CellsToValidate = new ConcurrentDictionary<Cell, bool>();

        //BlockingCollection<Cell> CellsToValidate = new BlockingCollection<Cell>(new ConcurrentQueue<Cell>());
        //Queue<Cell> CellsToValidate = new Queue<Cell>();
        ConcurrentQueue<Cell> CellsToValidate = new ConcurrentQueue<Cell>();
        ConcurrentQueue<Cell> CellsToActivate = new ConcurrentQueue<Cell>();

        public Dictionary<Cell, float> CellUpdateTimers = new Dictionary<Cell, float>();
        #endregion

        static public readonly int UnloadTimerMax = 3 * Engine.TargetFps;
        public int UnloadTimer = UnloadTimerMax ;//- 1;
        public bool SkylightUpdated = true;//false;
        public bool ChunkBoundariesUpdated = true;
        public bool LightValid = false;
        public bool EdgesValid = false;
        public void InvalidateEdges()
        {
            this.EdgesValid = false;
            //this.Saved = false;
        }

        #region Public Properties
        public Cell this[int localx, int localy, int localz]
        {
            get
            {
                if (localx < 0 || localx > Chunk.Size - 1 || localy < 0 || localy > Chunk.Size - 1 || localz < 0 || localz > Map.GetMaxHeight() - 1)
                    return null; 

                int ind = FindIndex(localx, localy, localz);
                return CellGrid2[ind];
            }
        }
        public Cell this[float localx, float localy, float localz]
        {
            get
            {
                //GetCellAt 
                if (localx < 0 || localx > Chunk.Size - 1 || localy < 0 || localy > Chunk.Size - 1 || localz < 0 || localz > Map.GetMaxHeight() - 1)
                    //throw new IndexOutOfRangeException();
                    return null; // Map.GetCellAt(Start.X + localx, Start.Y + localy, localz);

                //int ind = FindIndex((int)(Start.X + localx), (int)(Start.Y + localy), (int)localz);
                int ind = FindIndex(localx, localy, localz);
                return CellGrid2[ind];
                //return new Position(GetLocalCell((int)localx, (int)localy, (int)localz), this);
            }
        }
        public Cell this[Vector3 localCoords]
        {
            get
            {
                //return CellGrid[(int)pos.X % Size, (int)pos.Y % Size, (int)pos.Z];
                if (!this.IsCellInChunk(localCoords))
                    return null;
                return CellGrid2[FindIndex(localCoords)];
                //return this[localCoords.X, localCoords.Y, localCoords.Z];
            }
        }
        public Cell this[int cellIndex]
        {
            get { return CellGrid2[cellIndex]; }
        }
        public Cell GetCellFromGlobal(int globalX, int globalY, int globalZ)
        { return CellGrid2[FindIndex(globalX - Start.X, globalY - Start.Y, globalZ)]; }
        public Cell GetCellFromGlobal(Vector3 global)
        {
            //return CellGrid2[FindIndex(global)]; 
            return this[(int)(global.X - Start.X), (int)(global.Y - Start.Y), (int)global.Z];
        }
        //public float GetDepthFar(Camera camera)
        //{

        //    //return (int)(Start.X * camera.RotSin + Start.Y * camera.RotCos);
        //    //return (int)(Start.X * 255 * 255 + Start.Y * 255);

        //    return Position.GetDepth(new Vector3(Start.X, Start.Y, 0));
        //    //return (int)(Start.X + Start.Y);
        //    //return (int)((Start.X + Start.Y) * Map.MaxHeight);
        //}
        //public float GetDepthNear(Camera camera)
        //{
        //    //return (int)((Start.X + Size) * camera.RotSin + (Start.Y + Size) * camera.RotCos) + 255;
        //    //return (int)((Start.X + Size) * 255 * 255 + (Start.Y + Size) * 255 + 255);

        //    return Position.GetDepth(new Vector3(Start.X + Chunk.Size - 1, Start.Y + Chunk.Size - 1, Map.MaxHeight - 1));
        //    //return (int)(Start.X + Start.Y + Chunk.Size + Chunk.Size - 2 + Map.MaxHeight - 1);
        //    //return (int)((Start.X + Start.Y + Size + Size) * Map.MaxHeight + Map.MaxHeight - 1);
        //}

        public bool Visible(Camera camera)
        {
            return GetBounds(camera).Intersects(camera.ViewPort);
        }

        public Vector2 MapCoords
        {
            get { return new Vector2(X, Y); }
            set
            {
                X = (int)value.X;
                Y = (int)value.Y;
                Start = this.MapCoords * Chunk.Size;
            }
        }
        public int GetTop(Camera camera)
        {
            //get { return (int)(GetCellAt(0, 0, Map.MaxHeight - 1).Bounds.Top); }
            return (int)GetScreenLocation(camera).Y;
            //get { return (int)(Coords.iso(new Vector3(Start.X, Start.Y, Map.MaxHeight - 1)).Y * Camera.Zoom); }
        }
        public int GetBottom(Camera camera)
        {
            return GetBounds(camera).Bottom;
            // get { return (int)Coords.iso(new Vector3(Start.X + Chunk.Size - 1, Start.Y + Chunk.Size - 1, 0)).Y; }
        }
        public int GetLeft(Camera camera)
        {
            return (int)GetScreenLocation(camera).X;
            //get { return this[0, Size - 1, 0].GetBounds(this).Left; }

        }
        public int GetRight(Camera camera)
        {
            return GetBounds(camera).Right;
            //get { return (int)Coords.iso(new Vector3(Start.X + Chunk.Size - 1, Start.Y, 0)).X; }
        }

        public Vector2 GetScreenLocation(Camera camera)
        {
            //return camera.Transform((int)Start.X, (int)Start.Y, Map.MaxHeight - 1);
            int xx, yy;
            Coords.Iso(camera, (int)Start.X, (int)Start.Y, Start_a_Town_.Map.MaxHeight - 1, out xx, out yy);
            return new Vector2((int)(xx * camera.Zoom), (int)(yy * camera.Zoom));
            //return camera.Transform(new Vector3((int)Start.X, (int)Start.Y, Map.MaxHeight - 1));
        }
        static public int Width = Block.Width * Size;
        static public int Height = Start_a_Town_.Map.MaxHeight * Block.BlockHeight + Chunk.Size * Block.Depth;

        public Rectangle GetBounds(Camera camera)
        {
            //Vector2 screenLoc = GetScreenLocation(camera);
            //int width = (int)(Width * camera.Zoom);
            ////int height = (int)((Map.MaxHeight * TileBase.Height) * camera.Zoom);
            //int height = (int)(Height * camera.Zoom);
            //return new Rectangle((int)screenLoc.X - width / 2, (int)screenLoc.Y, width, height);
            return camera.GetScreenBounds((int)Start.X, (int)Start.Y, this.Map.GetMaxHeight() - 1, new Rectangle(-Width / 2, 0, Width, Height));
        }
        public Rectangle GetBounds()
        {
            //return new Rectangle(-Width / 2, -Map.MaxHeight * TileBase.BlockHeight, Width, Height);
            return new Rectangle(-Width / 2, -Height / 2, Width, Height);
        }
        public Rectangle GetScreenBounds(Camera cam)
        {
            Rectangle chunkBounds = cam.GetScreenBounds(this.Start.X + Chunk.Size / 2, this.Start.Y + Chunk.Size / 2, global::Start_a_Town_.Map.MaxHeight / 2, this.GetBounds());  //chunk.Value.GetBounds(camera);
            return chunkBounds;
        }

        //public Rectangle GetScreenBounds(Camera camera)
        //{
        //    Rectangle chunkBounds = camera.GetScreenBounds(this.Start.X + Chunk.Size / 2, this.Start.Y + Chunk.Size / 2, Start_a_Town_.Map.MaxHeight / 2, this.GetBounds());  //chunk.Value.GetBounds(camera);
        //    return chunkBounds;
        //}
        public List<Chunk> Neighbors
        {
            get
            {
                List<Chunk> n = new List<Chunk>();
                if (North != null)
                    n.Add(North);
                if (South != null)
                    n.Add(South);
                if (West != null)
                    n.Add(West);
                if (East != null)
                    n.Add(East);
                return n;
            }
        }
        public List<Chunk> NeighborsDiag
        {
            get
            {
                List<Chunk> n = new List<Chunk>();
                if (North != null)
                    n.Add(North);
                if (South != null)
                    n.Add(South);
                if (West != null)
                    n.Add(West);
                if (East != null)
                    n.Add(East);
                if (NorthEast != null)
                    n.Add(NorthEast);
                if (SouthEast != null)
                    n.Add(SouthEast);
                if (NorthWest != null)
                    n.Add(NorthWest);
                if (SouthWest != null)
                    n.Add(SouthWest);

                //n.Add(NorthWest);
                //n.Add(North);
                //n.Add(NorthEast);
                //n.Add(West);
                //n.Add(East);
                //n.Add(SouthWest);
                //n.Add(South);
                //n.Add(SouthEast);
                return n;
            }
        }

        #region Neighbors
        public Chunk North
        {
            get
            {
                Vector2 coords = MapCoords + new Vector2(0, -1);
                if (Map.GetActiveChunks().ContainsKey(coords))
                    return Map.GetActiveChunks()[coords];
                return null;
            }
        }
        public Chunk South
        {
            get
            {
                Vector2 coords = MapCoords + new Vector2(0, 1);
                if (Map.GetActiveChunks().ContainsKey(coords))
                    return Map.GetActiveChunks()[coords];
                return null;
            }
        }
        public Chunk West
        {
            get
            {
                Vector2 coords = MapCoords + new Vector2(-1, 0);
                if (Map.GetActiveChunks().ContainsKey(coords))
                    return Map.GetActiveChunks()[coords];
                return null;
            }
        }
        public Chunk East
        {
            get
            {
                Vector2 coords = MapCoords + new Vector2(1, 0);
                if (Map.GetActiveChunks().ContainsKey(coords))
                    return Map.GetActiveChunks()[coords];
                return null;
            }
        }

        public Chunk NorthWest
        {
            get
            {
                Vector2 coords = MapCoords + new Vector2(-1, -1);
                if (Map.GetActiveChunks().ContainsKey(coords))
                    return Map.GetActiveChunks()[coords];
                return null;
            }
        }
        public Chunk NorthEast
        {
            get
            {
                Vector2 coords = MapCoords + new Vector2(1, -1);
                if (Map.GetActiveChunks().ContainsKey(coords))
                    return Map.GetActiveChunks()[coords];
                return null;
            }
        }
        public Chunk SouthWest
        {
            get
            {
                Vector2 coords = MapCoords + new Vector2(-1, 1);
                if (Map.GetActiveChunks().ContainsKey(coords))
                    return Map.GetActiveChunks()[coords];
                return null;
            }
        }
        public Chunk SouthEast
        {
            get
            {
                Vector2 coords = MapCoords + new Vector2(1, 1);
                if (Map.GetActiveChunks().ContainsKey(coords))
                    return Map.GetActiveChunks()[coords];
                return null;
            }
        }
        #endregion

        #endregion

        public bool TryGetBlockObject(Vector3 local, out GameObject blockObj)
        {
            return BlockObjects.TryGetValue(FindIndex(local), out blockObj);
        }
        public Chunk(IMap map, Vector2 pos)
            : this()
        {
            this.Map = map;
            this.MapCoords = pos;
            this.InitCells();
        }
        Chunk(Vector2 pos)
            : this()
        {
            MapCoords = pos;
            //Start = pos * Chunk.Size;
        }
        Chunk()
        {
            //MapCoords = pos;
            //Start = pos * Chunk.Size;

            //CellGrid2 = new List<Cell>(Size * Size * Map.MaxHeight);
            CellGrid2 = new Cell[Chunk.Size * Chunk.Size * Start_a_Town_.Map.MaxHeight];

           // VisibleOutdoorCells = new SortedDictionary<int, Cell>();

            VisibleIndoorCells = new SortedList<int, Cell>();

            //CellsToActivate = new ConcurrentStack<Cell>();
            //CellsToDeactivate = new ConcurrentStack<Cell>();

            Objects = new List<GameObject>();
            BlockObjects = new Dictionary<int, GameObject>();
            this.Sunlight = new List<byte>(Capacity);
            for (int i = 0; i < Capacity; i++)
                this.Sunlight.Add(15);
            this.HeightMap = new int[Size][];
            for (int i = 0; i < Size; i++)
                this.HeightMap[i] = new int[Size];
                //for(int j = 0; j<Size; j++)

            this["Valid"] = false;

            //Sunlight = new byte[Size][][];
            //for (int i = 0; i < Size; i++)
            //{
            //    Sunlight[i] = new byte[Size][];
            //    for (int j = 0; j < Size; j++)
            //        Sunlight[i][j] = new byte[Map.MaxHeight];
            //}
            ResetSunlight();
            ResetCellLight();
            //Sunlight = new List<byte>() { 0 };
        }
        //Vector2 Location;


        static public Chunk Create(IMap map, int x, int y)//, byte[] seedArray)
        {
            Chunk chunk = new Chunk(new Vector2(x, y));
            chunk.Map = map;

            //chunk.InitCells();



            //if (map.World.Caves)
            //    chunk.GenerateCaves();
            return chunk;
        }



        #region Adding and removing objects
        static public bool AddBlockObject(GameObject obj)
        {
            return AddBlockObject(obj.Map, obj, obj.Global);
        }
        static public bool AddBlockObject(IMap map, GameObject obj, Vector3 global)
        {
            Vector3 loc;
            Chunk chunk; Cell cell;
            //if (!Position.TryGetAll(map, global, out cell, out chunk, out loc))
            if (!map.TryGetAll(global, out chunk, out cell, out loc))
                return false;
            chunk.BlockObjects[FindIndex(loc)] = obj;
            chunk.Invalidate();//.Saved = false;
            return true;
        }
        static public bool AddBlockObject(Net.IObjectProvider net, GameObject obj)
        {
            Vector3 loc;
            Chunk chunk; Cell cell;
            //if (!Position.TryGetAll(net.Map, obj.Global, out cell, out chunk, out loc))
            if (!net.Map.TryGetAll(obj.Global, out chunk, out cell, out loc))

            if (!net.Map.TryGetAll(obj.Global, out chunk, out cell, out loc))
                return false;
            chunk.BlockObjects[FindIndex(loc)] = obj;
            chunk.Invalidate();//.Saved = false;
            return true;
        }
        static public bool RemoveBlockObject(IMap map, Vector3 global)
        {
            Vector3 loc;
            Chunk chunk; Cell cell;
            //if (!Position.TryGetAll(map, global, out cell, out chunk, out loc))
            if (!map.TryGetAll(global, out chunk, out cell, out loc))
                return false;
            return chunk.BlockObjects.Remove(FindIndex(loc));
        }
        //static public bool RemoveBlockObject(GameObject blockObj)
        //{
        //    Vector3 loc;
        //    Chunk chunk; Cell cell;
        //    if (!Position.TryGetAll(blockObj.Map, blockObj.Global, out cell, out chunk, out loc))
        //        return false;
        //    return chunk.BlockObjects.Remove(FindIndex(loc));
        //}

        static public bool AddObject(GameObject obj, IMap map, Chunk chunk, Vector3 local)
        {
            Vector3 global = local + new Vector3(chunk.MapCoords * Size, 0);

            obj.Global = global;
            chunk.Objects.Add(obj);

            chunk._Saved = false;
            return true;
        }
        static public bool AddObject(GameObject obj, IMap map)
        {
            return AddObject(obj, map, obj.Global);
        }
        static public bool AddObject(GameObject obj, IMap map, Vector3 global, Vector3 speed)
        {
            if (!AddObject(obj, map, global))
                return false;
            obj.Velocity = speed;
            return true;
        }
        static public bool AddObject(GameObject obj, IMap map, Vector3 global)
        {
            Chunk chunk;
            //if (Position.TryGetChunk(map, global.Round(), out chunk))
            if (map.TryGetChunk(global.RoundXY(), out chunk))
            {
                chunk.Objects.Add(obj);
                chunk.Invalidate();//.Saved = false;

                return true;
            }
            return false;
        }


        static public bool RemoveObject(GameObject obj, Chunk chunk)
        {
            //chunk.Saved = !chunk.Objects.Remove(obj);
            if (chunk.Objects.Remove(obj))
                chunk.Invalidate(); //maybe invalidate in Remove()?
            else
                // add a temporary failsafe here in case entity is being removed from wrong chunk? scan chunks for correct one and remove correctly?
                throw new Exception();
                //"wetf".ToConsole();
            return true;
        }
        //static bool RemoveObject(GameObject obj, Vector3 global)
        //{
        //    Chunk chunk;
        //    if (Position.TryGetChunk(obj.Map, global, out chunk))
        //    {
        //        return RemoveObject(obj, chunk);// chunk.Objects.Remove(obj);//, pos.Local);
        //    }
        //    return false;
        //}
        static public bool RemoveObject(GameObject obj)//Position pos)
        {
            //return RemoveObject(obj, obj.Global.GetChunk(obj.Map));
            return RemoveObject(obj, obj.Map.GetChunk(obj.Global));

        }
        static public bool RemoveObject(IMap map, GameObject obj)//Position pos)
        {
            //return RemoveObject(obj, obj.Global.GetChunk(map));
            return RemoveObject(obj, map.GetChunk(obj.Global));

        }
        #endregion

        #region Dunno
        public Cell GetLocalCell(int x, int y, int z)
        {
            Cell cell = CellGrid2[z * (Chunk.Size * Chunk.Size) + x * Chunk.Size + y];
            //Cell cell = CellGrid2[x * (Chunk.Size * Map.MaxHeight) + y * Map.MaxHeight + z];
            return cell;
        }
        static public int FindIndex(int x, int y, int z)
        { return (z * Chunk.Size + x) * Chunk.Size + y; }
        static public int FindIndex(float x, float y, float z)
        { return (int)((Math.Round(z) * Chunk.Size + Math.Round(x)) * Chunk.Size + Math.Round(y)); }
        static public int FindIndex(Vector3 local)
        { return (int)((Math.Round(local.Z) * Chunk.Size + Math.Round(local.X)) * Chunk.Size + Math.Round(local.Y)); }
        //static public int FindDrawIndex(Vector3 loc)
        //{ return (int)((Math.Round(loc.Y) * Chunk.Size + Math.Round(loc.X)) * Map.MaxHeight + Math.Round(loc.Z)); }
        static public int FindDrawIndex(Vector3 loc, Camera camera)
        {
            return FindDrawIndex(loc, -(int)camera.Rotation);
        }
        static public int FindDrawIndex(Vector3 loc, int rotation)
        {
            Vector3 rotated;
            Coords.Rotate(rotation, loc, out rotated);
            return (int)((Math.Round(rotated.Y) * Chunk.Size + Math.Round(rotated.X)) * Start_a_Town_.Map.MaxHeight + Math.Round(rotated.Z)); 
        }


        public static int Capacity = Size * Size * Start_a_Town_.Map.MaxHeight;
        public List<byte> Sunlight;// = new List<byte>(Capacity);
        //public ConcurrentBag<byte> Sunlight;// = new List<byte>(Capacity);
        public List<byte> BlockLight;// = new List<byte>(Capacity);
        void ResetSunlight()
        {
            this.LightCache = new ConcurrentDictionary<Vector3, Color>();
            // dont reset visible cells because now i now draw every block neighbored by air independent of light
            //this.VisibleOutdoorCells = new SortedDictionary<int, Cell>();
        }

        void ResetCellLight()
        {
            BlockLight = new List<byte>(Size * Size * Start_a_Town_.Map.MaxHeight);
            BlockLight.AddRange(new byte[BlockLight.Capacity]);
        }

        public int[][] HeightMap;// = new int[Size, Size];

        public int GetHeightMapValue(Vector3 local)
        {
            //return HeightMap[(int)local.X][(int)local.Y];
            return this.GetHeightMapValue((int)local.X, (int)local.Y);
        }
        public int GetHeightMapValue(int localx, int localy)
        {
            return HeightMap[localx][localy];
        }
        public bool IsAboveHeightMap(Vector3 local)
        {
            return local.Z > HeightMap[(int)local.X][(int)local.Y];
        }
        public bool IsAboveHeightMap(int localx, int localy, int localz)
        {
            return localz > HeightMap[localx][localy];
        }

        //HashSet<Vector3> LightChanges = new HashSet<Vector3>();
        Queue<Vector3> LightChanges = new Queue<Vector3>();


        /// <summary>
        /// Recalculates the skylight of a chunk and returns a list of cells whose skylight that changed.
        /// </summary>
        /// <returns>A list of cells whose skylight that changed</returns>
        public Queue<Vector3> ResetHeightMap()
        {
            this.ResetSunlight();
            Queue<Vector3> lightsourcesToHandle = new Queue<Vector3>();
            for (int j = 0; j < Size; j++)
                for (int i = 0; i < Size; i++)
                    foreach (var pos in ResetHeightMapColumn(i, j))
                        //LightChanges.Add(pos);
                        this.LightChanges.Enqueue(pos);
            Queue<Vector3> toReturn = new Queue<Vector3>(this.LightChanges);
            this.LightChanges = new Queue<Vector3>();// new HashSet<Vector3>();
            return toReturn;
        }

        public void UpdateHeightMap()
        {
            //this.ResetSunlight();
            this.LightCache = new ConcurrentDictionary<Vector3, Color>();
            for (int j = 0; j < Size; j++)
                for (int i = 0; i < Size; i++)
                    UpdateHeightMapColumn(i, j, false);
         
        }
        public void UpdateHeightMapColumnWithoutLight(int localx, int localy)
        {
            int z;
            byte light;
            Cell cell;
            light = 15;
            z = Start_a_Town_.Map.MaxHeight - 1;
            int firstContact = z;
            bool hit = false;
            while (z >= 0)
            {
                cell = GetLocalCell(localx, localy, z);
                if (!hit)
                    if (cell.Block.Type != Block.Types.Air)
                    {
                        hit = true;
                        firstContact = z;
                    }
                if (cell.Opaque)
                {
                    if (light > 0)
                    {
                        HeightMap[localx][localy] = z;
                        light = 0;
                    }
                }

                this.InvalidateCell(cell);
                z--;
            }

            if (light > 0)
                HeightMap[localx][localy] = z;
        }
        public void UpdateHeightMapColumn(int localx, int localy, bool invalidate = true)
        {
            Queue<Vector3> lightsourcesToHandle = new Queue<Vector3>();
            int z;
            byte light;
            Cell cell;
            light = 15;
            z = Start_a_Town_.Map.MaxHeight - 1;
            int firstContact = z;
            bool hit = false;
            while (z >= 0)
            {
                cell = GetLocalCell(localx, localy, z);
                if (!hit)
                    if (cell.Block.Type != Block.Types.Air)
                    {
                        hit = true;
                        firstContact = z;
                    }
                if (cell.Opaque)
                {
                    if (light > 0)
                    {
                        HeightMap[localx][localy] = z;
                        light = 0;
                    }
                }

                byte oldLight;
                oldLight = GetSunlight(new Vector3(localx, localy, z));
                SetSunlight(localx, localy, z, light);

                //if (z <= firstContact)
                //    lightsourcesToHandle.Enqueue(cell.LocalCoords.ToGlobal(this));
                //if (z == firstContact && hit)
                //    this.VisibleOutdoorCells[FindIndex(cell.LocalCoords)] = cell;
                if(invalidate)
                this.InvalidateCell(cell);
                z--;
            }

            if (light > 0)
                HeightMap[localx][localy] = z;

        }

        /// <summary>
        /// Resets the list of visible blocks to draw.
        /// </summary>
        public void ResetVisibleCells()
        {
            this.ResetVisibleCells((c) => { });
            //foreach(var cell in this.CellGrid2)
            //{
            //    if(cell.Block.Type == Block.Types.Air)
            //        continue;
            //    foreach (var n in cell.LocalCoords.GetNeighbors())
            //    {
            //        //if (n.X < 0 || n.X > 15 ||
            //        //    n.Y < 0 || n.Y > 15 ||
            //        //    n.Z < 0 || n.Z > Map.MaxHeight - 1)
            //        //    continue;
            //        //var nindex = FindIndex(n);
            //        //var ncell = this.CellGrid2[nindex];

            //        Cell ncell;

            //        // if neighbor doesnt exist, continue to next one
            //        if (!this.TryGetCell(n, out ncell))
            //            continue;

            //        // if at least one neighbor is air, add it to drawing list/dictionary
            //        if (ncell.Block.Type == Block.Types.Air)
            //        {
            //            this.VisibleOutdoorCells[FindIndex(cell.LocalCoords)] = cell;
            //            this.UpdateBlockFaces(cell.LocalCoords, Edges.All, VerticalEdges.All);
            //            break;
            //        }
            //    }
            //}
        }
        public void ResetVisibleCells(Action<Cell> callback)
        {
            this.VisibleOutdoorCells.Clear();
            foreach (var cell in this.CellGrid2)
                this.UpdateBlockVisibility(cell);
            return;
            foreach (var cell in this.CellGrid2)
            {
                if (cell.Block.Type == Block.Types.Air)
                    continue;
                foreach (var n in cell.LocalCoords.GetNeighbors())
                {
                    //if (n.X < 0 || n.X > 15 ||
                    //    n.Y < 0 || n.Y > 15 ||
                    //    n.Z < 0 || n.Z > Map.MaxHeight - 1)
                    //    continue;
                    //var nindex = FindIndex(n);
                    //var ncell = this.CellGrid2[nindex];

                    Cell ncell;

                    // if neighbor doesnt exist, continue to next one
                    if (!this.TryGetCell(n, out ncell))
                        continue;

                    //// if at least one neighbor is air, add it to drawing list/dictionary
                    //if (ncell.Block.Type == Block.Types.Air)

                    // if at least one neighbor is non-opaque, add it to drawing list/dictionary
                    if (!ncell.Opaque)
                    {
                        this.VisibleOutdoorCells[FindIndex(cell.LocalCoords)] = cell;
                        this.UpdateBlockFaces(cell.LocalCoords, Edges.All, VerticalEdges.All);
                        //if(cell.IsDrawable())
                        //    this.VisibleOutdoorCells[FindIndex(cell.LocalCoords)] = cell;
                        //else
                        //    this.VisibleOutdoorCells.Remove(FindIndex(cell.LocalCoords));

                        callback(cell);
                        break;
                    }
                }
            }
        }
        internal void ResetVisibleOuterBlocks()
        {
            for (int i = 0; i < Chunk.Size; i++)
                for (int z = 0; z < this.Map.GetMaxHeight(); z++)
                {
                    //Vector3[] cellCoords = new Vector3[]{
                    //    new Vector3(Chunk.Size - 1, i, z), 
                    //    new Vector3(i, 0, z), 
                    //    new Vector3(0, i, z),
                    //    new Vector3(i, Chunk.Size - 1, z)};

                    //foreach (var pos in cellCoords)
                    //{
                    //    var cell = this[pos];
                    //    if (cell.Block.Type == Block.Types.Air)
                    //        continue;
                    //    foreach (var n in cell.LocalCoords.GetNeighbors())
                    //    {
                    //        Cell ncell;

                    //        // if neighbor doesnt exist, continue to next one
                    //        if (!this.TryGetCell(n, out ncell))
                    //            continue;

                    //        // if at least one neighbor is air, add it to drawing list/dictionary
                    //        if (ncell.Block.Type == Block.Types.Air)
                    //        {
                    //            this.VisibleOutdoorCells[FindIndex(cell.LocalCoords)] = cell;
                    //            this.UpdateBlockFaces(cell.LocalCoords, Edges.All, VerticalEdges.All);
                    //            break;
                    //        }
                    //    }

                    //    //this.UpdateBlockFaces(new Vector3(Chunk.Size - 1, i, z), Edges.East, VerticalEdges.None);
                    //    //this.UpdateBlockFaces(new Vector3(i, 0, z), Edges.North, VerticalEdges.None);
                    //    //this.UpdateBlockFaces(new Vector3(0, i, z), Edges.West, VerticalEdges.None);
                    //    //this.UpdateBlockFaces(new Vector3(i, Chunk.Size - 1, z), Edges.South, VerticalEdges.None);
                    //}


                    Vector3[] cellCoords = new Vector3[]{
                        new Vector3(Chunk.Size - 1, i, z), 
                        new Vector3(i, 0, z), 
                        new Vector3(0, i, z),
                        new Vector3(i, Chunk.Size - 1, z)};
                    foreach (var pos in cellCoords)
                    {
                        Cell cell = this[pos];
                        //if (this.UpdateBlockFaces(cell, Edges.All, VerticalEdges.None))
                        //    this.VisibleOutdoorCells[FindIndex(pos)] = cell;
                        if (cell.IsInvisible())
                            continue;
                        this.UpdateBlockFaces(cell, Edges.All, VerticalEdges.All); //VerticalEdges.None);// 
                        if (cell.HorizontalEdges != 0 || cell.VerticalEdges != 0)
                            this.CellsToActivate.Enqueue(cell);
                        //    this.VisibleOutdoorCells[FindIndex(pos)] = cell;
                        //else
                        //    this.VisibleOutdoorCells.Remove(FindIndex(pos));
                    }
                }
        }

        public Queue<Vector3> ResetHeightMapColumn(Vector3 local)
        {
            // i can just pass a global and % it to get locals
            return ResetHeightMapColumn((int)local.X, (int)local.Y);
        }
        public Queue<Vector3> ResetHeightMapColumn(int localx, int localy)
        {
            Queue<Vector3> lightsourcesToHandle = new Queue<Vector3>();
            int z;
            byte light;
            Cell cell;
            light = 15;
            z = this.Map.GetMaxHeight() - 1;
            int firstContact = z;
            bool hit = false;
            while (z >= 0)
            {
                cell = GetLocalCell(localx, localy, z);
                //if (cell.Type == Block.Types.Door)
                //    "tha to gamisw".ToConsole();
                //if (cell.Type != Block.Types.Air)// && cell.Type != Block.Types.Empty)
                if(!hit)
                    if (cell.Block.Type != Block.Types.Air)
                    {
                        hit = true;
                        firstContact = z;
                    }
                if (cell.Opaque)
                {
                    //hit = true;
                    if (light > 0)
                    {
                        HeightMap[localx][localy] = z;
                        light = 0;
                    }
                }
                
                byte oldLight;
                oldLight = GetSunlight(new Vector3(localx, localy, z));
                SetSunlight(localx, localy, z, light);//, out oldLight);

                /// HERE IS THE PROBLEM FOR THE THIRD HIGHER NON-OPAQUE NOT SHOWING
                /// NON OPAQUE BLOCKS DONT EVEN GET IN THE QUEUE TO BE HANDLED
                //if (oldLight != light)
                //    lightsourcesToHandle.Enqueue(cell.LocalCoords.ToGlobal(this));//Position.GetGlobal(this, cell));

                /// INSTEAD, QUEUE BLOCK IF IT IS EQUAL OR UNDER THE FIRST HIT SURFACE
                if (z <= firstContact)
                    lightsourcesToHandle.Enqueue(cell.LocalCoords.ToGlobal(this));
                //if (z == firstContact && hit)
                //    this.VisibleOutdoorCells[FindIndex(cell.LocalCoords)] = cell;
                z--;
            }

            if (light > 0)
                HeightMap[localx][localy] = z;
            return lightsourcesToHandle;
        }


        static public Vector2 GetChunkCoords(Vector3 global)
        {
            return new Vector2((int)Math.Floor(global.X / Chunk.Size), (int)Math.Floor(global.Y / Chunk.Size));
        }
        static public void GetChunkCoords(Vector3 global, out float chunkX, out float chunkY)
        {
            chunkX = (int)Math.Floor(global.X / Chunk.Size);
            chunkY = (int)Math.Floor(global.Y / Chunk.Size);
        }

        public List<Cell> GetEdgeCells()
        {
            List<Cell> list = new List<Cell>();
            return list;
        }


        #endregion

        public void ValidateCells()
        {
            while (this.CellsToValidate.Count > 0)
            {
                Cell cell;
                //this.CellsToValidate.TryTake(out cell);
                //cell = this.CellsToValidate.Dequeue();
                if (!this.CellsToValidate.TryDequeue(out cell))
                    continue;

                //Cell.UpdateEdgesBoundaries(this.Map, cell.GetGlobalCoords(this), Edges.All, VerticalEdges.All);

                // do this here? or when i'm invalidating the cell?
                // WARNING: doing lighting in a separate thread caused the chunk vertex buffer (and the lighting texture) to be built BEFORE the operation is finished
                //Task.Factory.StartNew(() =>
                //{
                    new LightingEngine(this.Map).HandleBatchSync(new Vector3[] { cell.LocalCoords.ToGlobal(this) });
                //});
                    this.Map.ApplyLightChanges();
                //this.UpdateHeightMapColumn(cell.X, cell.Y);
                //var global = cell.LocalCoords.ToGlobal(this);
                //var n = global.GetNeighbors().Concat(new Vector3[] { global }).ToList();
                //new LightingEngine(this.Map).HandleBatchSync(n);

                this.UpdateBlockVisibility(cell);
                cell.Valid = true;
                this.Rebuild();

                //var drawable = cell.IsDrawable();
                //if (drawable)
                //    VisibleOutdoorCells[FindIndex(cell.LocalCoords)] = cell;
                //else
                //    VisibleOutdoorCells.Remove(FindIndex(cell.LocalCoords));
            }
        }

        /// <summary>
        /// Adds or removes a block from the draw list based on whether it's fully surrounded by opaque blocks
        /// </summary>
        /// <param name="cell"></param>
        public void UpdateBlockVisibility(Cell cell)
        {
            this.UpdateBlockFaces(cell);
            var drawable = cell.IsDrawable();
            if (drawable)
                VisibleOutdoorCells[FindIndex(cell.LocalCoords)] = cell;
            else
                VisibleOutdoorCells.Remove(FindIndex(cell.LocalCoords));
        }

        public bool InvalidateCell(Cell cell)
        {
            if (cell == null)
                throw new Exception();
            this.InvalidateLight(cell);
            
            //if (this.CellsToValidate.Contains(cell))
            if(!cell.Valid)
                return false;

            this.CellsToValidate.Enqueue(cell);
            cell.Valid = false;
            //this.CellsToValidate.Add(cell);
            return true;
        }

        public byte GetBlockLight(Vector3 local)
        {
            if (local.Z >= Start_a_Town_.Map.MaxHeight)
                return 15; return BlockLight[FindIndex(local)];
        }
        public byte GetBlockLight(int x, int y, int z)
        {
            int index = FindIndex(x, y, z);
            if (z >= this.Map.GetMaxHeight())
                return 15;
            byte l = BlockLight[index];
            return l;
        }
        public byte GetSunlight(Vector3 local)
        {
            var id = FindIndex(local);
            return Sunlight[id];
            // return (byte)(Sunlight[FindIndex(local)] - (Map.Instance.DayTime * 15));
        }

        public byte GetSunlight(int x, int y, int z)
        {
            if (z >= Map.GetMaxHeight())
                return 15;
            int index = FindIndex(x, y, z);
            return Sunlight[index];
            //return Sunlight.ElementAt(index);
            //return (byte)(Sunlight[FindIndex(x, y, z)] - (Map.Instance.DayTime * 15));
        }

        public void SetSunlight(Vector3 local, byte value) 
        { 
            Sunlight[FindIndex(local)] = value; 
            _Saved = false; 
        }
        public void SetSunlight(int x, int y, int z, byte value) 
        {
            Sunlight[FindIndex(x, y, z)] = value; 
            _Saved = false; 
        }
        public void SetSunlight(int x, int y, int z, byte newValue, out byte oldValue)
        {
            int index = FindIndex(x, y, z);
            oldValue = Sunlight[index];
            this.Sunlight[index] = newValue;
            //this.InvalidateLight(new Vector3(x, y, z)); // is this needed?
            //Position.GetNeighbors(new Vector3(Start.X + x, Start.Y + y, z)).ForEach(global => InvalidateLight(Map, global)); //foo=>InvalidateLight(foo)); // 
            foreach (var n in new Vector3(Start.X + x, Start.Y + y, z).GetNeighbors())
            {
                InvalidateLight(Map, n);
            }//foo=>InvalidateLight(foo)); // 

            _Saved = false;
        }
        void Swap(ref byte reference, byte newValue, out byte oldValue)
        {
            oldValue = reference;
            reference = newValue;
        }

        public void SetBlockLight(Vector3 local, byte value, out byte oldValue)
        {
            int index = FindIndex(local);
            oldValue = BlockLight[index];
            BlockLight[index] = value;
            Position.GetNeighbors(new Vector3(Start.X + local.X, Start.Y + local.Y, local.Z)).ForEach(global => InvalidateLight(Map, global)); //foo=>InvalidateLight(foo)); // 
            _Saved = false;
        }
        public void SetBlockLight(Vector3 local, byte value) 
        {
            BlockLight[FindIndex(local)] = value;
            var index = FindIndex(local);
            BlockLight[index] = value;
            Position.GetNeighbors(new Vector3(Start.X + local.X, Start.Y + local.Y, local.Z)).ForEach(global => InvalidateLight(Map, global)); //foo=>InvalidateLight(foo)); // 
            _Saved = false;
        }

        static public bool TryGetSunlight(Map map, int globalX, int globalY, int globalZ, out byte sunlight)
        {
            sunlight = 0;
            Chunk chunk;

            if (globalZ > Start_a_Town_.Map.MaxHeight - 1)
                return false;
            if (globalZ < 0)
                return false;

            if (!Position.TryGetChunk(map, globalX, globalY, out chunk))
                return false;

            int lx = globalX - chunk.X * Chunk.Size;
            int ly = globalY - chunk.Y * Chunk.Size;
            sunlight = chunk.GetSunlight(lx, ly, globalZ);
            return true;
        }

        /// <summary>
        /// TODO: Move light cache to camera class
        /// </summary>
        public ConcurrentDictionary<Vector3, Color> LightCache = new ConcurrentDictionary<Vector3, Color>();
        //public Dictionary<Vector3, LightToken> LightCache2 = new Dictionary<Vector3, LightToken>();
        
        /// <summary>
        /// TODO: optimize: convert to dictionary for speed
        /// </summary>
        //public ConcurrentDictionary<Vector3, LightToken> LightCache2 = new ConcurrentDictionary<Vector3, LightToken>();
        public Dictionary<Vector3, LightToken> LightCache2 = new Dictionary<Vector3, LightToken>();

        public void ClearLightCache()
        {
            this.LightCache2.Clear();
            //foreach (var cell in this.CellGrid2) // TODO: optimize! slow! (?)
            //    cell.Light = null;
        }
        public void CacheLight(Vector3 global, Color color)
        {
            //this.CachedLight[global] = color;
            this.LightCache.AddOrUpdate(global, color, (pos, existing) => color);
        }
        //bool InvalidateLight(Vector3 local)
        //{
        //    if (local.X < 0 || local.X > Chunk.Size - 1 || local.Y < 0 || local.Y > Chunk.Size - 1)
        //        throw (new Exception("Local coords out of chunk bounds."));
        //    //return CachedLight.Remove(local);
        //    Color oldCol;
        //    return CachedLight.TryRemove(local, out oldCol);
        //}
        static public bool InvalidateLight(IMap map, Vector3 global)
        {
            Chunk chunk;
            Cell cell;
            int lx, ly;
            Color oldCol;
            //if (Position.TryGet(map, (int)global.X, (int)global.Y, (int)global.Z, out cell, out chunk, out lx, out ly))
            if (map.TryGetAll((int)global.X, (int)global.Y, (int)global.Z, out chunk, out cell, out lx, out ly))
            {
                //cell.Light = null;
                LightToken token;
                //return chunk.LightCache2.TryRemove(global, out token);
                return chunk.LightCache2.Remove(global);
            }
            return false;
        }
        public bool InvalidateLight(Cell cell)
        {
            //cell.Light = null;
            LightToken token;
            //this.LightCache2.TryRemove(cell.GetGlobalCoords(this), out token);
            this.LightCache2.Remove(cell.GetGlobalCoords(this));

            Color old;
            return this.LightCache.TryRemove(cell.GetGlobalCoords(this), out old);
        }
        public bool InvalidateLight(Vector3 global)
        {
            //this.Map.GetCell(global).Light = null;
            //var cell = this[global.ToLocal()];
            //cell.Light = null;
            LightToken cached;
            //this.LightCache2.TryRemove(global, out cached);
            this.LightCache2.Remove(global);
            Color old;
            this.Valid = false;
            return this.LightCache.TryRemove(global, out old);
        }

        //static public bool TryGetFinalLight(Map map, int globalX, int globalY, int globalZ, out byte light)
        //{
        //    light = 0;
        //    Chunk chunk;

        //    if (globalZ > Map.MaxHeight - 1)
        //        return false;
        //    if (globalZ < 0)
        //        return false;

        //    if (!Position.TryGetChunk(map, globalX, globalY, out chunk))
        //        return false;

        //    int lx = globalX - chunk.X * Chunk.Size;
        //    int ly = globalY - chunk.Y * Chunk.Size;
        //    byte finalsun = (byte)Math.Max(0, chunk.GetSunlight(lx, ly, globalZ) - map.SkyDarkness);
        //    //byte finalsun = Math.Max((byte)0, chunk.GetSunlight(lx, ly, globalZ));
        //    light = Math.Max(finalsun, chunk.GetBlockLight(lx, ly, globalZ));
        //    return true;
        //}

        static public bool TryGetFinalLight(IMap map, int globalX, int globalY, int globalZ, out byte sky, out byte block)
        {
            Chunk chunk;
            sky = 0;
            block = 0;
            if (globalZ > Start_a_Town_.Map.MaxHeight - 1)
                return false;
            if (globalZ < 0)
                return false;

            //if (!Position.TryGetChunk(map, globalX, globalY, out chunk))
            var global = new Vector3(globalX, globalY, globalZ);
            if (!map.TryGetChunk(global, out chunk))

            {
                // return full skylight if adjacent neighbor chunk doesn't exist?
                sky = 15;
                return false;
            }
            int lx = globalX - chunk.X * Chunk.Size;
            int ly = globalY - chunk.Y * Chunk.Size;
            byte finalsun = (byte)Math.Max(0, chunk.GetSunlight(lx, ly, globalZ) - map.GetSkyDarkness());
            sky = finalsun;
            block = chunk.GetBlockLight(lx, ly, globalZ);
            return true;
        }

        static public bool TryGetSunlight(IMap map, Vector3 global, out byte sunlight)
        {
            sunlight = 0;
            Chunk chunk;

            if (global.Z > map.GetMaxHeight() - 1)
                return false;
            if (global.Z < 0)
                return false;

            //if (!Position.TryGetChunk(map, global, out chunk))
            if (!map.TryGetChunk(global, out chunk))
                return false;

            //Vector3 rounded = new Vector3((int)Math.Round(global.X), (int)Math.Round(global.Y), (int)Math.Floor(global.Z));
            int x = (int)(global.X - chunk.Start.X);
            int y = (int)(global.Y - chunk.Start.Y);
            //sunlight = chunk.Sunlight[x, y, (int)global.Z];
            //sunlight = chunk.Sunlight[x][y][(int)global.Z];
            sunlight = chunk.GetSunlight(x, y, (int)global.Z);
            return true;
        }
        static public bool TryGetBlocklight(Map map, Vector3 global, out byte sunlight)
        {
            sunlight = 0;
            Chunk chunk;

            if (global.Z > Start_a_Town_.Map.MaxHeight - 1)
                return false;
            if (global.Z < 0)
                return false;

            if (!Position.TryGetChunk(map, global, out chunk))
                return false;

            //Vector3 rounded = new Vector3((int)Math.Round(global.X), (int)Math.Round(global.Y), (int)Math.Floor(global.Z));
            int x = (int)(global.X - chunk.Start.X);
            int y = (int)(global.Y - chunk.Start.Y);
            //sunlight = chunk.Sunlight[x, y, (int)global.Z];
            //sunlight = chunk.Sunlight[x][y][(int)global.Z];
            sunlight = chunk.GetBlockLight(x, y, (int)global.Z);
            return true;
        }
        public void UpdateCell(Cell cell, float time)
        {
            //CellsToUpdate.Enqueue(cell);
            CellUpdateTimers[cell] = time;
        }

        //public void RefreshVisibleCells(Map map)
        //{
        //    foreach (Cell cell in CellGrid2)
        //    {
        //        if (cell.Type != Block.Types.Air && cell.Type != Block.Types.Empty)
        //            continue;
        //        foreach (Vector3 n in Position.GetNeighbors(cell.LocalCoords))
        //        {
        //            Cell ncell;
        //            Chunk nchunk;
        //            if (!Position.TryGet(map, n, out ncell, out nchunk))
        //                continue;
        //            if (ncell.Type != Block.Types.Air && ncell.Type != Block.Types.Empty)
        //                Chunk.Show(nchunk, ncell);
        //        }
        //    }
        //}
        //public void ResetVisibleCells()
        //{
        //    VisibleOutdoorCells = new SortedDictionary<int, Cell>();
        //    List<Vector3> handled = new List<Vector3>();
        //    Queue<Vector3> toHandle = new Queue<Vector3>(new Vector3[] { new Vector3(0, 0, Map.MaxHeight - 1) });//new Vector3(Start.X, Start.Y, Map.MaxHeight - 1) });

        //      //  ToggleCell(Map, handled, toHandle);
        //    foreach (var cell in CellGrid2)
        //    {
        //        Cell.UpdateEdges(this.Map, cell.LocalCoords.ToGlobal(this), Edges.All, VerticalEdges.All);// Position.GetGlobal(this, cell));
        //        if (cell.Visible)
        //            if (cell.Type != Block.Types.Air && cell.Type != Block.Types.Empty)
        //                this.Show(cell);
        //            //this.VisibleOutdoorCells[FindDrawIndex(cell.LocalCoords)] = cell;
        //    }
        //}

        #region Updating
        /// <summary>
        /// pass parent map too?
        /// </summary>
        /// <param name="net"></param>
        public void Update(Net.IObjectProvider net)//, GameTime gt)
        {
            while(this.CellsToActivate.Count>0)
            {
                Cell cell;
                this.CellsToActivate.TryDequeue(out cell);
                this.VisibleOutdoorCells[FindIndex(cell.LocalCoords)] = cell;
            }

            UpdateSkyLight();
            UpdateChunkBoundaries();

            this.ValidateCells();


            // TODO: OPTIMIZE
            //if (!this.EdgesValid)
            //    this.UpdateEdges();

            List<GameObject> objectList = new List<GameObject>(Objects);
            foreach (GameObject obj in objectList)
                obj.Update(net, this);

            //foreach (var obj in this.BlockObjects.Values.ToList())
            //    obj.Update(net, this);
            foreach (var blockentity in this.BlockEntities.ToList())
                blockentity.Value.Update(net, blockentity.Key.ToGlobal(this));
        }

        private void UpdateChunkBoundaries()
        {
            if (!this.ChunkBoundariesUpdated)
                return;
            if (!this.Map.ChunkNeighborsExist(this.MapCoords))
                return;
            this.ChunkBoundariesUpdated = false;
            this.ResetVisibleOuterBlocks();
            //this.ResetVisibleCells();
            this.LightCache2.Clear();
        }

        public void UpdateSkyLight()
        {
            if (this.SkylightUpdated)
            {
                //if(this.Map.ChunkNeighborsExist(this.MapCoords))
                if (this.Map.ChunksExist(this.MapCoords, 1))
                {
                    this.SkylightUpdated = false;
                    Queue<WorldPosition> items = new Queue<WorldPosition>();
                    for (int i = 0; i < Chunk.Size; i++)
                    {
                        for (int j = 0; j < Chunk.Size; j++)
                        {
                            var h = this.GetHeightMapValue(i, j);
                            for (int z = 0; z < h; z++)
                            {
                                // schedule updating of each block's neighbor blocks
                                var pos = new WorldPosition(this.Map, new Vector3(i, j, z).ToGlobal(this));
                                items.Enqueue(pos);
                            }
                            //for (int z = h + 1; z < Map.MaxHeight - 1; z++)
                            //// h+1 cause the value is the z value of the first solid block (should i shange it to be the last skylit block?)
                            //{
                            //    // schedule updating of each block's neighbor blocks
                            //    //items.Enqueue(new WorldPosition(this.Map, new Vector3(i, j, z).ToGlobal(this)));
                            //    items.Enqueue(new WorldPosition(this.Map, new Vector3(i - 1, j, z).ToGlobal(this)));
                            //    items.Enqueue(new WorldPosition(this.Map, new Vector3(i + 1, j, z).ToGlobal(this)));
                            //    items.Enqueue(new WorldPosition(this.Map, new Vector3(i, j - 1, z).ToGlobal(this)));
                            //    items.Enqueue(new WorldPosition(this.Map, new Vector3(i, j + 1, z).ToGlobal(this)));

                            //}
                        }
                    }
                    //(new LightingEngine(this.Map)).HandleSkyLight(items);
                    this.Map.UpdateLight(items);

                    //this.ResetVisibleOuterBlocks();
                    //this.LightCache2.Clear();
                }
            }
        }


        #endregion

        #region Drawing
        public void DrawObjects(MySpriteBatch sb, Camera camera, Controller controller, Player player, IMap map, SceneState scene)
        {
            foreach (GameObject obj in this.Objects) //make a copy of the list first because currently the player character might be added while drawing
            {

                Vector3 global = obj.Global;
                //if (global.Z > map.DrawLevel + 1)// - 1)
                if (global.Z > camera.DrawLevel + 1)// - 1)
                    continue;
                //if (Engine.HideTerrain && Player.Actor != null)
                if (camera.HideTerrainAbovePlayer && Player.Actor != null)
                    if (global.Z > Player.Actor.Transform.Global.Z + 2)// - 1)
                        continue;
                Cell cell;
                //if (!Position.TryGetCell(map, global, out cell))

                if (!map.TryGetCell(global, out cell))
                    continue;
                float x = cell.X, y = cell.Y, z = global.Z;// cell.Z;
                // float x, y, z = global.Z;
                //   Position.ToLocal(global, out x, out y);
                // TODO: figure out a way to get depth from actual precise global coords instead of cell coords

                float rx, ry;
                Coords.Rotate(camera, x, y, out rx, out ry);
                Vector3 rotated = new Vector3(rx, ry, z);

                if (!obj.Components.ContainsKey("Sprite"))
                    continue;

                if ((bool)obj["Sprite"]["Hidden"])
                    continue;

                Sprite sprite = obj.GetComponent<SpriteComponent>().Sprite;// (Sprite)obj["Sprite"]["Sprite"];
                Rectangle spriteBounds = sprite.GetBounds();
                Rectangle screenBounds = camera.GetScreenBounds(global, spriteBounds);//(int)Start.X + x, (int)Start.Y + y, z, spriteBounds);
                screenBounds.X -= Graphics.Borders.Thickness;
                screenBounds.Y -= Graphics.Borders.Thickness;
                if (!camera.ViewPort.Intersects(screenBounds))
                    continue;
                //float cd = 1 - Position.GetDepth(rotated) / Dmax;
                //float _cd = cd;
                float cd = global.GetDrawDepth(map, camera);
                //Game1.Instance.Effect.Parameters["ObjectHeight"].SetValue(_cd);


                byte light = Math.Max((byte)(GetSunlight(cell.LocalCoords) - map.GetSkyDarkness()), GetBlockLight(cell.LocalCoords));
                float l = (light + 1) / 16f;
                Color color = new Color(l, l, l, 1);// new Color((light + 1) / 16f, 0, 0);
                Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));

                obj.Draw(sb, new DrawObjectArgs(camera, controller, player, map, this, cell, spriteBounds, screenBounds, obj, color, cd));
                //SpriteComponent.DrawShadow(camera, spriteBounds, map, obj.Global, cd,  cd);//_cd);
                SpriteComponent.DrawShadow(camera, spriteBounds, map, obj, cd, cd);//_cd);

                //map.ObjectsDrawn++;
                if (scene.ObjectsDrawn.Contains(obj))
                    throw new Exception(); // WARNING!!! has thrown!!! race condition? object added from another thread while drawing?
                scene.ObjectsDrawn.Add(obj);
                scene.ObjectBounds.Add(obj, screenBounds);
            }
        }

        public void DrawInterface(SpriteBatch sb, Camera cam)//, SceneState scene)
        {
            foreach (GameObject obj in Objects.ToList().Concat(BlockObjects.Values.ToList()))
                obj.DrawInterface(sb, cam);
            foreach (var blockentity in this.BlockEntities)
                blockentity.Value.DrawUI(sb, cam, blockentity.Key.ToGlobal(this));
            //Rectangle chunkBounds = cam.GetScreenBounds(this.Start.X + Chunk.Size / 2, this.Start.Y + Chunk.Size / 2, Map.MaxHeight / 2, this.GetBounds());  //chunk.Value.GetBounds(camera);
            //chunkBounds.DrawHighlight(sb, 0.1f);
        }


        public void DrawHighlight(SpriteBatch sb, Rectangle bounds)
        {
            sb.Draw(UI.UIManager.Highlight, bounds, null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.None, 0);
            //camera.SpriteBatch.Draw(UI.UIManager.Highlight, new Vector2(bounds.X, bounds.Y), null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, 1, SpriteEffects.None, 0);
        }
        #endregion
        public string GetDirectoryPath()
        {
            return this.Map.GetFullPath() + "/chunks/" + this.DirectoryName;
        }
        #region Saving and Loading
        internal void SaveToFile()//Action<Chunk> callback)
        {
            //return;
            //if (Saved)
            //    return;
            //DateTime now = DateTime.Now;
            Chunk copy = this.Clone() as Chunk;
            //(DateTime.Now - now).ToConsole();
            //Task.Factory.StartNew(() =>
            //{
                string filename = GetFilename(this.MapCoords);
                string newFile = "_" + filename;

                string directory = this.GetDirectoryPath();// @"/Saves/Worlds/" + Map.World.Name + "/" + Map.GetFolderName() + "/chunks/";
                //if (!Directory.Exists(directory))
                //    Directory.CreateDirectory(directory);
                directory = @"/Saves/Worlds/" + Map.GetWorld().GetName() + "/" + Map.GetFolderName() + "/chunks/";
                
                string working = Directory.GetCurrentDirectory();
                string fullpath = working + directory + this.DirectoryName;
                fullpath = this.Map.GetFullPath() + "/chunks/" + this.DirectoryName;
                if (!Directory.Exists(fullpath))
                    Directory.CreateDirectory(fullpath);
                copy.SaveToFile(newFile);
                if (File.Exists(fullpath + filename))
                    try
                    {
                        //File.Replace(working + directory + newFile, working + directory + filename, working + directory + filename + ".bak");
                        File.Replace(fullpath + newFile, fullpath + filename, fullpath + filename + ".bak");
                        File.Delete(fullpath + filename + ".bak");
                    }
                    catch (IOException)
                    {
                        Net.Server.Console.Write(Color.Red, "SERVER", "Error saving Chunk " + copy.MapCoords.ToString());
                        // recover back up here?
                    }
                else
                    File.Move(fullpath + newFile, fullpath + filename);
                //File.Move(working + directory + newFile, working + directory + filename);

                //GenerateThumbnails(fullpath);

                // race condition for adding ui controls
                Net.Server.Console.Write(Color.Lime, "SERVER", "Chunk " + copy.MapCoords.ToString() + " saved succesfully \"" + directory + filename + "\"");
                this._Saved = true;
                //callback(this);
            //});
        }

        //internal override List<SaveTag> Save()
        internal string SaveToFile(string filename)
        {
            //string directory = @"/Saves/Worlds/" + Map.World.Name + "/" + Map.GetFolderName() + "/chunks/"; //Directory.GetCurrentDirectory() +
            string directory = FullDirPath;

           // string filename = GetFilename(MapCoords);// MapCoords.X.ToString() + "." + MapCoords.Y.ToString() + ".chunk";
            //FileStream stream = new FileStream(directory + filename, System.IO.FileMode.OpenOrCreate);
            DateTime now = DateTime.Now;
            SaveTag chunktag;
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);

                chunktag = new SaveTag(SaveTag.Types.Compound, "Chunk");
                SaveTag cellstag = new SaveTag(SaveTag.Types.List, "Cells", SaveTag.Types.Compound);
                //  Tag sunlightTag = new Tag(Tag.Types.List, "Sunlight", Tag.Types.Byte);
                //  Tag celllightTag = new Tag(Tag.Types.List, "CellLight", Tag.Types.Byte);
                SaveTag lightTag = new SaveTag(SaveTag.Types.List, "Light", SaveTag.Types.Byte);
                SaveTag heightTag = new SaveTag(SaveTag.Types.List, "Heightmap", SaveTag.Types.Byte);
                SaveTag entitiestag = new SaveTag(SaveTag.Types.List, "Entities", SaveTag.Types.Compound);
                SaveTag blockObjectsTag = new SaveTag(SaveTag.Types.List, "Block Objects", SaveTag.Types.Compound);
                SaveTag visibleCells = new SaveTag(SaveTag.Types.List, "VisibleCells", SaveTag.Types.Int);
                SaveTag blockEntitiesTag = new SaveTag(SaveTag.Types.List, "BlockEntities", SaveTag.Types.Compound);

                int n = 0;
                foreach (Cell cell in CellGrid2)
                {
                    cellstag.Add(Cell.Save(cell));//cell.SaveData);
                    byte light = (byte)((Sunlight[n] << 4) + BlockLight[n++]);
                    lightTag.Add(new SaveTag(SaveTag.Types.Byte, "", light));
                    //        sunlightTag.Add(new Tag(Tag.Types.Byte, "", Sunlight[n]));//FindIndex(cell.LocalCoords)]));
                    //         celllightTag.Add(new Tag(Tag.Types.Byte, "", CellLight[n++]));
                }

                for (int j = 0; j < Size; j++)
                    for (int i = 0; i < Size; i++)
                        heightTag.Add(new SaveTag(SaveTag.Types.Byte, "", (byte)HeightMap[i][j]));

                foreach (Cell cell in VisibleOutdoorCells.Values)
                    visibleCells.Add(new SaveTag(SaveTag.Types.Int, "", FindIndex(cell.LocalCoords)));

                //if (Player.Actor != null)
                //    Objects.Remove(Player.Actor);
           //     foreach (var player in Net.Server.Players)

                // don't save player characters
                //Objects.RemoveAll(o => (from player in Net.Server.Players select Net.Server.Instance.GetNetworkObject(player.CharacterID)).Contains(o));
                Objects.RemoveAll(o => o.ID == GameObject.Types.Actor);

                foreach (GameObject obj in Objects)
                    if (obj.ID != GameObject.Types.Dummy)
                        entitiestag.Add(new SaveTag(SaveTag.Types.Compound, obj.Name, obj.Save()));

                foreach (KeyValuePair<int, GameObject> blockObj in BlockObjects)
                {
                    SaveTag blobjTag = new SaveTag(SaveTag.Types.Compound, "");
                    blobjTag.Add(new SaveTag(SaveTag.Types.Int, "Index", blockObj.Key));//FindIndex(blockObj.Key)));
                    blobjTag.Add(new SaveTag(SaveTag.Types.Compound, "Object", blockObj.Value.Save()));
                    blockObjectsTag.Add(blobjTag);
                }

                foreach (var blockEntity in this.BlockEntities)
                {
                    SaveTag tag = new SaveTag(SaveTag.Types.Compound, "");
                    tag.Add(blockEntity.Key.Save("Local"));
                    var entitysavetag = blockEntity.Value.Save("Entity");
                    if(entitysavetag!=null)
                    tag.Add(entitysavetag);
                    blockEntitiesTag.Add(tag);
                }
                //if (Player.Actor != null)
                //    Objects.Add(Player.Actor);

                //chunktag.Add(sunlightTag);
                //chunktag.Add(celllightTag);
                chunktag.Add(new SaveTag(SaveTag.Types.Bool, "LightValid", this.LightValid));
                chunktag.Add(new SaveTag(SaveTag.Types.Bool, "EdgesValid", this.EdgesValid));
                chunktag.Add(lightTag);
                chunktag.Add(heightTag);
                chunktag.Add(cellstag);
                chunktag.Add(visibleCells);
                chunktag.Add(entitiestag);
                chunktag.Add(blockObjectsTag);
                chunktag.Add(blockEntitiesTag);
                chunktag.WriteTo(writer);

                Compress(stream, directory + filename);// GetFilename(this.MapCoords));//MapCoords.X.ToString() + "." + MapCoords.Y.ToString() + ".chunk"); //@"/Saves/"
                //Compress(stream, filename);
                stream.Close();
            }
            //writer.Close();
            Console.WriteLine(filename + " saved in " + (DateTime.Now - now).ToString());
            return directory + GetFilename(this.MapCoords);
            // return chunktag;
            //return true;

        }

        static public Chunk Load(IMap map, string fullpath)
        {
            string filename = fullpath.Split('\\').Last();
            string[] c = filename.Split('.');
            Vector2 coords = new Vector2(Convert.ToInt32(c[0]), Convert.ToInt32(c[1]));
            Chunk chunk = new Chunk(coords);
            chunk.Map = map;
            using (FileStream stream = new FileStream(fullpath, System.IO.FileMode.Open))
            {
                using (MemoryStream decompressedStream = Decompress(stream))
                {
                    BinaryReader reader = new BinaryReader(decompressedStream);

                    SaveTag chunktag = SaveTag.Read(reader);

                    chunk.LightValid = chunktag.TagValueOrDefault<bool>("LightValid", false);
                    chunk.EdgesValid = chunktag.TagValueOrDefault<bool>("EdgesValid", false);

                    List<SaveTag> celllist = chunktag["Cells"].Value as List<SaveTag>;
                    List<SaveTag> lightTag = chunktag["Light"].Value as List<SaveTag>;

                    int n = 0;
                    for (int h = 0; h < Start_a_Town_.Map.MaxHeight; h++)
                        for (int i = 0; i < Size; i++)
                            for (int j = 0; j < Size; j++)
                            {
                                Cell cell = Cell.Load(celllist[n]);
                                cell.LocalCoords = new Vector3(i, j, h);
                                //chunk.CellGrid2.Add(cell);
                                chunk.CellGrid2[n] = cell;
                                byte light = (byte)lightTag[n].Value;
                                // DONT save light, recalculate on load
                                chunk.Sunlight[n] = (byte)((light & 0xF0) >> 4);
                                chunk.BlockLight[n] = (byte)(light & 0x0F);
                                //if ((byte)(light & 0x0F) > 0)
                                //    "ole".ToConsole();
                                n++;
                            }

                    List<SaveTag> visibleCells = chunktag["VisibleCells"].Value as List<SaveTag>;
                    foreach (SaveTag tag in visibleCells)
                    {
                        int index = (int)tag.Value;
                        var cell = chunk.CellGrid2[index];
                        chunk.VisibleOutdoorCells[index] = cell;
                        //chunk.InvalidateCell(cell);
                    }


                    List<SaveTag> heightTag = chunktag["Heightmap"].Value as List<SaveTag>;
                    n = 0;
                    for (int j = 0; j < Size; j++)
                        for (int i = 0; i < Size; i++)
                            chunk.HeightMap[i][j] = (byte)heightTag[n++].Value;

                    List<SaveTag> entitytags = chunktag["Entities"].Value as List<SaveTag>;

                    foreach (SaveTag tag in entitytags)
                    {
                        // TODO: pass the object factory as an argument
                        GameObject obj = GameObject.Load(tag);//, Net.Server.NetworkIDGenerator);
                        chunk.Objects.Add(obj);
                        //obj.GetComponent<PositionComponent>().Exists = true;
                        //obj.Map = map;

                        // figure out if necessary to call this
                        //obj.ChunkLoaded();
                    }

                    SaveTag blobjTag;
                    if (chunktag.TryGetTag("Block Objects", out blobjTag))
                        foreach (SaveTag tag in blobjTag.Value as List<SaveTag>)
                        {
                            int index = (int)tag["Index"].Value;
                            GameObject obj = GameObject.Load(tag["Object"]);
                            chunk.BlockObjects[index] = obj;
                        }

                    SaveTag blentitiesjTag;
                    if (chunktag.TryGetTag("BlockEntities", out blentitiesjTag))
                        foreach (SaveTag tag in blentitiesjTag.Value as List<SaveTag>)
                        {
                            var local = tag["Local"].LoadVector3();
                            //var entity = chunk.CellGrid2[FindIndex(local)].Block.GetBlockEntity();
                            var entity = chunk[local].Block.GetBlockEntity();
                            //entity.Load(tag["Entity"]); // dont do that because some blockentities don't require to save data, they return a null savetag when saving and it isn't saved in the file
                            tag.TryGetTag("Entity", t => entity.Load(t));
                            chunk.BlockEntities[local] = entity;
                        }


                    reader.Close();
                }
            }
            chunk._Saved = true;
            return chunk;
        }
        static public void Compress(Stream stream, string filename)
        {
            //try
            //{
                using (stream)
                {
                    stream.Position = 0;
                    //string fullpath = Directory.GetCurrentDirectory() + filename;
                    using (FileStream outFile = File.Create(filename)) //filename))//
                    {
                        using (GZipStream zip = new GZipStream(outFile, CompressionMode.Compress))
                        {
                            stream.CopyTo(zip);
                        }
                    }
                }
            //}
            //catch (IOException) {
            //    Net.Server.Console.Write(Color.Red, "SERVER", "Error file " + filename); 
            //}
        }
        static public MemoryStream Decompress(FileStream compressed)
        {
            using (compressed)
            {
                using (GZipStream decompress = new GZipStream(compressed, CompressionMode.Decompress))
                {
                    //  using (MemoryStream memory = new MemoryStream())
                    //  {
                    MemoryStream memory = new MemoryStream();
                    decompress.CopyTo(memory);
                    memory.Position = 0;
                    return memory;
                    //   }
                }
            }
        }
        static public string GetFilename(Vector2 pos)
        {
            return pos.X.ToString() + "." + pos.Y.ToString() + ".chunk.sat";
        }
        static public string GetDirName(Vector2 pos)
        {
            return pos.X.ToString() + "." + pos.Y.ToString() + "/";
        }
        public string GetFilename()
        {
            return ((int)(this.MapCoords.X)).ToString() + "." + ((int)(this.MapCoords.Y)).ToString() + ".chunk.sat";
        }
        #endregion
        public bool IsCellInChunk(Vector3 local)
        {
            //return !(local.X < 0 || local.X > Chunk.Size - 1 || local.Y < 0 || local.Y > Chunk.Size - 1 || local.Z < 0 || local.Z > this.Map.GetMaxHeight() - 1);
            return !(local.X < 0 || local.X > Chunk.Size - 1 || local.Y < 0 || local.Y > Chunk.Size - 1 || local.Z < 0 || local.Z > Start_a_Town_.Map.MaxHeight - 1);

        }
        public bool IsCellInChunk(int localx, int localy, int localz)
        {
            return (localx < 0 || localx > Chunk.Size - 1 || localy < 0 || localy > Chunk.Size - 1 || localz < 0 || localz > Start_a_Town_.Map.MaxHeight - 1);
        }
        public bool TryGetCell(Vector3 local, out Cell cell)
        {
            cell = this[local];
            if (cell == null)
                return this.Map.TryGetCell(local.ToGlobal(this), out cell);
                //return local.ToGlobal(this).TryGetCell(this.Map, out cell);
            return true;
            //return cell != null;
        }

        public bool UpdateBlockFaces(Vector3 local, Edges horEdgesToCheck, VerticalEdges verEdgesToCheck)
        {
            Cell cell = this[local];
            return this.UpdateBlockFaces(cell, horEdgesToCheck, verEdgesToCheck);
            //if (cell == null)
            //    return false;
        }

        public bool UpdateBlockFaces(Cell cell)
        {
            return this.UpdateBlockFaces(cell, Edges.All, VerticalEdges.All);
        }

        /// <summary>
        /// Do this on chunk loading or after it's been added to the map's active chunks?
        /// </summary>
        /// <param name="local"></param>
        /// <param name="horEdgesToCheck"></param>
        /// <param name="verEdgesToCheck"></param>
        /// <returns></returns>
        public bool UpdateBlockFaces(Cell cell, Edges horEdgesToCheck, VerticalEdges verEdgesToCheck)
        {
            //Cell cell = this[local];
            if (cell == null)
                return false;
            var local = cell.LocalCoords;
            Edges lastEdges = cell.HorizontalEdges;
            VerticalEdges lastVerticalEdges = cell.VerticalEdges;

            if ((horEdgesToCheck & Edges.West) == Edges.West)
            {
                Cell west;
                if (this.TryGetCell(local - new Vector3(1, 0, 0), out west))
                {
                    //if (!west.IsInvisible())
                    if (west.Opaque || (cell.Block == Block.Water && (west.Block == Block.Water && west.BlockData == 1))) //if current block is water and neightbor block is water and is full, hide face
                        cell.HorizontalEdges &= ~Edges.West;
                    else
                        cell.HorizontalEdges |= Edges.West;
                }
                else
                    cell.HorizontalEdges &= ~Edges.West;
            }
            if ((horEdgesToCheck & Edges.North) == Edges.North)
            {
                Cell north;
                if (TryGetCell((local - new Vector3(0, 1, 0)), out north))
                {
                    //if (!north.IsInvisible())
                    if (north.Opaque || (cell.Block == Block.Water && (north.Block == Block.Water && north.BlockData == 1)))
                        cell.HorizontalEdges &= ~Edges.North;
                    else
                        cell.HorizontalEdges |= Edges.North;
                }
                else
                    cell.HorizontalEdges &= ~Edges.North;
            }
            if ((horEdgesToCheck & Edges.South) == Edges.South)
            {
                Cell south;
                if (this.TryGetCell((local + new Vector3(0, 1, 0)), out south))
                {
                    //if (!south.IsInvisible())
                    if (south.Opaque || (cell.Block == Block.Water && (south.Block == Block.Water && south.BlockData == 1)))
                        cell.HorizontalEdges &= ~Edges.South;
                    else
                        cell.HorizontalEdges |= Edges.South;
                }
                else
                    cell.HorizontalEdges &= ~Edges.South;
            }
            if ((horEdgesToCheck & Edges.East) == Edges.East)
            {
                Cell east;
                if (this.TryGetCell((local + new Vector3(1, 0, 0)), out east))
                {
                    //if (!east.IsInvisible())
                    if (east.Opaque || (cell.Block == Block.Water && (east.Block == Block.Water && east.BlockData == 1)))
                        cell.HorizontalEdges &= ~Edges.East;
                    else
                        cell.HorizontalEdges |= Edges.East;
                }
                else
                    cell.HorizontalEdges &= ~Edges.East;
            }
            if ((verEdgesToCheck & VerticalEdges.Top) == VerticalEdges.Top)
            {
                Cell top;
                if (this.TryGetCell((local + new Vector3(0, 0, 1)), out top))
                {
                    //if (!top.IsInvisible())
                    if (top.Opaque || (cell.Block == Block.Water && (top.Block == Block.Water && top.BlockData == 1)))
                        cell.VerticalEdges &= ~VerticalEdges.Top;
                    else
                        cell.VerticalEdges |= VerticalEdges.Top;
                }
                else
                    cell.VerticalEdges &= ~VerticalEdges.Top;
            }
            if ((verEdgesToCheck & VerticalEdges.Bottom) == VerticalEdges.Bottom)
            {
                Cell bottom;
                if (this.TryGetCell((local - new Vector3(0, 0, 1)), out bottom))
                {
                    //if (!bottom.IsInvisible())
                    if (bottom.Opaque || (cell.Block == Block.Water && (bottom.Block == Block.Water && bottom.BlockData == 1)))
                        cell.VerticalEdges &= ~VerticalEdges.Bottom;
                    else
                        cell.VerticalEdges |= VerticalEdges.Bottom;
                }
                else
                    cell.VerticalEdges &= ~VerticalEdges.Bottom;
            }
            if (cell.VerticalEdges != lastVerticalEdges || cell.HorizontalEdges != lastEdges)
                this.InvalidateLight(local);
            //return !cell.IsInvisible() && (cell.VerticalEdges != 0 || cell.HorizontalEdges != 0);
            return true;
        }



        public List<Vector3> GetEdges()
        {
            List<Vector3> list = new List<Vector3>();
            for (int i = 0; i < Chunk.Size; i++)
                for (int z = 0; z < Map.GetMaxHeight(); z++)
                {
                    list.AddRange(new Vector3[]{
                        new Vector3(Start.X + i, Start.Y, z),
                        new Vector3(Start.X, Start.Y + i, z),
                        new Vector3(Start.X + i, Start.Y + Chunk.Size - 1, z),
                        new Vector3(Start.X + Chunk.Size - 1, Start.Y + i, z)});
                }
            return list;
        }
        public List<Vector3> GetEdges(Edges edges)
        {
            HashSet<Vector3> list = new HashSet<Vector3>();
            if ((edges & Edges.East) == Edges.East)
                for (int i = 0; i < Chunk.Size; i++)
                    for (int z = 0; z < Start_a_Town_.Map.MaxHeight; z++)
                        list.Add(new Vector3(Start.X + Chunk.Size - 1, Start.Y + i, z));

            if ((edges & Edges.West) == Edges.West)
                for (int i = 0; i < Chunk.Size; i++)
                    for (int z = 0; z < Start_a_Town_.Map.MaxHeight; z++)
                        list.Add(new Vector3(Start.X, Start.Y + i, z));

            if ((edges & Edges.North) == Edges.North)
                for (int i = 0; i < Chunk.Size; i++)
                    for (int z = 0; z < Start_a_Town_.Map.MaxHeight; z++)
                        list.Add(new Vector3(Start.X + i, Start.Y, z));

            if ((edges & Edges.South) == Edges.South)
                for (int i = 0; i < Chunk.Size; i++)
                    for (int z = 0; z < Start_a_Town_.Map.MaxHeight; z++)
                        list.Add(new Vector3(Start.X + i, Start.Y + Chunk.Size - 1, z));

            return list.ToList();
        }

        internal List<GameObject> GetObjects()
        {
            return new List<GameObject>(Objects);
        }
        internal List<GameObject> GetBlockObjects()
        {
            return new List<GameObject>(BlockObjects.Values);
        }

        public void OnCameraRotated(Camera camera)
        {
            var oldList = VisibleOutdoorCells;
            int r = -(int)camera.Rotation;
            VisibleOutdoorCells = new SortedDictionary<int, Cell>();
            //VisibleOutdoorCells = new Dictionary<int, Cell>();
            foreach (var cell in oldList.ToList())
            {
                VisibleOutdoorCells[FindIndex(cell.Value.LocalCoords)] = cell.Value;
                
            }
            this.ClearLightCache();
            LightCache = new ConcurrentDictionary<Vector3, Color>();
            this.LightCache2.Clear();// new Dictionary<Vector3, LightToken>();
        }


        void WriteCells(BinaryWriter writer)
        {
            var w = writer;
            int consecutiveAirblocks = 0;
            foreach (var cell in CellGrid2)
            {
                if (cell.Block.Type == Block.Types.Air)
                {
                    consecutiveAirblocks++;
                    continue;
                }
                if (consecutiveAirblocks > 0)
                {
                    // write air block length
                    w.Write(0);
                    w.Write(consecutiveAirblocks);
                    consecutiveAirblocks = 0;
                }
                w.Write((int)cell.Block.Type);
                w.Write(cell.Variation);

                w.Write((byte)cell.HorizontalEdges);
                w.Write((byte)cell.VerticalEdges);

                w.Write(cell.Data2.Data);
            }
            w.Write(-1);
        }
        void ReadCells(BinaryReader r)
        {
            int cellIndex = 0;
            int type;
            do
            {
                type = r.ReadInt32();
                if (type == 0)
                {
                    // read length of consecutive air blocks
                    int consecutiveAirblocks = r.ReadInt32();
                    for (int j = 0; j < consecutiveAirblocks; j++)
                        this.CellGrid2[cellIndex++].SetBlockType(Block.Types.Air);
                }
                else if (type > 0)
                {
                    Cell cell = this.CellGrid2[cellIndex++];
                    //cell.Type = (Block.Types)type;
                    cell.SetBlockType(type);
                    cell.Variation = r.ReadByte();

                    cell.HorizontalEdges = (Edges)r.ReadByte();
                    cell.VerticalEdges = (VerticalEdges)r.ReadByte();

                    cell.Data2 = new BitVector32(r.ReadInt32());
                }
            } while (type > -1);
        }
        #region Serialization
        public override void Write(BinaryWriter writer)
        {
            writer.Write(this.MapCoords);

            writer.Write(this.LightValid);
            writer.Write(this.EdgesValid);

            //var notAir = CellGrid2.Where(c => c.Type != Block.Types.Air);
            //writer.Write(notAir.Count());
            //foreach (var cell in notAir)
            //    cell.Write(writer);

            this.WriteCells(writer);

            // must handle object transfer seperately to assign unique network ids
            // SOLVED by making each object knowing their id
            // DONT save player entities (player entity id == 0)
            //var entitiesToSave = this.Objects.Where(o => o.ID != 0).ToList(); ;
            //writer.Write(entitiesToSave.Count);
            //foreach (var obj in entitiesToSave.ToList())
            //    obj.Write(writer);
            writer.Write(this.Objects.Count);
            foreach (var obj in this.Objects.ToList())
                obj.Write(writer);

            writer.Write(BlockObjects.Count);
            foreach (var obj in BlockObjects.Values.ToList())
                obj.Write(writer);

            writer.Write(this.BlockEntities.Count);
            foreach (var entity in BlockEntities.ToList())
            {
                writer.Write(entity.Key);
                entity.Value.Write(writer);
            }

            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    writer.Write(this.HeightMap[i][j]);

            writer.Write(this.Sunlight.ToArray());
            writer.Write(this.BlockLight.ToArray());

            writer.Write(this.VisibleOutdoorCells.Count);
            foreach (var item in this.VisibleOutdoorCells.Values.ToList())
            {
                var vector = item.LocalCoords;
                writer.Write((byte)vector.X);
                writer.Write((byte)vector.Y);
                writer.Write((byte)vector.Z);
            }
            //foreach (var item in this.VisibleOutdoorCells)
            //{
            //    var vector = item.Value.LocalCoords;
            //    writer.Write((byte)vector.X);
            //    writer.Write((byte)vector.Y);
            //    writer.Write((byte)vector.Z);
            //}
        }
        static new public Chunk Read(byte[] data)
        {
            return Read(new BinaryReader(new MemoryStream(data)));
        }
        static new public Chunk Read(BinaryReader reader)
        {
            Chunk chunk = new Chunk();
            chunk.MapCoords = reader.ReadVector2();

            chunk.LightValid = reader.ReadBoolean();
            chunk.EdgesValid = reader.ReadBoolean();

            //chunk.InitCells(c => { });
            //int notAirCount = reader.ReadInt32();
            //for (int i = 0; i < notAirCount; i++)
            //{
            //    Cell cell = Cell.Create(reader);
            //    chunk.CellGrid2[FindIndex(cell.LocalCoords)] = cell;
            //}

            // TODO: OPTIMIZE
            chunk.InitCells();
            chunk.ReadCells(reader);

            // must handle object transfer seperately to assign unique network ids
            int objCount = reader.ReadInt32();
            for (int i = 0; i < objCount; i++)
                chunk.Objects.Add(GameObject.CreatePrefab(reader));//.CreateCustomObject(reader));

            int blockObjCount = reader.ReadInt32();
            for (int i = 0; i < blockObjCount; i++)
            {
                GameObject blobj = GameObject.CreatePrefab(reader);
                chunk.BlockObjects[FindIndex(blobj.Global.ToLocal())] = blobj;
            }

            int blockEntityCount = reader.ReadInt32();
            for (int i = 0; i < blockEntityCount; i++)
            {
                var local = reader.ReadVector3();
                var entity = chunk[local].Block.GetBlockEntity();
                entity.Read(reader);
                chunk.BlockEntities[local] = entity;
            }

            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    chunk.HeightMap[i][j] = reader.ReadInt32();

            chunk.Sunlight = reader.ReadBytes(Size * Size * Start_a_Town_.Map.MaxHeight).ToList();
            chunk.BlockLight = reader.ReadBytes(Size * Size * Start_a_Town_.Map.MaxHeight).ToList();

            int visiblecount = reader.ReadInt32();
            for (int i = 0; i < visiblecount; i++)
            {
                //Vector3 local = reader.ReadVector3();
                byte x = reader.ReadByte();
                byte y = reader.ReadByte();
                byte z = reader.ReadByte();
                Vector3 local = new Vector3(x, y, z);
                Cell cell = chunk[local];
                chunk.VisibleOutdoorCells[FindIndex(local)] = cell;
                chunk.InvalidateCell(cell);
            }

            // TODO: OPTIMIZE
            //foreach (var cell in chunk.VisibleOutdoorCells.Values)
            //    chunk.InvalidateCell(cell);
            return chunk;
        }
        #endregion

        //public string FullDirPath { get { return @"/Saves/Worlds/" + this.Map.GetWorld().GetName() + "/" + this.Map.GetFolderName() + "/chunks/" + this.DirectoryName; } }
        public string FullDirPath { get { return this.Map.GetFullPath() + "/chunks/" + this.DirectoryName; } }

        public string DirectoryName { get { return (((int)(this.MapCoords.X)).ToString() + "." + ((int)(this.MapCoords.Y)).ToString()) + "/"; } }
        public void SaveThumbnails()
        {
            this.GenerateThumbnails();
        }
        public void GenerateThumbnails()
        {
            return;
            string fullpath = this.GetDirectoryPath();
            if (!Directory.Exists(fullpath))
                Directory.CreateDirectory(fullpath);

            using (Texture2D thumbnail = GetThumbnail())
            {
                using (FileStream stream = new FileStream(fullpath + "thumbnailSmall.png", FileMode.OpenOrCreate))
                {
                    thumbnail.SaveAsPng(stream, thumbnail.Width, thumbnail.Height);
                    stream.Close();
                }
                using (FileStream stream = new FileStream(fullpath + "thumbnailSmaller.png", FileMode.OpenOrCreate))
                {
                    thumbnail.SaveAsPng(stream, thumbnail.Width / 2, thumbnail.Height / 2);
                    stream.Close();
                }
                using (FileStream stream = new FileStream(fullpath + "thumbnailSmallest.png", FileMode.OpenOrCreate))
                {
                    thumbnail.SaveAsPng(stream, thumbnail.Width / 4, thumbnail.Height / 4);
                    stream.Close();
                }
            }
        }
        public Texture2D GetThumbnail()
        {
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            float zoom = 1 / 8f;
            int width = (int)(Chunk.Size * Block.Width * zoom);
            int height = (int)((Start_a_Town_.Map.MaxHeight * Block.BlockHeight + Chunk.Size * Block.Depth) * zoom);
            //Camera camera = new Camera(width, height, x: this.MapCoords.X, y: this.MapCoords.Y, z: Map.MaxHeight / 2, zoom: zoom);
            Camera camera = new Camera(width, height, this.Start.X, this.Start.Y, z: Start_a_Town_.Map.MaxHeight / 2, zoom: zoom);
            RenderTarget2D final = new RenderTarget2D(gd, width, height);
            Map map = new Map(new World(), Vector2.Zero);
            map.ActiveChunks[Vector2.Zero] = this;
            camera.NewDraw(final, map, gd, EngineArgs.Default, new SceneState(), PlayerControl.ToolManager.Instance);
            gd.SetRenderTarget(null);
            return final;
        }
        public MySpriteBatch VertexBuffer, TransparentBlocksVertexBuffer, NonOpaqueBuffer, TopSliceMesh, TopSliceTransparentMesh, TopSliceNonOpaqueBuffer;// = new MySpriteBatch(Game1.Instance.GraphicsDevice);

      
        public void BuildSlice(Camera camera, IMap map, int slice)
        {
            this.TopSliceMesh = new MySpriteBatch(Game1.Instance.GraphicsDevice);
            this.TopSliceNonOpaqueBuffer = new MySpriteBatch(Game1.Instance.GraphicsDevice);
            this.TopSliceTransparentMesh = new MySpriteBatch(Game1.Instance.GraphicsDevice);
            for (int i = 0; i < Chunk.Size; i++)
            {
                for (int j = 0; j < Chunk.Size; j++)
                {
                    Cell cell;
                    var local = new Vector3(i, j, slice);//this.DrawLevel);
                    cell = this.CellGrid2[FindIndex(local)];
                    if (cell.Block.Type == Block.Types.Air)
                        continue;
                    camera.DrawCell(this.TopSliceMesh, this.TopSliceNonOpaqueBuffer, this.TopSliceTransparentMesh, map, this, cell);
                }
            }
        }
        public void BuildFrontmostBlocks(Camera camera)
        {
            var chunkX = (int)this.MapCoords.X;
            var chunkY = (int)this.MapCoords.Y;
            var mapSizeInChunks = this.Map.GetSizeInChunks();
            int edgeX = 0, edgeY = 0;
            switch ((int)camera.Rotation)
            {
                case 0:
                    edgeX = mapSizeInChunks - 1;
                    edgeY = mapSizeInChunks - 1;
                    break;
                case 1:
                    edgeX = mapSizeInChunks - 1;
                    edgeY = 0;
                    break;
                case 2:
                    edgeX = 0;
                    edgeY = 0;
                    break;
                case 3:
                    edgeX = 0;
                    edgeY = mapSizeInChunks - 1;
                    break;
                default:
                    break;
            }
            var maxheight = this.Map.GetMaxHeight();

            if (chunkX == edgeX)// || chunk.Key.Y == this.Size.Chunks)
            {
                for (int i = 0; i < Chunk.Size; i++)
                    for (int j = 0; j < maxheight; j++)
                    {
                        {
                            Cell cell;
                            Vector3 pos = new Vector3(Chunk.Size - 1, i, j);
                            switch ((int)camera.Rotation)
                            {
                                case 0:
                                case 1:
                                    pos = new Vector3(Chunk.Size - 1, i, j);
                                    break;
                                case 2:
                                case 3:
                                    pos = new Vector3(0, i, j);
                                    break;

                                default:
                                    break;
                            }
                            var cellIndex = Chunk.FindIndex((int)pos.X, (int)pos.Y, (int)pos.Z);// Chunk.FindIndex(pos); // FASTER WITH INTS
                            cell = this.CellGrid2[cellIndex];
                            if (cell.Block.Type == Block.Types.Air)
                                continue;
                            // redraw visible cells to prevent glitchy black lines
                            // TODO: find way to prevent glitchy black lines without redrawing blocks
                            //if (chunk.Value.VisibleOutdoorCells.ContainsKey(cellIndex)) // TODO: SLOW!!! OPTIMIZE
                            //    continue;
                            camera.DrawCell(this.VertexBuffer, this.NonOpaqueBuffer, this.TransparentBlocksVertexBuffer, this.Map, this, cell);
                            //Block.Soil.Draw(sb, screenBounds, light.Sun, light.Block, this.Zoom, cd, cell); // CURRENT WORKING ONE
                        }
                    }
            }
            if (chunkY == edgeY)// || chunk.Key.Y == this.Size.Chunks)
            {
                for (int i = 0; i < Chunk.Size; i++)
                    for (int j = 0; j < maxheight; j++)
                    {

                        //for (int j = maxheight - 1; j >= 0; j--)
                        {
                            Cell cell;
                            Vector3 pos = new Vector3(i, Chunk.Size - 1, j);
                            switch ((int)camera.Rotation)
                            {
                                case 0:
                                case 3:
                                    pos = new Vector3(i, Chunk.Size - 1, j);
                                    break;
                                case 2:
                                case 1:
                                    pos = new Vector3(i, 0, j);
                                    break;

                                default:
                                    break;
                            }
                            //var cellIndex = Chunk.FindIndex(pos);
                            var cellIndex = Chunk.FindIndex((int)pos.X, (int)pos.Y, (int)pos.Z);// Chunk.FindIndex(pos); // FASTER WITH INTS

                            cell = this.CellGrid2[cellIndex];
                            if (cell.Block.Type == Block.Types.Air)
                                continue;
                            // redraw visible cells to prevent glitchy black lines
                            // TODO: find way to prevent glitchy black lines without redrawing blocks
                            //if (chunk.Value.VisibleOutdoorCells.ContainsKey(cellIndex))
                            //    continue;

                            //camera.DrawCell(sb, this, chunk.Value, cell, playerGlobal, hiddenRects, a);
                            camera.DrawCell(this.VertexBuffer, this.NonOpaqueBuffer, this.TransparentBlocksVertexBuffer, this.Map, this, cell);

                            //Block.Soil.Draw(sb, screenBounds, light.Sun, light.Block, this.Zoom, cd, cell); // CURRENT WORKING ONE
                        }
                    }
            }
        }
        public void Build(Camera cam)
        {
            ("chunk: " + this.MapCoords.ToString() + " invalid, rebuilding mesh").ToConsole();

            this.VertexBuffer = new MySpriteBatch(Game1.Instance.GraphicsDevice);
            this.NonOpaqueBuffer = new MySpriteBatch(Game1.Instance.GraphicsDevice);
            this.TransparentBlocksVertexBuffer = new MySpriteBatch(Game1.Instance.GraphicsDevice);
            foreach(var cell in this.VisibleOutdoorCells)
            {
                cam.DrawCell(this.VertexBuffer, this.NonOpaqueBuffer, this.TransparentBlocksVertexBuffer, this.Map, this, cell.Value);
            }
            this.BuildFrontmostBlocks(cam);
            this.Valid = true;
        }

        public void DrawOpaqueLayers(Camera cam, Effect effect)
        {
            float x, y;
            Coords.Iso(cam, this.MapCoords.X * Chunk.Size, this.MapCoords.Y * Chunk.Size, 0, out x, out y);
            int rotx, roty;
            Coords.Rotate(cam, this.MapCoords.X, this.MapCoords.Y, out rotx, out roty);
           // var world = Matrix.CreateTranslation(new Vector3(x, y, ((this.MapCoords.X + this.MapCoords.Y) * Chunk.Size)));
            var world = Matrix.CreateTranslation(new Vector3(x, y, ((rotx + roty) * Chunk.Size)));

            effect.Parameters["World"].SetValue(world);

            //this.PrepareShader(map);

            effect.CurrentTechnique.Passes["Pass1"].Apply();
            EffectParameter effectMaxDrawZ = effect.Parameters["MaxDrawLevel"];
            EffectParameter effectHideWalls = effect.Parameters["HideWalls"];
            
            effectMaxDrawZ.SetValue(cam.MaxDrawZ);
            effectHideWalls.SetValue(Engine.HideWalls);
            effect.CurrentTechnique.Passes["Pass1"].Apply();
            this.VertexBuffer.Draw();

            effectHideWalls.SetValue(false);
            effect.CurrentTechnique.Passes["Pass1"].Apply();
            this.NonOpaqueBuffer.Draw();

            effectMaxDrawZ.SetValue(cam.MaxDrawZ + 1);
            effectHideWalls.SetValue(Engine.HideWalls);
            effect.CurrentTechnique.Passes["Pass1"].Apply();
            this.TopSliceMesh.Draw();

            effectHideWalls.SetValue(false);
            effect.CurrentTechnique.Passes["Pass1"].Apply();
            this.TopSliceNonOpaqueBuffer.Draw();

            foreach (var blockentity in this.BlockEntities)
                blockentity.Value.Draw(cam, this.Map, blockentity.Key.ToGlobal(this));
        }
        public void DrawTransparentLayers(Camera cam, Effect effect)
        {
            float x, y;
            Coords.Iso(cam, this.MapCoords.X * Chunk.Size, this.MapCoords.Y * Chunk.Size, 0, out x, out y);

            var world = Matrix.CreateTranslation(new Vector3(x, y, ((this.MapCoords.X + this.MapCoords.Y) * Chunk.Size)));
            effect.Parameters["World"].SetValue(world);

            //this.PrepareShader(map);

            effect.CurrentTechnique.Passes["Pass1"].Apply();
            EffectParameter effectMaxDrawZ = effect.Parameters["MaxDrawLevel"];

            effectMaxDrawZ.SetValue(cam.MaxDrawZ);
            this.TransparentBlocksVertexBuffer.Draw();
            effectMaxDrawZ.SetValue(cam.MaxDrawZ + 1);
            this.TopSliceTransparentMesh.Draw();

            //foreach (var blockentity in this.BlockEntities)
            //    blockentity.Value.Draw(cam, this.Map, blockentity.Key.ToGlobal(this));
        }
    }
}
