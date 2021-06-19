using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
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
using Start_a_Town_.GameModes;
using Start_a_Town_.GameModes.StaticMaps;

namespace Start_a_Town_.Editor
{
    public interface IObjectSpace
    {
        float Distance(GameObject obj1, GameObject obj2);
        Vector3? DistanceVector(GameObject obj1, GameObject obj2);
    }
    public class EditorMap : Component, IMap, IObjectSpace, IDisposable, ITooltippable
    {
        public override string ComponentName
        {
            get { return "Map"; }
        }
        public string Network = "local";
        public override object Clone()
        {
            return this;
        }
        public float LoadProgress
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
        public class MapSize
        {
            public string Name { get; private set; }
            public int Blocks { get; private set; }
            public int Chunks { get; private set; }
            public MapSize(string name, int blocks)
            {
                Name = name;
                Blocks = blocks;
                this.Chunks = blocks / Chunk.Size;
            }
            public static readonly MapSize Micro = new MapSize("Micro", 32);
            public static readonly MapSize Tiny = new MapSize("Tiny", 64);
            public static readonly MapSize Small = new MapSize("Small", 128);
            public static readonly MapSize Normal = new MapSize("Normal", 256);
            public static readonly MapSize Huge = new MapSize("Huge", 512);
        }
        public static List<MapSize> Sizes
        { get { return new List<MapSize>() { MapSize.Micro, MapSize.Tiny, MapSize.Small, MapSize.Normal, MapSize.Huge }; } }


        #endregion



        ConcurrentQueue<Chunk> ChunksToActivate;
        //public ConcurrentDictionary<Vector2, Chunk> ActiveChunks;
        Dictionary<Vector2, Chunk> _ActiveChunks;
        public Dictionary<Vector2, Chunk> ActiveChunks
        {
            get { return this._ActiveChunks; }
            set { this._ActiveChunks = value; }
        }

        public bool AddChunk(Chunk chunk)
        {
            //return this.ActiveChunks.TryAdd(chunk.MapCoords, chunk);
            this.ActiveChunks.Add(chunk.MapCoords, chunk);
            // sort chunks back to front to prevent glitches with semi-transparent blocks on chunk edges
            this.ActiveChunks = this.ActiveChunks.OrderBy(c => c.Key.X + c.Key.Y).ToDictionary(i => i.Key, i => i.Value);

            return true;
        }
        public bool Lighting = true;
        public int TimeSpeed = 1;
        public const int Zenith = 14;

        public double DayTimeNormal = 0;
        public bool AddTime()
        {

            Time = Time.Add(new TimeSpan(0, TimeSpeed, 0));
            //double normal = (Time.TotalMinutes - 120) / 1440f;
            double normal = (Time.TotalMinutes - Engine.TargetFps * (Zenith - 12)) / 1440f;
            double nn = normal * 2 * Math.PI;
            nn = 3 * Math.Cos(nn);
            // nn = Math.Pow(nn, 0.5f);
            DayTimeNormal = Math.Max(0, Math.Min(1, (1 + nn) / 2f));
            //   DayTimeNormal = Math.Pow(DayTimeNormal, 2f);

            byte oldDarkness = SkyDarkness;
            SkyDarkness = 0;// (byte)(Math.Round(DayTimeNormal * SkyDarknessMax));
            if (SkyDarkness != oldDarkness)
                foreach (var ch in ActiveChunks)
                    ch.Value.LightCache.Clear();

            return true;
        }
        public byte SkyDarkness = 0, SkyDarknessMax = 13;
        public Color AmbientColor = Color.Blue;//Color.MidnightBlue; //Color.RoyalBlue;//Color.MidnightBlue; //Color.MediumPurple; //Color.Lerp(Color.White, Color.Cornsilk, 0.5f);
        //public bool Fog = false, BorderShading = true;
        //public bool
        //    HideUnderground = false,//true,
        //    HideOverground = false;
        public static int VisibleCellCount = 0;//SeaLevel,
        public Dictionary<int, Rectangle[,]> BaseTileRegions;
        public Game1 game;
        public Town Town { get; set; }
        public bool hasClicked = false;
        public static int MaxHeight = 128;//256; //
        public static float MaxDepth = 0, MinDepth = 0;
        static public Texture2D TerrainSprites, CharacterSprites, ShaderMouseMap, BlockDepthMap;//, BlockDepthMapFlatTop;
        public List<Texture2D> VisibleTileTypes;
        public Vector2 tileLocation = new Vector2(16, 8);
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
        public Vector2 Coordinates { get { return (Vector2)this["Coordinates"]; } set { this["Coordinates"] = value; } }
        public World World;// { get { return (World)this["World"]; } set { this["World"] = value; } }
        public string Name { get { return (string)this["Name"]; } set { this["Name"] = value; } }
        public TimeSpan Time { get { return (TimeSpan)this["Time"]; } set { this["Time"] = value; } }
        public Texture2D[] Thumbnails { get { return (Texture2D[])this["Thumbnails"]; } set { this["Thumbnails"] = value; } }
        public MapThumb Thumb { get { return (MapThumb)this["Thumb"]; } set { this["Thumb"] = value; } }
        // public DirectoryInfo DirectoryInfo { get { return (DirectoryInfo)this["DirectoryInfo"]; } set { this["DirectoryInfo"] = value; } }
        public Net.IObjectProvider Net { get; set; }
        #region Initialization

        public Vector2 Global;// { get { return Coordinates * SizeInBlocks;}}// (Engine.ChunkRadius * 2 + 1); } }

        public EditorMap(string name = "")
        {
            this.Name = name; this.Time = new TimeSpan(Zenith, 0, 0);
            //ActiveChunks = new ConcurrentDictionary<Vector2, Chunk>();
            this.ActiveChunks = new Dictionary<Vector2, Chunk>();
            ChunksToLight = new Queue<Chunk>();
            ChunksToActivate = new ConcurrentQueue<Chunk>();
            this.Thumbnails = new Texture2D[3];
            this.Town = new Town(this);
        }
        public EditorMap(World world, Vector2 coords, string name = "")
            : this(name)
        {
            this.World = world;
            this.Coordinates = coords;
            this.Size = MapSize.Micro;// MapSize.Normal;
            this.Global = Coordinates * this.Size.Blocks;
            this.Thumb = new MapThumb(this);
        }
        public EditorMap(World world, string name, Vector2 coords, MapSize size)
            : this(name)
        {
            this.World = world;
            this.Coordinates = coords;
            this.Size = size;
            this.Global = Coordinates * this.Size.Blocks;
            this.Thumb = new MapThumb(this);
        }

        #endregion
        public int DrawLevel = MaxHeight - 1;
        void Controller_KeyPress(object sender, KeyEventArgs2 e)
        {
            if (Controller.Instance.GetKeys().Contains(Microsoft.Xna.Framework.Input.Keys.OemMinus))
                DrawLevel -= 10;
            //DrawLevel.Max--;
            else if (Controller.Instance.GetKeys().Contains(Microsoft.Xna.Framework.Input.Keys.OemPlus))
                DrawLevel += 10;
            //DrawLevel.Max++;

            //if (Controller.Instance.GetKeys().Contains(Microsoft.Xna.Framework.Input.Keys.Z))
            //    RotateRight();
            //else if (Controller.Instance.GetKeys().Contains(Microsoft.Xna.Framework.Input.Keys.X))
            //    RotateLeft();

        }
        public Rectangle GetRandomTile(int type)
        {
            //return BaseTileRegions[type][rand.Next(BaseTileRegions[type].Count - 1)];
            return BaseTileRegions[type][World.Random.Next(BaseTileRegions[type].Length - 1), 0];
        }
        float t = 0;
        #region Updating
        public void Update(Net.IObjectProvider net)//GameTime gt, Camera camera)
        {
            t -= 1;//GlobalVars.DeltaTime;
            if (t <= 0)
            {
                AddTime();
                t = Engine.TargetFps;
            }
            PerformCellOperations();
            // AnimateWater();
            while (ChunksToActivate.Count > 0)
            {
                Chunk chunk;
                if (!this.ChunksToActivate.TryDequeue(out chunk))
                    continue;
                ActiveChunks[chunk.MapCoords] = chunk;
                if (ActiveChunks.Count > ChunkLoader.ChunkMemoryCapacity)
                {
                    float maxDist = 0, dist;
                    Vector2 furthestChunkCoords = new Vector2();
                    foreach (KeyValuePair<Vector2, Chunk> pair in ActiveChunks)
                    {
                        dist = Vector2.Distance(chunk.MapCoords, pair.Key);
                        if (dist > maxDist)
                        {
                            maxDist = dist;
                            furthestChunkCoords = pair.Key;
                        }
                    }
                    Chunk furthestChunk;
                    //ActiveChunks.TryRemove(furthestChunkCoords, out furthestChunk);
                    ActiveChunks.Remove(furthestChunkCoords);

                    ChunkLoader.UnloadChunk(furthestChunkCoords);

                }
            }
            //Dictionary<Vector2, Chunk> copyOfActiveChunks = new Dictionary<Vector2, Chunk>(ActiveChunks);
            //foreach (KeyValuePair<Vector2, Chunk> chunk in copyOfActiveChunks)
            //    chunk.Value.Update(net);
            if (this.Running)
                foreach (var chunk in this.ActiveChunks.Values.ToList())
                    chunk.Update(net);
            this.Town.Update();
        }
        public bool Running;

        private void PerformCellOperations()
        {
            while (CellOperations.Count > 0)
            {
                CellOperation op;
                if (!CellOperations.TryPeek(out op))
                    break;
                if (op.Perform())
                    CellOperations.TryDequeue(out op);
            }
        }
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

        struct ChunkTexture
        {
            public RenderTarget2D Bitmap;
            public RenderTarget2D Depth;
            public RenderTarget2D Light;
            public RenderTarget2D Mouse;
            public Color[] MouseMap;
        }

        Dictionary<Chunk, ChunkTexture> ChunkSprites = new Dictionary<Chunk, ChunkTexture>();
        public bool Redraw;

        public bool RefreshChunk(Chunk chunk)
        {
            return ChunkSprites.Remove(chunk);
        }

        public WorldPosition Mouseover;

        public void DrawBlocks(MySpriteBatch sb, Camera camera, EngineArgs a)//, SelectionArgs selection)
        {
            Redraw = false;
            ChunksDrawn = 0;
            TilesDrawn = 0;
            TileOutlinesDrawn = 0;
            ObjectsDrawn = 0;
            CullingChecks = 0;
            Dictionary<Vector2, Chunk> copyOfActiveChunks = new Dictionary<Vector2, Chunk>(ActiveChunks);
            Vector3? playerGlobal = null;// = new Nullable<Vector3>(Player.Actor != null ? Player.Actor.Global : null);
            List<Rectangle> hiddenRects = new List<Rectangle>();
            if (Player.Actor != null)
            {
                if (Player.Actor.Exists)
                {
                    playerGlobal = new Nullable<Vector3>(Player.Actor.Global.RoundXY());
                    Sprite sprite = Player.Actor.GetSprite();// (Sprite)Player.Actor["Sprite"]["Sprite"];
                    Rectangle spriteBounds = sprite.GetBounds(); // make bounds a field
                    Rectangle screenBounds = camera.GetScreenBounds(playerGlobal.Value, spriteBounds);
                    hiddenRects.Add(screenBounds);
                }
            }
            //playerGlobal = Player.Actor != null ? new Nullable<Vector3>(Player.Actor.Global) : null;
            //playerGlobal.Value = Player.Actor != null ? Player.Actor.Global : null;
            // Mouseovers = new SortedList<float, Vector3>();

            //Mouseover.Clear();
            Mouseover = null;
            //int d = 0;
            foreach (KeyValuePair<Vector2, Chunk> chunk in copyOfActiveChunks)//.OrderBy(foo => foo.Value.GetDepthFar(camera))) //Depth))
            {
                Rectangle chunkBounds = camera.GetScreenBounds(chunk.Value.Start.X + Chunk.Size / 2, chunk.Value.Start.Y + Chunk.Size / 2, EditorMap.MaxHeight / 2, chunk.Value.GetBounds());  //chunk.Value.GetBounds(camera);
                if (!camera.ViewPort.Intersects(chunkBounds))
                    continue;
                camera.DrawChunk(sb, this, chunk.Value, playerGlobal, hiddenRects, a);
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

        public bool TryGetBlockObject(Vector3 global, out GameObject blockObj)
        {
            //return (global.GetChunk(this).TryGetBlockObject(global.ToLocal().Round(), out blockObj));
            return (this.GetChunk(global).TryGetBlockObject(global.ToLocal().RoundXY(), out blockObj));

            //return (global.GetChunk(this).TryGetBlockObject(global.Round(), out blockObj));
        }
        public bool GetBlockObjectOrDefault(Vector3 global, out GameObject blockObj)
        {
            Vector3 rnd = global.RoundXY();
            Chunk chunk;
            Cell cell;
            //if (!Position.TryGet(this, rnd, out cell, out chunk))
            if (!this.TryGetAll(rnd, out chunk, out cell))
            {
                blockObj = null;
                return false;
            }
            //if (cell.TileType == Tile.Types.Farmland)
            //    "test".ToConsole();
            // if the block object exists, assign it and return true
            if (chunk.TryGetBlockObject(rnd.ToLocal(), out blockObj))
                return true;

            // otherwise, create a new block object
            if (!BlockComponent.Blocks.ContainsKey(cell.Block.Type))
            {
                blockObj = null;
                return false;
            }
            return false;
            //return Cell.TryGetObject(this, global, out blockObj);

        }

        //public float GlobalDepthFar, GlobalDepthNear;
        //public void UpdateDepths(Camera camera)
        //{
        //    GetGlobalDepthRange(out GlobalDepthFar, out GlobalDepthNear);

        //    //GlobalDepthNear = 0;
        //    //GlobalDepthFar = 1;
        //    //Dictionary<Vector2, Chunk> copyOfActiveChunks = new Dictionary<Vector2, Chunk>(ActiveChunks);
        //    //foreach (KeyValuePair<Vector2, Chunk> chunk in copyOfActiveChunks)
        //    //{
        //    //    GlobalDepthFar = Math.Min(chunk.Value.GetDepthFar(camera), GlobalDepthFar);
        //    //    GlobalDepthNear = Math.Max(chunk.Value.GetDepthNear(camera), GlobalDepthNear);

        //    //}
        //}

        //static public float GlobalDepthFar { get { return (-Engine.ChunkRadius * Chunk.Size) * 2; } }
        //static public float GlobalDepthNear { get { return (Engine.ChunkRadius * Chunk.Size + Chunk.Size - 1) * 2 + Map.MaxHeight - 1; } }
        [Obsolete]
        static public float GlobalDepthFar { get { return (-Engine.ChunkRadius * Chunk.Size * 2); } }
        [Obsolete]
        static public float GlobalDepthNear { get { return (Engine.ChunkRadius + 2) * Chunk.Size * 2 + EditorMap.MaxHeight; } }

        static public float GlobalDepthFarNew { get { return (Engine.ChunkRadius * Chunk.Size * 2); } }
        static public float GlobalDepthNearNew { get { return -(Engine.ChunkRadius * Chunk.Size * 2 + EditorMap.MaxHeight); } } // + Map.MaxHeight



        public void DrawObjects(MySpriteBatch sb, Camera camera, SceneState scene)
        {
            foreach (KeyValuePair<Vector2, Chunk> chunk in ActiveChunks)
            {
                Rectangle chunkBounds = camera.GetScreenBounds(chunk.Value.Start.X + Chunk.Size / 2, chunk.Value.Start.Y + Chunk.Size / 2, EditorMap.MaxHeight / 2, chunk.Value.GetBounds());  //chunk.Value.GetBounds(camera);
                if (camera.ViewPort.Intersects(chunkBounds))
                    chunk.Value.DrawObjects(sb, camera, Controller.Instance, Player.Instance, this, scene);//, globaldMin, globaldMax);
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

        public void DrawInterface(SpriteBatch sb, Camera camera)//, SceneState state)
        {
            Dictionary<Vector2, Chunk> copyOfActiveChunks = new Dictionary<Vector2, Chunk>(ActiveChunks);
            foreach (KeyValuePair<Vector2, Chunk> chunk in copyOfActiveChunks)//.OrderBy(foo => foo.Value.GetDepthFar(camera))) //Depth))
            {


                //if (chunk.Value.Visible(camera))
                Rectangle chunkBounds = camera.GetScreenBounds(chunk.Value.Start.X + Chunk.Size / 2, chunk.Value.Start.Y + Chunk.Size / 2, EditorMap.MaxHeight / 2, chunk.Value.GetBounds());  //chunk.Value.GetBounds(camera);
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

        }


        #endregion
        private void Chunk_VisibleChanged(object sender, EventArgs e)
        {
            OnVisibleChunksChanged();
        }
        public event EventHandler<EventArgs> VisibleChunksChanged;
        private void OnVisibleChunksChanged()
        {
            if (VisibleChunksChanged != null)
                VisibleChunksChanged(this, EventArgs.Empty);
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
            Vector2 vec = new Vector2((float)Math.Floor(x / (float)Chunk.Size), (float)Math.Floor(y / (float)Chunk.Size));
            //Vector2 vec = new Vector2(x / Chunk.Size, y / Chunk.Size);
            if (!ActiveChunks.ContainsKey(vec))
                return null;
            return ActiveChunks[vec];
        }
        public void Dispose()
        {
            //ActiveChunks = new ConcurrentDictionary<Vector2, Chunk>();
            this.ActiveChunks = new Dictionary<Vector2, Chunk>();
            ChunksToActivate = new ConcurrentQueue<Chunk>();
            //foreach (var tex in Thumbnails)
            //    if(!tex.IsNull())
            //        tex.Dispose();
            //this.Thumb.Dispose();
        }
        public string GetFullPath()
        {
            return GlobalVars.SaveDir + @"Worlds\Static\" + World.Name + @" \" + GetFolderName() + @"\";
        }
        public string GetFolderName()
        {
            return this.Coordinates.X.ToString() + "." + this.Coordinates.Y.ToString();
        }
        //internal void SaveServer()
        //{
        //    foreach (var chunk in
        //        from ch in this.ActiveChunks
        //        where !ch.Value.Saved
        //        select ch.Value)
        //        chunk.SaveServer();
        //}
        public new SaveTag Save()
        {
            ChunkLoader.Paused = true;
            //ChunkLighter.Paused = true;

            //string directory = GlobalVars.SaveDir + @"/Worlds/" + this["Name"] + "/";// Directory.GetCurrentDirectory() + @"/Saves/";
            //string worldPath = @"/Saves/Worlds/" + this["Name"] + "/";
            //string fullPath = worldPath + this["Name"] + ".map.sat";
            string mapDir = @"/Saves/Worlds/Static/" + this.World.Name + "/" + GetFolderName() + "/";
            string fullMapDir = GlobalVars.SaveDir + @"/Worlds/Static/" + this.World.Name + "/" + GetFolderName() + "/"; // Directory.GetCurrentDirectory() + @"/Saves/";
            string fullFileName = mapDir + GetFolderName() + ".map.sat";
            SaveTag mapTag;
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);

                mapTag = new SaveTag(SaveTag.Types.Compound, "Map");

                mapTag.Add(new SaveTag(SaveTag.Types.Int, "X", (int)this.Coordinates.X));
                mapTag.Add(new SaveTag(SaveTag.Types.Int, "Y", (int)this.Coordinates.Y));
                mapTag.Add(new SaveTag(SaveTag.Types.String, "Name", this.Name));
                mapTag.Add(new SaveTag(SaveTag.Types.Double, "Time", this.Time.TotalSeconds));

                SaveTag playerTag = new SaveTag(SaveTag.Types.Compound, "Player");
                if (Player.Actor != null)
                    playerTag.Add(new SaveTag(SaveTag.Types.Compound, Player.Actor.Name, Player.Actor.Save()));
                mapTag.Add(playerTag);


                mapTag.WriteTo(writer);
                if (!Directory.Exists(fullMapDir))
                    Directory.CreateDirectory(fullMapDir);

                Chunk.Compress(stream, fullFileName);

                stream.Close();

            }

            // don't save thumbnail until i figure out how to organize regions
            //GenerateThumbnails(fullMapDir);

            foreach (KeyValuePair<Vector2, Chunk> pair in ActiveChunks)
                pair.Value.Save();

            ChunkLoader.Paused = false;

            return mapTag;
        }

        public void GenerateThumbnails()
        {
            //string fullMapDir = GlobalVars.SaveDir + @"/Worlds/" + this.World.Name + "/" + GetFolderName() + "/";
            string fullMapDir = this.World.GetPath() + GetFolderName() + "/";

            this.GenerateThumbnails(fullMapDir);
        }
        public void GenerateThumbnails(string fullMapDir)//DirectoryInfo mapDir)//
        {
            // string fullMapDir = mapDir.FullName;
            if (!Directory.Exists(fullMapDir))
                Directory.CreateDirectory(fullMapDir);
            if (ActiveChunks.Count > 0)
            {
                using (Texture2D thumbnail = GetThumbnail())
                {
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
        }
        static public EditorMap Load(string filename)
        {
            return new EditorMap();
        }
        static public EditorMap Load(DirectoryInfo mapDir, World world, Vector2 coords)
        {
            string filename = mapDir.FullName + @"\" + coords.X.ToString() + "." + coords.Y.ToString() + ".map.sat";
            EditorMap map = new EditorMap(world, coords);// { DirectoryInfo = new DirectoryInfo(mapDir + @"\") };
            DateTime start = DateTime.Now;
            string directory = GlobalVars.SaveDir;
            using (FileStream stream = new FileStream(filename, System.IO.FileMode.Open))
            {

                using (MemoryStream decompressedStream = Chunk.Decompress(stream))
                {
                    BinaryReader reader = new BinaryReader(decompressedStream); //stream);//
                    SaveTag mapTag = SaveTag.Read(reader);


                    map.Name = (string)mapTag["Name"].Value;
                    map.Coordinates = new Vector2((int)mapTag["X"].Value, (int)mapTag["Y"].Value);

                    map["Time"] = TimeSpan.FromSeconds((double)mapTag["Time"].Value);

                    // DONT LOAD PLAYER todo: remove player saving from map
                    //SaveTag playerTag = mapTag["Player"] as SaveTag;
                    //List<SaveTag> tagList = playerTag.Value as List<SaveTag>;
                    //if (tagList.Count>1)
                    //{
                    //    GameObject player;
                    //    Dictionary<string, SaveTag> byName = playerTag.ToDictionary();
                    //    player = GameObject.Create(byName.First().Value);
                    //    player.Name = byName.First().Key;
                    //    map["Player"] = player;
                    //}


                    map["Name"] = (string)mapTag["Name"].Value;
                }


                stream.Close();
            }
            map.LoadThumbnails();//mapDir.FullName);
            // Console.WriteLine("map loaded in " + (DateTime.Now - start).ToString() + " ms");
            return map;
        }
        public void LoadThumbnails()//string folderPath)//DirectoryInfo mapDir)
        {
            var folderPath = this.GetFullPath();
            try
            {
                DirectoryInfo mapDir = new DirectoryInfo(folderPath);
                List<FileInfo> thumbFiles = new List<FileInfo>();
                thumbFiles.AddRange(mapDir.GetFiles("thumbnailSmall.png"));
                thumbFiles.AddRange(mapDir.GetFiles("thumbnailSmaller.png"));
                thumbFiles.AddRange(mapDir.GetFiles("thumbnailSmallest.png"));

                int i = 0;
                //     this.Thumb = new MapThumb(this);
                foreach (FileInfo thumbFile in thumbFiles)
                    using (FileStream stream = new FileStream(thumbFile.FullName, FileMode.Open))
                    {
                        Texture2D tex = Texture2D.FromStream(Game1.Instance.GraphicsDevice, stream);
                        this.Thumbnails[i] = tex;
                        Thumb.Sprites[i++] = new Sprite(tex, new Rectangle[][] { new Rectangle[] { tex.Bounds } }, tex.Bounds.Center.ToVector());
                    }
            }
            catch (Exception) { }
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

            EditorMap map = new EditorMap();//args);
        }
        public void HandleInput(InputState input)
        {

        }

        internal void DrawObjectSelection(SpriteBatch sb, Camera camera)
        {
            //GameObject mouseover;// = Controller.Instance.Mouseover.Object as GameObject;
            //if (!Controller.Instance.Mouseover.TryGet<GameObject>(out mouseover))
            //{
            //    Console.WriteLine(Controller.Instance.Mouseover.Object);
            //    return;
            //}// Console.WriteLine(mouseover);

            GameObject mouseover = Controller.Instance.Mouseover.Object as GameObject;
            // Console.WriteLine(Controller.Instance.Mouseover.Object);
            //Console.WriteLine(mouseover);
            if (mouseover.IsNull())
                return;


            //return;

            if ((bool)mouseover["Sprite"]["Flash"])
                return;

            //// TODO: this is also at the chunk.drawobjects
            //if ((bool)mouseover["Sprite"]["Hidden"])
            //    return;

            //Console.WriteLine(mouseover);
            if (mouseover.Transform == null)
                return;
            //Console.WriteLine(mouseover);
            Vector3 g = mouseover.Global, off = SpriteComponent.GetOffset((Vector3)mouseover["Sprite"]["Offset"], (double)mouseover["Sprite"]["OffsetTimer"]);//sprComp.GetOffset();
            Rectangle bounds;
            // camera.CullingCheck(g.X + off.X, g.Y + off.Y, g.Z + off.Z, sprComp.Sprite.GetBounds(), out bounds);
            Sprite sprite = (Sprite)mouseover["Sprite"]["Sprite"];
            camera.CullingCheck(g.X + off.X, g.Y + off.Y, g.Z + off.Z, sprite.GetBounds(), out bounds);
            // TODO: do i not clip the object if out of screen bounds???!?!?
            Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
            Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);
            int variation = (int)mouseover["Sprite"]["Variation"];
            mouseover.DrawMouseover(sb, camera);
            //sb.Draw(sprite.Texture, screenLoc,
            //    sprite.SourceRects[variation][SpriteComponent.GetOrientation((int)mouseover["Sprite"]["Orientation"], camera, sprite.SourceRects[variation].Length)],// sprComp.GetOrientation(camera)], 
            //    new Color(255, 255, 255, 127), 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);

            Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);

        }

        public Texture2D GetThumbnail()
        {
            GraphicsDevice gd = Game1.Instance.GraphicsDevice;
            float zoom = 1 / 8f;
            //Effect fx = Game1.Instance.Content.Load<Effect>("blur");
            //int width = (int)((Engine.ChunkRadius + Engine.ChunkRadius + 1) * Chunk.Size * Tile.Width * zoom);
            int width = (int)(this.Size.Blocks * Block.Width * zoom);
            Vector2 mapCoords = this.Global;
            Camera camera = new Camera(width, width, x: mapCoords.X, y: mapCoords.Y, z: EditorMap.MaxHeight / 2, zoom: zoom);
            RenderTarget2D final = new RenderTarget2D(gd, width, width);
            camera.NewDraw(final, this, gd, EngineArgs.Default, new SceneState(), PlayerControl.ToolManager.Instance);
            gd.SetRenderTarget(null);
            return final;
        }

        static public EditorMap Create(WorldArgs a)
        {
            EditorMap map = new EditorMap();//a);
            return map;
        }
        static public EditorMap Create(World world, Vector2 coords)
        {
            EditorMap map = new EditorMap(world, coords);//a);
            world.Maps[coords] = map;
            return map;
        }
        public void GetTooltipInfo(Tooltip tooltip)
        {
            tooltip.Controls.Add(ToString().ToLabel());
        }

        public ConcurrentQueue<CellOperation> CellOperations = new ConcurrentQueue<CellOperation>();
        //public CellOperation SetCell(Vector3 position, Block.Types type, int variation = 0, int orientation = 0)
        //{
        //    CellOperation op = new CellOperation(this, position, type, variation, orientation);
        //    CellOperations.Enqueue(op);
        //    return op;
        //}
        public void SetCell(CellOperation operation)
        {
            CellOperations.Enqueue(operation);
        }
        public void SetCell(IEnumerable<CellOperation> operations)
        {
            foreach (var op in operations)
                CellOperations.Enqueue(op);
        }

        public bool Contains(GameObject obj)
        {
            Chunk chunk;
            //if (.TryGetChunk(this, out chunk))
            if (this.TryGetChunk(obj.Global, out chunk))
                return chunk.GetObjects().Contains(obj);
            return false;
        }

        public List<GameObject> GetObjects()
        {
            var list = new List<GameObject>();
            foreach (var chunk in this.ActiveChunks)
                list.AddRange(chunk.Value.GetObjects());
            return list;
        }
        public IEnumerable<GameObject> GetObjects(Vector3 min, Vector3 max)
        {
            //Chunk minChunk, maxChunk;
            //minChunk = min.GetChunk(this);
            //maxChunk = max.GetChunk(this);
            List<GameObject> list = new List<GameObject>();
            Vector2 minChunk, maxChunk;
            minChunk = min.GetChunkCoords();
            maxChunk = max.GetChunkCoords();
            BoundingBox box = new BoundingBox(min, max);
            for (float i = minChunk.X; i < maxChunk.X + 1; i++)
                for (float j = minChunk.Y; j < maxChunk.Y + 1; j++)
                {
                    var currentChunkCoords = new Vector2(i, j);
                    Chunk currentChunk;
                    if (!this.ActiveChunks.TryGetValue(currentChunkCoords, out currentChunk))
                        continue;
                    list.AddRange(from obj in currentChunk.GetObjects()
                                  where box.Contains(obj.Global) == ContainmentType.Contains
                                  select obj);
                }
            return list;
        }
        public IEnumerable<GameObject> GetObjects(BoundingBox box)
        {
            return this.GetObjects(box.Min, box.Max);
        }

        static public void GetData(EditorMap map, BinaryWriter bin)
        {
            bin.Write(map.Name);
            bin.Write(map.Coordinates.X);
            bin.Write(map.Coordinates.Y);
            bin.Write(map.Time.TotalSeconds);
        }
        public void GetData(BinaryWriter bin)
        {
            bin.Write(this.Name);
            bin.Write(this.Coordinates.X);
            bin.Write(this.Coordinates.Y);
            bin.Write(this.Time.TotalSeconds);
        }

        //static public Tuple<string, float, float, double> ReadData(BinaryReader reader)
        //{
        //    return Tuple.Create(reader.ReadString(), reader.ReadSingle(), reader.ReadSingle(), reader.ReadDouble());
        //}
        static public EditorMap ReadData(BinaryReader reader)
        {
            EditorMap map = new EditorMap();
            map.Name = reader.ReadString();
            map.Coordinates = new Vector2(reader.ReadSingle(), reader.ReadSingle());
            map.Time = TimeSpan.FromSeconds(reader.ReadDouble());
            return map;
        }

        [Obsolete]
        public void HandleEvent(GameEvent e)
        {
            this.Town.HandleGameEvent(e);
        }
        public void OnGameEvent(GameEvent e)
        {

        }
        public bool SetCell(int x, int y, int z, Block.Types type, byte data)
        {
            return this.SetCell(new Vector3(x, y, z), type, data);
        }
        public bool SetCell(Vector3 global, Block.Types type, byte data, int variation = 0)
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
        public bool SetBlockLuminance(Vector3 global, byte luminance)
        {
            global = global.RoundXY();
            Cell cell;// = global.GetCell(net.Map);
            Chunk chunk;
            //if (!global.TryGetAll(net.Map, out chunk, out cell))
            if (!this.TryGetAll(global, out chunk, out cell))
                return false;
            cell.Luminance = luminance;
            //chunk.Invalidate();//.Saved = false;
            this.InvalidateCell(global);
            //new LightingEngine(net.Map).HandleBatchSync(new Vector3[] { global });
            //net.SpreadBlockLight(global);
            return true;
        }
        public bool SetBlock(int x, int y, int z, Block.Types type, byte data, int variation = 0)
        {
            return this.SetBlock(new Vector3(x, y, z), type, data, variation);
        }
        public bool SetBlock(Vector3 global, Block.Types type)
        {
            Cell cell = this.GetCell(global);

            if (cell.IsNull())
                return false;

            Chunk chunk = this.GetChunk(global);
            cell.SetBlockType(type);

            chunk.InvalidateCell(cell);
            chunk.Invalidate();
            chunk.UpdateBlockVisibility(cell);
            //foreach(var n in global.GetNeighbors())

            return true;
        }
        public bool SetBlock(Vector3 global, Block.Types type, byte data, int variation = 0)
        {
            Cell cell = this.GetCell(global);

            if (cell.IsNull())
                return false;

            Chunk chunk = this.GetChunk(global);
            cell.SetBlockType(type);
            cell.Variation = (byte)variation;
            cell.BlockData = data;
            chunk.InvalidateCell(cell);
            chunk.Invalidate();
            chunk.UpdateBlockVisibility(cell);
            //foreach(var n in global.GetNeighbors())

            return true;
        }

        public void UpdateBlockFaces(Vector3 global)
        {
            Chunk ch;
            Cell ce;
            if (this.TryGetAll(global, out ch, out ce))
                ch.UpdateBlockFaces(ce, Edges.All, VerticalEdges.All);

            foreach(var n in global.GetNeighbors())
            {
                Chunk nch;
                Cell nce;
                if (this.TryGetAll(global, out nch, out nce))
                    ch.UpdateBlockFaces(nce, Edges.All, VerticalEdges.All);  
            }
        }

        public bool InvalidateCell(Vector3 global)
        {
            Chunk chunk;
            Cell cell;
            //if (!global.TryGetAll(this, out chunk, out cell))
            if (!this.TryGetAll(global, out chunk, out cell))
                return false;
            return chunk.InvalidateCell(cell);
        }
        public bool InvalidateCell(Vector3 global, Cell cell)
        {
            Chunk chunk;
            //if (!global.TryGetChunk(this, out chunk))
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
            var max = (size * size);
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

        public void Generate()
        {
            //Chunk chunk = ChunkLoader.Load(map, Vector2.Zero);
            //Chunk chunk = new Chunk(this, Vector2.Zero);
            var loader = new ChunkLoader(this);
            Chunk chunk = loader.Generate(Vector2.Zero);
            this.AddChunk(chunk);
            LightingEngine engine = new LightingEngine(this)
            {
                OutdoorBlockHandler = (ch, cell) =>
                {
                    ch.VisibleOutdoorCells[Chunk.FindIndex(cell.LocalCoords)] = cell;
                    //Cell.UpdateEdges(this, cell.GetGlobalCoords(ch), Edges.All, VerticalEdges.All);
                }
            };
            var positions = chunk.ResetHeightMap();
            engine.HandleBatchSync(positions);
        }

        public void DrawWorld(MySpriteBatch mySB, Camera camera)
        {
            //this.Town.DrawWorld(mySB, this, camera);
        }

        public bool ChunkNeighborsExist(Vector3 global)
        {
            return this.ChunkNeighborsExist(global.GetChunkCoords());
        }
        public bool ChunkNeighborsExist(Vector2 chunkCoords)
        {
            foreach (var n in chunkCoords.GetNeighbors())
                if (!this.ActiveChunks.ContainsKey(n))
                    return false;
            return true;
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
        public bool PositionExists(Vector3 global)
        {
            return this.GetChunk(global) != null;
        }
        LightingEngine LightingEngine;
        public void UpdateLight(IEnumerable<Vector3> globals)
        {
            if (this.LightingEngine == null)
                this.LightingEngine = LightingEngine.StartNew(this.Net, a => { }, a => { });// new LightingEngine(this);

            var list = from gl in globals select new WorldPosition(this, gl);
            this.UpdateLight(list);
        }
        public void UpdateLight(IEnumerable<WorldPosition> positions)
        {
            if (this.LightingEngine == null)
                this.LightingEngine = LightingEngine.StartNew(this.Net, a => { }, a => { });// new LightingEngine(this);

            this.LightingEngine.Enqueue(positions);
        }

        #region IMap implementation
        public void RemoveBlock(Vector3 global)
        {
            this.GetBlock(global).Remove(this, global);
        }
        public BlockEntity RemoveBlockEntity(Vector3 global)
        {
            Chunk chunk = this.GetChunk(global);
            var local = global.ToLocal();
            BlockEntity entity;
            if (chunk.BlockEntities.TryGetValue(local, out entity))
            {
                chunk.BlockEntities.Remove(local);
                return entity;
            }
            return null;
            //return chunk.BlockEntities.Remove(local);
        }
        public void AddBlockEntity(Vector3 global, BlockEntity entity)
        {
            Chunk chunk = this.GetChunk(global);
            chunk.BlockEntities[global.ToLocal()] = entity;
        }
        public BlockEntity GetBlockEntity(Vector3 global)// IObjectProvider net)
        {
            Chunk chunk = this.GetChunk(global);
            BlockEntity entity;
            chunk.BlockEntities.TryGetValue(global.ToLocal(), out entity);
            return entity;
        }
        public int GetHeightmapValue(Vector3 global)
        {
            var ch = this.GetChunk(global);
            return ch.GetHeightMapValue(global.ToLocal());
        }
        public Block GetBlock(Vector3 global)
        {
            Cell cell;
            if (!this.TryGetCell(global, out cell))
                //throw new Exception("Block doesn't exist");
                return null;
            return cell.Block;//.Solid;
        }
        public Cell GetCell(Vector3 global)
        {
            Vector3 globalRound = new Vector3((int)Math.Round(global.X), (int)Math.Round(global.Y), (int)Math.Floor(global.Z));
            Chunk chunk;
            if (this.TryGetChunk(globalRound, out chunk))
            {
                Cell cell = chunk[globalRound.X - chunk.Start.X, globalRound.Y - chunk.Start.Y, globalRound.Z];
                if (cell == null)
                    Console.WriteLine("GAMW TO SPITI");
                return cell;
            }
            return null;
        }
        public Chunk GetChunk(Vector3 global)
        {
            Chunk chunk;
            if (this.TryGetChunk(global, out chunk))
                return chunk;
            return null;
            //Vector3 globalRound = new Vector3((int)Math.Round(global.X), (int)Math.Round(global.Y), (int)Math.Floor(global.Z));
            //Chunk chunk;
            //if (this.TryGetChunk(globalRound, out chunk))
            //    return chunk;
            //return null;
        }
        public Chunk GetChunkAt(Vector2 chunkCoords)
        {
            return this.ActiveChunks[chunkCoords];
        }
        public bool ChunkExists(Vector2 chunkCoords)
        {
            return this.ActiveChunks.ContainsKey(chunkCoords);
        }
        public List<Chunk> GetChunks(Vector2 pos, int radius = 1)//, int radius = Engine.ChunkRadius)
        {
            Chunk ch;
            List<Chunk> list = new List<Chunk>();
            int x = (int)pos.X, y = (int)pos.Y;
            for (int i = x - radius; i <= x + radius; i++)
                for (int j = y - radius; j <= y + radius; j++)
                    //if (ChunkLoader.TryGetChunk(map, new Vector2(i, j), out ch))
                    if (this.ActiveChunks.TryGetValue(new Vector2(i, j), out ch))
                        list.Add(ch);
            return list;
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

            // I changed this because chunkloader exists only on the server now
            // return ChunkLoader.TryGetChunk(map, new Vector2(chunkX, chunkY), out chunk);
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
        public bool TryGetAll(Vector3 global, out Chunk chunk, out Cell cell, out Vector3 local)
        {
            Vector3 rnd = global.RoundXY();// Position.Round(global);
            local = rnd.ToLocal();// Position.ToLocal(rnd);
            return this.TryGetAll(global, out chunk, out cell);
        }
        public bool TryGetAll(int gx, int gy, int gz, out Chunk chunk, out Cell cell, out int lx, out int ly)
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
                cell = chunk[Chunk.FindIndex(lx, ly, gz)];
                return true;
            }
            lx = 0;
            ly = 0;
            chunk = null;
            cell = null;
            return false;
        }
        public Vector2 GetOffset()
        {
            return this.Global;
        }
        public string GetName()
        {
            return this.Name;
        }
        public Dictionary<Vector2, Chunk> GetActiveChunks()
        {
            return this.ActiveChunks;
        }
        public void SetLight(Vector3 global, byte sky, byte block)
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
        public void SetSkyLight(Vector3 global, byte value)
        {
            Chunk ch = this.GetChunk(global);
            if (ch.IsNull())
                return;
            Vector3 loc = global.ToLocal();
            ch.SetSunlight(loc, value);
            ch.InvalidateLight(global);
            return;
        }
        public void SetBlockLight(Vector3 global, byte value)
        {
            Chunk ch = this.GetChunk(global);
            if (ch.IsNull())
                return;
            Vector3 loc = global.ToLocal();
            ch.SetBlockLight(loc, value);
            ch.InvalidateLight(global);
            return;
        }
        ConcurrentQueue<Dictionary<Vector3, byte>> SkyLightChanges = new ConcurrentQueue<Dictionary<Vector3, byte>>();
        public void AddSkyLightChanges(Dictionary<Vector3, byte> changes)
        {
            this.SkyLightChanges.Enqueue(changes);
        }
        ConcurrentQueue<Dictionary<Vector3, byte>> BlockLightChanges = new ConcurrentQueue<Dictionary<Vector3, byte>>();
        public void AddBlockLightChanges(Dictionary<Vector3, byte> changes)
        {
            this.BlockLightChanges.Enqueue(changes);
        }
        public void ApplyLightChanges()
        {
            Dictionary<Vector3, byte> current;
            while (this.SkyLightChanges.TryDequeue(out current))
                foreach (var item in current)
                    this.SetSkyLight(item.Key, item.Value);
            while (this.BlockLightChanges.TryDequeue(out current))
                foreach (var item in current)
                    this.SetBlockLight(item.Key, item.Value);
        }
        public bool GetLight(Vector3 global, out byte sky, out byte block)
        {
            return Chunk.TryGetFinalLight(this, (int)global.X, (int)global.Y, (int)global.Z, out sky, out block);
        }
        public byte GetSkyDarkness()
        {
            return this.SkyDarkness;
        }
        public byte GetData(Vector3 global)
        {
            Cell cell;
            //return global.TryGetCell(map, out cell) ? cell.BlockData : (byte)0;
            return this.TryGetCell(global, out cell) ? cell.BlockData : (byte)0;
        }
        public byte SetData(Vector3 global, byte data = 0)
        {
            Cell cell = this.GetCell(global);
            byte old = cell.BlockData;
            cell.BlockData = data;
            return old;
        }
        public byte GetSunLight(Vector3 global)
        {
            byte sunlight;
            Chunk.TryGetSunlight(this, global, out sunlight);
            return sunlight;
        }
        public IObjectProvider GetNetwork()
        {
            return this.Net;
        }
        public void SetNetwork(IObjectProvider net)
        {
            this.Net = net;
        }
        public float GetSolidObjectHeight(Vector3 global)
        {
            var cell = this.GetCell(global);
            if (cell.Block != Block.Air)
                return cell.Block.GetHeight(global.ToBlock());

            foreach (var entity in this.GetObjects(global - new Vector3(5), global + new Vector3(5)))
            {
                if (!entity.Physics.Solid)
                    return 0;
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
        public bool IsSolid(Vector3 global)
        {
            Cell cell;
            //global = global.Round();
            if (!this.TryGetCell(global, out cell))
                return true; // return true to prevent crashing by trying to add object to missing chunk
            //return false; // return false to let entity attempt to enter unloaded chunk so we can handle the event of that

            return cell.Block.Solid;//.Solid;
        }
        public bool IsEmpty(Vector3 global)
        {
            global = global.Round();
            if (this.GetBlock(global) != Block.Air)
                return false;
            var blockbox = new BoundingBox(global - (Vector3.UnitX + Vector3.UnitY) * .5f, global + Vector3.UnitZ + (Vector3.UnitX + Vector3.UnitY) * .5f);
            var entities = GetEntitiesAround(global);
            foreach (var entity in entities)
            {
                var entitybox = new BoundingBox(entity.Transform.Global - (Vector3.UnitX + Vector3.UnitY) * .2f, entity.Transform.Global + Vector3.UnitZ * entity.Physics.Height + (Vector3.UnitX + Vector3.UnitY) * .2f);
                if (blockbox.Intersects(entitybox))
                    return false;
            }
            return true;
        }
        public List<GameObject> GetEntitiesAround(Vector3 global)
        {
            var chunks = GetChunks(global.GetChunkCoords(), 1);
            List<GameObject> entities = new List<GameObject>();
            foreach (var ch in chunks)
                entities.AddRange(ch.GetObjects());
            return entities;
        }
        public int GetSizeInChunks()
        {
            return this.Size.Chunks;
        }
        public Town GetTown()
        {
            return this.Town;
        }
        public IWorld GetWorld()
        {
            return this.World;
        }
        public int GetMaxHeight()
        {
            return MaxHeight;
        }
        public WorldPosition GetMouseover()
        {
            return this.Mouseover;
        }
        public void SetMouseover(WorldPosition pos)
        {
            this.Mouseover = pos;
        }
        public Color GetAmbientColor()
        {
            return this.AmbientColor;
        }
        public void SetAmbientColor(Color color)
        {
            this.AmbientColor = color;
        }
        public MapThumb GetThumb()
        {
            return this.Thumb;
        }
        public double GetDayTimeNormal()
        {
            return this.DayTimeNormal;
        }
        #endregion

        Chunk Generate(Vector2 chunkvector)
        {
            System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
            Chunk newChunk = Chunk.Create(this, (int)chunkvector.X, (int)chunkvector.Y); //(Map.Instance.MapArgs, (int)pos.X, (int)pos.Y, Map.Instance.GetSeedArray());

            newChunk.InitCells(this.World.GetMutators().ToList());//.FinalizeCells(Net.Server.Random); // WARNING!
            newChunk.ResetVisibleCells();

            newChunk.UpdateHeightMap();

            this.World.GetMutators().ToList().ForEach(m => m.Finally(newChunk));

            return newChunk;
        }

        readonly MapRules _Rules = new MapRules() { UnloadChunks = false, SaveChunks = false };
        public MapRules Rules { get { return this._Rules; ; } }

        public void EventOccured(Message.Types type, params object[] p)
        {
            if (this.Net != null)
                this.Net.EventOccured(type, p);
        }
    }
}
