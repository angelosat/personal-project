using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketOrderAdd
    {
        static internal void Init()
        {
            // TODO
            Server.RegisterPacketHandler(PacketType.CraftingOrderPlaceNew, Receive);
            Client.RegisterPacketHandler(PacketType.CraftingOrderPlaceNew, Receive);
        }

        internal static void Send(IObjectProvider net, Vector3 global, int reactionID)
        {
            var w = net.GetOutgoingStream();
            w.Write(PacketType.CraftingOrderPlaceNew);
            w.Write(global);
            w.Write(reactionID);
        }
        private static void Receive(IObjectProvider net, BinaryReader r)
        {
            var station = r.ReadVector3();
            var reactionID = r.ReadInt32();
            net.Map.Town.CraftingManager.AddOrder(station, reactionID);
            if (net is Server)
                Send(net, station, reactionID);
        }
    }
}
