using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Rooms
{
    class MainScreen : GameScreen
    {
        static MainScreen _Instance;
        public static MainScreen Instance
        {
            get
            {
                return _Instance ??= new MainScreen();
            }
        }

        static Texture2D Background;

        public static void LoadContent()
        {
            Background = Game1.Instance.Content.Load<Texture2D>("Graphics/bg");
        }

        public override GameScreen Initialize(IObjectProvider net)
        {
            base.Initialize(net);
            WindowManager.Initialize();
            //MainMenuWindow.Instance.Show(WindowManager);
            var mainmenu = new MainMenuWindow(null);
            //GameModes.GameMode.Current.OnMainMenuCreated(mainmenu);
            mainmenu.Show();
            return this;
        }
        MainScreen()
        {
            WindowManager = new UIManager();
         //   InputHandlers.Push(WindowManager);

          //  MainMenuWindow.Instance.Show(WindowManager);

            KeyHandlers.Push(WindowManager);
       //     WindowManager.Parent = this;


            //WindowManager.ToggleSingletonWindow<MainMenuWindow>();

            
        }

        public override void Update(Game1 game, GameTime gt)
        {
            //UIManager.Instance.Update(gameTime);
            WindowManager.Update(game, gt);
            base.Update(game, gt);
        }

        public override void Draw(SpriteBatch sb)
        {
            
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
           // sb.Draw(Background, new Rectangle(0, 0, Game1.Instance.Window.ClientBounds.Width, Game1.Instance.Window.ClientBounds.Height), Color.White);
            sb.Draw(Background, new Rectangle(0, 0, Game1.Instance.graphics.GraphicsDevice.Viewport.Width, Game1.Instance.graphics.GraphicsDevice.Viewport.Height), Color.White);
            sb.End();
            WindowManager.Draw(sb, null);
        }
    }
}
