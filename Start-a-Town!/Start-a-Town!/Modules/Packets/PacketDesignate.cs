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
    class PacketDesignate
    {
        internal static byte[] Write(int designatorID, int designationID, Vector3 begin, Vector3 end, bool value)
        {
            return Network.Serialize(w =>
            {
                w.Write(designatorID);
                w.Write(designationID);
                w.Write(begin);
                w.Write(end);
                w.Write(value);
            });
        }
        internal static void Read(BinaryReader r, out int designatorID, out int designationID, out Vector3 begin, out Vector3 end, out bool value)
        {
            designatorID = r.ReadInt32();
            designationID = r.ReadInt32();
            begin = r.ReadVector3();
            end = r.ReadVector3();
            value = r.ReadBoolean();
        }
    }
}
