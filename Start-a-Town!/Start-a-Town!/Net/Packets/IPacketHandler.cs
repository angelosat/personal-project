using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.Net
{
    interface IPacketHandler
    {
        void Handle(IObjectProvider net, Packet packet);
    }
}
