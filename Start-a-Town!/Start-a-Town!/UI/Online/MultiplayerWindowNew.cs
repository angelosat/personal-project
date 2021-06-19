using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using Microsoft.Xna.Framework;
using System.Threading;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_
{
    class MultiplayerWindowNew : Window
    {
        const string DefaultName = "";
        const string DefaultHost = "127.0.0.1";
        public static MultiplayerWindowNew Instance;
        static MultiplayerWindowNew()
        {
            Instance = new MultiplayerWindowNew();
        }
        Panel PanelTextBoxes, PanelButtons;
        TextBox Txt_Name, TxtBox_HostName;
        //CharacterPanel Character;

        MultiplayerWindowNew()
        {
            Title = "Multiplayer";
            this.AutoSize = true;
            //Size = new Rectangle(0, 0, 200, 200);
            int btnWidth = 200;
            AutoSize = true;
            Func<string> hover = () => !CheckName() ? "Invalid name" : "";
            //this.Client.Controls.Add(new Button(this.Client.Controls.BottomLeft, btnWidth, "Host") { LeftClickAction = () => { if (CheckName()) Host(); }, HoverFunc = hover });
            var joinButton = new Button("Join game", btnWidth)
            {
                LeftClickAction = () =>
                {
                    if (CheckName())
                        //Join();
                        this.ConnectTo(this.TxtBox_HostName.Text);
                },
                HoverFunc = hover
            };

            var name = DefaultName;
            var host = DefaultHost;
            var namenode = Engine.Config.Root.Descendants("PlayerName").FirstOrDefault();
            var hostnode = Engine.Config.Root.Descendants("HostAddress").FirstOrDefault();
            name = namenode != null ? namenode.Value : name;
            host = hostnode != null ? hostnode.Value : host;

            var labelname = new Label("Player name");
            Txt_Name = new TextBox(this.Client.Controls.TopRight)
            {
                //Location = labelname.BottomLeft, 
                Text = name,
                Width = btnWidth
            };
            Txt_Name.InputFunc = (t, c) =>
            {
                if (char.IsLetter(c))
                    return t + c;
                return t;
            };
            var labelhost = new Label("Host address");
            this.TxtBox_HostName = new TextBox()
            {
                //Location = labelhost.BottomLeft,
                Text = host,// "127.0.0.1",
                Width = btnWidth,
                InputFilter = c => char.IsLetterOrDigit(c) || char.IsPunctuation(c)
            };

            //this.Character = new CharacterPanel() { Location = this.Txt_Name.BottomLeft };
            this.PanelTextBoxes = new Panel() { AutoSize = true };
            this.PanelTextBoxes.AddControlsVertically(
                labelname,
                this.Txt_Name,
                labelhost,
                this.TxtBox_HostName
                );

            this.PanelButtons = new Panel() { AutoSize = true };
            this.PanelButtons.AddControls(joinButton);

            this.Client.AddControlsVertically(this.PanelTextBoxes, this.PanelButtons
                );

            //Location = CenterScreen;
        }

        private void Dedicated()
        {
            Server.Start();
            ServerUI.Instance.Show();

        }

        private bool CheckName()
        {
            //if (this.Character.Character = null)
            //    return false;
            return !(String.IsNullOrEmpty(Txt_Name.Text) || String.IsNullOrWhiteSpace(Txt_Name.Text));
        }

        private void Host()
        {
            Server.Start();
            string localHost = "127.0.0.1";
            Net.Client.Instance.Connect(localHost, new PlayerData(Txt_Name.Text), a => { LobbyWindow.Instance.Console.Write("Connected to " + localHost); });
            LobbyWindow.Instance.Show();
        }

        public override bool Show()
        {
            //this.Location = this.CenterScreen;
            this.SnapToScreenCenter();
            return base.Show();
        }

        private void ConnectTo(string address)
        {
            //try
            //{
            UIConnecting.Create(address);

            SaveFieldsToConfig();

            Engine.PlayGame(null);//this.Character.Character);
            //Rooms.Ingame ingame = new Rooms.Ingame();
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
        }

        private void SaveFieldsToConfig()
        {
            var name = this.Txt_Name.Text;
            var host = this.TxtBox_HostName.Text;
            Engine.Config.Root.GetOrCreateElement("Mutiplayer").GetOrCreateElement("PlayerName").Value = name;
            Engine.Config.Root.GetOrCreateElement("Mutiplayer").GetOrCreateElement("HostAddress").Value = host;
            Engine.SaveConfig();
        }
    }
}
