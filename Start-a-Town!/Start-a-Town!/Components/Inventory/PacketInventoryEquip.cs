using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class PacketInventoryEquip
    {
        static public void Init()
        {
            Server.RegisterPacketHandler(PacketType.PacketInventoryEquip, Receive);
            Client.RegisterPacketHandler(PacketType.PacketInventoryEquip, Receive);
        }
        static public void Send(IObjectProvider net, int actorID, int itemID)
        {
            var stream = net.GetOutgoingStream();
            stream.Write((int)PacketType.PacketInventoryEquip);
            stream.Write(actorID);
            stream.Write(itemID);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var actorID = r.ReadInt32();
            var itemID = r.ReadInt32();
            var item = net.GetNetworkObject(itemID);
            var actor = net.GetNetworkObject(actorID) as Actor;
            actor.Equip(item);
            if (net is Server)
                Send(net, actorID, itemID);
        }
    }
}
