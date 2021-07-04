using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI.MainMenu;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.UI
{
    class MainMenuWindow : Window
    {
        MessageBox quitbox;
        public MainMenuWindow(Game1 game)
        {
            AutoSize = true;
            Closable = false;
            Panel panel = new Panel() { AutoSize = true, Color = Color.Black };
            Button newgame = new Button("Play", 100) { LeftClickAction = newgame_Click };
            Button load = new Button("Load", 100) { LeftClickAction = Load };
            Button online = new Button("Multiplayer", 100) { LeftClickAction = online_LeftClick };
            Button design = new Button("Design", 100) { LeftClickAction = design_Click };
            Button editor = new Button("Editor", 100) { LeftClickAction = editor_LeftClick };
            Button settings = new Button("Settings", 100) { LeftClickAction = settings_Click };
            Button about = new Button("About", 100) { LeftClickAction = about_Click };
            Button quit = new Button("Quit", 100) { LeftClickAction = quit_Click };
            Button create = new Button(new Vector2(0, quit.Bottom), 100, "Create Character") { LeftClickAction = create_Click };


            panel.AddControlsVertically(newgame, load, online,
                //design, editor, 
                settings, about, quit);
            
            Client.Controls.Add(panel);
            //this.Location = UIManager.Center;
            //Anchor = new Vector2(0.5f);
            this.SnapToScreenCenter();
            Title = "Start-a-Town!";
        }

        

        void editor_LeftClick()
        {
            throw new NotImplementedException();
        }

        void online_LeftClick()
        {
            if (GameMode.Registry.Count == 1)
            {
                GameMode.Current = GameMode.Registry.First();
                MultiplayerWindowNew.Instance.ShowFrom(this);
            }
        }

        void design_Click()
        {
            //ScreenManager.Add(Rooms.EditorRoom.Instance.Initialize());
        }

        void about_Click()
        {
            Window aboutwindow = new AboutWindow();
            aboutwindow.ShowDialog();
        }

        void settings_Click()
        {
            SettingsWindow.Instance.ShowFrom(this);
        }

        void quit_Click()
        {
            quitbox = new MessageBox("Quit game", "Are you sure you want to quit?", new ContextAction(() => "Yes", () => Game1.Instance.Exit()), new ContextAction(() => "No", () => { }));
                quitbox.ShowDialog();
        }

        void newgame_Click()
        {
            GameMode.Current = GameMode.Registry.First();
            this.Hide();

            var client = new GroupBox();
            client.AddControlsVertically(
                    GameMode.Registry.First().NewGame(),
                    new Button("Back")
                    {
                        LeftClickAction = () => { this.Show(); client.GetWindow().Hide(); }
                    });
            var win = new Window("New Game", client)//.Show();
            {
                //Previous = this, 
                Movable = false,
                Closable = false
            }.AnchorToScreenCenter().Show();

            return;
            //ScreenManager.Add(GameModes.StaticMaps.ScreenNewGame.Instance);
           
        }
        private void Load()
        {
            if (GameMode.Registry.Count == 1)
            {
                GameMode.Current = GameMode.Registry.First();
            }
            var control = GameMode.Current.Load().ToWindow("Load");
            control.Previous = this;
            control.LocationFunc = () => UIManager.Center;
            control.Movable = false;
            control.Anchor = Vector2.One * .5f;
            control.Show();
            this.Hide();
        }
        void create_Click()
        {
            (new NewCharacterWindow()).ShowDialog();
        }

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.Draw(sb, this.BoundsScreen);
        }
    }
}
