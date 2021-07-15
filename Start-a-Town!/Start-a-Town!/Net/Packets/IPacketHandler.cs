using System;

namespace Start_a_Town_.Net
{
    [Obsolete]
    interface IPacketHandler
    {
        void Handle(INetwork net, Packet packet);
    }
}
