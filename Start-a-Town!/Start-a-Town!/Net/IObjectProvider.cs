using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public interface IObjectProvider
    {
        UI.ConsoleBoxAsync Log { get; }
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
        
        void Spawn(GameObject obj);
        void Spawn(GameObject obj, Vector3 global);

        void SyncReport(string text);
        /// <summary>
        /// Removes an entity from the map, but doesn't destroy it (it retains its networkID and can be referenced)
        /// </summary>
        /// <param name="obj"></param>
        void Despawn(GameObject obj);

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
