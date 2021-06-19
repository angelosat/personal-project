using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Stockpiles
{
    class PacketStockpileDelete
    {
        static public byte[] Write(int senderid, int stockpileid)
        {
            return Network.Serialize(w =>
                {
                    w.Write(senderid);
                    w.Write(stockpileid);
                });
        }
        static public void Read(BinaryReader r, out int senderid, out int stockpileid)
        {
            senderid = r.ReadInt32();
            stockpileid = r.ReadInt32();
        }
    }
}
