using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using Start_a_Town_.Rooms;
using Start_a_Town_.UI;

namespace Start_a_Town_.GameModes.StaticMaps
{
    public class ScreenNewGame : GameScreen
    {
        #region Singleton
        static ScreenNewGame _Instance;
        public static ScreenNewGame Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ScreenNewGame();
                return _Instance;
            }
        }
        #endregion

        //StaticWorldScreenUINew Interface;

        ScreenNewGame()
            : base()
        {
            this.WindowManager = new UIManager();
            //UINewGameWindow.Instance.Show(this.WindowManager);
            //UINewGameWindow.Instance.SnapToScreenCenter();

            KeyHandlers.Push(WindowManager);
            KeyHandlers.Push(ContextMenuManager.Instance);

        }
        public override void Update(Game1 game, GameTime gt)
        {
            ContextMenuManager.Update();
            WindowManager.Update(game, gt);
            base.Update(game, gt);
        }
        public override void Draw(SpriteBatch sb)
        {
            Game1.Instance.GraphicsDevice.Clear(Color.DarkSlateBlue);
            WindowManager.Draw(sb, null);
        }
    }
}
