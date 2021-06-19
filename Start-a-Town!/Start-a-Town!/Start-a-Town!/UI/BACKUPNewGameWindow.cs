using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class NewGameWindow : Window
    {
        TextBox Txt_name, Txt_Seed;
        CheckBox Chk_Caves, Chk_Trees;
        ComboBox Cb_mapsize;
        public NewGameWindow()
        {
            Title = "New Game";
            AutoSize = true;
            Closable = true;
            Movable = false;

            Panel panel_game = new Panel();
            panel_game.AutoSize = true;
            Label label_mapsize = new Label("Map size");

            Cb_mapsize = new ComboBox(new Vector2(0, label_mapsize.Bottom), 100);

            foreach (Map.MapSize mapsize in Map.Sizes)
                Cb_mapsize.Items.Add(mapsize);

            //Cb_mapsize.Items.Add("Small");
            //Cb_mapsize.Items.Add("Large");
            //Cb_mapsize.Items.Add("Huge");
            
            Cb_mapsize.DisplayMember = "Name";
            Cb_mapsize.SelectedIndex = 2;
            //Cb_mapsize.SelectedValue = "Medium";
            

            //Panel p_name = new Panel(new Vector2(0, panel_game.Bottom));
            //p_name.AutoSize = true;
            Label lbl_name = new Label(new Vector2(0, 0), "Name");
            Txt_name = new TextBox(new Vector2(0, lbl_name.Bottom), new Vector2(150, Label.DefaultHeight));
            //Txt_name.MouseLeftPress += new EventHandler<InputState>(Txt_MouseLeftPress);
            Label lbl_seed = new Label(new Vector2(0, Txt_name.Bottom), "Seed (0 - 4294967295)");
            Txt_Seed = new TextBox(new Vector2(0, lbl_seed.Bottom), new Vector2(150, Label.DefaultHeight));
            //Txt_Seed.KeyPress += new EventHandler<KeyPressEventArgs>(Txt_Seed_KeyPress);
            //Txt_Seed.MouseLeftPress += new EventHandler<InputState>(Txt_MouseLeftPress);
            Txt_Seed.TextEntered += new EventHandler<TextEventArgs>(Txt_Seed_TextEntered);
            Txt_Seed.Text = "";
            IconButton btn_random = new IconButton(new Vector2(Txt_Seed.Right, Txt_Seed.Top + (int)((Txt_Seed.Height - IconButton.DefaultHeight)/2f)), 1);
            btn_random.HoverText = "Randomize";
            btn_random.Click += new UIEvent(btn_random_Click);
            //txt_name.Text = "Bob";
            //p_name.Controls.AddRange(new Control[] { lbl_name, txt_name });

            Chk_Caves = new CheckBox("Caves", new Vector2(Txt_Seed.Left, Txt_Seed.Bottom));
            Chk_Caves.Checked = false;
            Chk_Caves.Click += new UIEvent(Chk_Caves_Click);
            Chk_Trees = new CheckBox("Trees", new Vector2(Chk_Caves.Left, Chk_Caves.Bottom));
            Chk_Trees.Checked = true;
            Chk_Trees.Click += new UIEvent(Chk_Trees_Click);

            panel_game.Controls.AddRange(new Control[] { lbl_name, Txt_name, lbl_seed, Txt_Seed, btn_random, Chk_Caves, Chk_Trees });
            Panel panel_button = new Panel(new Vector2(0, panel_game.Bottom));
            panel_button.AutoSize = true;
            Button btn_start = new Button(Vector2.Zero, 100, "Start-a-Town!");
            btn_start.Click += new UIEvent(btn_start_Click); 
            panel_button.Controls.Add(btn_start);
            
            panel_game.AutoSize = false;
            Controls.AddRange(new Control[] { panel_button, panel_game });
            Location = CenterScreen;
        }

        void Chk_Trees_Click(object sender, EventArgs e)
        {
            Chk_Trees.Checked = !Chk_Trees.Checked;
        }

        void Chk_Caves_Click(object sender, EventArgs e)
        {
            Chk_Caves.Checked = !Chk_Caves.Checked;
        }

        //void Txt_MouseLeftPress(object sender, EventArgs e)
        //{
        //    TextBox box = sender as TextBox;
        //    box.Select();
        //}

        void Txt_Seed_TextEntered(object sender, TextEventArgs e)
        {
            char letter = e.Char;
            //Console.WriteLine(letter + " " + (int)key);
            int n = (int)Char.GetNumericValue(letter);
            string newtext = Txt_Seed.Text + n;
            if (n >= 0 && n < 10)
            {
                //uint newvalue = UInt32.Parse(Txt_Seed.Text); // Convert.ToUInt32(Txt_Seed.Text);
                //if(value + n <= uint.MaxValue)
                //    Txt_Seed.Text += Char.GetNumericValue(letter);
                if (Txt_Seed.Text.Length == 0)
                    Txt_Seed.Text += n;
                else
                {
                    try
                    {
                        uint newvalue = UInt32.Parse(newtext);
                        Txt_Seed.Text = newtext;
                    }
                    catch (OverflowException) { Txt_Seed.Text = (uint.MaxValue).ToString(); }


                }
            }
        }

        void btn_random_Click(object sender, EventArgs e)
        {
            Random r = new Random();
            Txt_Seed.Text = ((uint)r.Next(int.MinValue, int.MaxValue) + int.MaxValue).ToString();
        }

        void Txt_Seed_KeyPress(object sender, KeyPressEventArgs e)
        {
            Microsoft.Xna.Framework.Input.Keys key = e.Key;
            char letter = (char)((int)key);
            //Console.WriteLine(letter + " " + (int)key);
            int n = (int)Char.GetNumericValue(letter);
            string newtext = Txt_Seed.Text + n;
            if (n >= 0 && n < 10)
            {
                //uint newvalue = UInt32.Parse(Txt_Seed.Text); // Convert.ToUInt32(Txt_Seed.Text);
                //if(value + n <= uint.MaxValue)
                //    Txt_Seed.Text += Char.GetNumericValue(letter);
                if (Txt_Seed.Text.Length == 0)
                    Txt_Seed.Text += n;
                else
                {
                    try
                    {
                        uint newvalue = UInt32.Parse(newtext);
                        Txt_Seed.Text = newtext;
                    }
                    catch (OverflowException) { Txt_Seed.Text = (uint.MaxValue).ToString(); }

                        
                }
            }
        }

        void btn_start_Click(object sender, EventArgs e)
        {
            Player.PlayerName = Txt_name.Text;
            
            Rooms.Ingame ingame = new Rooms.Ingame();
            ingame.Initialize();
            ScreenManager.Add(ingame);


            try { Map.Load(new MapArgs(Chk_Caves.Checked, Chk_Trees.Checked, Convert.ToUInt32(Txt_Seed.Text))); }
            catch (FormatException)
            {
                Random r = new Random();
                Map.Load(new MapArgs(Chk_Caves.Checked, Chk_Trees.Checked, (uint)r.Next(int.MinValue, int.MaxValue) + int.MaxValue));
            }


            GameObject actor = Objects.GameObjectDb.Actor;
            actor["Position"]["Position"] = new Position(new Vector3(0, 0, Map.MaxHeight - 1));
            Player.Instance.Control(actor);

            Close();
        }
    }
}
