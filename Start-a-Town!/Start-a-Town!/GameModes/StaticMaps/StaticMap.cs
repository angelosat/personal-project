using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Diagnostics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using System.IO;
using System.Threading;
using System.IO.Compression;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using Start_a_Town_.Towns;
using Start_a_Town_.Net;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.GameModes.StaticMaps
{
    public interface IObjectSpace
    {
        float Distance(GameObject obj1, GameObject obj2);
        Vector3? DistanceVector(GameObject obj1, GameObject obj2);
    }
    public class StaticMap : IMap, IObjectSpace, IDisposable, ITooltippable
    {
        //public string Network = "local";

        public override float LoadProgress
        {
            get { return this.ActiveChunks.Count / (float)(this.Size.Chunks * this.Size.Chunks); }
        }
        //static public int SizeInChunks { get { return (Engine.ChunkRadius * 2); } }
        //static public int SizeInBlocks { get { return SizeInChunks * Chunk.Size; } }
        public float Distance(GameObject obj1, GameObject obj2)
        {
            return Vector3.Distance(obj1.Global, obj2.Global);
        }
        public Vector3? DistanceVector(GameObject obj1, GameObject obj2)
        {
            return obj1.Global - obj2.Global;
        }

        public MapSize Size;
        #region Sizes
        public class MapSize : INamed
        {
            //public int Index { get; private set; }
            public string Name { get; private set; }
            public int Blocks { get; private set; }
            public int Chunks { get; private set; }
            public MapSize(//int index, 
                string name, int blocks)
            {
                //this.Index = index;
                Name = name;
                Blocks = blocks;
                this.Chunks = blocks / Chunk.Size;
            }
            public static readonly MapSize Micro = new("Micro", 32);
            public static readonly MapSize Tiny = new("Tiny", 64);
            public static readonly MapSize Small = new("Small", 128);
            public static readonly MapSize Normal = new("Normal", 256);
            public static readonly MapSize Huge = new("Huge", 512);

            public static MapSize Default = Micro;//Tiny;

            static public List<MapSize> GetList()
            {
                return new List<MapSize>() { Micro, Tiny, Small, Normal, Huge };
            }
        }

        internal void Init()
        {
            this.Town.Init();
        }

        internal Vector3 GetRandomEdgeCell()
        {
            var i = (int)(this.Size.Blocks * this.Random.NextDouble());
            var j = this.Random.Chance(.5f) ? 0 : this.Size.Blocks - 1;
            var vec2 = this.Random.Chance(.5f) ? new IntVec2(i, j) : new IntVec2(j, i);
            return new Vector3(vec2.X, vec2.Y, this.GetHeightmapValue(vec2.X, vec2.Y));
        }

        public static List<MapSize> Sizes
        { get { return new List<MapSize>() { MapSize.Micro, MapSize.Tiny, MapSize.Small, MapSize.Normal, MapSize.Huge }; } }

        
        #endregion

        public List<GameObject> SavedPlayers = new();

        Queue<Chunk> ChunksToActivate;

        public override bool AddChunk(Chunk chunk)
        {
            //return this.ActiveChunks.TryAdd(chunk.MapCoords, chunk);
            this.ActiveChunks.Add(chunk.MapCoords, chunk);
            // sort chunks back to front to prevent glitches with semi-transparent blocks on chunk edges
            this.ActiveChunks = this.ActiveChunks.OrderBy(c => c.Key.X + c.Key.Y).ToDictionary(i => i.Key, i => i.Value);

            return true;
        }
        public bool Lighting = true;
        public int TickLengthSeconds = (int)(60 * 1.44f); // one tick is 1.44 ingame minutes
        public const int Zenith = 14;
        //public Dictionary<>
        public double DayTimeNormal = 0;
        TimeSpan Clock => this.World.Clock;
        //public TimeSpan TimeFromCurrentTick => TimeSpan.FromMilliseconds((double)this.CurrentTick * TickLengthMilliseconds);
        public void AddTime()
        {
            //var seconds = TickLength;
            //Time = Time.Add(new TimeSpan(0, 0, seconds));
            //Time = Time.Add(TimeSpan.FromMilliseconds(TickLengthMilliseconds));
            //Clock = TimeFromCurrentTick;
            var clock = this.Clock;
            double normal = (clock.TotalMinutes - Engine.TicksPerSecond * (Zenith - 12)) / 1440f;
            double nn = normal * 2 * Math.PI;
            nn = 3 * Math.Cos(nn);
            DayTimeNormal = Math.Max(0, Math.Min(1, (1 + nn) / 2f));

            byte oldDarkness = SkyDarkness;
            SkyDarkness = 0;// (byte)(Math.Round(DayTimeNormal * SkyDarknessMax));
            if (SkyDarkness != oldDarkness)
                foreach (var ch in ActiveChunks)
                    ch.Value.LightCache.Clear();
        }
        [Obsolete]
        public void SetHour(int t)
        {
            throw new Exception();
            //this.Clock = new TimeSpan(this.Clock.Days, t, 0, 0);
            foreach (var ch in this.ActiveChunks)
                ch.Value.LightCache.Clear();
        }
        private void AdvanceTime()
        {
            //this.CurrentTick++;
            this.AddTime();
            return;
            t -= 1;//GlobalVars.DeltaTime;
            if (t <= 0)
            {
                AddTime();
                t = Engine.TicksPerSecond;// Engine.TargetFps / 10f;// 8f;
            }
        }
        public byte SkyDarkness = 0, SkyDarknessMax = 13;
        public Color AmbientColor = Color.Blue;//Color.MidnightBlue; //Color.RoyalBlue;//Color.MidnightBlue; //Color.MediumPurple; //Color.Lerp(Color.White, Color.Cornsilk, 0.5f);
        //internal void Spawn(Entity obj)
        //{
        //    this.Spawn(obj, obj.Global, obj.Velocity);
        //}

        
        internal void Despawn(Actor actor)
        {
            actor.Despawn();
            PacketEntityDespawn.Send(this.Net, actor);
        }
        //public bool Fog = false, BorderShading = true;
        //public bool
        //    HideUnderground = false,//true,
        //    HideOverground = false;
        public static int  VisibleCellCount = 0;//SeaLevel,
        public Dictionary<int, Rectangle[,]> BaseTileRegions;
        public Game1 game;
        public bool hasClicked = false;
        //public static int MaxHeight = 128;//256; //
        public static float MaxDepth = 0, MinDepth = 0;
        static public Texture2D TerrainSprites, CharacterSprites, ShaderMouseMap, BlockDepthMap;//, BlockDepthMapFlatTop;
        public List<Texture2D> VisibleTileTypes;
        public Vector2 tileLocation = new(16, 8);
        public const double GroundDensity = 0.1; //0.2
        static public Texture2D Shadow;
        static public List<Rectangle> Icons;
        static public Texture2D ItemSheet;

        static public void Initialize()
        {
           // content = Game1.Instance.Content;
            Generator.InitGradient3();
            TerrainSprites = Game1.Instance.Content.Load<Texture2D>("Graphics/spritesheet cubes");// New");//
            CharacterSprites = Game1.Instance.Content.Load<Texture2D>("Graphics/Characters/best/best2");
            ShaderMouseMap = Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap - Cube");
            BlockDepthMap = Game1.Instance.Content.Load<Texture2D>("Graphics/blockDepth09");
            //BlockDepthMapFlatTop = Game1.Instance.Content.Load<Texture2D>("Graphics/blockDepth09flatTop");
            Shadow = Game1.Instance.Content.Load<Texture2D>("Graphics/shadow");

            //Block.Initialize();
            //Cell.Initialize();

            

            ItemSheet = Game1.Instance.Content.Load<Texture2D>("Graphics/ItemSheet");
            ItemSheet.Name = "Default item sprites";
            int iconsH = ItemSheet.Width / 32, iconsV = ItemSheet.Height / 32;
            Icons = new List<Rectangle>(iconsH * iconsV);
            for (int j = 0; j < iconsV; j++)
                for (int i = 0; i < iconsH; i++)
                    Icons.Add(new Rectangle(i * 32, j * 32, 32, 32));


            //Cell.Initialize();


            
        }

        //public StaticWorld World;// { get { return (World)this["World"]; } set { this["World"] = value; } }
        public string Name;
        public Texture2D[] Thumbnails;
        public MapThumb Thumb;
       // public DirectoryInfo DirectoryInfo { get { return (DirectoryInfo)this["DirectoryInfo"]; } set { this["DirectoryInfo"] = value; } }
        #region Initialization

        public Vector2 Global;// { get { return Coordinates * SizeInBlocks;}}// (Engine.ChunkRadius * 2 + 1); } }

        public StaticMap(string name = "")
        {
            this.LightingEngine = new LightingEngine(this);
            this.Camera = new Camera((int)Game1.ScreenSize.X, (int)Game1.ScreenSize.Y);
            this.Name = name; 
            //this.Clock = new TimeSpan(Zenith, 0, 0);
            //ActiveChunks = new ConcurrentDictionary<Vector2, Chunk>();
            this.ActiveChunks = new Dictionary<Vector2, Chunk>();
            ChunksToLight = new Queue<Chunk>();
            ChunksToActivate = new Queue<Chunk>();
            this.Thumbnails = new Texture2D[3];
            this.Town = new Town(this);
            this.Regions = new RegionManager(this);
            this.UndiscoveredAreaManager = new UndiscoveredAreaManager(this);
            this.ParticleManager = new Particles.ParticleManager(this);
        }
        public StaticMap(StaticWorld world, Vector2 coords, string name = "")
            : this(name)
        {
            this.World = world;
            this.Coordinates = coords;
            this.Size = MapSize.Default;// MapSize.Normal;
            this.Global = Coordinates * this.Size.Blocks;
            this.Thumb = new MapThumb(this);
        }
        public StaticMap(StaticWorld world, string name, Vector2 coords, MapSize size)
            : this(name)
        {
            this.World = world;
            this.Coordinates = coords;
            this.Size = size;
            this.Global = Coordinates * this.Size.Blocks;
            this.Thumb = new MapThumb(this);
        }

        #endregion
        
        public Rectangle GetRandomTile(int type)
        {
            //return BaseTileRegions[type][rand.Next(BaseTileRegions[type].Count - 1)];
            return BaseTileRegions[type][World.Random.Next(BaseTileRegions[type].Length - 1), 0];
        }
        float t = 0;
        #region Updating
        public override void Update(IObjectProvider net)//GameTime gt, Camera camera)
        {
            IconOffset = (float)Math.Sin(net.Clock.TotalMilliseconds / Engine.TicksPerSecond);// DateTime.Now.Second);
            this.Update();
        }
        public void Update()//GameTime gt, Camera camera)
        {
            TryPerformQueuedRandomBlockUpdates();
            this.CachedAmbientColor = this.UpdateAmbientColor();

            this.CacheObjects();
            this.CacheBlockEntities();

            while (ChunksToActivate.Count > 0)
            {
                Chunk chunk = this.ChunksToActivate.Dequeue();
                //if (!this.ChunksToActivate.TryDequeue(out chunk))
                //    continue;
                ActiveChunks[chunk.MapCoords] = chunk;
                if (ActiveChunks.Count > ChunkLoader.ChunkMemoryCapacity)
                {
                    float maxDist = 0, dist;
                    var furthestChunkCoords = new Vector2();
                    foreach (KeyValuePair<Vector2, Chunk> pair in ActiveChunks)
                    {
                        dist = Vector2.Distance(chunk.MapCoords, pair.Key);
                        if (dist > maxDist)
                        {
                            maxDist = dist;
                            furthestChunkCoords = pair.Key;
                        }
                    }
                    //Chunk furthestChunk;
                    ActiveChunks.Remove(furthestChunkCoords);
                    ChunkLoader.UnloadChunk(furthestChunkCoords);
                }
            }
            //if(this.Running)
            foreach (var chunk in this.ActiveChunks.Values.ToList())
                chunk.Update();
            this.Town.Update();
            Block.UpdateBlocks(this);
            this.ApplyLightChanges();
            //PathFinding.PathingSync.UpdateInaccessibleAreas();
        }

        private void TryPerformQueuedRandomBlockUpdates()
        {
            while (this.RandomBlockUpdateQueue.Any())
            {
                var global = this.RandomBlockUpdateQueue.Peek();
                var cell = this.GetCell(global);
                if (cell == null)
                    continue;
                cell.Block.RandomBlockUpdate(this.Net, global, cell);
                this.RandomBlockUpdateQueue.Dequeue();
            }
        }
        public override void Tick(IObjectProvider net)
        {
            AdvanceTime();
            this.Regions.Update();
            foreach (var chunk in this.ActiveChunks.Values.ToList())
                chunk.Tick(this);
            this.Town.Tick();
        }

        
        public bool Running = true;

        float WaterAnim = 20;
        private void AnimateWater()
        {
            WaterAnim -= 1;// GlobalVars.DeltaTime;
            if (WaterAnim <= 0)
            {
                WaterAnim = 20;
                //Sprite water = Block.TileSprites[Block.Types.Water];
                //Rectangle[][] sources = water.SourceRects;
                //water.SourceRects = new Rectangle[][] { new Rectangle[] { sources[0][1], sources[0][2], sources[0][3], sources[0][0] } };
            }
        }

        #endregion

        public Queue<Chunk> ChunksToLight;

        public int TilesDrawn, TileOutlinesDrawn, ObjectsDrawn, ChunksDrawn, CullingChecks;
       
        #region Drawing

        //struct ChunkTexture
        //{
        //    public RenderTarget2D Bitmap;
        //    public RenderTarget2D Depth;
        //    public RenderTarget2D Light;
        //    public RenderTarget2D Mouse;
        //    public Color[] MouseMap;
        //}

        //Dictionary<Chunk, ChunkTexture> ChunkSprites = new Dictionary<Chunk, ChunkTexture>();
        public bool Redraw;

        //public bool RefreshChunk(Chunk chunk)
        //{
        //    return ChunkSprites.Remove(chunk);
        //}

        public WorldPosition Mouseover;

        public override void DrawBlocks(MySpriteBatch sb, Camera camera, EngineArgs a)//, SelectionArgs selection)
        {
            Redraw = false;
            ChunksDrawn = 0;
            TilesDrawn = 0;
            TileOutlinesDrawn = 0;
            ObjectsDrawn = 0;
            CullingChecks = 0;
            var copyOfActiveChunks = new Dictionary<Vector2, Chunk>(ActiveChunks);
            Vector3? playerGlobal = null;// = new Nullable<Vector3>(Player.Actor != null ? Player.Actor.Global : null);
            var hiddenRects = new List<Rectangle>();
            if (PlayerOld.Actor != null)
            {
                if (PlayerOld.Actor.IsSpawned)
                {
                    playerGlobal = new Nullable<Vector3>(PlayerOld.Actor.Global.RoundXY());
                    Sprite sprite = PlayerOld.Actor.GetSprite();// (Sprite)Player.Actor["Sprite"]["Sprite"];
                    Rectangle spriteBounds = sprite.GetBounds(); // make bounds a field
                    //spriteBounds.Inflate(spriteBounds.Width, spriteBounds.Height);
                    Rectangle screenBounds = camera.GetScreenBounds(playerGlobal.Value, spriteBounds);//, spriteBounds.Center.ToVector());
                    hiddenRects.Add(screenBounds);
                    //var playerpos = camera.GetScreenPositionFloat(playerGlobal.Value);
                    //var screenBounds = new Rectangle((int)(playerpos.X - 32 * camera.Zoom), (int)(playerpos.Y - 32 * camera.Zoom), (int)(64 * camera.Zoom), (int)(64 * camera.Zoom));
                    //hiddenRects.Add(screenBounds);
                }
            }
            //playerGlobal = Player.Actor != null ? new Nullable<Vector3>(Player.Actor.Global) : null;
            //playerGlobal.Value = Player.Actor != null ? Player.Actor.Global : null;
            // Mouseovers = new SortedList<float, Vector3>();
            camera.UpdateMaxDrawLevel();
            //Mouseover.Clear();
            Mouseover = null;
            //int d = 0;
            Towns.Housing.House house = null;
            if (PlayerOld.Actor != null)
                house = this.Town.GetHouseAt(PlayerOld.Actor.Global);
            if(house!=null)
            {
                foreach(var cell in house.GetVisibleBlocks(camera))
                    camera.DrawCell(sb, this, this.GetChunk(cell.Key), cell.Value, playerGlobal, hiddenRects, a);
            }
            else
            foreach (KeyValuePair<Vector2, Chunk> chunk in copyOfActiveChunks)//.OrderBy(foo => foo.Value.GetDepthFar(camera))) //Depth))
            {

                Rectangle chunkBounds = camera.GetScreenBounds(chunk.Value.Start.X + Chunk.Size / 2, chunk.Value.Start.Y + Chunk.Size / 2, this.MaxHeight / 2, Chunk.Bounds);  //chunk.Value.GetBounds(camera);
                if (!camera.ViewPort.Intersects(chunkBounds))
                    continue;
                camera.DrawChunk(sb, this, chunk.Value, playerGlobal, hiddenRects, a);

                // draw front of frontmost chunks here?
                //DrawFrontmostBlocks(sb, camera, a, playerGlobal, hiddenRects, chunk);
                // draw chunk faces first because otherwise a fucking glitch appears on topmost blocks cause of the draw order or something

                //d++;

                
            }
            //d.ToConsole();
            if (Mouseover != null)
                camera.CreateMouseover(this, Mouseover.Global);
            //else
            //        "asdasd".ToConsole();
            //camera.Mouseover(new Queue<Position>(Mouseover.Values.ToList()));
            //Queue<Position> qms = new Queue<Position>(Mouseover.Values.ToList());
            //while (Mouseover.Count > 0 && Controller.Instance.MouseoverNext.Object == null)
            //    camera.Mouseover(Mouseover.Dequeue());
        }

        internal void SpawnStartingActors(Actor[] actors)
        {
            var x = this.Size.Blocks / 2;
            var y = x;
            var z = this.GetHeightmapValue(x, y);
            var center = new IntVec3(x, y, z);
            //var vec = center;
            var radial = VectorHelper.GetRadialLarge(center).GetEnumerator();
            for (int i = 0; i < actors.Length; i++)
            {
                var actor = actors[i];
                IntVec3 current; 
                do
                {
                    radial.MoveNext();
                    current = radial.Current;
                } while (!this.IsStandableIn(current));
                //while (!this.IsStandableIn(current))
                //{
                //    radial.MoveNext();
                //    current = radial.Current;
                //}
                actor.Spawn(this, current);
            }
        }

        //private void DrawFrontmostBlocksSortFirst(MySpriteBatch sb, Camera camera, EngineArgs a, Vector3? playerGlobal, List<Rectangle> hiddenRects, KeyValuePair<Vector2, Chunk> chunk)
        //{
        //    var chunkX = (int)chunk.Key.X;
        //    var chunkY = (int)chunk.Key.Y;
        //    var sorted = new SortedSet<int>();
        //    int edgeX = 0, edgeY = 0;
        //    switch ((int)camera.Rotation)
        //    {
        //        case 0:
        //            edgeX = MapSize.Default.Chunks - 1;
        //            edgeY = MapSize.Default.Chunks - 1;
        //            break;
        //        case 1:
        //            edgeX = MapSize.Default.Chunks - 1;
        //            edgeY = 0;
        //            break;
        //        case 2:
        //            edgeX = 0;
        //            edgeY = 0;
        //            break;
        //        case 3:
        //            edgeX = 0;
        //            edgeY = MapSize.Default.Chunks - 1;
        //            break;
        //        default:
        //            break;
        //    }
        //    var maxheight = this.GetMaxHeight();

        //    if (chunkX == edgeX)// || chunk.Key.Y == this.Size.Chunks)
        //    {
        //        for (int i = 0; i < Chunk.Size; i++)
        //            for (int j = 0; j < maxheight; j++)
        //            {

        //                //for (int j = maxheight - 1; j >= 0; j--)
        //                {
        //                    Cell cell;
        //                    //Vector3 pos;
        //                    Vector3 pos = new Vector3(Chunk.Size - 1, i, j);
        //                    switch ((int)camera.Rotation)
        //                    {
        //                        case 0:
        //                        case 1:
        //                            pos = new Vector3(Chunk.Size - 1, i, j);
        //                            break;

        //                        //case 3:
        //                        //    pos = new Vector3(i, Chunk.Size - 1, j);
        //                        //    break;
        //                        case 2:
        //                        case 3:
        //                            pos = new Vector3(0, i, j);
        //                            break;

        //                        //case 1:
        //                        //    pos = new Vector3(i, 0, j);
        //                        //    break;

        //                        default:
        //                            break;
        //                    }
        //                    var cellIndex = Chunk.GetCellIndex((int)pos.X, (int)pos.Y, (int)pos.Z);// Chunk.FindIndex(pos); // FASTER WITH INTS
        //                    cell = chunk.Value[cellIndex];
        //                    if (cell.Block.Type == Block.Types.Air)
        //                        continue;
        //                    if (chunk.Value.VisibleOutdoorCells.ContainsKey(cellIndex)) // TODO: SLOW!!! OPTIMIZE
        //                        continue;
        //                    sorted.Add(cellIndex);
        //                    //camera.DrawCell(sb, this, chunk.Value, cell, playerGlobal, hiddenRects, a);
        //                    //Block.Soil.Draw(sb, screenBounds, light.Sun, light.Block, this.Zoom, cd, cell); // CURRENT WORKING ONE
        //                }
        //            }
        //    }
        //    if (chunkY == edgeY)// || chunk.Key.Y == this.Size.Chunks)
        //    {
        //        for (int i = 0; i < Chunk.Size; i++)
        //            for (int j = 0; j < maxheight; j++)
        //            {

        //                //for (int j = maxheight - 1; j >= 0; j--)
        //                {
        //                    Cell cell;
        //                    Vector3 pos = new Vector3(i, Chunk.Size - 1, j);
        //                    switch ((int)camera.Rotation)
        //                    {
        //                        case 0:
        //                        case 3:
        //                            pos = new Vector3(i, Chunk.Size - 1, j);
        //                            break;

        //                        //case 3:
        //                        //    pos = new Vector3(0, i, j);
        //                        //    break;

        //                        case 2:
        //                        case 1:
        //                            pos = new Vector3(i, 0, j);
        //                            break;

        //                        //case 1:
        //                        //    pos = new Vector3(Chunk.Size - 1, i, j);
        //                        //    break;

        //                        default:
        //                            break;
        //                    }
        //                    //var cellIndex = Chunk.FindIndex(pos);
        //                    var cellIndex = Chunk.GetCellIndex((int)pos.X, (int)pos.Y, (int)pos.Z);// Chunk.FindIndex(pos); // FASTER WITH INTS

        //                    cell = chunk.Value[cellIndex];
        //                    if (cell.Block.Type == Block.Types.Air)
        //                        continue;
        //                    if (chunk.Value.VisibleOutdoorCells.ContainsKey(cellIndex))
        //                        continue;
        //                    sorted.Add(cellIndex);
        //                    //camera.DrawCell(sb, this, chunk.Value, cell, playerGlobal, hiddenRects, a);
        //                    //Block.Soil.Draw(sb, screenBounds, light.Sun, light.Block, this.Zoom, cd, cell); // CURRENT WORKING ONE
        //                }
        //            }
        //    }
        //    foreach (var index in sorted)
        //    {
        //        var cell = chunk.Value.CellGrid2[index];
        //        camera.DrawCell(sb, this, chunk.Value, cell, playerGlobal, hiddenRects, a);
        //    }
        //}

        private void DrawFrontmostBlocks(MySpriteBatch sb, Camera camera, EngineArgs a, Vector3? playerGlobal, List<Rectangle> hiddenRects, KeyValuePair<Vector2, Chunk> chunk)
        {
            var chunkX = (int)chunk.Key.X;
            var chunkY = (int)chunk.Key.Y;

            int edgeX = 0, edgeY = 0;
            switch((int)camera.Rotation)
            {
                case 0:
                    edgeX = MapSize.Default.Chunks - 1;
                    edgeY = MapSize.Default.Chunks - 1;
                    break;
                case 1:
                    edgeX = MapSize.Default.Chunks - 1;
                    edgeY = 0;
                    break;
                case 2:
                    edgeX = 0;
                    edgeY = 0;
                    break;
                case 3:
                    edgeX = 0;
                    edgeY = MapSize.Default.Chunks - 1;
                    break;
                default:
                    break;
            }
            var maxheight = this.GetMaxHeight();

            if (chunkX == edgeX)// || chunk.Key.Y == this.Size.Chunks)
            {
                for (int i = 0; i < Chunk.Size; i++)
                    for (int j = 0; j < maxheight; j++)
                {
                    
                    //for (int j = maxheight - 1; j >= 0; j--)
                    {
                        Cell cell;
                        //Vector3 pos;
                        var pos = new Vector3(Chunk.Size - 1, i, j);
                        switch((int)camera.Rotation)
                        {
                            case 0:
                            case 1:
                                pos = new Vector3(Chunk.Size - 1, i, j);
                                break;

                            //case 3:
                            //    pos = new Vector3(i, Chunk.Size - 1, j);
                            //    break;
                            case 2:
                            case 3:
                                pos = new Vector3(0, i, j);
                                break;

                            //case 1:
                            //    pos = new Vector3(i, 0, j);
                            //    break;

                            default:
                                break;
                        }
                        var cellIndex = Chunk.GetCellIndex((int)pos.X, (int)pos.Y, (int)pos.Z);// Chunk.FindIndex(pos); // FASTER WITH INTS
                        cell = chunk.Value[cellIndex];
                        if (cell.Block.Type == Block.Types.Air)
                            continue;
                        // redraw visible cells to prevent glitchy black lines
                        // TODO: find way to prevent glitchy black lines without redrawing blocks
                        //if (chunk.Value.VisibleOutdoorCells.ContainsKey(cellIndex)) // TODO: SLOW!!! OPTIMIZE
                        //    continue;
                        camera.DrawCell(sb, this, chunk.Value, cell, playerGlobal, hiddenRects, a);
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

                            //case 3:
                            //    pos = new Vector3(0, i, j);
                            //    break;

                            case 2:
                            case 1:
                                pos = new Vector3(i, 0, j);
                                break;

                            //case 1:
                            //    pos = new Vector3(Chunk.Size - 1, i, j);
                            //    break;

                            default:
                                break;
                        }
                        //var cellIndex = Chunk.FindIndex(pos);
                        var cellIndex = Chunk.GetCellIndex((int)pos.X, (int)pos.Y, (int)pos.Z);// Chunk.FindIndex(pos); // FASTER WITH INTS

                        cell = chunk.Value[cellIndex];
                        if (cell.Block.Type == Block.Types.Air)
                            continue;
                        // redraw visible cells to prevent glitchy black lines
                        // TODO: find way to prevent glitchy black lines without redrawing blocks
                        //if (chunk.Value.VisibleOutdoorCells.ContainsKey(cellIndex))
                        //    continue;

                        camera.DrawCell(sb, this, chunk.Value, cell, playerGlobal, hiddenRects, a);
                        //Block.Soil.Draw(sb, screenBounds, light.Sun, light.Block, this.Zoom, cd, cell); // CURRENT WORKING ONE
                    }
                }
            }
        }

        public bool TryGetBlockObject(Vector3 global, out GameObject blockObj)
        {
            //return (global.GetChunk(this).TryGetBlockObject(global.ToLocal().Round(), out blockObj));
            return (this.GetChunk(global).TryGetBlockObject(global.ToLocal().RoundXY(), out blockObj));

            //return (global.GetChunk(this).TryGetBlockObject(global.Round(), out blockObj));
        }
        //public bool GetBlockObjectOrDefault(Vector3 global, out GameObject blockObj)
        //{
        //    Vector3 rnd = global.RoundXY();
        //    Chunk chunk;
        //    Cell cell;
        //    //if (!Position.TryGet(this, rnd, out cell, out chunk))
        //    if(!this.TryGetAll(rnd, out chunk, out cell))
        //    {
        //        blockObj = null;
        //        return false;
        //    }
        //    //if (cell.TileType == Tile.Types.Farmland)
        //    //    "test".ToConsole();
        //    // if the block object exists, assign it and return true
        //    if (chunk.TryGetBlockObject(rnd.ToLocal(), out blockObj))
        //        return true;

        //    // otherwise, create a new block object
        //    if (!BlockComponent.Blocks.ContainsKey(cell.Block.Type))
        //    {
        //        blockObj = null;
        //        return false;
        //    }
        //    return false;
        //    //return Cell.TryGetObject(this, global, out blockObj);

        //}

       


        public override void DrawObjects(MySpriteBatch sb, Camera camera, SceneState scene)
        {
            foreach (KeyValuePair<Vector2, Chunk> chunk in ActiveChunks)
            {
                Rectangle chunkBounds = camera.GetScreenBounds(chunk.Value.Start.X + Chunk.Size / 2, chunk.Value.Start.Y + Chunk.Size / 2, this.MaxHeight / 2, Chunk.Bounds);  //chunk.Value.GetBounds(camera);
                if (camera.ViewPort.Intersects(chunkBounds))
                    chunk.Value.DrawObjects(sb, camera, Controller.Instance, PlayerOld.Instance, this, scene);//, globaldMin, globaldMax);
            }
        }

        //public void DrawObjects(SpriteBatch sb, Camera camera, SceneState scene)
        //{
        //    Dictionary<Vector2, Chunk> copyOfActiveChunks = new Dictionary<Vector2, Chunk>(ActiveChunks);
        //    //float globaldNear = 0, globaldFar = 1;
        //    //foreach (KeyValuePair<Vector2, Chunk> chunk in copyOfActiveChunks.OrderBy(foo => foo.Value.GetDepthFar(camera))) //Depth))
        //    //{
        //    //    globaldFar = Math.Min(chunk.Value.GetDepthFar(camera), globaldFar);
        //    //    globaldNear = Math.Max(chunk.Value.GetDepthNear(camera), globaldNear);
        //    //    //Console.WriteLine(dMin + " " + dMax);
        //    //}

        //    foreach (KeyValuePair<Vector2, Chunk> chunk in copyOfActiveChunks)//.OrderBy(foo => foo.Value.GetDepthFar(camera))) //Depth))
        //    {
                

        //        //if (chunk.Value.Visible(camera))
        //        Rectangle chunkBounds = camera.GetScreenBounds(chunk.Value.Start.X + Chunk.Size / 2, chunk.Value.Start.Y + Chunk.Size / 2, Map.MaxHeight / 2, chunk.Value.GetBounds());  //chunk.Value.GetBounds(camera);
        //        if (camera.ViewPort.Intersects(chunkBounds))
        //        {
        //            //chunk.Value.DrawHighlight(camera, chunkBounds);
        //            //float localdNear, localdFar;
        //            //chunk.Value.GetLocalDepthRange(camera, out localdNear, out localdFar);

        //            //Game1.Instance.Effect.Parameters["NearDepth"].SetValue(localdNear);
        //            //Game1.Instance.Effect.Parameters["FarDepth"].SetValue(localdFar);
        //            chunk.Value.DrawObjects(sb, camera, Controller.Instance, Player.Instance, this, scene);//, globaldMin, globaldMax);
        //        }
        //        Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
        //    }
        //    Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));

        //    //DrawTileSelection(camera);

        //}

        public override void DrawInterface(SpriteBatch sb, Camera camera)//, SceneState state)
        {
            Dictionary<Vector2, Chunk> copyOfActiveChunks = new Dictionary<Vector2, Chunk>(ActiveChunks);
            foreach (KeyValuePair<Vector2, Chunk> chunk in copyOfActiveChunks)//.OrderBy(foo => foo.Value.GetDepthFar(camera))) //Depth))
            {


                //if (chunk.Value.Visible(camera))
                Rectangle chunkBounds = camera.GetScreenBounds(chunk.Value.Start.X + Chunk.Size / 2, chunk.Value.Start.Y + Chunk.Size / 2, this.MaxHeight / 2, Chunk.Bounds);  //chunk.Value.GetBounds(camera);
                if (camera.ViewPort.Intersects(chunkBounds))
                {
                    //chunk.Value.DrawHighlight(camera, chunkBounds);
                    //float localdNear, localdFar;
                    //chunk.Value.GetLocalDepthRange(camera, out localdNear, out localdFar);

                    //Game1.Instance.Effect.Parameters["NearDepth"].SetValue(localdNear);
                    //Game1.Instance.Effect.Parameters["FarDepth"].SetValue(localdFar);
                    chunk.Value.DrawInterface(sb, camera);//, scene);
                }
                Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
            }
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
            this.Town.DrawUI(sb, camera);
        }


        #endregion
        private void Chunk_VisibleChanged(object sender, EventArgs e)
        {
            OnVisibleChunksChanged();
        }
        public event EventHandler<EventArgs> VisibleChunksChanged;
        private void OnVisibleChunksChanged()
        {
            VisibleChunksChanged?.Invoke(this, EventArgs.Empty);
        }
        public Vector2 TranslateCellToChunk(int x, int y)
        {
            return new Vector2((float)Math.Floor(x / (float)Chunk.Size), (float)Math.Floor(y / (float)Chunk.Size));
        }

        public Chunk FindChunkAt(int x, int y)
        {
            //int size = Size / Chunk.Size;

            //if (x >= 0)
            //    if (x < Size)
            //        if (y >= 0)
            //            if (y < Size)
            //                return Chunks[x / Chunk.Size, y / Chunk.Size];

            //Vector2 vec = new Vector2(x, y);
            var vec = new Vector2((float)Math.Floor(x / (float)Chunk.Size), (float)Math.Floor(y / (float)Chunk.Size));
            //Vector2 vec = new Vector2(x / Chunk.Size, y / Chunk.Size);
            if (!ActiveChunks.ContainsKey(vec))
                return null;
            return ActiveChunks[vec];
        }
        public void Dispose()
        {
            this.ActiveChunks = new Dictionary<Vector2, Chunk>();
            ChunksToActivate = new Queue<Chunk>();
        }
        public override string GetFullPath()
        {
            return GlobalVars.SaveDir + @"Worlds\Static\" + World.Name + @" \" + GetFolderName() + @"\";
        }
        public override string GetFolderName()
        {
            return this.Coordinates.X.ToString() + "." + this.Coordinates.Y.ToString();
        }

        public void ForceSaveChunks()
        {
            foreach (var chunk in this.ActiveChunks.Values)
                chunk.SaveToFile();
        }
        

        public override SaveTag Save()
        {
            return this.SaveToTag();
            ChunkLoader.Paused = true;
            //ChunkLighter.Paused = true;

            //string directory = GlobalVars.SaveDir + @"/Worlds/" + this["Name"] + "/";// Directory.GetCurrentDirectory() + @"/Saves/";
            //string worldPath = @"/Saves/Worlds/" + this["Name"] + "/";
            //string fullPath = worldPath + this["Name"] + ".map.sat";
            string mapDir = @"/Saves/Worlds/Static/" + this.World.Name + "/" + GetFolderName() + "/";
            string fullMapDir = GlobalVars.SaveDir + @"/Worlds/Static/" + this.World.Name + "/" + GetFolderName() + "/"; // Directory.GetCurrentDirectory() + @"/Saves/";
            string fullFileName = mapDir + GetFolderName() + ".map.sat";
            SaveTag mapTag;
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);

                mapTag = SaveToTag();
                //mapTag.WriteTo(writer);
                //mapTag.WriteTo(writer);
                mapTag.WriteWithRefs(writer);
                if (!Directory.Exists(fullMapDir))
                    Directory.CreateDirectory(fullMapDir);

                Chunk.Compress(stream, Directory.GetCurrentDirectory() + fullFileName);

                stream.Close();

            }

            // don't save thumbnail until i figure out how to organize regions
            //GenerateThumbnails(fullMapDir);

            foreach (KeyValuePair<Vector2, Chunk> pair in ActiveChunks)
            {
                //pair.Value.Save();
                throw new Exception();
            }
            ChunkLoader.Paused = false;

            return mapTag;
        }

        public SaveTag SaveToTag()
        {
            SaveTag mapTag;
            mapTag = new SaveTag(SaveTag.Types.Compound, "Map");

            mapTag.Add(new SaveTag(SaveTag.Types.Int, "X", (int)this.Coordinates.X));
            mapTag.Add(new SaveTag(SaveTag.Types.Int, "Y", (int)this.Coordinates.Y));
            mapTag.Add(new SaveTag(SaveTag.Types.String, "Name", this.Name));
            this.CurrentTick.Save(mapTag, "CurrentTick");
            mapTag.Add(this.Town.Save("Town"));

            mapTag.Add(new SaveTag(SaveTag.Types.String, "Size", this.Size.Name));

            SaveTag playerTag = new SaveTag(SaveTag.Types.Compound, "Player");
            mapTag.Add(playerTag);

            var chunkstags = this.SaveChunks();
            mapTag.Add(chunkstags);

            var sw = Stopwatch.StartNew();
            mapTag.Add(this.UndiscoveredAreaManager.Save("UndiscoveredAreas"));
            sw.Stop();
            string.Format("undiscovered areas saved in {0} ms", sw.ElapsedMilliseconds).ToConsole();
            sw.Stop();

            mapTag.Add(this.RandomOrderedChunkIndices.Save("_RandomOrderedChunkIndices")); //save the property to force it to initialize if it's not already

            return mapTag;
        }
        private SaveTag SaveChunks()
        {
            var chunkstags = new SaveTag(SaveTag.Types.List, "Chunks", SaveTag.Types.Compound);
            foreach (var ch in this.ActiveChunks)
            {
                var chtag = new SaveTag(SaveTag.Types.Compound);
                chtag.Add(ch.Key.Save("Key"));
                chtag.Add(ch.Value.SaveToTag());
                chunkstags.Add(chtag);
            }
            return chunkstags;
        }
        public void LoadChunks(SaveTag tag)
        {
            var list = tag.Value as List<SaveTag>;
            foreach(var item in list)
            {
                var key = item["Key"].LoadVector2();
                var chunk = Chunk.Load(this, key, item["Chunk"]);
                this.ActiveChunks.Add(key, chunk);
            }
            this.InitChunks();
        }

        public override void GenerateThumbnails()
        {
            //string fullMapDir = this.World.GetPath() + GetFolderName() + "/";

            //this.GenerateThumbnails(fullMapDir);
            this.GenerateThumbnails(this.GetFullPath());
        }
        public override void GenerateThumbnails(string fullMapDir)//DirectoryInfo mapDir)//
        {
           // string fullMapDir = mapDir.FullName;
            if (!Directory.Exists(fullMapDir))
                Directory.CreateDirectory(fullMapDir);
            if (ActiveChunks.Count > 0)
            {
                using Texture2D thumbnail = GetThumbnail();
                //using (FileStream stream = new FileStream(Directory.GetCurrentDirectory() + worldPath + "thumbnailSmall.png", FileMode.OpenOrCreate))
                using (FileStream stream = new FileStream(fullMapDir + "thumbnailSmall.png", FileMode.OpenOrCreate))
                {
                    thumbnail.SaveAsPng(stream, thumbnail.Width, thumbnail.Height);
                    stream.Close();
                }
                //using (FileStream stream = new FileStream(Directory.GetCurrentDirectory() + worldPath + "thumbnailSmaller.png", FileMode.OpenOrCreate))
                using (FileStream stream = new FileStream(fullMapDir + "thumbnailSmaller.png", FileMode.OpenOrCreate))
                {
                    thumbnail.SaveAsPng(stream, thumbnail.Width / 2, thumbnail.Height / 2);
                    stream.Close();
                }
                //using (FileStream stream = new FileStream(Directory.GetCurrentDirectory() + worldPath + "thumbnailSmallest.png", FileMode.OpenOrCreate))
                using (FileStream stream = new FileStream(fullMapDir + "thumbnailSmallest.png", FileMode.OpenOrCreate))
                {
                    thumbnail.SaveAsPng(stream, thumbnail.Width / 4, thumbnail.Height / 4);
                    stream.Close();
                }
            }
        }
        public static StaticMap Load(StaticWorld world, Vector2 coords, SaveTag mapTag)
        {
            var map = new StaticMap(world, coords);
            map.Name = (string)mapTag["Name"].Value;
            map.Coordinates = new Vector2((int)mapTag["X"].Value, (int)mapTag["Y"].Value);

            //map.Time = TimeSpan.FromSeconds((double)mapTag["Time"].Value);
            //mapTag.TryGetTagValueNew("CurrentTick", ref map.CurrentTick);
            //mapTag.TryGetTagValue<double>("CurrentTick", v => map.CurrentTick = (ulong)v);

            // DONT LOAD PLAYER todo: remove player saving from map
            SaveTag playerTag = mapTag["Player"] as SaveTag;
            var tagList = playerTag.Value as Dictionary<string, SaveTag>;
            //List<SaveTag> tagList = playerTag.Value as List<SaveTag>;
            if (tagList.Count > 1)
            {
                //GameObject player;
                //Dictionary<string, SaveTag> byName = playerTag.ToDictionary();
                //player = GameObject.Create(byName.First().Value);
                //player.Name = byName.First().Key;
                ////map["Player"] = player;
                //map.SavedPlayers.Add(player);
                foreach (var tag in tagList.Values)
                {
                    if (tag.Type == 0)
                        continue;
                    var player = GameObject.Load(tag);
                    map.SavedPlayers.Add(player);
                }
            }


            //this.Name = (string)mapTag["Name"].Value;

            mapTag.TryGetTagValue<string>("Size", txt => map.Size = MapSize.GetList().First(f => f.Name == txt));

            //mapTag.TryGetTag("Town", tag => map.Town.Load(tag));

            mapTag.TryGetTag("Chunks", map.LoadChunks);
            mapTag.TryGetTag("Town", tag => map.Town.Load(tag)); // LOAD TOWN AFTER CHUNKS because references are resolved pertaining to the map

            mapTag.TryGetTag("UndiscoveredAreas", map.UndiscoveredAreaManager.Load);
            mapTag.TryGetTag("_RandomOrderedChunkIndices", t => map._RandomOrderedChunkIndices = new List<int>().Load(t).ToArray());

            return map;
        }
        //[Obsolete]
        //public StaticMap(StaticWorld world, Vector2 coords, SaveTag mapTag) :this(world, coords)
        //{
        //    this.Name = (string)mapTag["Name"].Value;
        //    this.Coordinates = new Vector2((int)mapTag["X"].Value, (int)mapTag["Y"].Value);

        //    this.Time = TimeSpan.FromSeconds((double)mapTag["Time"].Value);

        //    // DONT LOAD PLAYER todo: remove player saving from map
        //    SaveTag playerTag = mapTag["Player"] as SaveTag;
        //    var tagList = playerTag.Value as Dictionary<string, SaveTag>;
        //    //List<SaveTag> tagList = playerTag.Value as List<SaveTag>;
        //    if (tagList.Count > 1)
        //    {
        //        //GameObject player;
        //        //Dictionary<string, SaveTag> byName = playerTag.ToDictionary();
        //        //player = GameObject.Create(byName.First().Value);
        //        //player.Name = byName.First().Key;
        //        ////map["Player"] = player;
        //        //map.SavedPlayers.Add(player);
        //        foreach (var tag in tagList.Values)
        //        {
        //            if (tag.Type == 0)
        //                continue;
        //            var player = GameObject.Load(tag);
        //            this.SavedPlayers.Add(player);
        //        }
        //    }


        //    //this.Name = (string)mapTag["Name"].Value;

        //    mapTag.TryGetTagValue<string>("Size", txt => this.Size = MapSize.GetList().First(f => f.Name == txt));

        //    mapTag.TryGetTag("Town", tag => this.Town.Load(tag));

        //    mapTag.TryGetTag("Chunks", this.LoadChunks);

        //    mapTag.TryGetTag("UndiscoveredAreas", this.UndiscoveredAreaManager.Load);
        //    mapTag.TryGetTag("_RandomOrderedChunkIndices", t => this._RandomOrderedChunkIndices = new List<int>().Load(t).ToArray());
        //}
        static public StaticMap Load(string filename)
        {
            return new StaticMap();
        }
        static public StaticMap Load(DirectoryInfo mapDir, StaticWorld world, Vector2 coords)
        {
            string filename = mapDir.FullName + @"\" + coords.X.ToString() + "." + coords.Y.ToString() + ".map.sat";
            StaticMap map = new StaticMap(world, coords);// { DirectoryInfo = new DirectoryInfo(mapDir + @"\") };
            DateTime start = DateTime.Now;
            string directory = GlobalVars.SaveDir;
            using (FileStream stream = new FileStream(filename, System.IO.FileMode.Open))
            {
                
                using (MemoryStream decompressedStream = Chunk.Decompress(stream))
                {
                    BinaryReader reader = new BinaryReader(decompressedStream); //stream);//
                    var watch = Stopwatch.StartNew();
                    var mapTag = SaveTag.Read(reader);

                    watch.Stop();
                    string.Format("{0} bytes read in {1} ms", reader.BaseStream.Position, watch.ElapsedMilliseconds).ToConsole();

                    map.Name = (string)mapTag["Name"].Value;
                    map.Coordinates = new Vector2((int)mapTag["X"].Value, (int)mapTag["Y"].Value);

                    //map.Time = TimeSpan.FromSeconds((double)mapTag["Time"].Value);
                    //mapTag.TryGetTagValueNew("CurrentTick", ref map.CurrentTick);
                    //mapTag.TryGetTagValue<double>("CurrentTick", v => map.CurrentTick = (ulong)v);

                    // DONT LOAD PLAYER todo: remove player saving from map
                    SaveTag playerTag = mapTag["Player"] as SaveTag;
                    var tagList = playerTag.Value as Dictionary<string, SaveTag>;
                    //List<SaveTag> tagList = playerTag.Value as List<SaveTag>;
                    if (tagList.Count > 1)
                    {
                        //GameObject player;
                        //Dictionary<string, SaveTag> byName = playerTag.ToDictionary();
                        //player = GameObject.Create(byName.First().Value);
                        //player.Name = byName.First().Key;
                        ////map["Player"] = player;
                        //map.SavedPlayers.Add(player);
                        foreach(var tag in tagList.Values)
                        {
                            if (tag.Type == 0)
                                continue;
                            var player = GameObject.Load(tag);
                            map.SavedPlayers.Add(player);
                        }
                    }


                    map.Name = (string)mapTag["Name"].Value;

                    mapTag.TryGetTagValue<string>("Size", txt => map.Size = MapSize.GetList().First(f => f.Name == txt));

                    mapTag.TryGetTag("Town", tag => map.Town.Load(tag));

                    mapTag.TryGetTag("Chunks", map.LoadChunks);

                    mapTag.TryGetTag("UndiscoveredAreas", map.UndiscoveredAreaManager.Load);

                    mapTag.TryGetTag("_RandomOrderedChunkIndices", t => map._RandomOrderedChunkIndices = new List<int>().Load(t).ToArray());
                }


                stream.Close();
            }
            map.LoadThumbnails(mapDir.FullName);
           // Console.WriteLine("map loaded in " + (DateTime.Now - start).ToString() + " ms");
            return map;
        }
        public override void LoadThumbnails()
        {
            this.LoadThumbnails(this.GetFullPath());
        }
        public void LoadThumbnails(string folderPath)//DirectoryInfo mapDir)
        {
            //try
            //{
                DirectoryInfo mapDir = new DirectoryInfo(folderPath);
                List<FileInfo> thumbFiles = new List<FileInfo>();
                var filenames = new string[] { "thumbnailSmaller.png", "thumbnailSmaller.png", "thumbnailSmallest.png" };

                    foreach (var name in filenames)
                    {
                        var file = new FileInfo(mapDir + "/" + name);
                        //if (!file.Exists)
                        //    throw new FileNotFoundException("File not found", file.Name);
                    }


                //thumbFiles.AddRange(mapDir.GetFiles("thumbnailSmall.png"));
                //thumbFiles.AddRange(mapDir.GetFiles("thumbnailSmaller.png"));
                //thumbFiles.AddRange(mapDir.GetFiles("thumbnailSmallest.png"));

                int i = 0;
           //     this.Thumb = new MapThumb(this);
                foreach (FileInfo thumbFile in thumbFiles)
                    using (FileStream stream = new FileStream(thumbFile.FullName, FileMode.Open))
                    {
                        Texture2D tex = Texture2D.FromStream(Game1.Instance.GraphicsDevice, stream);
                        this.Thumbnails[i] = tex;
                        Thumb.Sprites[i++] = new Sprite(tex, new Rectangle[][] { new Rectangle[] { tex.Bounds } }, tex.Bounds.Center.ToVector());
                    }
            //}
            //catch (Exception) { }
        }
        public bool LoadThumbnails2()//DirectoryInfo mapDir)
        {
            //try
            //{
            DirectoryInfo mapDir = new DirectoryInfo(this.GetFullPath());
            List<FileInfo> thumbFiles = new List<FileInfo>();
            var filenames = new string[] { "thumbnailSmaller.png", "thumbnailSmaller.png", "thumbnailSmallest.png" };

            foreach (var name in filenames)
            {
                var file = new FileInfo(mapDir + "/" + name);
                if (!file.Exists)
                    return false;
            }


            //thumbFiles.AddRange(mapDir.GetFiles("thumbnailSmall.png"));
            //thumbFiles.AddRange(mapDir.GetFiles("thumbnailSmaller.png"));
            //thumbFiles.AddRange(mapDir.GetFiles("thumbnailSmallest.png"));

            int i = 0;
            //     this.Thumb = new MapThumb(this);
            foreach (FileInfo thumbFile in thumbFiles)
                using (FileStream stream = new FileStream(thumbFile.FullName, FileMode.Open))
                {
                    Texture2D tex = Texture2D.FromStream(Game1.Instance.GraphicsDevice, stream);
                    this.Thumbnails[i] = tex;
                    Thumb.Sprites[i++] = new Sprite(tex, new Rectangle[][] { new Rectangle[] { tex.Bounds } }, tex.Bounds.Center.ToVector());
                }
            //}
            //catch (Exception) { }
            return true;
        }
        public List<Chunk> GetChunkNeighbors(Chunk chunk)
        {
            List<Chunk> list = new List<Chunk>(4);
            Chunk neighbor;
            if (ActiveChunks.TryGetValue(chunk.MapCoords + new Vector2(-1, 0), out neighbor))
                list.Add(neighbor);
            if (ActiveChunks.TryGetValue(chunk.MapCoords + new Vector2(1, 0), out neighbor))
                list.Add(neighbor);
            if (ActiveChunks.TryGetValue(chunk.MapCoords + new Vector2(0, -1), out neighbor))
                list.Add(neighbor);
            if (ActiveChunks.TryGetValue(chunk.MapCoords + new Vector2(0, 1), out neighbor))
                list.Add(neighbor);
            return list;
        }

        public bool LoadAllChunks()
        {
            this.ActiveChunks = new Dictionary<Vector2, Chunk>();
            var loader = new ChunkLoader(this);
            var max = this.Size.Chunks;
            for (int i = 0; i < max; i++)
            {
                for (int j = 0; j < max; j++)
                {
                    Chunk ch;
                    var vector = new Vector2(i, j);
                    if (!loader.FromFile(vector, out ch))
                        return false;
                    this.ActiveChunks.Add(vector, ch);
                    ch.ResetVisibleCells(); //uncomment to fix incorrectly detected exposed block faces
                    

                }
            }

            this.Regions.Init();
            this.FinishLoading();
            return true;
        }
        public bool InitChunks(Action<string, float> callback = null)
        {
            callback?.Invoke("Post processing chunks", 0);
            var sw = Stopwatch.StartNew();
            //// TESTING IF REMOVING THIS BREAKS ANYTHING
            ////foreach (var ch in this.ActiveChunks.Values)
            ////    ch.ResetVisibleCells(); //uncomment to fix incorrectly detected exposed block faces
            //sw.Stop();
            //string.Format("visible cells initialized in {0} ms", sw.ElapsedMilliseconds).ToConsole();

            //sw.Restart();
            ResetChunkEdges();
            string.Format("chunk edges reset in {0} ms", sw.ElapsedMilliseconds).ToConsole();

            this.Regions.Init();
            //this.InitUndiscoveredAreas(); // because i load them from the save file afterwards, in the calling method
            callback?.Invoke("Cacheing objects", 0);

            this.FinishLoading();

            return true;
        }
        public IEnumerable<(string, Action)> InitChunksNew()
        {
            yield return ("Post processing chunks", () =>
            {
                var sw = Stopwatch.StartNew();
                //// TESTING IF REMOVING THIS BREAKS ANYTHING
                ////foreach (var ch in this.ActiveChunks.Values)
                ////    ch.ResetVisibleCells(); //uncomment to fix incorrectly detected exposed block faces
                //sw.Stop();
                //string.Format("visible cells initialized in {0} ms", sw.ElapsedMilliseconds).ToConsole();

                //sw.Restart();
                ResetChunkEdges();
                string.Format("chunk edges reset in {0} ms", sw.ElapsedMilliseconds).ToConsole();
            }
            );

            yield return ("Initializing Regions", () =>
            {
                this.Regions.Init();
            }
            );
            yield return ("Caching objects", () =>
            {
                //this.InitUndiscoveredAreas(); // because i load them from the save file afterwards, in the calling method
                this.FinishLoading();
            }
            );
        }
        void ResetChunkEdges()
        {
            foreach (var ch in this.ActiveChunks.Values)
            {
                if (!ch.LightValid)
                {
                    //ResetLight(ch, () =>
                    //{
                    //    ch.LightValid = true;
                    //    // only update neighbor lights if we updated this chunk's light
                    //    UpdateChunkNeighborsLight(ch);
                    //});
                    ResetLight(ch);

                    ch.LightValid = true;
                    UpdateChunkNeighborsLight(ch);
                }
                foreach (var vector in ch.MapCoords.GetNeighbors())
                {
                    Chunk neighbor;
                    if (this.ActiveChunks.TryGetValue(vector, out neighbor))
                        neighbor.InvalidateEdges();
                }
            }
        }
        void UpdateChunkNeighborsLight(Chunk chunk)
        {
            var actives = this.GetActiveChunks();
            Chunk neighbor;
            //if (actives.TryGetValue(chunk.MapCoords + new Vector2(1, 0), out neighbor))
            //    this.LightingEngine.Enqueue(neighbor.GetEdges(Edges.West));
            //if (actives.TryGetValue(chunk.MapCoords + new Vector2(-1, 0), out neighbor))
            //    this.LightingEngine.Enqueue(neighbor.GetEdges(Edges.East));
            //if (actives.TryGetValue(chunk.MapCoords + new Vector2(0, 1), out neighbor))
            //    this.LightingEngine.Enqueue(neighbor.GetEdges(Edges.North));
            //if (actives.TryGetValue(chunk.MapCoords + new Vector2(0, -1), out neighbor))
            //    this.LightingEngine.Enqueue(neighbor.GetEdges(Edges.South));

            if (actives.TryGetValue(chunk.MapCoords + new Vector2(1, 0), out neighbor))
                this.LightingEngine.HandleImmediate(neighbor.GetEdges(Edges.West));
            if (actives.TryGetValue(chunk.MapCoords + new Vector2(-1, 0), out neighbor))
                this.LightingEngine.HandleImmediate(neighbor.GetEdges(Edges.East));
            if (actives.TryGetValue(chunk.MapCoords + new Vector2(0, 1), out neighbor))
                this.LightingEngine.HandleImmediate(neighbor.GetEdges(Edges.North));
            if (actives.TryGetValue(chunk.MapCoords + new Vector2(0, -1), out neighbor))
                this.LightingEngine.HandleImmediate(neighbor.GetEdges(Edges.South));
        }
        void ResetLight(Chunk chunk, Action callback)
        {
            Queue<Vector3> cellList = chunk.ResetHeightMap();
            //this.LightingEngine.Enqueue(cellList, callback);
            this.UpdateLight(cellList, callback);
        }
        void ResetLight(Chunk chunk)
        {
            Queue<Vector3> cellList = chunk.ResetHeightMap();
            //this.LightingEngine.Enqueue(cellList, callback);
            this.UpdateLight(cellList);
        }
        //class ChunkLoadArgs
        //{
        //    public StaticMap Map;
        //    public Vector2 Coords;
        //    public ManualResetEvent ManualResetEvent;
        //    public void WaitCallbackAddChunk(object state)//object state)
        //    {
        //        ChunkLoadArgs a = state as ChunkLoadArgs;
        //        //Map.ActiveChunks.TryAdd(a.Coords, ChunkLoader.Demand(Map, a.Coords));
        //        Map.ActiveChunks.Add(a.Coords, ChunkLoader.Demand(Map, a.Coords));

        //        a.ManualResetEvent.Set();
        //    }
        //}

        /// <summary>
        /// 
        /// </summary>
        /// <param name="state">Vector2 of the chunk coords</param>
        //void WaitCallbackAddChunk(object state)//object state)
        //{
        //    ChunkLoadArgs a = state as ChunkLoadArgs;
        //    this.ActiveChunks.Add(a.Coords, ChunkLoader.Demand(this, a.Coords));
        //    a.ManualResetEvent.Set();
        //}

        private void Activate(Chunk chunk)
        {
            //newChunk.UpdateCellEdges();

            this.ChunksToActivate.Enqueue(chunk);
            //ActiveChunks[chunk.MapCoords] = chunk;
            //ChunksToLight.Enqueue(chunk);
        }
        static public void Load(WorldArgs args) //uint seed)
        {
            foreach (string file in Directory.GetFiles(GlobalVars.SaveDir))
                File.Delete(file);

            StaticMap map = new StaticMap();//args);
        }
        public void HandleInput(InputState input)
        {

        }

        //internal void DrawObjectSelection(SpriteBatch sb, Camera camera)
        //{
        //    //GameObject mouseover;// = Controller.Instance.Mouseover.Object as GameObject;
        //    //if (!Controller.Instance.Mouseover.TryGet<GameObject>(out mouseover))
        //    //{
        //    //    Console.WriteLine(Controller.Instance.Mouseover.Object);
        //    //    return;
        //    //}// Console.WriteLine(mouseover);
            
        //    GameObject mouseover = Controller.Instance.Mouseover.Object as GameObject;
        //   // Console.WriteLine(Controller.Instance.Mouseover.Object);
        //    //Console.WriteLine(mouseover);
        //    if (mouseover.IsNull())
        //        return;

            
        //    //return;

        //    if ((bool)mouseover["Sprite"]["Flash"])
        //        return;

        //    //// TODO: this is also at the chunk.drawobjects
        //    //if ((bool)mouseover["Sprite"]["Hidden"])
        //    //    return;

        //    //Console.WriteLine(mouseover);
        //    if (mouseover.Transform == null)
        //        return;
        //    //Console.WriteLine(mouseover);
        //    Vector3 g = mouseover.Global, off = SpriteComponent.GetOffset((Vector3)mouseover["Sprite"]["Offset"], (double)mouseover["Sprite"]["OffsetTimer"]);//sprComp.GetOffset();
        //    Rectangle bounds;
        //    // camera.CullingCheck(g.X + off.X, g.Y + off.Y, g.Z + off.Z, sprComp.Sprite.GetBounds(), out bounds);
        //    Sprite sprite = (Sprite)mouseover["Sprite"]["Sprite"];
        //    camera.CullingCheck(g.X + off.X, g.Y + off.Y, g.Z + off.Z, sprite.GetBounds(), out bounds);
        //    // TODO: do i not clip the object if out of screen bounds???!?!?
        //    Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
        //    Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);
        //    int variation = (int)mouseover["Sprite"]["Variation"];
        //    mouseover.DrawMouseover(sb, camera);
        //    //sb.Draw(sprite.Texture, screenLoc,
        //    //    sprite.SourceRects[variation][SpriteComponent.GetOrientation((int)mouseover["Sprite"]["Orientation"], camera, sprite.SourceRects[variation].Length)],// sprComp.GetOrientation(camera)], 
        //    //    new Color(255, 255, 255, 127), 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);

        //    Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);

        //}

        public override Texture2D GetThumbnail()
        {
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            float zoom = 1 / 8f;
            //Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            //int width = (int)((Engine.ChunkRadius + Engine.ChunkRadius + 1) * Chunk.Size * Tile.Width * zoom);
            int width = (int)(this.Size.Blocks * Block.Width * zoom);
            Vector2 mapCoords = this.Global;
            var camera = new Camera(width, width, x: mapCoords.X, y: mapCoords.Y, z: this.MaxHeight / 2, zoom: zoom);
            var final = new RenderTarget2D(gd, width, width);
            camera.NewDraw(final, this, gd, EngineArgs.Default, new SceneState(), ToolManager.Instance);
            gd.SetRenderTarget(null);
            return final;
        }

        static public StaticMap Create(WorldArgs a)
        {
            var map = new StaticMap();//a);
            return map;
        }
        static public StaticMap Create(StaticWorld world, Vector2 coords)
        {
            var map = new StaticMap(world, coords);//a);
            world.Maps[coords] = map;
            return map;
        }
        public override void GetTooltipInfo(Tooltip tooltip)
        {
            tooltip.Controls.Add(ToString().ToLabel());
        }

        public override bool Contains(GameObject obj)
        {
            return this.CachedObjects.Contains(obj);
        }

        public override List<GameObject> GetObjects()
        {
            return this.CachedObjects.Where(o => o.IsSpawned).ToList(); // because an object might have despawned during an earlier operation on the current frame
        }

        public void CacheBlockEntities()
        {
            var list = new Dictionary<Vector3, BlockEntity>();
            foreach (var chunk in this.ActiveChunks.Values)
                foreach (var (local, entity) in chunk.GetBlockEntitiesByPosition())
                    list.Add(local.ToGlobal(chunk), entity);
            this.CachedBlockEntities = list;
        }
        public void CacheObjects()
        {
            var list = new List<GameObject>();
            foreach (var chunk in this.ActiveChunks)
                list.AddRange(chunk.Value.GetObjects());
            this.CachedObjects = list;
        }
        public override IEnumerable<GameObject> GetObjects(Vector3 min, Vector3 max)
        {
            return this.GetObjects(new BoundingBox(min, max));

            //List<GameObject> list = new List<GameObject>();
            //Vector2 minChunk, maxChunk;
            //minChunk = min.GetChunkCoords();
            //maxChunk = max.GetChunkCoords();
            //for (float i = minChunk.X; i < maxChunk.X + 1; i++)
            //    for (float j = minChunk.Y; j < maxChunk.Y + 1; j++)
            //    {
            //        var currentChunkCoords = new Vector2(i, j);
            //        Chunk currentChunk;
            //        if (!this.ActiveChunks.TryGetValue(currentChunkCoords, out currentChunk))
            //            continue;
            //        list.AddRange(from obj in currentChunk.GetObjects() 
            //                      where box.Contains(obj.Global) != ContainmentType.Disjoint// == ContainmentType.Contains 
            //                      select obj);
            //    }
            //return list;
        }
        public override IEnumerable<GameObject> GetObjects(BoundingBox box)
        {
            //return (from o in this.CachedObjects where box.Contains(o.Global) != ContainmentType.Disjoint select o);
            foreach(var o in this.CachedObjects)
            {
                var type = box.Contains(o.Global);
                if (type != ContainmentType.Disjoint)
                    yield return o;
            }
            //return this.GetObjects(box.Min, box.Max);
        }

        //static public void GetData(StaticMap map, BinaryWriter bin)
        //{
        //    bin.Write(map.Name);
        //    bin.Write(map.Coordinates.X);
        //    bin.Write(map.Coordinates.Y);
        //    bin.Write(map.Time.TotalSeconds);
        //}
        public override void WriteData(BinaryWriter w)
        {
            w.Write(this.Name);
            w.Write(this.Coordinates.X);
            w.Write(this.Coordinates.Y);
            //w.Write(this.Clock.TotalSeconds);
            w.Write(this.Size.Name);
            this.Town.Write(w);
            this.UndiscoveredAreaManager.Write(w);
        }

        //static public Tuple<string, float, float, double> ReadData(BinaryReader reader)
        //{
        //    return Tuple.Create(reader.ReadString(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadDouble());
        //}
        static public StaticMap ReadData(IObjectProvider net, BinaryReader r)
        {
            var map = new StaticMap
            {
                Net = net,
                Name = r.ReadString(),
                Coordinates = new Vector2(r.ReadSingle(), r.ReadSingle()),
                //Clock = TimeSpan.FromSeconds(r.ReadDouble())
            };
            var size = r.ReadString();
            map.Size = MapSize.GetList().First(foo => foo.Name == size);
            map.Town.Read(r);
            //map.UndiscoveredAreaManager = UndiscoveredAreaManager.Create(r);
            map.UndiscoveredAreaManager.Read(r);
            return map;
        }


        public override void HandleEvent(GameEvent e)
        {
            this.Town.HandleGameEvent(e);
        }
        public override void OnGameEvent(GameEvent e)
        {
            base.OnGameEvent(e);
            this.Town.HandleGameEvent(e);
            this.Regions.OnGameEvent(e);
            this.UndiscoveredAreaManager.OnGameEvent(e);
        }

        public bool SetCell(int x, int y, int z, Block.Types type, byte data)
        {
            return this.SetCell(new Vector3(x, y, z), type, data);
        }
        public override bool SetCell(Vector3 global, Block.Types type, byte data, int variation = 0)
        {
            //Cell cell = global.GetCell(this);
            Cell cell = this.GetCell(global);

            if (cell.IsNull())
                return false;
            //Chunk chunk = global.GetChunk(this);
            Chunk chunk = this.GetChunk(global);

            //undoOperation = new CellOperation(net, global, cell.Type, cell.Variation, cell.Orientation);
            cell.SetBlockType(type);
            cell.Variation = (byte)variation;
            //cell.Orientation = orientation;
            cell.BlockData = data;
            //Cell.UpdateEdges(this, global, Edges.All, VerticalEdges.All);
            chunk.InvalidateCell(cell);
            //chunk.Saved = false;
            chunk.Invalidate();

            //// load neighbor chunks
            //bool neighborAdded = false;
            //foreach(var vector in chunk.MapCoords.GetNeighbors())
            //    neighborAdded |= this.AddChunk(new Chunk(this, vector));
            //if (neighborAdded)
            //    chunk.UpdateEdges(this);

            return true;
        }
        public override bool SetBlockLuminance(Vector3 global, byte luminance)
        {
            global = global.RoundXY();
            //if (!global.TryGetAll(net.Map, out chunk, out cell))
            if (!this.TryGetAll(global, out var chunk, out var cell))
                return false;
            if (cell.Luminance == luminance)
                return true;
            cell.Luminance = luminance;
            //chunk.Invalidate();//.Saved = false;
            this.InvalidateCell(global);
            //new LightingEngine(net.Map).HandleBatchSync(new Vector3[] { global });
            //net.SpreadBlockLight(global);
            return true;
        }
        public override bool SetBlock(Vector3 global, Block.Types type)
        {
            // WARNING!!! set block with default data and variation or retain previous values?
            return this.SetBlock(global, type, 0, 0);
            //Cell cell = this.GetCell(global);

            //if (cell.IsNull())
            //    return false;

            //Chunk chunk = this.GetChunk(global);
            //cell.SetBlockType(type);
            ////chunk.InvalidateCell(cell);
            //this.InvalidateCell(global);
            //chunk.Invalidate();

            //foreach (var n in global.GetNeighbors())
            //    this.InvalidateCell(n);

            //return true;
        }
        void InvalidateCell(IEnumerable<Vector3> globals)
        {
            foreach (var g in globals)
                this.InvalidateCell(g);
        }
        public override bool InvalidateCell(Vector3 global)
        {
            Chunk chunk;
            Cell cell;
            if (!this.TryGetAll(global, out chunk, out cell))
                return false;
            return chunk.InvalidateCell(cell);
        }

        public override bool InvalidateCell(Vector3 global, Cell cell)
        {
            Chunk chunk;
            if (!this.TryGetChunk(global, out chunk))
                return false;
            return chunk.InvalidateCell(cell);
        }

        public StaticMapLoadingProgressToken LoadAsync()
        {
            var token = new StaticMapLoadingProgressToken();
            System.Threading.Tasks.Task.Factory.StartNew(() => this.Load(token));
            return token;
        }
        public void Load(StaticMapLoadingProgressToken token)
        {
            var size = this.Size.Chunks;
            var max = (size*size);
            int loaded = 0;
            //updateProgress("Loading Chunks: " + loaded.ToString() + " of " + max.ToString(), 0);
            token.Text = "Loading Chunks: " + loaded.ToString() + " of " + max.ToString();
            token.Percentage = 0;
            token.Refresh();

            // load chunks
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    var pos = new Vector2(i, j);
                    //var chunk = new Chunk(this, pos);
                    var chunk = this.Generate(pos);
                    this.ActiveChunks.Add(pos, chunk);
                    loaded++;
                    //updateProgress("Loading Chunks: " + loaded.ToString() + " of " + max.ToString(), );
                    token.Text = "Loading Chunks: " + loaded.ToString() + " of " + max.ToString();
                    token.Percentage = loaded / (float)max;
                    token.Refresh();
                }
            }
        }
        /// <summary>
        /// reports back text and loading percentage after loading each chunk
        /// </summary>
        /// <param name="callback"></param>
        public void GenerateWithNotifications(Action<string, float> callback)
        {
            var size = this.Size.Chunks;
            var max = (size * size);
            //int loaded = 0;
            //callback("Initializing Chunks: " + loaded.ToString() + " of " + max.ToString(), 0);
            var n = 0;

            callback("Initializing Chunks", n);

            var mutatorlist = this.World.GetMutators().ToList();
            mutatorlist.ForEach(m => m.SetWorld(this.World));

            Dictionary<Chunk, Dictionary<Vector3, double>> gradCache = new Dictionary<Chunk, Dictionary<Vector3, double>>();
            var watch = new Stopwatch();
            watch.Start();
            for (int i = 0; i < size; i++)
            {
                for (int j = 0; j < size; j++)
                {
                    var pos = new Vector2(i, j);
                    //var chunk = this.Generate(pos);
                    var chunk = Chunk.Create(this, pos);// i, j); //(Map.Instance.MapArgs, (int)pos.X, (int)pos.Y, Map.Instance.GetSeedArray());
                    gradCache[chunk] = chunk.InitCells2(mutatorlist);//.FinalizeCells(Server.Random); // WARNING!
                    this.ActiveChunks.Add(pos, chunk);
                    //loaded++;
                    //callback("Loading Chunks: " + loaded.ToString() + " of " + max.ToString(), loaded / (float)max);
                }
            }
            watch.Stop();
            string.Format("chunks initialized in {0} ms", watch.ElapsedMilliseconds).ToConsole();

            //watch.Restart();

            //foreach (var chunk in this.ActiveChunks.Values)
            //{
            //    gradCache[chunk] = chunk.InitCells2(mutatorlist);//.FinalizeCells(Server.Random); // WARNING!
            //}
            foreach (var m in mutatorlist)//this.World.GetMutators())
            {
                callback("Applying " + m.Name, n++);

                watch.Restart();
                foreach (var chunk in this.ActiveChunks.Values)
                {
                    var cached = gradCache[chunk];
                    chunk.InitCells3(m, cached);
                    m.Finally(chunk, cached);
                }
                m.Generate(this);
                watch.Stop();

                string.Format("{0} finished in in {1} ms", m.ToString(), watch.ElapsedMilliseconds).ToConsole();
            }
            //foreach (var m in mutatorlist)//this.World.GetMutators())
            //{
            //    foreach (var chunk in this.ActiveChunks.Values)
            //    {
            //        m.Finally(chunk, gradCache[chunk]);
            //    }
            //}
            //string.Format("chunk {0} finalized in {1} ms", chunkvector, watch.ElapsedMilliseconds).ToConsole();
            //watch.Stop();

            this.InitChunks(callback);                  
            this.FinishCreating(callback); // initializes light and generates wild plants
            this.InitUndiscoveredAreas(callback);

            /* seed: "test"
             * chunks generated in 525 ms
                light calculated in 1996 ms
                regions initialized in 244 ms
                light updated in 846 ms
             * */
        }
        public void GenerateWithNotificationsNew(Action<string, float> callback)
        {
            //List<Action> tasks = new List<Action>();
            var tasks = new List<(string label, Action action)>();
            var size = this.Size.Chunks;
            var max = (size * size);
            //int loaded = 0;
            //callback("Initializing Chunks: " + loaded.ToString() + " of " + max.ToString(), 0);
            //var n = 0;
            var mutatorlist = this.World.GetMutators().ToList();
            mutatorlist.ForEach(m => m.SetWorld(this.World));
            var watch = new Stopwatch();
            Dictionary<Chunk, Dictionary<Vector3, double>> gradCache = new Dictionary<Chunk, Dictionary<Vector3, double>>();

            //callback("Initializing Chunks", n);
            tasks.Add(("Initializing Chunks", () =>
             {
                 watch.Start();
                 for (int i = 0; i < size; i++)
                 {
                     for (int j = 0; j < size; j++)
                     {
                         var pos = new Vector2(i, j);
                          //var chunk = this.Generate(pos);
                          var chunk = Chunk.Create(this, pos);// i, j); //(Map.Instance.MapArgs, (int)pos.X, (int)pos.Y, Map.Instance.GetSeedArray());
                          gradCache[chunk] = chunk.InitCells2(mutatorlist);//.FinalizeCells(Server.Random); // WARNING!
                          this.ActiveChunks.Add(pos, chunk);
                          //loaded++;
                          //callback("Loading Chunks: " + loaded.ToString() + " of " + max.ToString(), loaded / (float)max);
                      }
                 }
                 watch.Stop();
                 string.Format("chunks initialized in {0} ms", watch.ElapsedMilliseconds).ToConsole();
             }
            ));
            //watch.Restart();

            //foreach (var chunk in this.ActiveChunks.Values)
            //{
            //    gradCache[chunk] = chunk.InitCells2(mutatorlist);//.FinalizeCells(Server.Random); // WARNING!
            //}

            foreach (var m in mutatorlist)//this.World.GetMutators())
            {
                //callback("Applying " + m.Name, n++);
                tasks.Add(("Applying " + m.Name, () =>
                {
                    watch.Restart();
                    foreach (var chunk in this.ActiveChunks.Values)
                    {
                        var cached = gradCache[chunk];
                        chunk.InitCells3(m, cached);
                        m.Finally(chunk, cached);
                    }
                    m.Generate(this);
                    watch.Stop();

                    string.Format("{0} finished in in {1} ms", m.ToString(), watch.ElapsedMilliseconds).ToConsole();
                }));
            }
            //foreach (var m in mutatorlist)//this.World.GetMutators())
            //{
            //    foreach (var chunk in this.ActiveChunks.Values)
            //    {
            //        m.Finally(chunk, gradCache[chunk]);
            //    }
            //}
            //string.Format("chunk {0} finalized in {1} ms", chunkvector, watch.ElapsedMilliseconds).ToConsole();
            //watch.Stop();

            //tasks.Add(("Post processing map", () => this.InitChunks(null)));
            foreach (var a in this.InitChunksNew())
                tasks.Add(a);
            //tasks.Add(("Finalizing map", () => this.FinishCreating(null))); // initializes light and generates wild plants
            foreach (var a in this.FinishCreatingNew())
                tasks.Add(a);
            tasks.Add(("Detecting undiscovered areas", () => this.InitUndiscoveredAreas(null)));

            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                callback(string.Format(task.label, i, tasks.Count), i / (float)tasks.Count);
                task.action();
            }

            /* seed: "test"
             * chunks generated in 525 ms
                light calculated in 1996 ms
                regions initialized in 244 ms
                light updated in 846 ms
             * */
        }

        public override void Generate()
        {
            throw new Exception();

            //var loader = new ChunkLoader(this);
            //Chunk chunk = loader.Generate(Vector2.Zero);
            //this.AddChunk(chunk);
            //LightingEngine engine = new LightingEngine(this)
            //{
            //    OutdoorBlockHandler = (ch, cell) =>
            //    {
            //        ch.VisibleOutdoorCells[Chunk.GetCellIndex(cell.LocalCoords)] = cell;
            //    }
            //};
            //var positions = chunk.ResetHeightMap();
            //engine.HandleBatchSync(positions);
        }

        public override void DrawWorld(MySpriteBatch mySB, Camera camera)
        {
            //this.Town.DrawWorld(mySB, this, camera);
        }
        public override void DrawBeforeWorld(MySpriteBatch mySB, Camera camera)
        {
            this.Town.DrawBeforeWorld(mySB, this, camera);
        }
        public bool ChunkNeighborsExist(Vector3 global)
        {
            return this.ChunkNeighborsExist(global.GetChunkCoords());
        }
        public override bool ChunkNeighborsExist(Vector2 chunkCoords)
        {
            foreach (var n in chunkCoords.GetNeighbors())
                if (!this.ActiveChunks.ContainsKey(n))
                    if (this.IsChunkWithinBounds(n))
                        return false;
            return true;
        }

        bool IsChunkWithinBounds(Vector2 chunkcoords)
        {
            var bounds = new Rectangle((int)this.Coordinates.X, (int)this.Coordinates.Y, MapSize.Default.Chunks, MapSize.Default.Chunks);
            return bounds.Intersects(new Rectangle((int)chunkcoords.X, (int)chunkcoords.Y, 1, 1));
        }
        public override bool IsInBounds(Vector3 global)
        {
            var maxz = this.GetMaxHeight();
            var maxside = this.Size.Chunks * Chunk.Size;
            return
                global.X >= 0 && global.X < maxside &&
                global.Y >= 0 && global.Y < maxside &&
                global.Z >= 0 && global.Z < maxz;


            //return this.GetChunk(global) != null && global.Z > -1 && global.Z < this.MaxHeight;
        }



        public override void UpdateLight(IEnumerable<WorldPosition> positions)
        {
            if (this.LightingEngine == null)
                this.LightingEngine = LightingEngine.StartNew(this, a => { }, a => { });// new LightingEngine(this);

            this.LightingEngine.Enqueue(positions);
        }
        public void UpdateLight(IEnumerable<Vector3> positions, Action callback)
        {
            if (this.LightingEngine == null)
                this.LightingEngine = LightingEngine.StartNew(this, a => { }, a => { });// new LightingEngine(this);

            this.LightingEngine.Enqueue(positions, callback);
        }
        public void UpdateLight(IEnumerable<Vector3> positions)
        {
            this.LightingEngine.HandleImmediate(positions);
            //if (this.LightingEngine == null)
            //    this.LightingEngine = LightingEngine.StartNew(this, a => { }, a => { });// new LightingEngine(this);

            //this.LightingEngine.Enqueue(positions);
        }
        #region IMap implementation
        
        
        
        
        
        
        public override bool TryGetAll(int gx, int gy, int gz, out Chunk chunk, out Cell cell, out int lx, out int ly)
        {
            if (gz > Map.MaxHeight - 1 || gz < 0)
            {
                lx = 0;
                ly = 0;
                chunk = null;
                cell = null;
                return false;
            }
            if (this.TryGetChunk(gx, gy, out chunk))
            {
                lx = gx - (int)chunk.Start.X;
                ly = gy - (int)chunk.Start.Y;
                cell = chunk[Chunk.GetCellIndex(lx, ly, gz)];
                return true;
            }
            lx = 0;
            ly = 0;
            chunk = null;
            cell = null;
            return false;
        }
        public override Vector2 GetOffset()
        {
            return this.Global;
        }
        public override string GetName()
        {
            return this.Name;
        }
        public override Dictionary<Vector2, Chunk> GetActiveChunks()
        {
            return this.ActiveChunks;
        }
        public override void SetLight(Vector3 global, byte sky, byte block)
        {
            Chunk ch = this.GetChunk(global);
            if (ch.IsNull())
                return;
            Vector3 loc = global.ToLocal();
            ch.SetSunlight(loc, sky);
            ch.SetBlockLight(loc, block);
            ch.InvalidateLight(global);
            return;
        }
        public override void SetSkyLight(Vector3 global, byte value)
        {
            Chunk ch = this.GetChunk(global);
            //if (ch == null)
            //    return;
            Vector3 loc = global.ToLocal();
            ch.SetSunlight(loc, value);
            ch.InvalidateLight(global);
            foreach(var n in global.GetNeighbors())
            {
                Chunk nchunk;
                if (this.TryGetChunk(n, out nchunk))
                    nchunk.InvalidateLight(n);
            }
            return;
        }
        public override void SetBlockLight(Vector3 global, byte value)
        {
            Chunk ch = this.GetChunk(global);
            if (ch.IsNull())
                return;
            Vector3 loc = global.ToLocal();
            ch.SetBlockLight(loc, value);
            ch.InvalidateLight(global);
            foreach (var n in global.GetNeighbors())
            {
                Chunk nchunk;
                if (this.TryGetChunk(n, out nchunk))
                    nchunk.InvalidateLight(n);
            }
            return;
        }

        readonly Queue<Dictionary<Vector3, byte>> SkyLightChanges = new();
        public override void AddSkyLightChanges(Dictionary<Vector3, byte> changes)
        {
            this.SkyLightChanges.Enqueue(changes);
            this.ApplyLightChanges();
        }

        readonly Queue<Dictionary<Vector3, byte>> BlockLightChanges = new();
        public override void AddBlockLightChanges(Dictionary<Vector3, byte> changes)
        {
            this.BlockLightChanges.Enqueue(changes);
            this.ApplyLightChanges();
        }
        public override void ApplyLightChanges()
        {
            while (this.SkyLightChanges.Any())
                foreach (var item in this.SkyLightChanges.Dequeue())
                    this.SetSkyLight(item.Key, item.Value);

            while (this.BlockLightChanges.Any())
                foreach (var item in this.BlockLightChanges.Dequeue())
                    this.SetBlockLight(item.Key, item.Value);
        }
        
        public override byte GetSkyDarkness()
        {
            return this.SkyDarkness;
        }
        public override byte GetData(Vector3 global)
        {
            Cell cell;
            //return global.TryGetCell(map, out cell) ? cell.BlockData : (byte)0;
            return this.TryGetCell(global, out cell) ? cell.BlockData : (byte)0;
        }
        public override byte SetData(Vector3 global, byte data = 0)
        {
            Cell cell = this.GetCell(global);
            byte old = cell.BlockData;
            cell.BlockData = data;
            return old;
        }
        public override byte GetSunLight(Vector3 global)
        {
            byte sunlight;
            Chunk.TryGetSunlight(this, global, out sunlight);
            return sunlight;
        }
       
        
        
        
        //public override bool IsEmpty(Vector3 global)
        //{
        //    global = global.Round();
        //    if (this.GetBlock(global) != Block.Air)
        //        return false;
        //    var blockbox = new BoundingBox(global - (Vector3.UnitX + Vector3.UnitY) * .5f, global + Vector3.UnitZ + (Vector3.UnitX + Vector3.UnitY) * .5f);
        //    var entities = GetObjectsAtChunk(global);
        //    foreach (var entity in entities)
        //    {
        //        var entitybox = new BoundingBox(entity.Transform.Global - (Vector3.UnitX + Vector3.UnitY) * .2f, entity.Transform.Global + Vector3.UnitZ * entity.Physics.Height + (Vector3.UnitX + Vector3.UnitY) * .2f);
        //        if (blockbox.Intersects(entitybox))
        //            return false;
        //    }
        //    return true;
        //}
        public override bool IsExposed(Vector3 vec)
        {
            foreach (var n in vec.GetNeighbors())
                if (!Block.IsBlockSolid(this, n))
                    return true;
            return false;
        }
        //[Obsolete] // why get objects from neighbor chunks as well???
        public override List<GameObject> GetObjectsAtChunk(Vector3 global)
        {
            var chunks = GetChunks(global.GetChunkCoords(), 1);
            List<GameObject> entities = new List<GameObject>();
            foreach (var ch in chunks)
                entities.AddRange(ch.GetObjects());
            return entities;
        }
        public override int GetSizeInChunks()
        {
            return this.Size.Chunks;
        }
        public override Town GetTown()
        {
            return this.Town;
        }
        //public override IWorld GetWorld()
        //{
        //    return this.World;
        //}
        public override int GetMaxHeight()
        {
            return MaxHeight;
        }
        public override WorldPosition GetMouseover()
        {
            return this.Mouseover;
        }
        public override void SetMouseover(WorldPosition pos)
        {
            this.Mouseover = pos;
        }
        static readonly Color ColorMidnight = new(21, 27, 84);
        static readonly Color ColorMango = new(255, 128, 64);
        static readonly Color ColorBronze = new(205, 127, 50);
        //static readonly Dictionary<float, Color> AmbientColors = new Dictionary<float, Color>() { { 0, Color.White }, { 0.5f, ColorMango }, { 1f, Color.Blue } };
        //static readonly Dictionary<float, Color> AmbientColors = new Dictionary<float, Color>() { { 0, Color.White }, { 0.33f, Color.Red }, { 0.66f, Color.Orange }, { 1f, Color.Blue } };
        static readonly Dictionary<float, Color> AmbientColors = new() { { 0, Color.White }, { 0.5f, Color.Red }, { 1f, Color.Blue } };
        Color CachedAmbientColor;
        public override Color GetAmbientColor()
        {
            return this.CachedAmbientColor;
        }

        private Color UpdateAmbientColor()
        {
            var nightAmount = (float)this.GetDayTimeNormal();
            for (int i = 0; i < AmbientColors.Count - 2; i++)
            {
                var a = AmbientColors.ElementAt(i);
                var b = AmbientColors.ElementAt(i + 1);
                var c = AmbientColors.ElementAt(i + 2);

                if (a.Key <= nightAmount && nightAmount < c.Key)
                {
                    var t = (nightAmount - a.Key) / (c.Key - a.Key);
                    var ab = Color.Lerp(a.Value, b.Value, t);
                    var bc = Color.Lerp(b.Value, c.Value, t);
                    return Color.Lerp(ab, bc, t);// this.AmbientColor;
                }
                else if (nightAmount == c.Key)
                    return c.Value;
            }

            return Color.Lime;// this.AmbientColor;
        }
        public override void SetAmbientColor(Color color)
        {
            this.AmbientColor = color;
        }
        public override MapThumb GetThumb()
        {
            return this.Thumb;
        }
        public override double GetDayTimeNormal()
        {
            double normal = (Clock.TotalMinutes - Engine.TicksPerSecond * (Zenith - 12)) / 1440f;
            double nn = normal * 2 * Math.PI;
            nn = 3 * Math.Cos(nn);
            return Math.Max(0, Math.Min(1, (1 + nn) / 2f));
            //return this.DayTimeNormal;
        }
        #endregion


        Chunk Generate(Vector2 chunkvector)
        {
            var watch = System.Diagnostics.Stopwatch.StartNew();
            Chunk newChunk = Chunk.Create(this, (int)chunkvector.X, (int)chunkvector.Y); //(Map.Instance.MapArgs, (int)pos.X, (int)pos.Y, Map.Instance.GetSeedArray());
            var mutatorlist = this.World.GetMutators().ToList();


            var gradients = newChunk.InitCells(mutatorlist);//.FinalizeCells(Server.Random); // WARNING!
            watch.Stop();
            string.Format("chunk {0} generated in {1} ms", chunkvector, watch.ElapsedMilliseconds).ToConsole();

            newChunk.UpdateHeightMap();

            watch.Restart();
            foreach (var m in mutatorlist)//this.World.GetMutators())
                m.Finally(newChunk, gradients);
            string.Format("chunk {0} finalized in {1} ms", chunkvector, watch.ElapsedMilliseconds).ToConsole();
            watch.Stop();
            return newChunk;
        }
        readonly MapRules _Rules = new MapRules() { UnloadChunks = false, 
            SaveChunks = true//false 
            ,
            Autosaving = false
        };
        public override MapRules Rules { get { return this._Rules; ; } }


        internal void FinishCreating(Action<string, float> callback = null)
        {
            callback?.Invoke("Calculating light", 0);
            var watch = Stopwatch.StartNew();
            foreach (var ch in this.ActiveChunks)
            {
                ch.Value.UpdateSkyLight(true); //459
            }
            watch.Stop();
            string.Format("light updated in {0} ms", watch.ElapsedMilliseconds).ToConsole();

            callback?.Invoke("Generating plants", 0);
            watch.Restart();
            Terraformer.Trees.Generate(this);
            watch.Stop();
            string.Format("plants generated in {0} ms", watch.ElapsedMilliseconds).ToConsole();

            callback?.Invoke("Diffusing light", 0);
            foreach (var ch in this.ActiveChunks)
            {
                ch.Value.UpdateChunkBoundaries();
                //ch.Value.ActivateCells();
            } 
        }
        internal IEnumerable<(string, Action)> FinishCreatingNew()
        {
            Stopwatch watch;

            yield return ("Calculating light", () =>
            {
                watch = Stopwatch.StartNew();
                foreach (var ch in this.ActiveChunks)
                {
                    ch.Value.UpdateSkyLight(true); //459
                }
                watch.Stop();
                string.Format("light updated in {0} ms", watch.ElapsedMilliseconds).ToConsole();
            }
            );
            yield return ("Generating plants", () =>
            {
                watch = Stopwatch.StartNew();
                Terraformer.Trees.Generate(this);
                watch.Stop();
                string.Format("plants generated in {0} ms", watch.ElapsedMilliseconds).ToConsole();
            }
            );
            yield return ("Updating chunk edges", () =>
            {
                watch = Stopwatch.StartNew();
                foreach (var ch in this.ActiveChunks)
                {
                    ch.Value.UpdateChunkBoundaries();
                    //ch.Value.ActivateCells();
                }
                string.Format("chunk edges updated in {0} ms", watch.ElapsedMilliseconds).ToConsole();
            }
            );
        }
        internal void FinishLoading()
        {

            //this.FinishCreating(); //uncomment this if i want to reinitialize lighting on a loaded map
            this.CacheObjects();

        }
        readonly UndiscoveredAreaManager UndiscoveredAreaManager;
        internal void InitUndiscoveredAreas(Action<string, float> callback = null)
        {
            callback?.Invoke("Detecing undiscovered areas", 0);
            //foreach (var ch in this.ActiveChunks.Values)
            //    foreach (var c in ch.CellGrid2)
            //        c.Discovered = false; // TEMP
            this.UndiscoveredAreaManager.Init(); // TODO: send undiscovered areas to clients instead of them initializing them themselves?
           
        }
        internal override bool IsUndiscovered(Vector3 global)
        {
            //return false;
            //return this.UndiscoveredAreaManager.IsUndiscovered(global); //this.UndiscoveredAreaManager.IsUndiscovered(global);
            if (!this.UndiscoveredAreaManager.IsUndiscovered(global))
                return false;
            foreach (var n in global.GetAdjacentLazy())
            {
                if (this.IsAir(n) && !this.UndiscoveredAreaManager.IsUndiscovered(n))
                    return false;
            }
            return true;
        }
        internal override void AreaDiscovered(HashSet<Vector3> hashSet)
        {
            foreach(var global in hashSet)
            {
                var chunk = this.GetChunk(global);
                chunk.InvalidateMesh();
            }
            this.Net.Write("Area discovered!");
        }
        internal override void CameraRecenter()
        {
            this.Camera.CenterOn(this);
        }
        //internal void Spawn(GameObject obj, Vector3 global, Vector3 velocity)
        //{
        //    obj.Global = global;
        //    obj.Velocity = velocity;
        //    obj.Map = this;
        //    obj.Parent = null;
        //    obj.Spawn(this.Net);
        //    //PacketEntitySpawn.Send(this.Net, obj, this, global, velocity);
        //    Packets.Send(this.Net, obj, this, global, velocity);
        //}
        //static StaticMap()
        //{

        //}
        //class Packets
        //{
        //    static readonly int PacketSpawn;
        //    static Packets()
        //    {
        //        PacketSpawn = Network.RegisterPacketHandler(Receive);
        //    }
        //    static public void Send(IObjectProvider net, GameObject entity, StaticMap map, Vector3 global, Vector3 velocity)
        //    {
        //        if (net is Client)
        //            //throw new Exception();
        //            return;
        //        var w = net.GetOutgoingStream();
        //        w.Write(PacketType.SpawnEntityNew);
        //        w.Write(PacketSpawn);
        //        w.Write(entity.InstanceID);
        //        w.Write(global);
        //        w.Write(velocity);
        //    }
        //    static void Receive(IObjectProvider net, BinaryReader r)
        //    {
        //        var client = net as Client;
        //        var actor = client.GetNetworkObject(r.ReadInt32());
        //        var global = r.ReadVector3();
        //        var velocity = r.ReadVector3();
        //        var map = client.Map as StaticMap;
        //        map.Spawn(actor, global, velocity);
        //    }
        //}
    }
}
