using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class CharacterPanel : PanelLabeled
    {
        //Button btn_select;
        Button BtnCreate;
        CharacterBrowser Browser;
        public GameObject Character;
        Control CharacterInfo;
        Panel PanelButtons;
        PanelLabeled PanelSelected;
        public CharacterPanel()
            : base("Select Character")
        {
            this.AutoSize = true;
            //this.Width = 400;
            //this.Height = 400;
            //this.ClientSize = new Rectangle(0, 0, this.Width, this.Height);
            this.PanelSelected = new PanelLabeled("Selected") { ClientSize = new Rectangle(0, 0, 200, 200), Location = this.Controls.BottomLeft };

            this.Browser = new CharacterBrowser(200, 200, this.SelectCharacter) { Location = this.PanelSelected.BottomLeft };
            this.Browser.Refresh();
            //btn_select = new Button("Select Character", this.ClientSize.Width)
            //{
            //    Location = new Vector2(0, this.ClientSize.Height),
            //    Anchor = Vector2.UnitY,
            //    LeftClickAction = () =>
            //    {
            //        this.Browser.Refresh();
            //        this.Browser.Location = btn_select.ScreenLocation;// +this.BottomLeft;
            //        this.Browser.Anchor = Vector2.UnitX;
            //        this.Browser.Toggle();
            //    }
            //};
            this.PanelButtons = new Panel() { AutoSize = true, Location = this.Browser.BottomLeft };
            this.BtnCreate = new Button("Create New Character", this.Browser.ClientSize.Width)
            {
                //Location = new Vector2(0, this.ClientSize.Height),
                //Anchor = Vector2.UnitY,
                LeftClickAction = () =>
                {
                    NewCharacterWindow win = new NewCharacterWindow()
                    {
                    };
                    win.HideAction = () =>
                    {
                        SelectCharacter(win.Tag as GameObject);
                        this.Browser.Refresh();
                    };
                    win.ShowDialog();
                }
            };
            this.PanelButtons.Controls.Add(this.BtnCreate);
            this.Controls.Add(this.PanelSelected, this.Browser, this.PanelButtons);
        }

        private void SelectCharacter(GameObject ch)
        {
            if (ch is null)
                return;
            this.Character = ch;
            //this.Controls.Remove(this.CharacterInfo);
            this.PanelSelected.Controls.Clear();
            this.CharacterInfo = ch.GetTooltip();
            //this.Controls.Add(this.CharacterInfo);
            this.PanelSelected.Controls.Add(this.CharacterInfo);
        }
    }
}
