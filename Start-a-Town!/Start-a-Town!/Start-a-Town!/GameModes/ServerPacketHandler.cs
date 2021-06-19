using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.GameModes
{
    abstract class ServerPacketHandler
    {
        public abstract void Handle(Server server, Packet msg);
    }
}
