using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Start_a_Town_.Towns.Forestry
{
    class PacketGroveEdit
    {
        public static byte[] Write(int groveID, string name, float density)
        {
            return Net.Network.Serialize(w =>
            {
                w.Write(groveID);
                w.Write(name);
                w.Write(density);
            });
        }
        public static void Read(BinaryReader r, out int id, out string name, out float density)
        {
            id = r.ReadInt32();
            name = r.ReadString();
            density = r.ReadSingle();
        }
    }
}
