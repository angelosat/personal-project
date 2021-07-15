using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketInventoryDrop
    {
        static int p;
        static public void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(INetwork net, int actorID, int itemID, int amount)
        {
            var stream = net.GetOutgoingStream();
            stream.Write(p);
            stream.Write(actorID);
            stream.Write(itemID);
            stream.Write(amount);
        }
        static public void Receive(INetwork net, BinaryReader r)
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
