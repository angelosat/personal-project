using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketMousePosition
    {
        static int p;
        static internal void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static internal void Send(IObjectProvider net, int playerid, TargetArgs target)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(playerid);
            target.Write(w);
        }
        static internal void Receive(IObjectProvider net, BinaryReader r)
        {
            var playerid = r.ReadInt32();
            var target = TargetArgs.Read(net.Map, r);
            net.GetPlayer(playerid)?.UpdateTarget(target);
            if (net is Server)
                Send(net, playerid, target);
        }
    }
}
