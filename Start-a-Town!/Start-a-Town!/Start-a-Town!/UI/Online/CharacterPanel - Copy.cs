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
        Button btn_select;
        Button btn_create;
        CharacterBrowser Browser;
        GameObject Character;
        Control CharacterInfo;
        public CharacterPanel()
            : base("Character")
        {
            //this.AutoSize = true;
            this.Width = 200;
            this.Height = 200;
            this.ClientSize = new Rectangle(0, 0, this.Width, this.Height);
            this.Browser = new CharacterBrowser(this.Width, this.Height, this.SelectCharacter);
            btn_select = new Button("Select Character", this.ClientSize.Width)
            {
                Location = new Vector2(0, this.ClientSize.Height),
                Anchor = Vector2.UnitY,
                LeftClickAction = () =>
                {
                    this.Browser.Refresh();
                    this.Browser.Location = btn_select.ScreenLocation;// +this.BottomLeft;
                    this.Browser.Anchor = Vector2.UnitX;
                    this.Browser.Toggle();
                }
            };

            btn_create = new Button("Create New Character", this.ClientSize.Width)
            {
                Location = btn_select.Location,
                Anchor = Vector2.UnitY,
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
            this.Controls.Add(btn_select, btn_create);
        }

        private void SelectCharacter(GameObject ch)
        {
            if (ch.IsNull())
                return;
            this.Character = ch;
            this.Controls.Remove(this.CharacterInfo);
            this.CharacterInfo = ch.GetTooltip();
            this.Controls.Add(this.CharacterInfo);
        }
    }
}
