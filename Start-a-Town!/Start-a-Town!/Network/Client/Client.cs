using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;
using Start_a_Town_.UI;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace Start_a_Town_.Net
{
    enum PlayerSavingState { Saved, Changed, Saving }

    public class Client : INetwork
    {
        static Client _Instance;
        public static Client Instance => _Instance ??= new Client();

        public double CurrentTick => this.ClientClock.TotalMilliseconds;

        UI.ConsoleBoxAsync _Console;
        public UI.ConsoleBoxAsync ConsoleBox => this._Console ??= new UI.ConsoleBoxAsync(new Rectangle(0, 0, 800, 600)) { FadeText = false };
        public UI.ConsoleBoxAsync GetConsole()
        {
            return this.ConsoleBox;
        }
        bool IsRunning;

        long _packetID = 1;
        public long PacketID => this._packetID++;
        long RemoteSequence = 0;
        public long RemoteOrderedReliableSequence = 0;
        readonly Dictionary<int, GameObject> NetworkObjects = new();
        public MapBase Map
        {
            set => Engine.Map = value;
            get => Engine.Map;
        }

        public NetworkSideType Type => NetworkSideType.Local;

        readonly int TimeoutLength = Engine.TicksPerSecond * 2;
        int Timeout = -1;

        const int OrderedReliablePacketsHistoryCapacity = 64;
        readonly Queue<Packet> OrderedReliablePacketsHistory = new(OrderedReliablePacketsHistoryCapacity);
        Client()
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
        public static bool IsSaving;

        public TimeSpan Clock => this.ClientClock;

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
        public int Speed { get => this._Speed; set => this._Speed = value; }

        public void Disconnect()
        {
            this.IsRunning = false;
            Instance.World = null;
            Engine.Map = null;
            this.Timeout = -1;
            Instance.NetworkObjects.Clear();
            Packet.Create(this.PacketID, PacketType.PlayerDisconnected).BeginSendTo(this.Host, this.RemoteIP, a => { });
            this.IncomingAll = new ConcurrentQueue<Packet>();
            this.ClientClock = new TimeSpan();
            this.SyncedPackets = new Queue<Packet>();
        }

        /// <summary>
        /// Called when communication with server times out
        /// </summary>
        private void Disconnected()
        {
            this.IsRunning = false;
            "receiving pakets from server timed out".ToConsole();
            this.Timeout = -1;
            this.World = null;
            Engine.Map = null;
            this.IncomingAll = new ConcurrentQueue<Packet>();
            this.SyncedPackets = new Queue<Packet>();

            Instance.NetworkObjects.Clear();
            ScreenManager.GameScreens.Clear();
            ScreenManager.Add(MainScreen.Instance);
            this.EventOccured(Message.Types.ServerNoResponse);
            this.ClientClock = new TimeSpan();

        }

        public void Connect(string address, string playername, AsyncCallback callBack)
        {
            this.Connect(address, new PlayerData(playername), callBack);
        }
        public void Connect(string address, PlayerData playerData, AsyncCallback callBack)
        {
            this.SyncedPackets = new Queue<Packet>();
            this.Timeout = this.TimeoutLength;
            this.LastReceivedTime = int.MinValue;
            this.IsRunning = true;
            this.ChunkCallBackEvents = new ConcurrentDictionary<Vector2, ConcurrentQueue<Action<Chunk>>>();
            this.RecentPackets = new Queue<long>();
            this.RemoteSequence = 0;
            this.RemoteOrderedReliableSequence = 0;
            this.PlayerData = playerData;
            this._packetID = 1;
            Instance.OutgoingStream = new BinaryWriter(new MemoryStream());
            this.IncomingOrderedReliable.Clear();
            this.IncomingOrdered.Clear();
            this.IncomingSynced.Clear();
            this.IncomingAll = new ConcurrentQueue<Packet>();
            this.ClientClock = new TimeSpan();
            this.Players = new PlayerList(this);
            if (this.Host != null)
                this.Host.Close();
            this.Host = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            this.Host.ReceiveBufferSize = this.Host.SendBufferSize = Packet.Size;

            if (!IPAddress.TryParse(address, out IPAddress ipAddress))
            {
                var fromdns = Dns.GetHostEntry(address);
                ipAddress = fromdns.AddressList[0];
            }

            this.RemoteIP = new IPEndPoint(ipAddress, 5541);
            var state = new UdpConnection("Server", this.Host) { Buffer = new byte[Packet.Size] };
            this.Host.Bind(new IPEndPoint(IPAddress.Any, 0));

            byte[] data = Packet.Create(this.PacketID, PacketType.RequestConnection, Network.Serialize(w =>
            {
                w.Write(playerData.Name);
            })).ToArray();

            this.Host.SendTo(data, this.RemoteIP);
            this.Host.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, a =>
            {
                // connection established
                // enter main receive loop
                callBack(a);
                this.ReceiveMessage(a);
            }, state);
        }

        public IEnumerable<PlayerData> GetPlayers()
        {
            return this.Players.GetList();
        }

        public void Update()
        {
            this.Timeout--;
            if (this.Timeout == 0)
                this.Disconnected();
            if (!this.IsRunning)
                return;

            this.HandleOrderedPackets();
            this.HandleOrderedReliablePackets();
            this.ProcessSyncedPackets();
            this.ProcessIncomingPackets();
            GameMode.Current?.Update(Instance);

            if (Instance.Map is not null)
            {
                var size = Instance.Map.GetSizeInChunks();
                var maxChunks = size * size;
                if (Instance.Map.ActiveChunks.Count == maxChunks && !IsSaving)
                {
                    for (int i = 0; i < Instance.Speed; i++)
                        this.TickMap();
                    this.Map.Update();
                    this.UpdateWorldState();
                }
            }
            this.ClientClock = this.ClientClock.Add(TimeSpan.FromMilliseconds(Server.ClockIntervalMS));

            if (this.PlayerData is not null && this.Map is not null)
                PacketMousePosition.Send(Instance, this.PlayerData.ID, ToolManager.CurrentTarget); // TODO: do this at the toolmanager class instead of here

            this.SendAcks();
            this.SendOutgoingStream();
        }

        readonly SortedDictionary<ulong, (ulong worldtick, double servertick, byte[] data)> BufferTimestamped = new();

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
        void HandleBufferedTimestamped()
        {
            while (this.BufferTimestamped.Any())
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
            this.HandleBufferedTimestamped();
            this.Map.UpdateParticles();
            this.Map.World.Tick(Instance);
            this.Map.Tick();
            //this.HandleBufferedTimestamped();
            /// move this to after the map ticks because workcomponent ticks before aicomponent, 
            /// which results new interactions getting ticked in the next frame on the server,
            /// but getting ticked in the same frame when received on the client
            /// SOLUTIONS:
            /// 1) manually add aicomponent before workcomponent inside entities
            /// 2) process packets after ticking map
            /// 3) add a timestamp on the actual interaction class during the frame that it's first ticked on the server, and make clients tick it only then as well
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

            ToolManager.OnGameEvent(this, e);
            this.Map?.OnGameEvent(e);
        }

        [Obsolete]
        static readonly Dictionary<PacketType, Action<INetwork, BinaryReader>> PacketHandlersNew = new();
        [Obsolete]
        public static void RegisterPacketHandler(PacketType channel, Action<INetwork, BinaryReader> handler)
        {
            PacketHandlersNew.Add(channel, handler);
        }

        static readonly Dictionary<int, PacketHandler> PacketHandlersNewNewNew = new();
        internal static void RegisterPacketHandler(int id, PacketHandler handler)
        {
            PacketHandlersNewNewNew.Add(id, handler);
        }

        public void EventOccured(Message.Types type, params object[] p)
        {
            var e = new GameEvent(this.ClientClock.TotalMilliseconds, type, p);
            this.OnGameEvent(e);
        }

        private void ProcessSyncedPackets()
        {
            while (this.SyncedPackets.Count > 0)
            {
                var next = this.SyncedPackets.Peek();
                if (next.Tick > this.ClientClock.TotalMilliseconds)
                    return;
                this.SyncedPackets.Dequeue();
                this.HandleMessage(next);
            }
        }
        private void ProcessIncomingPackets()
        {
            while (this.IncomingAll.TryDequeue(out Packet packet))
            {
                if (packet.PacketType == PacketType.Chunk)
                    (DateTime.Now.ToString() + " " + packet.PacketType.ToString() + " dequeued").ToConsole();

                // if the timer is not stopped (not -1), reset it
                if (this.Timeout > -1)
                    this.Timeout = this.TimeoutLength;

                if (this.IsDuplicate(packet))
                {
                    continue;
                }
                this.RecentPackets.Enqueue(packet.ID);
                if (this.RecentPackets.Count > this.RecentPacketBufferSize)
                    this.RecentPackets.Dequeue();

                // for ordered packets, only handle last one (store most recent and discard and older ones)
                if (packet.SendType == SendType.Ordered)
                {
                    this.IncomingOrdered.Enqueue(packet.ID, packet);//e);
                }
                else if (packet.SendType == SendType.OrderedReliable)
                {
                    this.IncomingOrderedReliable.Enqueue(packet.OrderedReliableID, packet);
                }
                else
                {
                    var clientms = packet.Tick - ClientClockDelayMS;
                    if (this.CurrentTick < clientms)
                    {
                        this.ClientClock = TimeSpan.FromMilliseconds(clientms);
                        "client clock caught up".ToConsole();
                    }
                    this.HandleMessage(packet);
                }
            }
        }

        public void SavePlayer(GameObject actor, BinaryWriter writer)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, "Character");

            var charTag = new SaveTag(SaveTag.Types.Compound, "PlayerCharacter", actor.SaveInternal());

            // save metadata such as hotbar

            tag.Add(charTag);

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

                if (PacketHandlersNew.TryGetValue(type, out Action<INetwork, BinaryReader> handlerAction))
                    handlerAction(Instance, r);
                else if (PacketHandlersNewNewNew.TryGetValue(id, out var handlerActionNewNew))
                    handlerActionNewNew(Instance, r);
                else
                    this.Receive(type, r);
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
                    //Ingame.Instance.Hud.Chat.Write(Start_a_Town_.Log.EntryTypes.System, "Map saved");
                    Log.System("Game saved");
                    break;

                default:
                    break;
            }
        }

        private void HandleMessage(Packet msg)
        {
            if (PacketHandlersNew.TryGetValue(msg.PacketType, out Action<INetwork, BinaryReader> handlerNew))
            {
                Network.Deserialize(msg.Payload, r => handlerNew(Instance, r));
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
                    this.ConsoleBox.Write(Color.Lime, "CLIENT", "Connected to " + Instance.RemoteIP.ToString());
                    GameMode.Current.PlayerIDAssigned(Instance);
                    Instance.SyncTime(msg.Tick);
                    Instance.EventOccured(Message.Types.ServerResponseReceived);
                    break;

                case PacketType.PlayerDisconnected:
                    int plid = msg.Payload.Deserialize<int>(r => r.ReadInt32());
                    Instance.PlayerDisconnected(plid);
                    break;

                case PacketType.PlayerInput:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        var target = TargetArgs.Read(Instance, r);
                        if (Instance.GetNetworkObject(netid) is not Actor obj)
                            return;
                        var input = new PlayerInput(r);
                        var interaction = Start_a_Town_.PlayerInput.GetDefaultInput(obj, target, input);
                        obj.Work.Perform(interaction, target);
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

                case PacketType.SpawnChildObject:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        GameObject obj = GameObject.Create(r);
                        if (obj.RefID == 0)
                            throw new Exception("Uninstantiated entity");
                        if (!Instance.NetworkObjects.ContainsKey(obj.RefID))
                            Instance.Instantiate(obj);

                        int parentID = r.ReadInt32();
                        if (!Instance.TryGetNetworkObject(parentID, out GameObject parent))
                            throw (new Exception("Parent doesn't exist"));

                        obj.Parent = parent;
                        int childIndex = r.ReadInt32();
                        var slot = parent.GetChildren()[childIndex];
                        slot.Object = obj;
                    });
                    return;

                case PacketType.InstantiateObject: //register netID to list without spawning
                    var ent = Network.Deserialize<GameObject>(msg.Payload, GameObject.Create);
                    ent.Instantiate(Instance.Instantiator);
                    return;

                case PacketType.ServerBroadcast:
                    Network.Deserialize(msg.Payload, reader =>
                    {
                        string chatText = reader.ReadASCII();
                        Network.Console.Write(Color.Yellow, "SERVER", chatText);
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

                case PacketType.PlayerSlotClick:
                    msg.Payload.Deserialize(r =>
                    {
                        var actor = TargetArgs.Read(Instance, r);
                        var t = TargetArgs.Read(Instance, r);
                        var parent = t.Slot.Parent;
                        Instance.PostLocalEvent(parent, Components.Message.Types.SlotInteraction, actor.Object, t.Slot);
                    });
                    return;

                case PacketType.PlayerServerCommand:
                    msg.Payload.Deserialize(r => Instance.ParseCommand(r.ReadASCII()));
                    break;

                case PacketType.SetSaving:
                    msg.Payload.Deserialize(r => this.SetSaving(r.ReadBoolean()));
                    break;

                case PacketType.MergedPackets:
                    this.UnmergePackets(msg.Decompressed);
                    break;

                default:
                    break;
            }
        }

        private void SetSaving(bool val)
        {
            IsSaving = val;
            Log.System(IsSaving ? "Saving..." : "Game saved");
        }

        private void SyncTime(double serverMS)
        {
            if (this.LastReceivedTime > serverMS)
            {
                ("sync time packet dropped (last: " + this.LastReceivedTime.ToString() + ", received: " + serverMS.ToString()).ToConsole();// + "server: " + Server.ServerClock.TotalMilliseconds.ToString() + ")").ToConsole();
                return;
            }

            this.LastReceivedTime = serverMS;
            var newtime = serverMS - ClientClockDelayMS;

            var serverTime = TimeSpan.FromMilliseconds(newtime);

            this.ClientClock = serverTime;
        }

        private void ParseCommand(string command)
        {
            CommandParser.Execute(this, command);
        }

        readonly int OrderedPacketsHistoryCapacity = 32;
        readonly Queue<Packet> OrderedPacketsHistory = new(32);
        void HandleOrderedPackets()
        {
            while (this.IncomingOrdered.Count > 0)
            {
                Packet packet = this.IncomingOrdered.Dequeue();

                this.HandleMessage(packet);
                this.OrderedPacketsHistory.Enqueue(packet);
                while (this.OrderedPacketsHistory.Count > this.OrderedPacketsHistoryCapacity)
                    this.OrderedPacketsHistory.Dequeue();
            }
        }
        Queue<Packet> SyncedPackets = new();

        void HandleOrderedReliablePackets()
        {
            while (this.IncomingOrderedReliable.Count > 0)
            {
                var next = this.IncomingOrderedReliable.Peek();
                long nextid = next.OrderedReliableID;
                if (nextid == this.RemoteOrderedReliableSequence + 1)
                {

                    this.RemoteOrderedReliableSequence = nextid;
                    Packet packet = this.IncomingOrderedReliable.Dequeue();
                    if (next.Tick > Instance.Clock.TotalMilliseconds) // TODO maybe use this while changing clock to ad
                    {
                        this.SyncedPackets.Enqueue(next);
                        continue;
                    }
                    this.HandleMessage(packet);
                    this.OrderedReliablePacketsHistory.Enqueue(packet);
                    while (this.OrderedReliablePacketsHistory.Count > OrderedReliablePacketsHistoryCapacity)
                        this.OrderedReliablePacketsHistory.Dequeue();
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
            return this.DisposeObject(obj.RefID);
        }
        public bool DisposeObject(int netID)
        {
            if (!this.NetworkObjects.TryGetValue(netID, out GameObject o))
                return false;
            Console.WriteLine($"{this} disposing {o.DebugName}");
            this.NetworkObjects.Remove(netID);
            o.Net = null;
            if (o.IsSpawned)
                o.Despawn();
            foreach (var child in from slot in o.GetChildren() where slot.HasValue select slot.Object)
                this.DisposeObject(child);
            return true;
        }

        /// <summary>
        /// Is passed recursively to an object and its children objects (inventory items) to register their network ID.
        /// </summary>
        /// <param name="ob"></param>
        /// <returns></returns>
        public GameObject Instantiate(GameObject ob)
        {
            ob.Instantiate(this.Instantiator);
            return ob;
        }

        public void Instantiator(GameObject ob)
        {
            ob.Net = this;
            Instance.NetworkObjects.Add(ob.RefID, ob);
        }

        public IWorld World;

        internal void AddPlayer(PlayerData player)
        {
            this.Players.Add(player);
            UI.LobbyWindow.RefreshPlayers(this.Players.GetList());
            UI.LobbyWindow.Instance.Console.Write(Color.Yellow, player.Name + " connected");
            //UI.UIChat.Instance.Write(new Log.Entry(Start_a_Town_.Log.EntryTypes.System, player.Name + " connected"));
            Log.System($"{player.Name} connected");
        }
        void PlayerDisconnected(PlayerData player)
        {
            this.Players.Remove(player);
            if (Instance.Map != null && player.ControllingEntity != null)
            {
                Instance.NetworkObjects[player.CharacterID].Despawn();
                Instance.DisposeObject(player.CharacterID);
            }
            Network.Console.Write(Color.Yellow, player.Name + " disconnected");
            //UI.UIChat.Instance.Write(new Log.Entry(Start_a_Town_.Log.EntryTypes.System, player.Name + " disconnected"));
            Log.System($"{player.Name} disconnected");
        }
        public void PlayerDisconnected(int playerID)
        {
            PlayerData player = this.Players.GetList().FirstOrDefault(p => p.ID == playerID);
            if (player is null)
                return;
            this.PlayerDisconnected(player);
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
                this.Host.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, this.ReceiveMessage, state);

                Packet packet = Packet.Read(bytesReceived);

                if ((packet.SendType & SendType.Reliable) == SendType.Reliable)
                    Packet.Send(this.PacketID, PacketType.Ack, Network.Serialize(w => w.Write(packet.ID)), this.Host, this.RemoteIP);

                this.IncomingAll.Enqueue(packet);
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception e)
            {
                //ScreenManager.Remove(); // this is not the main thread. if i remove from here then in case the main thread is drawing, it won't be able to access the ingame camera class
                //// so either don't remove the screen here, or pass the camera to the root draw call instead of accessing it through the screenmanager currentscreen property
                this.Timeout = 0;
                e.ShowDialog();
            }
        }
        readonly int RecentPacketBufferSize = 32;
        Queue<long> RecentPackets = new();
        private bool IsDuplicate(Packet packet)
        {
            long id = packet.ID;
            if (id > this.RemoteSequence)
            {
                if (id - this.RemoteSequence > 31)
                {
                    // very large jump in packets
                    this.ConsoleBox.Write(Color.Orange, "CLIENT", "Warning! Large gap in received packets!");
                }
                this.RemoteSequence = id;
                return false;
            }
            else if (id == this.RemoteSequence)
                return true;
            BitVector32 field = this.GenerateBitmask();
            int distance = (int)(this.RemoteSequence - id);
            if (distance > 31)
            {
                // very old packet
                this.ConsoleBox.Write(Color.Orange, "CLIENT", "Warning! Received severely outdated packet: " + packet.PacketType.ToString());
                return false;
            }
            int mask = (1 << distance);
            bool found = (field.Data & mask) == mask;
            if (found)
                if (distance < 32)
                    if (!this.RecentPackets.Contains(id))
                        throw new Exception("duplicate detection error");
            return found;
        }

        BitVector32 GenerateBitmask()
        {
            int mask = 0;
            foreach (var recent in this.RecentPackets)
            {
                int distance = (int)(this.RemoteSequence - recent);
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
            this.ChunkRequests.Remove(chunk.MapCoords);

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
                /// why here too? BECAUSE some things dont get initialized properly on client. like initializing sprites from defs
                //obj.ObjectLoaded();
                /// FIXED by saving and serializing sprites along bones (by using the assetpath and the static sprite registry)
            });

            foreach (var (local, entity) in chunk.GetBlockEntitiesByPosition())
            {
                var global = local.ToGlobal(chunk);
                entity.MapLoaded(this.Map, global);
                entity.Instantiate(global, Instance.Instantiator);
            }

            Instance.Map.AddChunk(chunk);
            return;
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
            this.NetworkObjects.TryGetValue(netID, out var obj);
            return obj;
        }
        public T GetNetworkObject<T>(int netID) where T : GameObject
        {
            this.NetworkObjects.TryGetValue(netID, out var obj);
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
            return this.NetworkObjects.TryGetValue(netID, out obj);
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

            var time = TimeSpan.FromMilliseconds(totalMs);
            var worldState = new WorldSnapshot(time);

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                int netID = reader.ReadInt32();
                var obj = Instance.NetworkObjects.GetValueOrDefault(netID);
                if (obj is null)
                    continue; 
                var objsnapshot = new ObjectSnapshot(obj).Read(reader);
                worldState.ObjectSnapshots.Add(objsnapshot);
            }

            // insert world snapshot to world snapshot history
            this.WorldStateBuffer.Enqueue(worldState);
            while (this.WorldStateBuffer.Count > this.WorldStateBufferSize)
                this.WorldStateBuffer.Dequeue();
        }
        private void UpdateWorldState()
        {
            // iterate through the state buffer and find position
            List<WorldSnapshot> list = this.WorldStateBuffer.ToList();
            for (int i = 0; i < this.WorldStateBuffer.Count - 1; i++)
            {
                WorldSnapshot
                    prev = list[i],
                    next = list[i + 1];

                if (this.ClientClock == next.Time)
                {
                    this.SnapObjectPositions(prev, next);
                    return;
                }
            }
        }

        private void SnapObjectPositions(WorldSnapshot prev, WorldSnapshot next)
        {
            foreach (var objSnapshot in next.ObjectSnapshots)
            {
                var previousObjState = prev.ObjectSnapshots.Find(o => o.Object == objSnapshot.Object);
                if (previousObjState is null)
                    continue;
                objSnapshot.Object.SetPosition(objSnapshot.Position);
                objSnapshot.Object.Velocity = objSnapshot.Velocity;
                objSnapshot.Object.Direction = objSnapshot.Orientation;
                if (float.IsNaN(objSnapshot.Object.Direction.X) || float.IsNaN(objSnapshot.Object.Direction.Y))
                    throw new Exception();
            }
        }

        [Obsolete]
        public static void PlayerInventoryOperationNew(TargetArgs source, TargetArgs target, int amount)
        {
            Network.Serialize(w =>
            {
                source.Write(w);
                target.Write(w);
                w.Write(amount);
            }).Send(Instance.PacketID, PacketType.PlayerInventoryOperationNew, Instance.Host, Instance.RemoteIP);
        }
        [Obsolete]
        public static void PlayerSlotInteraction(GameObjectSlot slot)
        {
            var actor = Instance.GetPlayer().ControllingEntity;
            Network.Serialize(writer =>
            {
                TargetArgs.Write(writer, actor);
                TargetArgs.Write(writer, slot);
            }).Send(Instance.PacketID, PacketType.PlayerSlotClick, Instance.Host, Instance.RemoteIP);
        }
       
        [Obsolete]
        internal static void PlayerStartMoving()
        {
            throw new NotImplementedException();
        }
        [Obsolete]
        internal static void PlayerStopMoving()
        {
            throw new NotImplementedException();
        }
        [Obsolete]
        internal static void PlayerChangeDirection(Vector3 direction)
        {
            throw new NotImplementedException();
        }
        [Obsolete]
        internal static void PlayerJump()
        {
            throw new NotImplementedException();
        }
        [Obsolete]
        internal static void PlayerToggleWalk(bool toggle)
        {
            throw new NotImplementedException();
        }
        [Obsolete]
        internal static void PlayerToggleSprint(bool toggle)
        {
            throw new NotImplementedException();
        }
        [Obsolete]
        internal static void PlayerThrow(Vector3 dir, bool all)
        {
            throw new NotImplementedException();
        }
        [Obsolete]
        internal static void PlayerAttack()
        {
            throw new NotImplementedException();
        }
        [Obsolete]
        internal static void PlayerFinishAttack(Vector3 vector3)
        {
            throw new NotImplementedException();
        }
        [Obsolete]
        internal static void PlayerStartBlocking()
        {
            throw new NotImplementedException();
        }
        [Obsolete]
        internal static void PlayerFinishBlocking()
        {
            throw new NotImplementedException();
        }
        [Obsolete]
        internal static void PlayerInput(TargetArgs targetArgs, PlayerInput input)
        {
            Network.Serialize(w =>
            {
                w.Write(Instance.GetPlayer().ControllingEntity.RefID);
                targetArgs.Write(w);
                input.Write(w);
            }).Send(Instance.PacketID, PacketType.PlayerInput, Instance.Host, Instance.RemoteIP);
        }
        [Obsolete]
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

        void Send(PacketType packetType, byte[] data)
        {
            data.Send(this.PacketID, packetType, this.Host, this.RemoteIP);
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

        public void PopLoot(GameObject loot, Vector3 startPosition, Vector3 startVelocity) { }
        public void PopLoot(LootTable table, Vector3 startPosition, Vector3 startVelocity) { }

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
                    sourceSlot.Object.StackSize -= amount;
                    // DO NOTHING. WAIT FOR NEW OBJECT FROM SERVER INSTEAD
                    return;
                }
                else
                    sourceSlot.Clear();
                targetSlot.Object = obj;
                return;
            }
            if (targetSlot.Object.CanAbsorb(sourceSlot.Object))
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

        public PlayerData GetPlayer(int id)
        {
            return this.Players.GetPlayer(id);
        }
        public PlayerData GetPlayer()
        {
            return this.GetPlayer(this.PlayerData.ID);
        }
        public PlayerData CurrentPlayer => this.PlayerData;

        internal void HandleServerResponse(int playerID, PlayerList playerList, int speed)
        {
            throw new Exception();
        }
        public void SetSpeed(int playerID, int playerSpeed)
        {
            this.GetPlayer(playerID).SuggestedSpeed = playerSpeed;
            var newspeed = this.Players.GetLowestSpeed();
            if (newspeed != this.Speed)
                Ingame.Instance.Hud.Chat.Write(Start_a_Town_.Log.EntryTypes.System, string.Format("Speed set to {0}x ({1})", newspeed, string.Join(", ", this.Players.GetList().Where(p => p.SuggestedSpeed == newspeed).Select(p => p.Name))));
            this.Speed = newspeed;
        }

        public void Write(string text)
        {
            Log.Write(text);
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
