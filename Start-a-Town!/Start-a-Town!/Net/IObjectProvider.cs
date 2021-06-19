using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;
using Start_a_Town_.Net;

namespace Start_a_Town_
{
    public interface IObjectProvider
    {
        //UI.ConsoleBoxAsync GetConsole();
        UI.ConsoleBoxAsync Log { get; }
        PlayerData CurrentPlayer { get; }
        TimeSpan Clock { get; }

        GameObject GetNetworkObject(int netID);
        T GetNetworkObject<T>(int netID) where T : GameObject;

        IEnumerable<GameObject> GetNetworkObjects(IEnumerable<int> ids);
        //List<GameObject> GetNetworkObjects(params int[] ids);
        IEnumerable<GameObject> GetNetworkObjects();

        bool TryGetNetworkObject(int netID, out GameObject obj);
        void Enqueue(PacketType packetType, byte[] payload, SendType sendType);

        IEnumerable<PlayerData> GetPlayers();
        //void HandleTimestamped(BinaryReader r);
        PlayerData GetPlayer(int id);
        PlayerData GetPlayer();

        //GameObject Create(GameObject.Types type, Vector3 global, Vector3 speed);
        GameObject Instantiate(GameObject obj);
        void InstantiateAndSpawn(GameObject obj);
        void InstantiateAndSpawn(IEnumerable<GameObject> objects);


        bool DisposeObject(GameObject obj);
        bool DisposeObject(int netID);
        void SyncDisposeObject(GameObject obj);
        void SyncDisposeObject(GameObjectSlot Slot);

        GameObject InstantiateAndSync(GameObject obj);
        void InstantiateInContainer(GameObject o, GameObject parent, byte containerID, byte slotID, byte amount);
        void InstantiateInContainer(GameObject input, Vector3 blockentity, byte slotID);

        GameObject InstantiateObject(GameObject obj);
        //Action<GameObject> Instantiator { get; }
        void Instantiator(GameObject o);
        IMap Map { get; }
        int Speed { get; set; }

        void Spawn(GameObject obj);
        void Spawn(GameObject obj, Vector3 global);
        void Spawn(GameObject obj, WorldPosition pos);
        void Spawn(GameObject obj, GameObject parent, int childID);
        void Spawn(GameObject gameObject, GameObjectSlot slot);

        void SyncSlotInsert(GameObjectSlot slot, GameObject obj);
        void SyncReport(string text);
        /// <summary>
        /// Removes an entity from the map, but doesn't destroy it (it retains its networkID and can be referenced)
        /// </summary>
        /// <param name="obj"></param>
        void Despawn(GameObject obj);

        /// <summary>
        /// syncs an object over the network
        /// </summary>
        /// <param name="obj"></param>
        //void Sync(GameObject obj);
        [Obsolete]
        void SyncEvent(GameObject recipient, Components.Message.Types msg, Action<BinaryWriter> writer);
        bool LogStateChange(int netID);
        bool LogLightChange(Vector3 global);

        //void PopLoot(GameObject parent, GameObject loot);
        [Obsolete]
        void PopLoot(GameObject obj);
        [Obsolete]
        void PopLoot(GameObject obj, GameObject parent);
        void PopLoot(GameObject loot, Vector3 startPosition, Vector3 startVelocity);
        //void PopLoot(Components.LootTable table, GameObject parent);
        void PopLoot(LootTable table, Vector3 startPosition, Vector3 startVelocity);
        List<GameObject> GenerateLoot(LootTable loot);

        void PostLocalEvent(GameObject recipient, ObjectEventArgs args);
        void PostLocalEvent(GameObject recipient, Components.Message.Types type, params object[] args);
        //void PostLocalEvent(GameObject recipient, Components.Message.Types type, GameObject source, byte[] data);

        //event EventHandler<GameEvent> GameEvent;
        void EventOccured(Components.Message.Types type, params object[] p);//EventOccured(GameObject sender, ObjectEventArgs args);
        void EventOccured(object e, params object[] p);//EventOccured(GameObject sender, ObjectEventArgs args);

        //void EventOccured(GameObject obj, Components.Message.Types type, params object[] args);

        void UpdateLight(Vector3 global);

        void RandomEvent(GameObject target, ObjectEventArgs a, Action<double> rnEvent);
        void RandomEvent(TargetArgs target, ObjectEventArgs a, Action<double> rnEvent);

        void SendBlockMessage(Vector3 global, Components.Message.Types msg, params object[] parameters);


        void SyncSetBlock(Vector3 global, Block.Types type);
        void SyncSetBlock(Vector3 global, Block.Types type, byte data, int orientation);
        void SetBlock(Vector3 global, Block.Types type, byte data);
        void SetBlock(Vector3 global, Block.Types type);
        void UpdateBlock(Vector3 global, Action<Cell> updater);

        void TryGetRandomValue(Action<double> action);
        void TryGetRandomValue(int min, int max, Action<int> action);
        bool TryGetRandomValue(int min, int max, out int rand);

        void SpreadBlockLight(Vector3 global);

        void Forward(Packet p);

        BinaryWriter GetOutgoingStream();
        BinaryWriter GetOutgoingStreamTimestamped();

        void WriteToStream(params object[] args);

        void SetSpeed(int playerID, int speed);
        void Write(string text);
        void Write(Log.EntryTypes type, string text);
        void Report(string text);

        double CurrentTick { get; }
    }
}
