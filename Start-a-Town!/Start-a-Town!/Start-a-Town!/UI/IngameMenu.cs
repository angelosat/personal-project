using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Start_a_Town_.UI
{
    class IngameMenu : Window
    {
        //static IngameMenu _Instance;
        //public static IngameMenu Instance
        //{
        //    get
        //    {
        //        if (_Instance == null)
        //            _Instance = new IngameMenu();
        //        return _Instance;
        //    }
        //}

        public Panel PanelButtons;

        public IngameMenu()
        {
            this.PanelButtons = new Panel();
            this.PanelButtons.AutoSize = true;
            int w = 150;
            //this.AutoSize = true;
            Button save = new Button(new Vector2(0), w, "Save");
            Button load = new Button(new Vector2(0, 23), w, "Load");
            Button settings = new Button(Vector2.Zero, w, "Settings");
            Button debug = new Button(new Vector2(0, settings.Bottom), w, "Debug");
            Button help = new Button(new Vector2(0, debug.Bottom), w, "Help");
            Button quit = new Button(new Vector2(0, help.Bottom), w, "Quit to main menu");
            Button saveexit = new Button(new Vector2(0, quit.Bottom), w, "Save & exit");
            Button exit = new Button(new Vector2(0, saveexit.Bottom), w, "Exit to desktop");
            //panel.ClientSize = new Rectangle(0, 0, 100, 4 * UIManager.DefaultButtonHeight);

            save.LeftClick += new UIEvent(save_Click);
            load.LeftClick += new UIEvent(load_Click);
            settings.LeftClick += new UIEvent(settings_Click);
            debug.LeftClick += new UIEvent(debug_Click);
            help.LeftClick += new UIEvent(help_Click);
            quit.LeftClick += new UIEvent(quit_Click);
            saveexit.LeftClick += new UIEvent(saveexit_Click);
            exit.LeftClick += new UIEvent(exit_Click);

            this.PanelButtons.Controls.Add(settings, debug, help, quit);//, saveexit, exit);
            Client.Controls.Add(this.PanelButtons);
            SizeToControl(this.PanelButtons);

            //IsDraggable = true;
            //ScreenLocation = CenterScreen;
            
            Title = "Options";
        }

        void saveexit_Click(object sender, EventArgs e)
        {
    //        Engine.Map.Save();
            //Net.Client.Exit();
            Net.Client.Disconnect();
            ScreenManager.Remove();
        }

        //void quit_Click(object sender, EventArgs e)
        //{
        //    //Game1.Instance.CurrentRoom.Active = false;
        //    //Map.Instance = null;
        //    Rooms.ScreenManager.Remove();
        //}

        void quit_Click(Object sender, EventArgs e)
        {
            //MessageBox quitbox = new MessageBox("Quit game", "Are you sure you want to quit to main menu without saving?");
            //quitbox.Yes += new EventHandler<EventArgs>(quitbox_Yes);
            //quitbox.Hidden += new EventHandler<EventArgs>(quitbox_Closed);
            //quitbox.ShowDialog();
            new MessageBox("Quit game", "Are you sure you want to quit to main menu without saving?",
                new ContextAction(() => "Yes",
                    () =>
                    {
                        //Net.Client.Exit();
                        Net.Client.Disconnect();
                        ScreenManager.Remove();
                    }),
            new ContextAction(() => "No", () => { })).ShowDialog();
        }

        void help_Click(object sender, EventArgs e)
        {
            //Game1.Instance.CurrentRoom.WindowManager.CreateSingletonWindow<HelpWindow>();
            HelpWindow.Instance.Toggle();
        }

        void debug_Click(object sender, EventArgs e)
        {
           // Game1.Instance.CurrentRoom.WindowManager.CreateSingletonWindow<DebugWindow>();
           // Close();
            GlobalVars.DebugMode = !GlobalVars.DebugMode;
        }

        void exit_Click(Object sender, EventArgs e)
        {
            //Game1.Instance.Exit();
            MessageBox exitbox = new MessageBox("Quit game", "Are you sure you want to exit the game without saving?");
            exitbox.Yes += new EventHandler<EventArgs>(exitbox_Yes);
            exitbox.Hidden += new EventHandler<EventArgs>(exitbox_Closed);
            //WindowManager.AddWindow(quitbox);
            exitbox.ShowDialog();
        }

        void exitbox_Yes(object sender, EventArgs e)
        {
            Game1.Instance.Exit();
            MessageBox box = sender as MessageBox;
            box.Yes -= exitbox_Yes;
            box.Hidden -= exitbox_Closed;
        }
        void exitbox_Closed(object sender, EventArgs e)
        {
            MessageBox box = sender as MessageBox;
            box.Yes -= exitbox_Yes;
            box.Hidden -= exitbox_Closed;
        }

        void quitbox_Yes(object sender, EventArgs e)
        {
            //Net.Client.Exit();
            Net.Client.Disconnect();

            ScreenManager.Remove();
            MessageBox box = sender as MessageBox;
            box.Yes -= quitbox_Yes;
            box.Hidden -= quitbox_Closed;
        }
        void quitbox_Closed(object sender, EventArgs e)
        {
            MessageBox box = sender as MessageBox;
            box.Yes -= quitbox_Yes;
            box.Hidden -= quitbox_Closed;
        }

        void settings_Click(Object sender, EventArgs e)
        {
            //UIManager.CreateWindow<Settings>(new Vector2(300));
            //Game1.Instance.CurrentRoom.WindowManager.CreateSingletonWindow<Settings>();
            SettingsWindow.Instance.ToggleDialog();
            //Close();
        }

        void load_Click(Object sender, EventArgs e)
        {
            Console.WriteLine("load");
        }

        void save_Click(Object sender, EventArgs e)
        {
            Console.WriteLine("save");
        }

        //public override void HandleKeyPress(System.Windows.Forms.KeyPressEventArgs e)
        //{
        //    if(e.KeyChar == 27)
        //        if(this.TopLevelControl != null)
        //        this.Hide();
        //    this.Show();
        //}

        public override bool Show(params object[] p)
        {
            Location = CenterScreen;
            this.Anchor = new Vector2(.5f);
            return base.Show(p);
        }
    }
}
