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
    class PacketChat
    {
        internal static void Init()
        {
            Server.RegisterPacketHandler(PacketType.Chat, Receive);
            Client.RegisterPacketHandler(PacketType.Chat, Receive);
        }
        internal static void Send(IObjectProvider net, int playerID, string text)
        {
            var w = net.GetOutgoingStream();
            w.Write(PacketType.Chat);
            w.Write(playerID);
            w.WriteASCII(text);
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            var playerid = r.ReadInt32();
            var text = r.ReadASCII();
            if(net is Server)
            {
                Send(net, playerid, text);
            }
            else 
            {
                var player = net.GetPlayer(playerid);
                //GameObject plachar;
                //if (!Instance.TryGetNetworkObject(pla.CharacterID, out plachar))
                //    throw new Exception("Player character with id: " + pla.CharacterID + " is missing (" + plachar.Name + ")");
                net.EventOccured(Components.Message.Types.ChatPlayer, player.Name, text);
            }
        }
    }
}
