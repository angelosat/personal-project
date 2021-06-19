﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.PlayerControl;
using Start_a_Town_.UI;

namespace Start_a_Town_.Rooms
{
    class EditorRoom : GameScreen
    {
        #region Singleton
        static EditorRoom _Instance;
        public static EditorRoom Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new EditorRoom();
                return _Instance;
            }
        }
        #endregion
        SceneState Scene = new SceneState();
        public CancellationTokenSource Cancel;
        World BpWorld;
        //Map BpMap;
        static public Vector3 Start = new Vector3(0, 0, Map.MaxHeight / 2);

        public void CancelLoading()
        {
            Cancel.Cancel();
            Cancel = new CancellationTokenSource();
        }

        EditorRoom()
        {        //  CancellationToken = new CancellationToken();
            Cancel = new CancellationTokenSource();
        }

        public override GameScreen Initialize()
        {
            this.Camera = new Camera(Game1.Instance.Window.ClientBounds.Width, Game1.Instance.Window.ClientBounds.Height);
            this.WindowManager = new UI.UIManager();
            EditorUI.Instance.Show(WindowManager);
            this.ToolManager = new PlayerControl.ToolManager();

            //Net.Server.Start();
            //string localHost = "127.0.0.1";
            //Net.Client.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });

            Camera.CenterOn(Start);
            KeyHandlers.Clear();
            KeyHandlers.Push(Camera); 
            KeyHandlers.Push(ToolManager);           
            KeyHandlers.Push(WindowManager);

            return this;
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            Camera.Update(gameTime);
            if (Engine.Map != null)
                Engine.Map.Update(Net.Client.Instance);// BpMap.Update();//gameTime, Camera);
            ToolManager.Update();
            this.WindowManager.Update(gameTime);
        }

        public override void Draw(SpriteBatch sb)
        {
            Scene = new SceneState();
            if (Engine.Map != null)
            this.Camera.DrawMap(sb, Engine.Map, ToolManager, WindowManager, Scene);//BpMap);
            //ToolManager.Draw(sb, Camera);
            this.WindowManager.Draw(sb);
        }
    }
}
