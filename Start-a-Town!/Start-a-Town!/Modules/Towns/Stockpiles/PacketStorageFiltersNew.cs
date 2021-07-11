using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketStorageFiltersNew : Packet
    {
        static readonly int p;
        static PacketStorageFiltersNew()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        static internal void Init()
        {
        }
        public static void Send(Stockpile stockpile, int[] nodeIndices = null, int[] leafIndices = null)
        {
            var s = stockpile.Map.Net.GetOutgoingStream();
            s.Write(p);
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
