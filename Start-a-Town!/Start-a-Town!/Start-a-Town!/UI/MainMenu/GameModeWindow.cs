using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.UI.MainMenu
{
    class GameModeWindow : Window
    {
        PanelLabeled Information;
        Panel ModesList;
        public GameModeWindow()
        {
            this.Name = this.Title = "Choose game mode";
            this.Movable = false;
            this.AutoSize = true;

            this.ModesList = new Panel(new Rectangle(0, 0, 150, 300));
            this.Information = new PanelLabeled("Information") { Size = this.ModesList.Size, Location = this.ModesList.TopRight };

            var modes = GameMode.Registry;
            foreach(var mode in modes)
            {
                var btn = new Button(mode.Name) { Width = this.ModesList.ClientSize.Width, Location = this.ModesList.Controls.BottomLeft, LeftClickAction = () => { StartGame(mode); } };
                this.ModesList.Controls.Add(btn);
            }

            this.Client.Controls.Add(this.ModesList, this.Information);
        }

        private void StartGame(GameMode mode)
        {
            GameMode.Current = mode;
            ScreenManager.Add(mode.GetWorldSelectScreen());
        }
        public override bool Show(params object[] p)
        {
            this.Location = this.CenterScreen;
            this.Anchor = new Vector2(.5f);
            return base.Show(p);
        }
    }
}
