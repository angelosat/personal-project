using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketPlayerSetSpeed
    {
        static readonly PacketType Type = PacketType.PlayerSetSpeed;
        internal static void Init()
        {
            // TODO
            Server.RegisterPacketHandler(Type, Receive);
            Client.RegisterPacketHandler(Type, Receive);
        }
        internal static void Send(IObjectProvider net, int playerID, int speed)
        {
            var w = net.GetOutgoingStream();
            w.Write(Type);
            w.Write(playerID);
            w.Write(speed);
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            var playerID = r.ReadInt32();
            int speed = r.ReadInt32();
            net.SetSpeed(playerID, speed);
            if (net is Server)
                Send(net, playerID, speed);
        }
    }
}
