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

namespace Start_a_Town_
{
    //public class TaskException : Exception
    //{
    //    public TaskException(string message) : base(message) { }
    

    public delegate void UIEvent(Object sender, EventArgs e);
    public delegate void MouseEventHandler(object sender, MouseEventArgs e);
    public delegate void ScrollEventHandler(Object sender, ScrollEventArgs e);
    public delegate void InputEvent();

    public delegate void CellEvent(Chunk cell);

    public delegate void MessageEvent(int message);
    /// <summary>
    /// This is the main type for your game
    /// </summary>
    public class Game1 : Microsoft.Xna.Framework.Game
    {
        public GraphicsDeviceManager graphics;
        public SpriteBatch spriteBatch;
        public SpriteBatch UIspriteBatch;
        public Effect Effect;
        public GameScreen CurrentRoom;
        public Random Random;
        public static Game1 Instance;
        public static TextInputHandler TextInput;

        bool DeviceReset = false;
       // public static XmlDocument Settings;
        //public Stack<GameScreen> GameScreens;

        static public Vector2 ScreenSize{get;set;}
        //{
        //    get
        //    {
        //        return new Vector2(
        //             Instance.Window.ClientBounds.Width,
        //             Instance.Window.ClientBounds.Height);

        //    }
        //}
        public List<GameComponent> GameComponents = new List<GameComponent>();
        //public List<GameComponent> GetGameComponents() { return this.GameComponents; }

        protected override void OnExiting(object sender, EventArgs args)
        {
            ChunkLoader.End();

            Server.Stop();
          //  ChunkLighter.End();
           // Pathfinding.End();
            base.OnExiting(sender, args);
        }

        public Game1()
        {
            Instance = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            //ScreenSize = new Vector2(
            //         Instance.Window.ClientBounds.Width,
            //         Instance.Window.ClientBounds.Height);
            IsFixedTimeStep = false;
            
            //IsMouseVisible = true;

            //graphics.PreferredBackBufferWidth = 1600;// 1280;// 800;
            //graphics.PreferredBackBufferHeight = 900;// 720;// 600;

            //Settings = new XmlDocument();
            //Settings.Load("config.xml");
            //Engine = new Engine(Settings);
           // Engine = new Engine();

            //graphics.PreferredBackBufferWidth = Int16.Parse(Engine.Settings["Settings"]["Graphics"]["Resolution"]["Width"].InnerText);
            //graphics.PreferredBackBufferHeight = Int16.Parse(Engine.Settings["Settings"]["Graphics"]["Resolution"]["Height"].InnerText);
            //graphics.IsFullScreen = bool.Parse(Engine.Settings["Settings"]["Graphics"]["Fullscreen"].InnerText);

            graphics.SynchronizeWithVerticalRetrace = false;
            graphics.PreparingDeviceSettings += new EventHandler<PreparingDeviceSettingsEventArgs>(graphics_PreparingDeviceSettings);
            graphics.DeviceReset += new EventHandler<EventArgs>(graphics_DeviceReset);
            graphics.DeviceResetting += new EventHandler<EventArgs>(graphics_DeviceResetting);
            graphics.DeviceCreated += new EventHandler<EventArgs>(graphics_DeviceCreated);
            Window.AllowUserResizing = true;
            Window.ClientSizeChanged += new EventHandler<EventArgs>(Window_ClientSizeChanged);
            //graphics.ApplyChanges();
            
            this.IsMouseVisible = true;
            //GameScreens = new Stack<GameScreen>();
        }

        void Window_ClientSizeChanged(object sender, EventArgs e)
        {
            var displayMode = Instance.graphics.GraphicsDevice.Adapter.SupportedDisplayModes[SurfaceFormat.Color].First();
            graphics.PreferredBackBufferWidth = Math.Max(displayMode.Width, Instance.Window.ClientBounds.Width);// Instance.Window.ClientBounds.Width;
            graphics.PreferredBackBufferHeight = Math.Max(displayMode.Height, Instance.Window.ClientBounds.Height); //Instance.Window.ClientBounds.Height;
            graphics.ApplyChanges();
            //Engine.Settings["Settings"]["Graphics"]["Resolution"]["Width"].InnerText = graphics.PreferredBackBufferWidth.ToString();
            //Engine.Settings["Settings"]["Graphics"]["Resolution"]["Height"].InnerText = graphics.PreferredBackBufferHeight.ToString();
            //Engine.Settings.Save("config.xml");

            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Graphics").GetOrCreateElement("Resolution").GetOrCreateElement("Width").Value = graphics.PreferredBackBufferWidth.ToString();
            Engine.Config.GetOrCreateElement("Settings").GetOrCreateElement("Graphics").GetOrCreateElement("Resolution").GetOrCreateElement("Height").Value = graphics.PreferredBackBufferHeight.ToString();
            Engine.Config.Save("config.xml");
        }

        void graphics_DeviceCreated(object sender, EventArgs e)
        {
            //graphics.PreferredBackBufferWidth = Int16.Parse(Engine.Settings["Settings"]["Graphics"]["Resolution"]["Width"].InnerText);
            //graphics.PreferredBackBufferHeight = Int16.Parse(Engine.Settings["Settings"]["Graphics"]["Resolution"]["Height"].InnerText);
            var resolution = Engine.Config.Descendants("Graphics").FirstOrDefault();
            graphics.PreferredBackBufferWidth = (int?)resolution.Element("Resolution").Element("Width") ?? graphics.PreferredBackBufferWidth;
            graphics.PreferredBackBufferHeight = (int?)resolution.Element("Resolution").Element("Height") ?? graphics.PreferredBackBufferHeight;
            graphics.IsFullScreen = (bool?)resolution.Element("Fullscreen") ?? true;
            //graphics.ApplyChanges();
        }

        void graphics_DeviceResetting(object sender, EventArgs e)
        {
            //var displayMode = Instance.graphics.GraphicsDevice.Adapter.SupportedDisplayModes[SurfaceFormat.Color].First();
            //graphics.PreferredBackBufferWidth = Math.Max(displayMode.Width, Instance.Window.ClientBounds.Width);
            //graphics.PreferredBackBufferHeight = Math.Max(displayMode.Height, Instance.Window.ClientBounds.Height);
            //Sprite.Atlas.Initialize();
            //Block.Atlas.Initialize();
            "resetting".ToConsole();

        }

        void graphics_DeviceReset(object sender, EventArgs e)
        {
            this.DeviceReset = true;
            //Sprite.Atlas.Initialize();
            //Block.Atlas.Initialize();
            "reset".ToConsole();

        }

        void graphics_PreparingDeviceSettings(object sender, PreparingDeviceSettingsEventArgs e)
        {
            e.GraphicsDeviceInformation.PresentationParameters.RenderTargetUsage = RenderTargetUsage.PreserveContents;
         //   e.GraphicsDeviceInformation.PresentationParameters.MultiSampleCount = 4;
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
            // TODO: Add your initialization logic here    


            Engine.Init(this);
            

            TextInput = new TextInputHandler(Window.Handle);
            
            ScreenManager.Initialize();
            //ScreenManager.Add(new MainScreen().Initialize());

           // ScreenManager.Add(new MainScreen());
           // TextInput.KeyPress += new System.Windows.Forms.KeyPressEventHandler(TextInput_KeyPress);

            
         //   UIspriteBatch = new SpriteBatch(Game1.Instance.GraphicsDevice);
            
            //interactionManager = new InteractionManager();
            //itemManager = new ItemManager();
            //SkillManager = new SkillManager();
            //constructionManager = new ConstructionManager();

            Random = new Random();
            //UIManager.Instance.Initialize();

           // Camera.Instance.Initialize();
            TooltipManager.Instance.Initialize();

            //CurrentRoom = new MainScreen();
            ////UIManager.LoadContent();
            ////GameScreens.Push(CurrentRoom);
            //ScreenManager.Add(CurrentRoom);
            //CurrentRoom.Initialize();
            ////CurrentRoom.Initialize();

            this.GameComponents.Add(new Modules.Base.GameManager());
            this.GameComponents.Add(new Towns.TownsManager());
            this.GameComponents.Add(new AI.AIManager());
            //this.GameComponents.Add(new Systems.Construction.ConstructionManager());

            foreach (var item in this.GameComponents)
                item.Initialize();

            base.Initialize();

            Graphics.AnimationCollection.Export();
            //Console.WriteLine(graphics.GraphicsDevice.Adapter.SupportedDisplayModes.ToString());
        }

        //void TextInput_KeyPress(object sender, System.Windows.Forms.KeyPressEventArgs e)
        //{
        //    Console.WriteLine(e.KeyChar);
        //}
        

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
            ScreenSize = new Vector2(
                      Instance.Window.ClientBounds.Width,
                      Instance.Window.ClientBounds.Height);
            // Create a new SpriteBatch, which can be used to draw textures.
            spriteBatch = new SpriteBatch(Game1.Instance.GraphicsDevice);
            UIManager.LoadContent();
            ScreenManager.LoadContent();
            //CurrentRoom.LoadContent();
        //    ScreenManager.Current.LoadContent();
            Effect = Game1.Instance.Content.Load<Effect>("Effect3");
            Map.Initialize();
            // TODO: use this.Content to load your game content here
            //Start_a_Town_.Components.Materials.Material.Initialize();

            //PlayerEntity.Initialize();

            Start_a_Town_.Components.Materials.Material.Initialize();
            Start_a_Town_.Components.Materials.MaterialType.Initialize();
 
            GameObject.LoadObjects();
            //Sprite.Initialize();

            Start_a_Town_.Components.Items.ItemTemplate.Initialize();
            Sprite.Initialize(); // why did i put it before the precious call? i have to bake the sprite atlas after initializing item templates

            Block.Initialize();
            PlayerEntity.Initialize();
            //Start_a_Town_.Components.Materials.MaterialType.Initialize();
            Start_a_Town_.Components.Crafting.Reaction.Initialize();
            //GameObject.LoadObjects();
          
            Cell.Initialize();

            //var towns = new Towns.TownsManager();
            //towns.Initialize();
            //this.GameComponents.Add(towns);

        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// all content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        double UPDATE_INTERVAL = 1.0 / Engine.TargetFps;
        double lastFrameTime = 0.0;
        double cyclesLeftOver = 0.0;

        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {
            if (DeviceReset)
            {
                //Block.LoadContent();
                //Sprite.LoadContent();

                //Sprite.Atlas.Initialize();
                //Block.Atlas.Initialize();

                Sprite.Atlas.OnDeviceLost();
                Block.Atlas.OnDeviceLost();
                DeviceReset = false;
            }
            ScreenSize = new Vector2(
                        Instance.Window.ClientBounds.Width,
                        Instance.Window.ClientBounds.Height);
            // fixed time step
            double currentTime, updateIterations;
            currentTime = gameTime.TotalGameTime.TotalSeconds;
            updateIterations = ((currentTime - lastFrameTime) + cyclesLeftOver);

            int updatesOccured = 0;

            Controller.Instance.Update(); // this has to be out of the loop to prevent weird mouseover behavior
            while (updateIterations > UPDATE_INTERVAL)
            {
                updateIterations -= UPDATE_INTERVAL;
                // Allows the game to exit
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed)
                    this.Exit();

                Block.UpdateBlocks();
                Server.Update(gameTime); 
                Client.Update();
                //Console.WriteLine("server:  " + Server.Instance.Clock.TotalMilliseconds.ToString() + " client: " + Client.Instance.Clock.TotalMilliseconds.ToString());
                Engine.Update(gameTime);

                ScreenManager.Instance.Update(gameTime); // TODO: move client world updating to client update method instead of screen update method

                // TODO: Add your update logic here
                TooltipManager.Instance.Update();

                Log.Update();
                base.Update(gameTime);
                updatesOccured++;
            }
            cyclesLeftOver = updateIterations; 
            lastFrameTime = currentTime;
        }

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            //if (DeviceReset)
            //{
            //    //Block.LoadContent();
            //    //Sprite.LoadContent();

            //    //Sprite.Atlas.Initialize();
            //    //Block.Atlas.Initialize();

            //    Sprite.Atlas.OnDeviceLost();
            //    Block.Atlas.OnDeviceLost();
            //    //"reloaded".ToConsole();
            //    DeviceReset = false;
            //}
            //if (gameTime.ElapsedGameTime == TimeSpan.Zero)
            //{
            //    graphics.PreferredBackBufferWidth = Window.ClientBounds.Width;
            //    graphics.PreferredBackBufferHeight = Window.ClientBounds.Height;
            //    graphics.ApplyChanges();
            //}
            //GraphicsDevice.Clear(Color.CornflowerBlue);
            //GraphicsDevice.Clear(Color.SteelBlue);
            GraphicsDevice.SetRenderTarget(null);
            GraphicsDevice.Clear(Color.Black);
            //GraphicsDevice.Clear(Color.SkyBlue);
            //GraphicsDevice.Clear(new Color(new Vector3(135, 206, 235)));
            // TODO: Add your drawing code here
            
            //CurrentRoom.Draw(spriteBatch);
            ScreenManager.Instance.Draw(spriteBatch);
            //spriteBatch.Begin();
            //spriteBatch.DrawString(UIManager.Font, ChunkLoader.ToString(), Vector2.Zero, Color.White);
            //spriteBatch.End();
            base.Draw(gameTime);
        }
    }
}
