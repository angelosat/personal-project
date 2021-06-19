using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    public class Message
    {
        public enum Types
        {
            Default,
            DropOn,
            //  Drop,
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
            PickUp,
            SkillAward,
            Loot,
            //   Spawn,
            //Despawn,
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
            Interrupt,
            /// <summary>
            /// <para>arg0: Vector3 direction</para>
            /// <para>arg1: float: speed</para>
            /// </summary>
            Move,
            Task,
            SetContent,
            Shovel,
            Saw,
            Mine,
            Action,
            //    Destroy,
            Build,
            Consume,
            SetBlueprint,
            Toggle,
            /// <summary>
            /// <para>arg0: gameobjectslot to receive</para>
            /// <para>arg1: object contained in slot at time of message creation</para>
            /// <para>arg2: index of slot to insert the item</para>
            /// </summary>
            Receive,
            Chop,
            Plant,
            Retrieve,
            //       LoadStats,
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
            Ownership,
            Order,
            MoveToObject,
            ManageEquipment,
            ManageEquipmentOk,
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
            SetSprite,
            SetShadow,
            /// <summary>
            /// <para>arg0: GameObjectSlot to receive</para>
            /// <para>arg1: GameObject contained in slot at time of message creation</para>
            /// </summary>
            Hold,
            Throw,
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
            Dialogue,
            DialogueRequest,
            DialogueStart,
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
            HoldInventoryItem,
            EquipInventoryItem,
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
            DurabilityLoss,
            InvalidTarget,
            InvalidTargetType,
            ArrangeChildren,
            InsertAt,
            SlotInteraction,
            InventoryChanged,
            Memorization,
            StepOn,
            TooDense,
            ScriptMismatch,
            EntityMovedCell,
            InsufficientMaterials,
            WrongTool,
            TargetNotInventoryable,
            HitGround,
            Jumped,
            Chat,
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
            AgentsUpdated,
            LaborsUpdated,
            StockpileContentsChanged,
            AILogUpdated
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

        //public Types Type;
        //public GameObject Sender;
        //public object[] Args;

        //public Message(GameObject sender, Types type, object[] args)
        //{
        //    Sender = sender;
        //    Type = type;
        //    Args = args;
        //}

        //public override string ToString()
        //{
        //    //string info = Type.ToString();
        //    //foreach (object obj in Args)
        //    //    info += "\n" + obj;
        //    return Type.ToString();
        //}
    }

}
