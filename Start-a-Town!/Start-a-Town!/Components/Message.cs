﻿using System;

namespace Start_a_Town_.Components
{
    public class Message
    {
        public enum Types
        {
            Default,
            Death,
            InteractionInterrupted,
            OutOfRange,
            Attacked,
            HealthLost,
            SlotInteraction,
            HitGround,
            Jumped,
            ChatPlayer,
            EntityCollision,
            BlockEntityAdded,
            BlockEntityRemoved,
            BlockChanged,
            BlocksChanged,
            ItemGot,
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
            PlayerConnected,
            PlayerDisconnected,
            ServerResponseReceived,
            ChunksLoaded,
            ServerNoResponse,
            PlayerControlNpc,
            SelectedChanged,
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
            QuestAbandoned,
            ItemLost,
            AttackTargetChanged
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
