using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Towns
{
    class PacketStockpileCreate
    {
        static public byte[] Write(int id, Vector3 start, int width, int height)
        {
            return Network.Serialize(w =>
                {
                    w.Write(id);
                    w.Write(start);
                    w.Write(width);
                    w.Write(height);
                });
        }
        static public void Read(BinaryReader r, out int id, out Vector3 start, out int width, out int height)
        {
            id = r.ReadInt32();
            start = r.ReadVector3();
            width = r.ReadInt32();
            height = r.ReadInt32();
        }
    }
}
