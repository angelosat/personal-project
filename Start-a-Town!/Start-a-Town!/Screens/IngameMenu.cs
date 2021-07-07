using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.UI
{
    class IngameMenu : Window
    {
        public Panel PanelButtons;

        public IngameMenu()
        {
            this.PanelButtons = new Panel();
            this.PanelButtons.AutoSize = true;
            int w = 150;
            Button save = new Button(new Vector2(0), w, "Save") { LeftClickAction = save_Click };
            Button load = new Button(new Vector2(0, 23), w, "Load"){ LeftClickAction = load_Click };
            Button settings = new Button(Vector2.Zero, w, "Settings") { LeftClickAction = settings_Click };
            Button debug = new Button(new Vector2(0, settings.Bottom), w, "Debug") { LeftClickAction = debug_Click };
            Button help = new Button(new Vector2(0, debug.Bottom), w, "Help") { LeftClickAction = help_Click };
            Button quit = new Button(new Vector2(0, help.Bottom), w, "Quit to main menu") { LeftClickAction = quit_Click };
            Button saveexit = new Button(new Vector2(0, quit.Bottom), w, "Save & exit") { LeftClickAction = saveexit_Click };
            Button exit = new Button(new Vector2(0, saveexit.Bottom), w, "Exit to desktop") { LeftClickAction = exit_Click };

            this.PanelButtons.Controls.Add(settings, debug, help, quit);
            Client.Controls.Add(this.PanelButtons);
            SizeToControl(this.PanelButtons);

            Title = "Options";
        }

        void saveexit_Click()
        {
            Net.Client.Instance.Disconnect();
            ScreenManager.Remove();
        }

        void quit_Click()
        {
            new MessageBox("Quit game", "Are you sure you want to quit to main menu?",
                new ContextAction(() => "Yes",
                    () =>
                    {
                        Net.Client.Instance.Disconnect();
                        Net.Server.Stop();
                        ScreenManager.Remove();
                    }),
            new ContextAction(() => "No", () => { })).ShowDialog();
        }

        void help_Click()
        {
            HelpWindow.Instance.Toggle();
        }

        void debug_Click()
        {
            GlobalVars.DebugMode = !GlobalVars.DebugMode;
        }

        void exit_Click()
        {
            MessageBox exitbox = new MessageBox("Quit game", "Are you sure you want to exit the game without saving?", exitbox_Yes, () => { });
            exitbox.ShowDialog();
        }

        void exitbox_Yes()
        {
            Game1.Instance.Exit();
        }

        void settings_Click()
        {
            SettingsWindow.Instance.ToggleDialog();
        }

        void load_Click()
        {
            Console.WriteLine("load");
        }

        void save_Click()
        {
            Console.WriteLine("save");
        }

        public override bool Show()
        {
            this.SnapToScreenCenter();
            return base.Show();
        }
    }
}
