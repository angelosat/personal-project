using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketPlayerSetSpeed
    {
        static readonly PacketType Type = PacketType.PlayerSetSpeed;
        internal static void Init()
        {
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
        //internal static void ReceiveOld(IObjectProvider net, BinaryReader r)
        //{
        //    int speed = r.ReadInt32();
        //    net.Speed = speed;
        //    if(net is Client)
        //        Rooms.Ingame.Instance.Hud.Chat.Write(Log.EntryTypes.System, string.Format("Speed set to {0}x", speed));
        //        //Client.Console.Write(string.Format("Speed set to {0}x", speed));
        //    else if (net is Server)
        //        Send(net, speed);
        //}
    }
}
