using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Farming
{
    class PacketFarmDelete
    {
        public static void Send(IObjectProvider net, int farmID)
        {
            var w = net.GetOutgoingStream();
            w.Write(PacketType.FarmDelete);
            w.Write(farmID);
        }
        public static void Receive(IObjectProvider net, BinaryReader r)
        {
            int farmid = r.ReadInt32();
            //var farm = net.Map.Town.FarmingManager.GrowZones[farmid];
            net.Map.Town.FarmingManager.RemoveFarm(farmid);
            if (net is Server)
                Send(net, farmid);
        }
    }
}
