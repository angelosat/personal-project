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
        class PacketPlayer : IPacketHandler
        {
            public void Handle(IObjectProvider net, Packet packet)
            {
                packet.Player.Character.GetComponent<MobileComponent>().Stop(packet.Player.Character);
                net.Enqueue(PacketType.PlayerStopMoving, packet.Payload, SendType.Ordered | SendType.Reliable);
            }
        }
    }
}
