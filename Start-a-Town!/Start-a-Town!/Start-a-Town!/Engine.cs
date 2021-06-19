using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using Start_a_Town_.Components;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;

namespace Start_a_Town_
{
    public class EngineArgs
    {
        public bool HideWalls, HideTerrain, BlockOutlines;
        public List<BlockBorderToken> BlockBorders = new List<BlockBorderToken>();
        EngineArgs()
        {
            this.HideWalls = Engine.HideWalls;
            this.HideTerrain = Engine.HideTerrain;
            this.BlockOutlines = Engine.BlockOutlines;
        }

        static public EngineArgs Default
        { get { return new EngineArgs(); } }
    }
    public class Engine : Component
    {
        #region Singleton
        static Engine _Instance;
        static public Engine Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Engine();
                return _Instance;
            }
        }
        #endregion

        public override string ComponentName
        {
            get { return "Engine"; }
        }

        //Map Map;
        public const int ChunkRadius = 2;// 4;//1;//3;
        public static XDocument Config = XDocument.Load("config.xml");

        static XmlDocument _Settings;
        public static XmlDocument Settings
        {
            get
            {
                if (_Settings == null)
                    _Settings = InitSettings();
                return _Settings;
            }
            set { _Settings = value; }
        }

        static public int LightQueries;
        //    static  Stack<Map> Maps;
        static IMap _Map;
        static public IMap Map
        {
            get { return _Map; }
            set
            {
                _Map = value;
                //if(!value.IsNull())
                //    Random = new RandomThreaded(value.World);
            }
        }
        //   {get{return Maps.Peek();}{
        static public int TargetFps = 60;
        static public float Tick = 1000 / (float)TargetFps;
        static public int MaxChunkLoadThreads = 3;
        static public Stopwatch TileDrawTime = Stopwatch.StartNew();//new Stopwatch();
        //   static public Stopwatch Peak = new Stopwatch();
        static public TimeSpan Average = TimeSpan.Zero;

        public override string ToString()
        {
            return "ChunkLoader: " + ChunkLoader.Status +
              //  "\nChunkLighter: " + ChunkLighter.Status +
          //      "\nPathfinder: " + Pathfinding.Status +
                "\nChunks in memory: " + ChunkLoader.Count +
                "\nActive Chunks: " + Engine.Map.GetActiveChunks().Count +
                //"\nChunks Drawn: " + Engine.Map.ChunksDrawn +
                //"\nCulling checks: " + Engine.Map.CullingChecks +
                //"\nTiles Drawn: " + Engine.Map.TilesDrawn +
                //"\nTile Outlines Drawn: " + Engine.Map.TileOutlinesDrawn +
                //"\nTotal Tiles Drawn: " + (Engine.Map.TilesDrawn + Engine.Map.TileOutlinesDrawn) +
                "\nLight Queries: " + LightQueries +
                //"\nObjects Drawn: " + Engine.Map.ObjectsDrawn.ToString() +
                "\nTile draw time: " + TileDrawTime.Elapsed.ToString("0.00 ns");
        }

        static public bool HideWalls { get { return (bool)Instance["HideWalls"]; } set { Instance["HideWalls"] = value; } }
        static public bool HideTerrain { get { return (bool)Instance["HideTerrain"]; } set { Instance["HideTerrain"] = value; } }
        static public bool HideOccludedBlocks;// { get { return (bool)Instance["HideOccludedBlocks"]; } set { Instance["HideOccludedBlocks"] = value; } }
        static public bool CullDarkFaces { get { return (bool)Instance["CullDarkFaces"]; } set { Instance["CullDarkFaces"] = value; } }
        static public bool BlockOutlines { get { return (bool)Instance["BlockOutlines"]; } set { Instance["BlockOutlines"] = value; } }
        //Engine(XmlDocument settingsXml)
        //{
        //    Settings = settingsXml;
        //    this["AsyncLoading"] = bool.Parse(Settings["Settings"]["Engine"]["AsyncLoading"].InnerText);
        //    this["MouseTooltip"] = bool.Parse(Settings["Settings"]["Engine"]["MouseTooltip"].InnerText);
        //}

        //static XmlDocument InitSettings(GraphicsDevice gfx)
        static XmlDocument InitSettings()
        {
            XmlDocument settings = new XmlDocument();
            try { settings.Load("config.xml"); }
            catch (System.IO.FileNotFoundException e)
            {
                //GenerateConfigFile(gfx);//Game1.Instance.graphics.GraphicsDevice);
                GenerateConfigFile(Game1.Instance.graphics.GraphicsDevice);
                settings.Load("config.xml");
            }

            Instance["AsyncLoading"] = bool.Parse(settings["Settings"]["Engine"]["AsyncLoading"].InnerText);
            Instance["MouseTooltip"] = bool.Parse(settings["Settings"]["Interface"]["MouseTooltip"].InnerText);
            HideWalls = bool.Parse(settings["Settings"]["Engine"]["HideWalls"].InnerText);
            HideTerrain = bool.Parse(settings["Settings"]["Engine"]["HideTerrain"].InnerText);
            BlockOutlines = true;
            CullDarkFaces = false;
            HideOccludedBlocks = false;
            return settings;
        }

      //  static public Random Random = new Random();

        static public void Update(GameTime gt)
        {
            GameObject.Update();
            LightQueries = 0;
            //TileDrawTime.Reset()
        }

        public override object Clone()
        {
            return this;
        }

        static void GenerateConfigFile(GraphicsDevice gfx)
        {
            var displayMode = gfx.Adapter.SupportedDisplayModes[SurfaceFormat.Color].First();

            //new XDocument(
            //    new XElement("Settings",
            //        new XElement("Graphics",
            //            new XElement("Resolution",
            //                new XElement("Width", displayMode.Width.ToString()),
            //                new XElement("Height", displayMode.Height.ToString())
            //            ),
            //            new XElement("Fullscreen", false)
            //        ),
            //        new XElement("Engine",
            //            new XElement("AsyncLoading", true),
            //            new XElement("HideWalls", true),
            //            new XElement("HideTerrain", true)
            //        ),
            //        new XElement("Interface",
            //            new XElement("MouseTooltip", true),
            //            new XElement("UIScale", 1)
            //        )
            //    )
            //).Save("config.xml");

            new XDocument(
                new XElement("Settings",
                    new XElement("Video",
                        new XElement("Resolution",
                            new XElement("Width", displayMode.Width.ToString()),
                            new XElement("Height", displayMode.Height.ToString())
                        ),
                        new XElement("Fullscreen", false)
                    ),
                    new XElement("Engine",
                        new XElement("AsyncLoading", true),
                        new XElement("HideWalls", true),
                        new XElement("HideTerrain", true)
                    ),
                    new XElement("Interface",
                        new XElement("MouseTooltip", true),
                        new XElement("UIScale", 1)
                    ),
                    new XElement("Graphics",
                        new XElement("Particles", Components.Particles.ParticleDensityLevel.Current.Name)
                    )
                )
            ).Save("config.xml");

            //settings["Settings"]["Graphics"]["Resolution"]["Width"].InnerText = displayMode.Width.ToString();
            //settings["Settings"]["Graphics"]["Resolution"]["Height"].InnerText = displayMode.Height.ToString();
            //settings["Settings"]["Graphics"]["Fullscreen"].InnerText = false.ToString();

            //settings["Settings"]["Engine"]["AsyncLoading"].InnerText = true.ToString();
            //settings["Settings"]["Engine"]["HideWalls"].InnerText = true.ToString();
            //settings["Settings"]["Engine"]["HideTerrain"].InnerText = true.ToString();

            //settings["Settings"]["Interface"]["MouseTooltip"].InnerText = true.ToString();
            //settings["Settings"]["Interface"]["UIScale"].InnerText = 1.ToString();


        }

        static public void Init(Game1 game)
        {
            //game.graphics.PreferredBackBufferWidth = Int16.Parse(Engine.Settings["Settings"]["Graphics"]["Resolution"]["Width"].InnerText);
            //game.graphics.PreferredBackBufferHeight = Int16.Parse(Engine.Settings["Settings"]["Graphics"]["Resolution"]["Height"].InnerText);
            //game.graphics.IsFullScreen = bool.Parse(Engine.Settings["Settings"]["Graphics"]["Fullscreen"].InnerText);
            //game.graphics.ApplyChanges();


            var resolution = Engine.Config.Descendants("Graphics").FirstOrDefault();
            game.graphics.PreferredBackBufferWidth = (int?)resolution.Element("Resolution").Element("Width") ?? game.graphics.PreferredBackBufferWidth;
            game.graphics.PreferredBackBufferHeight = (int?)resolution.Element("Resolution").Element("Height") ?? game.graphics.PreferredBackBufferHeight;
            game.graphics.IsFullScreen = (bool?)resolution.Element("Fullscreen") ?? true;
            game.graphics.ApplyChanges();

        }


        internal static void InitializeComponents()
        {
            JobBoardComponent.Initialize();
            ConstructionOldComponent.Initialize();
            //UI.Nameplate.Reset();
        }

        static public void PlayGame(GameObject mapPlayer)//, Map map)
        {
            XDocument xml = Engine.Settings.ToXDocument();

            //var profile = xml.Root.Element("Profile");
            //if (profile.IsNull())
            //    xml.Root.Add(new XElement("Profile"));

            //var xLastChar = profile.Element("LastCharacter");
            //if (xLastChar.IsNull())
            //    profile.Add(new XElement("LastCharacter", mapPlayer.Name));
            //else
            //    xLastChar.Value = mapPlayer.Name;
            //xml.Save("config.xml");
            //Settings = xml.ToXmlDocument();

            Engine.Config.GetOrCreateElement("Profile").GetOrCreateElement("LastCharacter").Value = mapPlayer.Name;


            Nameplate.Reset();
            ChunkLoader.Restart();
            GC.Collect();

            //Rooms.Ingame ingame = new Rooms.Ingame();
            //ScreenManager.Add(ingame.Initialize());

            //string localHost = "127.0.0.1";
            //Net.Client.Connect(localHost, new PlayerData("host"), a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
            
            //Net.Client.RequestWorldInfo();
            Player.Actor = mapPlayer;

        }
    }
}
