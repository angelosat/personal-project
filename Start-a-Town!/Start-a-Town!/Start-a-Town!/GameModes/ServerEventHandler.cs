using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.GameModes
{
    //interface IServerEventHandler
    //{
    //    void HandleEvent(Server server, GameEvent e);
    //}
    abstract class ServerEventHandler
    {
        public abstract void HandleEvent(Server server, GameEvent e);
    }
}
