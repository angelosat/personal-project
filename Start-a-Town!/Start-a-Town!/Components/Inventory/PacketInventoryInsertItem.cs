using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketInventoryInsertItem
    {
        static int p;
        static public void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(IObjectProvider net, Actor actor, Entity item, OffsiteAreaDef area)
        {
            if (net is Client)
                throw new Exception();

            var stream = net.GetOutgoingStream();
            stream.Write(p);
            stream.Write(actor.RefID);
            stream.Write(item.RefID);
            stream.Write(area.Name);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            if (net is Server)
                throw new Exception();

            var actorID = r.ReadInt32();
            var itemID = r.ReadInt32();
            var item = net.GetNetworkObject(itemID) as Entity;
            var actor = net.GetNetworkObject(actorID) as Actor;
            var area = Def.GetDef<OffsiteAreaDef>(r.ReadString());
            actor.Loot(item, area);
        }
    }
}
