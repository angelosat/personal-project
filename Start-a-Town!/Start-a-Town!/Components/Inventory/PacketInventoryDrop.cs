﻿using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketInventoryDrop
    {
        static int p;
        internal static void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetwork net, GameObject actor, GameObject item, int amount)
        {
            var stream = net.GetOutgoingStream();
            stream.Write(p);
            stream.Write(actor.RefID);
            stream.Write(item.RefID);
            stream.Write(amount);
        }
        static void Receive(INetwork net, BinaryReader r)
        {
            var actorID = r.ReadInt32();
            var itemID = r.ReadInt32();
            var amount = r.ReadInt32();
            var item = net.GetNetworkObject(itemID);
            var actor = net.GetNetworkObject(actorID);
            actor.Inventory.Drop(item, amount); // TODO: this happens immediately when the game is paused. maybe create an interaction with a 1 frame duration?
            if (amount == item.StackSize)
                NpcComponent.RemovePossesion(actor, item);
            if (net is Server)
                Send(net, actor, item, amount);
        }
    }
}
