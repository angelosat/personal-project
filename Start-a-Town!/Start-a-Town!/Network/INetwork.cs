﻿using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;
using System;
using System.Collections.Generic;
using System.IO;

namespace Start_a_Town_
{
    public interface INetwork
    {
        ConsoleBoxAsync ConsoleBox { get; }
        PlayerData CurrentPlayer { get; }
        TimeSpan Clock { get; }
        double CurrentTick { get; }
        MapBase Map { get; }
        int Speed { get; set; }

        GameObject GetNetworkObject(int netID);
        T GetNetworkObject<T>(int netID) where T : GameObject;

        IEnumerable<GameObject> GetNetworkObjects(IEnumerable<int> ids);
        IEnumerable<GameObject> GetNetworkObjects();

        bool TryGetNetworkObject(int netID, out GameObject obj);
        void Enqueue(PacketType packetType, byte[] payload, SendType sendType);

        IEnumerable<PlayerData> GetPlayers();
        PlayerData GetPlayer(int id);
        PlayerData GetPlayer();

        GameObject Instantiate(GameObject obj);

        bool DisposeObject(GameObject obj);
        bool DisposeObject(int netID);

        GameObject InstantiateObject(GameObject obj);
        void Instantiator(GameObject o);

        void SyncReport(string text);

        bool LogStateChange(int netID);

        void PopLoot(GameObject loot, Vector3 startPosition, Vector3 startVelocity);
        void PopLoot(LootTable table, Vector3 startPosition, Vector3 startVelocity);

        void PostLocalEvent(GameObject recipient, ObjectEventArgs args);
        void PostLocalEvent(GameObject recipient, Components.Message.Types type, params object[] args);

        void EventOccured(Components.Message.Types type, params object[] p);

        BinaryWriter GetOutgoingStream();

        void WriteToStream(params object[] args);

        void SetSpeed(int playerID, int speed);
        void Write(string text);
        void Report(string text);
    }
}
