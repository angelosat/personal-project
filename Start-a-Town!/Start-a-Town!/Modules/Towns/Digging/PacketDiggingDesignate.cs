using Start_a_Town_.Net;
using System.IO;
using System;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class PacketDiggingDesignate : Packet
    {
        static int p;
        static public void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static public void Send(IObjectProvider net, Vector3 begin, Vector3 end, bool remove)
        {
            var stream = net.GetOutgoingStream();
            stream.Write(p);
            stream.Write(begin);
            stream.Write(end);
            stream.Write(remove);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var begin = r.ReadVector3();
            var end = r.ReadVector3();
            var remove = r.ReadBoolean();
            var positions = new BoundingBox(begin, end).GetBox();
            net.EventOccured(Components.Message.Types.MiningDesignation, positions, remove);
            if (net is Server)
                Send(net, begin, end, remove);
        }
        
        public int EntityID;
        public Vector3 Begin, End;
        public bool Remove;

        [Obsolete]
        public PacketDiggingDesignate(byte[] data)
        {
            data.Deserialize(r =>
            {
                this.EntityID = r.ReadInt32();
                this.Begin = r.ReadVector3();
                this.End = r.ReadVector3();
                this.Remove = r.ReadBoolean();
            });
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.EntityID);
            w.Write(this.Begin);
            w.Write(this.End);
            w.Write(this.Remove);
        }
    }
}
