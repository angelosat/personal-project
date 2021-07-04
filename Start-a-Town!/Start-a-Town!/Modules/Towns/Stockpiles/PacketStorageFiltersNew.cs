using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketStorageFiltersNew : Packet
    {
        static internal void Init()
        {
            // TODO
            Server.RegisterPacketHandler(PacketType.StorageFiltersNew, Receive);
            Client.RegisterPacketHandler(PacketType.StorageFiltersNew, Receive);
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
            var stockpile = net.Map.Town.ZoneManager.GetZone<Stockpile>(stockpileID);

            stockpile.ToggleItemFiltersCategories(nodes);
            stockpile.ToggleItemFilters(items);
            if (net is Server)
                Send(stockpile, nodes, items);
        }
    }
}
