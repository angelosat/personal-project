using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class SelectWorldWindow : Window
    {
        Panel Panel_Main, Panel_Buttons, Panel_WorldInfo, Panel_Thumbnail;
        Button Btn_Create, Btn_Start, Btn_Delete;
        DirectoryInfo SelectedWorld;
        ScrollableList<DirectoryInfo> List;
        Scrollbar Scroll;
        static public Map Map;
        static public GameObject Player;


        public SelectWorldWindow()
        {
            Title = "Worlds";
            AutoSize = true;

            Panel_Main = new Panel(Vector2.Zero, new Vector2(200, 300));
            List = new ScrollableList<DirectoryInfo>(Vector2.Zero, Panel_Main.ClientSize, foo => foo.Name);
            List.SelectedItemChanged += new EventHandler<EventArgs>(List_SelectedItemChanged);
            List.ControlAdded += new EventHandler<EventArgs>(List_ControlAdded);
            List.ControlRemoved += new EventHandler<EventArgs>(List_ControlRemoved);
            Scroll = new Scrollbar(new Vector2(Panel_Main.ClientSize.Width - Scrollbar.Width, 0), Panel_Main.ClientSize.Height, List);
            //Scroll.Tag = List;

            Panel_Buttons = new Panel(new Vector2(0, Panel_Main.Bottom), new Vector2(200, 300));
            Panel_Buttons.AutoSize = true;


            Btn_Create = new Button(Vector2.Zero, Panel_Buttons.ClientSize.Width, "Create New World");
            
           // Btn_Start
            RefreshWorldList();

            
            Panel_Buttons.Controls.Add(Btn_Create);//, Btn_Start);

            int thumbSize = (int)((Engine.ChunkRadius + Engine.ChunkRadius + 1) * Chunk.Size);
            Panel_Thumbnail = new Panel(new Vector2(Panel_Main.Right, 0), new Vector2(thumbSize));
            Panel_Thumbnail.ClientSize = new Rectangle(0, 0, thumbSize, thumbSize);
            Controls.Add(Panel_Thumbnail, Panel_Main, Panel_Buttons);
            Panel_WorldInfo = new Panel(new Vector2(Panel_Main.Right, Panel_Thumbnail.Bottom), new Vector2(Panel_Thumbnail.Width, ClientSize.Height - Panel_Thumbnail.Height));//, new Vector2(Panel_Thumbnail.Width, ClientSize.Height - Panel_Thumbnail.Height));// Panel_Main.Height / 2));
            
            
            
            //InitWorldInfo();

            
                Controls.Add(Panel_WorldInfo);
            //Panel_WorldInfo.Size = new Rectangle(0,0,Panel_Thumbnail.Width, ClientSize.Height - Panel_Thumbnail.Height);
            
            Btn_Delete = new Button(Vector2.Zero, Panel_WorldInfo.ClientSize.Width, "Delete World");
            Btn_Start = new Button(Vector2.Zero, Panel_WorldInfo.ClientSize.Width, "Enter world");//Start-a-Town!");
            
            Location = CenterScreen;

            Btn_Delete.Click += new UIEvent(Btn_Delete_Click);
            Btn_Create.Click += new UIEvent(Btn_Create_Click);
            Btn_Start.Click += new UIEvent(Btn_Start_Click);
        }

        void List_ControlRemoved(object sender, EventArgs e)
        {
            if (List.ClientSize.Height <= Panel_Main.ClientSize.Height)
            {
                List.Size = new Rectangle(0, 0, Panel_Main.ClientSize.Width, List.Size.Height);
                Panel_Main.Controls.Remove(Scroll);
                foreach (ButtonBase btn in List.Controls)
                    btn.Width = Panel_Main.ClientSize.Width;
            }
        }

        void List_ControlAdded(object sender, EventArgs e)
        {
            if (List.ClientSize.Height > Panel_Main.ClientSize.Height)
            {
                List.Size = new Rectangle(0, 0, Panel_Main.ClientSize.Width - 16, List.Size.Height);
                Panel_Main.Controls.Add(Scroll);
                foreach (ButtonBase btn in List.Controls)
                    btn.Width = Panel_Main.ClientSize.Width - Scrollbar.Width;
            }
        }

        void List_SelectedItemChanged(object sender, EventArgs e)
        {
            DirectoryInfo worldDirectory = List.SelectedItem;
            SelectedWorld = worldDirectory;
            RefreshWorldInfo(worldDirectory);
        }

        private void RefreshWorldInfo(DirectoryInfo worldDirectory = null)
        {
            Panel_WorldInfo.Controls.Clear();
            Panel_Thumbnail.Controls.Clear();
            if (worldDirectory == null)
                return;
            FileInfo[] worldFiles = worldDirectory.GetFiles("*.map", SearchOption.TopDirectoryOnly);
            
            Panel_Thumbnail.Controls.Clear();
            if (worldFiles.Length != 1)
            {
                Panel_WorldInfo.Controls.Add(new Label("Error Loading Map"));
                return;
            }

            FileInfo worldSave = worldFiles.First();

            try
            {
                FileInfo[] thumbFiles = worldDirectory.GetFiles("thumbnailSmallest.png");
                //FileInfo[] thumbFiles = worldDirectory.GetFiles("thumbnailSmaller.png");
                Texture2D thumbnail;
                if (thumbFiles.Length > 0)
                {
                    string withoutExt = thumbFiles.First().Name.Split('.')[0];
                    using (FileStream stream = new FileStream(thumbFiles.First().FullName, FileMode.Open))
                    {
                        thumbnail = Texture2D.FromStream(Game1.Instance.GraphicsDevice, stream);
                        PictureBox box_thumb = new PictureBox(Vector2.Zero, thumbnail, null, TextAlignment.Left);
                        Panel_Thumbnail.Controls.Add(box_thumb);
                    }
                }
            }
            catch (Exception) { Panel_Thumbnail.Controls.Add(new Label("Error Loading Thumbnail")); }

            try
            {
                Map = Map.Load(worldSave.FullName);
                
                //PictureBox box_thumb = new PictureBox(Vector2.Zero, Map.GetThumbnail(), null, TextAlignment.Left);

                Btn_Start.Text = Map["Player"] != null ? "Enter World" : "Select character";
                Btn_Delete.Location = new Vector2(0, Panel_WorldInfo.ClientSize.Height - Btn_Delete.Height);
                Btn_Start.Location = new Vector2(0, Btn_Delete.Top - Btn_Start.Height);
                Panel_WorldInfo.Controls.Add(new Label(Vector2.Zero, Map.ToString()), Btn_Start, Btn_Delete);

                
            }
            catch (Exception e)
            {
                Panel_WorldInfo.Controls.Add(new Label("Error Loading Map!\n" + e.ToString()));
                Btn_Delete.Location = new Vector2(0, Panel_WorldInfo.ClientSize.Height - Btn_Delete.Height);
                Panel_WorldInfo.Controls.Add(Btn_Delete);
            }

         //   if (Map == null)
         //       return;
            
        }

        void Btn_Delete_Click(object sender, EventArgs e)
        {
            if (SelectedWorld == null)
                return;
            //Console.WriteLine(Directory.GetFiles(SelectedWorld.FullName));

            MessageBox delete = new MessageBox("Warning!", "Are you sure you want to delete \"" + SelectedWorld.Name + "\" ?");
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
            Directory.Delete(SelectedWorld.FullName, true);
            SelectedWorld = null;
            RefreshWorldInfo();
            RefreshWorldList();
        }

        void Btn_Start_Click(object sender, EventArgs e)
        {
            if (SelectedWorld == null)
                return;


            GameObject mapPlayer;
            Map.TryGetProperty<GameObject>("Player", out mapPlayer);
            if (mapPlayer == null)
            {
                if (Player == null)
                {
                    //(new NewCharacterWindow()).ShowDialog();
                    SelectCharacterWindow select = new SelectCharacterWindow();
                    select.ShowDialog();
                   // select.Previous = this;
                    Close();
                    return;
                }
                else
                    Map["Player"] = Player;
            }

            Map.Instance = Map;
            
            //Map.Focus(Vector3.Zero);
            ChunkLoader.Reset();
            ChunkLoader.Map = Map;
            
            Map.Focus(Map.GetProperty<GameObject>("Player")["Position"].GetProperty<Position>("Position").Global);
            Map = null;
            SelectedWorld = null;
            Player = null;
            RefreshWorldInfo(null);
            Rooms.Ingame ingame = new Rooms.Ingame();
            ingame.Initialize();
            
            Start_a_Town_.Player.Instance.Control(mapPlayer != null ? mapPlayer : Player);
            Close();

            ScreenManager.Add(ingame);
            //Map map;
            //try { map = Map.Load(new MapArgs(Txt_MapName.Text, Chk_Caves.Checked, Chk_Trees.Checked, Convert.ToUInt32(Txt_Seed.Text))); }
            //catch (FormatException)
            //{
            //    Random r = new Random();
            //    map = Map.Create(new MapArgs(Txt_MapName.Text, Chk_Caves.Checked, Chk_Trees.Checked, (uint)r.Next(int.MinValue, int.MaxValue) + int.MaxValue));
            //}
        }

        private void RefreshWorldList()
        {
            foreach (Control control in Panel_Main.Controls)
            {
                control.DrawTooltip -= world_DrawTooltip;
                control.DrawItem -= world_DrawItem;
                //control.Click -= world_Click;
            }
            Panel_Main.Controls.Clear();

            DirectoryInfo[] worlds = GetWorlds();
            List.Build(worlds);
            Panel_Main.Controls.Add(List);
            //int n = 0;
            //foreach (DirectoryInfo file in worlds)
            //{
            //    Label world = new Label(new Vector2(0, (n++) * Label.DefaultHeight), file.Name);
            //    world.Active = true;
            //    world.Tag = file;
            //    world.CustomTooltip = true;
            //    world.DrawTooltip += new EventHandler<TooltipArgs>(world_DrawTooltip);
            //    world.DrawMode = UI.DrawMode.OwnerDrawFixed;
            //    world.DrawItem += new EventHandler<DrawItemEventArgs>(world_DrawItem);
            //    world.Click += new UIEvent(world_Click);
            //    Panel_Main.Controls.Add(world);
            //}
        }

        //void world_Click(object sender, EventArgs e)
        //{
        //    Label label = sender as Label;
        //    DirectoryInfo worldDirectory = label.Tag as DirectoryInfo;
        //    SelectedWorld = worldDirectory;

            
        //  //  if(worldFiles.Length != 1)
        //  //      throw(new Exception("Error loading map"));

        //    RefreshWorldInfo(worldDirectory);
        //    //Console.WriteLine(map.ToString());
        //}

        void world_DrawItem(object sender, DrawItemEventArgs e)
        {
            Label label = sender as Label;
            DirectoryInfo world = label.Tag as DirectoryInfo;
            if (SelectedWorld != null)
                if (world.Name == SelectedWorld.Name)
                    label.DrawHighlight(e.SpriteBatch, 0.5f);
                    //e.SpriteBatch.Draw(label.TextSprite, label.ScreenLocation, null, Color.Lerp(Color.Transparent, Color.White, label.Opacity), 0, label.Origin, 1, SpriteEffects.None, label.Depth);
        }

        void world_DrawTooltip(object sender, TooltipArgs e)
        {
            DirectoryInfo file = (sender as Label).Tag as DirectoryInfo;
            e.Tooltip.Controls.Add(new Label(file.Name));
        }

        void Btn_Create_Click(object sender, EventArgs e)
        {
            NewWorldWindow win = new NewWorldWindow();
            win.ShowDialog();
            win.Closed += new EventHandler<EventArgs>(createWorld_Closed);
        }

        void createWorld_Closed(object sender, EventArgs e)
        {
            //Console.WriteLine((sender as SelectCharacterWindow).Tag as GameObject);
            Console.WriteLine((sender as NewWorldWindow).Tag as GameObject);
            RefreshWorldList();
        }


        static public DirectoryInfo[] GetWorlds()
        {
            DirectoryInfo directory = new DirectoryInfo(GlobalVars.SaveDir + "/Worlds/");
            if (!Directory.Exists(directory.FullName))
                Directory.CreateDirectory(directory.FullName);
            return directory.GetDirectories();
        }
    }
}
