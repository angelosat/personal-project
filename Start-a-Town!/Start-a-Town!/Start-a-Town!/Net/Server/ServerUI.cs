using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.UI;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    class ServerUI : GroupBox
    {
        static ServerUI _Instance;
        static public ServerUI Instance
        {
            get
            {
                if (_Instance.IsNull())
                    _Instance = new ServerUI();
                return _Instance;
            }
        }

        ServerUI()
        {
            //Panel console = new Panel() { AutoSize = true };
            //console.Controls.Add(Server.Console);

            //Panel input = new Panel() { Location = console.BottomLeft, AutoSize = true };
            //TextBox txtbox = new TextBox()
            //{
            //    Width = Server.Console.Width,
            //    EnterFunc = (text) =>
            //    {
            //        if (text.Length > 0)
            //            Server.Command(text);
            //    },// Server.Console.Write(text),
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

             


            //Panel panel_players = new Panel() { Location = console.TopRight, Dimensions = new Vector2(150, console.Height + input.Height) };
            var panel_players = UIPlayerList.Instance;
            UIPlayerList.Instance.Location = ServerConsole.Instance.TopRight;
            UIPlayerList.Instance.Show(Server.Instance.Players);

            this.Controls.Add(ServerConsole.Instance, panel_players);
          //  this.Show();
        }
    }
}
