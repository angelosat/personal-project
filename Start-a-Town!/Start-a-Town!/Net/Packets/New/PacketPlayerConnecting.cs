using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketPlayerConnecting
    {
        internal static void Init()
        {
            //Server.Instance.RegisterPacketHandler(PacketType.EntityInteract, Receive);
            Client.RegisterPacketHandler(PacketType.PlayerConnecting, Receive);
        }
        internal static void Send(IObjectProvider net, PlayerData player)
        {
            var w = (net as Server).OutgoingStream;
            w.Write(PacketType.PlayerConnecting);
            player.Write(w);
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            PlayerData player = PlayerData.Read(r);
            var client = net as Client;
            client.AddPlayer(player);
            
        }
    }
}
