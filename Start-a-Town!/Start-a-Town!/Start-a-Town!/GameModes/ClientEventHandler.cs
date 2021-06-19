using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.GameModes
{
    //interface IClientEventHandler
    //{
    //    void HandleEvent(Client client, GameEvent e);
    //}
    abstract class ClientEventHandler
    {
        //public Server Server { get; protected set; }
        public abstract void HandleEvent(Client client, GameEvent e);

        //public ServerEventHandler(Server server)
        //{
        //    this.Server = server;
        //}
    }
}
