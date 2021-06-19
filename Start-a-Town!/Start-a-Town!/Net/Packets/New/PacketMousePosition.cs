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
    class PacketMousePosition
    {
        static internal void Init()
        {
            Server.RegisterPacketHandler(PacketType.MousePosition, Receive);
            Client.RegisterPacketHandler(PacketType.MousePosition, Receive);
        }
        //static internal void Send(IObjectProvider net, int playerid, Vector2 cameraposition, float camerazoom, Vector2 mouseposition, TargetArgs target)
        static internal void Send(IObjectProvider net, int playerid, TargetArgs target)
        {
            var w = net.GetOutgoingStream();
            w.Write(PacketType.MousePosition);
            w.Write(playerid);
            //w.Write(cameraposition);
            //w.Write(camerazoom);
            //w.Write(mouseposition);
            target.Write(w);
        }
        static internal void Receive(IObjectProvider net, BinaryReader r)
        {
            var playerid = r.ReadInt32();
            var target = TargetArgs.Read(net.Map, r);
            net.GetPlayer(playerid)?.UpdateTarget(target);
            if (net is Server)
                Send(net, playerid, target);
            //if (net is Server)
            //    Send(net, playerid, target);
            //else
            //{
            //    var pl = (net as Client).GetPlayer(playerid);
            //    if (pl != null)
            //        pl.UpdateTarget(target);
            //}
        }
    }
}
