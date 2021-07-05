﻿using System;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketPlayerInput
    {
        static int p;
        internal static void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(IObjectProvider net, Vector2 direction)
        {
            if (net is Server)
                throw new NotImplementedException();
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(net.GetPlayer().ID);
            w.Write(direction);
        }
        private static void Receive(IObjectProvider net, BinaryReader r)
        {
            if(net is Client)
                throw new NotImplementedException();
            var pl = net.GetPlayer(r.ReadInt32());
            var dir = r.ReadVector2();
            pl.ControllingEntity.Transform.Direction = dir;
        }
    }
}
