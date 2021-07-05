using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketEntityMoveToggle
    {
        static readonly PacketType PType = PacketType.EntityMoveToggle;
        internal static void Init()
        {
            // TODO
            Server.RegisterPacketHandler(PType, Receive);
            Client.RegisterPacketHandler(PType, Receive);
        }
        internal static void Send(IObjectProvider net, int entityID, bool toggle)
        {
            var w = net.GetOutgoingStream();
            w.Write(PType);
            w.Write(entityID);
            w.Write(toggle);
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            var id = r.ReadInt32();
            var entity = net.GetNetworkObject(id) as Actor;
            var toggle = r.ReadBoolean();
            entity.MoveToggle(toggle);
            if (net is Server)
                Send(net, entity.RefID, toggle);
        }
    }
}
