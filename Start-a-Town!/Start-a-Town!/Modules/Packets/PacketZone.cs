using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns
{
    class PacketZone
    {
        internal static byte[] Write(int editorID, int zoneID, Vector3 begin, int width, int height, bool value)
        {
            return Network.Serialize(w =>
            {
                w.Write(editorID);
                w.Write(zoneID);
                w.Write(begin);
                w.Write(width);
                w.Write(height);
                w.Write(value);
            });
        }
        internal static void Read(BinaryReader r, out int editorID, out int zoneID, out Vector3 begin, out int width, out int height, out bool value)
        {
            editorID = r.ReadInt32();
            zoneID = r.ReadInt32();
            begin = r.ReadVector3();
            width = r.ReadInt32();
            height = r.ReadInt32();
            value = r.ReadBoolean();
        }
    }
}
