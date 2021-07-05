using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketChat
    {
        internal static void Init()
        {
            // TODO
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
                net.EventOccured(Components.Message.Types.ChatPlayer, player.Name, text);
            }
        }
    }
}
