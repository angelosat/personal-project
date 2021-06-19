using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Net;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class PacketDiggingDesignate : Packet
    {
        static PacketType Packet = PacketType.DiggingDesignate; 
        static public void Send(IObjectProvider net, Vector3 begin, Vector3 end, bool remove)
        {
            var stream = net.GetOutgoingStream();
            stream.Write((int)Packet);
            stream.Write(begin);
            stream.Write(end);
            stream.Write(remove);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var manager = net.Map.Town.DiggingManager;
            var begin = r.ReadVector3();
            var end = r.ReadVector3();
            var remove = r.ReadBoolean();
            var positions = new BoundingBox(begin, end).GetBox();
            net.EventOccured(Components.Message.Types.MiningDesignation, positions, remove);
            if (net is Server)
                Send(net, begin, end, remove);
        }
        static public void Init()
        {
            Server.RegisterPacketHandler(Packet, Receive);
            Client.RegisterPacketHandler(Packet, Receive);
        }
        public int EntityID;
        public Vector3 Begin, End;
        //public int Width;
        //public int Height;
        public bool Remove;

        public PacketDiggingDesignate(int entityID, Vector3 begin, Vector3 end, bool remove)
        {
            // TODO: Complete member initialization
            this.EntityID = entityID;
            this.Begin = begin; //global;
            this.End = end;
            //this.Width = w;
            //this.Height = h;
            this.Remove = remove;
        }

        public PacketDiggingDesignate(byte[] data)
        {
            data.Deserialize(r =>
            {
                this.EntityID = r.ReadInt32();
                this.Begin = r.ReadVector3();
                this.End = r.ReadVector3();
                //this.Width = r.ReadInt32();
                //this.Height = r.ReadInt32();
                this.Remove = r.ReadBoolean();
            });
        }

        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.EntityID);
            w.Write(this.Begin);
            w.Write(this.End);
            //w.Write(this.Width);
            //w.Write(this.Height);
            w.Write(this.Remove);
        }

    }
}
