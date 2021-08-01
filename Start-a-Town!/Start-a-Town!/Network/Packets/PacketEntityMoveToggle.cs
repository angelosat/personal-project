﻿using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketEntityMoveToggle
    {
        static readonly int PType;
        static PacketEntityMoveToggle()
        {
            PType = Network.RegisterPacketHandler(Receive);
        }
        internal static void Init()
        {
        }
        internal static void Send(INetwork net, int entityID, bool toggle)
        {
            //var w = net.GetOutgoingStream();
            var server = net as Server;
            var w = server.OutgoingStreamTimestamped;
            w.Write(PType);
            w.Write(entityID);
            w.Write(toggle);
        }
        internal static void Receive(INetwork net, BinaryReader r)
        {
            var id = r.ReadInt32();
            var entity = net.GetNetworkObject(id) as Actor;
            var toggle = r.ReadBoolean();
            entity.MoveToggle(toggle);
            if (net is Server)
                Send(net, entity.RefID, toggle);
        }
    }
}
