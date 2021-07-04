using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.Modules.Crafting
{
    class PacketCraftingOrderSync
    {
        static internal void Init()
        {
            // TODO
            Server.RegisterPacketHandler(PacketType.CraftingOrderSync, Receive);
            Client.RegisterPacketHandler(PacketType.CraftingOrderSync, Receive);
        }

        internal static void Send(CraftOrderNew order)
        {
            var net = order.Map.Net;
            var w = net.GetOutgoingStream();
            var bench = order.Workstation;
            w.Write(PacketType.CraftingOrderSync);
            w.Write(bench);
            w.Write(order.GetIndex());
            order.Write(w);
        }
        private static void Receive(IObjectProvider net, BinaryReader r)
        {
            var station = r.ReadVector3();
            var index = r.ReadInt32();
            var order = new CraftOrderNew(net.Map, r);
            var existing = net.Map.Town.CraftingManager.GetOrder(station, index);
            existing.HaulOnFinish = order.HaulOnFinish;
            existing.FinishMode = order.FinishMode;
            net.Map.EventOccured(Components.Message.Types.OrderParametersChanged, existing);
            if (net is Server)
                Send(existing);
        }
    }
}
