﻿using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketChunkRequest
    {
        static int p;
        internal static void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(IObjectProvider net, int playerid)
        {
            var w = (net as Client).OutgoingStream;
            w.Write(p);
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
