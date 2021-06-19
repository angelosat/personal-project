using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.GameModes.StaticMaps.Packets
{
    class StaticMapsServerPacketHandler : ServerPacketHandler
    {
        public override void Handle(Server net, Packet msg)
        {
            switch (msg.PacketType)
            {
                case PacketType.PlayerEnterWorld:
                    //GameObject obj = Network.Deserialize<GameObject>(msg.Payload, GameObject.CreatePrefab);//CreateCustomObject);
                    GameObject obj = Network.Deserialize<GameObject>(msg.Payload, PlayerEntity.Create);//CreateCustomObject);


                    // let obj instantiate itself
                    //Instance.Instantiate(obj);
                    obj.Instantiate(net.Instantiator);
                    
                    // msg.Player.Character = obj;
                    //Instance.Spawn(obj);
                    msg.Player.CharacterID = obj.Network.ID;
                    msg.Player.Character = obj;
                    obj.Network.PlayerID = msg.Player.ID;

                    //add player to list of active players (whose character is in the world and must receive world updates
                    //Players.Add(msg.Player);


                    SendWorldInfo(net as Server, msg.Player);
                    SendMapInfo(net as Server, msg.Player);
                    msg.Player.IsActive = true;
                    
                    // send map to player
                    // send all chunks

                    return;
                default: break;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player">Sends to specific player, or to all players if null.</param>
        void SendMapInfo(Server server, PlayerData player)
        {
            byte[] data = Network.Serialize(server.Map.GetData); // why does it let me do that?
            if (player.IsNull())
                server.Players.GetList().ForEach(p => server.Enqueue(p, Packet.Create(player, PacketType.MapData, data, SendType.Ordered | SendType.Reliable)));
            else
                server.Enqueue(player, Packet.Create(player, PacketType.MapData, data, SendType.Ordered | SendType.Reliable));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="player">Sends to specific player, or to all players if null.</param>
        void SendWorldInfo(Server server, PlayerData player)
        {
            byte[] data = Network.Serialize(server.Map.GetWorld().WriteData);

            if (player.IsNull())
                server.Players.GetList().ForEach(p => server.Enqueue(p, Packet.Create(player, PacketType.WorldInfo, data, SendType.Ordered | SendType.Reliable)));
            else
                server.Enqueue(player, Packet.Create(player, PacketType.WorldInfo, data, SendType.Ordered | SendType.Reliable));
        }
    }
}
