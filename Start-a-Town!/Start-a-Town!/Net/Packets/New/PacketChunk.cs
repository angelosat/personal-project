using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketChunk
    {
        internal static void Init()
        {
            Client.RegisterPacketHandler(PacketType.Chunk, Receive);
        }
        internal static void Send(IObjectProvider net, Vector2 vector2, byte[] chunkData, PlayerData player)
        {
            var ch = net.Map.GetChunkAt(vector2);
            var server = net as Server;
            //player.OutReliable.Enqueue(Packet.Create(player, PacketType.Chunk, chunkData, sendType: SendType.OrderedReliable));
            server.Enqueue(player, Packet.Create(player, PacketType.Chunk, chunkData, sendType: SendType.OrderedReliable));
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            var chunk = Chunk.Create(net.Map, r);
            var client = net as Client;
            client.ReceiveChunk(chunk);
            ("chunk received " + chunk.MapCoords.ToString()).ToConsole();
            PacketChunkReceived.Send(client, Client.Instance.PlayerData, chunk.MapCoords);
            //Client.Instance.Send(PacketType.ChunkReceived, Network.Serialize(w => w.Write(chunk.MapCoords)));

            // change screen when player entity is assigned instead of here?
            //if (this.ChunksPending.Count == 0)
            if(net.Map.AreChunksLoaded)
            {
                GameModes.GameMode.Current.AllChunksReceived(net);
                //// all chunks received, enter world
                //"all chunks loaded!".ToConsole();
                //net.EventOccured(Components.Message.Types.ChunksLoaded);
                //var map = net.Map as GameModes.StaticMaps.StaticMap;
                //map.Regions.Init();
                //map.InitUndiscoveredAreas();
                //map.FinishLoading();//.CacheObjects();
                //client.EnterWorld(global::Start_a_Town_.Player.Actor);


                //var ingame = Rooms.Ingame.Instance.Initialize(client);
                //ingame.Camera.CenterOn(map);
                //ScreenManager.Add(ingame); // TODO: find out why there's a freeze when ingame screen begins (and causing rendertargets during ingame.initialize() not work

                ////var ingame = new Rooms.Ingame();
                ////ingame.Camera.CenterOn(map);
                ////ScreenManager.Add(ingame);
            }
        }
    }
}
