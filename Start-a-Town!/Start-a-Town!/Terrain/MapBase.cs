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
using System.Threading.Tasks;

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
                PacketSpawn = Network.RegisterPacketHandler(ReceiveSpawnEntity);
            }
            static public void SendSpawnEntity(INetwork net, GameObject entity, MapBase map, Vector3 global, Vector3 velocity)
            {
                if (net is Client)
                    return;
                var w = net.GetOutgoingStream();
                w.Write(PacketSpawn);
                w.Write(entity.RefID);
                w.Write(global);
                w.Write(velocity);
            }
            static void ReceiveSpawnEntity(INetwork net, BinaryReader r)
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
            private static void SyncSetCellData(INetwork net, BinaryReader r)
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
        protected Queue<IntVec3> RandomBlockUpdateQueue = new();
        public LightingEngine LightingEngine;
        public IWorld World;
        public Dictionary<Vector2, Chunk> ActiveChunks;
        INetwork _net;
        public INetwork Net => this._net ??= this.World.Net;
        public GameObject PlayerCharacter;
        public ParticleManager ParticleManager;
        public RegionManager Regions;
        protected List<GameObject> CachedObjects = new();
        protected Dictionary<IntVec3, BlockEntity> CachedBlockEntities = new();

        public abstract Color GetAmbientColor();
        public abstract void SetAmbientColor(Color color);
        public abstract double GetDayTimeNormal();
        public abstract Texture2D GetThumbnail();
        public abstract float LoadProgress { get; }
        public ulong CurrentTick => this.World.CurrentTick;
        public TimeSpan Clock => this.World.Clock;
        
        public abstract Vector2 GetOffset();

        static public Texture2D Shadow;
        static internal void Initialize()
        {
            Generator.InitGradient3();
            Shadow = Game1.Instance.Content.Load<Texture2D>("Graphics/shadow");
        }

        internal bool IsDeconstructible(IntVec3 global)
        {
            return (this.GetBlockEntity(global)?.HasComp<BlockEntityCompDeconstructible>() ?? false) || this.GetBlock(global).IsDeconstructible;
        }

        public Vector2 Coordinates;
        public abstract string GetName();
        public float Gravity => this.World.Gravity;
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

        internal bool IsDesignation(IntVec3 global)
        {
            return this.Town.DesignationManager.IsDesignation(global);
        }

        public abstract int GetMaxHeight();
        public abstract int GetSizeInChunks();

        protected int[] _randomOrderedChunkIndices;
        protected int[] RandomOrderedChunkIndices
        {
            get
            {
                if (this._randomOrderedChunkIndices is null)
                {
                    this._randomOrderedChunkIndices = Enumerable.Range(0, this.ActiveChunks.Count).Shuffle(this.Random).ToArray();
                    // force initialization on all chunks
                    foreach (var ch in this.ActiveChunks.Values)
                        _ = ch.GetRandomCellInOrder(0);
                }
                return this._randomOrderedChunkIndices;
            }
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
                    this.RandomCellIndex = 0;
            }

            return randomCell.ToGlobal(randomChunk);
        }

        public IEnumerable<Cell> AllCells
        {
            get
            {
                foreach (var ch in this.ActiveChunks.Values)
                    foreach (var c in ch.CellGrid2)
                        yield return c;
            }
        }

        internal void ResolveReferences()
        {
            this.World.ResolveReferences();
            this.Town.ResolveReferences();
            foreach (var chunk in this.ActiveChunks.Values)
                chunk.ResolveReferences();
        }

        internal void ReplaceBlock(Vector3 global, Block block, byte data, int variation, int orientation, bool raiseEvent = true)
        {
            this.RemoveBlock(global);

            var blockentity = this.RemoveBlockEntity(global);
            if (blockentity != null)
            {
                blockentity.OnRemoved(this, global);
                blockentity.Dispose();
                this.Net.EventOccured(Components.Message.Types.BlockEntityRemoved, blockentity, global);
            }

            // reenable physics of entities resting on block
            foreach (var entity in this.GetObjects(global.Above()))
                PhysicsComponent.Enable(entity);

            this.SetBlock(global, block, data, variation, orientation, raiseEvent);
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
        public void RemoveBlock(IntVec3 global, bool notify = true)
        {
            var cell = this.GetCell(global);
            var block = cell.Block;
            var data = cell.BlockData;
            var center = block.GetCenter(data, global);
            foreach (var u in block.UtilitiesProvided)
                this.Town.RemoveUtility(u, center);
            var blockentity = this.GetBlockEntity(global);
            var parts = block.GetParts(data, global);
            this.GetBlock(global).PreRemove(this, global); // preremove only center part or all parts?
            if (blockentity != null)
            {
                parts = blockentity.CellsOccupied;
                foreach (var g in parts)
                    this.RemoveBlockEntity(g);
                blockentity.OnRemoved(this, center);
                blockentity.Dispose();
                if (notify)
                    this.Net.EventOccured(Message.Types.BlockEntityRemoved, blockentity, global);
            }
            foreach (var p in parts)
            {
                this.SetBlock(p, BlockDefOf.Air, 0, 0, 0, notify);
                this.SetBlockLuminance(p, 0);
                // reenable physics of entities resting on block
                foreach (var entity in this.GetObjects(p - new IntVec3(1, 1, 0), p + new IntVec3(1, 1, 2)))
                    PhysicsComponent.Enable(entity);
                var above = p.Above;
                this.GetBlock(above)?.BlockBelowChanged(this, above);
            }
        }
        /// <summary>
        /// starts and returns an async task handling map generation
        /// </summary>
        /// <returns></returns>
        public abstract Task Generate(bool showDialog);

        internal void RemoveBlocks(IEnumerable<IntVec3> positions, bool notify = true)
        {
            var nonAirPositions = positions.Where(vec => this.GetBlock(vec) != BlockDefOf.Air).ToList();
            foreach (var global in nonAirPositions)
                this.RemoveBlock(global, false);
            if (notify)
                this.NotifyBlocksChanged(nonAirPositions);
        }
        public Block GetBlock(IntVec3 global)
        {
            if (!this.TryGetCell(global, out var cell))
                return null;
            return cell.Block;
        }
        public Block GetBlock(IntVec3 global, out Cell cell)
        {
            if (!this.TryGetCell(global, out cell))
                return null;
            return cell.Block;
        }

        public BlockEntity RemoveBlockEntity(IntVec3 global)
        {
            Chunk chunk = this.GetChunk(global);
            var local = global.ToLocal();
            
            if (chunk.TryRemoveBlockEntity(local, out var entity))
                return entity;
            return null;
        }
        public void AddBlockEntity(IntVec3 global, BlockEntity entity)
        {
            entity.CellsOccupied.Add(global);
            Chunk chunk = this.GetChunk(global);
            entity.Place(this, global);
            var local = global.ToLocal();
            chunk.AddBlockEntity(entity, local);
        }

        internal IntVec3 GetFrontOfBlock(IntVec3 global)
        {
            var cell = this.GetCell(global);
            return global + cell.Front;
        }
        internal IntVec3 GetBehindOfBlock(IntVec3 global)
        {
            var cell = this.GetCell(global);
            return global + cell.Back;
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
            var globalRound = new Vector3((int)Math.Round(global.X), (int)Math.Round(global.Y), (int)Math.Floor(global.Z));
            if (this.TryGetChunk(globalRound, out var chunk))
                return chunk[globalRound.X - chunk.Start.X, globalRound.Y - chunk.Start.Y, globalRound.Z];
            return null;
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

        public abstract bool TryGetAll(int gx, int gy, int gz, out Chunk chunk, out Cell cell, out int lx, out int ly);
        
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

        internal bool Remove(GameObject obj)
        {
            return this.GetChunk(obj.Global).Remove(obj);
        }
        internal void Add(GameObject obj)
        {
            this.GetChunk(obj.Global).Add(obj);
        }
        public IEnumerable<GameObject> GetObjects(Vector3 global)
        {
            var ch = this.GetChunk(global);
            var objects = ch.Objects;
            var count = objects.Count;
            var globalIntVec3 = global.ToCell();
            for (int i = 0; i < count; i++)
            {
                var e = objects[i];
                if (e.Global.ToCell() == globalIntVec3)
                    yield return e;
            }
        }
        public bool IsCellEmptyNew(IntVec3 global)
        {
            return !this.GetObjects(global).Any();
        }
        
        internal virtual IEnumerable<GameObject> GetObjects(IEnumerable<Vector3> positions)
        {
            var chunks = new HashSet<Chunk>();
            foreach (var pos in positions)
                chunks.Add(this.GetChunk(pos));
            IEnumerable<GameObject> objects = chunks.SelectMany(ch => ch.GetObjects());
            return objects.Where(obj => positions.Contains(obj.Global.ToCell()));
        }

        public abstract bool IsInBounds(Vector3 global);

        public abstract void SetSkyLight(IntVec3 global, byte value);
        public abstract void SetBlockLight(IntVec3 global, byte value);

        public abstract void AddSkyLightChanges(Dictionary<IntVec3, byte> List);
        public abstract void AddBlockLightChanges(Dictionary<IntVec3, byte> List);
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
        public abstract byte GetSunLight(IntVec3 global);
        public abstract byte GetBlockData(IntVec3 global);
        public abstract byte SetBlockData(IntVec3 global, byte data = 0);

        public abstract void Update();
        public virtual void Tick() { }
        public abstract SaveTag Save();

        public abstract bool InvalidateCell(IntVec3 global);
        public abstract void GenerateThumbnails();
        public abstract void GenerateThumbnails(string fullpath);
        public abstract void LoadThumbnails();
        public abstract MapThumb GetThumb();

        public Town Town;

        public abstract void WriteData(BinaryWriter w);

        public abstract string GetFolderName();
        public abstract string GetFullPath();

        public abstract void UpdateLight(IEnumerable<IntVec3> positions);

        public abstract void DrawBlocks(MySpriteBatch sb, Camera cam, EngineArgs a);
        public abstract void DrawObjects(MySpriteBatch sb, Camera cam, SceneState scene);
        public abstract void DrawInterface(SpriteBatch sb, Camera cam);
        public abstract void DrawWorld(MySpriteBatch sb, Camera cam);
        public abstract void DrawBeforeWorld(MySpriteBatch sb, Camera cam);

        public abstract void GetTooltipInfo(Control tooltip);

        public virtual bool SetBlock(IntVec3 global, Block block, byte data, int variation = 0, int orientation = 0, bool raiseEvent = true)
        {
            if (global.Z == 0)
                return false;
            var cell = this.GetCell(global);

            if (cell is null)
                return false;

            var chunk = this.GetChunk(global);
            cell.Block = block;
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
        public void PlaceBlockNew(IntVec3 global, Block block, byte data, int variation = 0, int orientation = 0, bool notify = true)
        {
            if (block.IsValidPosition(this, global, orientation))
                return;
            var parts = block.GetParts(data, global);
            foreach (var pos in parts)
            {
                if (!this.SetBlock(pos, block, data, variation, orientation, notify))
                    return;
            }
            var entity = block.CreateBlockEntity(global);
            if (entity is not null)
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
        internal void DrawParticles(Camera camera)
        {
            if (this.Net is Server)
                return;
            this.ParticleManager.Draw(camera);
            foreach (var ch in this.ActiveChunks.Values)
                foreach (var (local, entity) in ch.GetBlockEntitiesByPosition())
                    entity.Draw(camera, this, local.ToGlobal(ch));
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

        internal MaterialDef GetBlockMaterial(Vector3 global)
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

        internal void RandomBlockUpdate(IntVec3 global)
        {
            var cell = this.GetCell(global);
            if (cell is not null)
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

        public bool IsActive => Ingame.CurrentMap == this;
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

        internal void Draw(ToolManager toolManager, UIManager windowManager, SceneState scene)
        {
            this.Camera.DrawMap(this, toolManager, windowManager, scene);
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
                    if (e.Exists)
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
        public IEnumerable<T> Find<T>(Func<T, bool> filter) where T : Entity
        {
            foreach (T o in this.GetObjectsLazy().OfType<T>())
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

        internal IEnumerable<KeyValuePair<IntVec3, BlockEntity>> GetBlockEntitiesWithComp<T>() where T : BlockEntityComp
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
            this.SyncSpawn(obj);
        }
        internal void SyncSpawn(GameObject obj)
        {
            obj.Spawn(this);
            Packets.SendSpawnEntity(this.Net, obj, this, obj.Global, obj.Velocity);
        }
        static MapBase()
        {

        }
    }
}
