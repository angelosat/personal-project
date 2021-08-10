﻿using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.Crafting;

namespace Start_a_Town_.Modules.Crafting
{
    class PacketCraftOrderToggleHaul
    {
        static int p;
        static internal void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }

        internal static void Send(CraftOrder order, bool value)
        {
            var net = order.Map.Net;
            var w = net.GetOutgoingStream();
            var bench = order.Workstation;
            w.Write(p);
            w.Write(bench);
            w.Write(order.ID);
            w.Write(value);
        }
        private static void Receive(INetwork net, BinaryReader r)
        {
            var station = r.ReadIntVec3();
            var id = r.ReadInt32();
            var order = net.Map.Town.CraftingManager.GetOrder(station, id);
            //var order = net.Map.GetBlockEntity<BlockEntityWorkstation>(station).GetOrder(id);
            order.HaulOnFinish = r.ReadBoolean();
            net.Map.EventOccured(Components.Message.Types.OrderParametersChanged, order);
            if (net is Server)
                Send(order, order.HaulOnFinish);
        }
    }
}
