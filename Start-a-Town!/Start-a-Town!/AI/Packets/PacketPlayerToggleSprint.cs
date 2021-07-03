﻿using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketPlayerToggleSprint
    {
        internal static void Init()
        {
            Server.RegisterPacketHandler(PacketType.PlayerNpcControlToggleSprint, Receive);
        }
        internal static void Send(IObjectProvider net, bool toggle)
        {
            if (net is Server)
                throw new Exception();
            var w = net.GetOutgoingStream();
            w.Write(PacketType.PlayerNpcControlToggleSprint);
            w.Write(net.GetPlayer().ID);
            w.Write(toggle);
        }
        private static void Receive(IObjectProvider net, BinaryReader r)
        {
            if (net is Client)
                throw new Exception();
            var pl = net.GetPlayer(r.ReadInt32());
            pl.ControllingEntity.SprintToggle(r.ReadBoolean());
        }
    }
}
