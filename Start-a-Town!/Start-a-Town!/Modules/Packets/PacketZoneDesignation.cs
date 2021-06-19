using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;
using Start_a_Town_.Towns.Farming;
using Start_a_Town_.Towns.Stockpiles;

namespace Start_a_Town_.Towns
{
    class PacketZoneDesignation
    {
        static readonly int PacketPlayerZoneDesignation;
        
        static public void Send(IObjectProvider net, Type zonetype, int zoneID, Vector3 begin, int w, int h, bool remove)
        {
            var stream = net.GetOutgoingStream();
            stream.Write(PacketPlayerZoneDesignation);
            stream.Write(zonetype.FullName);
            stream.Write(zoneID);
            stream.Write(begin);
            stream.Write(w);
            stream.Write(h);
            stream.Write(remove);
        }
        static public void Receive(IObjectProvider net, BinaryReader r)
        {
            var zoneType = Type.GetType(r.ReadString());
            var zoneID = r.ReadInt32();
            var begin = r.ReadVector3();
            var width = r.ReadInt32();
            var height = r.ReadInt32();
            var remove = r.ReadBoolean();
            //ZoneNew zoneCreated = null;
            net.Map.Town.ZoneManager.PlayerEdit(zoneID, zoneType, begin, width, height, remove);

            //if (zoneType == typeof(GrowingZone))
            //{
            //    //zoneCreated = 
            //        net.Map.Town.FarmingManager.PlayerEdit(zoneID, begin, width, height, remove);
            //}
            //else if(zoneType == typeof(Stockpile))
            //{
            //    //zoneCreated =
            //        net.Map.Town.StockpileManager.PlayerEdit(zoneID, begin, width, height, remove);
            //}
            if (net is Server)
                Send(net, zoneType, zoneID, begin, width, height, remove);
                //Send(net, entityID, zoneType, zoneCreated != null ? zoneCreated.ID : zoneID, begin, width, height, remove);
        }
        static PacketZoneDesignation()
        {
            PacketPlayerZoneDesignation = Network.RegisterPacketHandler(Receive);
        }
        static public void Init()
        {
        }
    }
}
