using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns
{
    class PacketJobRemoved
    {
        static public void Write(BinaryWriter w, string jobDescription, string reason, int workerID)
        {
            w.Write(jobDescription);
            w.Write(reason);
            w.Write(workerID);
        }
        static public byte[] GetData(string jobDescription, string reason, int workerID)
        {
            return Network.Serialize(w =>
            {
                w.Write(jobDescription);
                w.Write(reason);
                w.Write(workerID);
            });
        }
        static public void Read(BinaryReader r, out string jobDescription, out string reason, out int workerid)
        {
            jobDescription = r.ReadString();
            reason = r.ReadString();
            workerid = r.ReadInt32();
        }
    }
}
