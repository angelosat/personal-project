using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Housing
{
    class PacketResidenceAdd
    {
        public static byte[] Write(int playerID, int residenceID, Vector3 global, int width, int height, bool remove)
        {
            return Network.Serialize(w =>
            {
                w.Write(playerID);
                w.Write(residenceID);
                w.Write(global);
                w.Write(width);
                w.Write(height);
                w.Write(remove);
            });
        }
        public static void Read(BinaryReader r, out int playerID, out int residenceID, out Vector3 global, out int width, out int height, out bool remove)
        {
            playerID = r.ReadInt32();
            residenceID = r.ReadInt32();
            global = r.ReadVector3();
            width = r.ReadInt32(); ;
            height = r.ReadInt32();
            remove = r.ReadBoolean();
        }
    }
}
