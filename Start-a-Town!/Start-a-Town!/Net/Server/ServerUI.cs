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
            //var panel_players = UIPlayerList.Instance;
            //UIPlayerList.Instance.Location = ServerConsole.Instance.TopRight;
            //UIPlayerList.Instance.Show(Server.Instance.Players);

            this.Controls.Add(ServerConsole.Instance);//, panel_players);
        }
    }
}
