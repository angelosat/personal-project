using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Start_a_Town_.Towns;
using System.IO;

namespace Start_a_Town_
{
    class PacketStockpileSync
    {
        static int PacketStockpileSyncPriority;
        static internal void Init()
        {
            PacketStockpileSyncPriority = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(Stockpile stockpile, StoragePriority p)
        {
            var net = stockpile.Map.Net;
            if(net is Server)
                stockpile.Settings.Priority = p;
            var w = net.GetOutgoingStream();
            w.Write(PacketStockpileSyncPriority);
            w.Write(stockpile.ID);
            //w.Write(stockpile.Settings.Priority.Value);
            w.Write(p.Value);
        }
        private static void Receive(IObjectProvider net, BinaryReader r)
        {
            var stockpileID = r.ReadInt32();
            var p = r.ReadInt32();
            var stockpile = net.Map.Town.StockpileManager.Stockpiles[stockpileID];
            net.Map.EventOccured(Components.Message.Types.StockpileUpdated, stockpile);
            if (net is Server)
                Send(stockpile, stockpile.Settings.Priority);
            else
                stockpile.Settings.Priority = StoragePriority.GetFromValue(p);
        }
    }
}
