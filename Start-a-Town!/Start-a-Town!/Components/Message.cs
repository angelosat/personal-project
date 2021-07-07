﻿using System;

namespace Start_a_Town_.Components
{
    public class Message
    {
        public enum Types
        {
            Default,
            DropOn,
            Give,
            Activate,
            Initialize,
            Death,
            Attack,
            Equip,
            Unequip,
            Craft,
            Eat,
            Update,
            ConditionFinished,
            SkillAward,
            Loot,
            UIContainer,
            ContainerClose,
            Create,
            Mechanical,
            Progress,
            Till,
            True,
            False,
            Perform,
            Begin,
            InteractionInterrupted,
            InteractionSucceeded,
            /// <summary>
            /// <para>arg0: Vector3 direction</para>
            /// <para>arg1: float: speed</para>
            /// </summary>
            Task,
            SetContent,
            Shovel,
            Saw,
            Mine,
            Action,
            Build,
            Consume,
            SetBlueprint,
            Toggle,
            /// <summary>
            /// <para>arg0: gameobjectslot to receive</para>
            /// <para>arg1: object contained in slot at time of message creation</para>
            /// <para>arg2: index of slot to insert the item</para>
            /// </summary>
            Chop,
            Plant,
            Retrieve,
            Aggro,
            Need,
            ModifyNeed,
            InteractionFinished,
            Detect,
            Insert,
            Extract,
            Followed,
            Unfollowed,
            Buff,
            RestoreHealth,
            InteractionFailed,
            NoInteraction,
            StartBehavior,
            SetTarget,
            Work,
            NeedAbility,
            Carry,
            BeginInteraction,
            PushInteraction,
            SetOwner,
            /// <summary>
            /// <para>arg0: a Predicate&lt;GameObject&gt; for similar items</para>
            /// </summary>
            UIOwnership,
            Constructed,
            Order,
            MoveToObject,
            ManageEquipment,
            /// <summary>
            /// <para>arg0: source GameObjectSlot</para>
            /// <para>arg1: target GameObjectSlot</para>
            /// <para>arg2: int amount to transfer</para>
            /// </summary>
            ArrangeInventory,
            ConsumeItem,
            Follow,
            SetPosition,
            SetJob,
            AssignJob,
            AIStart,
            AIStop,
            AIToggle,
            Construct,
            ChangeOrientation,
            Structure,
            ChunkLoaded,
            SetTile,
            /// <summary>
            /// <para>arg0: GameObjectSlot to receive</para>
            /// <para>arg1: GameObject contained in slot at time of message creation</para>
            /// </summary>
            CraftObject,
            JobAccepted,
            GetGoals,
            JobGet,
            UpdateAbilities,
            Dropped,
            Walk,
            JobComplete,
            OutOfRange,
            Thrown,
            ApplyForce,
            FinishConstruction,
            ControlDisable,
            ControlEnable,
            PostAll,
            Post,
            JobRemove,
            JobDelete,
            UIJobBoard,
            BlockCollision,
            UIConstruction,
            UIConversation,
            Speak,
            DialogueRequest,
            DialogueEnd,
            DialogueOption,
            ApplyMaterial,
            UpdateJobs,
            Think,
            ObjectStateChanged,
            JobDeleteAll,
            JobRemoveAll,
            Clear,
            Remember,
            Attacked,
            StartAnimation,
            Open,
            UISetBlueprint,
            UICrafting,
            Interface,
            Crafted,
            Use,
            Refresh,
            CraftOnBench,
            Wear,
            ExecuteScript,
            DropInventoryItem,
            StoreCarried,
            ContainerOperation,
            UsedItem,
            BeginInteraction2,
            BeginScript,
            AddItem,
            ReceiveItem,
            StartScript,
            FinishScript,
            ChangeDirection,
            HealthLost,
            AddProduct,
            UseInventoryItem,
            Threat,
            ScriptFinished,
            JobStepFinished,
            UseHauledItem,
            InvalidTarget,
            InvalidTargetType,
            ArrangeChildren,
            InsertAt,
            SlotInteraction,
            StepOn,
            EntityMovedCell,
            HitGround,
            Jumped,
            ChatEntity,
            ChatPlayer,
            Start,
            Finish,
            NoDurability,
            EntityChangedChunk,
            EntityEnteringUnloadedChunk,
            SpawnChunkNotLoaded,
            SyncAI,
            AICommand,
            AttributeChanged,
            AttributeProgressChanged,
            EntityCollision,
            AttackTargetChanged,
            SetBlockVariation,
            SetProduct,
            BlockEntityStateChanged,
            PlayerSlotRightClick,
            BlockEntityRemoved,
            EntityRemoved,
            HousesUpdated,
            BlockChanged,
            InteractionSuccessful,
            AttackTelegraph,
            //Speech
            AISetLeader,
            ConversationStart,
            ConversationFinish,
            AIStopFollowing,
            ItemGot,
            ItemLost,
            NotEnoughSpace,
            PlayerStartConstruction,
            StockpileCreated,
            StockpileDeleted,
            StockpileUpdated,
            FarmCreated,
            FarmSeedChanged,
            EntityDespawned,
            PlantGrown,
            OrdersUpdated,
            FarmRemoved,
            CraftingComplete,
            JobsUpdated,
            GrovesUpdated,
            GroveEdited,
            GroveRemoved,
            GroveAdded,
            FarmUpdated,
            WorkstationOrderSet,
            EntitySpawned,
            NpcsUpdated,
            LaborsUpdated,
            StockpileContentsChanged,
            AILogUpdated,
            LaborsPrioritiesUpdated,
            EntityHitCeiling,
            EntityHitGround,
            EntityFootStep,
            ParticleEmitterAdd,
            ResidenceAdded,
            ResidenceUpdated,
            ResidenceRemoved,
            DebugSetStackSize,
            Haul,
            StackSizeChanged,
            NameChanged,
            ItemReserved,
            AIBehaviorChanged,
            OrdersUpdatedNew,
            TaskComplete,
            MiningDesignation,
            ObjectDisposed,
            OrderParametersChanged,
            FarmSeedSowed,
            PlantHarvested,
            PlantReady,
            ConstructionReady,
            EntityPlacedItem,
            EntityAttacked,
            NeedUpdated,
            ItemOwnerChanged,
            FarmDesignation,
            ZoneRemoved,
            BlocksChanged,
            StockpileContentsUpdated,
            ConstructionAdded,
            ConstructionRemoved,
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
            GuidanceReceived,
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
