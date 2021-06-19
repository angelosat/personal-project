using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketEntityRequestDispose
    {
        internal static void Send(IObjectProvider net, int entityID)
        {
            var w = net.GetOutgoingStream();
            w.Write(PacketType.DisposeObject);
            w.Write(entityID);
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            var id = r.ReadInt32();
            net.DisposeObject(id);
            if (net is Server)
                Send(net, id);
        }

        internal static void Init()
        {
            Server.RegisterPacketHandler(PacketType.DisposeObject, Receive);
            Client.RegisterPacketHandler(PacketType.DisposeObject, Receive);
        }
    }
}
