using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    class PacketStorageFiltersNew : Packet
    {
        static readonly int p, pNew, pCategory, pVariation;
        static PacketStorageFiltersNew()
        {
            p = Network.RegisterPacketHandler(Receive);
            pNew = Network.RegisterPacketHandler(ReceiveNew);
            pCategory = Network.RegisterPacketHandler(ReceiveCategory);
            pVariation = Network.RegisterPacketHandler(ReceiveVariation);
        }
        public static void Send(IStorageNew storage, ItemDef item, Def v)
        {
            var stockpile = storage as Stockpile;
            var s = stockpile.Map.Net.GetOutgoingStream();
            s.Write(pVariation);
            s.Write(stockpile.ID);
            s.Write(item.Name);
            s.Write(v.Name);
        }
        private static void ReceiveVariation(INetwork net, BinaryReader r)
        {
            var stockpileID = r.ReadInt32();
            var stockpile = net.Map.Town.ZoneManager.GetZone<Stockpile>(stockpileID);
            var item = Def.GetDef<ItemDef>(r);
            var v = Def.GetDef(r.ReadString());
            stockpile.ToggleFilter(item, v);
            if (net is Server)
                Send(stockpile, item, v);
        }

        public static void Send(IStorageNew storage, ItemCategory category)
        {
            var stockpile = storage as Stockpile;
            var s = stockpile.Map.Net.GetOutgoingStream();
            s.Write(pCategory);
            s.Write(stockpile.ID);
            s.Write(category?.Name ?? "");
        }
        private static void ReceiveCategory(INetwork net, BinaryReader r)
        {
            var stockpileID = r.ReadInt32();
            var stockpile = net.Map.Town.ZoneManager.GetZone<Stockpile>(stockpileID);
            var cat = r.ReadString() is string catName && !catName.IsNullEmptyOrWhiteSpace() ? Def.GetDef<ItemCategory>(catName) : null;
            stockpile.ToggleFilter(cat);
            if (net is Server)
                Send(stockpile, cat);
        }

        public static void Send(IStorageNew storage, ItemDef item, MaterialDef mat)
        {
            var stockpile = storage as Stockpile;
            var s = stockpile.Map.Net.GetOutgoingStream();
            s.Write(pNew);
            s.Write(stockpile.ID);
            s.Write(item.Name);
            s.Write(mat?.Name ?? "");
        }
        private static void ReceiveNew(INetwork net, BinaryReader r)
        {
            var stockpileID = r.ReadInt32();
            var stockpile = net.Map.Town.ZoneManager.GetZone<Stockpile>(stockpileID);
            var item = Def.GetDef<ItemDef>(r);
            MaterialDef mat = null;
            if (r.ReadString() is string matName && !matName.IsNullEmptyOrWhiteSpace())
            {
                mat = Def.GetDef<MaterialDef>(matName);
                stockpile.ToggleFilter(item, mat);
            }
            else
                stockpile.ToggleFilter(item);
            if (net is Server)
                Send(stockpile, item, mat);
        }

        public static void Send(Stockpile stockpile, int[] nodeIndices = null, int[] leafIndices = null)
        {
            var s = stockpile.Map.Net.GetOutgoingStream();
            s.Write(p);
            s.Write(stockpile.ID);
            s.Write(nodeIndices ?? new int[] { });
            s.Write(leafIndices ?? new int[] { });
        }
        static void Receive(INetwork net, BinaryReader r)
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
