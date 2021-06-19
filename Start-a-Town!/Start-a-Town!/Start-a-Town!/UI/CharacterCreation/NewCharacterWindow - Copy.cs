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
        Panel Panel_Main, Panel_Name, Panel_Tabs, Panel_Buttons;
        TextBox Txt_CharName;
        Button Btn_Create;
        IconButton Btn_Randomize;
        GameObject Character;
        CharacterCustomize Customization;

        public NewCharacterWindow()
        {
            Title = "Create new character";
            AutoSize = true;
            Character = new GameObject();
            Character["Info"] = new GeneralComponent(GameObject.Types.Actor, "Name", "Your character.");

            Panel_Tabs = new Panel(Vector2.Zero);
            Panel_Tabs.AutoSize = true;
            RadioButton rad_name = new RadioButton("Name", Vector2.Zero);
            rad_name.Checked = true;
            rad_name.LeftClick += new UIEvent(tab_Click);
            Panel_Tabs.Controls.Add(rad_name);
            InitNameTab();

            

            rad_name.Tag = Panel_Name;
            Panel_Main = Panel_Name;
           // rad_name.PerformClick();
            

            //Panel_Buttons = new Panel(new Vector2(0, Math.Max(Panel_Main.Bottom, Panel_Tabs.Bottom)));
            Panel_Buttons = new Panel(new Vector2(Panel_Main.Right, Math.Max(Panel_Main.Bottom, Panel_Tabs.Bottom)));
            Panel_Buttons.AutoSize = true;
            Btn_Create = new Button(Vector2.Zero, 100, "Create");
            Btn_Create.CustomTooltip = true;
            Btn_Create.DrawTooltip += new EventHandler<TooltipArgs>(Btn_Create_DrawTooltip);
            Btn_Create.LeftClick += new UIEvent(Btn_Create_Click);
            Panel_Buttons.Controls.Add(Btn_Create);
            Panel_Buttons.Location = new Vector2(Panel_Main.Right - Panel_Buttons.Width, Math.Max(Panel_Main.Bottom, Panel_Tabs.Bottom));//.X = Panel_Main.Right - Btn_Create.Width;

            this.Customization = new CharacterCustomize() { AutoSize = true,  Location = Txt_CharName.BottomLeft };
            this.Panel_Main.Controls.Add(this.Customization);

            Client.Controls.Add(Panel_Tabs, Panel_Main, Panel_Buttons);
            Location = CenterScreen;
        }

        void tab_Click(object sender, EventArgs e)
        {
            Client.Controls.Remove(Panel_Main);
            Panel panel = (sender as Control).Tag as Panel;
            if (panel == null)
                return;
            Client.Controls.Add(panel);
            Panel_Main = panel;
        }

        void Btn_Create_Click(object sender, EventArgs e)
        {
            if (AlreadyExists(Txt_CharName.Text))
                return;

            DirectoryInfo directory = new DirectoryInfo( GlobalVars.SaveDir + "Characters/");
            if (!Directory.Exists(directory.FullName))
                Directory.CreateDirectory(directory.FullName);

            GameObject actor = GameObjectDb.Actor;
            actor["Info"]["Name"] = Txt_CharName.Text;


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

                SaveTag charTag = new SaveTag(Start_a_Town_.SaveTag.Types.Compound, Txt_CharName.Text, actor.Save());
                //Tag charTag = new Tag(Start_a_Town_.Tag.Types.Compound, Txt_CharName.Text);//, actor.Save());
                //charTag.Add(actor.Save());
                charTag.WriteTo(writer);

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
            Panel_Name = new Panel(new Vector2(Panel_Tabs.Right, 0), new Vector2(200)) { AutoSize = true };
           // Panel_Name.AutoSize = true;
            Label lbl_charName = new Label(new Vector2(0, 0), "Character name");
            Txt_CharName = new TextBox(new Vector2(0, lbl_charName.Bottom), new Vector2(150, Label.DefaultHeight));
            Txt_CharName.Text = "Reggie Finklebottom";
            if (AlreadyExists(Txt_CharName.Text))
                Txt_CharName.Text = NpcComponent.RandomName();
                //RandomizeName();
            Txt_CharName.TextEntered += new EventHandler<TextEventArgs>(Txt_CharName_TextEntered);

            Btn_Randomize = new IconButton()
            {
                BackgroundTexture = UIManager.Icon16Background,
                Location = new Vector2(Txt_CharName.Right, Txt_CharName.Top + (int)((Txt_CharName.Height - UIManager.Icon16Background.Height) / 2f)),
                Icon = new Icon(UIManager.Icons16x16, 1, 16)
            };
            Btn_Randomize.HoverText = "Randomize";
            Btn_Randomize.LeftClick += new UIEvent(Btn_Randomize_Click);


            Panel_Name.Controls.Add(lbl_charName, Txt_CharName, Btn_Randomize);
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
