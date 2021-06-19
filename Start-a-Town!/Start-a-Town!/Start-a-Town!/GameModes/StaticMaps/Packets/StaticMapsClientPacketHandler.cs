using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.GameModes.StaticMaps.Packets
{
    class StaticMapsClientPacketHandler : ClientPacketHandler
    {
        public override void Handle(Client client, Packet msg)
        {
            switch (msg.PacketType)
            {
                case PacketType.RequestPlayerID:
                    // request map?
                    //client.Enqueue(PacketType.StaticMaps, )
                    break;
                default: break;
            }
        }
    }
}
