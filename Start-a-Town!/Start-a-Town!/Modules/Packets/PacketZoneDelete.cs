using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.Towns;
using Start_a_Town_.Towns.Farming;

namespace Start_a_Town_
{
    class PacketZoneDelete
    {
        //public const PacketType Packet = PacketType.ZoneDelete;
        static readonly int PacketPlayerZoneDelete;
        public static void Send(IObjectProvider net, Type zoneType, int zoneID)
        {
            var w = net.GetOutgoingStream();
            w.Write(PacketPlayerZoneDelete);
            w.Write(zoneType.FullName);
            w.Write(zoneID);
        }
        public static void Receive(IObjectProvider net, BinaryReader r)
        {
            Type zoneType = Type.GetType(r.ReadString());
            int zoneID = r.ReadInt32();
            //if (zoneType == typeof(Stockpile))
            //{
            //    net.Map.Town.StockpileManager.RemoveStockpile(zoneID);
            //}
            //else if (zoneType == typeof(GrowingZone))
            //{
            //    net.Map.Town.FarmingManager.RemoveFarm(zoneID);
            //}
            //else
            //    throw new Exception();
            net.Map.Town.ZoneManager.Delete(zoneID);
            if (net is Server)
                Send(net, zoneType, zoneID);
        }
        static PacketZoneDelete()
        {
            PacketPlayerZoneDelete = Network.RegisterPacketHandler(Receive);
        }
        static public void Init()
        {
            //Server.RegisterPacketHandler(Packet, Receive);
            //Client.RegisterPacketHandler(Packet, Receive);
        }
    }
}
