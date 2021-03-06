using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    static class PacketPlayerJump
    {
        static readonly int p;
        static PacketPlayerJump()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetwork net)
        {
            if (net is Server)
                throw new Exception();
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(net.GetPlayer().ID);
        }
        private static void Receive(INetwork net, BinaryReader r)
        {
            if (net is Client)
                throw new Exception();
            var pl = net.GetPlayer(r.ReadInt32());
            pl.ControllingEntity.Jump();
        }
    }
}
