using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketEntityRequestDispose
    {
        static readonly int p;
        static PacketEntityRequestDispose()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetwork net, int entityID)
        {
            var w = net is Server server ? server.OutgoingStreamTimestamped : net.GetOutgoingStream();
            w.Write(p);
            w.Write(entityID);
        }
        internal static void Receive(INetwork net, BinaryReader r)
        {
            var id = r.ReadInt32();
            net.DisposeObject(id);
            if (net is Server)
                Send(net, id);
        }
    }
}
