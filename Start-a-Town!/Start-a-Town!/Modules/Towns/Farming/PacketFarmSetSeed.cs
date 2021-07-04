using System;
using Start_a_Town_.Net;

namespace Start_a_Town_.Towns.Farming
{
    [Obsolete]
    class PacketFarmSetSeed : Packet
    {
        static public void Handle(IObjectProvider net, Packet p, FarmingManager manager)
        {
          
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
