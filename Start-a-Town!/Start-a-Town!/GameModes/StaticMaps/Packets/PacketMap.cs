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
    class PacketMap
    {
        internal static void Init()
        {
            Client.RegisterPacketHandler(PacketType.MapData, Receive);
        }
        internal static void Send(IObjectProvider net, PlayerData player)
        {
            var server = net as Server;
            byte[] data = Network.Serialize(server.Map.WriteData); // why does it let me do that?
            if (player == null)
            {
                //server.Players.GetList().ForEach(p => server.Enqueue(p, Packet.Create(player, PacketType.MapData, data, SendType.Ordered | SendType.Reliable)));
                foreach (var p in server.Players.GetList())
                    server.Enqueue(p, Packet.Create(player, PacketType.MapData, data, SendType.OrderedReliable));// SendType.Ordered | SendType.Reliable);
            }
            else
                server.Enqueue(player, Packet.Create(player, PacketType.MapData, data, SendType.OrderedReliable));// SendType.Ordered | SendType.Reliable));
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            var client = net as Client;
            if (client.Map != null)
            {
                // create new empty map? or throw?
                throw new Exception("map already received");
                //"map already received, dropping packet".ToConsole();
            }
            if (client.World == null)
                throw new Exception("map received before world");


            StaticMap map = StaticMap.ReadData(client, r);// Network.Deserialize<StaticMap>(msg.Payload, StaticMap.ReadData);// as Map;
            map.World = client.World as StaticWorld;
            map.World.GetMaps().Add(map.Coordinates, map);
            map.Net = client;
            client.Map = map;
            GameModes.GameMode.Current.MapReceived(map);
            //this.ChunksPending = new List<Vector2>();
            //var size = map.Size.Chunks;//  StaticMap.MapSize.Default.Chunks;
            //for (int i = 0; i < size; i++)
            //    for (int j = 0; j < size; j++)
            //        this.ChunksPending.Add(new Vector2(i, j));
            //"map data received".ToConsole();
        }
    }
}
