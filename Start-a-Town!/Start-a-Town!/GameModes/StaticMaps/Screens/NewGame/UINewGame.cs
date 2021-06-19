using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI.WorldSelection;
using Start_a_Town_.GameModes;
using Start_a_Town_.UI;

namespace Start_a_Town_.GameModes.StaticMaps
{
    class UINewGame : GroupBox
    {
        public Action<IWorld> Callback { get; set; }
        TextBox Txt_Seed;// Txt_MapName, , Txt_CharName;
        //CheckBox Chk_Trees; //Chk_Caves
        //PictureBox Pic_Preview;
        //ComboBox<Terraformer> Combo_Terraformers;
        //Panel Panel_Preview;
        //RadioButton Rd_Name, Rd_Mutators;
        Panel
            //Panel_Tabs,
            Panel_Main;
            //, Panel_Character;
        GroupBox //Tab_Mutators, 
            Tab_World;
        //GroupBox Tab_Properties = new GroupBox();
        MutatorBrowser MutatorBrowser = new MutatorBrowser();
        PanelMapSizes PanelMapSizes;
        private StaticMap.MapSize SelectedSize;
        public UINewGame()
        {
            AutoSize = true;

            //Panel_Tabs = new Panel(Vector2.Zero);
            //Panel_Tabs.AutoSize = true;
            //Rd_Name = new RadioButton("Name") { Location = Vector2.Zero };
            //Rd_Mutators = new RadioButton("Mutators") { Location = Rd_Name.BottomLeft };
            //Rd_Name.Checked = true;

            //Rd_Name.LeftClick += new UIEvent(tab_Click);
            //Rd_Mutators.LeftClick += new UIEvent(tab_Click);

            //Panel_Tabs.Controls.Add(Rd_Name, Rd_Mutators);

            Tab_World = new GroupBox();
            Label label_mapsize = new Label("Map size");

            //Label lbl_name = new Label(new Vector2(0, 0), "Name");
            //Txt_MapName = new TextBox(new Vector2(0, lbl_name.Bottom), new Vector2(150, Label.DefaultHeight));

            Label lbl_seed = new Label("Seed");// (Leave blank for random)");//0 - 4294967295)"); //new Vector2(0, Txt_MapName.Bottom), 
            Txt_Seed = new TextBox(150);// { BackgroundColor = Color.White * .1f};// new Vector2(0, lbl_seed.Bottom), new Vector2(150, Label.DefaultHeight));
            this.Txt_Seed.Text = Path.GetRandomFileName().Replace(".", "");
            this.Txt_Seed.InputFilter = char.IsLetterOrDigit;

            IconButton btn_random = new IconButton()
            {
                HoverText = "Randomize",
                BackgroundTexture = UIManager.Icon16Background,
                Name = "Randomize seed",
                Location = Txt_Seed.TopRight,
                Icon = new Icon(UIManager.Icons16x16, 1, 16),
                LeftClickAction = () => this.Txt_Seed.Text = Path.GetRandomFileName().Replace(".", "") // btn_random_Click }; //BackgroundTexture = UIManager.Icon16Background, 
            };
            var seedBox = new GroupBox();
            seedBox.AddControls(//lbl_seed, 
                Txt_Seed, btn_random);
            //btn_random.HoverText = "Randomize";
            //btn_random.LeftClick += new UIEvent(btn_random_Click);

            this.PanelMapSizes = new PanelMapSizes() { Location = this.Txt_Seed.BottomLeft };

            var defaultSizes = StaticMap.MapSize.GetList();
            this.SelectedSize = defaultSizes.First();
            var comboSize = new ComboBoxLatest<StaticMap.MapSize>(defaultSizes.ToArray(), seedBox.Width, defaultSizes.Count, (c, s) => this.SelectedSize = s, (c) => this.SelectedSize);// { Location = this.Txt_Seed.BottomLeft }

            this.Tab_World.AddControlsVertically(
                seedBox.ToPanelLabeled("Seed"), 
                comboSize.ToPanelLabeled("Map Size"));

            ////Tab_World.Controls.Add(//lbl_name, Txt_MapName, 
            //    //lbl_seed, Txt_Seed, btn_random
            //    seedBox
            //    //, this.PanelMapSizes
            //    , comboSize
            //    );


            //InitCharacterTab();
            //CreateTabMutators();
            //Rd_Name.Tag = Tab_World;
            //Rd_Mutators.Tag = this.MutatorBrowser;

            Panel_Main = new Panel() { AutoSize = true };
            //Panel_Main.Conform(Tab_World, Tab_Mutators);
            //Panel_Main.Size = new Rectangle(0, 0, 500, 500);

            Panel_Main.Controls.Add(Tab_World);

            //Panel_Preview = new Panel() { Location = Panel_Main.TopRight, ClientSize = new Rectangle(0, 0, Map.SizeInBlocks, Map.SizeInBlocks) };
            //Pic_Preview = new PictureBox();
            //Panel_Preview.Controls.Add(Pic_Preview);

            Panel panel_button = new Panel() { Location = Panel_Main.BottomLeft, AutoSize = true };// new Vector2(0, Panel_Main.Bottom), new Vector2(Panel_Tabs.Width + Panel_Main.Width, 300));
            panel_button.AutoSize = true;
            Button btn_create = new Button(Vector2.Zero, panel_button.ClientSize.Width, "Create");
            //btn_create.CustomTooltip = true;
            //btn_create.DrawTooltip += new EventHandler<TooltipArgs>(btn_start_DrawTooltip);
            btn_create.LeftClickAction = () =>// btn_Create_Click;
            {
                var actorsCreateBox = new GroupBox();
                var actors = new List<Actor>();
                var actorsui = new UIActorCreation(actors);
                var btnstart = new Button("Start") { LeftClickAction = () => btn_Create_Click(actors.ToArray()) };
                var btnback = new Button("Back") { LeftClickAction = ()=> { actorsui.GetWindow().Hide(); this.GetWindow().Show(); } };
                actorsCreateBox.AddControlsVertically(actorsui, btnstart, btnback);
                //var createActorsWindow = new Window(actorsCreateBox) { Closable = false, Movable = false, Previous = this.GetWindow() }.SnapToScreenCenter().Show();
                var createActorsWindow = new Window(actorsCreateBox) { Closable = false, Movable = false, Previous = this.GetWindow() };//.SnapToScreenCenter().Show();
                createActorsWindow.LocationFunc = () => UIManager.Center;
                createActorsWindow.Anchor = Vector2.One * .5f;
                createActorsWindow.Show();
                this.GetWindow().Hide();
            };
            //Panel panel_btnPreview = new Panel() { Location = Panel_Preview.BottomLeft, AutoSize = true };

            //Button btn_preview = new Button(Vector2.Zero, Panel_Preview.ClientSize.Width, "Preview");
            //panel_btnPreview.Controls.Add(btn_preview);
            //btn_preview.LeftClick += new UIEvent(btn_preview_Click);

            panel_button.Controls.Add(btn_create);//, btn_preview);

            this.Controls.Add(
                //Panel_Tabs,
                panel_button, Panel_Main
                //,
                //Panel_Preview, 
                //panel_btnPreview
                );
            //this.SnapToScreenCenter();
            //this.Anchor = Vector2.One * 0.5f;
        }

        //ListBox<Terraformer, Button> List_Available, List_Added;
        

        //float Progress;



        //void btn_start_DrawTooltip(object sender, TooltipArgs e)
        //{
        //    if (AlreadyExists(Txt_MapName.Text))
        //        e.Tooltip.Controls.Add(new Label("Map name already exists!"));
        //}

        //private void InitWorldTab()
        //{
        //    Txt_MapName.Text = GetDefaultMapName();
        //    Txt_Seed.Text = "";
        //    //  Chk_Caves.Checked = false;
        //    //Chk_Trees.Checked = true;
        //    //   Chk_Flat.Checked = false;
        //}


        string GetDefaultMapName()
        {
            DirectoryInfo directory = new DirectoryInfo(GlobalVars.SaveDir + "/Worlds/Static/");
            if (!Directory.Exists(directory.FullName))
                Directory.CreateDirectory(directory.FullName);
            DirectoryInfo[] directories = directory.GetDirectories("World_*", SearchOption.TopDirectoryOnly);
            SortedSet<int> worldNumbers = new SortedSet<int>();//directories.ForEach(foo => Int16.Parse(foo.Name.Split('.')[0].Split('_')[1])));
            foreach (DirectoryInfo dir in directories)
            {
                short i;
                if (Int16.TryParse(dir.Name.Split('.')[0].Split('_')[1], out i))
                    worldNumbers.Add(i);
            }
            if (directories.Length == 0)
                return "World_0";

            int k = 0;
            while (k < worldNumbers.Count)
            {
                if (k < worldNumbers.ElementAt(k))
                    return "World_" + k;
                k++;
            }

            return "World_" + k;
        }

        static public DirectoryInfo[] GetWorlds()
        {
            DirectoryInfo directory = new DirectoryInfo(GlobalVars.SaveDir + "/Worlds/Static/");
            if (!Directory.Exists(directory.FullName))
                Directory.CreateDirectory(directory.FullName);
            return directory.GetDirectories();
        }

        bool AlreadyExists(string name)
        {
            DirectoryInfo[] files = GetWorlds();

            foreach (DirectoryInfo filename in files)
                if (filename.Name == name)
                    return true;

            return false;
        }

        //void Txt_MapName_TextEntered(object sender, TextEventArgs e)
        //{
        //    TextBox.DefaultTextHandling(Txt_MapName, e);
        //    //if (e.Char == 13) //enter
        //    //{
        //    //    Txt_MapName.Enabled = false;
        //    //}
        //    //else if (e.Char == 27) //escape
        //    //{
        //    //    Txt_MapName.Enabled = false;
        //    //}
        //    //else
        //    //    Txt_MapName.Text += e.Char;
        //}

        //void Txt_CharName_TextEntered(object sender, TextEventArgs e)
        //{
        //    if (e.Char == 13) //enter
        //    {
        //        Txt_CharName.Enabled = false;
        //    }
        //    else if (e.Char == 27) //escape
        //    {
        //        Txt_CharName.Enabled = false;
        //    }
        //    else
        //        Txt_CharName.Text += e.Char;
        //}

        void tab_Click(object sender, EventArgs e)
        {
            Panel_Main.Controls.Clear();
            //Controls.Remove(Panel_Main);
            global::Start_a_Town_.UI.Control panel = (sender as global::Start_a_Town_.UI.Control).Tag as Control;
            if (panel == null)
                return;
            Panel_Main.AddControls(panel);
            //Controls.Add(panel);
            // Panel_Main = panel;
        }

        //void Txt_Seed_TextEntered(object sender, TextEventArgs e)
        //{
        //    //if (e.Char == 13 || e.Char == 27) //enter || escape
        //    //    Txt_Seed.Enabled = false;
        //    //else
        //    //    Txt_Seed.Text += e.Char;

        //    switch (e.Char)
        //    {
        //        //case '\033':
        //        case '\n':
        //            Txt_Seed.Enabled = false;
        //            break;
        //        case '\b':
        //            break;
        //        default:
        //            Txt_Seed.Text += e.Char;
        //            break;
        //    }
        //}

        void btn_random_Click()
        {
            Random r = new Random();

            Txt_Seed.Text = r.Next(int.MinValue, int.MaxValue).ToString();
        }

        void btn_Create_Click(Actor[] actors)
        {
            //if (AlreadyExists(Txt_MapName.Text))
            //    return;

            StaticWorld world;
            var seedString = Txt_Seed.Text;

            //world = new StaticWorld(Txt_MapName.Text, seedString, Terraformer.Defaults);// mutators);
            world = new StaticWorld(seedString, Terraformer.Defaults);// mutators);

            Tag = world;

            //var size = this.PanelMapSizes.SelectedSize;
            var size = this.SelectedSize;

            var map = new StaticMap(world, "test", Vector2.Zero, size);
            var loadingToken = new StaticMapLoadingProgressToken();
            var loadingDialog = new DialogLoading();
            loadingDialog.ShowDialog();
            Hide();
            Net.Server.Start();

            Task.Factory.StartNew(() =>
            {

                int chunksCount = map.Size.Chunks * map.Size.Chunks;
                int maxTasks = chunksCount * 2;
                //map.GenerateWithNotifications((t, p) => loadingDialog.Refresh(t, p / 2f));
                map.GenerateWithNotificationsNew(loadingDialog.Refresh);
                //map.Save();

                world.Maps.Add(map.Coordinates, map);

                string localHost = "127.0.0.1";
                UIConnecting.Create(localHost);
                map.CameraRecenter();
                //Net.Server.Start();

                Net.Server.InstantiateMap(map); // is this needed??? YES!!! it enumerates all existing entities in the network
                map.SpawnStartingActors(actors);

                Net.Client.Instance.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });

                //world.Save();
                //this.Callback(world);

                loadingDialog.Close();
            });
        }

        void GenerateMap(Map map)
        {
            throw new Exception();

            //Chunk chunk = ChunkLoader.Load(map, Vector2.Zero);
            //map.AddChunk(chunk);
            //LightingEngine engine = new LightingEngine(map)
            //{
            //    OutdoorBlockHandler = (ch, cell) =>
            //    {
            //        ch.VisibleOutdoorCells[Chunk.GetCellIndex(cell.LocalCoords)] = cell;
            //    }
            //};
            //var positions = chunk.ResetHeightMap();
            //engine.HandleBatchSync(positions);
        }
    }
}
