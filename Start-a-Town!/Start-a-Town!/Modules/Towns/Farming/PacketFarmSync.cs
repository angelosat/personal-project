using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Farming
{
    class PacketFarmSync
    {
        //public static byte[] Write(int id, string name, int seedID, bool harvesting, bool planting)
        //{
        //    return Network.Serialize(w =>
        //    {
        //        w.Write(id);
        //        w.Write(name);
        //        w.Write(seedID);
        //        w.Write(harvesting);
        //        w.Write(planting);
        //    });
        //}

        //public static void Read(BinaryReader r, out int id, out string name, out int seedID, out bool harvesting, out bool planting)
        //{
        //    id = r.ReadInt32();
        //    name = r.ReadString();
        //    seedID = r.ReadInt32();
        //    harvesting = r.ReadBoolean();
        //    planting = r.ReadBoolean();
        //}

        //public static void Send(IObjectProvider net, int id, string name, int seedID, bool harvesting, bool planting)
        //{
        //    var w = net.GetOutgoingStream();
        //    w.Write(PacketType.FarmSync);
        //    w.Write(id);
        //    w.Write(name);
        //    w.Write(seedID);
        //    w.Write(harvesting);
        //    w.Write(planting);
        //}
        //public static void Receive(IObjectProvider net, BinaryReader r)
        //{
        //    int id, seedID;
        //    bool harvesting, planting;
        //    string name;
        //    PacketFarmSync.Read(r, out id, out name, out seedID, out harvesting, out planting);
        //    var manager = net.Map.Town.FarmingManager;
        //    var farm = manager.GrowZones[id];
        //    farm.SeedType = seedID == -1 ? null : GameObject.Objects[seedID];
        //    farm.SetHarvesting(harvesting);
        //    farm.SetSowing(planting);
        //    farm.Name = name;
        //    net.EventOccured(Components.Message.Types.FarmUpdated, farm);
        //    if (net is Server)
        //        Send(net, id, name, seedID, harvesting, planting);
        //}
        ////internal static byte[] Write(Farmland farmland)
        ////{
        ////    return Write(farmland.ID, farmland.Name, farmland.GetSeedID(), farmland.Harvesting, farmland.Planting);
        ////}
        //static public void Init()
        //{
        //    Server.RegisterPacketHandler(PacketType.FarmSync, Receive);
        //    Client.RegisterPacketHandler(PacketType.FarmSync, Receive);
        //}
    }
}
