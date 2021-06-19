using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using System.Windows.Forms;
using System.Threading;
using Start_a_Town_.Net;

namespace Start_a_Town_.UI
{
    class MultiplayerWindow : Window
    {
        static MultiplayerWindow _Instance;
        public static MultiplayerWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new MultiplayerWindow();
                return _Instance;
            }
        }

        TextBox Txt_Name;
        CharacterPanel Character;

        MultiplayerWindow()
        {
            Title = "Online";
            Size = new Rectangle(0, 0, 200, 200);
            int btnWidth = 100;
            AutoSize = true;
            Func<string> hover = () => !CheckName() ? "Invalid name" : "";
            //   new List<Button>() {
            this.Client.Controls.Add(new Button(this.Client.Controls.BottomLeft, btnWidth, "Host") { LeftClickAction = () => { if (CheckName()) Host(); }, HoverFunc = hover });
            this.Client.Controls.Add(new Button(this.Client.Controls.BottomLeft, btnWidth, "Join") { LeftClickAction = () => { if (CheckName()) Join(); }, HoverFunc = hover });
            this.Client.Controls.Add(new Button(this.Client.Controls.BottomLeft, btnWidth, "Dedicated") { LeftClickAction = () => { Dedicated(); }, HoverFunc = hover });
            // }.ForEach(b => this.Controls.Add(b));

            this.Client.Controls.Add(new Label(this.Client.Controls.TopRight, "Name:"));
            Txt_Name = new TextBox(this.Client.Controls.TopRight) { Width = 100 };
            Txt_Name.InputFunc = (t, c) =>
            {
                if (char.IsLetter(c))
                    return t + c;
                return t;
            };
            //Txt_Name.TextEnterFunc = (e) =>
            //{
            //    if (char.IsLetter(e.KeyChar))
            //        Txt_Name.Text += e.KeyChar;
            //};
            this.Character = new CharacterPanel() { Location = this.Txt_Name.BottomLeft };

            this.Client.Controls.Add(Txt_Name, this.Character);

            //Location = CenterScreen;
        }

        private void Dedicated()
        {
            Server.Start();
            //new UIServer().Show();
            ServerUI.Instance.Show();

            //GroupBox box = new GroupBox();

            //Panel console = new Panel() { AutoSize = true };
            //console.Controls.Add(Server.Console);

            //Panel input = new Panel() { Location = console.BottomLeft, AutoSize = true };
            //TextBox txtbox = new TextBox()
            //{
            //    Width = Server.Console.Width,
            //    EnterFunc = (text) => Server.Command(text),// Server.Console.Write(text),
            //    InputFunc = (prev, c) =>
            //    {
            //        //if (!char.IsControl(c)) 
            //        //    return prev + c;
            //        return char.IsControl(c) ? prev : (prev + c);
            //    }
            //};
            //txtbox.Enabled = true;
            //input.Controls.Add(txtbox);
            ////Server.Console.Show();
            //box.Controls.Add(console, input);
            //box.Show();
        }

        private bool CheckName()
        {
            if (this.Character.Character is null)
                return false;
            return !(String.IsNullOrEmpty(Txt_Name.Text) || String.IsNullOrWhiteSpace(Txt_Name.Text));
        }

        private void Host()
        {
            Server.Start();
            string localHost = "127.0.0.1";
            Net.Client.Instance.Connect(localHost, new PlayerData(Txt_Name.Text), a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
            LobbyWindow.Instance.Show();
        }

        private void Join()
        {
            EnterIPWindow.ShowDialog();
        }

        Window EnterIPWindow
        {
            get
            {
                Window window = new Window() { Title = "Enter IP", AutoSize = true, Movable = false};//, Location = CenterScreen };
                window.SnapToScreenCenter();
                TextBox txt = new TextBox() { Width = 150, Enabled = true, Text = "127.0.0.1" };
                //txt.TextEnterFunc = (a) =>
                //{
                //    if (char.IsDigit(a.KeyChar) || char.IsPunctuation(a.KeyChar))
                //        txt.Text += a.KeyChar;
                //};
                txt.InputFunc = (t,c) =>
                {
                    if (char.IsDigit(c) || char.IsPunctuation(c))
                        return t + c;
                    return t;
                };
                window.Client.Controls.Add(txt);
                window.Client.Controls.Add(new Button(window.Client.Controls.BottomLeft, txt.Width, "Connect")
                {
                    LeftClickAction = () =>
                    {
                        string address = txt.Text;

                        //try
                        //{
                            Engine.PlayGame(this.Character.Character);
                            //Rooms.Ingame ingame = new Rooms.Ingame();
                            //ScreenManager.Add(ingame.Initialize());
                            Net.Client.Instance.Connect(address, new PlayerData(Txt_Name.Text), ar =>
                            {
                            });
                        //}
                        //catch (Exception e)
                        //{
                        //    ScreenManager.Remove();
                        //    new MessageBox(e.GetType().ToString(), e.Message, new ContextAction(() => "Back", () => { })).ShowDialog();
                        //}
                        LobbyWindow.Instance.Console.Write("Connected to " + address);
                        window.Hide();
                    }
                });

                

                return window;
            }
        }
    }
}
