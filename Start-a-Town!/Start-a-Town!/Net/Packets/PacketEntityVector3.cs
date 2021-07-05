using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net.Packets
{
    [Obsolete]
    class PacketEntityVector3 : Packet
    {
        public int EntityID;
        public Vector3 Vector;
        public PacketEntityVector3()
        {
            this.PacketType = Net.PacketType.Chunk;
        }
        public PacketEntityVector3(byte[] data)
            : this()
        {

        }
        public PacketEntityVector3(int entityID, Vector3 direction)
        {
            this.EntityID = entityID;
            this.Vector = direction;
        }

        public override void Write(BinaryWriter w)
        {
            w.Write(this.EntityID);
            w.Write(this.Vector);
        }
    }
}
