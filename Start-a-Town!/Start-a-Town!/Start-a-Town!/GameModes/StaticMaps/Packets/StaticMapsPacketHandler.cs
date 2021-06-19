using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.GameModes.StaticMaps.Packets
{
    class StaticMapsPacketHandler : PacketHandler
    {
        public enum Channels { RequestChunks };

        public override void Update(Client client)
        {
            Chunk chunk;
            while (this.IncomingChunks.TryDequeue(out chunk))
            {
                client.ReceiveChunk(chunk);
                this.ChunksPending.Remove(chunk.MapCoords);

                if (this.ChunksPending.Count == 0)
                {
                    // all chunks received, enter world
                    "all chunks loaded!".ToConsole();
                    client.EnterWorld(Player.Actor);
                    Rooms.Ingame ingame = Rooms.Ingame.Instance;// new Rooms.Ingame();
                    ScreenManager.Add(ingame.Initialize()); // TODO: find out why there's a freeze when ingame screen begins (and causing rendertargets during ingame.initialize() not work
                }
            }
        }

        public override void Handle(Server server, Packet msg)
        {

            switch (msg.PacketType)
            {
                case PacketType.RequestPlayerID:
                    server.SendWorldInfo(msg.Player);
                    server.SendMapInfo(msg.Player);
                    break;

                case PacketType.PlayerEnterWorld:
                    GameObject obj = Network.Deserialize<GameObject>(msg.Payload, PlayerEntity.Create);//CreateCustomObject);
                    obj.Instantiate(server.Instantiator);
                    msg.Player.CharacterID = obj.Network.ID;
                    msg.Player.Character = obj;
                    obj.Network.PlayerID = msg.Player.ID;
                    msg.Player.IsActive = true;
                    //obj.Global = Vector3.Zero;
                    var map = server.Map as StaticMap;
                    var savedPlayer = map.SavedPlayers.FirstOrDefault(foo => foo.Name == obj.Name);
                    Vector3 spawnPosition = Vector3.Zero;
                    if (savedPlayer == null)
                        spawnPosition = Server.FindValidSpawnPosition(obj);
                    else
                        spawnPosition = savedPlayer.Global + Vector3.UnitZ;

                    server.InstantiateSpawnPlayerEntity(msg, obj, spawnPosition);
                    
                    break;

                case PacketType.StaticMaps:
                    msg.Payload.Deserialize(r =>
                    {
                        Channels channel = (Channels)(r.ReadInt32());
                        switch (channel)
                        {
                            case Channels.RequestChunks:
                                var watch = System.Diagnostics.Stopwatch.StartNew();
                                "sending chunks".ToConsole();
                                foreach (var ch in server.Map.GetActiveChunks().Values)
                                {
                                    server.SendChunk(msg.Player, ch);
                                    //(ch.MapCoords.ToString() + " sent").ToConsole();
                                }
                                ("chunks sent in " + watch.ElapsedMilliseconds.ToString()).ToConsole();
                                //server.SendChunks(msg.Player, server.Map.GetActiveChunks().Keys.ToArray());
                                break;

                            default:
                                break;
                        }
                    });
                    break;

                default: 
                    break;
            };
        }

        ConcurrentQueue<Chunk> IncomingChunks = new ConcurrentQueue<Chunk>();
        List<Vector2> ChunksPending = new List<Vector2>();
        //ConcurrentDictionary<Vector2, object> ChunksPending = new ConcurrentDictionary<Vector2, object>();

        public override void Handle(Client client, Packet msg)
        {
            switch (msg.PacketType)
            {
                case PacketType.RequestPlayerID:
                    // request map?
                    //client.Enqueue(PacketType.StaticMaps, Network.Serialize(w => w.Write((int)Channels.RequestMap)), SendType.OrderedReliable);
                    client.Send(PacketType.StaticMaps, Network.Serialize(w => w.Write((int)Channels.RequestChunks)));
                    break;

                case PacketType.MapData:
                    if (client.Map != null)
                    {
                        // create new empty map? or throw?
                        throw new Exception("map already received");
                        "map already received, dropping packet".ToConsole();
                        break;
                    }
                    if (client.World == null)
                        throw new Exception("map received before world");
                  
                    //Map map = Network.Deserialize<Map>(msg.Payload, Map.ReadData);// as Map;
                    //map.World = client.World;
                    //map.Net = client;
                    //client.Map = map;

                    StaticMap map = Network.Deserialize<StaticMap>(msg.Payload, StaticMap.ReadData);// as Map;
                    map.World = client.World as StaticWorld;
                    map.Net = client;
                    client.Map = map;

                    this.ChunksPending = new List<Vector2>();
                    //this.ChunksPending = new ConcurrentDictionary<Vector2,object>();

                    var size = map.Size.Chunks;//  StaticMap.MapSize.Default.Chunks;
                    for (int i = 0; i < size; i++)
                        for (int j = 0; j < size; j++)
                            this.ChunksPending.Add(new Vector2(i, j));
                            //this.ChunksPending.TryAdd(new Vector2(i, j), new object());
                    "map data received".ToConsole();
                    break;

                case PacketType.WorldInfo:
                    if (!client.World.IsNull())
                    {
                        throw new Exception("world already received");
                        "world already received, dropping packet".ToConsole();
                        break;
                    }
                    StaticWorld world = Network.Deserialize<StaticWorld>(msg.Payload, StaticWorld.ReadData);// as World;
                    client.World = world;
                    break;

                case PacketType.Chunk:
                    //"reading chunk...".ToConsole();
                    Chunk chunk = Chunk.Read(msg.Decompressed);
                    this.IncomingChunks.Enqueue(chunk);
                    return;
                    //chunk.MapCoords.ToConsole();
                    //client.Map.GetActiveChunks().AddOrUpdate(chunk.MapCoords, chunk, ex => chunk);
                    //chunk.Map = client.Map;
                    //// instantiate chunk here
                    //(chunk.MapCoords.ToString() + " received").ToConsole();

                    client.ReceiveChunk(chunk);
                    this.ChunksPending.Remove(chunk.MapCoords);

                    //object foo;
                    //this.ChunksPending.TryRemove(chunk.MapCoords, out foo);

                    //foreach (var cell in chunk.VisibleOutdoorCells)
                    //    chunk.InvalidateCell(cell.Value);
                    //(chunk.MapCoords.ToString() + " received, " + this.ChunksPending.Count.ToString() + " remaining").ToConsole();

                    if(this.ChunksPending.Count == 0)
                    {
                        // all chunks received, enter world
                        "all chunks loaded!".ToConsole();
                        client.EnterWorld(Player.Actor);
                        Rooms.Ingame ingame = Rooms.Ingame.Instance;// new Rooms.Ingame();
                        ScreenManager.Add(ingame.Initialize());
                        //if (Player.Actor != null)
                        //    "asd".ToConsole();
                    }
                    break;

                default: break;
            }
        }
    }
}
