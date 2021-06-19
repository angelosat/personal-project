using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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


namespace Start_a_Town_
{
  
    public class Map : Component, IDisposable
    {
        public override object Clone()
        {
            return this;
        }

        //public override string ToString()
        //{
        //    //return "ChunkLoader: " + ChunkLoader.Status + //State.ToString() +
        //    //    "\nChunkLighter: " + ChunkLighter.Status + //.State.ToString() +
        //    //    "\nPathfinder: " + Pathfinding.Status + //.State.ToString() +
        //    //    "\nChunks in memory: " + ChunkLoader.Count +
        //    //    "\nActive Chunks: " + ActiveChunks.Count +
        //    //    "\nChunks Drawn: " + ChunksDrawn +
        //    //    "\nCulling checks: " + CullingChecks +
        //    //    "\nTiles Drawn: " + TilesDrawn +
        //    //    "\nTile Outlines Drawn: " + TileOutlinesDrawn +
        //    //    "\nTotal Tiles Drawn: " + (TilesDrawn + TileOutlinesDrawn) +
        //    //    "\nObjects Drawn: " + ObjectsDrawn.ToString() +

        //    return "Name: " + this["Name"] + "\nSeed: " + get; // SeedArray[0] + " " + SeedArray[1] + " " + SeedArray[2] + " " + SeedArray[3];
        //}

        //public Selection Selection;
        public struct MapSize
        {
            public string Name { get; set; }
            public int Size { get; set; }
            public MapSize(string name, int size)
                : this()
            {
                Name = name;
                Size = size;
            }
        }
        public static List<MapSize> Sizes
        { get { return new List<MapSize>() { new MapSize("Tiny", 64), new MapSize("Small", 128), new MapSize("Normal", 256), new MapSize("Huge", 512) }; } }

        Queue<Chunk> ChunksToActivate;
        public Dictionary<Vector2, Chunk> ActiveChunks;

         Tile.Types _DefaultTile;// = Tile.Types.Soil;
         public Tile.Types DefaultTile
         {
             get { return _DefaultTile; }
             set { _DefaultTile = value; }
         }
         public World World;
        public bool Lighting = true;
        public int TimeSpeed = 1;
        public const int Zenith = 14;
        public TimeSpan Time// = new TimeSpan(Zenith, 0, 0); //= TimeSpan.Zero;//
        { get { return (TimeSpan)this["Time"]; } set { this["Time"] = value; } }
       // public double Seconds;
        public double DayTimeNormal = 0;
        public bool AddTime()
        {

            Time = Time.Add(new TimeSpan(0, TimeSpeed, 0));
            //double normal = (Time.TotalMinutes - 120) / 1440f;
            double normal = (Time.TotalMinutes - 60*(Zenith - 12)) / 1440f;
            double nn = normal * 2 * Math.PI;
            nn = 3*Math.Cos(nn);
           // nn = Math.Pow(nn, 0.5f);
            DayTimeNormal = Math.Max(0, Math.Min(1, (1 + nn) / 2f));
         //   DayTimeNormal = Math.Pow(DayTimeNormal, 2f);
            SkyDarkness = (byte)(Math.Round(DayTimeNormal * SkyDarknessMax));

            
            return true;
        }

        public byte SkyDarkness = 0, SkyDarknessMax = 13;
        public Color AmbientColor = Color.MidnightBlue; //Color.MediumPurple; //Color.Lerp(Color.White, Color.Cornsilk, 0.5f);
        public bool Fog = false, BorderShading = true;
        public bool 
            HideUnderground = true,
            HideOverground = false;


        public static int SeaLevel, VisibleCellCount = 0;

        public Dictionary<int, Rectangle[,]> BaseTileRegions;

        public Game1 game;
      //  public ContentManager content;
        public bool hasClicked = false;

        public static int MaxHeight = 128;//256; //
        public  Random Random;

        //static Map _Instance;
        //public static Map Instance
        //{
        //    get { return _Instance; }
        //    set { _Instance = value; }
        //}

        public event EventHandler TopMostEntityChanged;
        public GameObject NextHoverEntity;
        GameObject _TopMostEntity;
        public GameObject TopMostEntity
        {
            get { return _TopMostEntity; }
            set
            {
                GameObject old = _TopMostEntity;
                _TopMostEntity = value;
                if (old != _TopMostEntity)
                    OnTopMostEntityChanged();
            }
        }
        void OnTopMostEntityChanged()
        {
            if (TopMostEntityChanged != null)
                TopMostEntityChanged(this, EventArgs.Empty);
        }

        public static float MaxDepth = 0, MinDepth = 0;


        static public Texture2D TerrainSprites, CharacterSprites, ShaderMouseMap;


        public List<Texture2D> VisibleTileTypes;

        public static event InputEvent DrawLevelChanged;

        
        public Vector2 tileLocation = new Vector2(16, 8);


        public const double GroundDensity = 0.1; //0.2
        //uint _Seed;
        //public uint Seed
        //{
        //    get { return _Seed; }
        //    set
        //    {
        //        _Seed = value;
        //        //SeedArray = new byte[]{
        //        //(byte)(Seed >> 24),
        //        //(byte)(Seed >> 16),
        //        //(byte)(Seed >> 8),
        //        //(byte)Seed};

        //        Random = new Random((int)(Seed - int.MaxValue));
        //    }
        //}
       // public byte[] SeedArray;

        public byte[] GetSeedArray()
        {
            int Seed = GetProperty<int>("Seed");
            return new byte[]{
                (byte)(Seed >> 24),
                (byte)(Seed >> 16),
                (byte)(Seed >> 8),
                (byte)Seed};
        }
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
            Shadow = Game1.Instance.Content.Load<Texture2D>("Graphics/shadow");
            //Tile.Initiliaze();
            Tile.Initialize();
            //TileOverhang.Initialize();
            Cell.Initialize();
            //Objects.Tree.Initialize();

            

            ItemSheet = Game1.Instance.Content.Load<Texture2D>("Graphics/ItemSheet");
            ItemSheet.Name = "Default item sprites";
            int iconsH = ItemSheet.Width / 32, iconsV = ItemSheet.Height / 32;
            Icons = new List<Rectangle>(iconsH * iconsV);
            for (int j = 0; j < iconsV; j++)
                for (int i = 0; i < iconsH; i++)
                    Icons.Add(new Rectangle(i * 32, j * 32, 32, 32));


            
        }
        


        #region Initialization
        public WorldArgs MapArgs;
        Map(WorldArgs a)
        {
            MapArgs = a;
            //Seed = a.Seed;
            //Instance = this;
            //DrawLevel.Min = 0;
            //DrawLevel.Max = MaxHeight - 1;

            //    Components.Add("Info", new InfoComponent(GameObject.Types.Map, a.Name, "A world."));
            Properties["Name"] = a.Name;
            Properties["Seed"] = a.Seed;
            Properties["Caves"] = a.Caves;
            Properties["Flat"] = a.Flat;
            Properties["Trees"] = a.Trees;
            Properties["Player"] = null;
            Properties["Time"] = new TimeSpan(Zenith, 0, 0);
            //Random = new Random((int)(a.Seed - int.MaxValue));
            Random = new Random(a.Seed);
            //SaveDir = Directory.GetCurrentDirectory() + @"/Saves/";
            SeaLevel = MaxHeight / 2 + 4; //68; //
            Lighting = a.Lighting;
            this.DefaultTile = Tile.Types.Soil;// a.DefaultTile;
            //Initialize();

            //Wall.Init();
            //Selection = new Selection();
            //CellSelection = new CellReference();
            //ChunkLoader.Begin();
            //ChunkLighter.Begin();

            // Log = new Log();
            //VisibleChunks = new SortedList<float, Chunk>();
            ActiveChunks = new Dictionary<Vector2, Chunk>();
            ChunksToLight = new Queue<Chunk>();
            ChunksToActivate = new Queue<Chunk>();
             
            //Camera.RotationChanged += new EventHandler<EventArgs>(Camera_RotationChanged);

        }

        //void Camera_RotationChanged(object sender, EventArgs e)
        //{
        //    ChunkSprites.Clear();
        //    Controller.Instance.MouseoverLast = null;
        //    Controller.Instance.MouseoverNext = null;
        //}


        #endregion

        public int DrawLevel = MaxHeight - 1;


        void Controller_KeyPress(object sender, KeyEventArgs2 e)
        {
            if (Controller.Instance.GetKeys().Contains(Microsoft.Xna.Framework.Input.Keys.OemMinus))
                DrawLevel-=10;
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
            return BaseTileRegions[type][Random.Next(BaseTileRegions[type].Length - 1), 0];
        }

        float t = 0;
        #region Updating
        public void Update(GameTime gt, Camera camera)
        {
            //Selection.Update();
            // GetMouseoverTile(Controller.Instance, camera);
            t -= GlobalVars.DeltaTime;
            if (t <= 0)
            {
                AddTime();//Seconds + GlobalVars.DeltaTime*60);// / 60f);
                t = 60;
            }
            AnimateWater();
            while (ChunksToActivate.Count > 0)
            {
                Chunk chunk = this.ChunksToActivate.Dequeue();
                ActiveChunks[chunk.MapCoords] = chunk;
                //chunk.CellsToUpdate = new Queue<Cell>(chunk.CellGrid2);

                if (ActiveChunks.Count > ChunkLoader.ChunkMemoryCapacity)
                {
                    float maxDist = 0, dist;
                    Vector2 furthestChunk = new Vector2();
                    foreach (KeyValuePair<Vector2, Chunk> pair in ActiveChunks)
                    {
                        dist = Vector2.Distance(chunk.MapCoords, pair.Key);
                        if (dist > maxDist)
                        {
                            maxDist = dist;
                            furthestChunk = pair.Key;
                        }
                    }
                    ActiveChunks.Remove(furthestChunk);
                    ChunkLoader.UnloadChunk(furthestChunk);

                }

                //foreach (Chunk n in Map.Instance.GetChunkNeighbors(chunk))
                //    n.UpdateBorderOutlines();


            }

            //LightChunks();

            //Console.WriteLine("frame");
            Dictionary<Vector2, Chunk> copyOfActiveChunks = new Dictionary<Vector2, Chunk>(ActiveChunks);
            foreach (KeyValuePair<Vector2, Chunk> chunk in copyOfActiveChunks)
                chunk.Value.Update(this, gt);
        }
        float WaterAnim = 20;
        private void AnimateWater()
        {
            WaterAnim -= GlobalVars.DeltaTime;
            if (WaterAnim <= 0)
            {
                WaterAnim = 20;
                TileSprite water = Tile.TileSprites[Tile.Types.Water];
                Rectangle[,] sources = water.SourceRects;
                water.SourceRects = new Rectangle[,] { { sources[0, 1], sources[0, 2], sources[0, 3], sources[0, 0] } };
                //    Console.WriteLine(TileBase.SourceRects[TileBase.Types.Water]);
            }
        }

        #endregion

        /// <summary>
        /// Removes an object from the world without sending it a death message.
        /// </summary>
        /// <param name="obj">The object to be removed.</param>
        /// <returns>Returns true if the object was succesfully removed.</returns>
        static public bool RemoveObject(GameObject obj)
        {
            if (obj == null)
            {
                Log.Enqueue(Log.EntryTypes.System, "Tried to remove null object.");
                return false;
            }
            //obj.HandleMessage(new GameObjectEventArgs(Message.Types.Remove));
            MovementComponent posComp;
            if (!obj.TryGetComponent<MovementComponent>("Position", out posComp))
                return false;
            //return Position.RemoveObject(obj, posComp.CurrentPosition);
            
                if (posComp["Position"] == null)
                    return false;
                // TODO: cell solidity should be changed by the removed object
                //PhysicsComponent ph;
                //if (obj.TryGetComponent<PhysicsComponent>("Physics", out ph))
                //{
                //    if (ph.GetProperty<bool>("Solid"))
                //    {
                //        float z = 0;
                //        while (z < Map.MaxHeight && z < ph.GetProperty<int>("Height"))
                //        {
                //            Cell cell;
                //            if (Position.TryGetCell(posComp.GetProperty<Position>("Position").Global + new Vector3(0, 0, z), out cell))
                //                cell.Solid = false;
                //            z++;
                //        }
                //    }
                //}


                Chunk chunk = posComp.GetProperty<Position>("Position").GetChunk();
                //return Chunk.RemoveObject(obj, chunk);
                bool removed = Chunk.RemoveObject(obj, chunk);
                if (!removed)
                {
                    Log.Enqueue(Log.EntryTypes.Default, obj.Name + " failed to be removed from chunk " + chunk.MapCoords);
                }
                //obj.Exists = false;
            return false;
        }


        public Queue<Chunk> ChunksToLight;
        //Object TopMostEntity;
        //public CellReference CellSelection;
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

        public void Render(SpriteBatch sb, Camera camera)
        {
            Dictionary<Vector2, Chunk> copyOfActiveChunks = new Dictionary<Vector2, Chunk>(ActiveChunks);
            int oldx = camera.X, oldy = camera.Y;
            float oldZoom = camera.Zoom;
            //double oldRotation = camera.Rotation;
            //camera.Rotation = 0;
            camera.Zoom = 1;
            //camera.X = -TileBase.Width * Chunk.Size / 2;
            //camera.Y = -Map.MaxHeight * TileBase.Height / 2;

            if (camera.Rotation == 0)
            {
                camera.X = -Chunk.Width / 2;
                camera.Y = 0;// -Chunk.Height / 2;
            }
            if (camera.Rotation == 1)
            {
                camera.X = -Chunk.Width + Tile.Width / 2;
                camera.Y = -Chunk.Width / 4;// -Chunk.Height / 2;
            }
            if (camera.Rotation == 2)
            {
                camera.X = -Chunk.Width / 2;
                camera.Y = - Chunk.Width / 2;// -Chunk.Height / 2;
            }
            if (camera.Rotation == 3)
            {
                camera.X = -Tile.Width / 2;
                camera.Y = -Chunk.Width / 4;
            }

            //Coords.Rotate(camera, -Chunk.Width / 2, 0, out camera.X, out camera.Y);

            foreach (KeyValuePair<Vector2, Chunk> chunk in copyOfActiveChunks.OrderBy(foo => foo.Value.GetDepthFar(camera))) //Depth))
            {
                ChunkTexture chunkTex;
                if (ChunkSprites.TryGetValue(chunk.Value, out chunkTex))
                    continue;

                GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
                chunkTex.Bitmap = new RenderTarget2D(gfx, Chunk.Width, Chunk.Height);
                chunkTex.Depth = new RenderTarget2D(gfx, Chunk.Width, Chunk.Height, false, SurfaceFormat.Rg32, DepthFormat.Depth24, 0, RenderTargetUsage.PreserveContents); //Rg32
                chunkTex.Light = new RenderTarget2D(gfx, Chunk.Width, Chunk.Height); //Chunk.Width, Chunk.Height);
                chunkTex.Mouse = new RenderTarget2D(gfx, Chunk.Width, Chunk.Height); //Chunk.Width, Chunk.Height);
                gfx.SetRenderTargets(chunkTex.Bitmap, chunkTex.Depth, chunkTex.Light, chunkTex.Mouse);
                gfx.Clear(Color.Transparent);
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, Game1.Instance.Effect);
                Game1.Instance.Effect.CurrentTechnique = Game1.Instance.Effect.Techniques["Technique2"];
                Game1.Instance.Effect.CurrentTechnique.Passes[0].Apply();

                chunk.Value.Render(sb, camera, this);//, dMin, dMax);
                sb.End();
                gfx.SetRenderTarget(null);


                chunkTex.MouseMap = new Color[chunkTex.Mouse.Width * chunkTex.Mouse.Height];
                chunkTex.Mouse.GetData(chunkTex.MouseMap, 0, chunkTex.MouseMap.Length);

                ChunkSprites[chunk.Value] = chunkTex;

                string directory = Directory.GetCurrentDirectory() + @"/Screenshots/";
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                //string filename;
                //FileStream stream;

                //filename = @"chunk_" + (int)chunk.Value.Start.X + "_" + (int)chunk.Value.Start.Y + @"bitmap.png";
                //stream = new FileStream(directory + filename, System.IO.FileMode.OpenOrCreate);
                //chunkTex.Bitmap.SaveAsPng(stream, chunkTex.Bitmap.Width, chunkTex.Bitmap.Height);
                ////NotificationArea.Write("Screenshot saved as \"" + filename + "\".");
                //Console.WriteLine("Screenshot saved as \"" + filename + "\".");

                //filename = @"chunk_" + (int)chunk.Value.Start.X + "_" + (int)chunk.Value.Start.Y + @"mouse.png";
                //stream = new FileStream(directory + filename, System.IO.FileMode.OpenOrCreate);
                //chunkTex.Mouse.SaveAsPng(stream, chunkTex.Bitmap.Width, chunkTex.Bitmap.Height);
                ////NotificationArea.Write("Screenshot saved as \"" + filename + "\".");
                //Console.WriteLine("Screenshot saved as \"" + filename + "\".");

                //filename = @"chunk_" + (int)chunk.Value.Start.X + "_" + (int)chunk.Value.Start.Y + @"depth.png";
                //stream = new FileStream(directory + filename, System.IO.FileMode.OpenOrCreate);
                //chunkTex.Depth.SaveAsPng(stream, chunkTex.Bitmap.Width, chunkTex.Bitmap.Height);
                ////NotificationArea.Write("Screenshot saved as \"" + filename + "\".");
                //Console.WriteLine("Screenshot saved as \"" + filename + "\".");

                //filename = @"chunk_" + (int)chunk.Value.Start.X + "_" + (int)chunk.Value.Start.Y + @"light.png";
                //stream = new FileStream(directory + filename, System.IO.FileMode.OpenOrCreate);
                //chunkTex.Light.SaveAsPng(stream, chunkTex.Bitmap.Width, chunkTex.Bitmap.Height);
                ////NotificationArea.Write("Screenshot saved as \"" + filename + "\".");
                //Console.WriteLine("Screenshot saved as \"" + filename + "\".");
                //stream.Close();

            }
            camera.X = oldx;
            camera.Y = oldy;
            camera.Zoom = oldZoom;
            //camera.Rotation = oldRotation;
        }

       // public SortedList<float, Position> Mouseover = new SortedList<float, Position>();
        //public Queue<Vector3> Mouseover = new Queue<Vector3>();
        public Position Mouseover;
       // public Mouseover<GameObject> Mouseover = new Mouseover<GameObject>();
        public void DrawTiles(SpriteBatch sb, Camera camera)//, SelectionArgs selection)
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
                if (Player.Actor["Position"].GetProperty<Position>("Position") != null)
                {
                    playerGlobal = new Nullable<Vector3>(Player.Actor.Global);

                    Sprite sprite = (Sprite)Player.Actor["Sprite"]["Sprite"];
                    Rectangle spriteBounds = sprite.GetBounds();
                    Rectangle screenBounds = camera.GetScreenBounds(playerGlobal.Value, spriteBounds);//(int)Start.X + x, (int)Start.Y + y, z, spriteBounds);
                    hiddenRects.Add(screenBounds);
                }
            }
            //playerGlobal = Player.Actor != null ? new Nullable<Vector3>(Player.Actor.Global) : null;
            //playerGlobal.Value = Player.Actor != null ? Player.Actor.Global : null;
           // Mouseovers = new SortedList<float, Vector3>();

            //Mouseover.Clear();
            Mouseover = null;
            
            foreach (KeyValuePair<Vector2, Chunk> chunk in copyOfActiveChunks.OrderBy(foo => foo.Value.GetDepthFar(camera))) //Depth))
            {
                Rectangle chunkBounds = camera.GetScreenBounds(chunk.Value.Start.X + Chunk.Size / 2, chunk.Value.Start.Y + Chunk.Size / 2, Map.MaxHeight / 2, chunk.Value.GetBounds());  //chunk.Value.GetBounds(camera);
                if (camera.ViewPort.Intersects(chunkBounds))
                {

                    camera.DrawChunk(sb, this, chunk.Value, playerGlobal, hiddenRects);
                }
            }

            if (Mouseover != null)
                camera.Mouseover(this, Mouseover.Global);

            //camera.Mouseover(new Queue<Position>(Mouseover.Values.ToList()));
            //Queue<Position> qms = new Queue<Position>(Mouseover.Values.ToList());
            //while (Mouseover.Count > 0 && Controller.Instance.MouseoverNext.Object == null)
            //    camera.Mouseover(Mouseover.Dequeue());
        }

        public bool TryGetBlockObjectOrDefault(Vector3 global, out GameObject blockObj)
        {
            Vector3 rnd = Position.Round(global);
            Chunk chunk;
            Cell cell;
            if (!Position.TryGet(this, rnd, out cell, out chunk))
            {
                blockObj = null;
                return false;
            }

            // if the block object exists, assign it and return true
            if (chunk.TryGetBlockObjectOrDefault(Position.ToLocal(rnd), out blockObj))
                return true;

            // otherwise, create a new block object
            if (!TileComponent.TileMapping.ContainsKey(cell.TileType))
            {
                blockObj = null;
                return false;
            }
            blockObj = GameObject.Create(TileComponent.TileMapping.ContainsKey(cell.TileType) ? TileComponent.TileMapping[cell.TileType].ID : GameObject.Types.Tile);
            return true;
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
        static public float GlobalDepthFar { get { return (-Engine.ChunkRadius * Chunk.Size * 2); } }
        static public float GlobalDepthNear { get { return (Engine.ChunkRadius + 2) * Chunk.Size * 2 + Map.MaxHeight; } }

        //static void GetGlobalDepthRange(out float globalDepthFar, out float globalDepthNear)
        //{
        //    globalDepthFar = (-Engine.ChunkRadius * Chunk.Size) * 2;
        //    globalDepthNear = (Engine.ChunkRadius * Chunk.Size + Chunk.Size - 1) * 2 + Map.MaxHeight - 1;
        //}

        public void DrawObjects(SpriteBatch sb, Camera camera)
        {
            Dictionary<Vector2, Chunk> copyOfActiveChunks = new Dictionary<Vector2, Chunk>(ActiveChunks);
            //float globaldNear = 0, globaldFar = 1;
            //foreach (KeyValuePair<Vector2, Chunk> chunk in copyOfActiveChunks.OrderBy(foo => foo.Value.GetDepthFar(camera))) //Depth))
            //{
            //    globaldFar = Math.Min(chunk.Value.GetDepthFar(camera), globaldFar);
            //    globaldNear = Math.Max(chunk.Value.GetDepthNear(camera), globaldNear);
            //    //Console.WriteLine(dMin + " " + dMax);
            //}

            foreach (KeyValuePair<Vector2, Chunk> chunk in copyOfActiveChunks)//.OrderBy(foo => foo.Value.GetDepthFar(camera))) //Depth))
            {
                

                //if (chunk.Value.Visible(camera))
                Rectangle chunkBounds = camera.GetScreenBounds(chunk.Value.Start.X + Chunk.Size / 2, chunk.Value.Start.Y + Chunk.Size / 2, Map.MaxHeight / 2, chunk.Value.GetBounds());  //chunk.Value.GetBounds(camera);
                if (camera.ViewPort.Intersects(chunkBounds))
                {
                    //chunk.Value.DrawHighlight(camera, chunkBounds);
                    float localdNear, localdFar;
                    chunk.Value.GetLocalDepthRange(camera, out localdNear, out localdFar);

                    Game1.Instance.Effect.Parameters["NearDepth"].SetValue(localdNear);
                    Game1.Instance.Effect.Parameters["FarDepth"].SetValue(localdFar);
                    chunk.Value.DrawObjects(sb, camera, Controller.Instance, Player.Instance, this);//, globaldMin, globaldMax);
                }
                Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
            }
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));

            //DrawTileSelection(camera);

        }

        public void DrawInterface(SpriteBatch sb, Camera camera)
        {
            Dictionary<Vector2, Chunk> copyOfActiveChunks = new Dictionary<Vector2, Chunk>(ActiveChunks);
            foreach (KeyValuePair<Vector2, Chunk> chunk in copyOfActiveChunks)//.OrderBy(foo => foo.Value.GetDepthFar(camera))) //Depth))
            {


                //if (chunk.Value.Visible(camera))
                Rectangle chunkBounds = camera.GetScreenBounds(chunk.Value.Start.X + Chunk.Size / 2, chunk.Value.Start.Y + Chunk.Size / 2, Map.MaxHeight / 2, chunk.Value.GetBounds());  //chunk.Value.GetBounds(camera);
                if (camera.ViewPort.Intersects(chunkBounds))
                {
                    //chunk.Value.DrawHighlight(camera, chunkBounds);
                    float localdNear, localdFar;
                    chunk.Value.GetLocalDepthRange(camera, out localdNear, out localdFar);

                    Game1.Instance.Effect.Parameters["NearDepth"].SetValue(localdNear);
                    Game1.Instance.Effect.Parameters["FarDepth"].SetValue(localdFar);
                    chunk.Value.DrawInterface(sb, camera);
                }
                Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));
            }
            Game1.Instance.Effect.Parameters["SourceRectangle"].SetValue(new Vector4(0, 0, 1, 1));

        }

        //public void DrawTileSelection(SpriteBatch sb, Camera camera)
        //{
        //    TileComponent tileComp = null;
        //    GameObject targ = Controller.Instance.Mouseover.Object as GameObject;
        //    if (targ == null)
        //        return;

        //    if (!targ.TryGetComponent<TileComponent>("Physics", out tileComp))
        //        return;


        //    //Cell cell = tileComp.Cell;
            
        //    MovementComponent movComp = targ.GetComponent<MovementComponent>("Position");
        //    Chunk chunk;
        //    Cell cell;
        //    if (!movComp.GetProperty<Position>("Position").TryGetInfo(out chunk, out cell))
        //        return;

        //    if (cell.TileType == Tile.Types.Air)
        //        return;

        //    float localdNear, localdFar;
        //    chunk.GetLocalDepthRange(camera, this, out localdNear, out localdFar);
        //    //Console.WriteLine(chunk.MapCoords);

        //    Game1.Instance.Effect.Parameters["NearDepth"].SetValue(localdNear);
        //    Game1.Instance.Effect.Parameters["FarDepth"].SetValue(localdFar);
        //    //Game1.Instance.GraphicsDevice.Textures[1] = ChunkSprites[chunk].Depth;
        //    float cd = 1 - Position.GetDepth(cell.LocalCoords + new Vector3(0,0,1)) / Chunk.Dmax;//.DepthMax();
        //    //float _cd = cd;
        //    //float _cd = 1 - Position.GetDepth(cell.LocalCoords + new Vector3(0, 0, 4), camera) / (float)Chunk.DepthMax(camera); //staticObj.GetInfo().Height
        //    //Game1.Instance.Effect.Parameters["ObjectHeight"].SetValue(_cd);

        //    TileSprite tileSprite = Tile.TileSprites[cell.TileType];
        //    Position pos = movComp.GetProperty<Position>("Position");
        //    Rectangle tileBounds = camera.GetScreenBounds(pos.Global, tileSprite.GetBounds());
        //    Vector2 screenLoc = new Vector2(tileBounds.X, tileBounds.Y);
        //    //float depth = (Position.GetDepth(pos.Global, camera) + 1 - GlobalDepthFar) / (float)(GlobalDepthNear - GlobalDepthFar);

        //    //int highlightIndex = tileComp.Face;
        //    int highlightIndex = tileComp.GetProperty<int>("Face");
        //    if (highlightIndex > 2)
        //        highlightIndex = 2 + (highlightIndex % 2);

        //    //camera.SpriteBatch.Draw(TerrainSprites, screenLoc, tileSprite.Highlights[highlightIndex], Color.White);
        //    //Game1.Instance.GraphicsDevice.Textures[1] = null;

        //    sb.Draw(TerrainSprites, screenLoc, tileSprite.Highlights[highlightIndex], Color.White, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, cd);//depth);//0);
        //}

        //public void DrawTileSelectionNew2(SpriteBatch sb, Camera camera)
        //{
        //    GameObject targ = Controller.Instance.MouseoverNext.Object as GameObject; //draw the mouseovernext to avoid flickering incase the mouse has moved since the last frame
        //    if (targ == null)
        //        return;

        //    TileComponent tileComp = null;
        //    if (!targ.TryGetComponent<TileComponent>("Physics", out tileComp))
        //        return;


        //    MovementComponent movComp = targ.GetComponent<MovementComponent>("Position");
        //    Chunk chunk;
        //    Cell cell;
        //    if (!movComp.GetProperty<Position>("Position").TryGetInfo(out chunk, out cell))
        //        return;

        //    float localdNear, localdFar;
        //    chunk.GetLocalDepthRange(camera, out localdNear, out localdFar);

        //    Sprite sprite = targ["Sprite"]["Sprite"] as Sprite;


        //    Color[] mousemap = sprite.MouseMap.Map;

        //    Rectangle screenBounds = camera.GetScreenBounds(movComp.GetProperty<Position>("Position").Global, sprite.GetBounds());
        //    Vector2 inTileCoords = new Vector2((Controller.Instance.msCurrent.X - screenBounds.X) / (float)camera.Zoom, (Controller.Instance.msCurrent.Y - screenBounds.Y) / (float)camera.Zoom);
        //    int faceIndex = (int)inTileCoords.Y * Tile.MouseMapSprite.Width + (int)inTileCoords.X;

        //    // check in case the mouse has moved further since the hittest occured
        //    if (faceIndex >= mousemap.Length)
        //        return;
        //    if (faceIndex < 0)
        //        return;



        //    Color faceColor = mousemap[faceIndex];
        //    float cd = (cell.X + cell.Y + cell.Z + faceColor.R + faceColor.G + faceColor.B) / (float)(Chunk.Size + Chunk.Size - 3 + Map.MaxHeight);
        //    cd = 1 - (localdFar + cd * (localdNear - localdFar));

        //    Position pos = movComp.GetProperty<Position>("Position");
        //    Rectangle tileBounds = camera.GetScreenBounds(pos.Global + new Vector3(0, 0, (float)faceColor.B), sprite.GetBounds());

        //    Vector2 screenLoc = new Vector2(tileBounds.X, tileBounds.Y);
        //    int highlightIndex = faceColor.R * 2 + faceColor.G;

        //    cd = Cell.GetGlobalDepth(targ.Global);
        //    sb.Draw(TerrainSprites, screenLoc, Tile.TileHighlights[highlightIndex], Color.White, 0, new Vector2(0, -sprite.Origin.Y + 16), camera.Zoom, SpriteEffects.None, 1);
        //   // Console.WriteLine(highlightIndex);
        //}

        public void DrawTileSelectionNew(SpriteBatch sb, Camera camera)
        {
            TileComponent tileComp = null;

            /// TODO:  draw the mouseovernext to avoid flickering incase the mouse has moved since the last frame
            /// BUT! if i draw the mouseovernext, then the tile highlights on walls don't work...

            if (Control.ToolManager.Instance.ActiveTool == null)
                return;
            GameObject targ = Control.ToolManager.Instance.ActiveTool.Target;// Controller.Instance.Mouseover.Object as GameObject;
            if (targ == null)
                return;
            
            if (!targ.TryGetComponent<TileComponent>("Physics", out tileComp))
                return;


            MovementComponent movComp = targ.GetComponent<MovementComponent>("Position");
            Chunk chunk;
            Cell cell;
            if (!movComp.GetProperty<Position>("Position").TryGetInfo(out chunk, out cell))
                return;



            float localdNear, localdFar;
            chunk.GetLocalDepthRange(camera, out localdNear, out localdFar);




            Sprite sprite = targ["Sprite"]["Sprite"] as Sprite;

            
            Color[] mousemap = sprite.MouseMap.Map[0][0];

            Rectangle screenBounds = camera.GetScreenBounds(movComp.GetProperty<Position>("Position").Global, sprite.GetBounds());//TileBase.TileSprites[TileBase.Types.Soil].GetBounds());
            Vector2 inTileCoords = new Vector2((Controller.Instance.msCurrent.X - screenBounds.X) / (float)camera.Zoom, (Controller.Instance.msCurrent.Y - screenBounds.Y) / (float)camera.Zoom);
            int faceIndex = (int)inTileCoords.Y * Tile.MouseMapSprite.Width + (int)inTileCoords.X;

            // check in case the mouse has moved further since the hittest occured
            //if (faceIndex >= mousemap.Length)
            //    return;
            //if (faceIndex < 0)
            //    return;




            Vector3 face = Control.ToolManager.Instance.ActiveTool.Face;
           // sprite.MouseMap.HitTest((int)inTileCoords.X, (int)inTileCoords.Y, out face);

            int rx, ry;
            Camera cam = new Camera(camera.Width, camera.Height); // have to create a new camera with the opposite rotation to maintain correct mouseover face orientation
            cam.Rotation = -camera.Rotation;
            Coords.Rotate(cam, face.X, face.Y, out rx, out ry);

            Cell faceCell; Chunk faceChunk;
            if (!Position.TryGet(this, targ.Global + new Vector3(rx, ry, face.Z), out faceCell, out faceChunk))
                return;
            faceChunk.GetLocalDepthRange(camera, out localdNear, out localdFar);
            Coords.Rotate(camera, faceCell.X, faceCell.Y, out rx, out ry);

            Vector3 local = new Vector3(rx, ry, faceCell.Z);
            float cd = Camera.GetCellDepth(localdNear, localdFar, ref local);

            Position pos = movComp.GetProperty<Position>("Position");
            Rectangle tileBounds = camera.GetScreenBounds(pos.Global + new Vector3(0, 0, face.Z), sprite.GetBounds());

            Vector2 screenLoc = new Vector2(tileBounds.X, tileBounds.Y);


            int highlightIndex = (int)(Math.Abs(face.X) * 2 + Math.Abs(face.Y));
            int frontback = InputState.IsKeyDown(System.Windows.Forms.Keys.RShiftKey) ? 1 : 0; //Menu

            //Vector3 rotatedGlobal;
            //Coords.Rotate(camera, faceCell.GetGlobalCoords(faceChunk), out rotatedGlobal);
            //cd = Cell.GetGlobalDepth(rotatedGlobal);
            Vector3 global = faceCell.GetGlobalCoords(faceChunk);
            int rgx, rgy;
            Coords.Rotate(camera, global.X, global.Y, out rgx, out rgy);
            Vector3 globalRotated = new Vector3(rgx, rgy, global.Z);
            // TODO: wtf? in this one i have to subtract from 1 but on chunk drawing i don't have to
            cd = 1 - Cell.GetGlobalDepth(globalRotated);

         //   sb.Draw(TerrainSprites, screenLoc, Tile.TileHighlights[frontback][highlightIndex], Color.White, 0, new Vector2(0, -sprite.Origin.Y + Tile.Depth), camera.Zoom, SpriteEffects.None, cd);//depth);//0);  + Tile.Height - Tile.Depth
         //   sb.Draw(TerrainSprites, screenLoc, Tile.TileHighlights[frontback][highlightIndex], Color.White * 0.4f, 0, new Vector2(0, -sprite.Origin.Y + Tile.Depth), camera.Zoom, SpriteEffects.None, 0);//depth);//0); + Tile.Height - Tile.Depth
            sb.Draw(TerrainSprites, screenLoc, Tile.TileHighlights[frontback][highlightIndex], Color.White, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, cd);//depth);//0);  + Tile.Height - Tile.Depth
            sb.Draw(TerrainSprites, screenLoc, Tile.TileHighlights[frontback][highlightIndex], Color.White * 0.4f, 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);//depth);//0); + Tile.Height - Tile.Depth
          //  sb.Draw(TerrainSprites, screenLoc, Tile.TileHighlights[frontback][highlightIndex], Color.White, 0, new Vector2(0, Tile.BlockHeight), camera.Zoom, SpriteEffects.None, cd);//depth);//0);  + Tile.Height - Tile.Depth
          //  sb.Draw(TerrainSprites, screenLoc, Tile.TileHighlights[frontback][highlightIndex], Color.White * 0.4f, 0, new Vector2(0, Tile.BlockHeight), camera.Zoom, SpriteEffects.None, 0);//depth);//0); + Tile.Height - Tile.Depth

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



        static public bool TryGetPosition(Map map, Vector3 global, out Position pos)
        {
            pos = new Position(map, global);
            return pos.GetCell() != null;
        }

        


        //public Chunk GetChunkAt(int chunkX, int chunkY)
        //{
        //    Vector2 pos = new Vector2(chunkX, chunkY);
        //    return ChunkLoader.Demand(pos);
        //}

        //public Chunk GetChunkAt(Vector3 chunkCoords)
        //{
        //    return GetChunkAt((int)chunkCoords.X, (int)chunkCoords.Y);
        //}
        //public Chunk GetChunkAt(Vector2 chunkCoords)
        //{
        //    return GetChunkAt((int)chunkCoords.X, (int)chunkCoords.Y);
        //}

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
            //ChunkLoader.Closing = true;
            //ChunkThread.Join();
            //ChunkThread.Abort();

           // ChunkLoader.End();
          //  ChunkLighter.End();

         //   Map.Instance = null;
            ActiveChunks.Clear();// = null;
            ChunksToActivate.Clear();

            //Controller.KeyPress -= Controller_KeyPress;

         //   Instance = null;
        }

        public string GetFullPath()
        {
            return GlobalVars.SaveDir + "Worlds/" + this["Name"] + "/";
        }

        //static string SaveDir; 
        internal new Tag Save()
        {
            ChunkLoader.Paused = true;
            ChunkLighter.Paused = true;
            string directory = GlobalVars.SaveDir + @"/Worlds/" + this["Name"] + "/";// Directory.GetCurrentDirectory() + @"/Saves/";
            string worldPath = @"/Saves/Worlds/" + this["Name"] + "/";
            string fullPath = worldPath + this["Name"] + ".map.sat";
            Tag mapTag;
            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);

                mapTag = new Tag(Tag.Types.Compound, "Map");

                //Tag seedTag = new Tag(Tag.Types.List, "SeedArray", Tag.Types.Byte);
                //foreach (Byte seed in SeedArray)
                //    seedTag.Add(new Tag(Tag.Types.Byte, "", seed));
                //Tag seedTag = new Tag(Tag.Types.UInt32, "Seed", Seed);
                mapTag.Add(new Tag(Tag.Types.Int, "Seed", GetProperty<int>("Seed")));
                mapTag.Add(new Tag(Tag.Types.Bool, "Caves", GetProperty<bool>("Caves")));
                mapTag.Add(new Tag(Tag.Types.Bool, "Trees", GetProperty<bool>("Trees")));
                mapTag.Add(new Tag(Tag.Types.Bool, "Flat", GetProperty<bool>("Flat")));
                mapTag.Add(new Tag(Tag.Types.Double, "Time", Time.TotalSeconds));

                Tag playerTag = new Tag(Tag.Types.Compound, "Player");
                if (Player.Actor != null)
                    playerTag.Add(new Tag(Tag.Types.Compound, Player.Actor.Name, Player.Actor.Save()));
                mapTag.Add(playerTag);

                mapTag.Add(new Tag(Tag.Types.String, "Name", GetProperty<string>("Name")));
                mapTag.WriteTo(writer);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                Chunk.Compress(stream, fullPath);

                stream.Close();

            }


            //Texture2D thumbnail = GetThumbnail();

            using (Texture2D thumbnail = GetThumbnail())
            {
                using (FileStream stream = new FileStream(Directory.GetCurrentDirectory() + worldPath + "thumbnailSmall.png", FileMode.OpenOrCreate))
                {
                    thumbnail.SaveAsPng(stream, thumbnail.Width, thumbnail.Height);
                    stream.Close();
                }
                using (FileStream stream = new FileStream(Directory.GetCurrentDirectory() + worldPath + "thumbnailSmaller.png", FileMode.OpenOrCreate))
                {
                    thumbnail.SaveAsPng(stream, thumbnail.Width / 2, thumbnail.Height / 2);
                    stream.Close();
                }
                using (FileStream stream = new FileStream(Directory.GetCurrentDirectory() + worldPath + "thumbnailSmallest.png", FileMode.OpenOrCreate))
                {
                    thumbnail.SaveAsPng(stream, thumbnail.Width / 4, thumbnail.Height / 4);
                    stream.Close();
                }
            }

            //if (Player.Actor != null)
            //    Chunk.RemoveObject(Player.Actor);
            foreach (KeyValuePair<Vector2, Chunk> pair in ActiveChunks)
                pair.Value.Save();
            //if (Player.Actor != null)
            //    Chunk.AddObject(Player.Actor);

            ChunkLoader.Paused = false;
            ChunkLighter.Paused = false;

            return mapTag;
        }


        static public Map Load(string filename)
        {
            //Initialize();
            Map map = new Map(new WorldArgs());
            DateTime start = DateTime.Now;
            string directory = GlobalVars.SaveDir;// Directory.GetCurrentDirectory() + @"/Saves/";
            //using (FileStream stream = new FileStream(directory + filename, System.IO.FileMode.Open))
            using (FileStream stream = new FileStream(filename, System.IO.FileMode.Open))
            {
                
                using (MemoryStream decompressedStream = Chunk.Decompress(stream))
                {
                    BinaryReader reader = new BinaryReader(decompressedStream); //stream);//
                    Tag mapTag = Tag.Read(reader);


                    map["Seed"] = (int)mapTag["Seed"].Value;
                    map["Flat"] = (bool)mapTag["Flat"].Value;
                    map["Caves"] = (bool)mapTag["Caves"].Value;
                    map["Trees"] = (bool)mapTag["Trees"].Value;
                    map["Time"] = TimeSpan.FromSeconds((double)mapTag["Time"].Value);

                    Tag playerTag = mapTag["Player"] as Tag;
                    List<Tag> tagList = playerTag.Value as List<Tag>;
                    if (tagList.Count>1)
                    {
                        GameObject player;
                        Dictionary<string, Tag> byName = playerTag.ToDictionary();
                        player = GameObject.Create(byName.First().Value);
                        player.Name = byName.First().Key;
                        map["Player"] = player;
                    }


                    map["Name"] = (string)mapTag["Name"].Value;
                }


                stream.Close();
            }
            Console.WriteLine("map loaded in " + (DateTime.Now - start).ToString() + " ms");
            return map;
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

        List<Vector2> GetSpiral2(Vector2 pos)//, int radius = Engine.ChunkRadius)
        {
            List<Vector2> list = new List<Vector2>();
            for (int i = 1; i <= Engine.ChunkRadius; i++)
            {
                for (int j = 0; j < i + i; j++)
                {
                    list.Add(pos + new Vector2(-i, -i + j));
                    list.Add(pos + new Vector2(-i + j, i));
                    list.Add(pos + new Vector2(i, i - j));
                    list.Add(pos + new Vector2(i - j, -i));
                }
            }
            return list;
        }

        List<Vector2> GetSpiral(Vector2 pos)//, int radius = Engine.ChunkRadius)
        {
            List<Vector2> list = new List<Vector2>();
            for (int i = -Engine.ChunkRadius; i <= Engine.ChunkRadius; i++)
                for (int j = -Engine.ChunkRadius; j <= Engine.ChunkRadius; j++)
                {
                    list.Add(pos + new Vector2(i, j));
                }
            return list.OrderBy(foo => Vector2.Distance(pos, foo)).ToList();
        }

        

        //GameObject _Focus;
        //public new GameObject FocusObject
        //{
        //    get { return _Focus; }
        //    set
        //    {

        //        _Focus = value;
        //        //Chunk chunk = GetChunkAt(_Focus.
        //        //Activate(_Focus.Chunk);
        //        ChunkLoader.Request(Vector2.Zero, Activate);
        //        ChunkLoader.Request(GetSpiral(Vector2.Zero), Activate);

        //    }
        //}

        public void Focus(Vector3 global)
        {
            Vector2 chunk = Chunk.GetChunkCoords(global);
        //    ChunkLoader.Request(chunk, Activate);
            List<Vector2> spiral = GetSpiral(chunk);
            ChunkLoader.Request(this, spiral, Activate);
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

            Map map = new Map(args);
        }



        public void HandleInput(InputState input)
        {

        }

        //public void DrawSelection(SpriteBatch sb, Camera camera)
        //{
        //    GameObject mouseover;
        //    if (!Controller.Instance.Mouseover.TryGet<GameObject>(out mouseover))
        //        return;

        //    SpriteComponent sprComp;
        //    if (!mouseover.TryGetComponent<SpriteComponent>("Sprite", out sprComp))
        //        return;

        //    if (sprComp.Flash)
        //        return;
        //    // TODO: this is also at the chunk.drawobjects
        //    if (sprComp.GetProperty<bool>("Hidden"))
        //        return;

        //    MovementComponent posComp;
        //    if (!mouseover.TryGetComponent<MovementComponent>("Position", out posComp))
        //        return;

        //    Vector3 g = mouseover.Global, off = sprComp.GetOffset();
        //    Rectangle bounds;
        //    camera.CullingCheck(g.X + off.X, g.Y + off.Y, g.Z + off.Z, sprComp.Sprite.GetBounds(), out bounds);
        //    Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
        //    Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);
        //    sb.Draw(sprComp.Sprite.Texture, screenLoc,
        //        //     sprComp.Sprite.SourceRect[sprComp.Variation][(sprComp.Orientation + (int)camera.Rotation) % sprComp.Sprite.SourceRect[sprComp.Orientation].Length], 
        //   sprComp.Sprite.SourceRect[sprComp.Variation][sprComp.GetOrientation(camera)], //SpriteComponent.GetOrientation(sprComp.Orientation, camera)],
        //        new Color(255, 255, 255, 127), 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);// Cell.GetGlobalDepth(g));
        //    //  Console.WriteLine(Controller.Instance.Mouseover.Depth);
        //    Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);

        //    sprComp.DrawHighlight(sb, camera, bounds);


        //    /// AND NOW TILE
        //    TileComponent tileComp = null;
        //    if (!mouseover.TryGetComponent<TileComponent>("Physics", out tileComp))
        //        return;

        //    Chunk chunk;
        //    Cell cell;
        //    if (!posComp.GetProperty<Position>("Position").TryGetInfo(out chunk, out cell))
        //        return;


        //    float localdNear, localdFar;
        //    chunk.GetLocalDepthRange(camera, out localdNear, out localdFar);




        //    Sprite sprite = mouseover["Sprite"]["Sprite"] as Sprite;


        //    Color[] mousemap = sprite.MouseMap.Map;

        //    Rectangle screenBounds = camera.GetScreenBounds(posComp.GetProperty<Position>("Position").Global, sprite.GetBounds());//TileBase.TileSprites[TileBase.Types.Soil].GetBounds());
        //    Vector2 inTileCoords = new Vector2((Controller.Instance.msCurrent.X - screenBounds.X) / (float)camera.Zoom, (Controller.Instance.msCurrent.Y - screenBounds.Y) / (float)camera.Zoom);
        //    int faceIndex = (int)inTileCoords.Y * Tile.MouseMapSprite.Width + (int)inTileCoords.X;

        //    // check in case the mouse has moved further since the hittest occured
        //    if (faceIndex >= mousemap.Length)
        //        return;
        //    if (faceIndex < 0)
        //        return;



        //    Color faceColor = mousemap[faceIndex];

        //    float cd = (cell.X + cell.Y + cell.Z + faceColor.R + faceColor.G + faceColor.B) / (float)(Chunk.Size + Chunk.Size - 3 + Map.MaxHeight);
        //    cd = 1 - (localdFar + cd * (localdNear - localdFar));

        //    Position pos = posComp.GetProperty<Position>("Position");
        //    Rectangle tileBounds = camera.GetScreenBounds(pos.Global + new Vector3(0, 0, (float)faceColor.B), sprite.GetBounds());// sprite.GetBounds()); // TileBase.TileSprites[TileBase.Types.Soil]

        //    screenLoc = new Vector2(tileBounds.X, tileBounds.Y);
        //    int highlightIndex = faceColor.R * 2 + faceColor.G;

        //    sb.Draw(TerrainSprites, screenLoc, Tile.TileHighlights[highlightIndex], Color.White, 0, new Vector2(0, -sprite.Origin.Y + 16), camera.Zoom, SpriteEffects.None, cd);//depth);//0);

        //}

        internal void DrawObjectSelection(SpriteBatch sb, Camera camera)
        {
            GameObject mouseover;// = Controller.Instance.Mouseover.Object as GameObject;
            if (!Controller.Instance.Mouseover.TryGet<GameObject>(out mouseover))
                return;
            mouseover.DrawMouseover(sb, camera);
            MovementComponent posComp;

            if ((bool)mouseover["Sprite"]["Flash"])
                return;
            // TODO: this is also at the chunk.drawobjects
            if ((bool)mouseover["Sprite"]["Hidden"])
                return;

            if (!mouseover.TryGetComponent<MovementComponent>("Position", out posComp))
                return;

            Vector3 g = mouseover.Global, off = SpriteComponent.GetOffset((Vector3)mouseover["Sprite"]["Offset"], (double)mouseover["Sprite"]["OffsetTimer"]);//sprComp.GetOffset();
            Rectangle bounds;
            // camera.CullingCheck(g.X + off.X, g.Y + off.Y, g.Z + off.Z, sprComp.Sprite.GetBounds(), out bounds);
            Sprite sprite = (Sprite)mouseover["Sprite"]["Sprite"];
            camera.CullingCheck(g.X + off.X, g.Y + off.Y, g.Z + off.Z, sprite.GetBounds(), out bounds);
            Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
            Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);
            int variation = (int)mouseover["Sprite"]["Variation"];
            sb.Draw(sprite.Texture, screenLoc, 
                sprite.SourceRect[variation][SpriteComponent.GetOrientation((int)mouseover["Sprite"]["Orientation"], camera, sprite.SourceRect[variation].Length)],// sprComp.GetOrientation(camera)], 
                new Color(255, 255, 255, 127), 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);

            Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);

        }

        //internal void DrawObjectSelection(SpriteBatch sb, Camera camera)
        //{
        //    GameObject mouseover;// = Controller.Instance.Mouseover.Object as GameObject;
        //    if (!Controller.Instance.Mouseover.TryGet<GameObject>(out mouseover))
        //        return;

        //    SpriteComponent sprComp;
        //    MovementComponent posComp;
        //    if (mouseover.TryGetComponent<SpriteComponent>("Sprite", out sprComp))
        //    {
        //        if (sprComp.Flash)
        //            return;
        //        // TODO: this is also at the chunk.drawobjects
        //        if (sprComp.GetProperty<bool>("Hidden"))
        //            return;
        //        if (mouseover.TryGetComponent<MovementComponent>("Position", out posComp))
        //        {

        //            Vector3 g = mouseover.Global, off = sprComp.GetOffset();
        //            Rectangle bounds;
        //            camera.CullingCheck(g.X + off.X, g.Y + off.Y, g.Z + off.Z, sprComp.Sprite.GetBounds(), out bounds);
        //            Vector2 screenLoc = new Vector2(bounds.X, bounds.Y);
        //            Game1.Instance.Effect.Parameters["Alpha"].SetValue(0.5f);
        //            sb.Draw(sprComp.Sprite.Texture, screenLoc,

        //           sprComp.Sprite.SourceRect[sprComp.Variation][sprComp.GetOrientation(camera)],
        //                new Color(255, 255, 255, 127), 0, Vector2.Zero, camera.Zoom, SpriteEffects.None, 0);

        //            Game1.Instance.Effect.Parameters["Alpha"].SetValue(1);
        //        }
        //    }
        //}

        public Texture2D GetThumbnail()
        {
            GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            float zoom = 1 / 8f;
            //int width = (int)((Engine.ChunkRadius + Engine.ChunkRadius + 1) * Chunk.Size * 32), height = (int)((Engine.ChunkRadius + Engine.ChunkRadius + 1) * Chunk.Size * 32);// + Map.MaxHeight * 8);
            int width = (int)((Engine.ChunkRadius + Engine.ChunkRadius + 1) * Chunk.Size * Tile.Width * zoom);//, height = (int)((Engine.ChunkRadius + Engine.ChunkRadius + 1) * Chunk.Size * 8);// + Map.MaxHeight * 8);
            //int width = Game1.Instance.graphics.PreferredBackBufferWidth, height = Game1.Instance.graphics.PreferredBackBufferHeight;
            Camera camera = new Camera(width, width, x: 0, y: 0, z: Map.MaxHeight / 2, zoom: zoom);
            SpriteBatch sb = new SpriteBatch(gfx);

            //RenderTarget2D render = new RenderTarget2D(gfx, width, width);//(Engine.ChunkRadius + Engine.ChunkRadius + 1) * Chunk.Size * 4, (Engine.ChunkRadius + Engine.ChunkRadius + 1) * Chunk.Size * 4);// + 4 * Map.MaxHeight));
            RenderTarget2D MapRender = new RenderTarget2D(gfx, width, width, false, SurfaceFormat.Color, DepthFormat.Depth24Stencil8, 0, RenderTargetUsage.PreserveContents);//, false, SurfaceFormat.Vector4, DepthFormat.Depth16);
            RenderTarget2D DepthMap = new RenderTarget2D(gfx, width, width, false, SurfaceFormat.Rg32, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);
            RenderTarget2D LightMap = new RenderTarget2D(gfx, width, width, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);

            gfx.SetRenderTargets(MapRender, DepthMap, LightMap);
            gfx.Clear(Color.Black);
            //UpdateDepths(camera);
            gfx.Textures[2] = ShaderMouseMap;// Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap - Copy");
            gfx.SamplerStates[2] = SamplerState.PointClamp;
            Game1.Instance.Effect.CurrentTechnique = Game1.Instance.Effect.Techniques["RealTime"];
            //Game1.Instance.Effect.CurrentTechnique.Passes[0].Apply();
            Game1.Instance.Effect.Parameters["Viewport"].SetValue(new Vector2(width));//(Engine.ChunkRadius + Engine.ChunkRadius + 1) * Chunk.Size * 8));
            Game1.Instance.Effect.CurrentTechnique.Passes[0].Apply();
            sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone, Game1.Instance.Effect);
            //sb.Begin();
            gfx.Textures[2] = ShaderMouseMap;// Game1.Instance.Content.Load<Texture2D>("Graphics/mousemap - Copy");
            gfx.SamplerStates[2] = SamplerState.PointClamp;
            DrawTiles(sb, camera);
            sb.End();
            Game1.Instance.Effect.CurrentTechnique = Game1.Instance.Effect.Techniques["TechniqueObjects"];
            Game1.Instance.Effect.CurrentTechnique.Passes[0].Apply();

            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullCounterClockwise, Game1.Instance.Effect);
            DrawObjects(sb, camera);
            sb.End();

            gfx.SetRenderTarget(null);
            return MapRender;

            ///TOKALO
            //GraphicsDevice gfx = Game1.Instance.GraphicsDevice;
            //float zoom = 1 / 8f;
            ////int width = (int)(5 * Chunk.Size * 32), height = (int)(5 * Chunk.Size * 32);// + Map.MaxHeight * 8);
            ////int width = (int)(5 * Chunk.Size * 8), height = (int)(5 * Chunk.Size * 8);// + Map.MaxHeight * 8);
            //int width = Game1.Instance.graphics.PreferredBackBufferWidth, height = Game1.Instance.graphics.PreferredBackBufferHeight;
            //Camera camera = new Camera(width, height, x: 0, y: 0, z: Map.MaxHeight / 2, zoom: zoom);
            //SpriteBatch sb = new SpriteBatch(gfx);
            ////RenderTarget2D render = new RenderTarget2D(gfx, (int)(camera.ViewPort.Width), (int)(camera.ViewPort.Height));
            //RenderTarget2D render = new RenderTarget2D(gfx, width, height);// + 4 * Map.MaxHeight));
            //gfx.SetRenderTarget(render);
            //gfx.Clear(Color.Black);
            ////Game1.Instance.Effect.CurrentTechnique = Game1.Instance.Effect.Techniques["Thumbnail"];
            ////Game1.Instance.Effect.CurrentTechnique.Passes[0].Apply();
            //sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.AnisotropicClamp, DepthStencilState.Default, RasterizerState.CullNone, Game1.Instance.Effect);
            ////sb.Begin();
            //DrawOld(sb, camera);
            //sb.End();
            //gfx.SetRenderTarget(null);
            //return render;
        }

        static public Map Create(WorldArgs a)
        {
            Map map = new Map(a);
            return map;
        }
        
    }
    //public delegate void TargetHandler(TargetArgs a);
    //public struct TargetArgs
    //{
    //    Position CellChunkPair;
    //    Object Target;
    //    public TargetArgs(Position pair, object target)
    //    {
    //        CellChunkPair = pair;
    //        Target = target;
    //    }
    //}
}
