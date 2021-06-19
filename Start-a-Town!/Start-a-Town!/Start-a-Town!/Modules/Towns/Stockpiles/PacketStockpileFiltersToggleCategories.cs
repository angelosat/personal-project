using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Stockpiles
{
    class PacketStockpileFiltersToggleCategories : Packet
    {
        static public byte[] Write(int stockpileID, Dictionary<string, bool> changes)
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
        static public void Read(BinaryReader r, out int stockpileID, out Dictionary<string, bool> changes)
        {
            stockpileID = r.ReadInt32();
            changes = new Dictionary<string, bool>();
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                changes.Add(r.ReadString(), r.ReadBoolean());
        }

        static public void Handle(IObjectProvider net, System.IO.BinaryReader r, Packet packet)
        {
            int spID;
            Dictionary<string, bool> changes;
            Read(r, out spID, out changes);
            var stockpile = net.Map.Town.Stockpiles[spID];
            foreach (var f in changes)
                stockpile.FilterCategoryToggle(f.Value, f.Key);
            net.Map.EventOccured(Components.Message.Types.StockpileUpdated, stockpile);
            var server = net as Server;
            if (server != null)
                server.Enqueue(PacketType.StockpileFilters, packet.Payload, SendType.OrderedReliable, true);
        }
    }
}
