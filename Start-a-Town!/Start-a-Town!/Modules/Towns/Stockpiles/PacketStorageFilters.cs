using System.Collections.Generic;
using System.Linq;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketStorageFilters : Packet
    {
        static int p;
        static internal void Init()
        {
            // TODO
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(IStorage storage, StorageFilter[] filters)
        {
            var w = storage.Map.Net.GetOutgoingStream();
            w.Write(p);
            w.Write(storage.ID);
            w.Write(filters.Select(r => r.Name).ToArray());
        }
        private static void Receive(IObjectProvider net, BinaryReader r)
        {
            var storageID = r.ReadInt32();
            var filters = r.ReadStringArray().Select(s => Def.GetDef<StorageFilter>(s)).ToArray();
            var stockpile = net.Map.Town.StockpileManager.Stockpiles[storageID];
            stockpile.FilterToggle(filters);
            if (net is Server server)
                Send(stockpile, filters);
        }
    }
}
