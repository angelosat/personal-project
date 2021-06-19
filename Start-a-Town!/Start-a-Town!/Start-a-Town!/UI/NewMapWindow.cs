using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class NewMapWindow : Window
    {
        TextBox Txt_MapName, Txt_Seed, Txt_CharName;
        CheckBox Chk_Caves, Chk_Trees, Chk_Flat;
       // ComboBox Cb_mapsize;
        Panel Panel_Tabs, Panel_Main, Panel_World, Panel_Character;
        public NewMapWindow()
        {
            Title = "Create New World";
            AutoSize = true;
            Closable = true;
            Movable = false;

            Panel_Tabs = new Panel(Vector2.Zero);
            Panel_Tabs.AutoSize = true;
            RadioButton rad_world = new RadioButton("World", Vector2.Zero);
            rad_world.Checked = true;
            RadioButton rad_character = new RadioButton("Character", new Vector2(0, rad_world.Bottom));
            
            rad_character.Click += new UIEvent(tab_Click);
            rad_world.Click += new UIEvent(tab_Click);
            //rad_character.Click += new UIEvent(rad_character_Click);
            //rad_world.Click += new UIEvent(rad_world_Click);
            Panel_Tabs.Controls.Add(rad_world);//, rad_character);
            

            //Panel_Main = new Panel(new Vector2(Panel_Tabs.Right, 0));
            //Panel_Main.AutoSize = true;

            InitWorldTab();
            InitCharacterTab();

            rad_world.Tag = Panel_World;
            rad_character.Tag = Panel_Character;

           // Panel_Main = Panel_World;
            rad_world.PerformClick();

            Panel panel_button = new Panel(new Vector2(0, Panel_Main.Bottom), new Vector2(Panel_Tabs.Width + Panel_Main.Width, 300));
            panel_button.AutoSize = true;
            Button btn_create = new Button(Vector2.Zero, panel_button.ClientSize.Width, "Create"); //"Start-a-Town!");
            btn_create.CustomTooltip = true;
            btn_create.DrawTooltip += new EventHandler<TooltipArgs>(btn_start_DrawTooltip);
            btn_create.Click += new UIEvent(btn_Create_Click); 
            panel_button.Controls.Add(btn_create);
            
            Panel_Main.AutoSize = false;
            Controls.Add(Panel_Tabs, panel_button, Panel_Main);
            Location = CenterScreen;
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
            Panel_World = new Panel(new Vector2(Panel_Tabs.Right, 0));
            Panel_World.AutoSize = true;
            Label label_mapsize = new Label("Map size");

            //Cb_mapsize = new ComboBox(new Vector2(0, label_mapsize.Bottom), 100);

            //foreach (Map.MapSize mapsize in Map.Sizes)
            //    Cb_mapsize.Items.Add(mapsize);

            //Cb_mapsize.DisplayMember = "Name";
            //Cb_mapsize.SelectedIndex = 2;

            Label lbl_name = new Label(new Vector2(0, 0), "Name");
            Txt_MapName = new TextBox(new Vector2(0, lbl_name.Bottom), new Vector2(150, Label.DefaultHeight));
            Txt_MapName.Text = GetDefaultMapName();
            Txt_MapName.TextEntered += new EventHandler<TextEventArgs>(Txt_MapName_TextEntered);
            Label lbl_seed = new Label(new Vector2(0, Txt_MapName.Bottom), "Seed (Leave blank for random)");//0 - 4294967295)");
            Txt_Seed = new TextBox(new Vector2(0, lbl_seed.Bottom), new Vector2(150, Label.DefaultHeight));
            Txt_Seed.TextEntered += new EventHandler<TextEventArgs>(Txt_Seed_TextEntered);
            Txt_Seed.Text = "";
            IconButton btn_random = new IconButton() { Location = new Vector2(Txt_Seed.Right, Txt_Seed.Top + (int)((Txt_Seed.Height - UIManager.Icon16Background.Height) / 2f)), Icon = new Icon(UIManager.Icons16x16, 1, 16) };
            btn_random.HoverText = "Randomize";
            btn_random.Click += new UIEvent(btn_random_Click);

            Chk_Caves = new CheckBox("Caves", new Vector2(Txt_Seed.Left, Txt_Seed.Bottom));
            Chk_Caves.Checked = false;
          //  Chk_Caves.Click += new UIEvent(Chk_Caves_Click);
            Chk_Trees = new CheckBox("Trees", new Vector2(Chk_Caves.Left, Chk_Caves.Bottom));
            Chk_Trees.Checked = true;
          //  Chk_Trees.Click += new UIEvent(Chk_Trees_Click);
            Chk_Flat = new CheckBox("Flat", new Vector2(Chk_Caves.Left, Chk_Trees.Bottom));
            Chk_Flat.Checked = false;
          //  Chk_Flat.Click += new UIEvent(Chk_Flat_Click);

            Panel_World.Controls.Add(lbl_name, Txt_MapName, lbl_seed, Txt_Seed, btn_random, Chk_Caves, Chk_Trees, Chk_Flat);
        }

        //void Chk_Flat_Click(object sender, EventArgs e)
        //{
        //    (sender as CheckBox).Checked = !(sender as CheckBox).Checked;
        //}

        string GetDefaultMapName()
        {
            
            DirectoryInfo directory = new DirectoryInfo(GlobalVars.SaveDir + "/Worlds/");
            DirectoryInfo[] directories = directory.GetDirectories("World_*", SearchOption.TopDirectoryOnly);
            SortedSet<int> worldNumbers = new SortedSet<int>();//directories.ForEach(foo => Int16.Parse(foo.Name.Split('.')[0].Split('_')[1])));
            foreach (DirectoryInfo dir in directories)
                worldNumbers.Add(Int16.Parse(dir.Name.Split('.')[0].Split('_')[1]));

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

        //string GetDefaultMapName()
        //{
        //    DirectoryInfo[] files = SelectWorldWindow.GetWorlds(); //directory.GetDirectories("World_*", SearchOption.TopDirectoryOnly);

        //    if (files.Length == 0)
        //        return "World_0";

        //    int n = 0;
        //    string save = "World_" + n;

        //    while (n < files.Length)
        //    {
        //        if (files[n].Name != save)
        //        {
        //            return save;
        //        }
        //        n++;
        //        save = "World_" + n;
        //    }

        //    return save;
        //}

        

        bool AlreadyExists(string name)
        {
           // DirectoryInfo directory = new DirectoryInfo(GlobalVars.SaveDir);
            DirectoryInfo[] files = NewWorldWindow.GetWorlds();// directory.GetDirectories("World_*", SearchOption.TopDirectoryOnly);


            foreach (DirectoryInfo filename in files)
                if (filename.Name == name)
                    return true;

            return false;
        }

        void Txt_MapName_TextEntered(object sender, TextEventArgs e)
        {
            if (e.Char == 13) //enter
            {
                Txt_MapName.Enabled = false;
            }
            else if (e.Char == 27) //escape
            {
                Txt_MapName.Enabled = false;
            }
            else
                Txt_MapName.Text += e.Char;
        }

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

        //void rad_world_Click(object sender, EventArgs e)
        //{
        //    Controls.Remove(Panel_Main);
        //    Controls.Add(Panel_World);
        //    Panel_Main = Panel_World;
        //}

        //void rad_character_Click(object sender, EventArgs e)
        //{
        //    Controls.Remove(Panel_Main);
        //    Controls.Add(Panel_Character);
        //    Panel_Main = Panel_Character;
        //}

        void tab_Click(object sender, EventArgs e)
        {
            Controls.Remove(Panel_Main);
            Panel panel = (sender as Control).Tag as Panel;
            if(panel == null)
                return;
            Controls.Add(panel);
            Panel_Main = panel;
        }


        //void Chk_Trees_Click(object sender, EventArgs e)
        //{
        //    Chk_Trees.Checked = !Chk_Trees.Checked;
        //}

        //void Chk_Caves_Click(object sender, EventArgs e)
        //{
        //    Chk_Caves.Checked = !Chk_Caves.Checked;
        //}

        void Txt_Seed_TextEntered(object sender, TextEventArgs e)
        {
            if (e.Char == 13 || e.Char == 27) //enter || escape
                Txt_Seed.Enabled = false;
            else
                Txt_Seed.Text += e.Char;
        }

        //void Txt_Seed_TextEntered(object sender, TextEventArgs e)
        //{
        //    char letter = e.Char;
        //    //Console.WriteLine(letter + " " + (int)key);
        //    int n = (int)Char.GetNumericValue(letter);
        //    string newtext = Txt_Seed.Text + n;
        //    if (n >= 0 && n < 10)
        //    {
        //        if (Txt_Seed.Text.Length == 0)
        //            Txt_Seed.Text += n;
        //        else
        //        {
        //            try
        //            {
        //                uint newvalue = UInt32.Parse(newtext);
        //                Txt_Seed.Text = newtext;
        //            }
        //            catch (OverflowException) { Txt_Seed.Text = (uint.MaxValue).ToString(); }
        //        }
        //    }
        //}

        void btn_random_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            //Txt_Seed.Text = ((uint)r.Next(int.MinValue, int.MaxValue) + int.MaxValue).ToString();
            Txt_Seed.Text = r.Next(int.MinValue, int.MaxValue).ToString();
        }

        //void Txt_Seed_KeyPress(object sender, KeyPressEventArgs e)
        //{
        //    Microsoft.Xna.Framework.Input.Keys key = e.Key;
        //    char letter = (char)((int)key);
        //    int n = (int)Char.GetNumericValue(letter);
        //    string newtext = Txt_Seed.Text + n;
        //    if (n >= 0 && n < 10)
        //    {
        //        if (Txt_Seed.Text.Length == 0)
        //            Txt_Seed.Text += n;
        //        else
        //        {
        //            try
        //            {
        //                uint newvalue = UInt32.Parse(newtext);
        //                Txt_Seed.Text = newtext;
        //            }
        //            catch (OverflowException) { Txt_Seed.Text = (uint.MaxValue).ToString(); }

                        
        //        }
        //    }
        //}

        void btn_Create_Click(object sender, EventArgs e)
        {
            //Player.PlayerName = Txt_MapName.Text;
            if (AlreadyExists(Txt_MapName.Text))
                return;

            //Rooms.Ingame ingame = new Rooms.Ingame();
            //ingame.Initialize();
            //ScreenManager.Add(ingame);

            Map map;
            int seed;
            if (!int.TryParse(Txt_Seed.Text, out seed))
            {
                if (Txt_Seed.Text.Length > 0)
                    seed = Txt_Seed.Text.GetHashCode();
                else
                    seed = (new Random()).Next(int.MinValue, int.MaxValue);
            }
            map = Map.Create(new WorldArgs(Txt_MapName.Text, Chk_Flat.Checked, Chk_Caves.Checked, Chk_Trees.Checked, seed, Chk_Flat.Checked ? Terraformer.Flat : Terraformer.Normal));
            //try { map = Map.Create(new MapArgs(Txt_MapName.Text, Chk_Caves.Checked, Chk_Trees.Checked, Convert.ToUInt32(Txt_Seed.Text))); }
            //catch (FormatException)
            //{
            //    Random r = new Random();
            //    map = Map.Create(new MapArgs(Txt_MapName.Text, Chk_Caves.Checked, Chk_Trees.Checked, (uint)r.Next(int.MinValue, int.MaxValue) + int.MaxValue));
            //}

            map.Save();

            Close();
        }

        //void btn_start_Click(object sender, EventArgs e)
        //{
        //    //Player.PlayerName = Txt_MapName.Text;
        //    if (AlreadyExists(Txt_MapName.Text))
        //        return;

        //    Rooms.Ingame ingame = new Rooms.Ingame();
        //    ingame.Initialize();
        //    ScreenManager.Add(ingame);


        //    try { Map.Load(new MapArgs(Txt_MapName.Text, Chk_Caves.Checked, Chk_Trees.Checked, Convert.ToUInt32(Txt_Seed.Text))); }
        //    catch (FormatException)
        //    {
        //        Random r = new Random();
        //        Map.Load(new MapArgs(Txt_MapName.Text, Chk_Caves.Checked, Chk_Trees.Checked, (uint)r.Next(int.MinValue, int.MaxValue) + int.MaxValue));
        //    }

        //    GameObject actor = Objects.GameObjectDb.Actor;
        //    actor["Info"]["Name"] = Txt_CharName.Text;
        //    actor["Position"]["Position"] = new Position(new Vector3(0, 0, Map.MaxHeight - 1));
        //    Player.Instance.Control(actor);

        //    Close();
        //}
    }
}
