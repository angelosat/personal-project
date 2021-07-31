using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.Modules.Crafting
{
    class PacketCraftingOrderSync
    {
        static int p;
        static internal void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }

        internal static void Send(CraftOrder order)
        {
            var net = order.Map.Net;
            var w = net.GetOutgoingStream();
            var bench = order.Workstation;
            w.Write(p);
            w.Write(bench);
            w.Write(order.GetIndex());
            order.Write(w);
        }
        private static void Receive(INetwork net, BinaryReader r)
        {
            var station = r.ReadVector3();
            var index = r.ReadInt32();
            var order = new CraftOrder(net.Map, r);
            var existing = net.Map.Town.CraftingManager.GetOrder(station, index);
            existing.HaulOnFinish = order.HaulOnFinish;
            existing.FinishMode = order.FinishMode;
            net.Map.EventOccured(Components.Message.Types.OrderParametersChanged, existing);
            if (net is Server)
                Send(existing);
        }
    }
}
