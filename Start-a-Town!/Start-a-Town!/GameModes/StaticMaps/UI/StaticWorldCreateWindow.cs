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
    class StaticWorldCreateWindow : Window
    {
        #region Singleton
        static StaticWorldCreateWindow _Instance;
        public static StaticWorldCreateWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new StaticWorldCreateWindow();
                return _Instance;
            }
        }
        public override bool Close()
        {
            return base.Hide();
        }
        #endregion

        public Action<IWorld> Callback { get; set; }
        TextBox Txt_MapName, Txt_Seed, Txt_CharName;
        //CheckBox Chk_Trees; //Chk_Caves
        PictureBox Pic_Preview;
        //ComboBox<Terraformer> Combo_Terraformers;
        Panel Panel_Preview;
        RadioButton Rd_Name, Rd_Mutators;
        Panel Panel_Tabs, Panel_Main, Panel_Character;
        GroupBox Tab_Mutators, Tab_World;
        GroupBox Tab_Properties = new GroupBox();
        MutatorBrowser MutatorBrowser = new MutatorBrowser();
        PanelMapSizes PanelMapSizes;
        StaticWorldCreateWindow()
        {
            Title = "Create New World";
            AutoSize = true;
            Closable = true;
            Movable = false;

            Panel_Tabs = new Panel(Vector2.Zero);
            Panel_Tabs.AutoSize = true;
            Rd_Name = new RadioButton("Name") { Location = Vector2.Zero };
            Rd_Mutators = new RadioButton("Mutators") { Location = Rd_Name.BottomLeft };
            //Rd_Name = new RadioButton("Name",Vector2.Zero);
            //Rd_Mutators = new RadioButton("Mutators", Rd_Name.BottomLeft);
            Rd_Name.Checked = true;
           // RadioButton rad_character = new RadioButton("Character", new Vector2(0, Rd_Name.Bottom));
            
            //rad_character.LeftClick += new UIEvent(tab_Click);
            Rd_Name.LeftClick += new UIEvent(tab_Click);
            Rd_Mutators.LeftClick += new UIEvent(tab_Click);

            Panel_Tabs.Controls.Add(Rd_Name, Rd_Mutators);

            Tab_World = new GroupBox();
            Label label_mapsize = new Label("Map size");

            Label lbl_name = new Label(new Vector2(0, 0), "Name");
            Txt_MapName = new TextBox(new Vector2(0, lbl_name.Bottom), new Vector2(150, Label.DefaultHeight));

            //Txt_MapName.TextEntered += new EventHandler<TextEventArgs>(Txt_MapName_TextEntered);
            Label lbl_seed = new Label(new Vector2(0, Txt_MapName.Bottom), "Seed (Leave blank for random)");//0 - 4294967295)");
            Txt_Seed = new TextBox(new Vector2(0, lbl_seed.Bottom), new Vector2(150, Label.DefaultHeight));
            //Txt_Seed.TextEntered += new EventHandler<TextEventArgs>(Txt_Seed_TextEntered);

            IconButton btn_random = new IconButton() { Name = "Randomize seed", Location = Txt_Seed.TopRight,  Icon = new Icon(UIManager.Icons16x16, 1, 16) }; //BackgroundTexture = UIManager.Icon16Background, 
            
            btn_random.HoverText = "Randomize";
            btn_random.LeftClick += new UIEvent(btn_random_Click);

            //Chk_Trees = new CheckBox("Trees", Txt_Seed.BottomLeft);
            this.PanelMapSizes = new PanelMapSizes() { Location = this.Txt_Seed.BottomLeft };

            Tab_World.Controls.Add(lbl_name, Txt_MapName, lbl_seed, Txt_Seed, btn_random
                //,Chk_Trees
                ,this.PanelMapSizes
                );//, Combo_Terraformers); //Chk_Flat,Chk_Caves,


            InitCharacterTab();
            CreateTabMutators();
            Rd_Name.Tag = Tab_World;
            Rd_Mutators.Tag = this.MutatorBrowser;// new MutatorBrowser();// Tab_Mutators;

         //   rad_character.Tag = Panel_Character;
            Panel_Main = new Panel() { Location = Panel_Tabs.TopRight};//, AutoSize = true };
            Panel_Main.Conform(Tab_World, Tab_Mutators);
            Panel_Main.Size = new Rectangle(0, 0, 500, 500);
           // Panel_Main.AddControls(Tab_Mutators, Tab_World);
            //Panel_Main.Controls.Clear();
            Panel_Main.Controls.Add(Tab_World);

            Panel_Preview = new Panel() { Location = Panel_Main.TopRight, ClientSize = new Rectangle(0, 0, Map.SizeInBlocks, Map.SizeInBlocks) };
            Pic_Preview = new PictureBox();
            Panel_Preview.Controls.Add(Pic_Preview);

            Panel panel_button = new Panel(new Vector2(0, Panel_Main.Bottom), new Vector2(Panel_Tabs.Width + Panel_Main.Width, 300));
            panel_button.AutoSize = true;
            Button btn_create = new Button(Vector2.Zero, panel_button.ClientSize.Width, "Create");
            btn_create.CustomTooltip = true;
            btn_create.DrawTooltip += new EventHandler<TooltipArgs>(btn_start_DrawTooltip);
            btn_create.LeftClickAction = btn_Create_Click;

            Panel panel_btnPreview = new Panel() { Location = Panel_Preview.BottomLeft, AutoSize = true };

            Button btn_preview = new Button(Vector2.Zero, Panel_Preview.ClientSize.Width, "Preview");
            panel_btnPreview.Controls.Add(btn_preview);
            btn_preview.LeftClick += new UIEvent(btn_preview_Click);

            panel_button.Controls.Add(btn_create);//, btn_preview);

            Panel_Main.AutoSize = false;
            Client.Controls.Add(Panel_Tabs, panel_button, Panel_Main, Panel_Preview, panel_btnPreview);
            this.SnapToScreenCenter();
            this.Anchor = Vector2.One * 0.5f;
        }

        ListBox<Terraformer, Button> List_Available, List_Added;
        void CreateTabMutators()
        {
            Tab_Mutators = new GroupBox();
            Panel Panel_Available, Panel_Added, Panel_Info;
            

            Panel_Available = new Panel() { AutoSize = true };
            List_Available = new ListBox<Terraformer, Button>(new Rectangle(0, 0, 150, 300));
            Panel_Available.Controls.Add(List_Available);

            Panel_Added = new Panel() { Location = Panel_Available.TopRight, AutoSize = true };
            List_Added = new ListBox<Terraformer, Button>(new Rectangle(0, 0, 150, 300));
            Panel_Added.Controls.Add(List_Added);

            List_Available.Build(Terraformer.All, foo => foo.Name, onControlInit: (t, c) =>
            {
                // List<Terraformer> available = List_Available.List.Except(new Terraformer[] { t }).ToList();
                //List_Available.Build(List_Available.List.Except(new Terraformer[] { t }), 
                c.LeftClickAction = () => MutatorAdd(t);
            });

            Tab_Mutators.Controls.Add(Panel_Available, Panel_Added);
        }

        void MutatorAdd(Terraformer terra)
        {
            List_Available.Build(List_Available.List.Except(new Terraformer[] { terra }), foo => foo.Name, (t, c) => c.LeftClickAction = () => MutatorAdd(t));
            List_Added.Build(List_Added.List.Union(new Terraformer[] { terra }), foo => foo.Name, (t, c) => c.LeftClickAction = () => MutatorRemove(t));

            Tab_Properties.Controls.Add(terra.GetUI());
            Tab_Properties.Location = Tab_World.Controls.BottomLeft;
            Tab_World.Controls.Add(Tab_Properties);
        }
        void MutatorRemove(Terraformer terra)
        {
            List_Added.Build(List_Added.List.Except(new Terraformer[] { terra }), foo => foo.Name, (t, c) => c.LeftClickAction = () => MutatorRemove(t));
            List_Available.Build(List_Available.List.Union(new Terraformer[] { terra }), foo => foo.Name, (t, c) => c.LeftClickAction = () => MutatorAdd(t));
        }

        float Progress;
        void btn_preview_Click(object sender, EventArgs e)
        {
            Terraformer terra = List_Added.List.FirstOrDefault();// Combo_Terraformers.SelectedItem;
            if (terra.IsNull())
                return;
            Window win = new Window()
            {
                Title = "World Preview",
                AutoSize = true,
                Movable = true
            };
            int seed;
            if (!int.TryParse(Txt_Seed.Text, out seed))
            {
                if (Txt_Seed.Text.Length > 0)
                    seed = Txt_Seed.Text.GetHashCode();
                else
                    seed = (new Random()).Next(int.MinValue, int.MaxValue);
            }
            Pic_Preview.Sprite = null;
            Panel_Preview.Controls.Clear();
            LoadingBox loadbox = new LoadingBox()
            {
                Location = Panel_Preview.ClientDimensions * 0.5f,
                ProgressFunc = () => this.Progress.ToString("##0%"),
                TextFunc = () => "Generating preview",
                TintFunc = () => Color.Lerp(Color.Red, Color.Lime, this.Progress)
            };
            Panel_Preview.Controls.Add(loadbox);
            Progress = 0;
            new System.Threading.Thread(() =>
            {
                //Pic_Preview.Sprite = terra.GetThumbnail(World.Create(new WorldArgs(Txt_MapName.Text, Chk_Trees.Checked, seed, new Terraformer[] { terra })), ref Progress); // Chk_Caves.Checked,
                Pic_Preview.Sprite = terra.GetThumbnail(new World(Txt_MapName.Text, seed, new Terraformer[] { terra }), ref Progress); // Chk_Caves.Checked,

                Panel_Preview.Controls.Clear();
                Panel_Preview.Controls.Add(Pic_Preview);
            }) { Name = "Map thumbnail thread" }.Start();
        }
        

        void btn_start_DrawTooltip(object sender, TooltipArgs e)
        {
            if (AlreadyExists(Txt_MapName.Text))
                e.Tooltip.Controls.Add(new Label("Map name already exists!"));
        }

        private void InitCharacterTab()
        {
            Panel_Character = new Panel(new Vector2(Panel_Tabs.Right, 0));
            Panel_Character.AutoSize = true;
            Label lbl_charName = new Label(new Vector2(0, 0), "Character name");
            Txt_CharName = new TextBox(new Vector2(0, lbl_charName.Bottom), new Vector2(150, Label.DefaultHeight));
            Txt_CharName.Text = "Reggie Fiddlebottom";
            Txt_CharName.TextEntered += new EventHandler<TextEventArgs>(Txt_CharName_TextEntered);
            Panel_Character.Controls.Add(lbl_charName, Txt_CharName);
        }
        private void InitWorldTab()
        {
            Txt_MapName.Text = GetDefaultMapName();
            Txt_Seed.Text = "";
          //  Chk_Caves.Checked = false;
            //Chk_Trees.Checked = true;
         //   Chk_Flat.Checked = false;
        }


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
                if(k < worldNumbers.ElementAt(k))
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

        void Txt_CharName_TextEntered(object sender, TextEventArgs e)
        {
            if (e.Char == 13) //enter
            {
                Txt_CharName.Enabled = false;
            }
            else if (e.Char == 27) //escape
            {
                Txt_CharName.Enabled = false;
            }
            else
                Txt_CharName.Text += e.Char;
        }

        void tab_Click(object sender, EventArgs e)
        {
            Panel_Main.Controls.Clear();
            //Controls.Remove(Panel_Main);
            global::Start_a_Town_.UI.Control panel = (sender as global::Start_a_Town_.UI.Control).Tag as Control;
            if(panel == null)
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

        void btn_random_Click(object sender, EventArgs e)
        {
            Random r = new Random();

            Txt_Seed.Text = r.Next(int.MinValue, int.MaxValue).ToString();
        }

        void btn_Create_Click()
        {
            //var mutators = List_Added.List.ToList();
            var mutators = MutatorBrowser.GetSelected();
            if (mutators.Count == 0)
                return;

            if (AlreadyExists(Txt_MapName.Text))
                return;

            StaticWorld world;
            int seed;
            if (!int.TryParse(Txt_Seed.Text, out seed))
            {
                if (Txt_Seed.Text.Length > 0)
                    seed = Txt_Seed.Text.GetHashCode();
                else
                    seed = (new Random()).Next(int.MinValue, int.MaxValue);
            }
            world = new StaticWorld(Txt_MapName.Text, seed, mutators);

            Tag = world;

            var size = this.PanelMapSizes.SelectedSize;
            var map = new StaticMap(world, "test", Vector2.Zero, size);
            var loadingToken = new StaticMapLoadingProgressToken();
            var loadingDialog = new DialogLoading();
            loadingDialog.ShowDialog();
            Hide();
            Task.Factory.StartNew(() =>
            {
                //map.Load(loadingToken);
                int chunksCount = map.Size.Chunks * map.Size.Chunks;
                int maxTasks = chunksCount * 2;
                map.GenerateWithNotifications((t, p) => loadingDialog.Refresh(t, p / 2f));
                //map.Update();
                map.Save();
                //var n = 0;
                //foreach (var ch in map.ActiveChunks.Values)
                //{
                //    loadingDialog.Refresh("Saving chunk " + n.ToString() + " of " + chunksCount.ToString(), (1 + (n++ / (float)chunksCount)) / 2f);
                //    ch.SaveToFile();
                //}
                world.Maps.Add(map.Coordinates, map);


                world.Save();

                this.Callback(world);

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

        public override bool Show()
        {
            InitWorldTab();
            return base.Show();
        }
    }
}
