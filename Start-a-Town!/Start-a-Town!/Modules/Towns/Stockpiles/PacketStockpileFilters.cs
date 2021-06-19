using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns
{
    class PacketStockpileFilters : Packet
    {
        public int StockpileID;
        public Dictionary<string, bool> Changes = new Dictionary<string, bool>();
        public PacketStockpileFilters(IObjectProvider net, System.IO.BinaryReader r)
        {
            Read(net, r);
        }
        public PacketStockpileFilters(int stockpileID)
        {
            this.StockpileID = stockpileID;
        }
        public void Add(string filter, bool value)
        {
            this.Changes.Add(filter, value);
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            w.Write(this.StockpileID);
            w.Write(this.Changes.Count);
            foreach(var f in this.Changes)
            {
                w.Write(f.Key);
                w.Write(f.Value);
            }
        }
        public override void Read(IObjectProvider net, System.IO.BinaryReader r)
        {
            this.StockpileID = r.ReadInt32();
            var count = r.ReadInt32();
            for (int i = 0; i < count; i++)
                this.Changes.Add(r.ReadString(), r.ReadBoolean());
        }

        static public void Handle(IObjectProvider net, System.IO.BinaryReader r, Packet packet)
        {
            PacketStockpileFilters p = new PacketStockpileFilters(net, r);
            var stockpile = net.Map.GetTown().StockpileManager.Stockpiles[p.StockpileID];
            //foreach (var f in p.Changes)
            //    stockpile.FilterToggle(f.Value, f.Key);
            net.Map.EventOccured(Components.Message.Types.StockpileUpdated, stockpile);
            var server = net as Server;
            if (server != null)
                server.Enqueue(PacketType.StockpileFilters, packet.Payload, SendType.OrderedReliable, true);
        }
    }
}
