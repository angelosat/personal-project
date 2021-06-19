using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.Net.Packets.Player
{
    public partial class PlayerPacketHandler
    {
        class PacketPlayerJump : Packet, IPacketHandler
        {
            public void Handle(IObjectProvider net, Packet packet)
            {
                packet.Player.ControllingEntity.GetComponent<MobileComponent>().Jump(packet.Player.ControllingEntity);
                net.Enqueue(PacketType.PlayerJump, packet.Payload, SendType.Ordered | SendType.Reliable);
            }

        }
    }
}
