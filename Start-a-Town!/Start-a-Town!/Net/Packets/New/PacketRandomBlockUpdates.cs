using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketRandomBlockUpdates
    {
        static public void Init()
        {
            Server.RegisterPacketHandler(PacketType.RandomBlockUpdates, Receive);
            Client.RegisterPacketHandler(PacketType.RandomBlockUpdates, Receive);
        }
        static public void Send(IObjectProvider net, IEnumerable<Vector3> list)
        {
            if (net is Client)
                throw new Exception();
            var strem = net.GetOutgoingStream();
            strem.Write((int)PacketType.RandomBlockUpdates);
            strem.Write(list);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            if (net is Server)
                throw new Exception();
            var list = r.ReadListVector3();
			foreach(var vec in list)
                net.Map.RandomBlockUpdate(vec);
        }
    }
}
