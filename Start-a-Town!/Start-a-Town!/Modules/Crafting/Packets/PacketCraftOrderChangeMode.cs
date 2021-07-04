using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketCraftOrderChangeMode
    {
        static internal void Init()
        {
            // TODO
            Server.RegisterPacketHandler(PacketType.PacketCraftOrderChangeMode, Receive);
            Client.RegisterPacketHandler(PacketType.PacketCraftOrderChangeMode, Receive);
        }

        internal static void Send(CraftOrderNew order, int value)
        {
            var net = order.Map.Net;
            var w = net.GetOutgoingStream();
            var bench = order.Workstation;
            w.Write(PacketType.PacketCraftOrderChangeMode);
            w.Write(bench);
            w.Write(order.GetUniqueLoadID());
            w.Write(value);
        }
        private static void Receive(IObjectProvider net, BinaryReader r)
        {
            var station = r.ReadVector3();
            var index = r.ReadString();
            var bench = net.Map.Town.CraftingManager.GetWorkstation(station);
            var order = bench.GetOrder(index);
            order.FinishMode = CraftOrderFinishMode.GetMode(r.ReadInt32());
            net.Map.EventOccured(Components.Message.Types.OrderParametersChanged, order);
            if (net is Server)
                Send(order, (int)order.FinishMode.Mode);
        }
    }
}
