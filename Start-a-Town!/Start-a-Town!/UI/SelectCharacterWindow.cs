using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml;
using System.Xml.Linq;
using Start_a_Town_.Components;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.UI
{
    class SelectCharacterWindow : Window
    {
        #region Singleton
        static SelectCharacterWindow _Instance;
        public static SelectCharacterWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new SelectCharacterWindow();
                return _Instance;
            }
        }
        #endregion

        Panel Panel_Buttons, Panel_Info, Panel_List;
        ListBox<FileInfo, Button> Box_List;
        Button Btn_Create, Btn_Start, Btn_Delete, Btn_Folder, Btn_Refresh;

        //public GameObject Character;
        FileInfo SelectedFile;
        public IMap Map;

        public SelectCharacterWindow()
        {
            Title = "Characters";
            AutoSize = true;

            Panel_List = new Panel() { Dimensions = new Vector2(200, 300), AutoSize = true };

            Box_List = new ListBox<FileInfo, Button>(Panel_List.ClientSize);//, foo => foo.Name.Split('.')[0]);
            Box_List.SelectedItemChanged += new EventHandler<EventArgs>(Panel_Main_SelectedItemChanged);


            Panel_Buttons = new Panel() { Location = Panel_List.BottomLeft, AutoSize = true };// + Panel_Info.Width, 0));
            Panel_Buttons.AutoSize = true;
            Btn_Create = new Button(Vector2.Zero, Box_List.Width, "Create New Character");
            Btn_Folder = new Button(new Vector2(0, Btn_Create.Bottom), Btn_Create.Width, "Open Characters Folder");
            Btn_Refresh = new Button(Btn_Folder.BottomLeft, Btn_Create.Width, "Refresh List") { LeftClickAction = RefreshCharacterList };
            //Btn_Start = new Button(new Vector2(Btn_Create.Right, 0), Panel_Buttons.ClientSize.Width/2, "Start-a-Town!");
            Btn_Start = new Button(new Vector2(Btn_Create.Right, 0), Btn_Create.Width, "Start-a-Town!");
            Btn_Delete = new Button(new Vector2(0, Btn_Start.Bottom), Btn_Create.Width, "Delete Character");
            Panel_Buttons.Controls.Add(Btn_Create, Btn_Folder, Btn_Refresh);//, Btn_Start);
            Panel_Info = new Panel(Panel_List.TopRight, new Vector2(200, Panel_List.Height + Panel_Buttons.Height));//300));
            
            Panel_List.Controls.Add(Box_List);

            Btn_Create.LeftClick += new UIEvent(Btn_Create_Click);
            Btn_Start.LeftClick += new UIEvent(Btn_Start_Click);
            Btn_Delete.LeftClick += new UIEvent(Btn_Delete_Click);
            Btn_Folder.LeftClick += new UIEvent(Btn_Folder_Click);
            
            Client.Controls.Add(Panel_Info, Panel_List, Panel_Buttons);
            this.SnapToScreenCenter();
            
            RefreshCharacterList();
        }

        void Btn_Folder_Click(object sender, EventArgs e)
        {
            System.Diagnostics.Process.Start(GlobalVars.SaveDir + "/Characters/");
        }

        void Btn_Delete_Click(object sender, EventArgs e)
        {
            if (SelectedFile == null)
                return;

            MessageBox delete = new MessageBox("Warning!", "Are you sure you want to delete \"" + SelectedFile.Name + "\" ?");
            delete.ShowDialog();
            delete.Yes += new EventHandler<EventArgs>(delete_Yes);
        }

        void delete_Yes(object sender, EventArgs e)
        {
            (sender as MessageBox).Yes -= delete_Yes;
            DeleteCharacter();
            (sender as MessageBox).Close();
        }

        void DeleteCharacter()
        {
            File.Delete(SelectedFile.FullName);
            SelectedFile = null;
            RefreshCharacterInfo();
            RefreshCharacterList();
        }

        void Panel_Main_SelectedItemChanged(object sender, EventArgs e)
        {
            FileInfo charFile = Box_List.SelectedItem;
            SelectedFile = charFile;
            RefreshCharacterInfo(charFile);
        }
        static public GameObject LoadCharacter(FileInfo characterFile)
        {
            string filename = characterFile.FullName;
            using (FileStream stream = new FileStream(filename, System.IO.FileMode.Open))
            {
                using (MemoryStream decompressedStream = Chunk.Decompress(stream))
                {
                    BinaryReader reader = new BinaryReader(decompressedStream); //stream);//

                    //SaveTag charTag = Start_a_Town_.SaveTag.Read(reader);
                    //SaveTag hotbarTag = Start_a_Town_.SaveTag.Read(reader);

                    SaveTag tag = Start_a_Town_.SaveTag.Read(reader);
                    SaveTag charTag;
                    if (!tag.TryGetTag("PlayerCharacter", out charTag))
                        return null;

                    GameObject ch = PlayerEntity.Create(charTag);

                    tag.TryGetTag("HotBar", t => PlayerOld.Instance.HotBar.Load(t, ch));

                    // no need to replace object's name with savetag name, the name is saved with the generalcomponent
                    //  ch.Name = charTag.Name;
                    ch.Name = characterFile.Name.Replace(".character.sat", "");
                    return ch;
                }
            }
        }
        static public GameObject LoadCharacter(string name)
        {
            DirectoryInfo directory = new DirectoryInfo(GlobalVars.SaveDir + "Characters/");
            FileInfo charFile = GetCharacters().FirstOrDefault(file => file.Name.Split('.')[0] == name);
            if (charFile is null)
                return null; //TODO: throw error
            return LoadCharacter(charFile);
        }
        private void RefreshCharacterInfo(FileInfo characterFile = null)
        {
            if (characterFile is null)
                return;
            Panel_Info.Controls.Clear();
            //if (characterFile == null)
            //{
            //    InvWindow.Instance.Hide();
            //    return;
            //}
         //   try
        //    {
                string filename = characterFile.FullName;
                using (FileStream stream = new FileStream(filename, System.IO.FileMode.Open))
                {
                    using (MemoryStream decompressedStream = Chunk.Decompress(stream))
                    {
                        BinaryReader reader = new BinaryReader(decompressedStream); //stream);//
                        SaveTag charTag = Start_a_Town_.SaveTag.Read(reader);

                        try
                       {

                            GameObject character = GameObject.Load(charTag);//(charTag.Value as List<Tag>)[0]);
                            character.Name = charTag.Name; //(string)charTag["Name"].Value;

                            Tag = character;
                            Btn_Delete.Location = new Vector2(0, Panel_Info.ClientSize.Height - Btn_Start.Height);
                            Btn_Start.Location = new Vector2(0, Btn_Delete.Top - Btn_Start.Height);
                            //Btn_Start.Location = new Vector2(0, Panel_Info.ClientSize.Height - Btn_Start.Height);
                            Panel_Info.Controls.Add(new Label((Tag as GameObject)["Info"].ToString()), Btn_Start, Btn_Delete);

                           // InvWindow.Show(character);
                       }
                        catch (Exception e)
                        {
                            Panel_Info.Controls.Add(new Label("Error Loading Character!\n" + e.ToString()));
                            Btn_Delete.Location = new Vector2(0, Panel_Info.ClientSize.Height - Btn_Delete.Height);
                            Panel_Info.Controls.Add(Btn_Delete);
                        }

                    }
                }
           // }
           // catch (Exception) { Panel_Info.Controls.Add(new Label("Error loading character")); }
        }
        public override bool Close()
        {
            SelectedFile = null;
            return base.Close();
        }

        private void RefreshCharacterList()
        {
            foreach (Control control in Box_List.Controls)
                control.DrawTooltip -= world_DrawTooltip;
            //Box_List.Controls.Clear();

            FileInfo[] characters = GetCharacters();
            this.Box_List.Build(characters, foo => foo.Name.Split('.')[0]);
        }

        void character_DrawItem(object sender, DrawItemEventArgs e)
        {
            Label label = sender as Label;
            FileInfo characterFile = label.Tag as FileInfo;
            if (SelectedFile != null)
                //if ((Tag as GameObject).Name == characterFile.Name.Split('.')[0])
                if (SelectedFile.Name == characterFile.Name)
                    label.DrawHighlight(e.SpriteBatch, 0.5f);
            //e.SpriteBatch.Draw(label.TextSprite, label.ScreenLocation, null, Color.Lerp(Color.Transparent, Color.White, label.Opacity), 0, label.Origin, 1, SpriteEffects.None, label.Depth);
        }

        //void character_Click(object sender, EventArgs e)
        //{
        //    Label label = sender as Label;
        //    FileInfo charFile = label.Tag as FileInfo;
        //    SelectedFile = charFile;
        //    RefreshCharacterInfo(charFile);
        //}

        void world_DrawTooltip(object sender, TooltipArgs e)
        {
            FileInfo file = (sender as Label).Tag as FileInfo;
            e.Tooltip.Controls.Add(new Label(file.Name));
        }

        void Btn_Create_Click(object sender, EventArgs e)
        {
            NewCharacterWindow win = new NewCharacterWindow();
            win.ShowDialog();
            win.HideAction = RefreshCharacterList;
            //win.Hidden += new EventHandler<EventArgs>(createCharacter_Closed);
        }

        void createCharacter_Closed(object sender, EventArgs e)
        {
          //  (sender as Window).Hidden -= createCharacter_Closed;
            RefreshCharacterList();
        }

        static public string GetLastCharName()
        {
            XDocument xml = Engine.Settings.ToXDocument();
            //string lastChar = xml.Element("Profile").Element("LastCharacter").Value;
            return (from node in xml.Descendants() where node.Name == "LastCharacter" select node.Value).FirstOrDefault() ?? "";
        }
        static public GameObject GetLastChar()
        {
            return LoadCharacter(GetLastCharName());
        }
        static public FileInfo[] GetCharacters()
        {
            DirectoryInfo directory = new DirectoryInfo(GlobalVars.SaveDir + "Characters/");
            if (!Directory.Exists(directory.FullName))
                Directory.CreateDirectory(directory.FullName);
            return directory.GetFiles("*.character.sat");
        }

        void Btn_Start_Click(object sender, EventArgs e)
        {
          //  Map map = Tag as Map;
            if (Map == null)
                return;


            GameObject mapPlayer = this.Map.PlayerCharacter;
            GameObject player = Tag as GameObject;
            //Map.TryGetProperty<GameObject>("Player", out mapPlayer);
            if (mapPlayer == null)
            {
                if (player == null)
                {
                    throw (new Exception("Player is null!"));
                }
                else
                    Map.PlayerCharacter = player;
            }
            //InvWindow.Instance.Hide();
            Engine.PlayGame(player);//, Map);
        //    Engine.Map = Map;
        ////    ChunkLoader.Reset();
        //    ChunkLoader.Restart();
        //    //Map.Focus(Vector3.Zero);
        //    ChunkLoader.ForceLoad(Map);
        //    Close();
        //    Rooms.Ingame ingame = new Rooms.Ingame();
        //    ingame.Initialize();
        //    ScreenManager.Add(ingame);
        //    Start_a_Town_.Player.Instance.Control(Engine.Map, mapPlayer != null ? mapPlayer : player);

        }

        public override bool Show()
        {
            this.Tag = null;
            RefreshCharacterList();
            RefreshCharacterInfo();
            return base.Show();
        }
    }
}
