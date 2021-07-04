﻿using System;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns
{
    public partial class WorkplaceManager
    {
        private class Packets
        {
            static int PacketPlayerCreateShop, PacketPlayerDeleteShop, PacketPlayerAddStockpileToShop, PacketPlayerAssignWorkerToShop, PacketPlayerShopAssignCounter;

            static public void Init()
            {
                PacketPlayerCreateShop = Network.RegisterPacketHandler(ReceivePlayerCreateShop);
                PacketPlayerDeleteShop = Network.RegisterPacketHandler(ReceivePlayerDeleteShop);
                PacketPlayerAddStockpileToShop = Network.RegisterPacketHandler(ReceivePlayerAddStockpileToShop);
                PacketPlayerAssignWorkerToShop = Network.RegisterPacketHandler(HandlePlayerAssignWorkerToShop);
                PacketPlayerShopAssignCounter = Network.RegisterPacketHandler(ReceivePlayerShopAssignCounter);
            }
            public static void SendPlayerDeleteShop(IObjectProvider net, PlayerData player, int shopid)
            {
                if(net is Server)
                {
                    net.Map.Town.ShopManager.RemoveShop(shopid);
                }
                net.GetOutgoingStream().Write(PacketPlayerDeleteShop, player.ID, shopid);
            }
            private static void ReceivePlayerDeleteShop(IObjectProvider net, BinaryReader r)
            {
                var pl = net.GetPlayer(r.ReadInt32());
                var shopid = r.ReadInt32();
                if (net is Client)
                    net.Map.Town.ShopManager.RemoveShop(shopid);
                else
                    SendPlayerDeleteShop(net, pl, shopid);
            }

            static public void SendPlayerShopAssignCounter(IObjectProvider net, PlayerData player, Workplace shop, IntVec3 global)
            {
                var w = net.GetOutgoingStream();
                w.Write(PacketPlayerShopAssignCounter);
                w.Write(player.ID);
                w.Write(shop?.ID ?? -1);
                w.Write(global);
            }
            static void ReceivePlayerShopAssignCounter(IObjectProvider net, BinaryReader r)
            {
                var player = net.GetPlayer(r.ReadInt32());
                var manager = net.Map.Town.ShopManager;
                var shop = manager.GetShop(r.ReadInt32());
                var global = r.ReadIntVec3();
                if (shop != null)
                {
                    if (global.Z < 0)
                        throw new NotImplementedException();
                    shop.AddFacility(global);
                }
                else
                {
                    throw new NotImplementedException();
                }
                if (net is Server)
                    SendPlayerShopAssignCounter(net, player, shop, global);
            }

            static public void SendPlayerAssignWorkerToShop(IObjectProvider net, PlayerData player, Actor actor, Workplace shop)
            {
                var w = net.GetOutgoingStream();
                w.Write(PacketPlayerAssignWorkerToShop);
                w.Write(player.ID);
                w.Write(actor.RefID);
                w.Write(shop.ID);
            }
            private static void HandlePlayerAssignWorkerToShop(IObjectProvider net, BinaryReader r)
            {
                var playerID = r.ReadInt32();
                var actorID = r.ReadInt32();
                var shopID = r.ReadInt32();
                var manager = net.Map.Town.ShopManager;
                var actor = net.GetNetworkObject(actorID) as Actor;
                var shop = manager.GetShop(shopID);
                shop.AddWorker(actor);
                if (net is Server)
                    SendPlayerAssignWorkerToShop(net, net.GetPlayer(playerID), actor, shop);
            }

            static public void SendPlayerAddStockpileToShop(IObjectProvider net, int playerID, int shopID, int stockpileID)
            {
                if (shopID < 0)
                    return;
                var w = net.GetOutgoingStream();
                w.Write(PacketPlayerAddStockpileToShop);
                w.Write(playerID);
                w.Write(shopID);
                w.Write(stockpileID);
            }
            private static void ReceivePlayerAddStockpileToShop(IObjectProvider net, BinaryReader r)
            {
                var playerID = r.ReadInt32();
                var shopid = r.ReadInt32();
                var stockpileid = r.ReadInt32();
                var shopmanager = net.Map.Town.ShopManager;
                var stockpilemanager = net.Map.Town.StockpileManager;
                var stockpile = stockpilemanager.GetStockpile(stockpileid);
                var shop = shopmanager.GetShop(shopid) as Shop;
                shop.AddStockpile(stockpile);

                if (net is Server)
                    SendPlayerAddStockpileToShop(net, playerID, shopid, stockpileid);
            }

            static public void SendPlayerCreateShop(IObjectProvider net, int playerID, Type shopType, int shopID = 0)
            {
                var w = net.GetOutgoingStream();
                w.Write(PacketPlayerCreateShop);
                w.Write(playerID);
                w.Write(shopType.FullName);
                w.Write(shopID);
            }
            private static void ReceivePlayerCreateShop(IObjectProvider net, BinaryReader r)
            {
                var playerID = r.ReadInt32();
                var shoptypename = r.ReadString();

                var shopid = r.ReadInt32();
                var manager = net.Map.Town.ShopManager;

                if (net is Client)
                    manager.CurrentShopID = shopid;
                var shoptype = Type.GetType(shoptypename);
                var id = manager.GetNextShopID();
                var workplace = Activator.CreateInstance(shoptype, manager, id) as Workplace;
                manager.AddShop(workplace);
                if (net is Server)
                    SendPlayerCreateShop(net, playerID, shoptype, workplace.ID);
            }
        }
         
    }
}
