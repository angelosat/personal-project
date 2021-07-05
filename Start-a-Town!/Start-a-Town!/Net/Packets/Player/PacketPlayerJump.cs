using System;
using Start_a_Town_.Components;

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
