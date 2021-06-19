using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Net.Packets
{
    class PacketChunk : Packet
    {
        public Chunk Chunk;
        public PacketChunk()
        {
            this.PacketType = Net.PacketType.Chunk;
        }
        public PacketChunk(byte[] data)
            : this()
        {
            this.Chunk = Network.Deserialize<Chunk>(data, Chunk.Create);
        }
        public override void Read(IObjectProvider net, byte[] data)
        {
            this.Chunk = Network.Deserialize<Chunk>(data, Chunk.Create);
        }
    }
}
