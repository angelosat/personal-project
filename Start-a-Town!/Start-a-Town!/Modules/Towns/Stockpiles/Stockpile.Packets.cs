﻿using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public partial class Stockpile
    {
        class Packets
        {
            static int PacketStockpileSync;
            static internal void Init()
            {
                PacketStockpileSync = Network.RegisterPacketHandler(Receive);
            }
            internal static void SyncPriority(Stockpile stockpile, StoragePriority p)
            {
                var net = stockpile.Map.Net;
                if (net is Server)
                    stockpile.Settings.Priority = p;
                var w = net.GetOutgoingStream();
                w.Write(PacketStockpileSync);
                w.Write(stockpile.ID);
                w.Write(p.Value);
            }
            private static void Receive(IObjectProvider net, BinaryReader r)
            {
                var stockpileID = r.ReadInt32();
                var p = r.ReadInt32();
                var stockpile = net.Map.Town.ZoneManager.GetZone<Stockpile>(stockpileID);

                var newPriority = StoragePriority.GetFromValue(p);
                if (net is Server)
                    SyncPriority(stockpile, newPriority);
                else
                    stockpile.Settings.Priority = newPriority;
            }
        }
    }
}
