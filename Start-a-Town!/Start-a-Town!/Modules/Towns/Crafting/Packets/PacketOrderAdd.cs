using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketOrderAdd
    {
        static int p;
        static internal void Init()
        {
            // TODO
            p = Network.RegisterPacketHandler(Receive);
        }

        internal static void Send(IObjectProvider net, Vector3 global, int reactionID)
        {
            var w = net.GetOutgoingStream();
            w.Write(p);
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
