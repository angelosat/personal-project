using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Stockpiles
{
    class PacketStockpileEdit
    {
        //internal static byte[] Write(int stockpileID, Vector3 global, int width, int height, bool value)
        //{
        //    return Network.Serialize(w =>
        //    {
        //        w.Write(stockpileID);
        //        w.Write(global);
        //        w.Write(width);
        //        w.Write(height);
        //        w.Write(value);
        //    });
        //}

        //internal static void Read(BinaryReader r, out int spID, out Vector3 global, out int w, out int h, out bool value)
        //{
        //    spID = r.ReadInt32();
        //    global = r.ReadVector3();
        //    w = r.ReadInt32();
        //    h = r.ReadInt32();
        //    value = r.ReadBoolean();
        //}

        internal static byte[] Write(int stockpileID, Vector3 begin, Vector3 end, bool value)
        {
            return Network.Serialize(w =>
            {
                w.Write(stockpileID);
                w.Write(begin);
                w.Write(end);
                w.Write(value);
            });
        }
        internal static void Read(BinaryReader r, out int spID, out Vector3 begin, out Vector3 end, out bool value)
        {
            spID = r.ReadInt32();
            begin = r.ReadVector3();
            end = r.ReadVector3();
            value = r.ReadBoolean();
        }
    }
}
