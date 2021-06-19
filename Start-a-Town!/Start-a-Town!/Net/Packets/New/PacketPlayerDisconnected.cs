using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketPlayerDisconnected
    {
        internal static void Send(IObjectProvider net, int playerID)
        {
            var server = net as Server;
            var w = server.OutgoingStream;
            w.Write(PacketType.PlayerDisconnected);
            w.Write(playerID);
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            var playerID = r.ReadInt32();
            (net as Client).PlayerDisconnected(playerID);
        }

        internal static void Init()
        {
            Client.RegisterPacketHandler(PacketType.PlayerDisconnected, Receive);
        }
    }
}
