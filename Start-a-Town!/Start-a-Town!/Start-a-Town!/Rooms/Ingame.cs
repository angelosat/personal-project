using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Start_a_Town_.UI;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Rooms
{
    class Ingame : GameScreen
    {
        static Ingame _Instance;
        static public Ingame Instance
        {
            get
            {
                if (_Instance.IsNull())
                    _Instance = new Ingame();
                return _Instance;
            }
        }
            
        public NotificationArea NotificationArea;

        bool HideInterface = false;
        SceneState Scene = new SceneState();
        //Hud Hud = new Hud();

        public Ingame()
        {
            this.Camera = new Camera((int)Game1.ScreenSize.X, (int)Game1.ScreenSize.Y);
        }

        public override GameScreen Initialize()
        {
            base.Initialize();
            //Instance = this;
            WindowManager = new UIManager();

            NotificationArea = new NotificationArea();

            //Camera = new Camera((int)Game1.ScreenSize.X, (int)Game1.ScreenSize.Y);
            //Hud.Instance.Show(WindowManager);
            this.Hud = new UI.Hud();
            this.Hud.Initialize();
            GameMode.Current.OnHudCreated(this.Hud);
            this.Hud.Show(WindowManager);

            ToolManager = ToolManager.Instance;// new ToolManager();
            ToolManager.ActiveTool = new DefaultTool();

            KeyHandlers.Push(Camera);
            KeyHandlers.Push(ToolManager);
          //  KeyHandlers.Push(Nameplate.PlateManager);
            
            KeyHandlers.Push(WindowManager);
            KeyHandlers.Push(ContextMenuManager.Instance);
            KeyHandlers.Push(PopupManager.Instance);
            return this;
        }


        public Hud Hud;
        //SelectionArgs Selection;
        public override void Update(GameTime gameTime)
        {
            //Controller.Instance.MouseoverNext = null;
            
            if (ChunkLoader.Loading)
                if (!(bool)Engine.Instance["AsyncLoading"])
                    return;

            base.Update(gameTime);

            var global = Player.Actor == null ? Vector3.Zero : Player.Actor.Global;
            //Camera.Update(Net.Client.Instance, Player.Actor);
            Camera.Update(Net.Client.Instance, global);

            //if (Map.Instance != null)
            //    Map.Instance.Update(gameTime, Camera);
            if (Engine.Map != null)
                Engine.Map.Update(Net.Client.Instance);//gameTime, Camera);
            ToolManager.Update(this.Scene);
            Player.Instance.Update();
            //SpeechBubble.Update();
            ContextMenuManager.Update();
            WindowManager.Update(gameTime);
            SpeechBubbleOld.UpdateCollisions();
            Nameplate.UpdatePlates(this.Camera, Scene);
            NotificationArea.Update();
        //    ToolManager.Update();
            
        }


        public override void Draw(SpriteBatch sb)
        {
            this.Scene.ObjectBounds.Clear();
            this.Scene.ObjectsDrawn.Clear();


            //if (!Engine.Map.IsNull())
            //{
                //Camera.DrawMap(sb, this.DrawServer ? Net.Server.Instance.Map : Net.Client.Instance.Map, ToolManager, WindowManager, Scene);
                Camera.DrawMap(sb, DrawServer ? Net.Server.Instance.Map : Net.Client.Instance.Map, ToolManager, WindowManager, Scene);

                //Camera.DrawMap(sb, Net.Client.Instance.Map, ToolManager, WindowManager, Scene);//Map.Instance);
                //Camera.DrawMap(sb, Net.Server.Instance.Map, ToolManager, WindowManager, Scene);//Map.Instance);
                sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);
            //ToolManager.DrawUI(sb, this.Camera);
                if (!Engine.Map.IsNull())
                Engine.Map.DrawInterface(sb, this.Camera);//, scene);

                sb.End();
            //}



          //  ToolManager.Draw(sb, Camera);
          //  SpeechBubble.Draw(sb, Camera);
            //Console.WriteLine(Controller.Instance.Mouseover.Object);
            DrawInterface(sb, Scene);
            ToolManager.DrawUI(sb, this.Camera);
            NotificationArea.Draw(sb);

            //if(ChunkLoader.Loading)
            //    LoadingScreen.Draw(sb, "Loading chunks " + ChunkLoader.Counter);
            
        }

        private void DrawInterface(SpriteBatch sb, SceneState scene)
        {
       //     sb.Begin();//SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            //sb.Draw(Sprite, Vector2.Zero, Color.White);



            if (HideInterface)
                return;

           // Nameplate.PlateManager.Draw(sb);
            WindowManager.Draw(sb);
         //   Console.WriteLine(Controller.Instance.MouseoverNext.Object);

            //ToolManager.Instance.Draw(sb);
            // Console.WriteLine("end draw interface");
      //      sb.End();
        }

        //private void SmoothLight(SpriteBatch sb, GraphicsDevice gfx)
        //{
        //    if (final == null)
        //        final = new RenderTarget2D(gfx, Game1.Instance.graphics.PreferredBackBufferWidth, Game1.Instance.graphics.PreferredBackBufferHeight, false, SurfaceFormat.Color, DepthFormat.Depth16, 0, RenderTargetUsage.PreserveContents);  //new RenderTarget2D(gfx, Game1.Instance.graphics.PreferredBackBufferWidth, Game1.Instance.graphics.PreferredBackBufferHeight);
        //    gfx.SetRenderTarget(final);
        //    gfx.Textures[2] = LightMap;
        //    gfx.Clear(Color.Black);
        //    Game1.Instance.Effect.CurrentTechnique = Game1.Instance.Effect.Techniques["SmoothLight"];
        //    Game1.Instance.Effect.CurrentTechnique.Passes[0].Apply();
        //    sb.Begin(SpriteSortMode.FrontToBack, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone, Game1.Instance.Effect);
        //    sb.Draw(MapRender, new Vector2(0, 0), Color.White);
        //    sb.End();
        //}

        //public override void Dispose()
        //{
        //    ChunkLoader.Stop();
        //    ChunkLighter.Stop();
        //    KeyHandlers.Clear();
        //    Engine.Map.Dispose();
        //    Engine.Map = null;
        //    Player.Instance.Dispose();
        //    WindowManager.Dispose();
        //    Instance = null;
        //    base.Dispose();
        //}
        static public IMap GetMap()
        {
            return DrawServer ? Net.Server.Instance.Map : Net.Client.Instance.Map;
        }
        static public Net.IObjectProvider GetNet()
        {
            if (DrawServer)
                return Net.Server.Instance;
            return Net.Client.Instance;
            //return Instance.DrawServer ? Net.Server.Instance : Net.Client.Instance;
        }
        static public IMap CurrentMap { get { return DrawServer ? Net.Server.Instance.Map : Net.Client.Instance.Map; } }
        static bool DrawServer;
        public override void HandleKeyDown(System.Windows.Forms.KeyEventArgs e)
        {
            base.HandleKeyDown(e);
            if (e.Handled)
                return;
            List<System.Windows.Forms.Keys> pressed = Controller.Input.GetPressedKeys();
            if (pressed.Contains(GlobalVars.KeyBindings.HideInterface))
                HideInterface = !HideInterface;
            if (pressed.Contains(System.Windows.Forms.Keys.F6))
            {
                //this.DrawServer = !this.DrawServer;
                //Net.Client.Console.Write("draw server: " + this.DrawServer.ToString());
                DrawServer = !DrawServer;
                Net.Client.Console.Write("draw server: " + DrawServer.ToString());
            }
            if (pressed.Contains(GlobalVars.KeyBindings.DebugQuery))
            {
                //IDebuggable obj = Controller.Instance.Mouseover.Object as IDebuggable;
                var target = Controller.Instance.Mouseover.Target;
                if (target != null)
                {
                    if (target.Type == TargetType.Entity)
                    {
                        IDebuggable obj = target.Object as IDebuggable;
                        if (obj != null)
                        {
                            if (obj.Debug() != null)
                            {
                                DebugQueryWindow win = new DebugQueryWindow(obj.Debug());
                                win.Show();
                            }
                        }
                    }
                }
            }
        }
        
        //public override void HandleInput(InputState input)
        //{
        //    if (input.IsKeyPressed(Keys.F4))
        //    {
        //        Camera.RenderIndex = (Camera.RenderIndex + 1) % 3;
        //        input.Handled = true;
        //    }
        //    base.HandleInput(input);
        //}
    }
}
