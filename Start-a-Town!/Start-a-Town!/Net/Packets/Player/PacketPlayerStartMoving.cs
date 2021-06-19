using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.Net.Packets.Player
{
    public partial class PlayerPacketHandler
    {
        class PacketPlayerStartMoving : IPacketHandler
        {
            public void Handle(IObjectProvider net, Packet packet)
            {
                packet.Player.ControllingEntity.GetComponent<MobileComponent>().Start(packet.Player.ControllingEntity);
                net.Enqueue(PacketType.PlayerStartMoving, packet.Payload, SendType.Ordered | SendType.Reliable);
            }
        }
    }
}
