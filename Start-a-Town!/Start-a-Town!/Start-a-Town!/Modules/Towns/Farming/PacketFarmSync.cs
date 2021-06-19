using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Farming
{
    class PacketFarmSync
    {
        public static byte[] Write(int id, string name, int seedID, bool harvesting, bool planting)
        {
            return Network.Serialize(w =>
            {
                w.Write(id);
                w.Write(name);
                w.Write(seedID);
                w.Write(harvesting);
                w.Write(planting);
            });
        }

        public static void Read(BinaryReader r, out int id, out string name, out int seedID, out bool harvesting, out bool planting)
        {
            id = r.ReadInt32();
            name = r.ReadString();
            seedID = r.ReadInt32();
            harvesting = r.ReadBoolean();
            planting = r.ReadBoolean();
        }

        //internal static byte[] Write(Farmland farmland)
        //{
        //    return Write(farmland.ID, farmland.Name, farmland.GetSeedID(), farmland.Harvesting, farmland.Planting);
        //}
    }
}
