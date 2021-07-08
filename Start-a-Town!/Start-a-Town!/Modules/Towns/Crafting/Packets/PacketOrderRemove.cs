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
        internal static void Send(IObjectProvider net, CraftOrderNew order)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(order.Workstation);
            w.Write(order.GetUniqueLoadID());
        }
        internal static void Send(IObjectProvider net, Vector3 global, string orderID)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(global);
            w.Write(orderID);
        }
        internal static void SendOld(IObjectProvider net, Vector3 global, int orderIndex)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
            w.Write(global);
            w.Write(orderIndex);
        }
        private static void Receive(IObjectProvider net, BinaryReader r)
        {
            var station = r.ReadVector3();
            var orderID = r.ReadString();
            if(net.Map.Town.CraftingManager.RemoveOrder(station, orderID))
                if (net is Server)
                    Send(net, station, orderID);
        }
    }
}
