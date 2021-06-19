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
using Start_a_Town_.Modules.Crafting;

namespace Start_a_Town_
{
    class PacketOrderRemove
    {
        static internal void Init()
        {
            Net.Server.RegisterPacketHandler(PacketType.CraftingOrderRemoveNew, Receive);
            Net.Client.RegisterPacketHandler(PacketType.CraftingOrderRemoveNew, Receive);
        }
        internal static void Send(IObjectProvider net, CraftOrderNew order)
        {
            var w = net.GetOutgoingStream();
            w.Write(PacketType.CraftingOrderRemoveNew);
            w.Write(order.Workstation);
            //w.Write(orderIndex);
            w.Write(order.GetUniqueLoadID());
        }
        internal static void Send(IObjectProvider net, Vector3 global, string orderID)
        {
            var w = net.GetOutgoingStream();
            w.Write(PacketType.CraftingOrderRemoveNew);
            w.Write(global);
            //w.Write(orderIndex);
            w.Write(orderID);
        }
        internal static void SendOld(IObjectProvider net, Vector3 global, int orderIndex)
        {
            var w = net.GetOutgoingStream();
            w.Write(PacketType.CraftingOrderRemoveNew);
            w.Write(global);
            w.Write(orderIndex);
        }
        private static void Receive(IObjectProvider net, BinaryReader r)
        {
            var station = r.ReadVector3();
            //var orderIndex = r.ReadInt32();
            var orderID = r.ReadString();
            //net.Map.Town.CraftingManager.RemoveOrder(station, orderIndex);
            if(net.Map.Town.CraftingManager.RemoveOrder(station, orderID))
                if (net is Server)
                    Send(net, station, orderID);
            //if (net is Server)
            //    Send(net, station, orderIndex);
        }
    }
}
