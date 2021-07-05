using System.Linq;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketPlayerEnterWorld
    {
        internal static void Init()
        {
            Server.RegisterPacketHandler(PacketType.PlayerEnterWorld, ReceiveServer);
            Client.RegisterPacketHandler(PacketType.PlayerEnterWorld, ReceiveClient);
        }
        internal static void Send(IObjectProvider net, int playerid, GameObject entity)
        {
            var w = net.GetOutgoingStream();
            w.Write(PacketType.PlayerEnterWorld);
            w.Write(playerid);
            entity.Write(w);
        }
        internal static void ReceiveServer(IObjectProvider net, BinaryReader r)
        {
            Receive(net as Server, r);
        }
        internal static void ReceiveClient(IObjectProvider net, BinaryReader r)
        {
            Receive(net as Client, r);
        }
        internal static void Receive(Client client, BinaryReader r)
        {
            var playerid = r.ReadInt32();
            var player = client.GetPlayer(playerid);
            UI.LobbyWindow.Instance.Console.Write(Color.Yellow, player.Name + " connected");
            Client.Instance.Log.Write(Color.Lime, "CLIENT", player.Name + " connected");
            GameObject entity = PlayerEntity.Create(r);
            client.Instantiate(entity);
            client.Spawn(entity);
        }
        internal static void Receive(Server server, BinaryReader r)
        {
            var playerid = r.ReadInt32();
            GameObject obj = GameObject.CreatePrefab(r);
            obj.Instantiate(server.Instantiator);
            var player = server.GetPlayer(playerid);
            player.CharacterID = obj.RefID;
            player.ControllingEntity = obj as Actor;
            player.IsActive = true;
            var map = server.Map as GameModes.StaticMaps.StaticMap;
            var savedPlayer = map.SavedPlayers.FirstOrDefault(foo => foo.Name == obj.Name);
            Vector3 spawnPosition = Vector3.Zero;
            if (savedPlayer == null)
                spawnPosition = Server.FindValidSpawnPosition(obj);
            else
                spawnPosition = savedPlayer.Global;
            obj.Global = spawnPosition;
            server.Spawn(obj);
            //signal all players to spawn player entity
            PacketPlayerEnterWorld.Send(server, playerid, obj);
            // signal client to own their character
            PacketEntityAssignToPlayer.Send(server, playerid, obj.RefID);
        }
    }
}
