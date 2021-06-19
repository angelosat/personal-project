using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Start_a_Town_.Towns;

namespace Start_a_Town_
{
    class PacketStockpileFiltersNew : Packet
    {

        static internal void Init()
        {
            Net.Server.RegisterPacketHandler(PacketType.StockpileFilters, Receive);
            Net.Client.RegisterPacketHandler(PacketType.StockpileFilters, Receive);
        }

        //internal static void Send(IObjectProvider net, int stockpileID, Dictionary<string, bool> changes)
        internal static void Send(Stockpile stockpile, Dictionary<string, bool> changes)
        {
            var w = stockpile.Map.Net.GetOutgoingStream();
            w.Write(PacketType.StockpileFilters);
            w.Write(stockpile.ID);
            w.Write(changes.Count);
            foreach (var item in changes)
            {
                w.Write(item.Key);
                w.Write(item.Value);
            }
        }
        private static void Receive(IObjectProvider net, BinaryReader r)
        {
            var stockpileID = r.ReadInt32();
            var changes = new Dictionary<string, bool>();
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                changes.Add(r.ReadString(), r.ReadBoolean());

            var stockpile = net.Map.Town.StockpileManager.Stockpiles[stockpileID];
            foreach (var f in changes)
                stockpile.FilterToggle(f.Value, ItemCategory.Find(f.Key));
            net.Map.EventOccured(Components.Message.Types.StockpileUpdated, stockpile);
            var server = net as Server;
            if (server != null)
                Send(stockpile, changes);
        }
        //static public byte[] Write(int stockpileID, bool toggle, List<int> changes)
        //{
        //    return Network.Serialize(w =>
        //    {
        //        w.Write(stockpileID);
        //        w.Write(toggle);
        //        w.Write(changes.Count);
        //        foreach (var item in changes)
        //        {
        //            //w.Write(item.Key);
        //            //w.Write(item.Value);
        //            w.Write(item);
        //        }
        //    });
        //}
        //static public void Read(BinaryReader r, out int stockpileID, out bool toggle, out List<int> changes)
        //{
        //    stockpileID = r.ReadInt32();
        //    toggle = r.ReadBoolean();
        //    changes = new List<int>();// Dictionary<int, bool>();
        //    var count = r.ReadInt32();
        //    for (int i = 0; i < count; i++)
        //        //changes.Add(r.ReadInt32(), r.ReadBoolean());
        //        changes.Add(r.ReadInt32());
        //}
        static public byte[] Write(int stockpileID, Dictionary<int, bool> changes)
        {
            return Network.Serialize(w =>
            {
                w.Write(stockpileID);
                w.Write(changes.Count);
                foreach (var item in changes)
                {
                    w.Write(item.Key);
                    w.Write(item.Value);
                }
            });
        }
        static public void Read(BinaryReader r, out int stockpileID, out Dictionary<int, bool> changes)
        {
            stockpileID = r.ReadInt32();
            changes = new Dictionary<int, bool>();
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                changes.Add(r.ReadInt32(), r.ReadBoolean());
        }

        static public void Handle(IObjectProvider net, System.IO.BinaryReader r, Packet packet)
        {
            int spID;
            Dictionary<int, bool> changes;
            //bool toggle;
            //List<int> changes;
            //Read(r, out spID, out toggle, out changes);
            Read(r, out spID, out changes);
            var stockpile = net.Map.Town.StockpileManager.Stockpiles[spID];
            //stockpile.FilterToggle(toggle, changes.ToArray());
            foreach (var f in changes)
                stockpile.FilterToggle(f.Value, f.Key);
            net.Map.EventOccured(Components.Message.Types.StockpileUpdated, stockpile);
            var server = net as Server;
            if (server != null)
                server.Enqueue(PacketType.StockpileFilters, packet.Payload, SendType.OrderedReliable, true);

            //PacketStockpileFilters p = new PacketStockpileFilters(net, r);
            //var stockpile = net.Map.GetTown().Stockpiles[p.StockpileID];
            //foreach (var f in p.Changes)
            //    stockpile.FilterToggle(f.Value, f.Key);
            //net.Map.EventOccured(Components.Message.Types.StockpileUpdated, stockpile);
            //var server = net as Server;
            //if (server != null)
            //    server.Enqueue(PacketType.StockpileFilters, packet.Payload, SendType.OrderedReliable, true);
        }
    }
}
