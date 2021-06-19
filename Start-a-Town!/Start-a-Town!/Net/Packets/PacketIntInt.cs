using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

namespace Start_a_Town_.Net.Packets
{
    class PacketIntInt : Packet
    {
        public static byte[] Write(int arg1, int arg2)
        {
            return Network.Serialize(w =>
            {
                w.Write(arg1);
                w.Write(arg2);
            });
        }

        public static void Read(BinaryReader r, out int arg1, out int arg2)
        {
            arg1 = r.ReadInt32();
            arg2 = r.ReadInt32();
        }
    }
}
