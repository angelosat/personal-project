using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Forestry
{
    class PacketChoppingDesignation
    {
        static public byte[] Write(int entityID, Vector3 start, Vector3 end, bool value)
        {
            return Network.Serialize(w =>
            {
                w.Write(entityID);
                w.Write(start);
                w.Write(end);
                w.Write(value);
            });
        }
        static public void Read(BinaryReader r, out int entityID, out Vector3 start, out Vector3 end, out bool value)
        {
            entityID = r.ReadInt32();
            start = r.ReadVector3();
            end = r.ReadVector3();
            value = r.ReadBoolean();
        }
    }
}
