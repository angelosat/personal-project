using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    [EnsureStaticCtorCall]
    class PacketCraftOrderSync
    {
        static readonly int p;
        static PacketCraftOrderSync()
        {
            p = Network.RegisterPacketHandler(Receive);
        }

        internal static void Send(CraftOrder order, Stockpile input, Stockpile output)
        {
            var net = order.Map.Net;
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(order.Workstation);
            w.Write(order.ID);
            w.Write(input?.ID ?? -1);
            w.Write(output?.ID ?? -1);
        }
        private static void Receive(INetwork net, BinaryReader r)
        {
            var station = r.ReadIntVec3();
            var index = r.ReadInt32();
            var bench = net.Map.Town.CraftingManager.GetWorkstation(station);
            var order = bench.GetOrder(index);
            var manager = net.Map.Town.ZoneManager;
            var input = r.ReadInt32() is int inputID && inputID == -1 ? null : manager.GetZone<Stockpile>(inputID);
            var output = r.ReadInt32() is int outputID && outputID == -1 ? null : manager.GetZone<Stockpile>(outputID);
            order.Input = input;
            order.Output = output;
            net.Map.EventOccured(Components.Message.Types.OrderParametersChanged, order);
            if (net is Server)
                Send(order, input, output);
        }
        //internal static void Send(CraftOrder order)
        //{
        //    var net = order.Map.Net;
        //    var w = net.GetOutgoingStream();
        //    var bench = order.Workstation;
        //    w.Write(p);
        //    w.Write(bench);
        //    w.Write(order.GetIndex());
        //    order.Write(w);
        //}
        //private static void Receive(INetwork net, BinaryReader r)
        //{
        //    var station = r.ReadVector3();
        //    var index = r.ReadInt32();
        //    var order = new CraftOrder(net.Map, r);
        //    var existing = net.Map.Town.CraftingManager.GetOrder(station, index);
        //    existing.HaulOnFinish = order.HaulOnFinish;
        //    existing.FinishMode = order.FinishMode;
        //    net.Map.EventOccured(Components.Message.Types.OrderParametersChanged, existing);
        //    if (net is Server)
        //        Send(existing);
        //}
    }
}
