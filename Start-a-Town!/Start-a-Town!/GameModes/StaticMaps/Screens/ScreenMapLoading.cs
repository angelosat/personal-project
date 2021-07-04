using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;
using Start_a_Town_.Rooms;
using Start_a_Town_.GameModes.StaticMaps.UI;

namespace Start_a_Town_.GameModes.StaticMaps.Screens
{
    class ScreenMapLoading : GameScreen
    {
        readonly StaticMapLoadingWidget Widget;
        private readonly StaticMap Map;
        readonly StaticMapLoadingProgressToken ProgressToken;
      
        public ScreenMapLoading(StaticMap map)
        {
            this.Widget = new StaticMapLoadingWidget();
            this.Widget.SnapToScreenCenter();
            this.Widget.Show(this.WindowManager);
            this.Map = map;
            this.ProgressToken = new StaticMapLoadingProgressToken(this.Widget.Refresh);
            Task.Factory.StartNew(() => map.Load(this.ProgressToken));
        }
        public override void Update(Game1 game, GameTime gt)
        {
            base.Update(game, gt);
            this.WindowManager.Update(game, gt);
        }
        public override void HandleKeyUp(System.Windows.Forms.KeyEventArgs e)
        {
            if (Game1.Instance.IsActive)
                this.EnterMap(this.Map);
        }
        void EnterMap(IMap map)
        {
            if (map == null)
                return;
            if (this.ProgressToken.Percentage < 1)
                return;
            
            Engine.Config.GetOrCreateElement("Profile").GetOrCreateElement("LastWorld").Value = map.World.GetName();

            Rooms.WorldScreen.Instance.Map = null;
            
            Net.Server.InstantiateMap(map); // is this needed???
            
            string localHost = "127.0.0.1";
            Net.Client.Instance.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
        }

        public override void Draw(SpriteBatch sb)
        {
            Game1.Instance.GraphicsDevice.Clear(Color.DarkSlateBlue);
            this.WindowManager.Draw(sb, Net.Client.Instance.Map.Camera);
        }
    }
}
