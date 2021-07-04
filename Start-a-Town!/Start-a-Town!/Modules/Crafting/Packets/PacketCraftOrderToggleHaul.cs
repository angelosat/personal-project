using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.Crafting;

namespace Start_a_Town_.Modules.Crafting
{
    class PacketCraftOrderToggleHaul
    {
        static internal void Init()
        {
            // TODO
            Server.RegisterPacketHandler(PacketType.PacketCraftOrderToggleHaul, Receive);
            Client.RegisterPacketHandler(PacketType.PacketCraftOrderToggleHaul, Receive);
        }

        internal static void Send(CraftOrderNew order, bool value)
        {
            var net = order.Map.Net;
            var w = net.GetOutgoingStream();
            var bench = order.Workstation;
            w.Write(PacketType.PacketCraftOrderToggleHaul);
            w.Write(bench);
            w.Write(order.ID);
            w.Write(value);
        }
        private static void Receive(IObjectProvider net, BinaryReader r)
        {
            var station = r.ReadVector3();
            var id = r.ReadInt32();
            var order = net.Map.GetBlockEntity<BlockEntityWorkstation>(station).GetOrder(id);
            order.HaulOnFinish = r.ReadBoolean();
            net.Map.EventOccured(Components.Message.Types.OrderParametersChanged, order);
            if (net is Server)
                Send(order, order.HaulOnFinish);
        }
    }
}
