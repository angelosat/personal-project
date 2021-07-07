using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    [Obsolete]
    class NewCharacterWindow : Window
    {
        Panel Panel_Tabs, Panel_Buttons;
        PanelLabeled Panel_Name;
        TextBox Txt_CharName;
        Button Btn_Create;
        IconButton BtnRandomize;
        GameObject Character;
        UICharacterCustomization Customization;

        public NewCharacterWindow()
        {
            Title = "Create new character";
            AutoSize = true;
            Character = new GameObject();
            Character["Info"] = new DefComponent(GameObject.Types.Actor, "Name", "Your character.");

            Panel_Tabs = new Panel(Vector2.Zero);
            Panel_Tabs.AutoSize = true;
            InitNameTab();
            this.Client.Controls.Add(this.Panel_Name);

            this.Customization = new UICharacterCustomization() { Location = this.Client.Controls.BottomLeft };
            this.Client.Controls.Add(this.Customization);

            Panel_Buttons = new Panel() { AutoSize = true, Location = this.Customization.BottomLeft };

            Btn_Create = new Button(Vector2.Zero, 100, "Create");
            Btn_Create.CustomTooltip = true;
            Btn_Create.DrawTooltip += new EventHandler<TooltipArgs>(Btn_Create_DrawTooltip);
            Panel_Buttons.Controls.Add(Btn_Create);

            Client.Controls.Add(Panel_Buttons);
            this.SnapToScreenCenter();
        }

        void Btn_Create_DrawTooltip(object sender, TooltipArgs e)
        {
            if (AlreadyExists(Txt_CharName.Text))
                e.Tooltip.Controls.Add(new Label("Name already exists!"));
        }

        private bool AlreadyExists(string name)
        {
            FileInfo[] files = SelectCharacterWindow.GetCharacters();

            foreach (FileInfo filename in files)
                if (filename.Name.Split('.')[0] == name)
                    return true;

            return false;
        }

        private void InitNameTab()
        {
            this.Panel_Name = new PanelLabeled("Name") { AutoSize = true };
            Txt_CharName = new TextBox(150) { Location = this.Panel_Name.Controls.BottomLeft };
            Txt_CharName.Text = "Reggie Finklebottom";
            if (AlreadyExists(Txt_CharName.Text))
                Txt_CharName.Text = NpcComponent.GetRandomFullName();
            Txt_CharName.InputFunc = (t, c) =>
            {
                return ((char.IsLetter(c) || char.IsWhiteSpace(c)) ? t + c : t);
            };

            BtnRandomize = new IconButton()
            {
                BackgroundTexture = UIManager.Icon16Background,
                Location = Panel_Name.Label.TopRight,
                Icon = new Icon(UIManager.Icons16x16, 1, 16),
                HoverText = "Randomize"
            };
            BtnRandomize.LeftClick += new UIEvent(Btn_Randomize_Click);
            Panel_Name.Controls.Add(Txt_CharName, BtnRandomize);
        }

        void Btn_Randomize_Click(object sender, EventArgs e)
        {
            Txt_CharName.Text = NpcComponent.GetRandomFullName();
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
