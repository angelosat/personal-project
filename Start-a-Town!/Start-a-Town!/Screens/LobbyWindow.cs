using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.UI
{
    class LobbyWindow : Window
    {
        Panel Panel_Players, Panel_Chat, Panel_Input;
        ListBox<PlayerData, Label> Players;
        TextBox Txt_Input;
        public ConsoleBoxAsync Console;
        static LobbyWindow _Instance;
        public static LobbyWindow Instance => _Instance ??= new LobbyWindow();

        LobbyWindow()
        {
            Title = "Lobby";
            Size = new Rectangle(0, 0, 500, 300);
            
            AutoSize = true;
            Movable = false;

            Panel_Players = new Panel() { Dimensions = new Vector2(100, 200) };
            Players = new ListBox<PlayerData, Label>(Panel_Players.ClientSize);
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
                    PacketChat.Send(Net.Client.Instance, Net.Client.Instance.PlayerData.ID, this.Txt_Input.Text);
                    Txt_Input.Text = "";
                }
            };
            Panel_Input = new Panel(Panel_Chat.BottomLeft) { AutoSize = true };
            Panel_Input.Controls.Add(Txt_Input);

            this.Client.Controls.Add(Panel_Players, Panel_Chat, Panel_Input);

            this.SnapToScreenCenter();
        }
      
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
