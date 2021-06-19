using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.GameModes
{
    abstract class ClientPacketHandler
    {
        public abstract void Handle(Client client, Packet msg);
    }
}
