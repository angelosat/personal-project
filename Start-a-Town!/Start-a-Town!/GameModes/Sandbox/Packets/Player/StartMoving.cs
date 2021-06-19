using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;
using Start_a_Town_.Net;

namespace Start_a_Town_.GameModes.Sandbox.Packets.Player
{
    public partial class PlayerPacketHandler
    {
        class StartMoving : IPacketHandler
        {
            public void Handle(IObjectProvider net, Packet packet)
            {
                packet.Player.ControllingEntity.GetComponent<MobileComponent>().Start(packet.Player.ControllingEntity);
                net.Enqueue(PacketType.PlayerStartMoving, packet.Payload, SendType.Ordered | SendType.Reliable);
            }
        }
    }
}
