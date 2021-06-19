using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketServerHandshake
    {
        internal static void Init()
        {
            throw new Exception();

            //Client.RegisterPacketHandler(PacketType.ServerHandshake, Receive);
        }
        internal static void Send(Server server, PlayerData player)
        {
            throw new Exception();
            //var w = server.OutgoingStream;
            //w.Write(player.ID);
            //server.Players.Write(w);
            //w.Write(server.Speed);
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            throw new Exception();

            //var client = net as Client;
            //if (client == null)
            //    throw new Exception();
            //var playerID = r.ReadInt32();
            //var playerList = PlayerList.Read(net, r);
            //var speed = r.ReadInt32();
            //client.HandleServerResponse(playerID, playerList, speed);

        }
    }
}
