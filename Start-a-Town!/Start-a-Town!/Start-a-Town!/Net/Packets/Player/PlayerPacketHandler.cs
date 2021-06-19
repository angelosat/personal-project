using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_.Net.Packets.Player
{
    public partial class PlayerPacketHandler : IPacketHandler
    {
        Dictionary<PacketType, IPacketHandler> PacketHandlers = new Dictionary<PacketType, IPacketHandler>(){
            //{PacketType.PlayerStartMoving, new PlayerStartMoving()},
            //{PacketType.PlayerStopMoving, new PlayerStopMoving()},
            //            {PacketType.PlayerJump, new PlayerJump()}
        };

        public PlayerPacketHandler()
        {
            this.PacketHandlers.Add(PacketType.PlayerStartMoving, new PacketPlayerStartMoving());
            this.PacketHandlers.Add(PacketType.PlayerStopMoving, new PacketPlayerStartMoving());
            this.PacketHandlers.Add(PacketType.PlayerJump, new PacketPlayerStartMoving());
        }

        public void Handle(IObjectProvider net, Packet packet)
        {
            IPacketHandler handler;
            if (this.PacketHandlers.TryGetValue(packet.PacketType, out handler))
                handler.Handle(net, packet);
        }

        //public void HandlePacket(IObjectProvider net, PacketMessage packet)
        //{
        //    switch (packet.PacketType)
        //    {
        //        case PacketType.PlayerStartMoving:
        //            packet.Player.Character.GetComponent<MobileComponent>().Start(packet.Player.Character);
        //            net.Enqueue(PacketType.PlayerStartMoving, packet.Payload, SendType.Ordered | SendType.Reliable);
        //            break;

        //        case PacketType.PlayerStopMoving:
        //            packet.Player.Character.GetComponent<MobileComponent>().Stop(packet.Player.Character);
        //            net.Enqueue(PacketType.PlayerStopMoving, packet.Payload, SendType.Ordered | SendType.Reliable);
        //            break;

        //        default:
        //            break;
        //    }
        //}

        
    }
}
