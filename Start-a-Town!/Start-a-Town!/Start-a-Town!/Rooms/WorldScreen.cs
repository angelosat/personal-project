using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Windows.Forms;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Rooms
{
    class WorldScreen : GameScreen
    {
        #region Singleton
        static WorldScreen _Instance;
        public static WorldScreen Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new WorldScreen();
                return _Instance;
            }
        }
        #endregion
        SceneState Scene = new SceneState();
        GameObject _Player;
        public GameObject Player { get { return _Player; } set { _Player = value; } }
        IWorld _World;
        public IWorld World
        {
            get { return _World; }
            set
            {
                //if (!_World.IsNull())
                //    _World.Dispose();
                _World = value;
            }
        }
        public IMap Map;
       // Camera Camera;
        WorldScreenUI UI;

        WorldScreen()
            : base()
        {
            this.UI = WorldScreenUI.Instance;
            this.WindowManager = new UIManager();
            this.UI.Show(this.WindowManager);
            this.Camera = new Camera();
            KeyHandlers.Push(Camera);
            KeyHandlers.Push(WindowManager);
            KeyHandlers.Push(ContextMenuManager.Instance);

            //Server.Start();

            // CONNECT WHEN ENTERING WORLD INSTEAD OF HERE
            //string localHost = "127.0.0.1";
            //Net.Client.Connect(localHost, new PlayerData("host"), a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
        }

        WorldScreen(World world)
            : this()
        {
            this.Initialize(world);
        }

        public override GameScreen Initialize()
        {
            Nameplate.Reset();
            Initialize(Start_a_Town_.World.LoadLastWorld());
            return base.Initialize();
        }
        public WorldScreen Initialize(World world)
        {
            this.World = world;
            this.UI.Initialize(world);
            Net.Server.SetWorld(world);
            Camera.Coordinates = Vector2.Zero;
            return this;
        }
        
        public override void Update(GameTime gameTime)
        {
            if (!World.IsNull())
                foreach (var map in World.GetMaps())
                    map.Value.GetThumb().Update();
            Camera.Update(gameTime);
            ContextMenuManager.Update();
            WindowManager.Update(gameTime);
            Nameplate.UpdatePlates(this.Camera, Scene);
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch sb)
        {
            Game1.Instance.GraphicsDevice.Clear(Color.DarkSlateBlue);
            if (World != null)
            {
                sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                //this.Camera.Draw(sb, World);
                this.World.Draw(sb, this.Camera);
                sb.End();
            }
            WindowManager.Draw(sb);
        }

        public override void HandleLButtonDown(HandledMouseEventArgs e)
        {
            base.HandleLButtonDown(e);
            if (e.Handled)
                return;
            //Map map = Controller.Instance.Mouseover.Object as Map;
            //this.UI.Initialize(map);
            MapThumb mapThumb = Controller.Instance.Mouseover.Object as MapThumb;
            if (mapThumb.IsNull())
                return;
            this.Map = mapThumb.IsNull() ? null : mapThumb.Map;
            //this.UI.Initialize(this.Map);
            this.UI.Initialize(mapThumb);
        }

        public override void HandleRButtonDown(HandledMouseEventArgs e)
        {
            this.Map = null;
            this.UI.Initialize(default(MapThumb));
        }

        public override void HandleMouseWheel(HandledMouseEventArgs e)
        {
            int last = MapThumb.CurrentZoom;
            MapThumb.CurrentZoom = (int)MathHelper.Clamp(MapThumb.CurrentZoom - e.Delta, 0, 2);
            if(last == MapThumb.CurrentZoom)
                e.Handled = true;
            base.HandleMouseWheel(e);
        }
    }
}
