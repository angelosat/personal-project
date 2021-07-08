using System.Collections.Generic;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketCommandNpc
    {
        static int p;
        static public void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(IObjectProvider net, List<int> npcIDs, TargetArgs target, bool enqueue)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(npcIDs);
            target.Write(w);
            w.Write(enqueue);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var npcids = r.ReadListInt();
            var target = TargetArgs.Read(net, r);
            var enqueue = r.ReadBoolean();
            foreach(var npc in net.GetNetworkObjects(npcids))
                npc.MoveOrder(target, enqueue);
        }
    }
}
