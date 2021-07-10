﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.Diagnostics;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

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
    public class Engine// : EntityComponent
    {
        static Engine _Instance;
        static public Engine Instance => _Instance ??= new Engine();

        static Engine()
        {
            try { Config = XDocument.Load("config.xml"); }
            catch
            {
                GenerateDefaultConfigFile(Game1.Instance.graphics.GraphicsDevice);
                Config = XDocument.Load("config.xml");
            }
            _DrawRooms = Config.GetOrCreateElement("Settings").GetOrCreateElement("Engine").GetOrCreateElement("DrawRooms", false);
        }
        public const int ChunkRadius = 2;
        public static XDocument Config;
        static public void SaveConfig()
        {
            Config.Save("config.xml");
        }
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
        static MapBase _Map;
        static public MapBase Map
        {
            get => _Map;
            set => _Map = value;
        }
        static public int TicksPerSecond = 60;
        static public float Tick = 1000 / (float)TicksPerSecond;
        static public int MaxChunkLoadThreads = 3;
        static public Stopwatch TileDrawTime = Stopwatch.StartNew();
        static public TimeSpan Average = TimeSpan.Zero;

        public override string ToString()
        {
            return 
                "\nActive Chunks: " + Engine.Map.GetActiveChunks().Count +
                "\nLight Queries: " + LightQueries +
                "\nTile draw time: " + TileDrawTime.Elapsed.ToString("0.00 ns");
        }

        static public bool HideWalls;
        static public bool HideTerrain;
        static public bool HideOccludedBlocks;
        static public bool CullDarkFaces;
        static public bool BlockOutlines;
        static public bool AsyncLoading;
        static public bool MouseTooltip;
        public static bool DrawRegions;

        static XElement _DrawRooms;
        public static bool DrawRooms
        {
            get => bool.Parse(_DrawRooms.Value);
            set
            {
                _DrawRooms.SetValue(value);
                SaveConfig();
            }
        }

        static XmlDocument InitSettings()
        {
            XmlDocument settings = new XmlDocument();
            try { settings.Load("config.xml"); }
            catch (System.IO.FileNotFoundException e)
            {
                GenerateDefaultConfigFile(Game1.Instance.graphics.GraphicsDevice);
                settings.Load("config.xml");
            }

            AsyncLoading = bool.Parse(settings["Settings"]["Engine"]["AsyncLoading"].InnerText);
            MouseTooltip = bool.Parse(settings["Settings"]["Interface"]["MouseTooltip"].InnerText);
            HideWalls = bool.Parse(settings["Settings"]["Engine"]["HideWalls"].InnerText);
            HideTerrain = bool.Parse(settings["Settings"]["Engine"]["HideTerrain"].InnerText);
            BlockOutlines = true;
            CullDarkFaces = false;
            HideOccludedBlocks = false;
            DrawRooms = bool.Parse(settings["Settings"]["Engine"]["DrawRooms"]?.InnerText ?? bool.TrueString);
            return settings;
        }

        static public void Update(GameTime gt)
        {
            LightQueries = 0;
        }

        static void GenerateDefaultConfigFile(GraphicsDevice gfx)
        {
            var displayMode = gfx.Adapter.SupportedDisplayModes[SurfaceFormat.Color].First();

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
                        new XElement("HideTerrain", true),
                        new XElement("DrawRooms", false)
                    ),
                    new XElement("Interface",
                        new XElement("MouseTooltip", true),
                        new XElement("UIScale", 1)
                    ),
                    new XElement("Graphics",
                        new XElement("Particles", Particles.ParticleDensityLevel.Current.Name)
                    )
                )
            ).Save("config.xml");
        }

        static public void Init(Game1 game)
        {
            var resolution = Engine.Config.Descendants("Graphics").FirstOrDefault();
            game.graphics.PreferredBackBufferWidth = (int?)resolution.Element("Resolution").Element("Width") ?? game.graphics.PreferredBackBufferWidth;
            game.graphics.PreferredBackBufferHeight = (int?)resolution.Element("Resolution").Element("Height") ?? game.graphics.PreferredBackBufferHeight;
            game.graphics.IsFullScreen = (bool?)resolution.Element("Fullscreen") ?? true;
            game.graphics.ApplyChanges();

        }


        internal static void InitializeComponents()
        {
            
        }

        static public void PlayGame()
        {
            Nameplate.Reset();
            GC.Collect();
        }
    }
}
