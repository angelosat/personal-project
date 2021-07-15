﻿using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class GuiNewGame : GroupBox
    {
        public Action<IWorld> Callback { get; set; }

        readonly TextBox Txt_Seed;
        readonly Panel Panel_Main;
        readonly GroupBox Tab_World;
        private StaticMap.MapSize SelectedSize;

        public GuiNewGame()
        {
            this.AutoSize = true;

            this.Tab_World = new GroupBox();
            var label_mapsize = new Label("Map size");

            var lbl_seed = new Label("Seed");
            this.Txt_Seed = new(150);
            this.Txt_Seed.Text = Path.GetRandomFileName().Replace(".", "");
            this.Txt_Seed.InputFilter = char.IsLetterOrDigit;

            IconButton btn_random = new IconButton()
            {
                HoverText = "Randomize",
                BackgroundTexture = UIManager.Icon16Background,
                Name = "Randomize seed",
                Location = this.Txt_Seed.TopRight,
                Icon = new Icon(UIManager.Icons16x16, 1, 16),
                LeftClickAction = () => this.Txt_Seed.Text = Path.GetRandomFileName().Replace(".", "")
            };
            var seedBox = new GroupBox();
            seedBox.AddControls(
                this.Txt_Seed, btn_random);


            var defaultSizes = StaticMap.MapSize.GetList();
            this.SelectedSize = defaultSizes.First();
            var comboSize = new ComboBoxLatest<StaticMap.MapSize>(defaultSizes.ToArray(), seedBox.Width, defaultSizes.Count, (c, s) => this.SelectedSize = s, (c) => this.SelectedSize);

            this.Tab_World.AddControlsVertically(
                seedBox.ToPanelLabeled("Seed"),
                comboSize.ToPanelLabeled("Map Size"));

            this.Panel_Main = new Panel() { AutoSize = true };

            this.Panel_Main.Controls.Add(this.Tab_World);

            Panel panel_button = new Panel() { Location = this.Panel_Main.BottomLeft, AutoSize = true };
            panel_button.AutoSize = true;
            Button btn_create = new Button(Vector2.Zero, panel_button.ClientSize.Width, "Create");
            btn_create.LeftClickAction = () =>
            {
                var actorsCreateBox = new GroupBox();
                var actors = new List<Actor>();
                var actorsui = new UIActorCreation(actors);
                var btnstart = new Button("Start") { LeftClickAction = () => this.btn_Create_Click(actors.ToArray()) };
                var btnback = new Button("Back") { LeftClickAction = () => { actorsui.GetWindow().Hide(); this.GetWindow().Show(); } };
                actorsCreateBox.AddControlsVertically(actorsui, btnstart, btnback);
                var createActorsWindow = new Window(actorsCreateBox) { Closable = false, Movable = false, Previous = this.GetWindow() };
                createActorsWindow.LocationFunc = () => UIManager.Center;
                createActorsWindow.Anchor = Vector2.One * .5f;
                createActorsWindow.Show();
                this.GetWindow().Hide();
            };

            panel_button.Controls.Add(btn_create);

            this.Controls.Add(
                panel_button, this.Panel_Main

                );
        }

        public static DirectoryInfo[] GetWorlds()
        {
            DirectoryInfo directory = new DirectoryInfo(GlobalVars.SaveDir + "/Worlds/Static/");
            if (!Directory.Exists(directory.FullName))
            {
                Directory.CreateDirectory(directory.FullName);
            }

            return directory.GetDirectories();
        }

        void tab_Click(object sender, EventArgs e)
        {
            this.Panel_Main.Controls.Clear();
            global::Start_a_Town_.UI.Control panel = (sender as global::Start_a_Town_.UI.Control).Tag as Control;
            if (panel == null)
            {
                return;
            }

            this.Panel_Main.AddControls(panel);
        }

        void btn_Create_Click(Actor[] actors)
        {
            StaticWorld world;
            var seedString = this.Txt_Seed.Text;
            world = new StaticWorld(seedString, Terraformer.Defaults);

            this.Tag = world;

            var size = this.SelectedSize;

            var map = new StaticMap(world, "test", Vector2.Zero, size);
            var loadingToken = new StaticMapLoadingProgressToken();
            var loadingDialog = new DialogLoading();
            loadingDialog.ShowDialog();
            this.Hide();
            Net.Server.Start();

            Task.Factory.StartNew(() =>
            {
                int chunksCount = map.Size.Chunks * map.Size.Chunks;
                int maxTasks = chunksCount * 2;
                map.GenerateWithNotificationsNew(loadingDialog.Refresh);

                world.Maps.Add(map.Coordinates, map);

                string localHost = "127.0.0.1";
                UIConnecting.Create(localHost);
                map.CameraRecenter();

                Net.Server.InstantiateMap(map); // is this needed??? YES!!! it enumerates all existing entities in the network
                foreach (var a in actors)
                {
                    map.Net.Instantiate(a);
                }

                map.SpawnStartingActors(actors);

                Net.Client.Instance.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); }); // TODO dont manipulate the gui in concurrent threads!!!!!

                loadingDialog.Close();
            });
        }
    }
}
