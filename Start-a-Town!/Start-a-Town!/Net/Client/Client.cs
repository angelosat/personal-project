using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;
using Start_a_Town_.Net.Packets;
using Start_a_Town_.Net.Packets.Player;

namespace Start_a_Town_.Net
{
    enum PlayerSavingState { Saved, Changed, Saving }

    public class Client : IObjectProvider
    {
        static Client _Instance;
        static public Client Instance => _Instance ??= new Client();
     
        public double CurrentTick => ClientClock.TotalMilliseconds;
        
        UI.ConsoleBoxAsync _Console;
        public UI.ConsoleBoxAsync Log => _Console ??= new UI.ConsoleBoxAsync(new Rectangle(0, 0, 800, 600)) { FadeText = false };
        
        public UI.ConsoleBoxAsync GetConsole()
        {
            return Log;
        }
        bool IsRunning;

        long _packetID = 1;
        public long PacketID
        {
            get { return _packetID++; }
        }
        long RemoteSequence = 0;
        public long RemoteOrderedReliableSequence = 0;
        readonly Dictionary<int, GameObject> NetworkObjects = new();
        public IMap Map
        {
            set => Engine.Map = value;
            get => Engine.Map;
        }

        public NetworkSideType Type { get { return NetworkSideType.Local; } }

        readonly int TimeoutLength = Engine.TicksPerSecond * 2;
        int Timeout = -1;

        const int OrderedReliablePacketsHistoryCapacity = 64;
        readonly Queue<Packet> OrderedReliablePacketsHistory = new(OrderedReliablePacketsHistoryCapacity);
        public Client()
        {
        }
        public PlayerData PlayerData;
        public Socket Host;
        public EndPoint RemoteIP;
        public PlayerList Players;
        ConcurrentQueue<Packet> IncomingAll = new();
        readonly PriorityQueue<long, Packet> IncomingOrdered = new();
        readonly PriorityQueue<long, Packet> IncomingOrderedReliable = new();
        readonly PriorityQueue<long, Packet> IncomingSynced = new();
        ConcurrentDictionary<Vector2, ConcurrentQueue<Action<Chunk>>> ChunkCallBackEvents;
        TimeSpan ClientClock = new();
        double LastReceivedTime = int.MinValue;

        public TimeSpan Clock { get { return ClientClock; } }

        public BinaryWriter OutgoingStream = new(new MemoryStream());
        public BinaryWriter GetOutgoingStream()
        {
            return this.OutgoingStream;
        }

        public BinaryWriter OutgoingStreamTimestamped = new(new MemoryStream());
       

        readonly Queue<WorldSnapshot> WorldStateBuffer = new();
        readonly int WorldStateBufferSize = 10;
        public const int ClientClockDelayMS = Server.SnapshotIntervalMS * 4;
        int _Speed = 0;// 1;
        public int Speed { get { return this._Speed; } set { this._Speed = value; } }
        [Obsolete]
        public void EnterWorld(GameObject playerCharacter)
        {
            throw new NotImplementedException();
        }
      
        public void Disconnect()
        {
            IsRunning = false;
            Instance.World = null;
            Engine.Map = null;
            Timeout = -1;
            Instance.NetworkObjects.Clear();
            Packet.Create(PacketID, PacketType.PlayerDisconnected).BeginSendTo(Host, RemoteIP, a => { });
            IncomingAll = new ConcurrentQueue<Packet>();
            ClientClock = new TimeSpan();
            SyncedPackets = new Queue<Packet>();
        }

        /// <summary>
        /// Called when communication with server times out
        /// </summary>
        private void Disconnected()
        {
            IsRunning = false;
            "receiving pakets from server timed out".ToConsole();
            Timeout = -1;
            World = null;
            Engine.Map = null;
            IncomingAll = new ConcurrentQueue<Packet>();
            SyncedPackets = new Queue<Packet>();

            Instance.NetworkObjects.Clear();
            ScreenManager.GameScreens.Clear();
            ScreenManager.Add(Rooms.MainScreen.Instance);
            this.EventOccured(Message.Types.ServerNoResponse);
            ClientClock = new TimeSpan();

        }

        public void Connect(string address, string playername, AsyncCallback callBack)
        {
            Connect(address, new PlayerData(playername), callBack);
        }
        public void Connect(string address, PlayerData playerData, AsyncCallback callBack)
        {
            SyncedPackets = new Queue<Packet>();
            Timeout = TimeoutLength;
            LastReceivedTime = int.MinValue;
            IsRunning = true;
            ChunkCallBackEvents = new ConcurrentDictionary<Vector2, ConcurrentQueue<Action<Chunk>>>();
            RecentPackets = new Queue<long>();
            RemoteSequence = 0;
            RemoteOrderedReliableSequence = 0;
            PlayerData = playerData;
            _packetID = 1;
            Instance.OutgoingStream = new BinaryWriter(new MemoryStream());
            IncomingOrderedReliable.Clear();
            IncomingOrdered.Clear();
            IncomingSynced.Clear();
            IncomingAll = new ConcurrentQueue<Packet>();
            ClientClock = new TimeSpan();
            Players = new PlayerList(this);
            if (Host != null)
                Host.Close();
            Host = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Host.ReceiveBufferSize = Host.SendBufferSize = Packet.Size;

            if (!IPAddress.TryParse(address, out IPAddress ipAddress))
            {
                var fromdns = Dns.GetHostEntry(address);
                ipAddress = fromdns.AddressList[0];
            }

            RemoteIP = new IPEndPoint(ipAddress, 5541);
            var state = new UdpConnection("Server", Host) { Buffer = new byte[Packet.Size] };
            Host.Bind(new IPEndPoint(IPAddress.Any, 0));

            byte[] data = Packet.Create(PacketID, PacketType.RequestConnection, Network.Serialize(w =>
            {
                w.Write(playerData.Name);
            })).ToArray();

            Host.SendTo(data, RemoteIP);
            Host.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, a =>
            {
                // connection established
                // enter main receive loop
                callBack(a);
                ReceiveMessage(a);
            }, state);
        }
       
        void ReceivePlayerList(byte[] data)
        {
            this.Players = Network.Deserialize<PlayerList>(data, r=>PlayerList.Read(this, r));
        }

        public IEnumerable<PlayerData> GetPlayers()
        {
            return Players.GetList();
        }
        
        public void Update()
        {
            Timeout--;
            if (this.Timeout == 0)
                Disconnected();
            if (!IsRunning)
                return;

            // CALL THESE IN THE GAMESPEED LOOP
            HandleOrderedPackets();
            HandleOrderedReliablePackets();
            ProcessSyncedPackets();
            ProcessIncomingPackets();
            if (GameMode.Current != null)
                GameMode.Current.Update(Instance);

            if (Instance.Map != null)
            {
                var size = Instance.Map.GetSizeInChunks();
                var maxChunks = size * size;
                if (Instance.Map.ActiveChunks.Count == maxChunks)
                /// i had to add this check here because when i moved the call to map  update method, from the ingame update method (which was called only after the map has fully loaded),
                /// to the client update method here, the map update method was called before it was fully loaded so while doing cell operations, there were glitches at the edges of chunks where the
                /// neighbors weren't loaded yet. it's still ugly to put this check here, must find more elegant way
                {
                    if (!IsSaving)
                    {
                        for (int i = 0; i < Instance.Speed; i++)
                        {
                            // PACKETS MUST BE HANDLED AS THE CLOCK ADVANCES, AND THE CLOCK MUST ADVANCE ACCORDING TO GAMESPEED
                            //HandleOrderedPackets();
                            //HandleOrderedReliablePackets();
                            TickMap();
                            // moved it from here to be able to process packets from player input while game is paused
                            //ProcessEvents();
                            //ClientClock = ClientClock.Add(TimeSpan.FromMilliseconds(Server.ClockIntervalMS));
                        }
                        this.Map.Update(Instance);
                        //ProcessEvents();
                        UpdateWorldState();
                    }
                }
            }
            //HandleOrderedPackets();
            //HandleOrderedReliablePackets();
            ClientClock = ClientClock.Add(TimeSpan.FromMilliseconds(Server.ClockIntervalMS));
            // if there's any data in the outgoing stream, send it
            if (PlayerData is not null && this.Map is not null)
                PacketMousePosition.Send(Instance, PlayerData.ID, ToolManager.CurrentTarget); // TODO: do this at the toolmanager class instead of here

            // call these here or in the gamespeed loop?
            this.SendAcks();
            this.SendOutgoingStream();
        }
 
        SortedDictionary<ulong, (ulong worldtick, double servertick, byte[] data)> BufferTimestamped = new();

        ulong lasttickreceived;
        public void HandleTimestamped(BinaryReader r)
        {
            var currenttick = this.Map.World.CurrentTick;
            for (int i = 0; i < this.Speed; i++)
            {
                var tick = r.ReadUInt64();
                var serverTick = r.ReadDouble();
                var length = r.ReadInt64();
                
                if (length > 0)
                {
                    var array = r.ReadBytes((int)length);
                    if (tick == currenttick)
                        this.UnmergePackets(array);
                    else
                        this.BufferTimestamped.Add(tick, (tick, serverTick, array));
                }
                if (tick <= this.lasttickreceived)
                    throw new Exception();
                this.lasttickreceived = tick;
            }
        }
        void HandleTimestamped()
        {
            while(this.BufferTimestamped.Any())
            {
                var item = this.BufferTimestamped.First();
                var currenttick = this.Map.World.CurrentTick;
                if (item.Key != currenttick)
                    return;
                this.BufferTimestamped.Remove(item.Key);
                this.UnmergePackets(item.Value.data);
            }
        }
        private void TickMap()
        {
            this.HandleTimestamped();
            this.Map.UpdateParticles();
            this.Map.World.Tick(Instance);
            this.Map.Tick(Instance);
        }

        private void SendAcks()
        {
            if (!this.AckQueue.Any())
                return;
            this.OutgoingStream.Write(PacketType.Acks);
            this.OutgoingStream.Write(this.AckQueue.Count);
            while (this.AckQueue.Any())
            {
                if (this.AckQueue.TryDequeue(out long id))
                    this.OutgoingStream.Write(id);
            }
        }

        private void SendOutgoingStream()
        {
            if (this.OutgoingStream.BaseStream.Position > 0)
            {
                byte[] data;
                using (var output = new MemoryStream())
                {
                    using (var zip = new GZipStream(output, CompressionMode.Compress))
                    {
                        this.OutgoingStream.BaseStream.Position = 0;
                        this.OutgoingStream.BaseStream.CopyTo(zip);
                    }
                    data = output.ToArray();
                }
                if (data.Length > 0)
                    this.Send(PacketType.MergedPackets, data);
                this.OutgoingStream = new BinaryWriter(new MemoryStream());
            }
        }
      
        void OnGameEvent(GameEvent e)
        {
            GameMode.Current.HandleEvent(Instance, e);

            foreach (var item in Game1.Instance.GameComponents)
                item.OnGameEvent(e);
            UI.TooltipManager.OnGameEvent(e);
            ScreenManager.CurrentScreen.OnGameEvent(e);

            ToolManager.OnGameEvent(e);
            if(this.Map!=null)
            this.Map.OnGameEvent(e);
        }
        [Obsolete]
        readonly Dictionary<PacketType, IClientPacketHandler> PacketHandlers = new();
        [Obsolete]
        public void RegisterPacketHandler(PacketType channel, IClientPacketHandler handler)
        {
            this.PacketHandlers.Add(channel, handler);
        }
        [Obsolete]
        readonly static Dictionary<PacketType, Action<IObjectProvider, BinaryReader>> PacketHandlersNew = new();
        [Obsolete]
        static public void RegisterPacketHandler(PacketType channel, Action<IObjectProvider, BinaryReader> handler)
        {
            PacketHandlersNew.Add(channel, handler);
        }
      
        static int PacketSequence = 1;
        readonly static Dictionary<int, Action<IObjectProvider, BinaryReader>> PacketHandlersNewNewNew = new();
        internal static int RegisterPacketHandler(Action<IObjectProvider, BinaryReader> handler)
        {
            var id = PacketSequence++;
            PacketHandlersNewNewNew.Add(id, handler);
            return id;
        }
        internal static void RegisterPacketHandler(int id, Action<IObjectProvider, BinaryReader> handler)
        {
            PacketHandlersNewNewNew.Add(id, handler);
        }

        public void EventOccured(Message.Types type, params object[] p)
        {
            var e = new GameEvent(this, ClientClock.TotalMilliseconds, type, p);
            OnGameEvent(e);
        }
       
        private void ProcessSyncedPackets()
        {
            while (SyncedPackets.Count > 0)
            {
                var next = SyncedPackets.Peek();
                if (next.Tick > ClientClock.TotalMilliseconds)
                    return;
                SyncedPackets.Dequeue();
                HandleMessage(next);
            }
        }
        private void ProcessIncomingPackets()
        {
            while (this.IncomingAll.TryDequeue(out Packet packet))
            {
                if (packet.PacketType == PacketType.Chunk)
                    (DateTime.Now.ToString() + " " + packet.PacketType.ToString() + " dequeued").ToConsole();

                // if the timer is not stopped (not -1), reset it
                if (Timeout > -1)
                    Timeout = TimeoutLength;

                if (IsDuplicate(packet))
                {
                    continue;
                }
                RecentPackets.Enqueue(packet.ID);
                if (RecentPackets.Count > RecentPacketBufferSize)
                    RecentPackets.Dequeue();

                // for ordered packets, only handle last one (store most recent and discard and older ones)
                if (packet.SendType == SendType.Ordered)
                {
                    IncomingOrdered.Enqueue(packet.ID, packet);//e);
                }
                else if (packet.SendType == SendType.OrderedReliable)
                {
                    IncomingOrderedReliable.Enqueue(packet.OrderedReliableID, packet);
                }
                else
                {
                    var clientms = packet.Tick - ClientClockDelayMS;
                    if (this.CurrentTick < clientms)
                    {
                        this.ClientClock = TimeSpan.FromMilliseconds(clientms);
                        "client clock caught up".ToConsole();
                    }
                    HandleMessage(packet);
                }
            }
        }

        public void SavePlayer(GameObject actor, BinaryWriter writer)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, "Character");

            var charTag = new SaveTag(SaveTag.Types.Compound, "PlayerCharacter", actor.SaveInternal());

            // save metadata such as hotbar
            var hotbarTag = Rooms.Ingame.Instance.Hud.HotBar.Save();

            tag.Add(charTag);
            tag.Add(hotbarTag);

            tag.WriteTo(writer);
        }
        private void UnmergePackets(byte[] data)
        {
            using var mem = new MemoryStream(data);
            using var r = new BinaryReader(mem);
            var lastPos = mem.Position;
            while (mem.Position < data.Length)
            {
                var id = r.ReadInt32();
                var type = (PacketType)id;
                lastPos = mem.Position;

                if (PacketHandlersNew.TryGetValue(type, out Action<IObjectProvider, BinaryReader> handlerAction))
                    handlerAction(Instance, r);
                else if (PacketHandlersNewNewNew.TryGetValue(id, out var handlerActionNewNew))
                    handlerActionNewNew(Instance, r);
                else
                    Receive(type, r);
                if (mem.Position == lastPos)
                    break;
            }
        }
        void Receive(PacketType type, BinaryReader r)
        {
            switch (type)
            {
                case PacketType.SetSaving:
                    var val = r.ReadBoolean();
                    IsSaving = val;
                    Rooms.Ingame.Instance.Hud.Chat.Write(Start_a_Town_.Log.EntryTypes.System, "Map saved");
                    break;

                case PacketType.RemoteProcedureCall:
                    TargetArgs recipient = TargetArgs.Read(Instance.Map, r);
                    Components.Message.Types arg = (Components.Message.Types)r.ReadInt32();
                        if (recipient.Type == TargetType.Position)
                            Instance.Map.GetBlock(recipient.Global).RemoteProcedureCall(Instance, recipient.Global, arg, r); // TODO: FIX: CAN RECEIVE PACKET BEFORE INITIALIZING MAP!!!
                        else
                        {
                            GameObject obj = recipient.Object;
                            obj.RemoteProcedureCall(arg, r);
                        }
                    
                    break;

                default:
                    GameMode.Current.HandlePacket(Instance, type, r);
                    break;
            }
        }

        private void HandleMessage(Packet msg)
        {
            if (Instance.PacketHandlers.TryGetValue(msg.PacketType, out IClientPacketHandler handler))
            {
                handler.HandlePacket(Instance, msg);
                return;
            }

            if (PacketHandlersNew.TryGetValue(msg.PacketType, out Action<IObjectProvider, BinaryReader> handlerNew))
            {
                Network.Deserialize(msg.Payload, r=> handlerNew(Instance, r));
                return;
            }
            
            switch (msg.PacketType)
            {
                case PacketType.RequestConnection:
                    Instance.Timeout = Instance.TimeoutLength;
                    msg.Payload.Deserialize(r =>
                    {
                        Instance.PlayerData.ID = r.ReadInt32();
                        Instance.Players = PlayerList.Read(Instance, r);
                        Instance.Speed = r.ReadInt32();
                    });
                    Log.Write(Color.Lime, "CLIENT", "Connected to " + Instance.RemoteIP.ToString());
                    GameMode.Current.PlayerIDAssigned(Instance);
                    Instance.SyncTime(msg.Tick);
                    Instance.EventOccured(Message.Types.ServerResponseReceived);
                    break;

                case PacketType.PlayerDisconnected:
                    int plid = msg.Payload.Deserialize<int>(r => r.ReadInt32());
                    Instance.PlayerDisconnected(plid);
                    break;

                case PacketType.RemoteCall:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        double timestamp = r.ReadDouble();
                        TargetArgs recipient = TargetArgs.Read(Instance, r);
                        Message.Types type = (Components.Message.Types)r.ReadInt32();
                        byte[] data = r.ReadBytes(r.ReadInt32());
                        GameObject obj = recipient.Object;
                        obj.HandleRemoteCall(ObjectEventArgs.Create(type, data));
                    });
                    break;
               
                case PacketType.PlayerJump:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int netid = r.ReadInt32();
                        if (!Instance.TryGetNetworkObject(netid, out GameObject obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        obj.GetComponent<MobileComponent>().Jump(obj);
                    });
                    break;

                case PacketType.PlayerChangeDirection:
                    var packet = new PacketPlayerChangeDirection(Instance, msg.Decompressed);
                    if (packet.Entity is null)
                    {
                        Instance.RequestEntityFromServer(packet.EntityID);
                        return;
                    }
                    packet.Entity.Direction = packet.Direction;
                    break;

                case PacketType.PlayerToggleSprint:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        bool toggle = r.ReadBoolean();
                        if (!Instance.TryGetNetworkObject(netid, out GameObject obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        obj.GetComponent<MobileComponent>().ToggleSprint(toggle);
                    });
                    return;

                case PacketType.PlayerInteract:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        if (!Instance.TryGetNetworkObject(netid, out GameObject obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        obj.GetComponent<WorkComponent>().UseTool(obj, target);
                    });
                    return;

                case PacketType.PlayerUse:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        obj.GetComponent<WorkComponent>().Perform(obj, target.GetAvailableTasks(Instance).FirstOrDefault(), target);
                    });
                    return;

                case PacketType.PlayerUseHauled:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        var hauled = obj.GetComponent<HaulComponent>().GetObject();

                        if (hauled is null)
                            return;
                        obj.GetComponent<WorkComponent>().Perform(obj, hauled.GetHauledActions(target).FirstOrDefault(), target);
                    });
                    return;

                case PacketType.PlayerDropHauled:
                    msg.Payload.Deserialize(r =>
                    {
                        var netid = r.ReadInt32();
                        var target = TargetArgs.Read(Instance, r);
                        if (!Instance.TryGetNetworkObject(netid, out GameObject obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        obj.GetComponent<HaulComponent>().Throw(Vector3.Zero, obj);
                    });
                    return;

                case PacketType.PlayerInput:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        if (!Instance.TryGetNetworkObject(netid, out GameObject obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        var input = new PlayerInput(r);
                        var interaction =
                            Start_a_Town_.PlayerInput.GetDefaultInput(obj, target, input);

                        obj.GetComponent<WorkComponent>().Perform(obj, interaction, target);
                    });
                    return;

                case PacketType.PlayerDropInventory:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        byte slotid = r.ReadByte();
                        int amount = r.ReadInt32();
                        if (!Instance.TryGetNetworkObject(netid, out GameObject obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        Instance.PostLocalEvent(obj, Message.Types.DropInventoryItem, (int)slotid, amount);
                    });
                    return;

                case PacketType.PlayerRemoteCall:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        Message.Types call = (Message.Types)r.ReadInt32();

                        int dataLength = (int)(r.BaseStream.Length - r.BaseStream.Position);
                        byte[] args = r.ReadBytes(dataLength);

                        target.HandleRemoteCall(Instance, ObjectEventArgs.Create(call, args));
                    });
                    return;

                case PacketType.PlayerPickUp:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        if (!Instance.TryGetNetworkObject(netid, out GameObject obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        obj.GetComponent<WorkComponent>().Perform(obj, new InteractionHaul(), target);
                    });
                    return;

                case PacketType.PlayerHaul:
                    msg.Payload.Deserialize(r =>
                    {
                        int playerid = r.ReadInt32();
                        var entity = Instance.GetNetworkObject(playerid);
                        var topickup = Instance.GetNetworkObject(r.ReadInt32());
                        var target = new TargetArgs(topickup);
                        var amount = r.ReadInt32();
                        var interaction = new InteractionHaul(amount);
                        entity.GetComponent<WorkComponent>().Perform(entity, interaction, target);
                    });
                    return;

                case PacketType.PlayerUnequip:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        if (!Instance.TryGetNetworkObject(netid, out GameObject obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        PersonalInventoryComponent.Receive(obj, target.Slot, false);
                    });
                    return;

                case PacketType.EntityThrow:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        var dir = r.ReadVector3();
                        var all = r.ReadBoolean();
                        if (!Instance.TryGetNetworkObject(netid, out GameObject obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        HaulComponent.ThrowHauled(obj, dir, all);

                    });
                    return;

                case PacketType.PlayerStartAttack:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        obj.GetComponent<AttackComponent>().Start(obj);
                    });
                    return;

                case PacketType.PlayerFinishAttack:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        var dir = r.ReadVector3();
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        obj.GetComponent<AttackComponent>().Finish(obj, dir);
                    });
                    return;

                case PacketType.PlayerStartBlocking:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        obj.GetComponent<Components.BlockingComponent>().Start(obj);
                    });
                    return;

                case PacketType.PlayerFinishBlocking:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        obj.GetComponent<Components.BlockingComponent>().Stop(obj);
                    });
                    return;

                case PacketType.RemoteProcedureCall:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        TargetArgs recipient = TargetArgs.Read(Instance, r);
                        Components.Message.Types type = (Components.Message.Types)r.ReadInt32();
                        if (recipient.Type == TargetType.Position)
                            Instance.Map.GetBlock(recipient.Global).RemoteProcedureCall(Instance, recipient.Global, type, r); // TODO: FIX: CAN RECEIVE PACKET BEFORE INITIALIZING MAP!!!
                        else
                        {
                            GameObject obj = recipient.Object;
                            obj.RemoteProcedureCall(type, r);
                        }
                    });
                    break;

                case PacketType.SpawnChildObject:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        GameObject obj = GameObject.CreatePrefab(r);
                        if (obj.RefID == 0)
                            throw new Exception("Uninstantiated entity");
                        if (!Instance.NetworkObjects.ContainsKey(obj.RefID))
                            Instance.Instantiate(obj);

                        int parentID = r.ReadInt32();
                        GameObject parent;
                        if (!Instance.TryGetNetworkObject(parentID, out parent))
                            throw (new Exception("Parent doesn't exist"));

                        obj.Parent = parent;
                        int childIndex = r.ReadInt32();
                        var slot = parent.GetChildren()[childIndex];
                        slot.Object = obj;
                    });
                    return;

                case PacketType.InstantiateObject: //register netID to list without spawning
                    var ent = Network.Deserialize<GameObject>(msg.Payload, GameObject.CreatePrefab);
                    ent.Instantiate(Instance.Instantiator);
                    return;

                case PacketType.InstantiateAndSpawnObject: //register netID to list and spawn
                    Network.Deserialize(msg.Payload, r =>
                    {
                        GameObject ob = GameObject.CreatePrefab(r).ObjectCreated(); // the obj is received with a netid but without a position component// a  position component and netid
                        ob.Instantiate(Instance.Instantiator);
                        Instance.Spawn(ob);
                    });
                    return;

                case PacketType.DisposeObject:
                    TargetArgs tar = Network.Deserialize<TargetArgs>(msg.Payload, r => TargetArgs.Read(Instance, r));

                    switch (tar.Type)
                    {
                        case TargetType.Entity:
                            if (tar.Object == null)
                            {
                                Log.Write(Color.Orange, "CLIENT", "Can't dispose null entity");
                                break;
                            }
                            GameObject o;
                            Instance.Despawn(tar.Object);
                            Instance.DisposeObject(tar.Object);
                            break;

                        case TargetType.Slot:
                            o = tar.Slot.Object;
                            Instance.DisposeObject(o);
                            tar.Slot.Clear();
                            break;

                        default:
                            throw new Exception("Invalid object");
                    }
                    break;

                case PacketType.SyncEntity:

                    Network.Deserialize<GameObject>(msg.Payload, r =>
                    {
                        GameObject entity = GameObject.CreatePrefab(r);
                        GameObject existing;
                        if (Instance.TryGetNetworkObject(entity.RefID, out existing))
                        {
                            // TODO: sync existing entity's values from packet here
                            return existing;
                        }
                        Instance.Instantiate(entity);
                        if (!entity.IsSpawned)
                            Instance.Spawn(entity);
                        return entity;
                    });

                    break;

                case PacketType.IncreaseEntityQuantity:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        var tarentityID = r.ReadInt32(); 
                        var entity = Instance.NetworkObjects[tarentityID];
                        var quantity = r.ReadInt32();
                        entity.StackSize += quantity;
                    });
                    return;

                case PacketType.SpawnObject:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int nid = r.ReadInt32();
                        GameObject toSpawn = Instance.NetworkObjects[nid];
                        var pos = r.ReadVector3();
                        var vel = r.ReadVector3();
                        toSpawn.Global = pos;
                        toSpawn.Velocity = vel;
                        toSpawn.Spawn(this.Map, pos);
                    });
                    break;

                case PacketType.ServerBroadcast:
                    Network.Deserialize(msg.Payload, reader =>
                    {
                        string chatText = reader.ReadASCII();
                        Network.Console.Write(Color.Yellow, "SERVER", chatText);
                    });
                    break;

                case PacketType.RequestNewObject:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        TargetArgs obj = TargetArgs.Read(Instance, r);
                        byte amount = r.ReadByte();
                        DragDropManager.Create(new DragDropSlot(PlayerOld.Actor, GameObjectSlot.Empty, new GameObjectSlot(obj.Object, amount), DragDropEffects.Move | DragDropEffects.Copy));
                    });
                    break;

                case PacketType.PlayerSetBlock:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        Vector3 global = r.ReadVector3();
                        var type = (Start_a_Town_.Block.Types)r.ReadInt32();
                        var data = r.ReadByte();
                        var variation = r.ReadInt32();
                        var orientation = r.ReadInt32();

                        if (!Instance.Map.IsInBounds(global))
                            return;
                        Instance.Map.RemoveBlock(global);
                        var block = Block.Registry[type];
                        if (block != BlockDefOf.Air)
                            block.Place(Instance.Map, global, data, variation, orientation);
                    });
                    break;

                case PacketType.SyncPlaceBlock:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        var t = TargetArgs.Read(Instance, r);
                        Block.Types blocktype = (Block.Types)r.ReadInt32();
                        byte data = r.ReadByte();
                        int variation = r.ReadInt32();
                        int orientation = r.ReadInt32();
                        var block = Block.Registry[blocktype];
                        block.Place(Instance.Map, t.Global, data, variation, orientation);
                    });
                    break;

                case PacketType.EntityInventoryChange:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int count = r.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            GameObjectSlot slot = TargetArgs.Read(Instance, r).Slot;
                            var stacksize = r.ReadInt32();
                            if (stacksize > 0)
                            {
                                var id = r.ReadInt32();
                                slot.Object = Instance.GetNetworkObject(id);
                            }
                            slot.StackSize = stacksize;
                        }
                    });
                    break;

                case PacketType.PlayerInventoryOperationNew:
                    msg.Payload.Deserialize(r =>
                    {
                        var source = TargetArgs.Read(Instance, r);
                        var destination = TargetArgs.Read(Instance, r);
                        int amount = r.ReadInt32();
                        Instance.InventoryOperation(source.Slot, destination.Slot, amount);
                        return;
                    });
                    break;

                case PacketType.PlayerSlotRightClick:
                    msg.Payload.Deserialize(r =>
                    {
                        TargetArgs actor = TargetArgs.Read(Instance, r);
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        TargetArgs child = TargetArgs.Read(Instance, r);
                        target.HandleRemoteCall(Instance, new ObjectEventArgs(Components.Message.Types.PlayerSlotRightClick, actor.Object, child.Object));
                    });
                    return;

                case PacketType.PlayerSlotClick:
                    msg.Payload.Deserialize(r =>
                    {
                        TargetArgs actor = TargetArgs.Read(Instance, r);
                        TargetArgs t = TargetArgs.Read(Instance, r);
                        var parent = t.Slot.Parent as GameObject;
                        Instance.PostLocalEvent(parent, Components.Message.Types.SlotInteraction, actor.Object, t.Slot);
                    });
                    return;

                case PacketType.PlayerServerCommand:
                    msg.Payload.Deserialize(r =>
                    {
                        Instance.ParseCommand(r.ReadASCII());
                    });
                    break;

                case PacketType.ChangeEntityPosition:
                    msg.Payload.Deserialize(r =>
                    {
                        GameObject entity;
                        if (!Instance.NetworkObjects.TryGetValue(r.ReadInt32(), out entity))
                            return;
                        var pos = r.ReadVector3();
                        entity.ChangePosition(pos);
                    });
                    break;

                case PacketType.SetSaving:
                    msg.Payload.Deserialize(r => SetSaving(r.ReadBoolean()));
                    break;

                case PacketType.MergedPackets:
                    UnmergePackets(msg.Decompressed);
                    break;

                default:
                    GameMode.Current.HandlePacket(Instance, msg);
                    break;
            }
        }

        private void SetSaving(bool val)
        {
            IsSaving = val;
            Rooms.Ingame.Instance.Hud.Chat.Write(Start_a_Town_.Log.EntryTypes.System, IsSaving ? "Saving..." : "Map saved");
        }

        private void SyncTime(double serverMS)
        {
            if (this.LastReceivedTime > serverMS)
            {
                ("sync time packet dropped (last: " + LastReceivedTime.ToString() + ", received: " + serverMS.ToString()).ToConsole();// + "server: " + Server.ServerClock.TotalMilliseconds.ToString() + ")").ToConsole();
                return;
            }
            
            this.LastReceivedTime = serverMS;
            var newtime = serverMS - ClientClockDelayMS;
         
            TimeSpan serverTime = TimeSpan.FromMilliseconds(newtime);
            
            this.ClientClock = serverTime;
        }

        private void ParseCommand(string command)
        {
            CommandParser.Execute(this, command);
        }

        public void RequestEntityFromServer(int entityNetID)
        {
            Network.Serialize(w =>
            {
                w.Write(entityNetID);
            }).Send(PacketID, PacketType.RequestEntity, Host, RemoteIP);
        }

        public void SyncEvent(GameObject recipient, Components.Message.Types msg, Action<BinaryWriter> writer) { }

        readonly int OrderedPacketsHistoryCapacity = 32;
        readonly Queue<Packet> OrderedPacketsHistory = new(32);
        void HandleOrderedPackets()
        {
            while (this.IncomingOrdered.Count > 0)
            {
                Packet packet = IncomingOrdered.Dequeue();

                HandleMessage(packet);
                OrderedPacketsHistory.Enqueue(packet);
                while (OrderedPacketsHistory.Count > OrderedPacketsHistoryCapacity)
                    OrderedPacketsHistory.Dequeue();
            }
        }
        Queue<Packet> SyncedPackets = new();

        void HandleOrderedReliablePackets()
        {
            while (IncomingOrderedReliable.Count > 0)
            {
                var next = IncomingOrderedReliable.Peek();
                long nextid = next.OrderedReliableID;
                if (nextid == RemoteOrderedReliableSequence + 1)
                {

                    RemoteOrderedReliableSequence = nextid;
                    Packet packet = IncomingOrderedReliable.Dequeue();
                        if (next.Tick > Instance.Clock.TotalMilliseconds) // TODO maybe use this while changing clock to ad
                        {
                            SyncedPackets.Enqueue(next);
                            continue;
                        }
                    HandleMessage(packet);
                    OrderedReliablePacketsHistory.Enqueue(packet);
                    while (OrderedReliablePacketsHistory.Count > OrderedReliablePacketsHistoryCapacity)
                        OrderedReliablePacketsHistory.Dequeue();
                }
                else
                    return;
            }
        }
        
        /// <summary>
        /// Both removes an object form the game world and releases its networkID
        /// </summary>
        /// <param name="objNetID"></param>
        public bool DisposeObject(GameObject obj)
        {
            return DisposeObject(obj.RefID);
        }
        public bool DisposeObject(int netID)
        {
            if (!NetworkObjects.TryGetValue(netID, out GameObject o))
                return false;
            NetworkObjects.Remove(netID);
            if (o.IsSpawned)
                o.Despawn();
            foreach (var child in from slot in o.GetChildren() where slot.HasValue select slot.Object)
                this.DisposeObject(child);
            return true;
        }
        /// <summary>
        /// called ONLY upon recieving a message from the server
        /// </summary>
        /// <param name="obj"></param>
        public void SyncDisposeObject(GameObject obj)
        {
        }
       
        public GameObject InstantiateAndSync(GameObject obj)
        {
            return null;
        }

        /// <summary>
        /// Is passed recursively to an object and its children objects (inventory items) to register their network ID.
        /// </summary>
        /// <param name="ob"></param>
        /// <returns></returns>
        public GameObject Instantiate(GameObject ob)
        {
            ob.Instantiate(Instantiator);
            return ob;
        }
        public void InstantiateAndSpawn(GameObject obj) { }

        public void Instantiator(GameObject ob)
        {
            ob.Map = this.Map;
            ob.NetNew = this;
            Instance.NetworkObjects.Add(ob.RefID, ob);
        }
        
        public IWorld World;

        internal void AddPlayer(PlayerData player)
        {
            Players.Add(player);
            UI.LobbyWindow.RefreshPlayers(Players.GetList());
            UI.LobbyWindow.Instance.Console.Write(Color.Yellow, player.Name + " connected");
            UI.UIChat.Instance.Write(new Log.Entry(Start_a_Town_.Log.EntryTypes.System, player.Name + " connected"));
        }
        void PlayerDisconnected(PlayerData player)
        {
            this.Players.Remove(player);
            if (Instance.Map != null && player.ControllingEntity != null)
            {
                Instance.Despawn(Instance.NetworkObjects[player.CharacterID]);
                Instance.DisposeObject(player.CharacterID);
            }
            Network.Console.Write(Color.Yellow, player.Name + " disconnected");
            UI.UIChat.Instance.Write(new Log.Entry(Start_a_Town_.Log.EntryTypes.System, player.Name + " disconnected"));
        }
        public void PlayerDisconnected(int playerID)
        {
            PlayerData player = Players.GetList().FirstOrDefault(p => p.ID == playerID);
            if (player is null)
                return;
            PlayerDisconnected(player);
        }
        readonly ConcurrentQueue<long> AckQueue = new();
        void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                UdpConnection state = (UdpConnection)ar.AsyncState;
                int bytesRead = state.Socket.EndReceive(ar);
                if (bytesRead == Packet.Size)
                    throw new Exception("buffer full");

                byte[] bytesReceived = state.Buffer.Take(bytesRead).ToArray();
                Host.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveMessage, state);

                Packet packet = Packet.Read(bytesReceived);

                if ((packet.SendType & SendType.Reliable) == SendType.Reliable)
                    Packet.Send(PacketID, PacketType.Ack, Network.Serialize(w => w.Write(packet.ID)), Host, RemoteIP);

                this.IncomingAll.Enqueue(packet);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception e)
            {
                //ScreenManager.Remove(); // this is not the main thread. if i remove from here then in case the main thread is drawing, it won't be able to access the ingame camera class
                //// so either don't remove the screen here, or pass the camera to the root draw call instead of accessing it through the screenmanager currentscreen property
                Timeout = 0;
                e.ShowDialog();
            }
        }
        readonly int RecentPacketBufferSize = 32;
        Queue<long> RecentPackets = new();
        private bool IsDuplicate(Packet packet)
        {
            long id = packet.ID;
            if (id > RemoteSequence)
            {
                if (id - RemoteSequence > 31)
                {
                    // very large jump in packets
                    Log.Write(Color.Orange, "CLIENT", "Warning! Large gap in received packets!");
                }
                RemoteSequence = id;
                return false;
            }
            else if (id == RemoteSequence)
                return true;
            BitVector32 field = this.GenerateBitmask();
            int distance = (int)(RemoteSequence - id);
            if (distance > 31)
            {
                // very old packet
                Log.Write(Color.Orange, "CLIENT", "Warning! Received severely outdated packet: " + packet.PacketType.ToString());
                return false;
            }
            int mask = (1 << distance);
            bool found = (field.Data & mask) == mask;
            if (found)
                if (distance < 32)
                    if (!RecentPackets.Contains(id))
                        throw new Exception("duplicate detection error");
            return found;
        }

        BitVector32 GenerateBitmask()
        {
            int mask = 0;
            foreach (var recent in RecentPackets)
            {
                int distance = (int)(RemoteSequence - recent);
                if (distance > 31)
                    continue;
                mask |= 1 << distance;
                var test = new BitVector32(mask);
            }
            var bitvector = new BitVector32(mask);
            return bitvector;
        }

        public HashSet<Vector2> ChunkRequests = new();
        
        public void ReceiveChunk(Chunk chunk)
        {
            ChunkRequests.Remove(chunk.MapCoords);

            if (this.Map.GetActiveChunks().ContainsKey(chunk.MapCoords))
            {
                (chunk.MapCoords.ToString() + " already loaded").ToConsole();
                return;
            }
            chunk.Map = this.Map;

            chunk.GetObjects().ForEach(obj =>
            {
           
                obj.Instantiate(this.Instantiator);
                obj.MapLoaded(Instance.Map);
                obj.Transform.Exists = true;
                obj.ObjectLoaded();
            });
            
            foreach (var blockentity in chunk.GetBlockEntitiesByPosition())
            {
                var global = blockentity.local.ToGlobal(chunk);
                blockentity.entity.MapLoaded(this.Map, global);
                blockentity.entity.Instantiate(global, Instance.Instantiator);
            }

            Instance.Map.AddChunk(chunk);
            return;
        }

        /// <summary>
        /// Removes an object from the game world without releasing its NetworkID.
        /// </summary>
        /// <param name="obj"></param>
        public void Despawn(GameObject obj)
        {
            obj.Despawn();
        }
        
        public void Spawn(GameObject obj)
        {
            obj.Parent = null;
            obj.Map = this.Map;
            SpawnObject(obj);
        }
        public void Spawn(GameObject obj, Vector3 global)
        {
            obj.Parent = null;
            obj.Map = this.Map;
            obj.Global = global;
            this.SpawnObject(obj);
        }
        
        void SpawnObject(GameObject obj)
        {
            if (obj.RefID == 0)
                return;
            obj.Spawn(Instance);
        }
        public void Spawn(GameObject obj, GameObject parent, int childID)
        {
        }

        public void AddObject(GameObject obj, Vector3 global)
        {
            Packet.Create(this.PacketID, PacketType.InstantiateAndSpawnObject, Network.Serialize(w =>
            {
                obj.Write(w);
                Position.Write(w, global, Vector3.Zero);
            })).BeginSendTo(Host, RemoteIP);
        }
        

        /// <summary>
        /// The client can't create objects, must await for a server message
        /// </summary>
        /// <param name="obj"></param>
        public GameObject InstantiateObject(GameObject obj)
        {
            return obj;
        }

        public GameObject GetNetworkObject(int netID)
        {
            NetworkObjects.TryGetValue(netID, out var obj);
            return obj;
        }
        public T GetNetworkObject<T>(int netID) where T : GameObject
        {
            NetworkObjects.TryGetValue(netID, out var obj);
            return obj as T;
        }
        
        public IEnumerable<GameObject> GetNetworkObjects()
        {
            foreach (var o in this.NetworkObjects.Values)
                yield return o;
        }
        public IEnumerable<GameObject> GetNetworkObjects(IEnumerable<int> netID)
        {
            return (from o in this.NetworkObjects where netID.Contains(o.Key) select o.Value);
        }
        public bool TryGetNetworkObject(int netID, out GameObject obj)
        {
            return NetworkObjects.TryGetValue(netID, out obj);
        }
 
        /// <summary>
        /// find way to write specific changes, maybe by passing a state Object
        /// </summary>
        /// <param name="netID"></param>
        /// <returns></returns>
        public bool LogStateChange(int netID)
        {
            return false;
        }

        internal void ReadSnapshot(BinaryReader reader)
        {
            double totalMs = reader.ReadDouble();

            TimeSpan time = TimeSpan.FromMilliseconds(totalMs);
            WorldSnapshot worldState = new WorldSnapshot() { Time = time };

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                GameObject obj;
                int netID = reader.ReadInt32();
                obj = Instance.NetworkObjects.GetValueOrDefault(netID);
                var objsnapshot = ObjectSnapshot.Create(time, obj, reader);
                if (obj is null)
                {
                    continue;
                }
                worldState.ObjectSnapshots.Add(objsnapshot);
            }

            // insert world snapshot to world snapshot history
            WorldStateBuffer.Enqueue(worldState);
            while (WorldStateBuffer.Count > WorldStateBufferSize)
            {
                WorldSnapshot discarded = WorldStateBuffer.Dequeue();
            }
        }
        private void UpdateWorldState()
        {
            // iterate through the state buffer and find position
            List<WorldSnapshot> list = WorldStateBuffer.ToList();
            for (int i = 0; i < WorldStateBuffer.Count - 1; i++)
            {
                WorldSnapshot
                    prev = list[i],
                    next = list[i + 1];
                
                if (this.ClientClock == next.Time)
                {
                    SnapObjectPositions(prev, next);
                    return;
                }
            }
        }

        private void SnapObjectPositions(WorldSnapshot prev, WorldSnapshot next)
        {
            foreach (var objSnapshot in next.ObjectSnapshots)
            {
                ObjectSnapshot previousObjState = prev.ObjectSnapshots.Find(o => o.Object == objSnapshot.Object);
                if (previousObjState == null)
                    continue;
                objSnapshot.Object.ChangePosition(objSnapshot.Position);
                objSnapshot.Object.Velocity = objSnapshot.Velocity;
                objSnapshot.Object.Direction = objSnapshot.Orientation;
                if (float.IsNaN(objSnapshot.Object.Direction.X) || float.IsNaN(objSnapshot.Object.Direction.Y))
                    throw new Exception();
            }
        }

        static public void PostPlayerInput(Components.Message.Types type, Action<BinaryWriter> dataWriter, bool clientPrediction = true)
        {
            Network.Serialize(writer =>
            {
                writer.Write(Instance.ClientClock.TotalMilliseconds);
                TargetArgs.Write(writer, PlayerOld.Actor);
                ObjectEventArgs.Write(writer, type, dataWriter);
            }).Send(Instance.PacketID, PacketType.PlayerInputOld, Instance.Host, Instance.RemoteIP);
        }
        
        static public void PlayerInventoryOperationNew(TargetArgs source, TargetArgs target, int amount)
        {
            Network.Serialize(w =>
            {
                source.Write(w);
                target.Write(w);
                w.Write(amount);
            }).Send(Instance.PacketID, PacketType.PlayerInventoryOperationNew, Instance.Host, Instance.RemoteIP);
        }
       
        static public void PlayerSlotInteraction(GameObjectSlot slot)
        {
            Network.Serialize(writer =>
            {
                TargetArgs.Write(writer, PlayerOld.Actor);
                TargetArgs.Write(writer, slot);
            }).Send(Instance.PacketID, PacketType.PlayerSlotClick, Instance.Host, Instance.RemoteIP);
        }
        static public void PlayerSlotRightClick(TargetArgs parent, GameObject child)
        {
            Network.Serialize(writer =>
            {
                TargetArgs.Write(writer, PlayerOld.Actor);
                parent.Write(writer);
                TargetArgs.Write(writer, child);
            }).Send(Instance.PacketID, PacketType.PlayerSlotRightClick, Instance.Host, Instance.RemoteIP);
        }
        static public void PostPlayerInput(GameObject recipient, Components.Message.Types type, Action<BinaryWriter> dataWriter, bool clientPrediction = true)
        {
            Network.Serialize(writer =>
            {
                writer.Write(Instance.ClientClock.TotalMilliseconds);
                TargetArgs.Write(writer, recipient);
                ObjectEventArgs.Write(writer, type, dataWriter);
            }).Send(Instance.PacketID, PacketType.PlayerInputOld, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerSetBlock(Vector3 global, Block.Types type, byte data = 0, int variation = 0, int orientation = 0)
        {
            //send variation or let server decide on random variation? send -1 if want to let server?
            Network.Serialize(w =>
            {
                w.Write(global);
                w.Write((int)type);
                w.Write(data);
                w.Write(variation);
                w.Write(orientation);
            }).Send(Instance.PacketID, PacketType.PlayerSetBlock, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerStartMoving()
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
            }).Send(Instance.PacketID, PacketType.PlayerStartMoving, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerStopMoving()
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
            }).Send(Instance.PacketID, PacketType.PlayerStopMoving, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerChangeDirection(Vector3 direction)
        {
            // assign direction directly for prediction?
            PlayerOld.Actor.Direction = direction;
            new PacketPlayerChangeDirection(PlayerOld.Actor, direction).Send(Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerJump()
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
            }).Send(Instance.PacketID, PacketType.PlayerJump, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerToggleWalk(bool toggle)
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                w.Write(toggle);
            }).Send(Instance.PacketID, PacketType.PlayerToggleWalk, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerToggleSprint(bool toggle)
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                w.Write(toggle);
            }).Send(Instance.PacketID, PacketType.PlayerToggleSprint, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerInteract(TargetArgs target)
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                target.Write(w);
            }).Send(Instance.PacketID, PacketType.PlayerInteract, Instance.Host, Instance.RemoteIP);
        }
        
        internal static void PlayerUnequip(TargetArgs targetArgs)
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                targetArgs.Write(w);
            }).Send(Instance.PacketID, PacketType.PlayerUnequip, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerThrow(Vector3 dir, bool all)
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                w.Write(dir);
                w.Write(all);
            }).Send(Instance.PacketID, PacketType.EntityThrow, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerAttack()
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
            }).Send(Instance.PacketID, PacketType.PlayerStartAttack, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerFinishAttack(Vector3 vector3)
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                w.Write(vector3);
            }).Send(Instance.PacketID, PacketType.PlayerFinishAttack, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerStartBlocking()
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
            }).Send(Instance.PacketID, PacketType.PlayerStartBlocking, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerFinishBlocking()
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
            }).Send(Instance.PacketID, PacketType.PlayerFinishBlocking, Instance.Host, Instance.RemoteIP);
        }
       
        internal static void PlayerDropInventory(byte slotID, int amount)
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                w.Write(slotID);
                w.Write(amount);
            }).Send(Instance.PacketID, PacketType.PlayerDropInventory, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerRemoteCall(TargetArgs target, Message.Types types)
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                target.Write(w);
                w.Write((int)types);
            }).Send(Instance.PacketID, PacketType.PlayerRemoteCall, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerRemoteCall(TargetArgs target, Message.Types type, Action<BinaryWriter> argsWriter)
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                target.Write(w);
                w.Write((int)type);
                argsWriter(w);
            }).Send(Instance.PacketID, PacketType.PlayerRemoteCall, Instance.Host, Instance.RemoteIP);
        }

        internal static void PlayerInput(TargetArgs targetArgs, PlayerInput input)
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                targetArgs.Write(w);
                input.Write(w);
            }).Send(Instance.PacketID, PacketType.PlayerInput, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerCommand(string command)
        {
            var p = command.Split(' ');
            var type = p[0];
            switch (type)
            {
                case "reset":
                    ScreenManager.CurrentScreen.Camera.OnDeviceLost();
                    return;

                case "rebuildchunks":
                    foreach (var chunk in Instance.Map.GetActiveChunks().Values)
                    {
                        chunk.LightCache.Clear();
                        chunk.LightCache2.Clear();
                        chunk.InvalidateMesh();
                    }
                    return;

                default:
                    break;
            }

            Network.Serialize(writer =>
            {
                writer.WriteASCII(command);
            }).Send(Instance.PacketID, PacketType.PlayerServerCommand, Instance.Host, Instance.RemoteIP);
        }

        public void Send(PacketType packetType, byte[] data)
        {
            data.Send(PacketID, packetType, Host, RemoteIP);
        }

        /// <summary>
        /// Does nothing on client!
        /// </summary>
        /// <param name="packetType"></param>
        /// <param name="payload"></param>
        /// <param name="sendType"></param>
        public void Enqueue(PacketType packetType, byte[] payload, SendType sendType) { }

        /// <summary>
        /// Posts event data to a local object
        /// </summary>
        /// <param name="data">A serialized ObjectEventArgs array</param>
        public void PostLocalEvent(GameObject recipient, ObjectEventArgs args)
        {
            args.Network = Instance;
            recipient.PostMessage(args);
        }
        public void PostLocalEvent(GameObject recipient, Components.Message.Types type, params object[] args)
        {
            ObjectEventArgs a = ObjectEventArgs.Create(type, args);
            a.Network = Instance;
            recipient.PostMessage(a);
        }

        public void PopLoot(GameObject obj, GameObject parent)
        {
        }
        public void PopLoot(GameObject loot, Vector3 startPosition, Vector3 startVelocity) { }
        public void PopLoot(LootTable table, Vector3 startPosition, Vector3 startVelocity) { }

        [Obsolete]
        public void InventoryOperation(GameObjectSlot sourceSlot, GameObjectSlot targetSlot, int amount)
        {
            var sourceParent = sourceSlot.Parent;
            var destinationParent = targetSlot.Parent;
            if (targetSlot == sourceSlot)
                return;
            if (!targetSlot.Filter(sourceSlot.Object))
                return;

            this.Map.GetChunk(sourceParent.Global).Invalidate();
            this.Map.GetChunk(destinationParent.Global).Invalidate();

            var obj = sourceSlot.Object;
            if (!targetSlot.HasValue) // if target slot empty, set object of target slot without swapping and return
            {
                if (amount < sourceSlot.StackSize) // if the amount moved is smaller than the source amount
                {
                    sourceSlot.Object.GetInfo().StackSize -= amount;
                    // DO NOTHING. WAIT FOR NEW OBJECT FROM SERVER INSTEAD
                    return;
                }
                else
                    sourceSlot.Clear();
                targetSlot.Object = obj;
                return;
            }
            if (sourceSlot.Object.IDType == targetSlot.Object.IDType)
            {
                if (sourceSlot.StackSize + targetSlot.StackSize <= targetSlot.StackMax)
                {
                    targetSlot.StackSize += sourceSlot.StackSize;
                    this.DisposeObject(sourceSlot.Object.RefID);
                    sourceSlot.Clear();
                    //merge slots
                    return;
                }
            }
            else
                if (amount < sourceSlot.StackSize)
                    return;

            if (targetSlot.Filter(obj))
                if (sourceSlot.Filter(targetSlot.Object))
                    targetSlot.Swap(sourceSlot);

        }

        public void SyncSetBlock(Vector3 global, Block.Types type)
        {
        }
        
        public void Forward(Packet p)
        {

        }

        public static bool IsSaving;

        public PlayerData GetPlayer(int id)
        {
            return Players.GetPlayer(id);
        }
        public PlayerData GetPlayer()
        {
            return this.GetPlayer(PlayerData.ID);
        }
        public PlayerData CurrentPlayer => this.PlayerData;

        internal void AssignCharacter(int playerid, int entityid)
        {
            if (PlayerData.ID != playerid)
                return;
            var entity = this.GetNetworkObject(entityid);
            PlayerData.CharacterID = entity.RefID;
            PlayerOld.Actor = entity;
            Rooms.Ingame.Instance.Hud.Initialize(PlayerOld.Actor);
            Rooms.Ingame.Instance.Camera.CenterOn(PlayerOld.Actor.Global);
        }
        
        internal void HandleServerResponse(int playerID, PlayerList playerList, int speed)
        {
            throw new Exception();
        }
        public void SetSpeed(int playerID, int playerSpeed)
        {
            GetPlayer(playerID).SuggestedSpeed = playerSpeed;
            var newspeed = Players.GetLowestSpeed();
            if (newspeed != this.Speed)
                Rooms.Ingame.Instance.Hud.Chat.Write(Start_a_Town_.Log.EntryTypes.System, string.Format("Speed set to {0}x ({1})", newspeed, string.Join(", ", Players.GetList().Where(p => p.SuggestedSpeed == newspeed).Select(p => p.Name))));
            this.Speed = newspeed;
        }

        public void Write(string text)
        {
            Rooms.Ingame.Instance.Hud.Chat.Write(text);
        }
       
        public void Report(string text)
        {
            this.Write(text);
        }

        public void SyncReport(string text)
        {
        }

        public void WriteToStream(params object[] args)
        {
            this.GetOutgoingStream().Write(args);
        }
    }
}
