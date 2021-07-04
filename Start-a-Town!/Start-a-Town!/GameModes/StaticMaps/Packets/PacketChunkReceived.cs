using System.IO;
using Start_a_Town_.GameModes;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class PacketChunkReceived
    {
        internal static void Init()
        {
            Server.RegisterPacketHandler(PacketType.ChunkReceived, Receive);
        }
        internal static void Send(Client client, PlayerData player, Vector2 chunkCoords)
        {
            var w = client.OutgoingStream;
            w.Write(PacketType.ChunkReceived);
            w.Write(player.ID);
            w.Write(chunkCoords);
        }
        internal static void Receive(IObjectProvider net, BinaryReader r)
        {
            var playerid = r.ReadInt32();
            var vec = r.ReadVector2();
            GameMode.Current.ChunkReceived(net as Server, playerid, vec);
        }
    }
}
