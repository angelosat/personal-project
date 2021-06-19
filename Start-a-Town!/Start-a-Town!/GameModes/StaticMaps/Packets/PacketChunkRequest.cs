using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketChunkRequest
    {
        internal static void Init()
        {
            Server.RegisterPacketHandler(PacketType.RequestChunks, Receive);
            Client.RegisterPacketHandler(PacketType.RequestChunks, Receive);
        }
        internal static void Send(IObjectProvider net, int playerid)
        {
            var w = (net as Client).OutgoingStream;
            w.Write(PacketType.RequestChunks);
            w.Write(playerid);
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            var server = net as Server;
            if (server != null)
            {
                "sending chunks".ToConsole();
                var player = net.GetPlayer(r.ReadInt32());
                foreach (var ch in server.Map.GetActiveChunks().Values)
                {
                    player.PendingChunks.Add(ch.MapCoords, Network.Serialize(ch.Write));
                }
            }
        }
    }
}
