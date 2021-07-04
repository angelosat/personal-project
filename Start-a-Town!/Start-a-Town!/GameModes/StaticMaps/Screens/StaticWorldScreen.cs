using System.Windows.Forms;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Net;
using Start_a_Town_.Rooms;

namespace Start_a_Town_.GameModes.StaticMaps.Screens
{
    class StaticWorldScreen : GameScreen
    {
        static StaticWorldScreen _Instance;
        public static StaticWorldScreen Instance => _Instance ??= new StaticWorldScreen();

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
            KeyHandlers.Push(WindowManager);
            KeyHandlers.Push(ContextMenuManager.Instance);
        }

        public override GameScreen Initialize(IObjectProvider net)
        {
            Nameplate.Reset();
            return base.Initialize(net);
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
            ContextMenuManager.Update();
            WindowManager.Update(game, gt);
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
            WindowManager.Draw(sb, null);
        }

        public override void HandleLButtonDown(HandledMouseEventArgs e)
        {
            base.HandleLButtonDown(e);
            if (e.Handled)
                return;
            MapThumb mapThumb = Controller.Instance.MouseoverBlock.Object as MapThumb;
            if (mapThumb == null)
                return;
            this.Map = mapThumb == null ? null : mapThumb.Map;
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
