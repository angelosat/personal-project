using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

using Start_a_Town_.Components;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class SelectCharacterWindow : Window
    {
        Panel Panel_Main, Panel_Buttons, Panel_Info;
        Button Btn_Create, Btn_Start;
        Scrollbar Scroll;
        ScrollableList<FileInfo> List;
        //public GameObject Character;
        FileInfo SelectedFile;
        public SelectCharacterWindow()
        {
            Title = "Characters";
            AutoSize = true;

            Panel_Main = new Panel(Vector2.Zero, new Vector2(200, 300));
            List = new ScrollableList<FileInfo>(Vector2.Zero, Panel_Main.ClientSize, foo => foo.Name);
            List.SelectedItemChanged += new EventHandler<EventArgs>(List_SelectedItemChanged);
            Panel_Info = new Panel(new Vector2(Panel_Main.Right, 0), new Vector2(200, 300));

            Panel_Buttons = new Panel(new Vector2(0, Panel_Main.Bottom), new Vector2(Panel_Main.Width + Panel_Info.Width, 0));
            Panel_Buttons.AutoSize = true;
            Btn_Create = new Button(Vector2.Zero, Panel_Buttons.ClientSize.Width, "Create New Character");
            //Btn_Start = new Button(new Vector2(Btn_Create.Right, 0), Panel_Buttons.ClientSize.Width/2, "Start-a-Town!");
            Btn_Start = new Button(new Vector2(Btn_Create.Right, 0), Panel_Info.ClientSize.Width, "Start-a-Town!");
            RefreshCharacterList();

            Btn_Create.Click += new UIEvent(Btn_Create_Click);
            Btn_Start.Click +=new UIEvent(Btn_Start_Click);
            Panel_Buttons.Controls.Add(Btn_Create);//, Btn_Start);
            Controls.Add(Panel_Info, Panel_Main, Panel_Buttons);
            Location = CenterScreen;
        }

        void List_SelectedItemChanged(object sender, EventArgs e)
        {
            FileInfo charFile = List.SelectedItem;
            SelectedFile = charFile;
            RefreshCharacterInfo(charFile);
        }

        private void RefreshCharacterInfo(FileInfo characterFile)
        {
            Panel_Info.Controls.Clear();
            if (characterFile == null)
                return;

         //   try
        //    {
                string filename = characterFile.FullName;
                using (FileStream stream = new FileStream(filename, System.IO.FileMode.Open))
                {
                    using (MemoryStream decompressedStream = Chunk.Decompress(stream))
                    {
                        BinaryReader reader = new BinaryReader(decompressedStream); //stream);//
                        Tag charTag = Start_a_Town_.Tag.Read(reader);

                        GameObject character = GameObject.Create(charTag);//(charTag.Value as List<Tag>)[0]);
                        character.Name = charTag.Name; //(string)charTag["Name"].Value;

                        Tag = character;
                        Btn_Start.Location = new Vector2(0, Panel_Info.ClientSize.Height - Btn_Start.Height);
                        Panel_Info.Controls.Add(new Label((Tag as GameObject)["Info"].ToString()), Btn_Start);

                        //InvWindow inv;
                        //inv = WindowManager.ToggleSingletonWindow<InvWindow>() as InvWindow;
                        //try
                        //{
                        //    if (inv != null)
                        //    {
                        //        inv.Initialize(character);
                        //    }
                        //}
                        //catch (Exception) { inv.Controls.Add(new Label("Error loading window")); }
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
            foreach (Control control in Panel_Main.Controls)
                control.DrawTooltip -= world_DrawTooltip;
            Panel_Main.Controls.Clear();

            FileInfo[] characters = GetCharacters();
            List.Build(characters);
            Panel_Main.Controls.Add(List);
            //int n = 0;
            //foreach (FileInfo file in characters)
            //{
            //    Label character = new Label(new Vector2(0, (n++) * Label.DefaultHeight), file.Name.Split('.')[0]);
            //    character.Tag = file;
            //    character.CustomTooltip = true;
            //    character.DrawTooltip += new EventHandler<TooltipArgs>(world_DrawTooltip);
            //    character.DrawItem += new EventHandler<DrawItemEventArgs>(character_DrawItem);
            //    character.Click += new UIEvent(character_Click);
            //    Panel_Main.Controls.Add(character);
            //}
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
            win.Closed += new EventHandler<EventArgs>(createCharacter_Closed);
        }

        void createCharacter_Closed(object sender, EventArgs e)
        {
            RefreshCharacterList();
        }


        static public FileInfo[] GetCharacters()
        {
            DirectoryInfo directory = new DirectoryInfo(GlobalVars.SaveDir + "Characters/");
            if (!Directory.Exists(directory.FullName))
                Directory.CreateDirectory(directory.FullName);
            return directory.GetFiles("*.character");
        }

        void Btn_Start_Click(object sender, EventArgs e)
        {
            if (SelectWorldWindow.Map == null)
                return;


            GameObject mapPlayer;
            GameObject Player = Tag as GameObject;
            Map Map = SelectWorldWindow.Map;
            Map.TryGetProperty<GameObject>("Player", out mapPlayer);
            if (mapPlayer == null)
            {
                if (Player == null)
                {
                    //(new NewCharacterWindow()).ShowDialog();
                    (new SelectCharacterWindow()).ShowDialog();
                    return;
                }
                else
                    Map["Player"] = Player;
            }

            Map.Instance = Map;
            ChunkLoader.Reset();
            ChunkLoader.Map = Map;
            Map.Focus(Vector3.Zero);
            Close();
            Rooms.Ingame ingame = new Rooms.Ingame();
            ingame.Initialize();
            ScreenManager.Add(ingame);
            Start_a_Town_.Player.Instance.Control(mapPlayer != null ? mapPlayer : Player);

        }

        void List_ControlRemoved(object sender, EventArgs e)
        {
            if (List.ClientSize.Height <= ClientSize.Height)
            {
                List.Size = new Rectangle(0, 0, Panel_Main.ClientSize.Width, List.Size.Height);
                Panel_Main.Controls.Remove(Scroll);
                foreach (ButtonBase btn in List.Controls)
                    btn.Width = Panel_Main.ClientSize.Width;
            }
        }

        void List_ControlAdded(object sender, EventArgs e)
        {
            if (List.ClientSize.Height > ClientSize.Height)
            {
                List.Size = new Rectangle(0, 0, Panel_Main.ClientSize.Width - 16, List.Size.Height);
                Panel_Main.Controls.Add(Scroll);
                foreach (ButtonBase btn in List.Controls)
                    btn.Width = Panel_Main.ClientSize.Width - Scrollbar.Width;
            }
        }
    }
}
