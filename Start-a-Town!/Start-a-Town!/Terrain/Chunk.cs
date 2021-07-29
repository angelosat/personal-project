using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Blocks;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;
using Start_a_Town_.Terraforming;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Start_a_Town_
{
    [Flags]
    public enum Edges { None = 0x0, West = 0x1, North = 0x2, East = 0x4, South = 0x8, All = 0xF }
    public class Chunk
    {
        public Chunk Clone()
        {
            Chunk chunk;
            using (BinaryWriter w = new(new MemoryStream()))
            {
                this.Write(w);
                w.BaseStream.Position = 0;
                using BinaryReader r = new(w.BaseStream);
                chunk = Chunk.Create(r);
            }
            chunk.Map = this.Map;
            return chunk;
        }

        #region Initialization
        public Dictionary<IntVec3, double> InitCells(List<Terraformer> mutators)
        {
            var gradientCache = new Dictionary<IntVec3, double>();
            var world = this.Map.World;
            int n = 0; ;
            var grad = new GradientLowRes(this.World, this);
            mutators.ForEach(m => m.SetWorld(this.World));

            for (int z = 0; z < MapBase.MaxHeight; z++)
                for (int i = 0; i < Size; i++)
                    for (int j = 0; j < Size; j++)
                    {
                        Cell cell = new(i, j, z);
                        double gradient = grad.GetGradient(i, j, z);
                        gradientCache.Add(new IntVec3(i, j, z), gradient);
                        foreach (var m in mutators)
                            m.Initialize(world, cell, (int)this.Start.X + i, (int)this.Start.Y + j, z, gradient);
                        this.CellGrid2[n++] = cell;
                    }
            return gradientCache;
        }
        public Dictionary<IntVec3, double> InitCells2(List<Terraformer> mutators)
        {
            var gradientCache = new Dictionary<IntVec3, double>();
            int n = 0; ;
            var grad = new GradientLowRes(this.World, this);
            var maxh = MapBase.MaxHeight;
            for (int z = 0; z < maxh; z++)
                for (int i = 0; i < Size; i++)
                    for (int j = 0; j < Size; j++)
                    {
                        Cell cell = new(i, j, z);
                        double gradient = grad.GetGradient(i, j, z);
                        gradientCache.Add(new IntVec3(i, j, z), gradient);
                        this.CellGrid2[n++] = cell;
                    }
            return gradientCache;
        }
        public void InitCells3(Terraformer m, Dictionary<IntVec3, double> gradient)
        {
            var maxh = MapBase.MaxHeight;
            int n = 0;
            for (int z = 0; z < maxh; z++)
                for (int i = 0; i < Size; i++)
                    for (int j = 0; j < Size; j++)
                    {
                        var cell = this.CellGrid2[n++];
                        m.Initialize(this.Map.World, cell, (int)this.Start.X + i, (int)this.Start.Y + j, z, gradient[new IntVec3(i, j, z)]);
                    }
            this.UpdateHeightMap();

        }
        public Chunk InitCells()
        {
            int n = 0;
            for (int z = 0; z < MapBase.MaxHeight; z++)
                for (int i = 0; i < Size; i++)
                    for (int j = 0; j < Size; j++)
                    {
                        Cell cell = new(i, j, z);
                        this.CellGrid2[n++] = cell;
                    }
            return this;
        }
        #endregion

        public override string ToString()
        {
            string text =
                "Local: " + this.MapCoords.ToString() +
                  "\nGlobal: " + this.Start.ToString() +
                   "\nObjects: " + this.Objects.Count +
                "\nCells to validate: " + this.CellsToValidate.Count;

            text += "Objects: " + this.Objects.Count.ToString() + "\n";
            return text.Remove(text.Length - 1);
        }

        List<IntVec3> _RandomOrderedCells;
        List<IntVec3> RandomOrderedCells
        {
            get
            {
                if (this._RandomOrderedCells is null)
                {
                    var allPositions = new BoundingBox(IntVec3.Zero, new IntVec3(Chunk.Size - 1, Chunk.Size - 1, MapBase.MaxHeight - 1)).GetBoxIntVec3();
                    this._RandomOrderedCells = allPositions.Randomize(this.Map.Random);
                }
                return this._RandomOrderedCells;
            }
        }
        public IntVec3 GetRandomCellInOrder(int index)
        {
            if (index >= this.CellGrid2.Length)
                throw new Exception();
            return this.RandomOrderedCells[index];
        }

        public Cell[] CellGrid2;

        public void CopyFrom(Chunk chunk)
        {
            this.Objects = chunk.Objects;
            this.CellGrid2 = chunk.CellGrid2;
            this.HeightMap = chunk.HeightMap;
            this.Sunlight = chunk.Sunlight;
            this.BlockLight = chunk.BlockLight;
        }

        public List<GameObject> Objects;
        readonly Dictionary<int, GameObject> BlockObjects;
        readonly Dictionary<IntVec3, BlockEntity> BlockEntitiesByPosition = new();

        public bool IsQueuedForLight;
        public const int Size = 16;
        public SortedList<int, Cell> VisibleIndoorCells;

        public Vector2 Start, bottomRight;
        public void Invalidate()
        {
            foreach (var slice in this.Slices)
                if (slice != null)
                    slice.Valid = false;
            this.Valid = false;
        }
        public void InvalidateMesh()
        {
            this.Valid = false;
        }

        public int X, Y;
        public int RectHeight;
        public MapBase Map;
        public IWorld World => this.Map.World;
        public bool Valid;
        readonly Queue<Cell> CellsToValidate = new Queue<Cell>();

        public bool ChunkBoundariesUpdated = true;
        public bool LightValid = false;
        public bool EdgesValid = false;
        public void InvalidateEdges()
        {
            this.EdgesValid = false;
        }

        #region Public Properties
        public Cell this[int localx, int localy, int localz]
        {
            get
            {
                if (localx < 0 || localx > Chunk.Size - 1 || localy < 0 || localy > Chunk.Size - 1 || localz < 0 || localz > MapBase.MaxHeight - 1)
                    return null;

                int ind = GetCellIndex(localx, localy, localz);
                return this.CellGrid2[ind];
            }
        }
        public Cell this[float localx, float localy, float localz]
        {
            get
            {
                if (localx < 0 || localx > Chunk.Size - 1 || localy < 0 || localy > Chunk.Size - 1 || localz < 0 || localz > MapBase.MaxHeight - 1)
                    return null;

                int ind = GetCellIndex(localx, localy, localz);
                return this.CellGrid2[ind];
            }
        }
        public Cell this[IntVec3 localCoords]
        {
            get
            {
                if (!localCoords.IsWithinChunkBounds())
                    return null;

                return this.CellGrid2[GetCellIndex(localCoords)];
            }
        }
        public Cell this[int cellIndex] => this.CellGrid2[cellIndex];

        public Vector2 MapCoords
        {
            get => new Vector2(this.X, this.Y);
            set
            {
                this.X = (int)value.X;
                this.Y = (int)value.Y;
                this.Start = this.MapCoords * Chunk.Size;
            }
        }

        public static readonly int Width = Block.Width * Size;
        public static readonly int Height = MapBase.MaxHeight * Block.BlockHeight + Chunk.Size * Block.Depth;
        public static readonly Rectangle Bounds = new(-Width / 2, -Height / 2, Width, Height);

        public Rectangle GetScreenBounds(Camera cam)
        {
            Rectangle chunkBounds = cam.GetScreenBounds(this.Start.X + Chunk.Size / 2, this.Start.Y + Chunk.Size / 2, MapBase.MaxHeight / 2, Bounds);  //chunk.Value.GetBounds(camera);
            return chunkBounds;
        }
        #endregion

        public bool TryGetBlockObject(Vector3 local, out GameObject blockObj)
        {
            return this.BlockObjects.TryGetValue(GetCellIndex(local), out blockObj);
        }
        public Chunk(MapBase map, Vector2 pos)
            : this()
        {
            this.Map = map;
            this.MapCoords = pos;
            this.InitCells();
        }
        Chunk(Vector2 pos)
            : this()
        {
            this.MapCoords = pos;
        }
        Chunk()
        {
            this.CellGrid2 = new Cell[Chunk.Size * Chunk.Size * MapBase.MaxHeight];
            this.VisibleIndoorCells = new SortedList<int, Cell>();
            this.Objects = new List<GameObject>();
            this.BlockObjects = new Dictionary<int, GameObject>();
            this.Sunlight = new List<byte>(Volume);
            for (int i = 0; i < Volume; i++)
                this.Sunlight.Add(15);
            this.HeightMap = new int[Size][];
            for (int i = 0; i < Size; i++)
                this.HeightMap[i] = new int[Size];
            this.ResetCellLight();
            for (int i = 0; i < MapBase.MaxHeight; i++)
                this.Slices[i] = new Slice();
        }
        public static Chunk Create(MapBase map, Vector2 pos)
        {
            Chunk chunk = new(pos);
            chunk.Map = map;
            return chunk;
        }
        public static Chunk Create(MapBase map, int x, int y)
        {
            Chunk chunk = new(new Vector2(x, y));
            chunk.Map = map;
            return chunk;
        }
        public static Chunk Load(MapBase map, Vector2 key, SaveTag tag)
        {
            return new Chunk(map, key).LoadFromTag(tag);
        }

        public void Add(GameObject obj)
        {
            obj.Map = this.Map;
            if (this.Objects.Contains(obj))
                throw new Exception();
            this.Objects.Add(obj);
        }
        public bool Remove(GameObject obj)
        {
            if (!this.Objects.Remove(obj))
                throw new Exception();
            return true;
        }

        #region Dunno
        public Cell GetLocalCell(int x, int y, int z)
        {
            return this.CellGrid2[GetCellIndex(x, y, z)];
        }
        public static int GetCellIndex(int x, int y, int z)
        { return (z * Chunk.Size + x) * Chunk.Size + y; }
        public static int GetCellIndex(float x, float y, float z)
        { return (int)((Math.Round(z) * Chunk.Size + Math.Round(x)) * Chunk.Size + Math.Round(y)); }
        public static int GetCellIndex(IntVec3 local)
        {
            return (local.Z * Size + local.X) * Size + local.Y;
        }
        public static int Volume = Size * Size * MapBase.MaxHeight;
        public List<byte> Sunlight;
        public byte[] BlockLight = new byte[Volume];

        void ResetCellLight()
        {
            this.BlockLight = new byte[Volume];
        }

        public int[][] HeightMap;

        public int GetHeightMapValue(Vector3 local)
        {
            return this.GetHeightMapValue((int)local.X, (int)local.Y);
        }
        public int GetHeightMapValue(int localx, int localy)
        {
            return this.HeightMap[localx][localy];
        }
        public bool IsAboveHeightMap(Vector3 local)
        {
            return local.Z > this.HeightMap[(int)local.X][(int)local.Y];
        }
        public bool IsAboveHeightMap(int localx, int localy, int localz)
        {
            return localz > this.HeightMap[localx][localy];
        }

        Queue<IntVec3> LightChanges = new Queue<IntVec3>();

        /// <summary>
        /// Recalculates the skylight of a chunk and returns a list of cells whose skylight that changed.
        /// </summary>
        /// <returns>A list of cells whose skylight has changed</returns>
        public Queue<IntVec3> ResetHeightMap()
        {
            for (int j = 0; j < Size; j++)
                for (int i = 0; i < Size; i++)
                    foreach (var pos in this.ResetHeightMapColumn(i, j))
                        this.LightChanges.Enqueue(pos);
            var toReturn = new Queue<IntVec3>(this.LightChanges);
            this.LightChanges = new Queue<IntVec3>();
            return toReturn;
        }

        public void UpdateHeightMap()
        {
            for (int j = 0; j < Size; j++)
                for (int i = 0; i < Size; i++)
                    this.UpdateHeightMapColumn(i, j, false);
        }
        public void InvalidateHeightmap(int localx, int localy)
        {
            // invalidate heightmap immediately?
            // TODO: invalidate coordinates and update heightmap at the next tick, so as to prevent updating the same column multiple times in case of multiple block changes
            this.UpdateHeightMapColumnWithLightSmart(localx, localy);
        }
        HashSet<IntVec2> HeightMapUpdates = new();

        /// <summary>
        /// the current ont
        /// </summary>
        /// <param name="localx"></param>
        /// <param name="localy"></param>
        public void UpdateHeightMapColumnWithLightSmart(int localx, int localy)
        {
            int z;
            Cell cell;
            z = MapBase.MaxHeight - 1;
            bool found = false;
            bool hit = false;
            var oldValue = this.HeightMap[localx][localy];
            int minVal = 0, maxVal = this.Map.GetMaxHeight();
            while (z >= 0)
            {
                cell = this.GetLocalCell(localx, localy, z);
                if (!hit)
                    if (cell.Block.Type != Block.Types.Air)
                    {
                        hit = true;
                    }
                if (cell.Opaque)
                {
                    if (!found)
                    {
                        found = true;
                        int newValue = z;
                        this.HeightMap[localx][localy] = newValue;
                        if (newValue > oldValue)
                        {
                            minVal = oldValue;
                            maxVal = newValue;
                        }
                        else if (newValue < oldValue)
                        {
                            minVal = newValue;
                            maxVal = oldValue;
                        }
                        else return; // new heightmap value is same as previous one so return
                    }
                }
                if (found && (minVal < z && z <= maxVal)) // if a new heightmap value found, invalidate cells inbetween the old and the new one
                {
                    this.InvalidateCell(cell); // why did i have this commented out? it caused slice meshes not getting updated light
                }
                z--;
            }

            if (!found)
                this.HeightMap[localx][localy] = 0;
        }

        public void UpdateHeightMapColumn(int localx, int localy, bool invalidate = true)
        {
            int z;
            byte light;
            Cell cell;
            light = 15;
            z = MapBase.MaxHeight - 1;
            bool hit = false;
            while (z >= 0)
            {
                cell = this.GetLocalCell(localx, localy, z);
                if (!hit)
                    if (cell.Block.Type != Block.Types.Air)
                    {
                        hit = true;
                    }
                if (cell.Opaque)
                {
                    if (light > 0)
                    {
                        this.HeightMap[localx][localy] = z;
                        light = 0;
                    }
                }
                this.SetSunlight(localx, localy, z, light);
                if (invalidate)
                    this.InvalidateCell(cell);
                z--;
            }

            if (light > 0)
                this.HeightMap[localx][localy] = z;
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
                cell = this.GetLocalCell(localx, localy, z);
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
                        this.HeightMap[localx][localy] = z;
                        light = 0;
                    }
                }

                this.SetSunlight(localx, localy, z, light);
                if (z <= firstContact)
                    lightsourcesToHandle.Enqueue(cell.LocalCoords.ToGlobal(this));
                z--;
            }

            if (light > 0)
                this.HeightMap[localx][localy] = z;
            return lightsourcesToHandle;
        }
        #endregion

        public void ValidateCells()
        {
            if (this.CellsToValidate.Any())
            {
                while (this.CellsToValidate.Count > 0)
                {
                    Cell cell = this.CellsToValidate.Dequeue();
                    this.Map.LightingEngine.HandleImmediate(new IntVec3[] { cell.LocalCoords.ToGlobal(this) });
                    cell.Valid = true;
                    this.InvalidateSlice(cell.Z);
                    this.InvalidateMesh();
                }
            }
        }

        public void InvalidateSlice(byte z)
        {
            this.Slices[z].Valid = false;
            this.InvalidateMesh();
        }
        public void InvalidateSlice(int z)
        {
            this.InvalidateSlice((byte)z);
        }

        public bool InvalidateCell(Cell cell)
        {
            if (cell is null)
                throw new Exception();
            this.InvalidateLight(cell);

            if (!cell.Valid)
                return false;

            this.CellsToValidate.Enqueue(cell);
            cell.Valid = false;
            return true;
        }

        public byte GetBlockLight(IntVec3 local)
        {
            return this.GetBlockLight(local.X, local.Y, local.Z);
        }
        public byte GetBlockLight(int x, int y, int z)
        {
            int index = GetCellIndex(x, y, z);
            byte l = this.BlockLight[index];
            return l;
        }

        public byte GetSunlight(IntVec3 local)
        {
            return this.GetSunlight(local.X, local.Y, local.Z);
        }
        public byte GetSunlight(int x, int y, int z)
        {
            if (z >= this.Map.GetMaxHeight())
                return 15;
            int index = GetCellIndex(x, y, z);
            return this.Sunlight[index];
        }

        public void SetSunlight(IntVec3 local, byte value)
        {
            this.SetSunlight(local.X, local.Y, local.Z, value);
        }
        public void SetSunlight(int x, int y, int z, byte value)
        {
            this.Sunlight[GetCellIndex(x, y, z)] = value;
            var global = new IntVec3(this.Start.X + x, this.Start.Y + y, z);
            this.InvalidateLight(global);
        }

        public void SetBlockLight(IntVec3 local, byte value)
        {
            var index = GetCellIndex(local);
            this.BlockLight[index] = value;
            var global = local + new IntVec3(this.Start.X, this.Start.Y, 0);
            this.InvalidateLight(global);
        }

        /// <summary>
        /// TODO: optimize: convert to dictionary for speed
        /// </summary>
        public Dictionary<IntVec3, LightToken> LightCache2 = new();

        public static bool InvalidateLight(MapBase map, IntVec3 global)
        {
            if (map.TryGetAll(global.X, global.Y, global.Z, out Chunk chunk, out Cell cell, out int lx, out int ly))
            {
                return chunk.LightCache2.Remove(global);
            }
            return false;
        }
        public bool InvalidateLight(Cell cell)
        {
            //return this.LightCache2.Remove(cell.GetGlobalCoords(this));
            return this.InvalidateLight(cell.GetGlobalCoords(this));
        }
        public bool InvalidateLight(IntVec3 global)
        {
            this.LightCache2.Clear();
            if (this.Slices.Any())
            {
                var z = global.Z;
                if (z > 0)
                    this.InvalidateSlice(z - 1);
                this.InvalidateSlice(z);
                if (z < this.Map.GetMaxHeight() - 1)
                    this.InvalidateSlice(z + 1);
            }
            return true;
        }

        public static bool TryGetFinalLight(MapBase map, int globalX, int globalY, int globalZ, out byte sky, out byte block)
        {
            sky = 0;
            block = 0;
            if (globalZ > MapBase.MaxHeight - 1)
                return false;
            if (globalZ < 0)
                return false;

            var global = new IntVec3(globalX, globalY, globalZ);
            if (!map.TryGetChunk(global, out Chunk chunk))
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

        public static bool TryGetSunlight(MapBase map, IntVec3 global, out byte sunlight)
        {
            sunlight = 0;

            if (global.Z > map.GetMaxHeight() - 1)
                return false;
            if (global.Z < 0)
                return false;

            if (!map.TryGetChunk(global, out var chunk))
                return false;

            int x = (int)(global.X - chunk.Start.X);
            int y = (int)(global.Y - chunk.Start.Y);
            sunlight = chunk.GetSunlight(x, y, global.Z);
            return true;
        }

        #region Updating
        public void Update()
        {
            this.ValidateHeightmap();
            this.ValidateCells();
        }

        public void HitTestEntities(Camera camera)
        {
            foreach (var o in this.Objects)
                o.HitTest(camera);
        }

        private void ValidateHeightmap()
        {
            if (this.HeightMapUpdates.Any())
            {
                foreach (var pos in this.HeightMapUpdates)
                    this.UpdateHeightMapColumnWithLightSmart(pos.X, pos.Y);
                this.HeightMapUpdates = new();
            }
        }
        public void Tick()
        {
            this.UpdateEntities();
            this.UpdateBlockEntities();
        }

        private void UpdateBlockEntities()
        {
            foreach (var blockentity in this.BlockEntitiesByPosition.ToList())
                blockentity.Value.Tick(this.Map, blockentity.Key.ToGlobal(this));
        }
        private void UpdateEntities()
        {
            var objectList = this.Objects.ToArray();
            var objCount = objectList.Length;
            for (int i = 0; i < objCount; i++)
            {
                objectList[i].Update();
            }
        }

        public void UpdateSkyLight()
        {
            var items = new Queue<IntVec3>();
            for (int i = 0; i < Size; i++)
            {
                for (int j = 0; j < Size; j++)
                {
                    var h = this.GetHeightMapValue(i, j);
                    for (int z = 0; z < h; z++)
                        items.Enqueue(new IntVec3(i, j, z).ToGlobal(this));
                }
            }
            this.Map.UpdateLight(items);
        }
        #endregion

        #region Drawing
        public void DrawObjects(MySpriteBatch sb, Camera camera, Controller controller, MapBase map, SceneState scene)
        {
            foreach (GameObject obj in this.Objects) //make a copy of the list first because currently the player character might be added while drawing
            {
                Vector3 global = obj.Global;
                if (global.Z > camera.DrawLevel + 1)// - 1)
                    continue;
                var actor = map.Net.GetPlayer().ControllingEntity;
                if (camera.HideTerrainAbovePlayer && actor is not null)
                    if (global.Z > actor.Transform.Global.Z + 2)// - 1)
                        continue;

                if (!map.TryGetCell(global, out Cell cell))
                    continue;
                float x = cell.X, y = cell.Y, z = global.Z;
                // TODO: figure out a way to get depth from actual precise global coords instead of cell coords
                Coords.Rotate(camera, x, y, out float rx, out float ry);
                Vector3 rotated = new(rx, ry, z);

                if (!obj.Components.ContainsKey("Sprite"))
                    continue;

                Sprite sprite = obj.GetComponent<SpriteComponent>().Sprite;
                Rectangle spriteBounds = sprite.GetBounds();
                Rectangle screenBounds = camera.GetScreenBounds(global, spriteBounds);
                screenBounds.X -= Graphics.Borders.Thickness;
                screenBounds.Y -= Graphics.Borders.Thickness;
                if (!camera.ViewPort.Intersects(screenBounds))
                    continue;
                float cd = global.GetDrawDepth(map, camera);

                byte light = Math.Max((byte)(this.GetSunlight(cell.LocalCoords) - map.GetSkyDarkness()), this.GetBlockLight(cell.LocalCoords));
                float l = (light + 1) / 16f;
                Color color = new Color(l, l, l, 1);
                Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));

                obj.Draw(sb, new DrawObjectArgs(camera, controller, map, this, cell, spriteBounds, screenBounds, obj, color, cd));
                SpriteComponent.DrawShadow(camera, spriteBounds, map, obj, cd, cd);

                if (scene.ObjectsDrawn.Contains(obj))
                    throw new Exception();
                scene.ObjectsDrawn.Add(obj);
                scene.ObjectBounds.Add(obj, screenBounds);
            }
        }
        public void DrawInterface(SpriteBatch sb, Camera cam)
        {
            foreach (GameObject obj in this.Objects.ToList().Concat(this.BlockObjects.Values.ToList()))
                obj.DrawInterface(sb, cam);
            foreach (var blockentity in this.BlockEntitiesByPosition)
                blockentity.Value.DrawUI(sb, cam, blockentity.Key.ToGlobal(this));
        }
        public void DrawHighlight(SpriteBatch sb, Rectangle bounds)
        {
            sb.Draw(UI.UIManager.Highlight, bounds, null, Color.Lerp(Color.White, Color.Transparent, 0.5f), 0, Vector2.Zero, SpriteEffects.None, 0);
        }
        #endregion

        #region Saving and Loading
        public string GetDirectoryPath()
        {
            return this.Map.GetFullPath() + "/chunks/" + this.DirectoryName;
        }
        internal void SaveToFile()
        {
            Chunk copy = this.Clone();
            string filename = GetFilename(this.MapCoords);
            string newFile = "_" + filename;

            string directory = this.GetDirectoryPath();
            directory = @"/Saves/Worlds/" + this.Map.World.GetName() + "/" + this.Map.GetFolderName() + "/chunks/";

            string working = Directory.GetCurrentDirectory();
            string fullpath = this.Map.GetFullPath() + "/chunks/" + this.DirectoryName;

            if (!Directory.Exists(fullpath))
                Directory.CreateDirectory(fullpath);
            copy.SaveToFile(newFile);
            if (File.Exists(fullpath + filename))
                try
                {
                    File.Replace(fullpath + newFile, fullpath + filename, fullpath + filename + ".bak");
                    File.Delete(fullpath + filename + ".bak");
                }
                catch (IOException)
                {
                    Net.Server.Instance.ConsoleBox.Write(Color.Red, "SERVER", "Error saving Chunk " + copy.MapCoords.ToString());
                    // recover back up here?
                }
            else
                File.Move(fullpath + newFile, fullpath + filename);

            Net.Server.Instance.ConsoleBox.Write(Color.Lime, "SERVER", "Chunk " + copy.MapCoords.ToString() + " saved succesfully \"" + directory + filename + "\"");
        }
        internal string SaveToFile(string filename)
        {
            string directory = this.FullDirPath;
            DateTime now = DateTime.Now;
            SaveTag chunktag;
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                chunktag = this.SaveToTag();
                chunktag.WriteTo(writer);
                Compress(stream, directory + filename);
                stream.Close();
            }
            Console.WriteLine(filename + " saved in " + (DateTime.Now - now).ToString());
            return directory + GetFilename(this.MapCoords);
        }
        private void SaveCellsToTagCompressed(SaveTag chunktag)
        {
            SaveTag cellstag = new(SaveTag.Types.List, "Cells", SaveTag.Types.Compound);
            var airLength = 0;
            bool airIsDiscovered = false;
            foreach (var cell in this.CellGrid2)
            {
                if (cell.Block == BlockDefOf.Air)
                {
                    airLength++;
                    airIsDiscovered = cell.Discovered;
                    continue;
                }
                // TODO when the last cell in the cell array is air, the air savetag isn't written
                if (airLength > 0)
                {
                    saveAirTag(cellstag, airLength, airIsDiscovered);
                    airLength = 0;
                }

                cellstag.Add(cell.Save());
            }
            // TODO when the last cell in the cell array is air, the air savetag isn't written
            if (airLength > 0)
                saveAirTag(cellstag, airLength, airIsDiscovered);

            chunktag.Add(cellstag);

            static void saveAirTag(SaveTag cellstag, int airLength, bool airIsDiscovered)
            {
                var airtag = new SaveTag(SaveTag.Types.Compound);
                airtag.Add(new SaveTag(SaveTag.Types.Byte, "Tile", (byte)BlockDefOf.Air.Type));
                airtag.Add(new SaveTag(SaveTag.Types.Int, "Data", airLength));
                airtag.Add(new SaveTag(SaveTag.Types.Bool, "Discovered", airIsDiscovered));
                cellstag.Add(airtag);
            }
        }

        private Dictionary<BlockEntity, List<Vector3>> GetDistinctBlockEntities()
        {
            var distinct = new Dictionary<BlockEntity, List<Vector3>>();
            foreach (var ent in this.BlockEntitiesByPosition)
            {
                if (!distinct.TryGetValue(ent.Value, out var existing))
                {
                    existing = new List<Vector3>();
                    distinct.Add(ent.Value, existing);
                }
                existing.Add(ent.Key);
            }
            return distinct;
        }

        public static Chunk Load(MapBase map, string fullpath)
        {
            string filename = fullpath.Split('\\').Last();
            string[] c = filename.Split('.');
            var coords = new Vector2(Convert.ToInt32(c[0]), Convert.ToInt32(c[1]));
            var chunk = new Chunk(coords)
            {
                Map = map
            };
            using (FileStream stream = new FileStream(fullpath, FileMode.Open))
            {
                var buffer = DecompressAll(stream);
                using MemoryStream decompressedStream = new(buffer);
                using BinaryReader reader = new(decompressedStream);
                SaveTag chunktag = SaveTag.Read(reader);
                chunk.LoadFromTag(chunktag);
                reader.Close();
            }
            return chunk;
        }

        private SaveTag SaveBlockEntitiesDistinct()
        {
            var blockEntitiesTag = new SaveTag(SaveTag.Types.List, "BlockEntities", SaveTag.Types.Compound);
            var distinct = this.GetDistinctBlockEntities();
            foreach (var ent in distinct)
            {
                var tag = new SaveTag(SaveTag.Types.Compound, "");
                var origin = ent.Key.OriginGlobal;
                origin.Save(tag, "OriginGlobal");

                if (this.Contains(origin)) // ONLY SAVE BLOCKENTITY IF THE ORIGIN IS IN THIS CHUNK
                {
                    var entitysavetag = ent.Key.Save("Entity");
                    if (entitysavetag is not null)
                        tag.Add(entitysavetag);
                }
                else
                    tag.Add(ent.Value.Save("PositionsLocal")); // all local positions where the entity is occupying (NOT INCLUDING POSITIONS IN NEIGHBORING CHUNKS)
                blockEntitiesTag.Add(tag); // the block entity is saved ONCE in the chunk the origin is contained, and all occupied cells are saved with it (global positions)
                                           // secondary blockentity positions save only the global origin position and retrieve the blockentity on chunk load,
                                           // or if the origin chunk hasn't loaded yet, when it loads it registers the blockentity using the saved occupiedcells in the blockentity class
            }
            return blockEntitiesTag;
        }
        private void LoadBlockEntitiesDistinct(SaveTag chunktag)
        {
            if (chunktag.TryGetTag("BlockEntities", out var blentitiesjTag))
                foreach (SaveTag tag in blentitiesjTag.Value as List<SaveTag>)
                {
                    var origin = tag.LoadIntVec3("OriginGlobal");

                    if (this.Contains(origin))
                    {
                        var entity = this[origin.ToLocal()].Block.CreateBlockEntity();

                        tag.TryGetTag("Entity", t => entity.Load(t));

                        foreach (var global in entity.CellsOccupied)
                        {
                            if (this.Contains(global))
                                this.AddBlockEntity(entity, global.ToLocal()); // TODO add chunk in map before finishing loading??
                            else
                            {
                                if (this.Map.TryGetChunk(global, out var nchunk))
                                    nchunk.AddBlockEntity(entity, global.ToLocal());
                            }
                        }
                    }
                    else
                    {
                        var positions = tag["PositionsLocal"].LoadListVector3();

                        if (this.Map.TryGetBlockEntity(origin, out var entity))
                        {
                            foreach (var local in positions)
                                this.BlockEntitiesByPosition[local] = entity;
                        }
                    }
                }
        }
        private void WriteBlockEntitiesDistinct(BinaryWriter w)
        {
            var distinct = this.GetDistinctBlockEntities();
            w.Write(distinct.Count);
            foreach (var ent in distinct)
            {
                var entity = ent.Key;
                w.Write(entity.OriginGlobal);
                if (this.Contains(entity.OriginGlobal))
                {
                    w.Write(ent.Key.GetType().FullName);
                    ent.Key.Write(w);
                }
                else
                {
                    w.Write(ent.Value); // if this chunk doesnt contain the blockentity origin, only write the local cells that the blockentity appears in
                }
            }
        }
        private void ReadBlockEntitiesDistinct(BinaryReader r)
        {
            int blockEntityCount = r.ReadInt32();
            for (int i = 0; i < blockEntityCount; i++)
            {
                var originGlobal = r.ReadIntVec3();
                if (this.Contains(originGlobal))
                {
                    var str = r.ReadString();
                    var entity = Activator.CreateInstance(Type.GetType(str)) as BlockEntity;
                    entity.Read(r);
                    foreach (var global in entity.CellsOccupied)
                    {
                        if (this.Contains(global))
                            this.AddBlockEntity(entity, global.ToLocal());
                        else
                        {
                            if (this.Map.TryGetChunk(global, out var nchunk))
                                nchunk.AddBlockEntity(entity, global.ToLocal());
                        }
                    }
                }
                else
                {
                    var positionsLocal = r.ReadListVector3();

                    if (this.Map.TryGetBlockEntity(originGlobal, out var entity))
                        foreach (var local in positionsLocal)
                            this.BlockEntitiesByPosition[local] = entity;
                }
            }
        }

        public static void Compress(Stream stream, string filename)
        {
            using (stream)
            {
                stream.Position = 0;
                using FileStream outFile = File.Create(filename);
                using GZipStream zip = new(outFile, CompressionMode.Compress);
                stream.CopyTo(zip);
            }
        }
        public static MemoryStream Decompress(FileStream compressed)
        {
            using (compressed)
            {
                using GZipStream decompress = new(compressed, CompressionMode.Decompress);
                MemoryStream memory = new MemoryStream();
                decompress.CopyTo(memory);
                memory.Position = 0;
                return memory;
            }
        }
        public static byte[] DecompressAll(FileStream compressed)
        {
            byte[] buffer;
            using (GZipStream decompress = new(compressed, CompressionMode.Decompress))
            {
                using MemoryStream memory = new();
                decompress.CopyTo(memory);
                memory.Position = 0;
                buffer = new byte[memory.Length];
                memory.Read(buffer, 0, buffer.Length);
            }
            return buffer;
        }
        public static string GetFilename(Vector2 pos)
        {
            return pos.X.ToString() + "." + pos.Y.ToString() + ".chunk.sat";
        }
        public static string GetDirName(Vector2 pos)
        {
            return pos.X.ToString() + "." + pos.Y.ToString() + "/";
        }
        #endregion
        public bool TryGetCell(Vector3 local, out Cell cell)
        {
            cell = this[local];
            if (cell == null)
                return this.Map.TryGetCell(local.ToGlobal(this), out cell);
            return true;
        }
        public Cell GetCellLocal(Vector3 local)
        {
            return this.CellGrid2[GetCellIndex(local)];
        }

        public List<IntVec3> GetEdges(Edges edges)
        {
            var list = new HashSet<IntVec3>();
            if ((edges & Edges.East) == Edges.East)
                for (int i = 0; i < Chunk.Size; i++)
                    for (int z = 0; z < MapBase.MaxHeight; z++)
                        list.Add(new IntVec3(this.Start.X + Chunk.Size - 1, this.Start.Y + i, z));

            if ((edges & Edges.West) == Edges.West)
                for (int i = 0; i < Chunk.Size; i++)
                    for (int z = 0; z < MapBase.MaxHeight; z++)
                        list.Add(new IntVec3(this.Start.X, this.Start.Y + i, z));

            if ((edges & Edges.North) == Edges.North)
                for (int i = 0; i < Chunk.Size; i++)
                    for (int z = 0; z < MapBase.MaxHeight; z++)
                        list.Add(new IntVec3(this.Start.X + i, this.Start.Y, z));

            if ((edges & Edges.South) == Edges.South)
                for (int i = 0; i < Chunk.Size; i++)
                    for (int z = 0; z < MapBase.MaxHeight; z++)
                        list.Add(new IntVec3(this.Start.X + i, this.Start.Y + Chunk.Size - 1, z));

            return list.ToList();
        }
        internal IEnumerable<GameObject> GetObjectsLazy()
        {
            foreach (var obj in this.Objects)
                yield return obj;
        }
        internal List<GameObject> GetObjects()
        {
            return new List<GameObject>(this.Objects);
        }

        public void OnCameraRotated(Camera camera)
        {
            this.LightCache2.Clear();
        }

        void WriteCells(BinaryWriter writer)
        {
            var w = writer;
            int consecutiveAirblocks = 0;
            bool lastDiscovered = false;
            foreach (var cell in this.CellGrid2)
            {
                if (cell.Block.Type == Block.Types.Air)
                {
                    consecutiveAirblocks++;
                    lastDiscovered = cell.Discovered;
                    continue;
                }
                if (consecutiveAirblocks > 0)
                {
                    // write air block length
                    writeAir(w, consecutiveAirblocks, lastDiscovered);
                    consecutiveAirblocks = 0;
                }
                w.Write((int)cell.Block.Type);
                w.Write(cell.Data.Data);
                w.Write(cell.Discovered);
            }
            if (consecutiveAirblocks > 0)
                writeAir(w, consecutiveAirblocks, lastDiscovered);

            w.Write(-1);

            static void writeAir(BinaryWriter w, int consecutiveAirblocks, bool lastDiscovered)
            {
                w.Write(0);
                w.Write(consecutiveAirblocks);
                w.Write(lastDiscovered); // because all consecutive air blocks are either all discovered or none is
            }
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
                    bool discovered = r.ReadBoolean();
                    for (int j = 0; j < consecutiveAirblocks; j++)
                    {
                        var c = this.CellGrid2[cellIndex++];
                        c.SetBlockType(Block.Types.Air);
                        c.Discovered = discovered;
                    }
                }
                else if (type > 0)
                {
                    var cell = this.CellGrid2[cellIndex++];
                    cell.SetBlockType(type);

                    cell.Data = new BitVector32(r.ReadInt32());
                    cell.Discovered = r.ReadBoolean();
                }
            } while (type > -1);
        }
        #region Serialization
        public void Write(BinaryWriter writer)
        {
            writer.Write(this.MapCoords);
            writer.Write(this.LightValid);
            writer.Write(this.EdgesValid);

            this.WriteCells(writer);

            writer.Write(this.Objects.Count);
            foreach (var obj in this.Objects.ToList())
                obj.Write(writer);

            writer.Write(this.BlockObjects.Count);
            foreach (var obj in this.BlockObjects.Values.ToList())
                obj.Write(writer);

            this.WriteBlockEntitiesDistinct(writer);

            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    writer.Write(this.HeightMap[i][j]);

            writer.Write(this.Sunlight.ToArray());
            writer.Write(this.BlockLight.ToArray());
        }

        public static Chunk Create(MapBase map, BinaryReader reader)
        {
            var chunk = new Chunk() { Map = map };
            chunk.Read(reader);
            return chunk;
        }
        public static Chunk Create(BinaryReader reader)
        {
            Chunk chunk = new();
            chunk.Read(reader);
            return chunk;
        }
        void Read(BinaryReader reader)
        {
            this.MapCoords = reader.ReadVector2();

            this.LightValid = reader.ReadBoolean();
            this.EdgesValid = reader.ReadBoolean();

            // TODO: OPTIMIZE
            this.InitCells();
            this.ReadCells(reader);

            int objCount = reader.ReadInt32();
            for (int i = 0; i < objCount; i++)
                this.Objects.Add(GameObject.Create(reader));

            int blockObjCount = reader.ReadInt32();
            for (int i = 0; i < blockObjCount; i++)
            {
                GameObject blobj = GameObject.Create(reader);
                this.BlockObjects[GetCellIndex(blobj.Global.ToLocal())] = blobj;
            }

            this.ReadBlockEntitiesDistinct(reader);

            for (int i = 0; i < Size; i++)
                for (int j = 0; j < Size; j++)
                    this.HeightMap[i][j] = reader.ReadInt32();

            this.Sunlight = reader.ReadBytes(Volume).ToList();


            this.BlockLight = reader.ReadBytes(Volume);
        }
        #endregion

        public string FullDirPath => this.Map.GetFullPath() + "/chunks/" + this.DirectoryName;

        public string DirectoryName => (((int)(this.MapCoords.X)).ToString() + "." + ((int)(this.MapCoords.Y)).ToString()) + "/";

        public Canvas Canvas, CanvasTopSlice;

        public void Build(Camera cam)
        {
            this.ValidateSlices(cam);
            this.Valid = true;
        }

        public void DrawOpaqueLayers(Camera cam, Effect effect)
        {
            Coords.Iso(cam, this.MapCoords.X * Chunk.Size, this.MapCoords.Y * Chunk.Size, 0, out float x, out float y);
            Coords.Rotate(cam, this.MapCoords.X, this.MapCoords.Y, out int rotx, out int roty);
            var world = Matrix.CreateTranslation(new Vector3(x, y, ((rotx + roty) * Chunk.Size)));
            effect.Parameters["World"].SetValue(world);
            effect.CurrentTechnique.Passes["Pass1"].Apply();
            EffectParameter effectHideWalls = effect.Parameters["HideWalls"];
            effectHideWalls.SetValue(Engine.HideWalls);
            effect.CurrentTechnique.Passes["Pass1"].Apply();
            int foglvel = cam.GetFogLevel();
            for (int i = foglvel; i <= cam.MaxDrawZ; i++)
            {
                var slice = this.Slices[i];
                slice.Canvas.Opaque.Draw();
            }
            effectHideWalls.SetValue(false);
            effect.CurrentTechnique.Passes["Pass1"].Apply();
            for (int i = foglvel; i <= cam.MaxDrawZ; i++)
            {
                var slice = this.Slices[i];
                slice.Canvas.NonOpaque.Draw();
            }
            if (cam.DrawTopSlice)
            {
                effectHideWalls.SetValue(Engine.HideWalls);
                effect.CurrentTechnique.Passes["Pass1"].Apply();
                this.Slices[cam.MaxDrawZ].Unknown.Draw();
            }
            foreach (var blockentity in this.BlockEntitiesByPosition)
                blockentity.Value.Draw(cam, this.Map, blockentity.Key.ToGlobal(this));
        }
        public void DrawTransparentLayers(Camera cam, Effect effect)
        {
            Coords.Iso(cam, this.MapCoords.X * Chunk.Size, this.MapCoords.Y * Chunk.Size, 0, out float x, out float y);
            Coords.Rotate(cam, this.MapCoords.X, this.MapCoords.Y, out int rotx, out int roty);
            var world = Matrix.CreateTranslation(new Vector3(x, y, ((rotx + roty) * Chunk.Size)));
            effect.Parameters["World"].SetValue(world);
            effect.CurrentTechnique.Passes["Pass1"].Apply();
            // no need to apply pass?
            int foglvel = (int)Math.Max(0, cam.LastZTarget - Camera.FogZOffset - Camera.FogFadeLength);
            for (int i = foglvel; i <= cam.MaxDrawZ; i++)
            {
                var slice = this.Slices[i];
                slice.Canvas.Transparent.Draw();
                if (cam.DrawZones)
                    slice.Canvas.Designations.Draw();
            }
        }
        internal bool Contains(Vector3 global)
        {
            return global.GetChunkCoords() == this.MapCoords;
        }

        public SaveTag SaveToTag()
        {
            string.Format("saving chunk {0}", this.MapCoords).ToConsole();

            SaveTag chunktag;
            chunktag = new SaveTag(SaveTag.Types.Compound, "Chunk");

            SaveTag heightTag = new SaveTag(SaveTag.Types.List, "Heightmap", SaveTag.Types.Byte);
            SaveTag entitiestag = new SaveTag(SaveTag.Types.List, "Entities", SaveTag.Types.Compound);
            SaveTag visibleCells = new SaveTag(SaveTag.Types.List, "VisibleCells", SaveTag.Types.Int);
            SaveTag lightTag = new SaveTag(SaveTag.Types.List, "Light", SaveTag.Types.Byte);

            var sw = Stopwatch.StartNew();
            this.SaveCellsToTagCompressed(chunktag);
            sw.Stop();
            string.Format("cells saved in {0} ms", sw.ElapsedMilliseconds).ToConsole();

            sw.Restart();
            int n = 0;
            foreach (Cell cell in this.CellGrid2)
            {
                byte light = (byte)((this.Sunlight[n] << 4) + this.BlockLight[n++]);
                lightTag.Add(new SaveTag(SaveTag.Types.Byte, "", light));
            }
            sw.Stop();
            string.Format("light saved in {0} ms", sw.ElapsedMilliseconds).ToConsole();

            sw.Restart();
            for (int j = 0; j < Size; j++)
                for (int i = 0; i < Size; i++)
                    heightTag.Add(new SaveTag(SaveTag.Types.Byte, "", (byte)this.HeightMap[i][j]));
            sw.Stop();
            string.Format("heightmap saved in {0} ms", sw.ElapsedMilliseconds).ToConsole();

            foreach (GameObject obj in this.Objects)
                entitiestag.Add(new SaveTag(SaveTag.Types.Compound, obj.Name, obj.SaveInternal()));

            SaveTag blockEntitiesTag = this.SaveBlockEntitiesDistinct();

            chunktag.Add(new SaveTag(SaveTag.Types.Bool, "LightValid", this.LightValid));
            chunktag.Add(new SaveTag(SaveTag.Types.Bool, "EdgesValid", this.EdgesValid));
            chunktag.Add(lightTag);
            chunktag.Add(heightTag);
            chunktag.Add(visibleCells);
            chunktag.Add(entitiestag);
            chunktag.Add(blockEntitiesTag);
            chunktag.Add(this.RandomOrderedCells.Save("RandomOrderedCells"));
            string.Format("saved chunk {0}", this.MapCoords).ToConsole();
            return chunktag;
        }

        internal Chunk LoadFromTag(SaveTag chunktag)
        {
            this.LightValid = chunktag.TagValueOrDefault<bool>("LightValid", false);
            this.EdgesValid = chunktag.TagValueOrDefault<bool>("EdgesValid", false);

            List<SaveTag> lightTag = chunktag["Light"].Value as List<SaveTag>;

            this.LoadCellsFromTagCompressed(chunktag);

            var n = 0;
            for (int h = 0; h < MapBase.MaxHeight; h++)
                for (int i = 0; i < Size; i++)
                    for (int j = 0; j < Size; j++)
                    {
                        byte light = (byte)lightTag[n].Value;
                        // DONT save light, recalculate on load
                        this.Sunlight[n] = (byte)((light & 0xF0) >> 4);
                        this.BlockLight[n] = (byte)(light & 0x0F);
                        n++;
                    }

            List<SaveTag> heightTag = chunktag["Heightmap"].Value as List<SaveTag>;
            n = 0;
            for (int j = 0; j < Size; j++)
                for (int i = 0; i < Size; i++)
                    this.HeightMap[i][j] = (byte)heightTag[n++].Value;

            List<SaveTag> entitytags = chunktag["Entities"].Value as List<SaveTag>;

            foreach (SaveTag tag in entitytags)
            {
                GameObject obj = GameObject.Load(tag);
                if (obj is not null)
                    this.Add(obj);
            }

            if (chunktag.TryGetTag("Block Objects", out SaveTag blobjTag))
                foreach (SaveTag tag in blobjTag.Value as List<SaveTag>)
                {
                    int index = (int)tag["Index"].Value;
                    GameObject obj = GameObject.Load(tag["Object"]);
                    this.BlockObjects[index] = obj;
                }

            this.LoadBlockEntitiesDistinct(chunktag);

            chunktag.TryGetTag("RandomOrderedCells", t => this._RandomOrderedCells = new List<IntVec3>().Load(t));
            return this;
        }
        private void LoadCellsFromTagCompressed(SaveTag chunktag)
        {
            List<SaveTag> celllist = chunktag["Cells"].Value as List<SaveTag>;
            int n = 0;
            var airCount = 0;
            bool airDiscovered = true;
            var listPosition = 0;
            var maxn = Size * Size * MapBase.MaxHeight;
            while (listPosition < celllist.Count)
            {
                var celltag = celllist[listPosition++];
                if ((byte)celltag["Tile"].Value == (byte)BlockDefOf.Air.Type)
                {
                    airCount = (int)celltag["Data"].Value;
                    celltag.TryGetTagValueNew("Discovered", ref airDiscovered);
                    for (int i = n; i < n + airCount; i++)
                    {
                        var c = this.CellGrid2[i];
                        c.Discovered = airDiscovered;

                    }

                    n += airCount;

                    continue;
                }
                var cell = this.CellGrid2[n++];
                cell.Load(celltag);
            }
        }

        internal bool IsSolid(IntVec3 local)
        {
            if (local.Z > this.Map.GetMaxHeight() - 1)
                return false;
            return this[local].IsSolid();
        }

        internal Block GetBlockFromGlobal(float globalx, float globaly, float globalz)
        {
            return this[globalx - this.Start.X, globaly - this.Start.Y, globalz].Block;
        }

        public void ValidateSlices(Camera cam)
        {
            var count = this.Slices.Length;
            for (int i = 0; i < count; i++)
            {
                var slice = this.Slices[i];
                if (slice is null)
                {
                    slice = new Slice();
                    this.Slices[i] = slice;
                }
                if (slice.Valid)
                    continue;
                this.BuildSlice(slice, cam, this.Map, i);
            }
            //TESTING IF REMOVING THIS BREAKS ANYTHING
            this.BuildFrontmostBlocksNewSlices(cam);
            foreach (var sl in this.Slices)
                sl.Valid = true;
        }
        public void BuildSlice(Slice slice, Camera camera, MapBase map, int z)
        {
            var unknown = new List<Cell>();
            var visible = new List<Cell>();

            // create the slice's undiscovered blocks mesh
            for (int i = 0; i < Chunk.Size; i++)
                for (int j = 0; j < Chunk.Size; j++)
                {
                    Cell cell;
                    var local = new IntVec3(i, j, z);

                    cell = this.CellGrid2[GetCellIndex(local)];
                    var global = local.ToGlobal(this);

                    // DO I NEED THIS?
                    if (!camera.HideUnknownBlocks)
                    {
                        if (cell.Block != BlockDefOf.Air && map.IsVisible(global)) // did i need visibleoutercells list afterall?
                            visible.Add(cell);
                    }
                    else
                    {
                        if (map.IsUndiscovered(global) || !map.IsVisible(global)) // did i need visibleoutercells list afterall?
                            unknown.Add(cell);
                        else
                        {
                            if (cell.Block != BlockDefOf.Air)
                                visible.Add(cell);
                        }
                    }
                }

            var unknownCount = unknown.Count;
            var unknownSlice = new MySpriteBatch(Game1.Instance.GraphicsDevice, unknownCount);
            foreach (var cell in unknown)
            {
                camera.DrawUnknown(unknownSlice, map, this, cell);
            }

            var visibleCount = visible.Count;
            var canvas = new Canvas(Game1.Instance.GraphicsDevice, visibleCount);
            slice.Canvas = canvas;
            for (int i = 0; i < visibleCount; i++)
            {
                var cell = visible[i];
                camera.DrawCell(slice.Canvas, map, this, cell);
            }

            slice.Unknown = unknownSlice;
        }
        public void BuildFrontmostBlocksNewSlices(Camera camera)
        {
            //return;
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
            var map = this.Map;
            if (chunkX == edgeX)
            {
                for (int j = 0; j < maxheight; j++)
                {
                    var slice = this.Slices[j];
                    if (slice.Valid == true)
                        continue;
                    for (int i = 0; i < Chunk.Size; i++)
                    {
                        {
                            Cell cell;
                            var pos = new IntVec3(Chunk.Size - 1, i, j);
                            switch ((int)camera.Rotation)
                            {
                                case 0:
                                case 1:
                                    pos = new IntVec3(Chunk.Size - 1, i, j);
                                    break;
                                case 2:
                                case 3:
                                    pos = new IntVec3(0, i, j);
                                    break;

                                default:
                                    break;
                            }
                            var cellIndex = Chunk.GetCellIndex(pos.X, pos.Y, pos.Z);// FASTER WITH INTS
                            cell = this.CellGrid2[cellIndex];

                            if (camera.HideUnknownBlocks && map.IsUndiscovered(pos.ToGlobal(this)))
                                camera.DrawUnknown(slice.Canvas, map, this, cell);
                            // TESTING IF REMOVING THIS BREAKS ANYTHING
                            else if (cell.Block.Type != Block.Types.Air)
                                camera.DrawCell(slice.Canvas, map, this, cell);
                        }
                    }
                }
            }
            if (chunkY == edgeY)
            {
                for (int j = 0; j < maxheight; j++)
                {
                    var slice = this.Slices[j];
                    if (slice.Valid == true)
                        continue;
                    for (int i = 0; i < Chunk.Size; i++)
                    {
                        {
                            Cell cell;
                            var pos = new IntVec3(i, Chunk.Size - 1, j);
                            switch ((int)camera.Rotation)
                            {
                                case 0:
                                case 3:
                                    pos = new IntVec3(i, Chunk.Size - 1, j);
                                    break;
                                case 2:
                                case 1:
                                    pos = new IntVec3(i, 0, j);
                                    break;

                                default:
                                    break;
                            }
                            var cellIndex = Chunk.GetCellIndex(pos.X, pos.Y, pos.Z);// FASTER WITH INTS
                            cell = this.CellGrid2[cellIndex];
                            if (camera.HideUnknownBlocks && map.IsUndiscovered(pos.ToGlobal(this)))
                                camera.DrawUnknown(slice.Canvas, map, this, cell);
                            // TESTING IF REMOVING THIS BREAKS ANYTHING
                            else if (cell.Block.Type != Block.Types.Air)
                                camera.DrawCell(slice.Canvas, map, this, cell);
                        }
                    }
                }
            }
        }

        public Slice[] Slices = new Slice[128];
        public class Slice
        {
            public bool Valid;
            public Canvas Canvas;
            public MySpriteBatch Unknown;
        }
        public bool TryGetBlockEntity(IntVec3 local, out BlockEntity entity)
        {
            return this.BlockEntitiesByPosition.TryGetValue(local, out entity);
        }

        public void AddBlockEntity(BlockEntity entity, IntVec3 local)
        {
            this.BlockEntitiesByPosition[local] = entity;
        }
        public bool TryRemoveBlockEntity(IntVec3 local, out BlockEntity entity)
        {
            if (this.BlockEntitiesByPosition.TryGetValue(local, out entity))
                this.BlockEntitiesByPosition.Remove(local);
            return entity is not null;
        }

        public IEnumerable<(IntVec3 local, BlockEntity entity)> GetBlockEntitiesByPosition()
        {
            foreach (var be in this.BlockEntitiesByPosition)
                yield return (be.Key, be.Value);
        }
    }
}
