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
        static MainMenuWindow _Instance;
        public static MainMenuWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new MainMenuWindow();
                return _Instance;
            }
        }


        MessageBox quitbox;
        GameModeWindow GameModeWindow;

        public MainMenuWindow()
        {
            AutoSize = true;
            Closable = false;
            Panel panel = new Panel() { AutoSize = true, Color = Color.Black };
            Button newgame = new Button(new Vector2(0), 100, "Play");// { Color = Color.Transparent };
            Button online = new Button(newgame.BottomLeft, 100, "Online");// { Color = Color.Transparent };
         //   Button load = new Button(new Vector2(0, newgame.Bottom), 100, "Load Game");
            Button design = new Button(online.BottomLeft, 100, "Design");
            Button editor = new Button(design.BottomLeft, 100, "Editor");
            Button settings = new Button(new Vector2(0, editor.Bottom), 100, "Settings");
            Button about = new Button(new Vector2(0, settings.Bottom), 100, "About");
            Button quit = new Button(new Vector2(0, about.Bottom), 100, "Quit");
            Button create = new Button(new Vector2(0, quit.Bottom), 100, "Create Character");
            //panel.ClientSize = new Rectangle(0, 0, 100, 2 * UIManager.DefaultButtonHeight);

            this.GameModeWindow = new MainMenu.GameModeWindow();

            newgame.LeftClick += new UIEvent(newgame_Click);
            online.LeftClick += new UIEvent(online_LeftClick);
            quit.LeftClick += new UIEvent(quit_Click);
            settings.LeftClick += new UIEvent(settings_Click);
            about.LeftClick+=new UIEvent(about_Click);
           // load.Click += new UIEvent(load_Click);
            create.LeftClick += new UIEvent(create_Click);
            design.LeftClick += new UIEvent(design_Click);
            editor.LeftClick += editor_LeftClick;

            //panel.Controls.AddRange(new Control[] {newgame, settings, quit, about});
            panel.Controls.Add(newgame, online, design, editor,  settings, quit, about);
            
            //Controls.Add(panel);
            Client.Controls.Add(panel);
           // Controls.Add(Client);
            //SizeToControl(panel);

            //Location = CenterScreen;
            this.Location = UIManager.Center;
            Anchor = new Vector2(0.5f);
            Title = "Start-a-Town!";


            Net.Server.Start();

        }

        void editor_LeftClick(object sender, EventArgs e)
        {
            throw new NotImplementedException();
        }

        void online_LeftClick(object sender, EventArgs e)
        {
            MultiplayerWindow.Instance.Show();
        }

        void design_Click(object sender, EventArgs e)
        {
            //ChunkLoader.Paused = false;
            //ScreenManager.Add(Rooms.EditorRoom.Instance.Initialize());
            ScreenManager.Add(Rooms.EditorRoom.Instance.Initialize());
        }

        void about_Click(object sender, EventArgs e)
        {
            Window aboutwindow = new AboutWindow();
            aboutwindow.ShowDialog();
        }

        void settings_Click(object sender, EventArgs e)
        {
            //WindowManager.ToggleSingletonWindow<Settings>();
            //Window window = new Settings();
            SettingsWindow.Instance.Toggle();
           // window.ShowDialog();
        }

        void quit_Click(object sender, EventArgs e)
        {
            quitbox = new MessageBox("Quit game", "Are you sure you want to quit?", new ContextAction(() => "Yes", () => Game1.Instance.Exit()), new ContextAction(() => "No", () => { }));
                //quitbox.Yes += new EventHandler<EventArgs>(quitbox_Yes);
                //quitbox.Hidden += new EventHandler<EventArgs>(quitbox_Closed);
                quitbox.ShowDialog();
        }

        //void quitbox_Closed(object sender, EventArgs e)
        //{
        //    quitbox.Yes -= quitbox_Yes;
        //    quitbox.Hidden -= quitbox_Closed;
        //}

        //void quitbox_Yes(object sender, EventArgs e)
        //{
        //    Game1.Instance.Exit();
        //}

        void newgame_Click(object sender, EventArgs e)
        {
            if(GameMode.Registry.Count == 1)
            {
                GameMode.Current = GameMode.Registry.First();
                ScreenManager.Add(GameMode.Current.GetWorldSelectScreen());
                return;
            }

            this.GameModeWindow.ShowDialog();
            return;


            ScreenManager.Add(Rooms.WorldScreen.Instance.Initialize());
            Net.Server.Start();


            //string localHost = "127.0.0.1";
            //Net.Client.Connect(localHost, "host", a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
        }

        void create_Click(object sender, EventArgs e)
        {
            (new NewCharacterWindow()).ShowDialog();
        }

        //public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        //{
        //    this.ScreenClientBounds.DrawHighlight(sb);
        //    base.Draw(sb, this.Client.Bounds);
        //}

        public override void Draw(Microsoft.Xna.Framework.Graphics.SpriteBatch sb)
        {
            base.Draw(sb, this.ScreenBounds);
        }
    }
}
