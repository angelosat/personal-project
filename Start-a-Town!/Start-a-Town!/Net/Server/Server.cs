using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Net
{
    public partial class Server : IObjectProvider
    {
        public double CurrentTick => ServerClock.TotalMilliseconds;
      
        static Server()
        {
            RegisterPacketHandler(PacketType.Acks, Instance.ReceiveAcks);
        }

        // TODO: figure out why it's glitching out if I set it lower than 10
        public const int ClockIntervalMS = 10;// 10 is working
        public TimeSpan Clock { get { return ServerClock; } }
        public static TimeSpan ServerClock;

        internal static void ConnectAsHost()
        {
            Instance.PlayerData = new PlayerData("host test");
        }
        
        static bool IsRunning;

        bool IsSaving;
        
        UI.ConsoleBoxAsync _Console;
        public UI.ConsoleBoxAsync Log
        {
            get
            {
                if (this._Console == null)
                    this._Console = new UI.ConsoleBoxAsync(new Rectangle(0, 0, 800, 600)) { FadeText = false };
                return this._Console;
            }
        }

        [Obsolete]
        internal void AIInteract(Actor parent, Interaction goal, TargetArgs target)
        {
            AI.AIManager.Interact(parent, goal, target);
        }
        
        public const int SnapshotIntervalMS = 10;// send 60 snapshots per second to clients
        public const int LightIntervalMS = 10;// send 60 light updates per second to clients
        readonly Dictionary<int, GameObject> NetworkObjects;

        /// <summary>
        /// Contains objects that have changed since the last world delta state update
        /// </summary>
        public HashSet<GameObject> ObjectsChangedSinceLastSnapshot = new();

        [Obsolete]
        readonly static Dictionary<PacketType, Action<IObjectProvider, PlayerData, BinaryReader>> PacketHandlersNewNew = new();
        [Obsolete]
        static public void RegisterPacketHandler(PacketType channel, Action<IObjectProvider, PlayerData, BinaryReader> handler)
        {
            PacketHandlersNewNew.Add(channel, handler);
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

        static void OnGameEvent(GameEvent e)
        {
            GameMode.Current.HandleEvent(Instance, e);
            foreach (var item in Game1.Instance.GameComponents)
                item.OnGameEvent(e);

            Instance.Map.OnGameEvent(e);
        }
        public void EventOccured(Components.Message.Types type, params object[] p)
        {
            var e = new GameEvent(this, ServerClock.TotalMilliseconds, type, p);
            OnGameEvent(e);
        }
        
        static public CancellationTokenSource ChunkLoaderToken = new();
        static Server _Instance;
        static public Server Instance => _Instance ??= new Server();
        public NetworkSideType Type { get { return NetworkSideType.Server; } }

        static int _objID = 1;

        public static int GetNextObjID()
        {
            return _objID++;
        }
        static int _playerID = 1;
        public static int PlayerID
        {
            get { return _playerID++; }
        }
        public Server()
        {
            NetworkObjects = new Dictionary<int, GameObject>();
        }
        private void AdvanceClock()
        {
            ServerClock = ServerClock.Add(TimeSpan.FromMilliseconds(ClockIntervalMS));
        }

        /// <summary>
        /// TODO: Make it a field
        /// </summary>
        public MapBase Map { get; set; }

        private void SetMap(MapBase map)
        {
            this.Map = map;
            map.ResolveReferences();
        }

        static IWorld _World;
        static IWorld World
        {
            get { return _World; }
            set { _World = value; }
        }
        public static int Port = 5541;
        static Socket Listener;
        public PlayerList Players;

        public IEnumerable<PlayerData> GetPlayers()
        {
            return Players.GetList();
        }

        static public RandomThreaded Random;

        static public void Stop()
        {
            Server.ChunkLoaderToken.Cancel();
            if (Listener is not null)
                Listener.Close();

            if (Instance.Log != null)
                Instance.Log.Write("SERVER", "Stopped");
            Instance.OutgoingStream = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamUnreliable = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamReliable = new BinaryWriter(new MemoryStream());

            Instance.Players = new PlayerList(Instance);
            IsRunning = false;
            Instance.Map = null;
            Connections = new ConcurrentDictionary<EndPoint, UdpConnection>();
            ServerClock = new TimeSpan();
            Instance.Speed = 0;
        }
        static public void Start()
        {
            Instance.Speed = 0;
            IsRunning = true;
            Instance.OutgoingStream = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamUnreliable = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamReliable = new BinaryWriter(new MemoryStream());

            Connections = new ConcurrentDictionary<EndPoint, UdpConnection>();
            ServerClock = new TimeSpan();
            Instance.Log.Write("SERVER", "Started");
            if (Listener != null)
                Listener.Close();
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Listener.ReceiveBufferSize = Listener.SendBufferSize = Packet.Size;
            Instance.Players = new PlayerList(Instance);
            var anyIP = new IPEndPoint(IPAddress.Any, Port);
            Listener.Bind(anyIP);


            //// IGNORE CONNECTIONRESET EXCEPTIONS
            ////int SIO_UDP_CONNRESET = -1744830452;
            ////Listener.IOControl(
            ////    (IOControlCode)SIO_UDP_CONNRESET,
            ////    new byte[] { 0, 0, 0, 0 },
            ////    null
            ////);

            uint IOC_IN = 0x80000000;
            uint IOC_VENDOR = 0x18000000;
            uint SIO_UDP_CONNRESET = IOC_IN | IOC_VENDOR | 12;
            Listener.IOControl((int)SIO_UDP_CONNRESET, new byte[] { Convert.ToByte(false) }, null);

            EndPoint remote = new IPEndPoint(IPAddress.Any, Port);
            var state = new UdpConnection("player", remote) { Buffer = new byte[Packet.Size], IP = remote };
            Listener.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ref remote, ar =>
            {
                ReceiveMessage(ar);
            }, state);
            Instance.Log.Write(Color.Yellow, "SERVER", "Listening to port " + Port + "...");
        }
        int _Speed = 0;// 1;
        public int Speed { get { return this._Speed; } set { this._Speed = value; } }
        readonly float MapSaveInterval = Engine.TicksPerSecond * 60f;
        float MapSaveTimer = 0;
        readonly float BlockUpdateTimerMax = 1;
        float BlockUpdateTimer = 0;

        public void Update(GameTime gt)
        {
            if (!IsRunning)
                return;
            HandleIncoming();
            if (GameMode.Current != null)
                GameMode.Current.Update(Instance);
            if (Instance.Map == null)
                return;
            if (!Instance.IsSaving)
            {
                TickMap();
                UpdateMap();
                SendSnapshots(ServerClock);
            }

            Instance.MapSaveTimer--;
            if (Instance.MapSaveTimer <= 0)
            {
                Instance.MapSaveTimer = Instance.MapSaveInterval;
            }

            /// THESE MUST BE CALLED FROM WITHIN THE GAMESPEED LOOP
            CreatePacketsFromAllStreams();
            ResetOutgoingStreams();
            Instance.AdvanceClock();
            SendPackets();
        }
        private void SendPackets()
        {
            SendUnreliable();
            TryResendPacketsFirst(); //try resend first or all?
            SendOrderedReliable();
        }
        private void CreatePacketsFromAllStreams()
        {
            CreatePacketsFromStreamUnreliable();
            CreatePacketsFromStream();
        }
        private static void ResetOutgoingStreams()
        {
            Instance.OutgoingStream = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamUnreliable = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamReliable = new BinaryWriter(new MemoryStream());
        }
        
        private static void UpdateMap()
        {
            if (Instance.Map != null)
                Instance.Map.Update(Instance);
        }
        private void TickMap()
        {
            if (this.Map is null)
                return;

            var auxStream = new BinaryWriter(new MemoryStream());
            for (int i = 0; i < this.Speed; i++)
            {
                this.OutgoingStreamTimestamped = new(new MemoryStream());
                auxStream.Write(this.Map.World.CurrentTick);
                auxStream.Write(this.CurrentTick);
                this.Map.World.Tick(Instance);
                this.Map.Tick(Instance);
                var length = this.OutgoingStreamTimestamped.BaseStream.Position;
                auxStream.Write(length);// write length
                if (length > 0)
                {
                    this.OutgoingStreamTimestamped.BaseStream.Position = 0;
                    this.OutgoingStreamTimestamped.BaseStream.CopyTo(auxStream.BaseStream);
                }

                this.BlockUpdateTimer--;
                if (this.BlockUpdateTimer <= 0)
                {
                    this.BlockUpdateTimer = Instance.BlockUpdateTimerMax;
                    this.SendRandomBlockUpdates();
                }
                // i moved it from here because the player must be able to issue commands while the game is paused
                //ProcessEvents();
                // CLOCK AND PACKETS MUST BE ADVANCED HERE AND SENT WITH THE CORRECT CLOCK TICK HEADER
                //SendPackets();
                //ResetOutgoingStreams();
                //Instance.AdvanceClock();
            }
            if (auxStream.BaseStream.Position > 0)
            {
                auxStream.BaseStream.Position = 0;
                this.OutgoingStream.Write(Network.Packets.PacketTimestamped);
                auxStream.BaseStream.CopyTo(this.OutgoingStream.BaseStream);
            }
        }

        static public int RandomBlockUpdatesCount = 1;
        static int RandomBlockUpdateIndex = 0;
        void SendRandomBlockUpdates() // TODO move this to map class. server object shouldn't contain map logic
        {
            var tosend = new Vector3[Instance.Map.ActiveChunks.Count];
            var k = 0;
            foreach (var chunk in Instance.Map.ActiveChunks.Values)
            {
                var randcell = chunk.GetRandomCellInOrder(RandomBlockUpdateIndex);
                var global = randcell.ToGlobal(chunk);
                Instance.Map.RandomBlockUpdate(global);
                tosend[k++] = global;
            }
            RandomBlockUpdateIndex++;
            if (RandomBlockUpdateIndex == Chunk.Volume)
                RandomBlockUpdateIndex = 0;
            PacketRandomBlockUpdates.Send(this, tosend);
        }

        private static void HandleIncoming()
        {
            foreach (var player in from conn in Connections select conn.Value.Player)
                while (player.Incoming.TryDequeue(out Packet msg))
                    HandleMessage(msg);
        }
        
        void SendUnreliable()
        {
            foreach (var player in Players.GetList())
                while (player.OutUnreliable.Any())
                {
                    if (!player.OutUnreliable.TryDequeue(out Packet p))
                        return;
                    p.BeginSendTo(Listener, player.IP);
                }
        }
        
        const int RTT = 20000;// 5000;
        void SendOrderedReliable()
        {
            foreach (var player in Players.GetList())
                while (player.OutReliable.Any())
                {
                    if (!player.OutReliable.TryDequeue(out Packet packet))
                        return;
                    if (packet.PacketType == PacketType.RequestConnection)
                    {

                    }
                    packet.Player.WaitingForAck[packet.ID] = packet;
                    packet.BeginSendTo(Listener, player.IP);
                }
        }
        
        void TryResendPacketsFirst()
        {
            foreach (var player in Players.GetList())
            {
                var packet = player.WaitingForAck.Values.FirstOrDefault();
                if (packet != null)
                {
                    if (packet.RTT.ElapsedMilliseconds < RTT)
                        continue;
                    if (packet.Retries-- <= 0)
                    {
                        player.WaitingForAck.TryRemove(packet.ID, out _);
                        Log.Write(UI.ConsoleMessageTypes.Acks, Color.Orange, "SERVER", "Send retries exceeded maximum for packet " + packet);
                        Log.Write(Color.Red, "SERVER", player.Name + " timed out");
                        CloseConnection(player.Connection);
                        //send player disconnected packet to rest of players
                        break;
                    }
                    packet.BeginSendTo(Listener, player.IP);
                }
            }
        }
        
        private void CreatePacketsFromStream()
        {
            byte[] data;
            using (var output = new MemoryStream())
            {
                using (var zip = new GZipStream(output, CompressionMode.Compress))
                {
                    OutgoingStream.BaseStream.Position = 0;
                    OutgoingStream.BaseStream.CopyTo(zip);
                }
                data = output.ToArray();
            }
            //if (data.Length > 0) // send empty packet anyway to substitute pinging for keeping an active connection
            Enqueue(PacketType.MergedPackets, data);
        }
        private void CreatePacketsFromStreamUnreliable()
        {
            byte[] data;
            using (var output = new MemoryStream())
            {
                using (var zip = new GZipStream(output, CompressionMode.Compress))
                {
                    OutgoingStreamUnreliable.BaseStream.Position = 0;
                    OutgoingStreamUnreliable.BaseStream.CopyTo(zip);
                }
                data = output.ToArray();
            }
            //if (data.Length > 0) // send empty packet anyway to substitute pinging for keeping an active connection
            EnqueueUnreliable(PacketType.MergedPackets, data);
        }
       
        internal void EnqueueUnreliable(PacketType packetType, byte[] p)
        {
            this.Enqueue(packetType, p, SendType.Unreliable, true);
        }
        internal void Enqueue(PacketType packetType, byte[] p, bool sync = true)
        {
            this.Enqueue(packetType, p, SendType.OrderedReliable, sync);
        }

        internal void Enqueue(PlayerData player, Packet packet)
        {
            if ((packet.SendType & SendType.Reliable) == SendType.Reliable)
            {
                if (packet.SendType == SendType.OrderedReliable)
                    packet.Tick = this.Clock.TotalMilliseconds;
                player.OutReliable.Enqueue(packet);
            }
            else
                player.OutUnreliable.Enqueue(packet);
        }
        public void Enqueue(PacketType type, byte[] data, SendType send)
        {
            if (data.Length > 60000)
            {
                foreach (var player in Players.GetList())
                {
                    throw new NotImplementedException();
                    
                }
                return;
            }
            foreach (var player in Players.GetList())
                Enqueue(player, Packet.Create(player, type, data, send));
        }
        internal void Enqueue(PacketType type, byte[] data, SendType send, Vector3 global)
        {
            this.Enqueue(type, data, send, player => player.IsWithin(global));
        }
        internal void Enqueue(PacketType type, byte[] data, SendType send, Vector3 global, bool sync)
        {
            foreach (var player in Players.GetList().Where(player => player.IsWithin(global)))
            {
                var p = Packet.Create(player, type, data, send);
                var t = this.Clock.TotalMilliseconds;
                p.Synced = sync;
                p.Tick = this.Clock.TotalMilliseconds;
                Enqueue(player, p);
            }
        }
        internal void Enqueue(PacketType type, byte[] data, SendType send, bool sync)
        {
            foreach (var player in Players.GetList())
            {
                var p = Packet.Create(player, type, data, send);
                p.Synced = sync;
                p.Tick = this.Clock.TotalMilliseconds;
                Enqueue(player, p);
            }
        }
        internal void Enqueue(PacketType type, byte[] data, SendType send, Func<PlayerData, bool> filter)
        {
            if (data.Length > 60000)
            {
                foreach (var player in Players.GetList().Where(filter))
                {
                    throw new NotImplementedException();
                }
                return;
            }
            foreach (var player in Players.GetList().Where(filter))
                Enqueue(player, Packet.Create(player, type, data, send));
        }

        public BinaryWriter OutgoingStream = new(new MemoryStream());
        public BinaryWriter OutgoingStreamUnreliable = new(new MemoryStream());
        public BinaryWriter OutgoingStreamReliable = new(new MemoryStream());
        public BinaryWriter OutgoingStreamTimestamped = new(new MemoryStream());
        
        public BinaryWriter GetOutgoingStream()
        {
            return this.OutgoingStream;
        }
        static ConcurrentDictionary<EndPoint, UdpConnection> Connections = new();
        static void ReceiveMessage(IAsyncResult ar)
        {
            UdpConnection state = (UdpConnection)ar.AsyncState;
            EndPoint remoteIP = state.IP;
            EndPoint anyIP = new IPEndPoint(IPAddress.Any, Port);
            try
            {
                Packet packet = Packet.Read(state.Buffer);
                int bytesReceived = Listener.EndReceiveFrom(ar, ref remoteIP);

                state.Buffer = new byte[Packet.Size];
                Listener.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ref anyIP, ReceiveMessage, state);

                if (!Connections.TryGetValue(remoteIP, out state))
                {
                    // only register new connection if it's a requestplayerid packet type
                    if (packet.PacketType != PacketType.RequestConnection)
                    {
                        //throw new Exception("Invalid sender");
                        return;
                    }
                    Server.Instance.Log.Write(Color.Yellow, "SERVER", remoteIP + " connecting...");
                    UdpConnection newConnection = CreateConnection(remoteIP);
                    state = newConnection;
                }

                packet.Connection = state;
                packet.Player = state.Player;

                packet.Player.Incoming.Enqueue(packet);
            }
            catch (SocketException)
            {
                "connection closed".ToConsole();
                CloseConnection(state);
            }
            catch (ObjectDisposedException) { }
        }

        private static UdpConnection CreateConnection(EndPoint remoteIP)
        {
            var newPlayer = new PlayerData(remoteIP);
            var newConnection = new UdpConnection(newPlayer.IP.ToString(), remoteIP)
            {
                Player = newPlayer
            };
            newPlayer.Connection = newConnection;
            Connections.TryAdd(newConnection.IP, newConnection);

            newConnection.Ping = new System.Diagnostics.Stopwatch();

            return newConnection;
        }

        private static void HandleMessage(Packet msg)
        {
            switch (msg.PacketType)
            {
                case PacketType.RequestConnection:

                    string name = Network.Deserialize<string>(msg.Payload, r =>
                    {
                        return r.ReadString();
                    });

                    Instance.Log.Write(Color.Lime, "SERVER", name + " connected from " + msg.Connection.IP);

                    PlayerData pl = msg.Connection.Player;
                    pl.Name = name;
                    pl.ID = PlayerID;
                    pl.SuggestedSpeed = Instance.Speed;
                    msg.Player = pl;

                    // send packet back to the player
                    Instance.Enqueue(pl, Packet.Create(msg.Player, msg.PacketType, Network.Serialize(w =>
                    {
                        w.Write(msg.Player.ID);
                        Instance.Players.Write(w);
                        w.Write(Instance.Speed);
                    }), SendType.Reliable | SendType.Ordered));

                    var state = new UdpConnection(pl.Name + " listener", pl.IP) { Buffer = new byte[Packet.Size], Player = pl };
                    EndPoint ip = state.IP;
                    Listener.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ref ip, ReceiveMessage, state);
                    PacketPlayerConnecting.Send(Instance, msg.Player);

                    // add to players when entering world instead?
                    Instance.Players.Add(pl);
                    GameMode.Current.PlayerConnected(Instance, msg.Player);
                    return;

                case PacketType.PlayerDisconnected:
                    CloseConnection(msg.Connection);
                    break;

                case PacketType.PlayerServerCommand:
                    msg.Payload.Deserialize(r =>
                    {
                        CommandParser.Execute(Instance, msg.Player, r.ReadASCII());
                    });
                    break;

                case PacketType.PlayerInventoryOperationNew:
                    msg.Payload.Deserialize(r =>
                    {
                        var source = TargetArgs.Read(Instance, r);
                        var destination = TargetArgs.Read(Instance, r);
                        int amount = r.ReadInt32();
                        Instance.InventoryOperation(source.Slot, destination.Slot, amount);
                        Instance.SyncSlots(source, destination);
                        return;
                    });
                    return;
               

                case PacketType.PlayerSlotClick:
                    msg.Payload.Deserialize(r =>
                    {
                        TargetArgs actor = TargetArgs.Read(Instance, r);
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        Instance.PostLocalEvent(target.Slot.Parent, Components.Message.Types.SlotInteraction, actor.Object, target.Slot);
                    });
                    Instance.Enqueue(PacketType.PlayerSlotClick, msg.Payload, SendType.OrderedReliable);
                    return;

                case PacketType.PlayerSetBlock:
                    msg.Payload.Deserialize(r =>
                    {
                        Vector3 global = r.ReadVector3();
                        var type = (Block.Types)r.ReadInt32();
                        var data = r.ReadByte();
                        var variation = r.ReadInt32();
                        var orientation = r.ReadInt32();

                        if (!Instance.Map.IsInBounds(global))
                            return;
                        // DONT CALL PREVIOUS BLOCK'S REMOVE METHOD
                        // when in block editing mode, we don't want to call block's remove method, so for example they don't pop out their contents or have any other effects to the world
                        // HOWEVER we want to dispose their contents (gameobjects) if any! 
                        // so 1) query their contents and dispose them here? 
                        //    2) call something like dispose() on them and let them dispose them themselves?
                        // TODO: DECIDE!
                        Instance.Map.RemoveBlock(global);

                        if (type != Block.Types.Air)
                        {
                            var block = Block.Registry[type];
                            block.Place(Instance.Map, global, data, variation, orientation);
                        }
                        Instance.Enqueue(PacketType.PlayerSetBlock, msg.Payload, SendType.OrderedReliable, global, true);
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
                        Instance.Enqueue(PacketType.PlayerRemoteCall, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global);
                    });
                    return;

                case PacketType.PlayerInput:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        var input = new PlayerInput(r);
                        var interaction = PlayerInput.GetDefaultInput(msg.Player.ControllingEntity, target, input);
                        if (interaction == null)
                            return;
                        msg.Player.ControllingEntity.Work.Perform(interaction, target);
                        if (interaction.Conditions.Evaluate(msg.Player.ControllingEntity, target))
                            Instance.Enqueue(PacketType.PlayerInput, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;

                case PacketType.Ack:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        long ackID = r.ReadInt64();
                        if (msg.Player.WaitingForAck.TryRemove(ackID, out Packet existing))
                        {
                            existing.RTT.Stop();
                            msg.Connection.RTT = TimeSpan.FromMilliseconds(existing.RTT.ElapsedMilliseconds);
                            msg.Player.Ping = TimeSpan.FromMilliseconds(existing.RTT.ElapsedMilliseconds).Milliseconds;
                            if (msg.Player.OrderedPackets.Count > 0)
                                if (msg.Player.OrderedPackets.Peek().ID == ackID)
                                    msg.Player.OrderedPackets.Dequeue();
                        }
                    });
                    break;

                case PacketType.MergedPackets:
                    UnmergePackets(msg.Player, msg.Decompressed);
                    break;

                default:
                    GameMode.Current.HandlePacket(Instance, msg);

                    break;
            }
        }

        public void SpawnRequestFromTemplate(int templateID, TargetArgs target)
        {
            var entity = GameObject.Templates[templateID].Clone();

            entity.Initialize(Random);
            target.Map = Instance.Map;
            switch (target.Type)
            {
                case TargetType.Slot:
                    Instance.Instantiate(entity);
                    target.Slot.Object = entity;
                    Instance.SyncChild(entity, target.Slot.Parent, target.Slot.ID);
                    break;

                case TargetType.Position:
                    entity.Global = target.Global;
                    this.Instantiate(entity);
                    entity.Spawn(this.Map, target.Global);
                    PacketEntityInstantiate.SendFromTemplate(this, templateID, entity);
                    break;

                default:
                    break;
            }
        }
       
        public static Vector3 FindValidSpawnPosition(GameObject obj)
        {
            // TODO: move to map
            var xy = Vector3.Zero + (Vector3.UnitX + Vector3.UnitY) * (GameModes.StaticMaps.StaticMap.MapSize.Default.Blocks / 2);
            int z = MapBase.MaxHeight - (int)Math.Ceiling(obj.Physics.Height);
            while (!Instance.Map.IsSolid(xy + Vector3.UnitZ * z))
                z--;

            var zz = Vector3.UnitZ * (z + 1);
            var spawnPosition = xy + zz;
            return spawnPosition;
        }
       
        void SyncChild(GameObject obj, GameObject parent, int childIndex)
        {
            byte[] data = Network.Serialize(w =>
            {
                obj.Write(w);
                w.Write(parent.RefID);
                w.Write(childIndex);
            });
            foreach (var player in Players.GetList())
                Enqueue(player, Packet.Create(player, PacketType.SpawnChildObject, data, SendType.Ordered | SendType.Reliable));
        }
        
        private static void KickPlayer(int plid)
        {
            CloseConnection(Instance.Players.GetList().First(p => p.ID == plid).Connection);
        }
        
        static void CloseConnection(UdpConnection connection)
        {
            connection.Ping.Stop();
            if (!Connections.TryRemove(connection.IP, out UdpConnection existing))
            {
                throw new Exception("Tried to close nonexistent connection");
            }

            "connection closed".ToConsole();
            Instance.Players.Remove(existing.Player);
            if (existing.Player.IsActive)
                Instance.Despawn(existing.Player.ControllingEntity);
            Instance.DisposeObject(existing.Player.CharacterID);
            PacketPlayerDisconnected.Send(Instance, existing.Player.ID);
        }

        public void SyncEvent(GameObject recipient, Message.Types msg, Action<BinaryWriter> writer)
        {
            byte[] data;
            using (var w = new BinaryWriter(new MemoryStream()))
            {
                writer(w);
                data = (w.BaseStream as MemoryStream).ToArray();
            }
            byte[] payload = Network.Serialize(w =>
            {
                w.Write(ServerClock.TotalMilliseconds);
                TargetArgs.Write(w, recipient);
                w.Write((int)msg);
                w.Write(data.Length);
                w.Write(data);
            });
            recipient.HandleRemoteCall(ObjectEventArgs.Create(msg, writer));
            Enqueue(PacketType.ObjectEvent, payload, SendType.Ordered | SendType.Reliable);
        }

        public GameObject Instantiate(GameObject obj)
        {
            obj.Instantiate(Instantiator);
            return obj;
        }
        public void Instantiator(GameObject obj)
        {
            if (obj.RefID == 0)
                obj.RefID = GetNextObjID();
            else
                _objID = Math.Max(_objID, obj.RefID + 1);
            obj.Net = this;
            Instance.NetworkObjects.Add(obj.RefID, obj);
        }

        /// <summary>
        /// Instantiates an object on the server and syncs it across the clients
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        [Obsolete]
        public GameObject InstantiateObject(GameObject obj)
        {
            if (obj.RefID > 0)
                throw new Exception("object already has a network id");
            obj.Instantiate(Instantiator);
            byte[] data = Network.Serialize(obj.Write);
            foreach (var player in Players.GetList())
            {
                Enqueue(player, Packet.Create(player, PacketType.InstantiateObject, data, SendType.Ordered | SendType.Reliable));
            }
            return obj;
        }

        /// <summary>
        /// Removes an object from the game world without releasing its NetworkID.
        /// </summary>
        /// <param name="obj"></param>
        public void Despawn(GameObject obj)
        {
            obj.Despawn();
        }
      
        /// <summary>
        /// Releases the object's networkID.
        /// </summary>
        /// <param name="objNetID"></param>
        public bool DisposeObject(GameObject obj)
        {
            return DisposeObject(obj.RefID);
        }
        /// <summary>
        /// Releases the object's networkID.
        /// </summary>
        /// <param name="objNetID"></param>
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
       
        static public void InstantiateMap(MapBase map)
        {
            if (map is null)
                return;
            World = map.World;
            Instance.Map = map;
            Instance.Map.Net = Instance;

            foreach (var ch in map.GetActiveChunks().Values)
                InstantiateChunk(ch);
            foreach (var obj in Instance.NetworkObjects)
                obj.Value.MapLoaded(Instance.Map);
            Instance.SetMap(map);
            Random = new RandomThreaded(Instance.Map.Random);
        }
      
        private static void AddChunk(Chunk chunk)
        {
            var actives = Instance.Map.GetActiveChunks();
            actives.Add(chunk.MapCoords, chunk);
        }
        private static void InstantiateChunk(Chunk chunk)
        {
            chunk.GetObjects().ForEach(obj =>
            {
                Instance.Instantiate(obj);
                _objID = Math.Max(_objID, obj.RefID + 1);
                obj.Map = Instance.Map;
                obj.Transform.Exists = true;
            });
           
            foreach (var (local, entity) in chunk.GetBlockEntitiesByPosition())
            {
                var global = local.ToGlobal(chunk);
                entity.MapLoaded(Instance.Map, global);
                entity.Instantiate(global, Instance.Instantiator);
            }
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
        static void Save()
        {
            if (Instance.Map is null)
                return;
            foreach (var chunk in Instance.Map.GetActiveChunks().Values)
            {
                if (chunk.Saved)
                    continue;
            }
        }

        private static void SendSnapshots(TimeSpan gt)
        {
            if (Instance.ObjectsChangedSinceLastSnapshot.Count > 0)
            {
                PacketSnapshots.Send(Instance, Instance.ObjectsChangedSinceLastSnapshot);
                Instance.ObjectsChangedSinceLastSnapshot.Clear();
            }
        }
        
        public bool LogStateChange(int netID)
        {
            return this.ObjectsChangedSinceLastSnapshot.Add(NetworkObjects[netID]);
        }

        #region Loot
        public void PopLoot(LootTable table, Vector3 startPosition, Vector3 startVelocity)
        {
            foreach (var obj in GenerateLoot(table))
            {
                PopLoot(obj, startPosition, startVelocity);
            }
        }
        public void PopLoot(GameObject obj, Vector3 startPosition, Vector3 startVelocity)
        {
            double angle = Random.NextDouble() * (Math.PI + Math.PI);
            double w = Math.PI / 4f;

            float force = 0.3f;

            float x = 0.1f * (float)(Math.Sin(w) * Math.Cos(angle));
            float y = 0.1f * (float)(Math.Sin(w) * Math.Sin(angle));
            float z = (float)Math.Cos(w);

            var direction = new Vector3(x, y, z);
            var final = startVelocity + force * direction;

            obj.Global = startPosition;
            obj.Velocity = final;

            if (obj.RefID == 0)
                obj.SyncInstantiate(this);
            this.Map.SyncSpawn(obj, startPosition, final);
        }
        public IEnumerable<GameObject> GenerateLoot(LootTable lootTable)
        {
            foreach (var i in lootTable.Generate(Random))
                yield return i;
        }
        #endregion

        public RandomThreaded GetRandom()
        {
            return Random;
        }

        public static void SetWorld(IWorld world)
        {
            if (!UnloadWorld())
            {
                return;
            }
            World = world;
            if (World != null)
            {
                Instance.Log.Write(Color.Lime, "SERVER", "World " + World.GetName() + " loaded");
                var map = World.GetMap(Vector2.Zero);
                if (map is not null)
                {
                }
            }
        }
        private static bool UnloadWorld()
        {
            if (Connections.Count > 0)
            {
                Instance.Log.Write(Color.Red, "SERVER", "Can't unload world while active connections exist");
                return false;
            }
            if (World != null)
            {
                Instance.Log.Write(Color.Lime, "SERVER", "World " + World.GetName() + " unloaded");
            }
            World = null;
            return true;
        }

        ServerCommandParser Parser;
        static public void Command(string command)
        {
            if (Instance.Parser == null)
                Instance.Parser = new ServerCommandParser(Instance);
            Instance.Parser.Command(command);
        }
        [Obsolete]
        public void InventoryOperation(GameObjectSlot sourceSlot, GameObjectSlot targetSlot, int amount)
        {
            var sourceParent = sourceSlot.Parent;
            var destinationParent = targetSlot.Parent;
            if (targetSlot == sourceSlot)
                return;
            if (!targetSlot.Filter(sourceSlot.Object))
                return;

            // TODO: handle case where slots are blockentity's children (they don't have a gameobject parent)
            if (sourceParent != null)
                this.Map.GetChunk(sourceParent.Global).Invalidate();
            if (destinationParent != null)
                this.Map.GetChunk(destinationParent.Global).Invalidate();

            var obj = sourceSlot.Object;
            if (!targetSlot.HasValue) // if target slot empty, set object of target slot without swapping and return
            {
                if (amount < sourceSlot.StackSize) // if the amount moved is smaller than the source amount
                {
                    obj = sourceSlot.Object.Clone();
                    obj.StackSize = amount;
                    sourceSlot.Object.StackSize -= amount;
                    this.InstantiateObject(obj);
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
        
        public void SyncSlots(params TargetArgs[] slots)
        {
            byte[] data = Network.Serialize(w =>
            {
                w.Write(slots.Length);
                foreach (var slot in slots)
                {
                    slot.Write(w);
                    w.Write(slot.Slot.StackSize);
                    if (slot.Slot.StackSize > 0)
                        w.Write(slot.Slot.Object.RefID);
                }
            });
            Instance.Enqueue(PacketType.EntityInventoryChange, data, SendType.OrderedReliable); // WARNING!!! TODO: handle case where each slot is owned by a different entity     
        }
       

        private static void UnmergePackets(PlayerData player, byte[] data)
        {
            using var mem = new MemoryStream(data);
            using var r = new BinaryReader(mem);
            var lastPos = mem.Position;
            while (mem.Position < data.Length)
            {
                var typeID = r.ReadInt32();
                var type = (PacketType)typeID;
                lastPos = mem.Position;

                if (PacketHandlersNewNew.TryGetValue(type, out Action<IObjectProvider, PlayerData, BinaryReader> handlerActionNew))
                    handlerActionNew(Instance, player, r);
                else if (PacketHandlersNewNewNew.TryGetValue(typeID, out var handlerActionNewNew))
                    handlerActionNewNew(Instance, r);
                else
                    GameMode.Current.HandlePacket(Instance, type, r);

                if (mem.Position == lastPos)
                    break;
            }
        }
        static public void StartSaving()
        {
            Instance.IsSaving = true;
            Instance.Enqueue(PacketType.SetSaving, Network.Serialize(w => { w.Write(true); }));
        }
        static public void FinishSaving()
        {
            Instance.IsSaving = false;
            Instance.Enqueue(PacketType.SetSaving, Network.Serialize(w => { w.Write(false); }));
        }
        public PlayerData CurrentPlayer => this.PlayerData;
        PlayerData PlayerData;
        public PlayerData GetPlayer()
        {
            return PlayerData;
        }
        public PlayerData GetPlayer(int id)
        {
            return this.Players.GetPlayer(id);
        }
        public void SetSpeed(int playerID, int speed)
        {
            GetPlayer(playerID).SuggestedSpeed = speed;
            var newspeed = Players.GetLowestSpeed();
            this.Speed = newspeed;
        }

        public void Write(string text)
        {
        }

        void ReceiveAcks(IObjectProvider net, PlayerData player, BinaryReader r)
        {
            var acksCount = r.ReadInt32();
            for (int i = 0; i < acksCount; i++)
            {
                long ackID = r.ReadInt64();
                if (player.WaitingForAck.TryRemove(ackID, out Packet existing))
                {
                    existing.RTT.Stop();
                    player.Connection.RTT = TimeSpan.FromMilliseconds(existing.RTT.ElapsedMilliseconds);
                    player.Ping = TimeSpan.FromMilliseconds(existing.RTT.ElapsedMilliseconds).Milliseconds;
                    if (player.OrderedPackets.Count > 0)
                        if (player.OrderedPackets.Peek().ID == ackID)
                            player.OrderedPackets.Dequeue();
                }
            }
        }

        public void Report(string text)
        {
            this.Write(text);
        }

        public void SyncReport(string text)
        {
            this.Report(text);
            Network.SyncReport(this, text);
        }

        public void WriteToStream(params object[] args)
        {
            this.GetOutgoingStream().Write(args);
        }

        public void Spawn(GameObject obj)
        {
            throw new NotImplementedException();
        }

        public void Spawn(GameObject obj, Vector3 global)
        {
            throw new NotImplementedException();
        }
    }
}