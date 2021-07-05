using System;
using System.IO;

namespace Start_a_Town_.Net.Packets
{
    [Obsolete]
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
            this.EntityID = entity.RefID;
        }
        public override void Write(BinaryWriter w)
        {
            w.Write(this.EntityID);
        }

        public override byte[] Write()
        {
            return Write(this.EntityID);
        }
        
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
