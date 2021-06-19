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

namespace Start_a_Town_.GameModes.StaticMaps.UI
{
    class StaticWorldScreenUI : Start_a_Town_.UI.Control
    {
        #region Singleton
        static StaticWorldScreenUI _Instance;
        public static StaticWorldScreenUI Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new StaticWorldScreenUI();
                return _Instance;
            }
        }
        #endregion

        Panel Panel_Info, Panel_MapInfo, Panel_WorldButtons, Panel_WorldList;
        Button Btn_Play, Btn_EnterMap, Btn_NewWorld, Btn_Refresh, Btn_WorldRename, Btn_Back, Btn_WorldList;
        //ListBox<FileInfo, Button> Character_List;
        //PanelLabeled Panel_Map;
        MapPopUp MapPopUp;
        Button BtnEnter;
        GroupBox Box_WorldInfoButtons;

        GroupBox Box_CharacterInfo; //Box_CharacterList, 
        Window Window_Character;
        StaticWorldBrowser WorldBrowser;
        CharacterBrowser CharacterBrowser;
        StaticMapBrowser MapBrowser;

        StaticWorldScreenUI()
        {
            Panel_Info = new Panel(Vector2.Zero, new Vector2(200, 300));

            Box_WorldInfoButtons = new GroupBox(new Vector2(0, Panel_Info.ClientSize.Height - Button.DefaultHeight));
            Btn_WorldRename = new Button(Vector2.Zero, Panel_Info.ClientSize.Width, "Rename World");
            Btn_WorldRename.LeftClick += new UIEvent(Btn_WorldRename_Click);
            Box_WorldInfoButtons.Controls.Add(Btn_WorldRename);

            Panel_MapInfo = new Panel(new Vector2(WindowManager.ScreenWidth,0), new Vector2(200, 500));
            Panel_MapInfo.Anchor = new Vector2(1, 0);

            //InitCharacterList();
            InitCharacterPanel();

            Panel_WorldButtons = new Panel(Vector2.Zero);
            Panel_WorldButtons.AutoSize = true;

            Btn_Refresh = new Button(Vector2.Zero, Panel_Info.ClientSize.Width, "Refresh");
            Btn_Refresh.LeftClick += new UIEvent(Btn_Refresh_Click);

            Btn_NewWorld = new Button(Btn_Refresh.BottomLeft, Panel_Info.ClientSize.Width, "Create World");
            Btn_NewWorld.LeftClick += new UIEvent(Btn_NewWorld_Click);

            Btn_WorldList = new Button(Btn_NewWorld.BottomLeft, Panel_Info.ClientSize.Width, "World List")
            {
                LeftClickAction = () =>
                {
                    Panel_WorldList.Location = Panel_WorldButtons.BottomRight;
                    Panel_WorldList.Anchor = Vector2.UnitY;
                    this.WorldBrowser.Refresh();
                    Panel_WorldList.Toggle();
                }
            };
      

            Btn_Back = new Button(Btn_WorldList.BottomLeft, Panel_Info.ClientSize.Width, "Back");
            Btn_Back.LeftClick += new UIEvent(Btn_Back_Click);

            Panel_WorldButtons.Controls.Add(Btn_NewWorld, Btn_Refresh, Btn_WorldList, Btn_Back);
            Panel_WorldButtons.Location.Y = WindowManager.ScreenHeight - Panel_WorldButtons.Height;

            Panel_WorldList = new Panel() { AutoSize = true };
            WorldBrowser = new StaticWorldBrowser(200, WindowManager.ScreenHeight - Panel_Info.Height - Panel_WorldButtons.Height, Load);
            this.CharacterBrowser = new CharacterBrowser(200, this.WorldBrowser.Height, this.SelectCharacter);
            Panel_WorldList.Controls.Add(this.WorldBrowser);

            Btn_EnterMap = new Button(new Vector2(0, Panel_MapInfo.ClientSize.Bottom - Button.DefaultHeight), Panel_MapInfo.ClientSize.Width, "Enter Map")
            {
                HoverFunc = () => (Window_Character.Tag as GameObject).IsNull() ? "No character selected" : ""
            };
            Btn_EnterMap.LeftClick += new UIEvent(Btn_EnterMap_Click);

            //Panel_Map = new PanelLabeled(Vector2.Zero.ToString()) { AutoSize = true };
            //BtnEnter = new Button("Enter Map") { Location = Panel_Map.Controls.BottomLeft, LeftClickAction = EnterMap };
            this.MapPopUp = new MapPopUp(m => EnterMap(m));// { BtnEnter.HoverFunc = () => (Window_Character.Tag as GameObject).IsNull() ? "No character selected!" : "" };
            this.MapPopUp.BtnEnter.HoverFunc = () => (Window_Character.Tag as GameObject).IsNull() ? "No character selected!" : "";

            CheckBox platebox = Nameplate.CheckBox;
            platebox.Location = Panel_Info.TopRight;

            this.Controls.Add(Panel_Info, Panel_WorldButtons, platebox); //, Panel_MapInfo
            
            RenameWorldWindow.WorldRenamed += new EventHandler(RenameWorldWindow_WorldRenamed);

            this.MapBrowser = new StaticMapBrowser(this.EnterMap);
        }

        void EnterMap(StaticMap map)
        {
            ScreenManager.Add(new ScreenMapLoading(map as StaticMap));//, CharacterBrowser.Tag as GameObject));
            Player.Actor = this.Window_Character.Tag as GameObject;
        }

        private void InitCharacterPanel()
        {
            string lastCharName = SelectCharacterWindow.GetLastCharName();
            //if (!lastCharName.IsNull())
            //{
                
            //}
            int boxHeight = UIManager.Height - Panel_MapInfo.Height;
            Window_Character = new Window() { Title = "Character", Closable = false, Dimensions = new Vector2(200, boxHeight), Location = Panel_MapInfo.BottomRight, Anchor = Vector2.UnitX, TintFunc = () => Color.Black };

            Button btn_select = new Button("Select Character", Window_Character.Client.Width)
            {
                Location = new Vector2(0, Window_Character.Client.Height),
                Anchor = Vector2.UnitY,
                LeftClickAction = () =>
                {
                    //Box_CharacterList.Location = Window_Character.Location;
                    //Box_CharacterList.Anchor = Vector2.UnitX;
                    //RefreshCharacterList();
                    this.CharacterBrowser.Refresh();
                    //Box_CharacterList.Toggle();
                    this.CharacterBrowser.Location = Window_Character.BottomLeft;// Window_Character.Location;
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
                        //RefreshCharacterList();
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
            this.Controls.Add(Window_Character);
        }

        //private void InitCharacterList()
        //{
        //    int boxHeight = UIManager.Height - Panel_MapInfo.Height;
        //    Box_CharacterList = new GroupBox() { Dimensions = new Vector2(200, boxHeight), Location = Panel_MapInfo.BottomRight, Anchor = Vector2.UnitX };
        //    Panel panel_list = new Panel() { Dimensions = Box_CharacterList.Dimensions };
        //    Character_List = new ListBox<FileInfo, Button>(panel_list.ClientSize);
        //    panel_list.Controls.Add(Character_List);
        //    Box_CharacterList.Controls.Add(panel_list);

        //    RefreshCharacterList();

        //    Button btn_create = new Button("Create", panel_list.ClientSize.Width);
        //}

        //private void RefreshCharacterList()
        //{
        //    FileInfo[] characters = SelectCharacterWindow.GetCharacters();
        //    Character_List.Build(characters, foo => foo.Name.Split('.')[0],
        //        (file, ctrl) =>
        //        {
        //            ctrl.LeftClickAction = () =>
        //            {
        //                GameObject ch = SelectCharacterWindow.LoadCharacter(file);
        //                SelectCharacter(ch);

        //                //   Window_Character.Client.Controls.Add());
        //            };
        //            ctrl.IdleColor = Color.Black;
        //            ctrl.ColorFunc = () => new Color(0.5f, 0.5f, 0.5f, 1f);
        //        });
        //}

        private void SelectCharacter(GameObject ch)
        {
            if (ch.IsNull())
                return;
            Window_Character.Tag = ch;
            Box_CharacterInfo.Controls.Clear();
            Box_CharacterInfo.Controls.Add(ch.GetTooltip());
        }

        void Btn_Back_Click(object sender, EventArgs e)
        {
            //Net.Client.Disconnect();
            Net.Server.Stop();
            
            ScreenManager.GameScreens.Pop();
        }

        void RenameWorldWindow_WorldRenamed(object sender, EventArgs e)
        {
            this.WorldBrowser.Refresh();
        }

        void Btn_WorldRename_Click(object sender, EventArgs e)
        {
            RenameWorldWindow.Initialize(Rooms.WorldScreen.Instance.World).ShowDialog();
        }

        void Btn_Refresh_Click(object sender, EventArgs e)
        {
            this.WorldBrowser.Refresh();
        }

        void Btn_NewWorld_Click(object sender, EventArgs e)
        {
            StaticWorldCreateWindow.Instance.Callback = this.Load;
            StaticWorldCreateWindow.Instance.ShowDialog();
        }

        private void Load(IWorld world)
        {
            Rooms.WorldScreen.Instance.World = world;
            Initialize(world);
            Net.Server.SetWorld(world);
        }

        void Btn_EnterMap_Click(object sender, EventArgs e)
        {
            EnterMap();
        }
        void EnterMap(IMap map)
        {
            if (map == null)
                return;
            XDocument xml = Engine.Settings.ToXDocument();

            var profile = xml.Root.Element("Profile");
            if (profile.IsNull())
            {
                profile = new XElement("Profile");
                xml.Root.Add(profile);
            }
            var xLastWorld = profile.Element("LastWorld");
            if (xLastWorld.IsNull())
                profile.Add(new XElement("LastWorld", map.GetWorld().GetName()));
            else
                xLastWorld.Value = map.GetWorld().GetName();

            Engine.Settings = xml.ToXmlDocument();

            GameObject character = Window_Character.Tag as GameObject;
            if (character.IsNull())
                return;

            Rooms.WorldScreen.Instance.Map = null;
            Initialize(Rooms.WorldScreen.Instance.Map);

            //Net.Server.LoadMap(map);
            Engine.PlayGame(character);//, map);
            Rooms.Ingame ingame = new Rooms.Ingame();
            ScreenManager.Add(ingame.Initialize());

            string localHost = "127.0.0.1";
            Net.Client.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
            //System.Threading.Thread.Sleep(2000);
            //Net.Client.EnterWorld(Player.Actor);
        }
        void EnterMap()
        {
            Map map = Panel_MapInfo.Tag as Map;
            if (map == null)
                return;
            XDocument xml = Engine.Settings.ToXDocument();

            var profile = xml.Root.Element("Profile");
            if (profile.IsNull())
            {
                profile = new XElement("Profile");
                xml.Root.Add(profile);
            }
            var xLastWorld = profile.Element("LastWorld");
            if (xLastWorld.IsNull())
                profile.Add(new XElement("LastWorld", map.World.Name));
            else
                xLastWorld.Value = map.World.Name;

            Engine.Settings = xml.ToXmlDocument();

            GameObject character = Window_Character.Tag as GameObject;
            if (character.IsNull())
                return;

            Rooms.WorldScreen.Instance.Map = null;
            Initialize(Rooms.WorldScreen.Instance.Map);

            //Net.Server.LoadMap(map);
            Engine.PlayGame(character);//, map);
            Rooms.Ingame ingame = new Rooms.Ingame();
            ScreenManager.Add(ingame.Initialize());

            string localHost = "127.0.0.1";
            Net.Client.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
            Net.Client.Instance.EnterWorld(Player.Actor);
        }


        public void Initialize(IWorld world)
        {
            //this.World = world;
            
            ChunkLoader.Restart();
            Controls.Remove(Btn_Play);
            Panel_Info.Controls.Clear();
            Panel_MapInfo.Controls.Clear(); 
            if (world == null)
                return;
            this.Panel_Info.Controls.Add(new Label(Vector2.Zero, world.ToString()));
            Panel_Info.Controls.Add(Box_WorldInfoButtons);
            
            //if (world.GetMaps().Count == 0)//< 2)
            //    Controls.Add(Btn_Play);

            //Nameplate.Reset();
            Rooms.WorldScreen.Instance.Camera.Coordinates = Vector2.Zero;
            //WindowManager["Nameplates"].Clear();
            this.MapBrowser.SetWorld(world as StaticWorld);
            //this.MapBrowser.Location = this.MapBrowser.CenterScreen;
            this.MapBrowser.Show();
        }
        public void Initialize(MapThumb mapThumb)
        {
            if (mapThumb.IsNull())
            {
                this.MapPopUp.Hide();
                return;
            }
            var map = mapThumb.Map;
            this.MapPopUp.Show(mapThumb, Rooms.WorldScreen.Instance.Camera);
            //this.MapPopUp.Location = UIManager.Mouse;
            //new MapPopUp(EnterMap).Show();//mapThumb, Rooms.WorldScreen.Instance.Camera);
            //var panel = new PanelLabeled("GAMW");
            //panel.Location = UIManager.Mouse;
            //panel.Show();

            //this.MapPopUp.Show();
            //this.MapPopUp.Location = UIManager.Mouse;

            //Panel_Map.Controls.Remove(this.BtnEnter);// Clear();
            ////Panel_Map.Controls.Add(Btn_EnterMap);
            //Panel_Map.Controls.Add(this.BtnEnter);
            ////Panel_Map.Location = UIManager.Mouse;
            //Panel_Map.Location = Rooms.WorldScreen.Instance.Camera.GetScreenBounds(mapThumb.Global);
            //Panel_Map.Label.Text = map.Coordinates.ToString();
            ////Panel_Map.Anchor = new Vector2(0.5f, 0);
            //Panel_Map.Show();
        }
        public void Initialize(IMap map)
        {
            // Tag = map;
            //ChunkLoader.Restart();// et();
            Panel_MapInfo.Controls.Clear();
            Panel_MapInfo.Tag = map;
            if (map == null)
                return;

            Panel_MapInfo.Controls.Add(new Label(Vector2.Zero, map.ToString()), Btn_EnterMap);


            //Panel_Map.Controls.Remove(this.BtnEnter);// Clear();
            ////Panel_Map.Controls.Add(Btn_EnterMap);
            //Panel_Map.Controls.Add(this.BtnEnter);
            //Panel_Map.Location = UIManager.Mouse;
            //Panel_Map.Label.Text = map.Coordinates.ToString();
            ////Panel_Map.Anchor = new Vector2(0.5f, 0);
            //Panel_Map.Show();
        }
        //public override void HandleLButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    Panel_Map.Hide();
        //}
        //public override void HandleRButtonUp(System.Windows.Forms.HandledMouseEventArgs e)
        //{
        //    //Panel_Map.Hide();
        //    this.MapPopUp.Hide();
        //}

        public override void Reposition(Vector2 ratio)
        {
            Panel_WorldButtons.Location = Panel_WorldButtons.BottomLeftScreen;
            Panel_MapInfo.Location = Panel_MapInfo.TopRightScreen;
            this.Window_Character.Location = this.Window_Character.BottomRightScreen;
            this.Panel_MapInfo.Height = this.WindowManager.ScreenHeight - this.Window_Character.Height;
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