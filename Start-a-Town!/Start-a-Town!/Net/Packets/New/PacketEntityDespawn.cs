using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes.StaticMaps;

namespace Start_a_Town_
{
    class PacketEntityDespawn
    {
        static public void Init()
        {
            Client.RegisterPacketHandler(PacketType.PacketEntityDespawn, Receive);
        }
        static public void Send(IObjectProvider net, Entity entity)
        {
            if (net is Client)
                return;
            var w = net.GetOutgoingStream();
            w.Write(PacketType.PacketEntityDespawn);
            w.Write(entity.RefID);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var client = net as Client;
            var actor = client.GetNetworkObject(r.ReadInt32()) as Actor;
            var map = client.Map as StaticMap;
            map.Despawn(actor);
        }
    }
}
