using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class SelectWorldWindow : Window
    {
        #region Singleton
        static SelectWorldWindow _Instance;
        public static SelectWorldWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new SelectWorldWindow();
                return _Instance;
            }
        }
        #endregion

        PanelList<DirectoryInfo> Panel_Worlds, Panel_Maps;
        Button Btn_New, Btn_Play, Btn_Delete;
        Panel Panel_MapView, Panel_WorldInfo;

        GameObject Player;

        SelectWorldWindow()
        {
            this.Title = "Choose World";
            this.Movable = false;
            this.AutoSize = true;



            Panel_Worlds = new PanelList<DirectoryInfo>(Vector2.Zero, new Vector2(200, 250), foo => foo.Name);
            Panel_Worlds.SelectedItemChanged += new EventHandler<EventArgs>(Panel_Worlds_SelectedItemChanged);

            Panel_MapView = new Panel(Panel_Worlds.TopRight, new Vector2(500));
            Panel_Maps = new PanelList<DirectoryInfo>(Panel_MapView.TopRight, new Vector2(200, 250), foo => foo.Name);
           // Panel_Maps.SelectedItemChanged += new EventHandler<EventArgs>(Panel_Maps_SelectedItemChanged);
           // Panel_MapView.Controls.Add(Panel_Maps);

            Panel_WorldInfo = new Panel(Panel_Worlds.BottomLeft, new Vector2(200, 250));

            Btn_New = new Button(Panel_WorldInfo.BottomLeft, Panel_Worlds.Width, "New");
            Btn_New.LeftClick += new UIEvent(Btn_New_Click);

            Btn_Play = new Button(Btn_New.TopRight, Panel_MapView.Width, "Enter World");
            Btn_Play.LeftClick += new UIEvent(Btn_Play_Click);

            Btn_Delete = new Button(Vector2.Zero, Panel_Worlds.ClientSize.Width, "Delete");
            Btn_Delete.LeftClick += new UIEvent(Btn_Delete_Click);
            //Btn_New.Location = new Vector2(0, Panel_WorldInfo.ClientSize.Height - Btn_New.Height);
            //Panel_WorldInfo.Controls.Add(Btn_New);
            Controls.Add(Panel_Worlds, Panel_MapView, Btn_New, Panel_WorldInfo, Panel_Maps, Btn_Play);
            this.Location = CenterScreen;

            RefreshWorldList();
            NewWorldWindow.Instance.Hidden += new EventHandler<EventArgs>(Instance_Closed);
        }

        //void Panel_Maps_SelectedItemChanged(object sender, EventArgs e)
        //{
        //    Btn_Play.Text = "Enter map";
        //    Controls.Add(Btn_Play);
        //}

        public override bool Show(params object[] p)
        {
            Panel_Worlds.SelectedItem = null;
            Tag = null;
            RefreshWorldList();
            RefreshWorldInfo();
            RefreshMaps();
        //    Controls.Remove(Btn_Play);
            return base.Show(p);
        }

        //public override bool Close()
        //{
        //    return base.Hide();
        //}
        public override void Dispose()
        {
            //base.Dispose();
        }
        void Btn_Delete_Click(object sender, EventArgs e)
        {
            World world = Tag as World;
            if (world == null)
                throw new Exception("Tried to delete null world.");
            //Console.WriteLine(Directory.GetFiles(SelectedWorld.FullName));

            MessageBox delete = new MessageBox("Warning!", "Are you sure you want to delete \"" + world.Name + "\" ?");
            delete.ShowDialog();
            delete.Yes += new EventHandler<EventArgs>(delete_Yes);
        }

        void delete_Yes(object sender, EventArgs e)
        {
            (sender as MessageBox).Yes -= delete_Yes;
            DeleteWorld();
            (sender as MessageBox).Close();
        }

        void DeleteWorld()
        {
            DirectoryInfo dir = Panel_Worlds.SelectedItem as DirectoryInfo;
            if (dir == null)
                throw new Exception("Tried to delete null directory.");
            dir.Delete(true);
            Tag = null;
            RefreshWorldInfo();
            RefreshWorldList();
        }

        void Panel_Worlds_SelectedItemChanged(object sender, EventArgs e)
        {
            DirectoryInfo worldDirectory = Panel_Worlds.SelectedItem as DirectoryInfo;
           // SelectedWorld = worldDirectory;
            RefreshWorldInfo(worldDirectory);
            RefreshMaps();
            if(Panel_Maps.List.Count == 0)
            {
                Btn_Play.Text = "Enter World";
       //         Controls.Add(Btn_Play);
            }
            //Controls.Add(Btn_Play);
        }

        void RefreshMaps()
        {
            World world = Tag as World;
            DirectoryInfo worldDirectory = Panel_Worlds.SelectedItem as DirectoryInfo;
            if (worldDirectory == null)
            {
                Panel_Maps.Build();
                return;
            }
            DirectoryInfo[] mapFolders = worldDirectory.GetDirectories("*.*", SearchOption.TopDirectoryOnly);
        //    world.LoadMaps(mapFolders);
            Panel_Maps.Build(mapFolders);
        }

        void Btn_Play_Click(object sender, EventArgs e)
        {
            World world = Tag as World;
            if (Tag == null)
                return;
            ScreenManager.Add(Rooms.WorldScreen.Instance.Initialize(world));//new Rooms.WorldScreen(world));
            return;
            GameObject mapPlayer;
            Map map = Map.Create(world, Vector2.Zero);// new Map(world, Vector2.Zero);
            map.TryGetProperty<GameObject>("Player", out mapPlayer);
            if (mapPlayer == null)
            {
                if (Player == null)
                {
                    //(new NewCharacterWindow()).ShowDialog();
                  //  SelectCharacterWindow select = new SelectCharacterWindow();
                    SelectCharacterWindow.Instance.Map = map;
                    SelectCharacterWindow.Instance.ShowDialog();
                    // select.Previous = this;
                    Close();
                    return;
                }
                else
                    map["Player"] = Player;
            }
            Engine.Map = map;


            ChunkLoader.Reset();
           // map.Focus(map.GetProperty<GameObject>("Player")["Position"].GetProperty<Position>("Position").Global);
            ChunkLoader.ForceLoad(map);
            //Map = null;
            //SelectedWorld = null;
            //Player = null;
            //RefreshWorldInfo(null);
            Rooms.Ingame ingame = new Rooms.Ingame();
            ingame.Initialize();

            Start_a_Town_.Player.Instance.Control(Engine.Map, mapPlayer != null ? mapPlayer : Player);
            Close();

            ScreenManager.Add(ingame);
        }

        void Instance_Closed(object sender, EventArgs e)
        {
            this.RefreshWorldList();
        }

        void Btn_New_Click(object sender, EventArgs e)
        {
            NewWorldWindow.Instance.ShowDialog();
        }

        private void RefreshWorldList()
        {
            foreach (Control control in Panel_Worlds.Controls)
            {
                //control.DrawTooltip -= world_DrawTooltip;
                //control.DrawItem -= world_DrawItem;

            }
            Panel_Worlds.Controls.Clear();

            DirectoryInfo[] worlds = GetWorlds();
            Panel_Worlds.Build(worlds);
        }
        static public DirectoryInfo[] GetWorlds()
        {
            DirectoryInfo directory = new DirectoryInfo(GlobalVars.SaveDir + "/Worlds/");
            if (!Directory.Exists(directory.FullName))
                Directory.CreateDirectory(directory.FullName);
            return directory.GetDirectories();
        }
        static public DirectoryInfo[] GetMaps()
        {
            DirectoryInfo directory = new DirectoryInfo(GlobalVars.SaveDir + "/Worlds/");
            if (!Directory.Exists(directory.FullName))
                Directory.CreateDirectory(directory.FullName);
            return directory.GetDirectories();
        }
        private void RefreshWorldInfo(DirectoryInfo worldDirectory = null)
        {
            Panel_WorldInfo.Controls.Clear();
            Panel_MapView.Controls.Clear();
            if (worldDirectory == null)
                return;

            FileInfo[] worldFiles = worldDirectory.GetFiles("*.world.sat", SearchOption.TopDirectoryOnly);

            if (worldFiles.Length != 1)
            {
                Panel_WorldInfo.Controls.Add(new Label("Error Loading Map"));
                return;
            }
            
            FileInfo worldSave = worldFiles.First();

            try
            {
                FileInfo[] thumbFiles = worldDirectory.GetFiles("thumbnailSmallest.png");

                Texture2D thumbnail;
                if (thumbFiles.Length > 0)
                {
                    string withoutExt = thumbFiles.First().Name.Split('.')[0];
                    using (FileStream stream = new FileStream(thumbFiles.First().FullName, FileMode.Open))
                    {
                        thumbnail = Texture2D.FromStream(Game1.Instance.GraphicsDevice, stream);
                        PictureBox box_thumb = new PictureBox(Vector2.Zero, thumbnail, null, HorizontalAlignment.Left, VerticalAlignment.Top);
                        Panel_MapView.Controls.Add(box_thumb);
                    }
                }
            }
            catch (Exception) { Panel_MapView.Controls.Add(new Label("Error Loading Thumbnail")); }

            //Map = Map.Load(worldSave.FullName);
           // Tag = World.Load(worldSave.FullName);
       //     Btn_Play.Text = Map["Player"] != null ? "Enter World" : "Select character";
            Btn_Delete.Location = new Vector2(0, Panel_WorldInfo.ClientSize.Height - Btn_Delete.Height);
           // Btn_Start.Location = new Vector2(0, Btn_Delete.Top - Btn_Start.Height);
            Panel_WorldInfo.Controls.Add(new Label(Vector2.Zero, Tag.ToString()), Btn_Delete);
     //       Panel_MapView.Controls.Add(Panel_Maps);
        //    Controls.Remove(Btn_Play);
           // panel_world
        }

    }
}
