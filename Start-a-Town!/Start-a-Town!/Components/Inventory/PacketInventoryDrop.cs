using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketInventoryDrop
    {
        static public void Init()
        {
            // TODO
            Server.RegisterPacketHandler(PacketType.PacketInventoryDrop, Receive);
            Client.RegisterPacketHandler(PacketType.PacketInventoryDrop, Receive);
        }
        static public void Send(IObjectProvider net, int actorID, int itemID, int amount)
        {
            var stream = net.GetOutgoingStream();
            stream.Write((int)PacketType.PacketInventoryDrop);
            stream.Write(actorID);
            stream.Write(itemID);
            stream.Write(amount);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var actorID = r.ReadInt32();
            var itemID = r.ReadInt32();
            var amount = r.ReadInt32();
            var item = net.GetNetworkObject(itemID);
            var actor = net.GetNetworkObject(actorID);
            actor.DropInventoryItem(item, amount); // TODO: this happenes immediately when the game is paused, should poll it
            if (amount == item.StackSize)
                NpcComponent.RemovePossesion(actor, item);
            if (net is Server)
                Send(net, actorID, itemID, amount);
        }
    }
}
