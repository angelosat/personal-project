using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;

namespace Start_a_Town_
{
    class PacketOrderAdd
    {
        static internal void Init()
        {
            Net.Server.RegisterPacketHandler(PacketType.CraftingOrderPlaceNew, Receive);
            Net.Client.RegisterPacketHandler(PacketType.CraftingOrderPlaceNew, Receive);
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
