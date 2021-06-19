using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.Net
{
    public interface IServerPacketHandler
    {
        void HandlePacket(Server server, Packet packet);
    }
}
