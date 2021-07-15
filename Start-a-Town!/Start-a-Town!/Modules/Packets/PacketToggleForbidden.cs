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
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(INetwork net, List<int> instanceID)
        {
           
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(instanceID);
        }
        static public void Receive(INetwork net, BinaryReader r)
        {
            var list = r.ReadListInt();
            foreach (var id in list)
                net.GetNetworkObject(id).ToggleForbidden();
            if (net is Server)
                Send(net, list);
        }
    }
}
