using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketOrderRemove
    {
        static int p;
        static internal void Init()
        {
            p = Network.RegisterPacketHandler(Receive);
        }
        internal static void Send(INetwork net, CraftOrder order)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(order.Workstation);
            w.Write(order.ID);
        }
        internal static void Send(INetwork net, Vector3 global, string orderID)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(global);
            w.Write(orderID);
        }
        internal static void SendOld(INetwork net, Vector3 global, int orderIndex)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(global);
            w.Write(orderIndex);
        }
        private static void Receive(INetwork net, BinaryReader r)
        {
            var station = r.ReadIntVec3();
            var orderID = r.ReadInt32();// r.ReadString();
            if(net.Map.Town.CraftingManager.RemoveOrder(station, orderID) is CraftOrder order)
                if (net is Server)
                    Send(net, order);
        }
    }
}
