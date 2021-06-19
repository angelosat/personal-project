using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketToggleForbidden
    {
        static public void Init()
        {
            Client.RegisterPacketHandler(PacketType.ToggleForbidden, Receive);
            Server.RegisterPacketHandler(PacketType.ToggleForbidden, Receive);
        }
        static public void Send(IObjectProvider net, List<int> instanceID)
        {
            //if(net is Server)
            //{
            //    Execute(net, instanceID);
            //    return;
            //}
            var w = net.GetOutgoingStream();
            w.Write((int)PacketType.ToggleForbidden);
            w.Write(instanceID);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var list = r.ReadListInt();
            //foreach(var id in list)
            //    net.GetNetworkObject(id).ToggleForbidden();
            Execute(net, list);
            if (net is Server)
                Send(net, list);
        }
        static void Execute(IObjectProvider net, IEnumerable<int> items)
        {
            foreach (var id in items)
                net.GetNetworkObject(id).ToggleForbidden();
        }
        //static public void SendSingle(IObjectProvider net, int instanceID)
        //{
        //    var w = net.GetOutgoingStream();
        //    w.Write((int)PacketType.ToggleForbidden);
        //    w.Write(instanceID);
        //}
        //static public void ReceiveSingle(IObjectProvider net, BinaryReader r)
        //{
        //    var instanceID = r.ReadInt32();
        //    net.GetNetworkObject(instanceID).ToggleForbidden();
        //    if (net is Server)
        //        SendSingle(net, instanceID);
        //}
    }
}
