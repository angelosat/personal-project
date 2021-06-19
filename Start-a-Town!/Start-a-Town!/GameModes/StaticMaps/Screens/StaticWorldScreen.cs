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
using Start_a_Town_.Rooms;
using Start_a_Town_.GameModes.StaticMaps.UI;

namespace Start_a_Town_.GameModes.StaticMaps.Screens
{
    class StaticWorldScreen : GameScreen
    {
        #region Singleton
        static StaticWorldScreen _Instance;
        public static StaticWorldScreen Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new StaticWorldScreen();
                return _Instance;
            }
        }
        #endregion

        SceneState Scene = new SceneState();
        GameObject _Player;
        public GameObject Player { get { return _Player; } set { _Player = value; } }
        public StaticWorld World;
        
        public IMap Map;

        StaticWorldScreenUINew Interface;

        StaticWorldScreen()
            : base()
        {
            this.Interface = StaticWorldScreenUINew.Instance;
            this.WindowManager = new UIManager();
            this.Interface.Show(this.WindowManager);
            //this.Camera = new Camera();
            //KeyHandlers.Push(Camera);
            KeyHandlers.Push(WindowManager);
            KeyHandlers.Push(ContextMenuManager.Instance);

            //Server.Start();

            // CONNECT WHEN ENTERING WORLD INSTEAD OF HERE
            //string localHost = "127.0.0.1";
            //Client.Connect(localHost, new PlayerData("host"), a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
        }

        StaticWorldScreen(StaticWorld world)
            : this()
        {
            this.Initialize(world);
        }

        public override GameScreen Initialize(IObjectProvider net)
        {

            Nameplate.Reset();
            //Initialize(StaticWorld.LoadLastWorld());
            return base.Initialize(net);
            //Server.Start();

        }
        public StaticWorldScreen Initialize(StaticWorld world)
        {
            this.World = world;
            this.Interface.Initialize(world);
            Server.SetWorld(world);
            Camera.Coordinates = Vector2.Zero;
            return this;
        }
        
        public override void Update(Game1 game, GameTime gt)
        {
            if (World != null)
                foreach (var map in World.Maps)
                    map.Value.GetThumb().Update();
            //Camera.Update(gt);
            ContextMenuManager.Update();
            WindowManager.Update(game, gt);
            //Nameplate.UpdatePlates(this.Camera, Scene);
            base.Update(game, gt);
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
            WindowManager.Draw(sb, null);
        }

        public override void HandleLButtonDown(HandledMouseEventArgs e)
        {
            base.HandleLButtonDown(e);
            if (e.Handled)
                return;
            //Map map = Controller.Instance.Mouseover.Object as Map;
            //this.UI.Initialize(map);
            MapThumb mapThumb = Controller.Instance.MouseoverBlock.Object as MapThumb;
            if (mapThumb == null)
                return;
            this.Map = mapThumb == null ? null : mapThumb.Map;
            //this.UI.Initialize(this.Map);
            this.Interface.Initialize(mapThumb);
        }

        public override void HandleRButtonDown(HandledMouseEventArgs e)
        {
            this.Map = null;
            this.Interface.Initialize(default(MapThumb));
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
