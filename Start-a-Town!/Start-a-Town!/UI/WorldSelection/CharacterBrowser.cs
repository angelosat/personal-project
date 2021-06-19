using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class CharacterBrowser : PanelLabeled
    {
        ListBox<FileInfo, Button> Character_List;
        Action<GameObject> Callback;

        public CharacterBrowser(int width, int height, Action<GameObject> callback):base("Characters")
        {
            //this.Height = height;
            //this.Width = width;
            this.AutoSize = true;
            this.Callback = callback;
            Character_List = new ListBox<FileInfo, Button>(new Rectangle(0, 0, width, height)) { Location = this.Controls.BottomLeft };
            this.Controls.Add(this.Character_List);
        }

        public void Refresh()
        {
            FileInfo[] characters = SelectCharacterWindow.GetCharacters();
            Character_List.Build(characters, foo => foo.Name.Split('.')[0],
                (file, ctrl) =>
                {
                    ctrl.IdleColor = Color.Black;
                    ctrl.ColorFunc = () => new Color(0.5f, 0.5f, 0.5f, 1f);
                    ctrl.LeftClickAction = () =>
                    {
                        GameObject ch = SelectCharacterWindow.LoadCharacter(file);
                        SelectCharacter(ch);
                    };
                });
        }
        private void SelectCharacter(GameObject ch)
        {
            if (ch is null)
                return;
            this.Callback(ch);
            //Window_Character.Tag = ch;
            //Box_CharacterInfo.Controls.Clear();
            //Box_CharacterInfo.Controls.Add(ch.GetTooltip());
        }
    }
}
