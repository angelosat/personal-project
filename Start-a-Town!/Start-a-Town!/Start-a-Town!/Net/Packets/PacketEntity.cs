using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Start_a_Town_.Net.Packets
{
    class PacketEntity : Packet
    {
        public int EntityID;
        public PacketEntity()
        {
            this.PacketType = Net.PacketType.Chunk;
        }
        public PacketEntity(byte[] data)
            : this()
        {

        }
        public PacketEntity(int entityID)
        {
            this.EntityID = entityID;
        }
        public PacketEntity(GameObject entity)
        {
            this.EntityID = entity.InstanceID;
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this.EntityID);
        }

        public override byte[] Write()
        {
            //return BitConverter.GetBytes(this.EntityID);
            return Write(this.EntityID);
        }
        //public override void Read(byte[] data)
        //{
        //    data.Translate(r => this.EntityID = r.ReadInt32());
        //}
        static public byte[] Write(int entityID)
        {
            return BitConverter.GetBytes(entityID);
        }
        static public void Write(BinaryWriter io, int entityID)
        {
            io.Write(entityID);
        }
        static public void Read(BinaryReader io, out int entityID)
        {
            entityID = io.ReadInt32();
        }
    }
}
