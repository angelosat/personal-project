using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.PlayerControl;

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
            if (net is Client)
            {
                //player.CurrentTool = tool;
            }
            else
                Send(net, plid, tool);

            //var plid = r.ReadInt32();
            //var tool = ControlTool.Create(r);
            //if (net is Client)
            //{
            //    var player = net.GetPlayer(plid);
            //    player.CurrentTool = tool;
            //}
            //else
            //    Send(net, plid, tool);
        }
    }
}
