using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Towns.Digging
{
    class PacketDiggingDesignate : Packet
    {
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
