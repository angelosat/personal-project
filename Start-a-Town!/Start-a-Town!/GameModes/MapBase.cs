using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using Start_a_Town_.Blocks;
using Start_a_Town_.Particles;
using Start_a_Town_.GameModes;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public abstract class MapBase
    {
        class Packets
        { 
            static readonly int
                PacketSyncSetCellData,
                PacketSpawn;
            static Packets()
            {
                PacketSyncSetCellData = Network.RegisterPacketHandler(SyncSetCellData);
                PacketSpawn = Network.RegisterPacketHandler(Receive);
            }
            static public void Send(IObjectProvider net, GameObject entity, MapBase map, Vector3 global, Vector3 velocity)
            {
                if (net is Client)
                    return;
                var w = net.GetOutgoingStream();
                w.Write(PacketSpawn);
                w.Write(entity.RefID);
                w.Write(global);
                w.Write(velocity);
            }
            static void Receive(IObjectProvider net, BinaryReader r)
            {
                var client = net as Client;
                var actor = client.GetNetworkObject(r.ReadInt32());
                var global = r.ReadVector3();
                var velocity = r.ReadVector3();
                var map = client.Map;
                map.SyncSpawn(actor, global, velocity);
            }
            
            public static void SyncSetCellData(MapBase map, IntVec3 global, byte data)
            {
                var net = map.Net;
                if (net is Server)
                    map.SetCellData(global, data);
                net.WriteToStream(PacketSyncSetCellData, global, data);
            }
            private static void SyncSetCellData(IObjectProvider net, BinaryReader r)
            {
                var global = r.ReadIntVec3();
                var data = r.ReadByte();
                if (net is Client)
                    net.Map.SetCellData(global, data);
                else
                    SyncSetCellData(net.Map, global, data);
            }
        }

        public Camera Camera;
        public static float IconOffset = 0;
        public Biome Biome = new();
        protected Queue<Vector3> RandomBlockUpdateQueue = new();
        public LightingEngine LightingEngine;
        public IWorld World;
        public Dictionary<Vector2, Chunk> ActiveChunks;
        public IObjectProvider Net;
        public GameObject PlayerCharacter;
        public ParticleManager ParticleManager;
        public RegionManager Regions;
        protected List<GameObject> CachedObjects = new();
        protected Dictionary<IntVec3, BlockEntity> CachedBlockEntities = new Dictionary<IntVec3, BlockEntity>();

        public abstract Color GetAmbientColor();
        public abstract void SetAmbientColor(Color color);
        public abstract double GetDayTimeNormal();
        public abstract Texture2D GetThumbnail();
        public abstract float LoadProgress { get; }
        public ulong CurrentTick => this.World.CurrentTick;
        public TimeSpan Clock => this.World.Clock;
        
        public abstract Vector2 GetOffset();

        static public Texture2D ShaderMouseMap, BlockDepthMap; // TODO: move these to block class
        static public Texture2D Shadow;
        static internal void Initialize()
        {
            Generator.InitGradient3();
            ShaderMouseMap = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap - Cube");
            BlockDepthMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockDepth09height19");
            Shadow = Game1.Instance.Content.Load<Texture2D>("Graphics/shadow");
        }


        internal bool IsDeconstructible(Vector3 global)
        {
            return this.GetBlockEntity(global)?.HasComp<BlockEntityCompDeconstructible>() ?? this.GetBlock(global).IsDeconstructible;
        }

        public Vector2 Coordinates;
        public abstract string GetName();
        public float Gravity
        {
            get { return PhysicsComponent.Gravity; } // TODO: move gravity to world class
        }
        public readonly float PlantDensityTarget = .1f;
        public int ChunkVolume => Chunk.Size * Chunk.Size * this.GetMaxHeight();

        public int Area { get { return this.ActiveChunks.Count * Chunk.Size * Chunk.Size; } }

        internal Room GetRoomAt(IntVec3 global)
        {
            return this.Town.RoomManager.GetRoomAt(global);
        }

        public int Volume { get { return this.ActiveChunks.Count * ChunkVolume; } }

        public Random Random { get { return this.World.Random; } }
        public abstract Dictionary<Vector2, Chunk> GetActiveChunks();
        public abstract bool AddChunk(Chunk chunk);
        public abstract IEnumerable<GameObject> GetObjects();
        public abstract IEnumerable<GameObject> GetObjects(Vector3 min, Vector3 max);
        public abstract IEnumerable<GameObject> GetObjects(BoundingBox box);

        public static int MaxHeight = 128;

        internal bool IsDesignation(Vector3 global)
        {
            return this.Town.DesignationManager.IsDesignation(global);
        }

        public abstract int GetMaxHeight();
        public abstract int GetSizeInChunks();

        protected int[] _RandomOrderedChunkIndices;
        protected int[] RandomOrderedChunkIndices
        {
            get
            {
                if (this._RandomOrderedChunkIndices == null)
                {
                    this._RandomOrderedChunkIndices = Enumerable.Range(0, this.ActiveChunks.Count).Randomize(this.Random).ToArray();

                    foreach (var ch in this.ActiveChunks.Values)
                    {
                        // force initialization on all chunks
                        if (ch.GetRandomCellInOrder(0) == null)
                            throw new Exception();
                    }
                }
                return this._RandomOrderedChunkIndices;
            }
        }
        
        Chunk GetRandomChunkInOrder(int index)
        {
            if (index >= this.ActiveChunks.Count)
                throw new Exception();
            return this.ActiveChunks.ElementAt(this.RandomOrderedChunkIndices[index]).Value;
        }
        public Vector3 GetRandomCellInOrder(int index)
        {
            if (index >= this.Volume)
                throw new Exception();
            var chunkLength = ChunkVolume;
            var chunkIndex = index / chunkLength;
            var cellIndex = index % chunkLength;
            var chunk = this.GetRandomChunkInOrder(chunkIndex);
            var global = chunk.GetRandomCellInOrder(cellIndex).ToGlobal(chunk);
            return global;
        }

        int RandomChunkIndex, RandomCellIndex;
        public IntVec3 GetNextRandomCell()
        {
            var randomChunk = this.ActiveChunks.Values.ElementAt(this.RandomOrderedChunkIndices[this.RandomChunkIndex]);
            var randomCell = randomChunk.GetRandomCellInOrder(this.RandomCellIndex);
            this.RandomChunkIndex++;
            if (this.RandomChunkIndex >= this.ActiveChunks.Count)
            {
                this.RandomChunkIndex = 0;
                this.RandomCellIndex++;
                if (this.RandomCellIndex >= ChunkVolume)
                {
                    this.RandomCellIndex = 0;
                }
            }

            return randomCell.ToGlobal(randomChunk);
        }

        public IEnumerable<Cell> GetAllCells()
        {
            foreach (var ch in this.ActiveChunks.Values)
                foreach (var c in ch.CellGrid2)
                    yield return c;
        }

        internal void ResolveReferences()
        {
            this.World.ResolveReferences();
        }

        internal void ReplaceBlocks(IEnumerable<IntVec3> positions, Block.Types type, byte data, int variation, int orientation)
        {
            foreach (var global in positions)
                this.ReplaceBlock(global, type, data, variation, orientation, false);
            this.NotifyBlocksChanged(positions);
        }
        internal void ReplaceBlock(Vector3 global, Block.Types type, byte data, int variation, int orientation, bool raiseEvent = true)
        {
            this.GetBlock(global).Removed(this, global);

            var blockentity = this.RemoveBlockEntity(global);
            if (blockentity != null)
            {
                blockentity.OnRemove(this, global);
                blockentity.Dispose();
                this.Net.EventOccured(Components.Message.Types.BlockEntityRemoved, blockentity, global);
            }

            // reenable physics of entities resting on block
            foreach (var entity in this.GetObjects(global.Above()))
            {
                PhysicsComponent.Enable(entity);
            }

            this.SetBlock(global, type, data, variation, orientation, raiseEvent);
        }
        internal void SyncSetCellData(IntVec3 global, byte data)
        {
            Packets.SyncSetCellData(this, global, data);
        }
        internal void SetCellData(Vector3 global, byte v)
        {
            this.GetCell(global).BlockData = v;
            this.InvalidateCell(global);
        }
        public void RemoveBlockNew(IntVec3 global, bool notify = true)
        {
            var cell = this.GetCell(global);
            var block = cell.Block;
            var data = cell.BlockData;
            var center = block.GetCenter(data, global);
            foreach (var u in block.UtilitiesProvided)
                this.Town.RemoveUtility(u, center);
            var blockentity = this.GetBlockEntity(global);
            var parts = block.GetParts(data, global);

            if (blockentity != null)
            {
                parts = blockentity.CellsOccupied;
                foreach (var g in parts)
                    this.RemoveBlockEntity(g);
                blockentity.OnRemove(this, center);
                blockentity.Dispose();
                if (notify)
                    this.Net.EventOccured(Message.Types.BlockEntityRemoved, blockentity, global);
            }
            foreach (var p in parts)
            {
                this.SetBlock(p, Block.Types.Air, 0, 0, 0, notify);
                this.SetBlockLuminance(p, 0);
                // reenable physics of entities resting on block
                foreach (var entity in this.GetObjects(p - new IntVec3(1, 1, 0), p + new IntVec3(1, 1, 2)))
                    PhysicsComponent.Enable(entity);
                var above = p.Above;
                this.GetBlock(above)?.BlockBelowChanged(this, above);
            }
        }
        public void RemoveBlock(Vector3 global, bool notify = true)
        {
            this.RemoveBlockNew(global, notify);
        }
        internal void RemoveBlocks(List<IntVec3> positions, bool notify = true)
        {
            var nonAirPositions = positions.Where(vec => this.GetBlock(vec) != BlockDefOf.Air).ToList();
            foreach (var global in nonAirPositions)
                this.RemoveBlock(global, false);
            if (notify)
                this.NotifyBlocksChanged(nonAirPositions);
        }
        public Block GetBlock(int x, int y, int z)
        {
            return this.GetCell(x, y, z).Block;
        }
        public Block GetBlock(IntVec3 global)
        {
            if (!this.TryGetCell(global, out var cell))
                return null;
            return cell.Block;
        }
        
        public Block GetBlock(Vector3 global, out Cell cell)
        {
            if (!this.TryGetCell(global, out cell))
                return null;
            return cell.Block;
        }

        public BlockEntity RemoveBlockEntity(Vector3 global)
        {
            Chunk chunk = this.GetChunk(global);
            var local = global.ToLocal();
            
            if (chunk.TryRemoveBlockEntity(local, out var entity))
                return entity;
            return null;
        }
        
        public void AddBlockEntity(Vector3 global, BlockEntity entity)
        {
            entity.CellsOccupied.Add(global);
            Chunk chunk = this.GetChunk(global);
            entity.Place(this, global);
            var local = global.ToLocal();
            chunk.AddBlockEntity(entity, local);
        }
        public IEnumerable<KeyValuePair<Vector3, T>> GetBlockEntities<T>() where T : BlockEntity
        {
            foreach (var be in this.GetBlockEntitiesCache())
            {
                if (be.Value is T e)
                    yield return new KeyValuePair<Vector3, T>(be.Key, e);
            }
        }

        internal Vector3 GetFrontOfBlock(IntVec3 global)
        {
            var cell = this.GetCell(global);
            return global + cell.Front;
        }
        internal Vector3 GetBehindOfBlock(IntVec3 global)
        {
            var cell = this.GetCell(global);
            return global + cell.Back;
        }
       
        public IEnumerable<IntVec3> GetBlockEntities()
        {
            foreach (var ch in ActiveChunks.Values)
                foreach (var (local, entity) in ch.GetBlockEntitiesByPosition())
                    yield return local.ToGlobal(ch);
        }
        
        public Dictionary<IntVec3, BlockEntity> GetBlockEntitiesCache()
        {
            return this.CachedBlockEntities;
        }
        public bool TryGetBlockEntity(IntVec3 global, out BlockEntity entity)
        {
            entity = null;
            if (this.GetChunk(global) is not Chunk chunk)
                return false;
            return chunk.TryGetBlockEntity(global.ToLocal(), out entity);
        }
        public BlockEntity GetBlockEntity(IntVec3 global)
        {
            Chunk chunk = this.GetChunk(global);
            chunk.TryGetBlockEntity(global.ToLocal(), out var entity);
            return entity;
        }
        public T GetBlockEntity<T>(IntVec3 global) where T : BlockEntity
        {
            Chunk chunk = this.GetChunk(global);
            chunk.TryGetBlockEntity(global.ToLocal(), out var entity);

            return entity as T;
        }
        public virtual int GetHeightmapValue(int x, int y)
        {
            var global = new Vector3(x, y, 0);
            var ch = this.GetChunk(global);
            if (ch == null)
                return int.MinValue;
            return ch.GetHeightMapValue(global.ToLocal());
        }
        public virtual int GetHeightmapValue(Vector3 global)
        {
            var ch = this.GetChunk(global);
            if (ch == null)
                return int.MinValue;
            return ch.GetHeightMapValue(global.ToLocal());
        }

        internal bool IsAdjacentToSolid(Vector3 global)
        {
                foreach (var adj in VectorHelper.Adjacent)
                {
                    var n = global + adj;
                    if (this.Town.Map.IsSolid(n))
                        return true;
                }
                return false;
        }

        public Cell GetCell(int x, int y, int z)
        {
            var chunk = GetChunk(x, y);
            var cell = chunk[x - chunk.Start.X, y - chunk.Start.Y, z];
            return cell;
        }
        
        public Cell GetCell(Vector3 global)
        {
            Vector3 globalRound = new Vector3((int)Math.Round(global.X), (int)Math.Round(global.Y), (int)Math.Floor(global.Z));
            Chunk chunk;
            if (this.TryGetChunk(globalRound, out chunk))
            {
                Cell cell = chunk[globalRound.X - chunk.Start.X, globalRound.Y - chunk.Start.Y, globalRound.Z];
                return cell;
            }
            return null;
        }
        public Chunk GetChunkAt(Vector2 chunkCoords)
        {
            return this.ActiveChunks[chunkCoords];
        }
        public Chunk GetChunk(Vector3 global)
        {
            if (this.TryGetChunk(global, out var chunk))
                return chunk;
            return null;
        }
        public Chunk GetChunk(int x, int y)
        {
            int chunkX = x / Chunk.Size;
            int chunkY = y / Chunk.Size;
            return this.ActiveChunks[new Vector2(chunkX, chunkY)];
        }
        public List<Chunk> GetChunks(Vector2 pos, int radius = 1)
        {
            List<Chunk> list = new List<Chunk>();
            int x = (int)pos.X, y = (int)pos.Y;
            for (int i = x - radius; i <= x + radius; i++)
                for (int j = y - radius; j <= y + radius; j++)
                    if (this.ActiveChunks.TryGetValue(new Vector2(i, j), out Chunk ch))
                        list.Add(ch);
            return list;
        }
        public IEnumerable<Chunk> GetChunkNeighborhood(Vector2 chunkCoords, int radius = 1)
        {
            int x = (int)chunkCoords.X, y = (int)chunkCoords.Y;
            for (int i = x - radius; i <= x + radius; i++)
                for (int j = y - radius; j <= y + radius; j++)
                    if (this.ActiveChunks.TryGetValue(new Vector2(i, j), out var ch))
                        yield return ch;
        }
        public IEnumerable<Chunk> GetChunkNeighborhood(Vector3 global, int radius = 1)
        {
            var chunkCoords = this.GetChunk(global).MapCoords;
            return this.GetChunkNeighborhood(chunkCoords, radius);
        }
        public bool TryGetBlock(Vector3 global, out Block block)
        {
            block = this.GetBlock(global);
            return block != null;
        }
        public bool TryGetCell(Vector3 global, out Cell cell)
        {
            Chunk chunk;
            return this.TryGetAll(global, out chunk, out cell);
        }
        public bool TryGetChunk(Vector3 global, out Chunk chunk)
        {
            if (global.Z < 0 || global.Z >= MaxHeight)
            {
                chunk = null;
                return false;
            }
            var x = Math.Round(global.X);
            var y = Math.Round(global.Y);
            int chunkX = (int)Math.Floor(x / Chunk.Size);
            int chunkY = (int)Math.Floor(y / Chunk.Size);
            return this.ActiveChunks.TryGetValue(new Vector2(chunkX, chunkY), out chunk);
        }
        public bool TryGetChunk(int globalx, int globaly, out Chunk chunk)
        {
            float chunkX = (float)Math.Floor((float)globalx / Chunk.Size);
            float chunkY = (float)Math.Floor((float)globaly / Chunk.Size);

            return this.ActiveChunks.TryGetValue(new Vector2(chunkX, chunkY), out chunk);
        }
        public bool TryGetAll(Vector3 global, out Chunk chunk, out Cell cell)
        {
            cell = null;
            chunk = null;
            Vector3 rounded = global.RoundXY();
            if (rounded.Z < 0 || rounded.Z > this.World.MaxHeight - 1)
                return false;
            int chunkX = (int)Math.Floor(rounded.X / Chunk.Size);
            int chunkY = (int)Math.Floor(rounded.Y / Chunk.Size);
            if (this.ActiveChunks.TryGetValue(new Vector2(chunkX, chunkY), out chunk))
            {
                cell = chunk[(int)(rounded.X - chunk.Start.X), (int)(rounded.Y - chunk.Start.Y), (int)rounded.Z];
                return true;
            }
            return false;
        }
        
        internal bool IsStandableIn(Vector3 global)
        {
            var curblock = this.GetBlock(global);
            var belowBlock = this.GetBlock(global.Below());
            return curblock.IsStandableIn && belowBlock.IsStandableOn;
        }
        internal bool IsStandableOn(Vector3 global)
        {
            var above = global.Above();
            return this.GetBlock(global).IsStandableOn && this.GetBlock(above).IsStandableIn;
        }
        public bool TryGetAll(Vector3 global, out Chunk chunk, out Cell cell, out Vector3 local)
        {
            Vector3 rnd = global.RoundXY();
            local = rnd.ToLocal();
            return this.TryGetAll(global, out chunk, out cell);
        }
        public bool ChunkExists(Vector2 chunkCoords)
        {
            return this.ActiveChunks.ContainsKey(chunkCoords);
        }

        public bool ChunksExist(Vector2 chunkCoords, int radius)
        {
            int minX = (int)chunkCoords.X - radius;
            int minY = (int)chunkCoords.Y - radius;
            int maxX = (int)chunkCoords.X + radius;
            int maxY = (int)chunkCoords.Y + radius;
            for (int i = minX; i <= maxX; i++)
                for (int j = minY; j <= maxY; j++)
                {
                    var n = new Vector2(i, j);
                    if (!this.ActiveChunks.ContainsKey(n))
                        return false;
                }
            return true;
        }

        public abstract bool ChunkNeighborsExist(Vector2 chunkCoords);
        public abstract bool TryGetAll(int gx, int gy, int gz, out Chunk chunk, out Cell cell, out int lx, out int ly);
        public virtual bool IsHidden(Vector3 global)
        {
            var cell = this.GetCell(global);
            return cell.IsHidden();
        }
        public virtual bool IsSolid(Vector3 global)
        {
            if (!this.TryGetCell(global, out Cell cell))
                return true; // return true to prevent crashing by trying to add object to missing chunk
            //return false; // return false to let entity attempt to enter unloaded chunk so we can handle the event of that


            var offset = global + new Vector3(0.5f, 0.5f, 0);
            var blockCoords = offset - offset.FloorXY();

            var issolid = cell.Block.IsSolid(cell, blockCoords);
            return issolid;
        }
        public virtual bool IsPathable(Vector3 global)
        {
            if (this.IsInBounds(global))
            {
                var cell = this.GetCell(global);
                return cell.Block.IsPathable(cell, global.ToBlock());
            }
            return false;
        }

        public virtual bool IsEmpty(Vector3 global)
        {
            global = global.Round();
            if (this.GetBlock(global) != BlockDefOf.Air)
                return false;
            var blockbox = new BoundingBox(global - (Vector3.UnitX + Vector3.UnitY) * .5f, global + Vector3.UnitZ + (Vector3.UnitX + Vector3.UnitY) * .5f);
            var entities = GetObjectsAtChunk(global);
            foreach (var entity in entities)
            {
                var entitybox = new BoundingBox(entity.Transform.Global - (Vector3.UnitX + Vector3.UnitY) * .2f, entity.Transform.Global + Vector3.UnitZ * entity.Physics.Height + (Vector3.UnitX + Vector3.UnitY) * .2f);
                if (blockbox.Intersects(entitybox))
                    return false;
            }
            return true;
        }

        public abstract List<GameObject> GetObjectsAtChunk(Vector3 global);
        public List<GameObject> GetObjectsIntersectingBlock(Vector3 global)
        {
            var entities = this.GetObjectsAtChunk(global);
            var list = new List<GameObject>();
            var blockbox = new BoundingBox(global - new Vector3(.5f, .5f, 0), global + new Vector3(.5f, .5f, 1));
            foreach (var entity in entities)
            {
                var size = .5f;// .2f;
                var entitybox = new BoundingBox(entity.Global - new Vector3(size, size, 0), entity.Global + new Vector3(size, size, entity.Physics.Height));
                if (blockbox.Intersects(entitybox))
                    list.Add(entity);
            }
            return list;
        }
        public IEnumerable<GameObject> GetObjects(Vector3 global)
        {
            var ch = this.GetChunk(global);
            var objects = ch.Objects;
            var count = objects.Count;
            var globalIntVec3 = global.SnapToBlock();
            for (int i = 0; i < count; i++)
            {
                var e = objects[i];
                if (e.Global.SnapToBlock() == globalIntVec3)
                    yield return e;
            }
        }
        public bool IsEmptyNew(Vector3 global)
        {
            return !this.GetObjects(global).Any();
        }
        
        internal virtual IEnumerable<GameObject> GetObjects(IEnumerable<Vector3> positions)
        {
            var chunks = new HashSet<Chunk>();
            foreach (var pos in positions)
                chunks.Add(this.GetChunk(pos));
            IEnumerable<GameObject> objects = chunks.SelectMany(ch => ch.GetObjects());
            return objects.Where(obj => positions.Contains(obj.Global.SnapToBlock()));
        }

        public abstract bool IsInBounds(Vector3 global);

        public abstract void SetSkyLight(Vector3 global, byte value);
        public abstract void SetBlockLight(Vector3 global, byte value);

        public abstract void AddSkyLightChanges(Dictionary<Vector3, byte> List);
        public abstract void AddBlockLightChanges(Dictionary<Vector3, byte> List);
        public abstract void ApplyLightChanges();

        /// <summary>
        /// Vector must be rounded!!!
        /// </summary>
        /// <param name="global">must be rounded!!!</param>
        /// <param name="sun"></param>
        /// <param name="block"></param>
        /// <returns></returns>
        public virtual bool GetLight(Vector3 global, out byte sky, out byte block)
        {
            int x = (int)Math.Round(global.X);
            int y = (int)Math.Round(global.Y);
            int z = (int)Math.Floor(global.Z);
            return Chunk.TryGetFinalLight(this, x, y, z, out sky, out block);
        }
        public virtual bool GetLight(int x, int y, int z, out byte sky, out byte block)
        {
            return Chunk.TryGetFinalLight(this, x, y, z, out sky, out block);
        } 
        public abstract byte GetSkyDarkness();
        public abstract byte GetSunLight(Vector3 global);
        public abstract byte GetBlockData(Vector3 global);
        public abstract byte SetBlockData(Vector3 global, byte data = 0);

        public abstract void Update(IObjectProvider net);
        public virtual void Tick(IObjectProvider net) { }
        public abstract SaveTag Save();

        public abstract bool InvalidateCell(Vector3 global);
        public abstract void GenerateThumbnails();
        public abstract void GenerateThumbnails(string fullpath);
        public abstract void LoadThumbnails();
        public abstract MapThumb GetThumb();


        public abstract Towns.Town GetTown();
        public Towns.Town Town;

        public abstract void WriteData(BinaryWriter w);

        public abstract string GetFolderName();
        public abstract string GetFullPath();

        public abstract void UpdateLight(IEnumerable<WorldPosition> positions);


        public abstract void DrawBlocks(MySpriteBatch sb, Camera cam, EngineArgs a);
        public abstract void DrawObjects(MySpriteBatch sb, Camera cam, SceneState scene);
        public abstract void DrawInterface(SpriteBatch sb, Camera cam);
        public abstract void DrawWorld(MySpriteBatch sb, Camera cam);
        public abstract void DrawBeforeWorld(MySpriteBatch sb, Camera cam);

        public abstract void GetTooltipInfo(Tooltip tooltip);

        public abstract bool SetBlock(Vector3 global, Block.Types type);
        public virtual bool SetBlock(Vector3 global, Block.Types type, byte data, int variation = 0, int orientation = 0, bool raiseEvent = true)
        {
            if (global.Z == 0)
                return false;
            Cell cell = this.GetCell(global);

            if (cell == null)
                return false;

            Chunk chunk = this.GetChunk(global);
            cell.SetBlockType(type);
            cell.Variation = (byte)variation;
            cell.BlockData = data;
            cell.Orientation = orientation;
            // maybe put block entity creation here?

            chunk.InvalidateHeightmap(cell.X, cell.Y);

            // maybe i can refresh cell edges here on the spot?
            this.InvalidateCell(global);
            var neighbors = global.GetAdjacentLazy();

            foreach (var n in neighbors)
            {
                var nblock = this.GetBlock(n);
                if (nblock != BlockDefOf.Air)
                    this.InvalidateCell(n);

                if (nblock != null)
                    nblock.NeighborChanged(this.Net, n);
            }
            //this.Town.InvalidateBlock(global); // handle blockchanged event in town class instead
            if (raiseEvent)
                NotifyBlockChanged(global);
            return true;
        }
        public void PlaceBlockNew(IntVec3 global, Block.Types type, byte data, int variation = 0, int orientation = 0, bool notify = true)
        {
            var block = Block.Registry[type];
            if (block.IsValidPosition(this, global, orientation))
                return;
            var parts = block.GetParts(data, global);
            foreach (var pos in parts)
            {
                if (!this.SetBlock(global, type, data, variation, orientation, notify))
                    return;
            }
            var entity = block.CreateBlockEntity();
            if (entity != null)
            {
                this.AddBlockEntity(global, entity);
                this.EventOccured(Message.Types.BlockEntityAdded, entity, global);
            }
            this.SetBlockLuminance(global, block.Luminance);
        }


        public void NotifyBlocksChanged(IEnumerable<IntVec3> positions)
        {
            this.Net.EventOccured(Components.Message.Types.BlocksChanged, this, positions);
            this.Town.OnBlocksChanged(positions);
        }
        private void NotifyBlockChanged(IntVec3 pos)
        {
            this.NotifyBlocksChanged(new[] { pos });
        }

        public abstract bool SetBlockLuminance(IntVec3 global, byte luminance);
        [Obsolete]
        internal bool IsTraversable2Height(Vector3 source, Vector3 target)
        {
            var globalsource = source;
            var globaltarget = target;
            if (globalsource.Z == globaltarget.Z)
                return true;
            var lower = Math.Min(globalsource.Z, globaltarget.Z) == globalsource.Z ? globalsource : globaltarget;
            var above1 = lower.Above();
            var above2 = above1.Above();
            var above3 = above2.Above();
            var above3block = this.GetBlock(above3);
            if (above3block.Solid) // no need to check for doors because they are defined as non-solid
            {
                return false;
            }
            return true;
        }
        internal bool IsTraversable(Vector3 source, Vector3 target)
        {
            var globalsource = source;
            var globaltarget = target;
            if (globalsource.Z == globaltarget.Z)
                return true;
            var lower = Math.Min(globalsource.Z, globaltarget.Z) == globalsource.Z ? globalsource : globaltarget;
            var above1 = lower.Above();
            var above2 = above1.Above();
            return !this.GetBlock(above2).Solid;
        }
        public void EventOccured(Message.Types type, params object[] p)
        {
            this.Net?.EventOccured(type, p);
        }

        public virtual void OnGameEvent(GameEvent e)
        {
            this.ParticleManager.OnGameEvent(e);
            foreach (var obj in this.CachedObjects)
                obj.OnGameEvent(e);
        }
        public float GetSolidObjectHeight(Vector3 global)
        {
            var cell = this.GetCell(global);
            if (cell.Block != BlockDefOf.Air)
                return cell.Block.GetHeight(cell.BlockData, global.ToBlock());

            var entities = this.GetObjects(global - new Vector3(5), global + new Vector3(5));
            foreach (var entity in entities)
            {
                if (!entity.Physics.Solid)
                    continue;
                BoundingBox box = new BoundingBox(entity.Global - new Vector3(0.5f, 0.5f, 0), entity.Global + new Vector3(0.5f, 0.5f, entity.Physics.Height));
                var cont = box.Contains(global);
                if (cont == ContainmentType.Contains)
                {
                    if (Vector3.Distance(global * new Vector3(1, 1, 0), entity.Global * new Vector3(1, 1, 0)) < 0.5f)
                        return entity.Physics.Height;
                }
            }
            return 0;
        }

        public void InvalidateChunks()
        {
            foreach (var chunk in this.ActiveChunks)
                chunk.Value.Invalidate();
        }

        internal void UpdateParticles()
        {
            this.ParticleManager.Update();
        }
        internal void DrawParticles(MySpriteBatch mysb, Camera camera)
        {
            if (this.Net is Net.Server)
                return;
            this.ParticleManager.Draw(camera);
            foreach (var ch in this.ActiveChunks.Values)
                foreach (var be in ch.GetBlockEntitiesByPosition())
                    be.entity.Draw(camera, this, be.local.ToGlobal(ch));
        }
        internal IEnumerable<(string name, Action action)> GetInfoTabs()
        {
            yield break;
        }
        internal void OnTargetSelected(IUISelection info, ISelectable selected)
        {
            this.World.OnTargetSelected(info, selected);
            this.Town.OnTargetSelected(info, selected);
        }

        internal IEnumerable<GameObject> GetNearbyObjects(GameObject obj, float range, bool inclusive)  
        {
            return this.GetNearbyObjects(obj, r => r < range, inclusive);
        }
        internal IEnumerable<GameObject> GetNearbyObjects(GameObject obj, Func<float, bool> range, bool inclusive) 
        {
            if(inclusive)
                yield return obj;
            var objGlobal = obj.Global;
            foreach(var ch in this.GetChunkNeighborhood(objGlobal))
                foreach(var o in ch.GetObjectsLazy())
                {
                    if (o == obj)
                        continue;
                    if (!range(Vector3.Distance(o.Global, objGlobal)))
                        continue;
                    yield return o;
                }
        }
        public List<GameObject> GetNearbyObjects(Vector3 global, Func<float, bool> range, Func<GameObject, bool> filter = null, Action<GameObject> action = null)
        {
            var a = action ?? ((obj) => { });
            var f = filter ?? ((obj) => { return true; });
            List<GameObject> nearbies = new List<GameObject>();
            Chunk chunk = this.GetChunk(global);

            List<GameObject> objects = new List<GameObject>();
            foreach (Chunk ch in this.GetChunks(chunk.MapCoords))
                foreach (GameObject obj in ch.GetObjects())
                {
                    if (!range(Vector3.Distance(obj.Global, global)))
                        continue;
                    if (!f(obj))
                        continue;
                    a(obj);
                    nearbies.Add(obj);
                }
            return nearbies;
        }
        public IEnumerable<GameObject> GetNearbyObjectsNew(Vector3 global, Func<float, bool> range, Func<GameObject, bool> filter = null, Action<GameObject> action = null)
        {
            var a = action ?? ((obj) => { });
            var f = filter ?? ((obj) => { return true; });
            Chunk chunk = this.GetChunk(global);

            foreach (Chunk ch in this.GetChunks(chunk.MapCoords))
                foreach (GameObject obj in ch.GetObjects())
                {
                    if (!range(Vector3.Distance(obj.Global, global)))
                        continue;
                    if (!f(obj))
                        continue;
                    a(obj);
                    yield return obj;
                }
        }
        public bool LineOfSight(Vector3 a, Vector3 b)
        {
            var x0 = (int)a.X;
            var y0 = (int)a.Y;
            var z0 = (int)a.Z;
            var x1 = (int)b.X;
            var y1 = (int)b.Y;
            var z1 = (int)b.Z;
            var los = LineHelper.LineOfSight(x0, y0, z0, x1, y1, z1, this.IsSolid);
            return los;
        }

        internal Material GetBlockMaterial(Vector3 global)
        {
            return Block.GetBlockMaterial(this, global);
        }

        internal Region GetRegionAt(Vector3 north)
        {
            return this.Regions.GetRegionAt(north);
        }
        internal RegionNode GetNodeAt(Vector3 vector3)
        {
            return this.Regions.GetNodeAt(vector3);
        }

        internal bool CanReach(GameObject actor, Vector3 global)
        {
            return this.Regions.CanReach(actor, global);
        }
      
        internal int GetRegionDistance(Vector3 source, Vector3 target, Actor actor)
        {
            return this.Regions.GetRegionDistance(source, target, actor);
        }
        internal bool Contains(Vector3 global)
        {
            return this.GetChunk(global) != null;
        }

        internal bool IsAir(Vector3 global)
        {
            return this.GetBlock(global) == BlockDefOf.Air;
        }

        internal void RandomBlockUpdate(Vector3 global)
        {
            Cell cell = this.GetCell(global);
            if (cell != null)
                cell.Block.RandomBlockUpdate(this.Net, global, cell);
            else
                RandomBlockUpdateQueue.Enqueue(global);
        }
        public bool AreChunksLoaded
        {
            get
            {
                var size = this.GetSizeInChunks();
                var chunkcount = size * size;
                if (this.ActiveChunks.Count != chunkcount)
                    return false;
                if (this.ActiveChunks.Values.Any(c => c == null))
                    return false;
                return true;
            }
        }

        public bool IsActive => Rooms.Ingame.CurrentMap == this;
        public bool IsAboveHeightMap(IntVec3 global)
        {
            return this.IsAboveHeightMap((Vector3)global);
        }
        internal bool IsAboveHeightMap(Vector3 global)
        {
            var chunk = this.GetChunk(global);
            return chunk.IsAboveHeightMap(global.ToLocal());
        }
        
        internal virtual bool IsUndiscovered(Vector3 global)
        {
            return false;
        }


        internal virtual void AreaDiscovered(HashSet<Vector3> hashSet)
        {
            
        }

        internal void Draw(SpriteBatch sb, ToolManager toolManager, UIManager windowManager, SceneState scene)
        {
            this.Camera.DrawMap(sb, this, toolManager, windowManager, scene);
        }
        internal void MousePicking()
        {
            this.Camera.MousePicking(this);
        }
        internal virtual void CameraRecenter()
        {
        
        }
        public IEnumerable<GameObject> GetEntities()
        {
            var chunks = this.ActiveChunks.Values;
            foreach (var chunk in chunks)
            {
                var entities = chunk.Objects;
                foreach (var e in entities)
                    if (e.IsSpawned)
                        yield return e;
            }
        }
        public IEnumerable<GameObject> GetObjectsLazy()
        {
            var count = this.CachedObjects.Count;
            for (int i = 0; i < count; i++)
            {
                var obj = this.CachedObjects[i];
                if (obj.Exists)
                    yield return obj;
            }
        }
        public IEnumerable<Entity> Find(Func<Entity, bool> filter)
        {
            foreach (Entity o in this.GetObjectsLazy())
                if (filter(o))
                    yield return o;
        }
        internal bool IsVisible(Vector3 global)
        {
            if (global.Z == MaxHeight - 1)
                return true;
            var count = VectorHelper.Adjacent.Length;
            for (int i = 0; i < count; i++)
            {
                var n = global + VectorHelper.Adjacent[i];
                if (this.TryGetCell(n, out var ncell) && !ncell.Opaque)
                    return true;
            }
            return false;
        }

        internal IEnumerable<KeyValuePair<IntVec3, Blocks.BlockEntity>> GetBlockEntitiesWithComp<T>() where T : BlockEntityComp
        {
            var entities = this.GetBlockEntitiesCache();
            var count = entities.Count;
            for (int i = 0; i < count; i++)
            {
                var kv = entities.ElementAt(i);
                if (kv.Value.HasComp<T>())
                    yield return kv;
            }
        }

        internal void SyncSpawn(GameObject obj, Vector3 global, Vector3 velocity)
        {
            obj.Global = global;
            obj.Velocity = velocity;
            obj.Map = this;
            obj.Parent = null;
            obj.Spawn(this.Net);
            Packets.Send(this.Net, obj, this, global, velocity);
        }
        static MapBase()
        {

        }
    }
}
