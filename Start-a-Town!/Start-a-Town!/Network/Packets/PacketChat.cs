using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketChat
    {
        static readonly int p;
        static PacketChat()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetwork net, int playerID, string text)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(playerID);
            w.WriteASCII(text);
        }
        internal static void Receive(INetwork net, BinaryReader r)
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
                net.EventOccured(Components.Message.Types.ChatPlayer, player, text);
            }
        }
    }
}
