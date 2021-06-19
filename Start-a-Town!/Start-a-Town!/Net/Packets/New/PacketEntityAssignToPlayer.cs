﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    class PacketEntityAssignToPlayer
    {
        internal static void Init()
        {
            Client.RegisterPacketHandler(PacketType.AssignCharacter, ReceiveClient);
        }
        internal static void Send(Server server, int playerid, int entityid)
        {
            var w = server.OutgoingStream;
            w.Write(PacketType.AssignCharacter);
            w.Write(playerid);
            w.Write(entityid);
        }
        internal static void ReceiveClient(IObjectProvider net, BinaryReader r)
        {
            Receive(net as Client, r);
        }
        internal static void Receive(Client client, BinaryReader r)
        {
            var playerid = r.ReadInt32();
            var entityid = r.ReadInt32();
            client.AssignCharacter(playerid, entityid);
            //Client.PlayerData.CharacterID = playerChar.InstanceID;
            //Player.Actor = playerChar;// client.GetNetworkObject(playerChar.InstanceID);
            //Rooms.Ingame.Instance.Hud.Initialize(Player.Actor);
            //Rooms.Ingame.Instance.Camera.CenterOn(Player.Actor.Global);
            //PlayerSaveTimer.Change(PlayerSaveInterval, PlayerSaveInterval);
        }
    }
}
