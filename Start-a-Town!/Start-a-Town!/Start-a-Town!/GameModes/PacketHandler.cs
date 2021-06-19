using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.GameModes
{
    abstract class PacketHandler
    {
        public abstract void Handle(Server server, Packet msg);
        public abstract void Handle(Client client, Packet msg);
        public virtual void Update(Client client) { }
    }
}
