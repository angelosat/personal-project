using System;

namespace Start_a_Town_.Components
{
    public class Message
    {
        public enum Types
        {
            Default,
            Death,
            InteractionInterrupted,
            InteractionFailed,
            Walk,
            OutOfRange,
            Attacked,
            HealthLost,
            InvalidTarget,
            InvalidTargetType,
            SlotInteraction,
            HitGround,
            Jumped,
            ChatPlayer,
            NoDurability,
            AttributeChanged,
            AttributeProgressChanged,
            EntityCollision,
            AttackTargetChanged,
            AttackTelegraph,
            BlockEntityRemoved,
            BlockChanged,
            ItemGot,
            ItemLost,
            NotEnoughSpace,
            EntityDespawned,
            EntitySpawned,
            NpcsUpdated,
            AILogUpdated,
            EntityHitCeiling,
            EntityHitGround,
            EntityFootStep,
            OrdersUpdatedNew,
            MiningDesignation,
            ObjectDisposed,
            OrderParametersChanged,
            PlantHarvested,
            PlantReady,
            EntityAttacked,
            NeedUpdated,
            ItemOwnerChanged,
            BlocksChanged,
            PlayerConnected,
            PlayerDisconnected,
            ServerResponseReceived,
            ChunksLoaded,
            ServerNoResponse,
            PlayerControlNpc,
            SelectedChanged,
            BlockEntityAdded,
            ContentsChanged,
            SkillIncrease,
            ZoneDesignation,
            MoodletsUpdated,
            FuelConsumed,
            ActorGearUpdated,
            NewAdventurerCreated,
            ShopsUpdated,
            ShopUpdated,
            QuestDefsUpdated,
            QuestObjectivesUpdated,
            QuestDefAssigned,
            QuestReceived,
            JobUpdated,
            TavernMenuChanged,
            OrderDeleted,
            QuestAbandoned
        }

        public GameObject Receiver;
        public ObjectEventArgs Args;
        public Action<GameObject> Callback;
        public Message(GameObject receiver, ObjectEventArgs e, Action<GameObject> callback = null)
        {
            this.Receiver = receiver;
            this.Args = e;
            this.Callback = callback;
        }
    }
}
