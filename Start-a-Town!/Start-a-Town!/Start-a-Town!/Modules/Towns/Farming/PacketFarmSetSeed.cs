using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Farming
{
    class PacketFarmSetSeed : Packet
    {
        static public void Handle(IObjectProvider net, Packet p, FarmingManager manager)
        {
            p.Payload.Deserialize(r =>
            {
                var entityid = r.ReadInt32();
                var farmid = r.ReadInt32();
                var seedid = r.ReadInt32();
                
                var entity = net.GetNetworkObject(entityid);
                var farm = manager.Farmlands[farmid];
                var seed = seedid == -1 ? null : GameObject.Objects[seedid];
                farm.SeedType = seed;
                //farm.SetSeed(seed.ID);
            });
            var server = net as Server;
            if (server != null)
                server.Enqueue(PacketType.FarmSetSeed, p.Payload, SendType.OrderedReliable, true);
        }

        static public void Send(int entityID, int farmID, int seedID)
        {
            byte[] data = Network.Serialize(w =>
            {
                w.Write(entityID);
                w.Write(farmID);
                w.Write(seedID);
            });
            Client.Instance.Send(PacketType.FarmSetSeed, data);
        }
    }
}
