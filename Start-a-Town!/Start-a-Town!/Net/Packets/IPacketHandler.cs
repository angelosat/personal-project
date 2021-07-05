using System;

namespace Start_a_Town_.Net
{
    [Obsolete]
    interface IPacketHandler
    {
        void Handle(IObjectProvider net, Packet packet);
    }
}
