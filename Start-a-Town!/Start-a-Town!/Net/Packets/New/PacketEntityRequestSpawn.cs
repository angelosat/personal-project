using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketEntityRequestSpawn
    {
        static int p;
        internal static void Init()
        {
            p = Network.RegisterPacketHandler(ReceiveTemplate);
        }
        internal static void SendTemplate(IObjectProvider net, int templateID, TargetArgs target)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(templateID);
            target.Write(w);
        }
        
        internal static void ReceiveTemplate(IObjectProvider net, BinaryReader r)
        {
            var server = net as Server;
            var templateID = r.ReadInt32();
            var target = TargetArgs.Read(net, r);
            server.SpawnRequestFromTemplate(templateID, target);
        }
    }
}
