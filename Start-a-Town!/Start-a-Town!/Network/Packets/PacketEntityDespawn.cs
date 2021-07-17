using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes.StaticMaps;

namespace Start_a_Town_
{
    class PacketEntityDespawn
    {
        static int p;
        static public void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(INetwork net, Entity entity)
        {
            if (net is Client)
                return;
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(entity.RefID);
        }
        static public void Receive(INetwork net, BinaryReader r)
        {
            var client = net as Client;
            var actor = client.GetNetworkObject(r.ReadInt32()) as Actor;
            var map = client.Map as StaticMap;
            map.Despawn(actor);
        }
    }
}
