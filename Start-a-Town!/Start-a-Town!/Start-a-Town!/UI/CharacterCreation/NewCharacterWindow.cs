using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;

using Start_a_Town_.Components;

namespace Start_a_Town_.UI
{
    class NewCharacterWindow : Window
    {
        Panel Panel_Tabs, Panel_Buttons; //Panel_Main
        PanelLabeled Panel_Name;
        //GroupBox Box_Name;
        TextBox Txt_CharName;
        Button Btn_Create;
        IconButton BtnRandomize;
        GameObject Character;
        CharacterCustomization Customization;

        public NewCharacterWindow()
        {
            Title = "Create new character";
            AutoSize = true;
            Character = new GameObject();
            Character["Info"] = new GeneralComponent(GameObject.Types.Actor, "Name", "Your character.");

            Panel_Tabs = new Panel(Vector2.Zero);
            Panel_Tabs.AutoSize = true;
            InitNameTab();
            this.Client.Controls.Add(this.Panel_Name);

            //this.Panel_Main = new Panel() { Location = this.Client.Controls.BottomLeft, AutoSize = true };
            this.Customization = new CharacterCustomization() { Location = this.Client.Controls.BottomLeft };
            //this.Panel_Main.Controls.Add(this.Customization);
            this.Client.Controls.Add(this.Customization);

            Panel_Buttons = new Panel() { AutoSize = true, Location = this.Customization.BottomLeft };

            Btn_Create = new Button(Vector2.Zero, 100, "Create");
            Btn_Create.CustomTooltip = true;
            Btn_Create.DrawTooltip += new EventHandler<TooltipArgs>(Btn_Create_DrawTooltip);
            Btn_Create.LeftClick += new UIEvent(Btn_Create_Click);
            Panel_Buttons.Controls.Add(Btn_Create);
            //Panel_Buttons.Location = new Vector2(Panel_Main.Right - Panel_Buttons.Width, Math.Max(Panel_Main.Bottom, Panel_Tabs.Bottom));//.X = Panel_Main.Right - Btn_Create.Width;

            

            //Client.Controls.Add(Panel_Main, Panel_Buttons);
            Client.Controls.Add(Panel_Buttons);
            Location = CenterScreen;
        }

        //void tab_Click(object sender, EventArgs e)
        //{
        //    Client.Controls.Remove(Panel_Main);
        //    Panel panel = (sender as Control).Tag as Panel;
        //    if (panel == null)
        //        return;
        //    Client.Controls.Add(panel);
        //    Panel_Main = panel;
        //}

        void Btn_Create_Click(object sender, EventArgs e)
        {
            if (AlreadyExists(Txt_CharName.Text))
                return;

            DirectoryInfo directory = new DirectoryInfo( GlobalVars.SaveDir + "Characters/");
            if (!Directory.Exists(directory.FullName))
                Directory.CreateDirectory(directory.FullName);

            GameObject actor = GameObjectDb.Actor;
            actor["Info"]["Name"] = Txt_CharName.Text;

            //foreach(var item in this.Customization.Colors.Colors)
            //{
            //    actor.Body.Find(item.Key).Sprite.Tint = item.Value;
            //}
            //this.Customization.Colors.Apply(actor.Body);
            actor.GetComponent<SpriteComponent>().Customization = this.Customization.Colors;

            //InventoryComponent.Give(actor, GameObject.Types.Berries, 3);
            //InventoryComponent.Give(actor, GameObject.Types.Pickaxe, 1);
            //InventoryComponent.Give(actor, GameObject.Types.EpicShovel, 1);
            //InventoryComponent.Give(actor, GameObject.Types.Hoe, 1);
            //InventoryComponent.Give(actor, GameObject.Types.Axe, 1);
            //InventoryComponent.Give(actor, GameObject.Types.Hammer, 1);
            //InventoryComponent.Give(actor, GameObject.Types.WoodenPlank, 4);
            //InventoryComponent.Give(actor, GameObject.Types.Handsaw, 1);
            //InventoryComponent.Give(actor, GameObject.Types.StrengthPotion, 1);

            using (MemoryStream stream = new MemoryStream())
            {
                BinaryWriter writer = new BinaryWriter(stream);

                //SaveTag charTag = new SaveTag(Start_a_Town_.SaveTag.Types.Compound, Txt_CharName.Text, actor.Save());
                //charTag.WriteTo(writer);

                Net.Client.SavePlayer(actor, writer);

                Chunk.Compress(stream, @"\Saves\Characters\" + actor["Info"]["Name"] + ".character.sat");

                
               // stream.Close(); // no need for that if using
            }

          //  SelectWorldWindow.Player = actor;
            Tag = actor;
            Hide();
        }

        void Btn_Create_DrawTooltip(object sender, TooltipArgs e)
        {
            if (AlreadyExists(Txt_CharName.Text))
                e.Tooltip.Controls.Add(new Label("Name already exists!"));
        }

        private bool AlreadyExists(string name)
        {
            FileInfo[] files = SelectCharacterWindow.GetCharacters();// directory.GetDirectories("World_*", SearchOption.TopDirectoryOnly);


            foreach (FileInfo filename in files)
                if (filename.Name.Split('.')[0] == name)
                    return true;

            return false;
        }

        private void InitNameTab()
        {
            //Panel_Name = new Panel(new Vector2(Panel_Tabs.Right, 0), new Vector2(200)) { AutoSize = true };
            this.Panel_Name = new PanelLabeled("Name") { AutoSize = true };

            //Label lbl_charName = new Label(new Vector2(0, 0), "Character name") { Location = this.Panel_Name.Controls.BottomLeft };
            //Txt_CharName = new TextBox(new Vector2(0, lbl_charName.Bottom), new Vector2(150, Label.DefaultHeight));
            Txt_CharName = new TextBox(150) { Location = this.Panel_Name.Controls.BottomLeft };
            Txt_CharName.Text = "Reggie Finklebottom";
            if (AlreadyExists(Txt_CharName.Text))
                Txt_CharName.Text = NpcComponent.RandomName();

            //Txt_CharName.TextEntered += new EventHandler<TextEventArgs>(Txt_CharName_TextEntered);
            Txt_CharName.InputFunc = (t, c) =>
            {
                return ((char.IsLetter(c) || char.IsWhiteSpace(c)) ? t + c : t);
            };

            BtnRandomize = new IconButton()
            {
                BackgroundTexture = UIManager.Icon16Background,
                //Location = new Vector2(Txt_CharName.Right, Txt_CharName.Top + (int)((Txt_CharName.Height - UIManager.Icon16Background.Height) / 2f)),
                Location = Panel_Name.Label.TopRight,
                Icon = new Icon(UIManager.Icons16x16, 1, 16),
                HoverText = "Randomize"
            };
            BtnRandomize.LeftClick += new UIEvent(Btn_Randomize_Click);


            //Panel_Name.Controls.Add(lbl_charName, Txt_CharName, Btn_Randomize);
            Panel_Name.Controls.Add(Txt_CharName, BtnRandomize);
        }

        void Btn_Randomize_Click(object sender, EventArgs e)
        {
            Txt_CharName.Text = NpcComponent.RandomName();
            //RandomizeName();
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
    }
}
