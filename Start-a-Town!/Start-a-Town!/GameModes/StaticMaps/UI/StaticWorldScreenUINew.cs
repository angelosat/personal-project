using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using System.Xml;
using System.Xml.Linq;
using Start_a_Town_.UI.WorldSelection;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes.StaticMaps;
using Start_a_Town_.GameModes.StaticMaps.UI;
using Start_a_Town_.GameModes.StaticMaps.Screens;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class StaticWorldScreenUINew : Window
    {
        #region Singleton
        static StaticWorldScreenUINew _Instance;
        public static StaticWorldScreenUINew Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new StaticWorldScreenUINew();
                return _Instance;
            }
        }
        #endregion

        Panel Panel_Info, Panel_MapInfo, Panel_WorldButtons, Panel_WorldList;
        Button Btn_EnterMap, Btn_NewWorld, Btn_Refresh, Btn_WorldRename, Btn_Back, Btn_WorldList;
        MapPopUp MapPopUp;
        GroupBox Box_WorldInfoButtons;

        GroupBox Box_CharacterInfo; 
        Window Window_Character;
        StaticWorldBrowser WorldBrowser;
        CharacterBrowser CharacterBrowser;
        StaticMapBrowser MapBrowser;

        WindowSelectedWorldNew SelectedWorldWindow;

        StaticWorldScreenUINew()
        {
            this.AutoSize = true;
            this.Closable = false;
            Panel_Info = new Panel(Vector2.Zero, new Vector2(200, 300));

            Box_WorldInfoButtons = new GroupBox(new Vector2(0, Panel_Info.ClientSize.Height - Button.DefaultHeight));
            Btn_WorldRename = new Button(Vector2.Zero, Panel_Info.ClientSize.Width, "Rename World");
            Btn_WorldRename.LeftClick += new UIEvent(Btn_WorldRename_Click);
            Box_WorldInfoButtons.Controls.Add(Btn_WorldRename);

            Panel_MapInfo = new Panel(new Vector2(UIManager.Width, 0), new Vector2(200, 500));
            Panel_MapInfo.Anchor = new Vector2(1, 0);

            InitCharacterPanel();

            Panel_WorldButtons = new Panel(Vector2.Zero);
            Panel_WorldButtons.AutoSize = true;

            
            Btn_WorldList = new Button("World List", Panel_Info.ClientSize.Width)
            {
                LeftClickAction = () =>
                {
                    Panel_WorldList.Location = Panel_WorldButtons.BottomRight;
                    Panel_WorldList.Anchor = Vector2.UnitY;
                    this.WorldBrowser.Refresh();
                    Panel_WorldList.Toggle();
                }
            };


            
            
            Panel_WorldList = new Panel() { AutoSize = true };
            //WorldBrowser = new StaticWorldBrowser(200, UIManager.Height - Panel_Info.Height - Panel_WorldButtons.Height, OnWorldSelected);// Load);
            WorldBrowser = new StaticWorldBrowser(200, UIManager.Height / 3, OnWorldSelected);// Load);
            this.CharacterBrowser = new CharacterBrowser(200, this.WorldBrowser.Height, this.SelectCharacter);
            Panel_WorldList.Controls.Add(this.WorldBrowser);

            //Btn_Refresh = new Button(Vector2.Zero, Panel_Info.ClientSize.Width, "Refresh") { LeftClickAction = RefreshWorlds };
            //Btn_NewWorld = new Button(Btn_Refresh.BottomLeft, Panel_Info.ClientSize.Width, "Create World") { LeftClickAction = CreateWorld };
            Btn_Refresh = new Button("Refresh", 200) { LeftClickAction = RefreshWorlds };
            Btn_NewWorld = new Button("Create World", 200) { LeftClickAction = CreateWorld };
            Btn_Back = new Button("Back", 200) { LeftClickAction = Btn_Back_Click };

            Panel_WorldButtons.AddControlsVertically(
                Btn_NewWorld, 
                Btn_Refresh
                ,
                Btn_Back
                );
            //Panel_WorldButtons.Location.Y = UIManager.Height - Panel_WorldButtons.Height;


            Btn_EnterMap = new Button(new Vector2(0, Panel_MapInfo.ClientSize.Bottom - Button.DefaultHeight), Panel_MapInfo.ClientSize.Width, "Enter Map")
            {
                HoverFunc = () => (Window_Character.Tag as GameObject) == null ? "No character selected" : ""
            };
            //Btn_EnterMap.LeftClick += new UIEvent(Btn_EnterMap_Click);

            this.MapPopUp = new MapPopUp(m => EnterMap(null, m));// { BtnEnter.HoverFunc = () => (Window_Character.Tag as GameObject).IsNull() ? "No character selected!" : "" };
            this.MapPopUp.BtnEnter.HoverFunc = () => (Window_Character.Tag as GameObject) == null ? "No character selected!" : "";

            //CheckBox platebox = Nameplate.CheckBox;
            //platebox.Location = Panel_Info.TopRight;

          
            
            RenameWorldWindow.WorldRenamed += new EventHandler(RenameWorldWindow_WorldRenamed);

            this.MapBrowser = new StaticMapBrowser(this.EnterMap);

            //this.SelectedWorldWindow = new WindowSelectedWorldNew();

            Panel_WorldList.Location = Panel_WorldButtons.Location;
            Panel_WorldList.Anchor = Vector2.UnitY;
            this.WorldBrowser.Refresh();
            this.Client.AddControlsVertically(
                this.Panel_WorldList,
                Panel_WorldButtons
                );
            this.SnapToScreenCenter();
        }


        void EnterMap(StaticMap map)
        {
            ScreenManager.Add(new ScreenMapLoading(map as StaticMap));
            PlayerOld.Actor = this.Window_Character.Tag as GameObject;
        }

        private void InitCharacterPanel()
        {
            string lastCharName = SelectCharacterWindow.GetLastCharName();
            int boxHeight = UIManager.Height - Panel_MapInfo.Height;
            Window_Character = new Window() { Title = "Character", Closable = false, Dimensions = new Vector2(200, boxHeight), Location = Panel_MapInfo.BottomRight, Anchor = Vector2.UnitX, TintFunc = () => Color.Black };

            Button btn_select = new Button("Select Character", Window_Character.Client.Width)
            {
                Location = new Vector2(0, Window_Character.Client.Height),
                Anchor = Vector2.UnitY,
                LeftClickAction = () =>
                {
                    this.CharacterBrowser.Refresh();
                    this.CharacterBrowser.Location = Window_Character.BottomLeft;
                    this.CharacterBrowser.Anchor = Vector2.One;
                    this.CharacterBrowser.Toggle();
                }
            };

            Button btn_create = new Button("Create New Character", Window_Character.Client.Width)
            {
                Location = btn_select.Location,
                Anchor = Vector2.UnitY,
                LeftClickAction = () =>
                {
                    NewCharacterWindow win = new NewCharacterWindow()
                       {
                       };
                    win.HideAction = () =>
                    {
                        SelectCharacter(win.Tag as GameObject);
                        this.CharacterBrowser.Refresh();
                    };
                    win.ShowDialog();
                }
            };

            Box_CharacterInfo = new GroupBox();
            GameObject lastChar = SelectCharacterWindow.LoadCharacter(lastCharName);
            Window_Character.Tag = lastChar;
            if (!lastChar.IsNull())
                Box_CharacterInfo.Controls.Add(lastChar.GetTooltip());
            Window_Character.Client.Controls.Add(btn_select, btn_create, Box_CharacterInfo);
        }

        private void SelectCharacter(GameObject ch)
        {
            if (ch.IsNull())
                return;
            Window_Character.Tag = ch;
            Box_CharacterInfo.Controls.Clear();
            Box_CharacterInfo.Controls.Add(ch.GetTooltip());
        }

        void Btn_Back_Click()
        {
            Net.Server.Stop();

            ScreenManager.GameScreens.Pop();
        }
        //public override bool Hide()
        //{
        //    Server.Stop();
        //    ScreenManager.GameScreens.Pop();
        //    return base.Hide();
        //}
        void RenameWorldWindow_WorldRenamed(object sender, EventArgs e)
        {
            this.WorldBrowser.Refresh();
        }

        void Btn_WorldRename_Click(object sender, EventArgs e)
        {
            RenameWorldWindow.Initialize(Rooms.WorldScreen.Instance.World).ShowDialog();
        }
        private void RefreshWorlds()
        {
            this.WorldBrowser.Refresh();
        }
        void CreateWorld()
        {
            StaticWorldCreateWindow.Instance.Callback = this.Load;
            StaticWorldCreateWindow.Instance.ShowDialog();
        }
        void OnWorldSelected(IWorld world)
        {
            this.RefreshWorlds();
            this.Load(world);
        }

        private void Load(IWorld world)
        {
            PlayerOld.Actor = this.Window_Character.Tag as GameObject;


            var map = world.GetMaps().First().Value as StaticMap;

            //this.SelectedWorldWindow.Refresh(map);
            this.SelectedWorldWindow = WindowSelectedWorldNew.Refresh(map);
            //this.SelectedWorldWindow.Show();
            this.SelectedWorldWindow.ShowFrom(this);
            this.SelectedWorldWindow.SnapToScreenCenter();
            this.WorldBrowser.Refresh();
            return;

           
            //while (!map.LoadThumbnails2())
            //{
            //    var loader = new ChunkLoader(map);
            //    var max = StaticMap.MapSize.Default.Chunks;
            //    for (int i = 0; i < max; i++)
            //    {
            //        for (int j = 0; j < max; j++)
            //        {
            //            Chunk ch;
            //            var vector = new Vector2(i, j);
            //            loader.FromFile(vector, out ch);
            //            map.GetActiveChunks().Add(vector, ch);
            //        }
            //    }
            //    map.GenerateThumbnails();
            //};
            //return;
            //Rooms.WorldScreen.Instance.World = world;
            //Initialize(world);
            //Net.Server.SetWorld(world);
        }

        //void Btn_EnterMap_Click(object sender, EventArgs e)
        //{
        //    EnterMap();
        //}
        void EnterMap(IObjectProvider net, IMap map)
        {
            if (map == null)
                return;

            Engine.Config.GetOrCreateElement("Profile").GetOrCreateElement("LastWorld").Value = map.World.GetName();


            GameObject character = Window_Character.Tag as GameObject;
            if (character == null)
                return;

            Rooms.WorldScreen.Instance.Map = null;
            Initialize(Rooms.WorldScreen.Instance.Map);

            Engine.PlayGame(character);//, map);
            Rooms.Ingame ingame = Rooms.Ingame.Instance;
            ScreenManager.Add(ingame.Initialize(net));

            string localHost = "127.0.0.1";
            Net.Client.Instance.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
        }
        void EnterMap(Game1 game)
        {
            this.EnterMap(game, Panel_MapInfo.Tag as Map);
        }
        void EnterMap(Game1 game, Map map)
        {
            if (map == null)
                return;

            Engine.Config.GetOrCreateElement("Profile").GetOrCreateElement("LastWorld").Value = map.World.GetName();


            GameObject character = Window_Character.Tag as GameObject;
            if (character.IsNull())
                return;

            Rooms.WorldScreen.Instance.Map = null;
            Initialize(Rooms.WorldScreen.Instance.Map);

            Engine.PlayGame(character);//, map);
            Rooms.Ingame ingame = Rooms.Ingame.Instance;// new Rooms.Ingame();
            ScreenManager.Add(ingame.Initialize(game.Network.Client));

            string localHost = "127.0.0.1";
            Net.Client.Instance.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
            Net.Client.Instance.EnterWorld(PlayerOld.Actor);
        }


        public void Initialize(IWorld world)
        {
            ChunkLoader.Restart();
            //Controls.Remove(Btn_Play);
            Panel_Info.Controls.Clear();
            Panel_MapInfo.Controls.Clear(); 
            if (world == null)
                return;
            this.Panel_Info.Controls.Add(new Label(Vector2.Zero, world.ToString()));
            Panel_Info.Controls.Add(Box_WorldInfoButtons);

            Rooms.WorldScreen.Instance.Camera.Coordinates = Vector2.Zero;
            this.MapBrowser.SetWorld(world as StaticWorld);
            this.MapBrowser.Show();
        }
        public void Initialize(MapThumb mapThumb)
        {
            if (mapThumb == null)
            {
                this.MapPopUp.Hide();
                return;
            }
            var map = mapThumb.Map;
            this.MapPopUp.Show(mapThumb, Rooms.WorldScreen.Instance.Camera);
            
        }
        public void Initialize(IMap map)
        {
            Panel_MapInfo.Controls.Clear();
            Panel_MapInfo.Tag = map;
            if (map == null)
                return;

            Panel_MapInfo.Controls.Add(new Label(Vector2.Zero, map.ToString()), Btn_EnterMap);

        }


        public override void Reposition(Vector2 ratio)
        {
            Panel_WorldButtons.Location = Panel_WorldButtons.BottomLeftScreen;
            Panel_MapInfo.Location = Panel_MapInfo.TopRightScreen;
            this.Window_Character.Location = this.Window_Character.BottomRightScreen;
            this.Panel_MapInfo.Height = UIManager.Height - this.Window_Character.Height;

        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.Draw(sb, UIManager.Bounds);
        }
        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb, Rectangle viewport)
        {
            base.Draw(sb, viewport);
        }
    }
}