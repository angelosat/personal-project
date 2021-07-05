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
using Start_a_Town_.Net;

namespace Start_a_Town_.Rooms
{
    class Ingame : GameScreen
    {
        static Ingame _Instance;
        static public Ingame Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Ingame();
                return _Instance;
            }
        }
            
        public NotificationArea NotificationArea;
        public NameplateManager NameplateManager = new NameplateManager();
        //public EntityTextManager EntityTextManager = new EntityTextManager();

        bool HideInterface = false;
        public SceneState Scene = new SceneState();
        //Hud Hud = new Hud();
        public override Camera Camera { get => GetMap().Camera; set => base.Camera = value; }

        public override GameScreen Initialize(IObjectProvider net)
        {
            //Instance = this;
            //this.Camera = new Camera((int)Game1.ScreenSize.X, (int)Game1.ScreenSize.Y);
            var camera = net.Map.Camera;
            if (net is Net.Server)
                DrawServer = true;
            WindowManager = new UIManager();
            NotificationArea = new NotificationArea();

            //Camera = new Camera((int)Game1.ScreenSize.X, (int)Game1.ScreenSize.Y);
            //Hud.Instance.Show(WindowManager);
            //this.Hud = new UI.Hud(net, this.Camera);
            this.Hud = new Hud(net, camera);
            this.Hud.Initialize();
            GameMode.Current.OnHudCreated(this.Hud);
            net.Map.World.OnHudCreated(this.Hud);

            this.Hud.Show(WindowManager);
            this.NameplateManager.Show(WindowManager);
            //this.EntityTextManager.Show(WindowManager);

            ToolManager = ToolManager.Instance;// new ToolManager();
            KeyHandlers.Clear();
            // previously working
            //  KeyHandlers.Push(Camera);
            //  KeyHandlers.Push(ToolManager); // if i have it here then the hud handles 1,2,3 first instead of letting toolmanagement change game speed
            ////  KeyHandlers.Push(Nameplate.PlateManager);
            //  KeyHandlers.Push(WindowManager);
            //  KeyHandlers.Push(ContextMenuManager.Instance);
            //  KeyHandlers.Push(PopupManager.Instance);
            //  //KeyHandlers.Push(ToolManager);


            KeyHandlers.Push(ToolManager); // if i have it here then the hud handles 1,2,3 first instead of letting toolmanagement change game speed
            //  KeyHandlers.Push(Nameplate.PlateManager);
            KeyHandlers.Push(WindowManager);
            KeyHandlers.Push(ContextMenuManager.Instance);
            KeyHandlers.Push(PopupManager.Instance);

            return this;
        }

        static public IObjectProvider Net => DrawServer ? Server.Instance : Client.Instance;
        public Hud Hud;
        //SelectionArgs Selection;
        public override void Update(Game1 game, GameTime gt)
        {
            //Controller.Instance.MouseoverNext = null;
            
            if (ChunkLoader.Loading)
                if (!Engine.AsyncLoading)
                    return;

            base.Update(game, gt);

            //var global = Player.Actor == null ? Vector3.Zero : Player.Actor.Global;
            //Camera.Update(DrawServer ? Server.Instance.Map : Client.Instance.Map, global);

            //this.Camera.Update(DrawServer ? Net.Server.Instance.Map : Net.Client.Instance.Map);
            var map = DrawServer? Server.Instance.Map : Client.Instance.Map;
            //if (DrawServer)
            //    Net.Server.Instance.Map.Camera.Update(Net.Server.Instance.Map);
            //else
            //    Net.Client.Instance.Map.Camera.Update(Net.Client.Instance.Map);
            ToolManager.Update(map, this.Scene);

            map.Camera.Update(map);

            //Player.Instance.Update();

            //SpeechBubble.Update();
            //ContextMenuManager.Update();
            WindowManager.Update(game, gt);
            SpeechBubbleOld.UpdateCollisions();
            //Nameplate.UpdatePlates(this.Camera, Scene);
            this.NameplateManager.Update(this.Scene);
            //this.EntityTextManager.Update();
            NotificationArea.Update();
        //    ToolManager.Update();

        }


        public override void Draw(SpriteBatch sb)
        {
            this.Scene.ObjectBounds.Clear();
            this.Scene.ObjectsDrawn = new HashSet<GameObject>();

            var map = GetMap();
            map.Draw(sb, ToolManager, WindowManager, Scene);

            /// i moved this to camera.newdraw method
            /// DONT ERASE
            //sb.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.DepthRead, RasterizerState.CullNone);
            //map.DrawInterface(sb, map.Camera);
            //sb.End();
            /// DONT ERASE
            /// 

            ToolManager.DrawUI(sb, map);
            DrawInterface(sb, Scene);
            NotificationArea.Draw(sb);
            this.NameplateManager.Update(this.Scene);
        }

        private void DrawInterface(SpriteBatch sb, SceneState scene)
        {
       //     sb.Begin();//SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.Default, RasterizerState.CullNone);
            //sb.Draw(Sprite, Vector2.Zero, Color.White);



            if (HideInterface)
                return;

            // Nameplate.PlateManager.Draw(sb);
            var cam = DrawServer ? Server.Instance.Map.Camera : Client.Instance.Map.Camera;
            WindowManager.Draw(sb, cam);
         //   Console.WriteLine(Controller.Instance.MouseoverNext.Object);

            //ToolManager.Instance.Draw(sb);
            // Console.WriteLine("end draw interface");
      //      sb.End();
        }
        internal override void OnGameEvent(GameEvent e)
        {
            this.NameplateManager.OnGameEvent(e);
            base.OnGameEvent(e);
        } 
 
        static public IMap GetMap()
        {
            return DrawServer ? Server.Instance.Map : Client.Instance.Map;
        }
        static public IObjectProvider GetNet()
        {
            if (DrawServer)
                return Server.Instance;
            return Client.Instance;
            //return Instance.DrawServer ? Server.Instance : Client.Instance;
        }
        static public IMap CurrentMap { get { return DrawServer ? Server.Instance.Map : Client.Instance.Map; } }
        static public bool DrawServer;
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
                DrawServer = !DrawServer;
                GetMap().Camera.TopSliceChanged = true;
                //Net.Client.Console.Write("draw server: " + DrawServer.ToString());
                this.Hud.Chat.Write(Log.EntryTypes.System, string.Format("draw server: {0}", DrawServer));
            }
            if (pressed.Contains(GlobalVars.KeyBindings.DebugQuery))
            {
                //IDebuggable obj = Controller.Instance.Mouseover.Object as IDebuggable;
                var target = Controller.Instance.MouseoverBlock.Target;
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
