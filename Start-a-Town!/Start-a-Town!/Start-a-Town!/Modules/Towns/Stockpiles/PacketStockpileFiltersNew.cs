using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Stockpiles
{
    class PacketStockpileFiltersNew : Packet
    {
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
            var stockpile = net.Map.Town.Stockpiles[spID];
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
