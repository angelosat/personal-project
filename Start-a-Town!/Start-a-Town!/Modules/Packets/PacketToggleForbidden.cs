using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        internal static void Send(INetwork net, IEnumerable<GameObject> enumerable)
        {
            Send(net, enumerable.Select(o => o.RefID).ToList());
        }
        internal static void Send(INetwork net, List<int> instanceID)
        {
           
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(instanceID);
        }
        static void Receive(INetwork net, BinaryReader r)
        {
            var list = r.ReadListInt();
            foreach (var id in list)
                net.GetNetworkObject(id).ToggleForbidden();
            if (net is Server)
                Send(net, list);
        }

        
    }
}
