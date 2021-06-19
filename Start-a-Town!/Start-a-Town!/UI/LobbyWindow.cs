using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.Net;

namespace Start_a_Town_.UI
{
    class LobbyWindow : Window
    {
        Panel Panel_Players, Panel_Chat, Panel_Input;
        ListBox<PlayerData, Label> Players;
        TextBox Txt_Input;
        public ConsoleBoxAsync Console;
        //{
        //    get{return(if(_Console.IsNull())
        //        _Console = new ConsoleBox(
        //}
        static LobbyWindow _Instance;
        public static LobbyWindow Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new LobbyWindow();
                return _Instance;
            }
        }

        LobbyWindow()
        {
            Title = "Lobby";
            Size = new Rectangle(0, 0, 500, 300);
            
            AutoSize = true;
            Movable = false;

            Panel_Players = new Panel() { Dimensions = new Vector2(100, 200) };
            Players = new ListBox<PlayerData, Label>(Panel_Players.ClientSize);
           // Players.Build(Start_a_Town_.Client.Players, ip => ip.Name);// ip.ToString());
            Panel_Players.Controls.Add(Players);

            Panel_Chat = new Panel(Panel_Players.TopRight) { Dimensions = new Vector2(400, 200) };
            Console = new ConsoleBoxAsync(Panel_Chat.ClientSize);
            Panel_Chat.Controls.Add(Console);

            Txt_Input = new TextBox(Vector2.Zero, new Vector2(Panel_Chat.ClientSize.Width, TextBox.DefaultHeight))
            {
                TextEnterFunc = (e) =>
                {
                    if (!char.IsControl(e.KeyChar))
                        Txt_Input.Text += e.KeyChar;
                },
                EnterFunc = (e) =>
                {
                    //byte[] data = Net.Network.Serialize(writer =>
                    //{
                    //    writer.Write(Client.PlayerData.Name.Length);
                    //    writer.Write(Encoding.ASCII.GetBytes(Client.PlayerData.Name));
                    //    writer.Write(Txt_Input.Text.Length);
                    //    writer.Write(Encoding.ASCII.GetBytes(Txt_Input.Text));
                    //});
                    //Client.Send(Packet.Create(PacketType.Chat, data));

                    Packet.Create(Net.Client.Instance.PacketID, PacketType.Chat, Network.Serialize(writer =>
                    {
                        Net.Client.Instance.PlayerData.Write(writer);
                        writer.WriteASCII(Txt_Input.Text);
                    })).BeginSendTo(Net.Client.Instance.Host, Net.Client.Instance.RemoteIP);
                    
                    Txt_Input.Text = "";
                }
            };
            Panel_Input = new Panel(Panel_Chat.BottomLeft) { AutoSize = true };
            Panel_Input.Controls.Add(Txt_Input);

            this.Client.Controls.Add(Panel_Players, Panel_Chat, Panel_Input);

            this.SnapToScreenCenter();
        }
        public override void Update()
        {
            base.Update();
        }
        //void Server_OnPlayerConnect(object sender, EventArgs e)
        //{
        //    Players.Build(Start_a_Town_.Client.Players, player => player.Name.ToString());
        //}
        static public void RefreshPlayers(IEnumerable<PlayerData> players)
        {
            Instance.Players.Build(players, ip => ip.Name);
        }
        static public void RefreshPlayers()
        {
            Instance.Players.Build(Net.Client.Instance.Players.GetList(), ip => ip.Name);
        }
    }
}
