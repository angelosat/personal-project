using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Start_a_Town_.UI;
using Start_a_Town_.Rooms;
using Start_a_Town_.Net;
using Start_a_Town_.Animations;

namespace Start_a_Town_
{
    
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Game
    {
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public SpriteBatch UIspriteBatch;
        public Effect Effect;
        public GameScreen CurrentRoom;
        public Random Random;
        public static Game1 Instance;
        public static TextInputHandler TextInput;
        double FpsTimer;
        int Fps;

        bool DeviceReset = false;

        static public Vector2 ScreenSize{get;set;}
       
        public List<GameComponent> GameComponents = new List<GameComponent>();
        public Network Network;

        protected override void OnExiting(object sender, EventArgs args)
        {
            Server.Stop();
            base.OnExiting(sender, args);
        }

        public Game1()
        {
            Instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsFixedTimeStep = false;
           
            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            graphics.DeviceReset += new EventHandler<EventArgs>(graphics_DeviceReset);
            graphics.DeviceResetting += new EventHandler<EventArgs>(graphics_DeviceResetting);
            graphics.DeviceCreated += new EventHandler<EventArgs>(graphics_DeviceCreated);
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
            
            this.IsMouseVisible = true;

            // put these here to prevent thousands of framerate in the menus resulting in gpu whine noise
            this.IsFixedTimeStep = true;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 120d); 
        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            var displayMode = Instance.graphics.GraphicsDevice.Adapter.SupportedDisplayModes[SurfaceFormat.Color].First();
            graphics.PreferredBackBufferWidth = Math.Max(displayMode.Width, Instance.Window.ClientBounds.Width);
            graphics.PreferredBackBufferHeight = Math.Max(displayMode.Height, Instance.Window.ClientBounds.Height);
            graphics.ApplyChanges();

            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Resolution").GetOrCreateElement("Width").Value = graphics.PreferredBackBufferWidth.ToString();
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Video").GetOrCreateElement("Resolution").GetOrCreateElement("Height").Value = graphics.PreferredBackBufferHeight.ToString();
            Engine.Config.Save("config.xml");
        }

        void graphics_DeviceCreated(object sender, EventArgs e)
        {
            this.ApplyVideoSettings();
        }

        internal void ApplyVideoSettings()
        {
            var resolution = Engine.Config.Descendants("Video").FirstOrDefault();
            graphics.PreferredBackBufferWidth = (int?)resolution.Element("Resolution").Element("Width") ?? graphics.PreferredBackBufferWidth;
            graphics.PreferredBackBufferHeight = (int?)resolution.Element("Resolution").Element("Height") ?? graphics.PreferredBackBufferHeight;
            graphics.IsFullScreen = (bool?)resolution.Element("Fullscreen") ?? true;
        }

        void graphics_DeviceResetting(object sender, EventArgs e)
        {
            "resetting".ToConsole();
        }

        void graphics_DeviceReset(object sender, EventArgs e)
        {
            this.DeviceReset = true;
            "reset".ToConsole();
        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
            "preparing".ToConsole();
        }
        
        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            Engine.Init(this);
            TextInput = new TextInputHandler(Window.Handle);
            ScreenManager.Initialize();
            Random = new Random();
            base.Initialize();
            Animation.Export();
            this.Network = new Network();
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            ScreenSize = new Vector2(
                      Instance.Window.ClientBounds.Width,
                      Instance.Window.ClientBounds.Height);
            spriteBatch = new SpriteBatch(Game1.Instance.GraphicsDevice);
            UIManager.LoadContent();
            ScreenManager.LoadContent();
            Effect = Game1.Instance.Content.Load<Effect>("Effect3");
            MapBase.Initialize();
            // TODO: use this.Content to load your game content here
            Material.Initialize();
            MaterialType.Initialize();
            RawMaterialDef.Initialize();

            /// def init
            MoodletDef.Init();
            NeedLetDef.Init();
            ToolDefs.Init();

            GameObject.LoadObjects();

            Sprite.Initialize(); // why did i put it before the precious call? i have to bake the sprite atlas after initializing item templates

            Block.Initialize();
            Start_a_Town_.Components.Crafting.Reaction.Initialize();
          
            Cell.Initialize();

            this.GameComponents.Add(new Modules.Base.GameManager());
            this.GameComponents.Add(new Towns.TownsManager());
            this.GameComponents.Add(new AI.AIManager());

            foreach (var item in this.GameComponents)
                item.Initialize();

            Interaction.Initialize();
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        double UPDATE_INTERVAL = 1f / Engine.TicksPerSecond;
        double UPDATE_INTERVAL_UI = 1f / Engine.TicksPerSecond;
        double lastFrameTime = 0.0;
        double cyclesLeftOver = 0.0;
        double lastFrameTimeUI = 0.0;
        double cyclesLeftOverUI = 0.0;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (DeviceReset)
            {
                Sprite.Atlas.OnDeviceLost();
                Block.Atlas.OnDeviceLost();
                UIManager.Atlas.OnDeviceLost();
                DeviceReset = false;
            }
            ScreenSize = new Vector2(
                        Instance.Window.ClientBounds.Width,
                        Instance.Window.ClientBounds.Height);
            // fixed time step
            double currentTime, updateIterations, updateIterationsUI;
            currentTime = gameTime.TotalGameTime.TotalSeconds;
            updateIterations = ((currentTime - lastFrameTime) + cyclesLeftOver);
            updateIterationsUI = ((currentTime - lastFrameTimeUI) + cyclesLeftOverUI);
            int updatesOccured = 0;
            
            while (updateIterations > UPDATE_INTERVAL)
            {
                updateIterations -= UPDATE_INTERVAL;
                // Allows the game to exit
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                    this.Exit();

                // i put this inside the loop again because if it's outside it resets the mouseover faster than the camera has the chance to process it for mouseover in its update
                // if it's outside the loop, i had to do mousepicking in the camera draw method in order for it to work
                Controller.Instance.Update(); 

                Block.UpdateBlocks();
                this.Network.Update(gameTime);
                ScreenManager.Instance.Update(this, gameTime);

                Log.Update();
                base.Update(gameTime);
                updatesOccured++;
            }
           
            cyclesLeftOver = updateIterations;
            cyclesLeftOverUI = updateIterationsUI;
            lastFrameTime = currentTime;
            lastFrameTimeUI = lastFrameTime;
        }
        
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            ScreenManager.Instance.Draw(spriteBatch);
            Controller.Instance.MouseoverBlockNext.Valid = true; // i added this here to fix mouseover flickering but it caused other problems
            base.Draw(gameTime);
            var elapsed = gameTime.ElapsedGameTime.TotalSeconds;
            this.FpsTimer += elapsed;
            if (this.FpsTimer > 1)
            {
                this.FpsTimer -= 1;
                this.Window.Title = "FPS: " + this.Fps.ToString();
                this.Fps = 0;
            }
            Fps++;
        }
    }
}
