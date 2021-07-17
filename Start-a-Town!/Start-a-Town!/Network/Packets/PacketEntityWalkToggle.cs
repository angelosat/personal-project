using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketEntityWalkToggle
    {
        static int PType;
        internal static void Init()
        {
            PType = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetwork net, int entityID, bool toggle)
        {
            var w = net.GetOutgoingStream();
            w.Write(PType);
            w.Write(entityID);
            w.Write(toggle);
        }
        internal static void Receive(INetwork net, BinaryReader r)
        {
            var id = r.ReadInt32();
            var entity = net.GetNetworkObject(id) as Actor;
            var toggle = r.ReadBoolean();
            entity.WalkToggle(toggle);

            if (net is Server)
                Send(net, entity.RefID, toggle);
        }
    }
}
