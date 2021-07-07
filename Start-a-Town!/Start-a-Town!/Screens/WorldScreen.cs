using System.Windows.Forms;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;
using System;

namespace Start_a_Town_.Rooms
{
    [Obsolete]
    class WorldScreen : GameScreen
    {
        static WorldScreen _Instance;
        public static WorldScreen Instance => _Instance ??= new WorldScreen();
        
        SceneState Scene = new SceneState();
        GameObject _Player;
        public GameObject Player { get { return _Player; } set { _Player = value; } }
        IWorld _World;
        public IWorld World
        {
            get => _World; 
            set =>_World = value;
        }
        public IMap Map;
        WorldScreenUI UI;

        WorldScreen()
            : base()
        {
            this.UI = WorldScreenUI.Instance;
            this.WindowManager = new UIManager();
            this.UI.Show(this.WindowManager);
            this.Camera = new Camera();
            KeyHandlers.Push(WindowManager);
            KeyHandlers.Push(ContextMenuManager.Instance);
        }

        public override GameScreen Initialize(IObjectProvider net)
        {
            Nameplate.Reset();
            Initialize(Start_a_Town_.World.LoadLastWorld());
            return base.Initialize(net);
        }
        public WorldScreen Initialize(World world)
        {
            this.World = world;
            this.UI.Initialize(world);
            Server.SetWorld(world);
            Camera.Coordinates = Vector2.Zero;
            return this;
        }

        public override void Update(Game1 game, GameTime gt)
        {
            if (this.World is not null)
                foreach (var map in this.World.GetMaps())
                    map.Value.GetThumb().Update();
            Camera.Update(gt);
            ContextMenuManager.Update();
            WindowManager.Update(game, gt);
            Nameplate.UpdatePlates(this.Camera, Scene);
            base.Update(game, gt);
        }

        public override void Draw(SpriteBatch sb)
        {
            Game1.Instance.GraphicsDevice.Clear(Color.DarkSlateBlue);
            if (World != null)
            {
                sb.Begin(SpriteSortMode.BackToFront, BlendState.AlphaBlend);
                this.World.Draw(sb, this.Camera);
                sb.End();
            }
            WindowManager.Draw(sb, Net.Client.Instance.Map.Camera);
        }

        public override void HandleLButtonDown(HandledMouseEventArgs e)
        {
            base.HandleLButtonDown(e);
            if (e.Handled)
                return;
            MapThumb mapThumb = Controller.Instance.MouseoverBlock.Object as MapThumb;
            if (mapThumb is null)
                return;
            this.Map = mapThumb?.Map;
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
