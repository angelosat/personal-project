using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketEntitySync
    {
        static int PckType;
        static public void Init()
        {
            PckType = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(IObjectProvider net, GameObject entity)
        {
            if (net is Client)
                throw new Exception();
            var w = net.GetOutgoingStream();
            w.Write((int)PckType);
            w.Write(entity.RefID);
            entity.SyncWrite(w);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            if (net is Server)
                throw new Exception();
            var entity = net.GetNetworkObject(r.ReadInt32());
            entity.SyncRead(r);
        }
    }
}
