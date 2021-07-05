using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketEntityJump
    {
        static readonly PacketType PType = PacketType.EntityJump;
        internal static void Init()
        {
            // TODO
            Server.RegisterPacketHandler(PType, Receive);
            Client.RegisterPacketHandler(PType, Receive);
        }
        internal static void Send(IObjectProvider net, int entityID)
        {
            var w = net.GetOutgoingStream();
            w.Write(PType);
            w.Write(entityID);
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            var id = r.ReadInt32();
            var entity = net.GetNetworkObject(id) as Actor;
            entity.Jump();
            if (net is Server)
                Send(net, entity.RefID);
        }
    }
}
