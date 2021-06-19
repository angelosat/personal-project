using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Start_a_Town_.Towns;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class PacketStorageFiltersNew : Packet
    {
        static internal void Init()
        {
            Net.Server.RegisterPacketHandler(PacketType.StorageFiltersNew, Receive);
            Net.Client.RegisterPacketHandler(PacketType.StorageFiltersNew, Receive);
        }
        public static void Send(Stockpile stockpile, int[] nodeIndices = null, int[] leafIndices = null)
        {
            var s = stockpile.Map.Net.GetOutgoingStream();
            s.Write(PacketType.StorageFiltersNew);
            s.Write(stockpile.ID);
            s.Write(nodeIndices ?? new int[] { });
            s.Write(leafIndices ?? new int[] { });
        }
        static void Receive(IObjectProvider net, BinaryReader r)
        {
            var stockpileID = r.ReadInt32();
            var nodes = r.ReadIntArray();
            var items = r.ReadIntArray();
            var stockpile = net.Map.Town.StockpileManager.Stockpiles[stockpileID];
            stockpile.ToggleItemFiltersCategories(nodes);
            stockpile.ToggleItemFilters(items);
            if (net is Server)
                Send(stockpile, nodes, items);
        }
        //internal static void Send(IMap map, Vector3 blockEntity, int[] filtersIndices)
        //{
        //    var w = map.Net.GetOutgoingStream();
        //    w.Write(PacketType.StorageFiltersNew);
        //    w.Write(blockEntity);
        //    w.Write(filtersIndices);
        //}
        //private static void Receive(IObjectProvider net, BinaryReader r)
        //{
        //    var storageGlobal = r.ReadVector3();
        //    var filters = r.ReadIntArray();
        //    throw new NotImplementedException();
        //}
    }
}
