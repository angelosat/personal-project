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
                if (_Instance.IsNull())
                    _Instance = new MainScreen();
                return _Instance;
            }
        }

        static Texture2D Background;

        public static void LoadContent()
        {
            Background = Game1.Instance.Content.Load<Texture2D>("Graphics/bg");
        }

        public override GameScreen Initialize()
        {
            base.Initialize();
            WindowManager.Initialize();
            MainMenuWindow.Instance.Show(WindowManager);
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

        public override void Update(GameTime gameTime)
        {
            //UIManager.Instance.Update(gameTime);
            WindowManager.Update(gameTime);
            base.Update(gameTime);
        }

        public override void Draw(SpriteBatch sb)
        {
            
            sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
           // sb.Draw(Background, new Rectangle(0, 0, Game1.Instance.Window.ClientBounds.Width, Game1.Instance.Window.ClientBounds.Height), Color.White);
            sb.Draw(Background, new Rectangle(0, 0, Game1.Instance.graphics.GraphicsDevice.Viewport.Width, Game1.Instance.graphics.GraphicsDevice.Viewport.Height), Color.White);
            sb.End();
            WindowManager.Draw(sb);
        }
    }
}
