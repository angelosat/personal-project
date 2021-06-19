using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.UI;
using UI;

namespace Start_a_Town_.Rooms
{
    class DesignRoom : GameScreen
    {
        #region Singleton
        static DesignRoom _Instance;
        public static DesignRoom Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new DesignRoom();
                return _Instance;
            }
        }
        #endregion
        SceneState Scene = new SceneState();
        World BpWorld;
        Map BpMap;
        DesignRoom()
        {
            //this.Camera = new Camera(Game1.Instance.Window.ClientBounds.Width, Game1.Instance.Window.ClientBounds.Height);
            //this.WindowManager = new UI.UIManager(this);
            //DesignUI.Instance.Show(WindowManager);
            //this.ToolManager = ToolManager.Create();
            //BpWorld = World.Create(new WorldArgs(name: "New Design", flat: true, caves: false, trees: false, seed: 0, terraformer: Terraformer.Flat, lighting: false, defaultTile: Tile.Types.Blueprint));
            ////Map bpMap = bpWorld.Maps[Vector2.Zero];
            //Camera.CenterOn(new Vector3(0, 0, BpWorld.SeaLevel));
            //BpMap = Map.Create(BpWorld, Vector2.Zero);
            //BpMap.Focus(Vector3.Zero);
            //Engine.Map = BpMap;
            
            //KeyHandlers.Push(ToolManager);
            //KeyHandlers.Push(Camera);
            //KeyHandlers.Push(WindowManager);
        }

        public override GameScreen Initialize(IObjectProvider net)
        {
            this.Camera = new Camera(Game1.Instance.Window.ClientBounds.Width, Game1.Instance.Window.ClientBounds.Height);
            this.WindowManager = new UI.UIManager();
            DesignUI.Instance.Show(WindowManager);
            this.ToolManager = new ToolManager();// ToolManager.Create();
            BpWorld = World.Create(new WorldArgs(name: "New Design", trees: false, seed: 0, terraformers: new Terraformer[] { Terraformer.Land }, lighting: false, defaultTile: Block.Types.Blueprint)); //caves: false,
            //Map bpMap = bpWorld.Maps[Vector2.Zero];
            Camera.CenterOn(new Vector3(0, 0, Map.MaxHeight / 2));// BpWorld.SeaLevel));
            BpMap = Map.Create(BpWorld, Vector2.Zero);
            ChunkLoader.Restart();
          //  BpMap.Focus(Vector3.Zero);
            ChunkLoader.ForceLoad(this, BpMap, new System.Threading.CancellationToken());
            Engine.Map = BpMap;
            

            KeyHandlers = new Stack<IKeyEventHandler>();
            KeyHandlers.Push(ToolManager);
            //KeyHandlers.Push(Camera);
            KeyHandlers.Push(WindowManager);

            return this;
        }

        public override void Update(Game1 game, GameTime gt)
        {
            base.Update(game, gt);
            Camera.Update(gt);
            BpMap.Update(Net.Client.Instance);//gameTime, Camera);
            ToolManager.Update(Net.Client.Instance.Map);
            this.WindowManager.Update(game, gt);
            
        }

        public override void Draw(SpriteBatch sb)
        {
            Scene = new SceneState();
            this.Camera.DrawMap(sb, BpMap, ToolManager, WindowManager, Scene);
            //ToolManager.Draw(sb, Camera);
            this.WindowManager.Draw(sb, Net.Client.Instance.Map.Camera);
        }
    }
}
