using System.Collections.Generic;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketToggleForbidden
    {
        static int p;
        static public void Init()
        {
            // TODO
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(IObjectProvider net, List<int> instanceID)
        {
           
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(instanceID);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var list = r.ReadListInt();
            Execute(net, list);
            if (net is Server)
                Send(net, list);
        }
        static void Execute(IObjectProvider net, IEnumerable<int> items)
        {
            foreach (var id in items)
                net.GetNetworkObject(id).ToggleForbidden();
        }
    }
}
