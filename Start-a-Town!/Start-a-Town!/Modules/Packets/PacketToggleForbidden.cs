using System.Collections.Generic;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketToggleForbidden
    {
        static public void Init()
        {
            // TODO
            Client.RegisterPacketHandler(PacketType.ToggleForbidden, Receive);
            Server.RegisterPacketHandler(PacketType.ToggleForbidden, Receive);
        }
        static public void Send(IObjectProvider net, List<int> instanceID)
        {
           
            var w = net.GetOutgoingStream();
            w.Write((int)PacketType.ToggleForbidden);
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
