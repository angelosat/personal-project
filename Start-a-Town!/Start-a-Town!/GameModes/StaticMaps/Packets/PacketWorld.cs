using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes.StaticMaps;
namespace Start_a_Town_
{
    class PacketWorld
    {
        internal static void Init()
        {
            Client.RegisterPacketHandler(PacketType.WorldInfo, Receive);
        }
        internal static void Send(IObjectProvider net, PlayerData player)
        {
            var server = net as Server;
            byte[] data = Network.Serialize(server.Map.World.WriteData);

            if (player == null)
            {
                //server.Players.GetList().ForEach(p => server.Enqueue(p, Packet.Create(player, PacketType.WorldInfo, data, SendType.Ordered | SendType.Reliable)));
                foreach (var p in server.Players.GetList())
                    server.Enqueue(p, Packet.Create(player, PacketType.WorldInfo, data, SendType.OrderedReliable));// SendType.Ordered | SendType.Reliable));
            }
            else
                server.Enqueue(player, Packet.Create(player, PacketType.WorldInfo, data, SendType.OrderedReliable));// SendType.Ordered | SendType.Reliable));
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            var client = net as Client;
            if (client.World != null)
            {
                throw new Exception("world already received");
                //"world already received, dropping packet".ToConsole();
            }
            StaticWorld world = StaticWorld.ReadData(r);// Network.Deserialize<StaticWorld>(msg.Payload, StaticWorld.ReadData);// as World;
            client.World = world;
        }
    }
}
