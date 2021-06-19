using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Start_a_Town_.Net.Packets
{
    class PacketInt : Packet
    {
        public static byte[] Write(int value)
        {
            return Network.Serialize(w =>
            {
                w.Write(value);
            });
        }

        public static void Read(BinaryReader r, out int value)
        {
            value = r.ReadInt32();
        }
    }
}
