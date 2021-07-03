using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketPlayerJump
    {
        internal static void Init()
        {
            Server.RegisterPacketHandler(PacketType.PlayerNpcControlJump, Receive);
        }
        internal static void Send(IObjectProvider net)
        {
            if (net is Server)
                throw new Exception();
            var w = net.GetOutgoingStream();
            w.Write(PacketType.PlayerNpcControlJump);
            w.Write(net.GetPlayer().ID);
        }
        private static void Receive(IObjectProvider net, BinaryReader r)
        {
            if (net is Client)
                throw new Exception();
            var pl = net.GetPlayer(r.ReadInt32());
            pl.ControllingEntity.Jump();
        }
    }
}
