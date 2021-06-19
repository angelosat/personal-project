using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace Start_a_Town_.Net
{
    public enum PacketType
    {
        PlayerConnecting = 1, PlayerDisconnected, PlayerEnterWorld, PlayerData, PlayerList, PlayerInputOld, AssignCharacter, RequestPlayerID, //PlayerExitWorld, 
        Chat, SyncTime, MapData, RequestMapInfo, RequestWorldInfo, WorldInfo, RequestChunk, Chunk,
        InstantiateObject, DisposeObject, ObjectEvent, SyncEntity, Snapshot,
        //RequestConnection
        Ack,
        Ping,
        //   Pong,
        ServerBroadcast,
        InstantiateAndSpawnObject,
        InstantiateInContainer,
        RequestNewObject,
        SpawnObject,
        DespawnObject,
        JobCreate,
        JobDelete,
        SyncBlocks,
        SyncLight,
        PlayerInventoryOperation,
        PlayerInventoryOperationOld,
        PlayerSlotClick,
        SpawnChildObject,
        RandomEvent,
        RandomBlockUpdates,
        PlaceBlockConstruction, PlaceConstruction,
        ChunkSegment,
        Partial,
        PlayerSetBlock,
        PlayerRemoveBlock,
        PlayerStartMoving,
        PlayerChangeDirection,
        PlayerStopMoving,
        PlayerJump,
        PlayerToggleWalk,
        PlayerToggleSprint,
        PlayerInteract,
        PlayerUse,
        PlayerUseHauled,
        PlayerDropHauled,
        PlayerPickUp,
        PlayerCarry,
        PlayerEquip,
        PlaceStructure,
        PlayerKick,
        PlayerServerCommand,
        EntityThrow,
        PlayerStartAttack,
        PlayerFinishAttack,
        PlayerStartBlocking,
        PlayerFinishBlocking,
        PlayerCraft,
        PlayerCraftRequest,
        PlayerDropInventory,
        PlayerRemoteCall,
        SyncSlot,
        UnloadChunk,
        //SyncCells,
        UpdateChunkNeighbors,
        UpdateChunkEdges,
        RemoteCall,
        RequestEntity,
        SyncAI,
        Towns,
        IncreaseEntityQuantity,
        SpawnEntity,
        AI,
        EntityInteract,
        PlayerCraftBench,
        PlayerUnequip,
        PlayerInput,
        PlayerInventoryChange,
        StaticMaps,
        RemoteProcedureCall,
        PlayerInventoryOperationNew,
        SpawnObjectInSlot,
        PlayerSlotRightClick,
        PlayerCreateHouse,
        SetBlockVariation,
        SyncPlaceBlock,
        EntityCancelAttack,
        ChangeEntityPosition,
        ConversationStart,
        ConversationFinish,
        Conversation,
        MergeEntities,
        FarmCreate,
        FarmDelete,
        FarmSetSeed,
        FarmSync,
        ChoppingZoneCreate,
        DiggingDesignate,
        CraftingOrderPlace,
        CraftingOrderRemove,
        EntityInterrupt,
        StockpileFilters,
        StockpileRename,
        StockpileEdit,
        ChoppingDesignation,
        FarmlandDesignate,
        ZoneGrove,
        GroveEdit,
        PlayerUseInteraction,
        WorkstationSetCurrent,
        WorkstationToggle,
        StockpileFiltersCategories,
        CraftingOrdersClear,
        LaborToggle,
        NeedModifyValue,
        AIJobComplete,
        StockpileDelete,
        AIGenerateNpc,
    }

    //public enum SendType { Unreliable, Ordered, Reliable, ReliableOrdered }
    [Flags]
    public enum SendType { Unreliable = 0, Ordered = 0x1, Reliable = 0x2, OrderedReliable = 0x3 }//, ReliableOrdered = 0x4}

    public class Packet
    {
        public const int MaxAttempts = 5;
        public const int Size = 65535 * 2;
        public long ID { get; set; }
        public long OrderedReliableID;
        public EndPoint Sender { get; set; }
        public EndPoint Recipient { get; set; }

        public SendType SendType { get; set; }
        public double Tick;

        /// <summary>
        /// The connection from which the packet has been received, is null if the packet has just been created
        /// </summary>
        public UdpConnection Connection { get; set; }
        public Stopwatch RTT { get; set; }
        public PacketType PacketType { get; set; }
        public int Length { get; set; }
        public byte[] Payload { get; set; }
        public byte[] Decompressed { get; set; }
        public Socket Socket { get; set; }
        //public bool WaitForAck { get; set; }
        public int Retries { get; set; }
        public PlayerData Player { get; set; }
        public System.Threading.Timer ResendTimer { get; set; }
        protected Packet() { }
        public Packet(long id, PacketType type, int length, byte[] payload)
        {
            this.RTT = new Stopwatch();
            this.ID = id;
            this.PacketType = type;
            this.Length = length;
            this.Payload = payload;
            this.Retries = MaxAttempts;
          //  this.SendType = 0;//Net.SendType.Unreliable;
        }
        //static public Queue<Packet> ReadBuffer(byte[] buffer, Packet prevPartial, out Packet nextPartial)
        //{
        //    Queue<Packet> queue = new Queue<Packet>();
        //    using (BinaryReader reader = new BinaryReader(new MemoryStream(buffer)))
        //    {
        //        bool built = false;
        //        nextPartial = null;
        //        if (!prevPartial.IsNull())
        //        {
        //            if (prevPartial.Length < 0)
        //                prevPartial.Length = reader.ReadInt32();
        //            int diff = prevPartial.Length - prevPartial.Payload.Length;
        //            byte[] full = new byte[prevPartial.Length];
        //            prevPartial.Payload.CopyTo(full, 0);
        //            reader.ReadBytes(diff).CopyTo(full, prevPartial.Payload.Length);
        //            prevPartial.Payload = full;
        //            if(full.Length < prevPartial.Length)
        //                throw new Exception("packet remained partial");
        //            queue.Enqueue(prevPartial);
        //            "partial packet built".ToConsole();
        //            built = true;
        //            // handle case where new buffer still doesn't contain the rest of the message
        //        }
        //        while (reader.BaseStream.Position < buffer.Length)
        //        {
        //            PacketType type = (PacketType)reader.ReadByte();
        //            if (buffer.Length - reader.BaseStream.Position < 4)
        //            {
        //                // packet split in length section, create new packet with negative length to signal next receive to read length
        //                nextPartial = new Packet(type, -1, new byte[0]);
        //                "length wasn't received, splitting...".ToConsole();
        //                return queue;
        //            }
        //            int length = reader.ReadInt32();
        //            int remain = buffer.Length - (int)reader.BaseStream.Position;
        //            // if (reader.BaseStream.Position + length > buffer.Length)
        //            byte[] payload;
        //            if(length>30005)
        //                throw new Exception("invalid length");
        //            if (length > remain)
        //                payload = reader.ReadBytes(remain);
        //            else
        //                payload = reader.ReadBytes(length);
        //            Packet packet = new Packet(type, length, payload);

        //            if (packet.Payload.Length < packet.Length)
        //            {
        //                nextPartial = packet;
        //                return queue;
        //            }
        //            else
        //                queue.Enqueue(packet);
        //        }
        //    }
        //    return queue;
        //}

        //static public Queue<Packet> ReadBuffer(byte[] buffer, out Packet partial)
        //{
        //    Queue<Packet> queue = new Queue<Packet>();
        //    using (BinaryReader reader = new BinaryReader(new MemoryStream(buffer)))
        //    {
        //        partial = null;
        //        while (reader.BaseStream.Position < buffer.Length)
        //        {
        //            PacketType type = (PacketType)reader.ReadByte();
        //            int length = reader.ReadInt32();
        //            int remain = buffer.Length - (int)reader.BaseStream.Position;
        //            // if (reader.BaseStream.Position + length > buffer.Length)
        //            byte[] payload;
        //            if (length > remain)
        //                payload = reader.ReadBytes(remain);
        //            else
        //                payload = reader.ReadBytes(length);
        //            Packet packet = new Packet(type, length, payload);

        //            if (packet.Payload.Length < packet.Length)
        //            {
        //                partial = packet;
        //                return queue;
        //            }
        //            else
        //                queue.Enqueue(packet);
        //        }
        //    }
        //    return queue;
        //}

        //static public Queue<Packet> ReadBuffer(byte[] buffer, int bytesRead)
        //{
        //    Queue<Packet> queue = new Queue<Packet>();
        //    using (BinaryReader reader = new BinaryReader(new MemoryStream(buffer)))
        //    {
        //        while (reader.BaseStream.Position < bytesRead)
        //        {
        //            PacketType type = (PacketType)reader.ReadByte();
        //            int length = reader.ReadInt32();
        //            if (reader.BaseStream.Position + length > bytesRead)
        //                throw new Exception("partial data");
        //            byte[] payload = reader.ReadBytes(length);
        //            queue.Enqueue(new Packet(type, length, payload));
        //        } 
        //    }
        //    return queue;
        //}

        //public override int GetHashCode()
        //{
        //    return this.ID.GetHashCode();
        //}
        //public long OrderedReliableSequence;
        static public Packet Read(byte[] data)
        {
            //return new Message(socket, (MessageType)data[0], (int)data[1], data.Skip(2).ToArray());

            using (BinaryReader reader = new BinaryReader(new MemoryStream(data)))
            {
                long orderReliableseq = 0;
                long id = reader.ReadInt64();
                SendType sendType = (SendType)reader.ReadInt32(); //read and write sendtype as 2 bits
                if (sendType == SendType.OrderedReliable)
                    orderReliableseq = reader.ReadInt64();
                PacketType type = (PacketType)reader.ReadByte();
                int length = reader.ReadInt32();

                byte[] payload = reader.ReadBytes(length);
                byte[] decompressed = payload.Decompress(); // TODO: FIX: i already have a decompressed payload and i still deserialize everything when handling packets???
                double tick = reader.ReadDouble();
               // byte[] decompressed = new byte[0];
                return new Packet(id, type, length, payload) { SendType = sendType, Decompressed = decompressed, OrderedReliableID = orderReliableseq, Tick = tick };
            }
        }
        static public Packet Read(BinaryReader reader)
        {
            long id = reader.ReadInt64();
            SendType sendType = (SendType)reader.ReadInt32(); //read and write sendtype as 2 bits
            PacketType type = (PacketType)reader.ReadByte();
            int length = reader.ReadInt32();
            //byte[] payload = reader.ReadBytes((int)Math.Min(length, reader.BaseStream.Length - reader.BaseStream.Position));

            byte[] payload = reader.ReadBytes(length);
            double tick = reader.ReadDouble();
            //byte[] payload = new byte[length];
            //reader.ReadBytes((int)Math.Min(length, reader.BaseStream.Length - reader.BaseStream.Position)).CopyTo(payload, 0);

            return new Packet(id, type, length, payload) { SendType = sendType, Tick = tick };
        }
        static public Packet Create(PlayerData reciepient, PacketType type, byte[] data, SendType sendType = SendType.Unreliable)
        {
            return new Packet(reciepient.PacketSequence, type, data.Length, data) { 
                Player = reciepient, 
                SendType = sendType,
                OrderedReliableID = sendType == SendType.OrderedReliable ? reciepient.OrderedReliableSequence++ : 0
            };
        }
        static public Packet Create(long id, PacketType type, byte[] data, SendType sendType)
        {
            return new Packet(id, type, data.Length, data) { SendType = sendType };
        }
        static public Packet Create(long id, PacketType type, byte[] data, PlayerData reciepient, SendType sendType)
        {
            return new Packet(id, type, data.Length, data) { Player = reciepient, SendType = sendType };
        }
        static public Packet Create(long id, PacketType type, byte[] data, PlayerData reciepient)
        {
            return new Packet(id, type, data.Length, data) { Player = reciepient };
        }

        static public Packet Create(long id, PacketType type, byte[] data)
        {
            return new Packet(id, type, data.Length, data);
        }
        static public Packet Create(long id, PacketType type)
        {
            return new Packet(id, type, 0, new byte[] { });
        }
        static public Packet Create(long id, Packet toCopy, PlayerData player)
        {
            return Packet.Create(id, toCopy.PacketType, toCopy.Payload, player, toCopy.SendType);
        }

        public Packet Copy(long id, PlayerData player)
        {
            return Packet.Create(id, this.PacketType, this.Payload, player);
        }


        static public byte[] ToArray(PlayerData pl, long id, SendType sendType, PacketType type, byte[] data)
        {
            var mem = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(mem))
            {
                writer.Write(id);
                writer.Write((int)sendType);
                if (sendType == SendType.OrderedReliable)
                {
                    //pl.OrderedReliableSequence.ToConsole();
                    writer.Write(pl.OrderedReliableSequence++);
                }
                writer.Write((byte)type);
                writer.Write(data.Length);
                writer.Write(data);
                return mem.ToArray();
            }
        }
        public byte[] ToArray()
        {
            //return ToArray(this.Player, this.ID, this.SendType, this.PacketType, this.Payload);
            var mem = new MemoryStream();
            using (BinaryWriter writer = new BinaryWriter(mem))
            {
                writer.Write(this.ID);
                writer.Write((int)this.SendType);
                if (this.SendType == SendType.OrderedReliable)
                {
                    //pl.OrderedReliableSequence.ToConsole();
                    //writer.Write(pl.OrderedReliableSequence++);
                    writer.Write(this.OrderedReliableID);
                }
                writer.Write((byte)this.PacketType);
                writer.Write(this.Payload.Length);
                writer.Write(this.Payload);
                writer.Write(this.Tick);
                return mem.ToArray();
            }
        }

        public override string ToString()
        {
            return "ID: " + this.ID + " / Type: " + this.PacketType + " / Size: " + this.Length + " / Attempts: " + Retries;
            //return this.ID + " - " + this.PacketType + " - "  +this.Length + "b - " + ;
        }

        public void BeginSendTo(Socket socket, EndPoint ip)
        {
            this.BeginSendTo(socket, ip, ar => { });
        }
        public void BeginSendTo(Socket socket, EndPoint ip, AsyncCallback callback)
        {
            byte[] array = this.ToArray();

            try
            {
                socket.BeginSendTo(array, 0, array.Length, SocketFlags.None, ip, a =>
                {
                    socket.EndSend(a);
                    callback(a);
                    //("sent " + this.PacketType.ToString() + " (" + socket.EndSend(a).ToString() + " bytes)").ToConsole();
                }, socket);
            }
            catch (ObjectDisposedException) { }
            //catch (SocketException) { array.Length.ToConsole(); }
            //catch (SocketException) {
            //}
            //socket.BeginSend(array, 0, array.Length, SocketFlags.None, a =>
            //{
            //    socket.EndSend(a);
            //    callback(a);
            //    //("sent " + this.PacketType.ToString() + " (" + socket.EndSend(a).ToString() + " bytes)").ToConsole();
            //}, socket);
        }

        static public void SendReliable(long id, PacketType type, byte[] payload, Socket socket, EndPoint remoteIP)
        {
            Packet.Create(id, type, payload, SendType.Ordered | SendType.Reliable).BeginSendTo(socket, remoteIP);
        }
        static public void Send(long id, PacketType type, byte[] payload, Socket socket, EndPoint remoteIP)
        {
            Packet.Create(id, type, payload).BeginSendTo(socket, remoteIP);
        }

        public virtual void Send(Socket socket, EndPoint remoteIP)
        {
            Network.Serialize(this.Write).Send(this.ID, this.PacketType, socket, remoteIP);
        }
        public virtual void Write(BinaryWriter w) { }
        public virtual Packet Create(IObjectProvider net, byte[] data) { return null; }
        public virtual void Read(IObjectProvider net, byte[] data) { }
        public virtual void Read(IObjectProvider net, BinaryReader r) { }

        public virtual byte[] Write()
        {
            return Network.Serialize(this.Write);
            return new byte[0];
        }
        public byte[] Serialize()
        {
            return Network.Serialize(this.Write);
        }
        //public virtual void Read(byte[] data) { }

        public virtual void Handle(IObjectProvider net) { }

    }
}
