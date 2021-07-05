using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketPlayerToolSwitch
    {
        internal static void Init()
        {
            Server.RegisterPacketHandler(PacketType.PlayerToolSwitch, Receive);
            Client.RegisterPacketHandler(PacketType.PlayerToolSwitch, Receive);
        }
        internal static void Send(IObjectProvider net, int playerid, ControlTool tool)
        {
            var w = net.GetOutgoingStream();
            w.Write(PacketType.PlayerToolSwitch);
            w.Write(playerid);
            tool.Write(w);
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            var plid = r.ReadInt32();
            var player = net.GetPlayer(plid);
            var tool = ControlTool.CreateOrSync(r, player);
            player.CurrentTool = tool;
            if (net is Server)
                Send(net, plid, tool);
        }
    }
}
