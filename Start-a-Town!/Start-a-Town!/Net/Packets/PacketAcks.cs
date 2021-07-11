using Start_a_Town_.Net;
using System;
using System.IO;

namespace Start_a_Town_
{
    static class PacketAcks
    {
        static readonly int p;
        static PacketAcks()
        {
            p = Network.RegisterPacketHandler(Receive);
        }

        static void Receive(IObjectProvider arg1, BinaryReader arg2)
        {
            throw new NotImplementedException();
        }
        internal static void Send(IObjectProvider net)
        {
            throw new NotImplementedException();
        }
        internal static void Init() { }
    }
}
