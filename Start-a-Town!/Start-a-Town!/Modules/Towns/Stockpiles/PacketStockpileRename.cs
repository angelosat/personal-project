using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns
{
    class PacketStockpileRename
    {
        static public byte[] Write(int stockpileID, string name)
        {
            return Network.Serialize(w =>
            {
                w.Write(stockpileID);
                w.Write(name);
            });
        }
        static public void Read(BinaryReader r, out int stockpileID, out string name)
        {
            stockpileID = r.ReadInt32();
            name = r.ReadString();
        }
    }
}
