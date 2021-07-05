using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.UI;

namespace Start_a_Town_.Rooms
{
    class LoadingScreen
    {
        static public void Draw(SpriteBatch sb, string message)
        {
            sb.Begin();
            UIManager.DrawStringOutlined(sb, message, new Vector2(Game1.Instance.GraphicsDevice.Viewport.Width / 2, 3 * Game1.Instance.GraphicsDevice.Viewport.Height / 4), HorizontalAlignment.Center);
            sb.End();
        }
    }

    //class LoadingScreen : GameScreen
    //{
    //    static public Bar Bar;

    //    float _Progress;
    //    public float Progress
    //    {
    //        get { return _Progress; }
    //        set
    //        {
    //            _Progress = value;
    //            Bar.Percentage = value;
    //        }
    //    }
    //    string _Message;
    //    public string Message
    //    {
    //        get { return _Message; }
    //        set
    //        {
    //            _Message = value;
    //            Bar.Text = value;
    //        }
    //    }

    //    GameScreen LoadedScreen;

    //    //public override void Initialize()
    //    public LoadingScreen()
    //    {
    //        WindowManager = new UIManager();
    //        WindowManager.Parent = this;

    //        Progress Progress = new Progress(0, 100, 0);
    //        Bar = new Bar();
    //        Bar.Width = Game1.Instance.graphics.PreferredBackBufferWidth / 2;
    //        Bar.Location = new Vector2((Game1.Instance.graphics.PreferredBackBufferWidth - Bar.Width) / 2, (Game1.Instance.graphics.PreferredBackBufferHeight - Bar.DefaultHeight) / 2);
    //        Bar.Text = "Loading";
    //        //WindowManager.Windows.Add(Bar);

    //        //new Map();

    //    }
    //    public override void Update(GameTime gameTime)
    //    {
    //        WindowManager.Update(gameTime);
    //        //Bar.Percentage = Map.Instance.Initialize();
    //        //Bar.Percentage = LoadedScreen.Initialize();

    //        if (Bar.Percentage == 1)
    //        {
    //            ScreenManager.Remove();
    //            Rooms.GameScreen ingame = new Rooms.Ingame();
    //            ingame.Initialize();
    //            ScreenManager.Add(ingame);
    //        }
    //    }
    //   // public override void LoadContent() { }
    //    public override void Draw(SpriteBatch sb)
    //    {
    //        sb.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend);
    //        //WindowManager.Draw(sb);
    //        Bar.Draw(sb);
    //        sb.End();
    //    }

    //    static public void Load(GameScreen screenToLoad)
    //    {
    //        LoadingScreen loading = new LoadingScreen();
    //        loading.LoadedScreen = screenToLoad;
    //    }
    //}
}
