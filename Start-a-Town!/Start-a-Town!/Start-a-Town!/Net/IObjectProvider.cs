using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Net
{
    public interface IObjectProvider
    {
        UI.ConsoleBoxAsync GetConsole();

        TimeSpan Clock { get; }

        GameObject GetNetworkObject(int netID);
        bool TryGetNetworkObject(int netID, out GameObject obj);

        void Enqueue(PacketType packetType, byte[] payload, SendType sendType);

        List<PlayerData> GetPlayers();

        //GameObject Create(GameObject.Types type, Vector3 global, Vector3 speed);
        GameObject Instantiate(GameObject obj);

        //void Destroy(GameObject obj);
        bool DisposeObject(GameObject obj);
        bool DisposeObject(int netID);
        void SyncDisposeObject(GameObject obj);
        void SyncDisposeObject(GameObjectSlot Slot);

        GameObject InstantiateAndSync(GameObject obj);
        void InstantiateInContainer(GameObject o, GameObject parent, byte containerID, byte slotID, byte amount);
        GameObject InstantiateObject(GameObject obj);
        //Action<GameObject> Instantiator { get; }
        void Instantiator(GameObject o);
        IMap Map { get; }

        void Spawn(GameObject obj);
        void Spawn(GameObject obj, Vector3 global);
        void Spawn(GameObject obj, WorldPosition pos);
        void Spawn(GameObject obj, GameObject parent, int childID);
        void Spawn(GameObject gameObject, GameObjectSlot slot);

        void SyncSlotInsert(GameObjectSlot slot, GameObject obj);

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
        void SyncEvent(GameObject recipient, Components.Message.Types msg, Action<BinaryWriter> writer);
        bool LogStateChange(int netID);
        bool LogLightChange(Vector3 global);

        //void PopLoot(GameObject parent, GameObject loot);
        void PopLoot(GameObject obj);
        void PopLoot(GameObject obj, GameObject parent);
        void PopLoot(GameObject loot, Vector3 startPosition, Vector3 startVelocity);
        //void PopLoot(Components.LootTable table, GameObject parent);
        void PopLoot(Components.LootTable table, Vector3 startPosition, Vector3 startVelocity);
        List<GameObject> GenerateLoot(Components.LootTable loot);

        void PostLocalEvent(GameObject recipient, ObjectEventArgs args);
        void PostLocalEvent(GameObject recipient, Components.Message.Types type, params object[] args);
        //void PostLocalEvent(GameObject recipient, Components.Message.Types type, GameObject source, byte[] data);

        event EventHandler<GameEvent> GameEvent;
        void EventOccured(Components.Message.Types type, params object[] p);//EventOccured(GameObject sender, ObjectEventArgs args);
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
    }
}
