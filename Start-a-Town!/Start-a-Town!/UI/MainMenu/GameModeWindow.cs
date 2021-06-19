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
        public GameModeWindow(Game1 game)
        {
            this.Name = this.Title = "Choose game mode";
            this.Movable = false;
            this.AutoSize = true;

            this.ModesList = new Panel(new Rectangle(0, 0, 150, 300));
            this.Information = new PanelLabeled("Information") { Size = this.ModesList.Size, Location = this.ModesList.TopRight };

            var modes = GameMode.Registry;
            foreach(var mode in modes)
            {
                var btn = new Button(mode.Name) { Width = this.ModesList.ClientSize.Width, Location = this.ModesList.Controls.BottomLeft, LeftClickAction = () => { StartGame(game.Network.Client, mode); } };
                this.ModesList.Controls.Add(btn);
            }

            this.Client.Controls.Add(this.ModesList, this.Information);
        }

        private void StartGame(IObjectProvider net, GameMode mode)
        {
            GameMode.Current = mode;
            ScreenManager.Add(mode.GetWorldSelectScreen(net));
        }
        public override bool Show()
        {
            //this.Location = this.CenterScreen;
            this.SnapToScreenCenter();

            this.Anchor = new Vector2(.5f);
            return base.Show();
        }
    }
}
