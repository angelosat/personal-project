using System;
//using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.IO.Compression;
using System.Collections.Concurrent;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;
using Start_a_Town_.Net.Packets.Player;
using Start_a_Town_.AI;
using Start_a_Town_.Components.AI;

namespace Start_a_Town_.Net
{
    public partial class Server : IObjectProvider//, IObjectFactory
    {
        class ChunkRequest
        {
            public Chunk Chunk;
            public Action<Chunk> CachedCallback;
            public ChunkRequest(Chunk vector, Action<Chunk> callback)
            {
                this.Chunk = vector;
                this.CachedCallback = callback;
            }
        }
        public double CurrentTick => ServerClock.TotalMilliseconds;
      
        static Server()
        {
            RegisterPacketHandler(PacketType.Acks, Instance.ReceiveAcks);
        }

        private void ReceiveTimestamped(IObjectProvider arg1, BinaryReader arg2)
        {
            throw new NotImplementedException();
        }

        // TODO: figure out why it's glitching out if I set it lower than 10
        public const int ClockIntervalMS = 10;// 10 is working
        static TimeSpan TimeStarted;// = new TimeSpan();
        public TimeSpan Clock { get { return ServerClock; } }
        public static TimeSpan ServerClock;

        internal static void ConnectAsHost()
        {
            Instance.PlayerData = new PlayerData("host test");
            //throw new NotImplementedException();
        }

      

        //static System.Threading.Timer
        //    ServerClockTimer,
        //    SnapshotTimer,
        //    LightTimer,
        //    SaveTimer,
        //    PingTimer;
        static bool IsRunning;

        bool IsSaving;
        //public static UI.ConsoleBoxAsync Console;// = new UI.ConsoleBoxAsync(new Rectangle(0, 0, 800, 600)) { FadeText = false };
        //public UI.ConsoleBoxAsync GetConsole()
        //{
        //    return Console;
        //}
        UI.ConsoleBoxAsync _Console;// = new UI.ConsoleBoxAsync(new Rectangle(0, 0, 800, 600)) { FadeText = false };
        public UI.ConsoleBoxAsync Log
        {
            get
            {
                if (this._Console == null)
                    this._Console = new UI.ConsoleBoxAsync(new Rectangle(0, 0, 800, 600)) { FadeText = false };
                return this._Console;
            }
        }
        static readonly int SaveIntervalMS = 10000; // save changed chunks every 30 seconds /// 10000; // save changed chunks every 10 seconds
        //static int SaveInterval = Engine.TargetFps * 2;
        //static int SaveTimer = SaveInterval;

        //public const int SnapshotIntervalMS = 50;// send 20 snapshots per second to clients
        public const int SnapshotIntervalMS = 10;// send 60 snapshots per second to clients
        public const int LightIntervalMS = 10;// send 60 light updates per second to clients
        //    SnapshotTimer = SnapshotIntervalMS;
        public const int PingInterval = 1000
            ;
        //ConcurrentDictionary<int, GameObject> NetworkObjects;
        readonly Dictionary<int, GameObject> NetworkObjects;

        /// <summary>
        /// Contains objects that have changed since the last world delta state update
        /// </summary>
        //public HashSet<int> ObjectsChangedSinceLastSnapshot = new HashSet<int>();
        public HashSet<GameObject> ObjectsChangedSinceLastSnapshot = new();
        public HashSet<Vector3> LightUpdates = new();


        public LightingEngine LightingEngine;
        //ConcurrentQueue<ChunkRequest> ReadyChunks = new ConcurrentQueue<ChunkRequest>();
        readonly Queue<Chunk> ChunksToActivate = new();

        #region GameEvents
        //ServerEventHandler ServerEventHandlerInstance = new ServerEventHandler();
        
        readonly Dictionary<PacketType, IServerPacketHandler> PacketHandlers = new();
        public void RegisterPacketHandler(PacketType channel, IServerPacketHandler handler)
        {
            this.PacketHandlers.Add(channel, handler);
        }
        readonly static Dictionary<PacketType, Action<IObjectProvider, BinaryReader>> PacketHandlersNew = new();
        static public void RegisterPacketHandler(PacketType channel, Action<IObjectProvider, BinaryReader> handler)
        {
            PacketHandlersNew.Add(channel, handler);
        }
       
        readonly Dictionary<PacketType, Action<Server, Packet>> Packets = new();
        public void RegisterPacket(PacketType type, Action<Server, Packet> handler)
        {
            this.Packets.Add(type, handler);
        }
        readonly static Dictionary<PacketType, Action<IObjectProvider, PlayerData, BinaryReader>> PacketHandlersNewNew = new();
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
            //Instance.ServerEventHandlerInstance.HandleEvent(e);
            GameMode.Current.HandleEvent(Instance, e);
            //GameMode.Sandbox.ServerEventHandler.HandleEvent(Instance, e);

            foreach (var item in Game1.Instance.GameComponents) //GetGameComponents())//.
                item.OnGameEvent(e);

            Instance.Map.OnGameEvent(e);

        }
        static readonly Queue<GameEvent> EventQueue = new();
        public void EventOccured(Components.Message.Types type, params object[] p)
        {
            var e = new GameEvent(this, ServerClock.TotalMilliseconds, type, p);
            OnGameEvent(e);
        }
        public void EventOccured(object id, params object[] p)
        {
            var e = new GameEvent(this, ServerClock.TotalMilliseconds, id, p);
            OnGameEvent(e);
        }
        #endregion
        static public CancellationTokenSource ChunkLoaderToken = new();
        static Server _Instance;
        static public Server Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Server();
                return _Instance;
            }
        }

        public Server()
        {
            //Console = new UI.ConsoleBoxAsync(new Rectangle(0, 0, 800, 600)) { FadeText = false };
            NetworkObjects = new Dictionary<int, GameObject>();

        }
        private void AdvanceClock()
        {
            /// moving this after the sending of the packets because the outgoing stream packets are created in the send methods while the legacy packets have already been created
            //ServerClock = ServerClock.Add(TimeSpan.FromMilliseconds(ClockIntervalMS));


            //delay--;
            //if (delay <= 0)
            //{
            //SendPackets();
            //delay = delayMax;
            //}
            ServerClock = ServerClock.Add(TimeSpan.FromMilliseconds(ClockIntervalMS));


            // TODO: if i don't do this, movement on client in jerky. maybe find a way to do this less times or incorporate it in other packets?
            // attempt to sync time on client when receiving snapshots
            //SyncTime(); // TODO: uncomment
        }

        
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

        /// <summary>
        /// TODO: Make it a field
        /// </summary>
        public IMap Map { get; set; }

        private void SetMap(IMap map)
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
        static public event EventHandler OnPlayerConnect, OnPlayersChanged;

        static readonly ConcurrentDictionary<Vector2, ConcurrentQueue<PlayerData>> ChunkRequests = new();
        static public RandomThreaded Random;

        static public void Stop()
        {
            Server.ChunkLoaderToken.Cancel();
            if (Instance.LightingEngine != null)
                Instance.LightingEngine.Stop();
            if (Listener != null)
            {
                Listener.Close();
            }

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
            //SaveTimer.Change(SaveIntervalMS, SaveIntervalMS);
            //SaveTimer.Change(SaveIntervalMS, SaveIntervalMS);
            //PingTimer.Change(PingInterval, PingInterval);

            Instance.Speed = 0;
            IsRunning = true;
            Instance.OutgoingStream = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamUnreliable = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamReliable = new BinaryWriter(new MemoryStream());

            Connections = new ConcurrentDictionary<EndPoint, UdpConnection>();
            TimeStarted = DateTime.Now.TimeOfDay;
            ServerClock = new TimeSpan();
            //Console = new UI.ConsoleBox(new Rectangle(0, 0, 800, 600)) { FadeText = false };
            Instance.Log.Write("SERVER", "Started");
            if (Listener != null)
                Listener.Close();
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Listener.ReceiveBufferSize = Listener.SendBufferSize = Packet.Size;
            Instance.Players = new PlayerList(Instance);// = new List<IPAddress>();
            var anyIP = new IPEndPoint(IPAddress.Any, Port);
            //Block = new ManualResetEvent(false);
            Listener.Bind(anyIP);
            //SetWorld(World.GetLastWorldName());


            // IGNORE CONNECTIONRESET EXCEPTIONS

            //int SIO_UDP_CONNRESET = -1744830452;
            //Listener.IOControl(
            //    (IOControlCode)SIO_UDP_CONNRESET,
            //    new byte[] { 0, 0, 0, 0 },
            //    null
            //);

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
            //});
            Instance.Log.Write(Color.Yellow, "SERVER", "Listening to port " + Port + "...");
        }
        int _Speed = 0;// 1;
        public int Speed { get { return this._Speed; } set { this._Speed = value; } }
        readonly float MapSaveInterval = Engine.TicksPerSecond * 60f;// 10f;
        float MapSaveTimer = 0;
        readonly float BlockUpdateTimerMax = 1;// Engine.TargetFps / 10f;
        float BlockUpdateTimer = 0;

        public void Update(GameTime gt)
        {

            if (!IsRunning)
                return;
            //using (MemoryStream mem = new MemoryStream())
            //using (Instance.OutgoingStream = new BinaryWriter(mem))
            //{
            HandleIncoming();
            if (GameMode.Current != null)
                //GameMode.Current.PacketHandler.Update(Instance);
                GameMode.Current.Update(Instance);
            //SendPackets();
            //ResetOutgoingStreams();
            if (Instance.Map == null)
                return;
            if (!Instance.IsSaving)
            {
                TickMap();
                UpdateMap();
                //ProcessEvents();
                SendSnapshots(ServerClock);
            }
            //SendSnapshots(ServerClock);

            while (Instance.ChunksToActivate.Count > 0)
            {
                var ch = Instance.ChunksToActivate.Dequeue();
                AddChunk(ch);
            }
            Instance.MapSaveTimer--;
            if (Instance.MapSaveTimer <= 0)
            {
                Instance.MapSaveTimer = Instance.MapSaveInterval;
            }
            /// TODO: maybe reduce frequency of that call?
            UnloadChunks();

            /// THESE MUST BE CALLED WITHIN THE GAMESPEED LOOP
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
            //SendReliable(); //NEW
        }
        private void CreatePacketsFromAllStreams()
        {
            //CreatePacketsFromStreamReliable();
            CreatePacketsFromStreamUnreliable();
            CreatePacketsFromStream();
        }
        private static void ResetOutgoingStreams()
        {
            Instance.OutgoingStream = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamUnreliable = new BinaryWriter(new MemoryStream());
            Instance.OutgoingStreamReliable = new BinaryWriter(new MemoryStream());
        }
        static void PrependTickToStreams()
        {
            Instance.OutgoingStream.Write(Instance.CurrentTick);
            Instance.OutgoingStreamUnreliable.Write(Instance.CurrentTick);
            Instance.OutgoingStreamReliable.Write(Instance.CurrentTick);
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
            var cur = this.CurrentTick;
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
            //this.CopyTimestampedToStream();

            if (auxStream.BaseStream.Position > 0)
            {
                auxStream.BaseStream.Position = 0;
                this.OutgoingStream.Write(Network.Packets.PacketTimestamped);
                //this.OutgoingStream.Write(length);
                auxStream.BaseStream.CopyTo(this.OutgoingStream.BaseStream);
            }
        }

        private void CopyTimestampedToStream()
        {
            var length = this.OutgoingStreamTimestamped.BaseStream.Position;
            this.OutgoingStreamTimestamped.BaseStream.Position = 0;
            this.OutgoingStream.Write(Network.Packets.PacketTimestamped);
            //this.OutgoingStream.Write(this.Map.World.CurrentTick);
            this.OutgoingStream.Write(length);
            this.OutgoingStreamTimestamped.BaseStream.CopyTo(this.OutgoingStream.BaseStream);
        }

        /// <summary>
        /// TODO: maybe reduce frequency of that call?
        /// </summary>
        private static void UnloadChunks()
        {
            if (!Instance.Map.Rules.UnloadChunks)
                return;
            var actives = Instance.Map.GetActiveChunks();
            if (Instance.Map is null)
                return;
            var chunksToUnload = new List<Chunk>();
            foreach (var chunk in actives.Values.ToList())
            {
                bool unload = true;
                foreach (var player in Instance.GetPlayers())
                {
                    if (player.ControllingEntity is null)
                        continue;
                    var playerchunk = player.ControllingEntity.Global.GetChunkCoords();
                    if (Vector2.Distance(chunk.MapCoords, playerchunk) <= Engine.ChunkRadius)
                    {
                        unload = false;
                        break;
                    }
                    //if (Vector2.Distance(chunk.MapCoords, playerchunk) > Engine.ChunkRadius)
                    //    Instance.UnloadChunk(chunk.MapCoords);
                }
                if (unload)
                {
                    //if (chunk.UnloadTimer > 0)
                    //{
                    chunk.UnloadTimer--;
                    if (chunk.UnloadTimer <= 0)
                        chunksToUnload.Add(chunk);
                    //}
                }
            }
            foreach (var ch in chunksToUnload)
                Instance.UnloadChunk(ch.MapCoords);
        }

        //private static void ProcessEvents()
        //{
        //    while (EventQueue.Count > 0)
        //    {
        //        throw new Exception();
        //        //GameEvent e = EventQueue.Dequeue();
        //        //OnGameEvent(e);
        //    }
        //}
        //readonly Random RandomSimple = new System.Random();
        static public int RandomBlockUpdatesCount = 1;//24;
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
            //PacketRandomBlockUpdates.Send(this, tosend); // WHY SEND TWICE? TYPO?
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
        
        void SendReliable()
        {
            //CreatePacketsFromStreamReliable();
            foreach (var player in Players.GetList())
                while (player.OutReliable.Any())
                {
                    throw new NotImplementedException();
                    // I STARTED creating a stream for unordered reliable packets in order to use it for object instantiation
                    // the stream at server side is ready but on client side i need to split ordered from unordered reliable packets
                    // i stopped working on it because i think it's not necessary, i can instantiate objects in the ordered stream i think
                    if (!player.OutReliable.TryDequeue(out Packet packet))
                        return;
                    packet.Player.WaitingForAck[packet.ID] = packet;
                    packet.BeginSendTo(Listener, player.IP);

                }
        }
        void SendUnreliable()
        {
            //CreatePacketsFromStreamUnreliable();
            foreach (var player in Players.GetList())
                while (player.OutUnreliable.Any())
                {
                    if (!player.OutUnreliable.TryDequeue(out Packet p))
                        return;
                    p.BeginSendTo(Listener, player.IP);
                }
        }
        // WORKAROUND FOR CLIENT'S RECEIVE BUFFER OVERFLOW 
        //readonly int delayMax = Engine.TicksPerSecond / 30;
        //int delay = 0;
        const int RTT = 20000;// 5000;
        void SendOrderedReliable()
        {
            //CreatePacketsFromStream();



            foreach (var player in Players.GetList())
                while (player.OutReliable.Any())
                {
                    // WORKAROUND FOR CLIENT'S RECEIVE BUFFER OVERFLOW 
                    //delay--;
                    //if (delay > 0)
                    //    break;
                    //delay = delayMax;
                    //

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
                    //Console.Write(UI.ConsoleMessageTypes.Acks, Color.Orange, "SERVER", "Resending packet " + packet);
                    //("Resending packet " + packet.ToString()).ToConsole();
                    packet.BeginSendTo(Listener, player.IP);
                    //packet.RTT.Restart();
                }
            }
        }
        //void TryResendPacketsAll()
        //{
        //    foreach (var player in Players.GetList())
        //    {
        //        foreach (var packet in player.WaitingForAck.Values.ToList())
        //        {
        //            if (packet.RTT.ElapsedMilliseconds < RTT)
        //                continue;
        //            if (packet.Retries-- <= 0)
        //            {
        //                player.WaitingForAck.TryRemove(packet.ID, out _);
        //                Log.Write(UI.ConsoleMessageTypes.Acks, Color.Orange, "SERVER", "Send retries exceeded maximum for packet " + packet);
        //                Log.Write(Color.Red, "SERVER", player.Name + " timed out");
        //                CloseConnection(player.Connection);
        //                //send player disconnected packet to rest of players
        //                break;
        //            }
        //            //Console.Write(UI.ConsoleMessageTypes.Acks, Color.Orange, "SERVER", "Resending packet " + packet);
        //            //("Resending packet " + packet.ToString()).ToConsole();
        //            packet.BeginSendTo(Listener, player.IP);
        //            //packet.RTT.Restart();
        //        }
        //    }
        //}
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
        private void CreatePacketsFromStreamReliable()
        {
            byte[] data;
            using (var output = new MemoryStream())
            {
                using (var zip = new GZipStream(output, CompressionMode.Compress))
                {
                    OutgoingStreamReliable.BaseStream.Position = 0;
                    OutgoingStreamReliable.BaseStream.CopyTo(zip);
                }
                data = output.ToArray();
            }
            if (data.Length > 0) // send empty packet anyway to substitute pinging for keeping an active connection?
            EnqueueReliable(PacketType.MergedPackets, data);
        }
        public void Forward(Packet p)
        {
            this.Enqueue(p.PacketType, p.Payload);
        }
        internal void EnqueueUnreliable(PacketType packetType, byte[] p)
        {
            this.Enqueue(packetType, p, SendType.Unreliable, true);
        }
        internal void Enqueue(PacketType packetType, byte[] p, bool sync = true)
        {
            this.Enqueue(packetType, p, SendType.OrderedReliable, sync);
        }
        internal void EnqueueReliable(PacketType packetType, byte[] p)
        {
            this.Enqueue(packetType, p, SendType.Reliable, true);
        }
        public void Enqueue(PlayerData player, Packet packet)
        {
            //player.Outgoing.Enqueue(packet);
            //Outgoing.Enqueue(packet);
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
        public void Enqueue(PacketType type, byte[] data, SendType send, Vector3 global)
        {
            this.Enqueue(type, data, send, player => player.IsWithin(global));
        }
        public void Enqueue(PacketType type, byte[] data, SendType send, Vector3 global, bool sync)
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
        public void Enqueue(PacketType type, byte[] data, SendType send, bool sync)
        {
            foreach (var player in Players.GetList())
            {
                var p = Packet.Create(player, type, data, send);
                p.Synced = sync;
                //var t = this.Clock.TotalMilliseconds;
                p.Tick = this.Clock.TotalMilliseconds;
                Enqueue(player, p);
            }
        }
        public void Enqueue(Packet packet, Vector3 global, bool sync)
        {
            foreach (var player in Players.GetList().Where(player => player.IsWithin(global)))
            {
                packet.Tick = this.Clock.TotalMilliseconds;
                Enqueue(player, packet);
            }
        }
        public void Enqueue(PacketType type, byte[] data, SendType send, Func<PlayerData, bool> filter)
        {
            if (data.Length > 60000)
            {
                foreach (var player in Players.GetList().Where(filter))
                {
                    throw new NotImplementedException();
                    //var parts = PacketTransfer.Split(player.PacketSequence, data);
                    //var part1 = parts[0];
                    //var part2 = parts[1];
                    //Enqueue(player, Packet.Create(player.PacketSequence, PacketType.Partial, part1, player, send));
                    //Enqueue(player, Packet.Create(player.PacketSequence, PacketType.Partial, part2, player, send));
                }
                return;
            }
            foreach (var player in Players.GetList().Where(filter))
                Enqueue(player, Packet.Create(player, type, data, send));
        }

        //int OutgoingStreamCount = 0;
        public BinaryWriter OutgoingStream = new(new MemoryStream());
        public BinaryWriter OutgoingStreamUnreliable = new(new MemoryStream());
        public BinaryWriter OutgoingStreamReliable = new(new MemoryStream());
        public BinaryWriter OutgoingStreamTimestamped = new(new MemoryStream());
        public BinaryWriter GetOutgoingStreamTimestamped()
        {
            return this.OutgoingStreamTimestamped;
        }
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
                int bytesReceived = Listener.EndReceiveFrom(ar, ref remoteIP);// state.Socket.EndReceive(ar);

                state.Buffer = new byte[Packet.Size];
                Listener.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ref anyIP, ReceiveMessage, state);

                //   state = Connections.FirstOrDefault(c => IPAddress.Equals(c.IP, remoteIP));
                if (!Connections.TryGetValue(remoteIP, out state))
                //  if (state.IsNull())
                {
                    //if (World.IsNull())
                    //    // TODO: send reason for connection refuse
                    //    throw new Exception("No world loaded on server");
                    //    return;

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

            // set up ping timer for new connection
            //newConnection.Ping = new Timer(a =>
            //{
            //    Enqueue(Packet.Create(PacketID, PacketType.Ping, new byte[0], newPlayer, 5));
            //}, new object(), 0, 1000);
            newConnection.Ping = new System.Diagnostics.Stopwatch();

            return newConnection;
        }

        //static void ReceiveMessage(IAsyncResult ar)
        //{
        //    Socket socket = (Socket)ar.AsyncState;
        //  //  socket.EndReceive(ar);           
        //    Message msg = Message.Create(Buffer);
        //    ("received " + msg.MessageType.ToString() + " (" + socket.EndReceive(ar).ToString() + " bytes)").ToConsole();
        //    msg.Sender = socket;
        //    MessageQueue.Enqueue(msg);
        //    //HandleMessage(msg);

        //    //foreach (var player in Players)
        //    //    player.Socket.BeginSend(Buffer, 0, Buffer.Length, SocketFlags.None, (a) => { "sent".ToConsole(); }, player.Socket);

        //    Buffer = new byte[1024];
        //    socket.BeginReceive(Buffer, 0, Buffer.Length, SocketFlags.None, ReceiveMessage, socket);
        //}

        private static void HandleMessage(Packet msg)
        {
            if (Instance.Packets.TryGetValue(msg.PacketType, out Action<Server, Packet> registeredPacket))
            {
                registeredPacket(Instance, msg);
                return;
            }

            if (Instance.PacketHandlers.TryGetValue(msg.PacketType, out IServerPacketHandler handler))
            {
                handler.HandlePacket(Instance, msg);
                return;
            }
            //Action<IObjectProvider, BinaryReader> handlerAction;
            //if (Instance.PacketHandlersNew.TryGetValue(msg.PacketType, out handlerAction))
            //{
            //    handlerAction(Instance, msg.Payload);
            //    return;
            //}

            if (PacketHandlersNew.TryGetValue(msg.PacketType, out Action<IObjectProvider, BinaryReader> handlerNew))
            {
                Network.Deserialize(msg.Payload, r => handlerNew(Instance, r));
                return;
            }

            switch (msg.PacketType)
            {
                case PacketType.RequestConnection:
                    //PlayerData temp = Network.Deserialize<PlayerData>(msg.Payload, PlayerData.Read);

                    string name = Network.Deserialize<string>(msg.Payload, r =>
                    {
                        return r.ReadString();
                        //Encoding.ASCII.GetString();
                    });

                    Instance.Log.Write(Color.Lime, "SERVER", name + " connected from " + msg.Connection.IP);

                    PlayerData pl = msg.Connection.Player;//  new PlayerData(msg.Sender) { Socket = Listener, Name = name, ID = PlayerID };
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
                    //foreach (var player in from p in Instance.Players.GetList() select p)
                    //    Instance.Enqueue(player, Packet.Create(msg.Player, PacketType.PlayerConnecting, Network.Serialize(msg.Player.Write), SendType.Reliable | SendType.Ordered));

                    // add to players when entering world instead?
                    Instance.Players.Add(pl);
                    //GameMode.Current.HandlePacket(Instance, msg);
                    GameMode.Current.PlayerConnected(Instance, msg.Player);

                    return;

                case PacketType.PlayerDisconnected:
                    CloseConnection(msg.Connection);
                    break;

                case PacketType.PlayerKick:
                    msg.Payload.Deserialize(r =>
                    {
                        var plid = r.ReadInt32();
                        KickPlayer(plid);
                    });
                    break;


                case PacketType.PlayerList:
                    "playerlist".ToConsole();
                    break;

                case PacketType.RequestChunk:
                    Vector2 vec2 = Network.Deserialize<Vector2>(msg.Payload, reader =>
                    {
                        return reader.ReadVector2();
                    });
                    //HandleChunkRequest(msg.Player, vec2);
                    Instance.SendChunks(new List<Vector2>() { vec2 }, msg.Player);
                    return;

                case PacketType.RequestEntity:
                    int entityID = msg.Payload.Deserialize<int>(r => r.ReadInt32());
                    var entity = Instance.GetNetworkObject(entityID);
                    Instance.Enqueue(msg.Player, Packet.Create(msg.Player, PacketType.SyncEntity, Network.Serialize(entity.Write), SendType.OrderedReliable)); //error here when deleting entity
                    //Enqueue(msg.Player, PacketMessage.Create(msg.Player, PacketType.InstantiateObject, Network.Serialize(entity.Write), SendType.OrderedReliable));
                    return;



                case PacketType.PlayerServerCommand:
                    msg.Payload.Deserialize(r =>
                    {
                        //Command(r.ReadASCII());
                        //CommandParser.Execute(Instance, r.ReadASCII());
                        CommandParser.Execute(Instance, msg.Player, r.ReadASCII());

                    });
                    break;

                
                case PacketType.PlayerInventoryOperation:
                    msg.Payload.Deserialize(r =>
                    {
                        var target = TargetArgs.Read(Instance, r);
                        var source = TargetArgs.Read(Instance, r);
                        var destination = TargetArgs.Read(Instance, r);
                        int amount = r.ReadInt32();
                        Instance.InventoryOperation(source.Slot, destination.Slot, amount);
                        byte[] data = Network.Serialize(w =>
                        {
                            w.Write(2);
                            source.Write(w);
                            w.Write(source.Slot.StackSize);
                            if (source.Slot.StackSize > 0)
                                w.Write(source.Slot.Object.RefID);
                            destination.Write(w);
                            w.Write(destination.Slot.StackSize);
                            if (destination.Slot.StackSize > 0)
                                w.Write(destination.Slot.Object.RefID);
                        });
                        Instance.Enqueue(PacketType.EntityInventoryChange, data, SendType.OrderedReliable, msg.Player.ControllingEntity.Global);
                    });

                    return;
                case PacketType.PlayerInventoryOperationNew:
                    msg.Payload.Deserialize(r =>
                    {
                        var source = TargetArgs.Read(Instance, r);
                        var destination = TargetArgs.Read(Instance, r);
                        int amount = r.ReadInt32();
                        Instance.InventoryOperation(source.Slot, destination.Slot, amount);
                        // TODO: check if operation was valid before forwarding to clients
                        //Instance.Enqueue(PacketType.PlayerInventoryOperationNew, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global);
                        //return;

                        // sync the slots across clients
                        Instance.SyncSlots(source, destination);
                        //Instance.SyncSlots(source.Slot, destination.Slot);
                        return;
                    });

                    return;
               
                case PacketType.PlayerSlotRightClick:
                    msg.Payload.Deserialize(r =>
                    {
                        TargetArgs actor = TargetArgs.Read(Instance, r);
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        TargetArgs child = TargetArgs.Read(Instance, r);
                        target.HandleRemoteCall(Instance, new ObjectEventArgs(Components.Message.Types.PlayerSlotRightClick, actor.Object, child.Object));
                        //Instance.PostLocalEvent(target.Object, Components.Message.Types.PlayerSlotRightClick, actor.Object, child.Object);
                    });
                    Instance.Enqueue(PacketType.PlayerSlotRightClick, msg.Payload, SendType.OrderedReliable);
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
                        var type = (Start_a_Town_.Block.Types)r.ReadInt32();
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
                        //var previousBlock = Instance.Map.GetBlock(global);
                        //previousBlock.Remove(Instance.Map, global);
                        Instance.Map.RemoveBlock(global);

                        if (type != Start_a_Town_.Block.Types.Air)
                        {
                            var block = Start_a_Town_.Block.Registry[type];
                            block.Place(Instance.Map, global, data, variation, orientation);
                            //Instance.Map.PlaceBlockNew(global, type, data, variation, orientation);
                        }
                        Instance.Enqueue(PacketType.PlayerSetBlock, msg.Payload, SendType.OrderedReliable, global, true);
                    });
                    return;
                case PacketType.PlayerRemoveBlock:
                    msg.Payload.Deserialize(r =>
                    {
                        Instance.SyncSetBlock(r.ReadVector3(), Start_a_Town_.Block.Types.Air);
                    });
                    return;

                case PacketType.PlayerStartMoving:
                    //msg.Player.Character.GetComponent<ControlComponent>().StartScript(Script.Types.Walk, new ScriptArgs(Instance, msg.Player.Character));
                    throw new Exception();
                    msg.Player.ControllingEntity.GetComponent<MobileComponent>().Start(msg.Player.ControllingEntity);
                    Instance.Enqueue(PacketType.PlayerStartMoving, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    return;

                case PacketType.PlayerStopMoving:
                    //msg.Player.Character.GetComponent<ControlComponent>().FinishScript(Script.Types.Walk);
                    throw new Exception();
                    msg.Player.ControllingEntity.GetComponent<MobileComponent>().Stop(msg.Player.ControllingEntity);
                    Instance.Enqueue(PacketType.PlayerStopMoving, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    return;

                case PacketType.PlayerJump:
                    //msg.Player.Character.GetComponent<ControlComponent>().StartScript(Script.Types.Jumping, new ScriptArgs(Instance, msg.Player.Character));
                    throw new Exception();
                    entity = msg.Player.ControllingEntity;
                    entity.GetComponent<MobileComponent>().Jump(entity);
                    //Instance.Enqueue(PacketType.PlayerJump, msg.Payload, SendType.OrderedReliable, entity.Global, true);
                    return;

                case PacketType.PlayerChangeDirection:
                    throw new Exception();
                    var packet = new PacketPlayerChangeDirection(Instance, msg.Decompressed);//Payload);
                    packet.Entity.Direction = packet.Direction;
                    
                    return;

                case PacketType.PlayerToggleWalk:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        bool toggle = r.ReadBoolean();
                        msg.Player.ControllingEntity.GetComponent<MobileComponent>().ToggleWalk(toggle);
                        Instance.Enqueue(PacketType.PlayerToggleWalk, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;

                case PacketType.PlayerToggleSprint:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        bool toggle = r.ReadBoolean();
                        msg.Player.ControllingEntity.GetComponent<MobileComponent>().ToggleSprint(toggle);
                        Instance.Enqueue(PacketType.PlayerToggleSprint, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;

                case PacketType.PlayerInteract:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        msg.Player.ControllingEntity.GetComponent<WorkComponent>().UseTool(msg.Player.ControllingEntity, target);
                        Instance.Enqueue(PacketType.PlayerInteract, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;

                case PacketType.PlayerUse:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        msg.Player.ControllingEntity.GetComponent<WorkComponent>().Perform(msg.Player.ControllingEntity, target.GetAvailableTasks(Instance).FirstOrDefault(), target);
                        Instance.Enqueue(PacketType.PlayerUse, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;

                case PacketType.PlayerUseHauled:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        var hauled = msg.Player.ControllingEntity.GetComponent<HaulComponent>().GetObject();//.Slot.Object;

                        if (hauled.IsNull())
                            return;
                        msg.Player.ControllingEntity.GetComponent<WorkComponent>().Perform(msg.Player.ControllingEntity, hauled.GetHauledActions(target).FirstOrDefault(), target);
                        Instance.Enqueue(PacketType.PlayerUseHauled, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;

                case PacketType.PlayerDropHauled:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        msg.Player.ControllingEntity.GetComponent<HaulComponent>().Throw(Vector3.Zero, msg.Player.ControllingEntity);
                        Instance.Enqueue(PacketType.PlayerDropHauled, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;

                case PacketType.PlayerDropInventory:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        byte slotid = r.ReadByte();
                        int amount = r.ReadInt32();
                        Instance.PostLocalEvent(msg.Player.ControllingEntity, Message.Types.DropInventoryItem, (int)slotid, amount);
                        Instance.Enqueue(PacketType.PlayerDropInventory, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
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
                        var interaction =
                            PlayerInput.GetDefaultInput(msg.Player.ControllingEntity, target, input);

                        if (interaction == null)
                            return;
                        msg.Player.ControllingEntity.GetComponent<WorkComponent>().Perform(msg.Player.ControllingEntity, interaction, target);
                        if (interaction.Conditions.Evaluate(msg.Player.ControllingEntity, target))
                            Instance.Enqueue(PacketType.PlayerInput, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;


                case PacketType.PlayerPickUp:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        
                        msg.Player.ControllingEntity.GetComponent<WorkComponent>().Perform(msg.Player.ControllingEntity, new InteractionHaul(), target);
                        Instance.Enqueue(PacketType.PlayerPickUp, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;

                case PacketType.PlayerHaul:
                    msg.Payload.Deserialize(r =>
                    {
                        int playerid = r.ReadInt32();
                        var topickup = Instance.GetNetworkObject(r.ReadInt32());
                        var target = new TargetArgs(topickup);
                        var amount = r.ReadInt32();
                        var interaction = new InteractionHaul(amount);
                        msg.Player.ControllingEntity.GetComponent<WorkComponent>().Perform(msg.Player.ControllingEntity, interaction, target);
                        if (interaction.Conditions.Evaluate(msg.Player.ControllingEntity, target))
                            Instance.Enqueue(PacketType.PlayerHaul, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;

                case PacketType.PlayerUnequip:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        PersonalInventoryComponent.Receive(msg.Player.ControllingEntity, target.Slot, false);
                        Instance.Enqueue(PacketType.PlayerUnequip, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;

                
                case PacketType.EntityThrow:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        var dir = r.ReadVector3();
                        var all = r.ReadBoolean();
                        HaulComponent.ThrowHauled(msg.Player.ControllingEntity, dir, all);
                        Instance.Enqueue(PacketType.EntityThrow, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;

                case PacketType.PlayerStartAttack:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        msg.Player.ControllingEntity.GetComponent<AttackComponent>().Start(msg.Player.ControllingEntity);
                        Instance.Enqueue(PacketType.PlayerStartAttack, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;

                case PacketType.PlayerFinishAttack:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        var dir = r.ReadVector3();
                        msg.Player.ControllingEntity.GetComponent<AttackComponent>().Finish(msg.Player.ControllingEntity, dir);
                        Instance.Enqueue(PacketType.PlayerFinishAttack, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;

                case PacketType.PlayerStartBlocking:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        msg.Player.ControllingEntity.GetComponent<Components.BlockingComponent>().Start(msg.Player.ControllingEntity);
                        Instance.Enqueue(PacketType.PlayerStartBlocking, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;

                case PacketType.PlayerFinishBlocking:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        msg.Player.ControllingEntity.GetComponent<Components.BlockingComponent>().Stop(msg.Player.ControllingEntity);
                        Instance.Enqueue(PacketType.PlayerFinishBlocking, msg.Payload, SendType.OrderedReliable, msg.Player.ControllingEntity.Global, true);
                    });
                    return;

                case PacketType.Ack:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        long ackID = r.ReadInt64();
                        if (msg.Player.WaitingForAck.TryRemove(ackID, out Packet existing))
                        {
                            if (existing.PacketType == PacketType.Ping) // i will calculate ping by the acking of the constant stream packets instead of a seperate ping packet
                                msg.Connection.Ping.Stop();
                            existing.RTT.Stop();
                            msg.Connection.RTT = TimeSpan.FromMilliseconds(existing.RTT.ElapsedMilliseconds);
                            msg.Player.Ping = TimeSpan.FromMilliseconds(existing.RTT.ElapsedMilliseconds).Milliseconds;
                            if (msg.Player.OrderedPackets.Count > 0)
                                if (msg.Player.OrderedPackets.Peek().ID == ackID)
                                    msg.Player.OrderedPackets.Dequeue();
                        }
                    });
                    break;

                case PacketType.RequestNewObject:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        GameObject toDrag = GameObject.CreatePrefab(r);//.Instantiate(Instance.Instantiator);
                        Instance.InstantiateObject(toDrag);
                        byte amount = r.ReadByte();
                        foreach (var player in Instance.Players.GetList())
                            Instance.Enqueue(player, Packet.Create(msg.Player, PacketType.RequestNewObject, Network.Serialize(w =>
                            {
                                TargetArgs.Write(w, toDrag);
                                w.Write(amount);
                            }), SendType.Ordered | SendType.Reliable));
                    });
                    break;

                case PacketType.MergedPackets:
                    UnmergePackets(msg.Player, msg.Decompressed);
                    break;

                default:
                    GameMode.Current.HandlePacket(Instance, msg);

                    break;
            }

            // send world state changes periodically (delta states)
            //foreach (var p in Players)
            //    msg.BeginSendTo(p.Socket, p.IP);
        }

        private static void SpawnEntity(Packet msg)
        {
            Network.Deserialize(msg.Payload, r =>
            {
                var entity = GameObject.CreatePrefab(r);
                entity.Initialize();
                var target = TargetArgs.Read(Instance, r);
                target.Map = Instance.Map;
                //Instance.SyncInstantiate(entity);
                switch (target.Type)
                {
                    case TargetType.Slot:
                        //Instance.SyncInstantiate(entity);
                        Instance.Instantiate(entity);
                        target.Slot.Object = entity;
                        Instance.SyncChild(entity, target.Slot.Parent, target.Slot.ID);

                        break;

                    case TargetType.Position:
                        entity.Global = target.Global;
                        Instance.SyncInstantiate(entity);
                        Instance.SyncSpawn(entity);
                        break;

                    default:
                        break;
                }
            });
        }
        public void SpawnRequestFromTemplate(int templateID, TargetArgs target)
        {
            var entity = GameObject.Templates[templateID].Clone();//.CloneTemplate(templateID);// GameObject.Create(entityType);
            //var entity = GameObject.Generate(entityType, Random);

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
                    //entity.Spawn(this);
                    entity.Spawn(this.Map, target.Global);
                    PacketEntityInstantiate.SendFromTemplate(this, templateID, entity);
                    //Instance.SyncInstantiate(entity);
                    //Instance.SyncSpawn(entity);
                    break;

                default:
                    break;
            }
        }
        public void SpawnRequest(int entityType, TargetArgs target)
        {
            var entity = GameObject.Create(entityType);
            //var entity = GameObject.Generate(entityType, Random);

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
                    PacketEntityInstantiate.Send(this, entity);
                    //Instance.SyncInstantiate(entity);
                    //Instance.SyncSpawn(entity);
                    break;

                default:
                    break;
            }
        }
        
        public static Vector3 FindValidSpawnPosition(GameObject obj)
        {
            var xy = Vector3.Zero + (Vector3.UnitX + Vector3.UnitY) * (GameModes.StaticMaps.StaticMap.MapSize.Default.Blocks / 2);
            int z = global::Start_a_Town_.Map.MaxHeight - (int)Math.Ceiling(obj.Physics.Height);
            //while (!(Vector3.UnitZ * z).IsSolid(Instance.Map))
            while (!Instance.Map.IsSolid(xy + Vector3.UnitZ * z))
                z--;

            var zz = Vector3.UnitZ * (z + 1);
            var spawnPosition = xy + zz;
            return spawnPosition;
        }
        public void InstantiateSpawnPlayerEntity(Packet msg, GameObject obj)
        {
            this.InstantiateSpawnPlayerEntity(msg, obj, FindValidSpawnPosition(obj));
        }
        public void InstantiateSpawnPlayerEntity(Packet msg, GameObject obj, Vector3 spawnPosition)
        {
            throw new Exception();
        }

        public void InstantiateAndSpawn(IEnumerable<GameObject> objects)
        {

        }

        public void InstantiateAndSpawn(GameObject obj)
        {
            this.SyncInstantiate(obj);
            this.SyncSpawn(obj);
        }

        /// <summary>
        /// Creates the same item across the network (but doesn't spawn it in the game world)
        /// </summary>
        /// <param name="obj"></param>
        public byte[] SyncInstantiate(GameObject obj)
        {
            obj.SyncInstantiate(this);
            return null;
            //if (obj.NetworkID > 0)
            //    return new byte[] { };

            if (obj.RefID == 0)
                Instantiate(obj);
            
            byte[] newData = Network.Serialize(w =>
            {
                obj.Write(w);

                // pos.Write(w);
            });
           
            Sync(newData);
            return newData;
        }
        void Sync(byte[] data)
        {
            foreach (var player in Players.GetList())
                //Enqueue(player, Packet.Create(player, PacketType.InstantiateObject, data, SendType.Ordered | SendType.Reliable));
                Enqueue(player, Packet.Create(player, PacketType.InstantiateObject, data, SendType.Reliable)); 
            /// WARNING i removed ordered tag because the client might need to have
            /// a new object instantiated and use that object at the exact same tick as it happened server-side
            /// the unordered packet should get processed immediately when it arrives on the client instead of waiting for the server-delay
        }
        
        
        [Obsolete]
        public void SyncSpawn(GameObject obj)
        {
            throw new Exception();
            obj.Spawn(this);
            byte[] newData = Network.Serialize(w =>
            {
                //obj.Write(w);
                w.Write(obj.RefID);
                w.Write(obj.Global);
                w.Write(obj.Velocity);
            });
            foreach (var player in Players.GetList())
                Enqueue(player, Packet.Create(player, PacketType.SpawnObject, newData, SendType.Ordered | SendType.Reliable));
        }
        public void SyncSpawn(GameObject obj, GameObjectSlot slot)
        {
            byte[] newData = Network.Serialize(w =>
            {
                w.Write(obj.RefID);
                TargetArgs.Write(w, slot);
            });
            Enqueue(PacketType.SpawnObjectInSlot, newData, SendType.Ordered | SendType.Reliable, slot.Parent.Global);
            slot.Object = obj;
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
        //private static void SyncTime()
        //{
           
        //    foreach (var player in Instance.Players.GetList())
        //    {
        //        Packet.Create(player, PacketType.SyncTime, Network.Serialize(w => w.Write(ServerClock.TotalMilliseconds))).BeginSendTo(Listener, player.IP);
        //    }
        //}
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

        public GameObject InstantiateAndSync(GameObject obj)
        {
            this.SyncInstantiate(obj);
            return obj;
        }

        public void SyncEvent(GameObject recipient, Components.Message.Types msg, Action<BinaryWriter> writer)
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
            obj.NetNew = this;
            Instance.NetworkObjects.Add(obj.RefID, obj);
        }

        /// <summary>
        /// Instantiates an object on the server and syncs it across the clients
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
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

        public void InstantiateInContainer(GameObject input, Vector3 blockentity, byte slotID)
        {
            
        }
        public void InstantiateInContainer(GameObject obj, GameObject parent, byte containerID, byte slotID, byte amount)
        {
            //instantiate the new object and sync it across clients
            Instance.InstantiateObject(obj);
            PostLocalEvent(parent, ObjectEventArgs.Create(Components.Message.Types.AddItem, new object[] { obj, containerID, slotID, amount }));
            byte[] data = Network.Serialize(w =>
            {
                ObjectEventArgs.Write(w, Components.Message.Types.ContainerOperation, ww =>
                Components.ArrangeInventoryEventArgs.Write(ww, new TargetArgs(parent), TargetArgs.Null, new TargetArgs(obj), containerID, slotID, amount));
            });
            foreach (var player in Players.GetList())
                Enqueue(player, Packet.Create(player, PacketType.ObjectEvent, data, SendType.Ordered | SendType.Reliable));
        }
        public void InstantiateInContainer2(GameObject obj, GameObject container, byte containerID, byte slotID, byte amount)
        {
            obj.Instantiate(Instance.Instantiator);
            PostLocalEvent(container, ObjectEventArgs.Create(Components.Message.Types.AddItem, new object[] { obj, containerID, slotID, amount }));
            byte[] data = Network.Serialize(w =>
            {
                obj.Write(w);
                TargetArgs.Write(w, container);
                w.Write(containerID);
                w.Write(slotID);
                w.Write(amount);
            });
            foreach (var player in Players.GetList())
                Enqueue(player, Packet.Create(player, PacketType.InstantiateInContainer, data, SendType.Ordered | SendType.Reliable));
        }

        /// <summary>
        /// Removes an object from the game world without releasing its NetworkID.
        /// </summary>
        /// <param name="obj"></param>
        public void Despawn(GameObject obj)
        {
            //obj.Despawn(Instance);
            obj.Despawn();
        }
        public void Spawn(GameObject obj)
        {
            //obj.Net = this;
            obj.Map = this.Map;
            this.SpawnObject(obj);
        }
        
        public void Spawn(GameObject obj, Vector3 global)
        {
            obj.Parent = null;
            obj.Map = this.Map;
            obj.Global = global;
            this.SpawnObject(obj);
        }
        public void Spawn(GameObject obj, WorldPosition pos)
        {
            obj.Parent = null;
            obj.Map = this.Map;
            this.SpawnObject(obj);
        }
        public void Spawn(GameObject obj, GameObject parent, int childID)
        {
            if (obj.RefID == 0)
            {
                this.SyncInstantiate(obj);
                SyncChild(obj, parent, childID);
            }
            obj.Parent = parent;
            var slot = parent.GetChildren()[childID];
            slot.Object = obj;
        }
        public void Spawn(GameObject obj, GameObjectSlot slot)
        {
            if (obj.RefID == 0)
            {
                obj.Instantiate(Instantiator);
                SyncInstantiate(obj);
                slot.Object = obj;
                SyncSlots(slot);
                return;
            }
            slot.Object = obj;
        }
        public void SyncSlotInsert(GameObjectSlot slot, GameObject obj)
        {
            if (!slot.Insert(obj.ToSlotLink()))
                return;
            //sync slot
            var parent = slot.Object.Parent;
            var slotid = slot.ID;
            byte[] data = Network.Serialize(w =>
            {
                w.Write(parent.RefID);
                w.Write(slotid);
                slot.Write(w);
            });
            Enqueue(PacketType.SyncSlot, data, SendType.OrderedReliable);
        }

        void SpawnObject(GameObject obj)
        {
            if (obj.RefID == 0)
                SyncInstantiate(obj);
            SyncSpawn(obj);
        }

        public void SyncDespawn(GameObject entity)
        {
            this.Despawn(entity);
            Enqueue(PacketType.DespawnObject, Network.Serialize(w => w.Write(entity.RefID)), SendType.OrderedReliable);
        }

        /// <summary>
        /// Both removes an object form the game world and releases its networkID
        /// </summary>
        /// <param name="objNetID"></param>

        /// <summary>
        /// Releases the object's networkID.
        /// </summary>
        /// <param name="objNetID"></param>
        public bool DisposeObject(GameObject obj)
        {
            //GameObject o;
            //if (NetworkObjects.TryRemove(objNetID, out o)) // WARNING! BLOCK OBJECTS DONT HAVE A NETID
            //  Despawn(o);
            //return NetworkObjects.TryRemove(obj.NetworkID, out o);
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
        /// <summary>
        /// Releases the object's networkID, and syncs the disposal among clients.
        /// NOTICE: if both server and client are executing the same code, then both should individually call DisposeObject instead of calling this method on the server
        /// </summary>
        /// <param name="obj"></param>
        public void SyncDisposeObject(GameObject obj)
        {
            this.DisposeObject(obj);
            byte[] data = Network.Serialize(w => TargetArgs.Write(w, obj));
            foreach (var player in Players.GetList())
                Enqueue(player, Packet.Create(player, PacketType.DisposeObject, data, SendType.OrderedReliable));
        }
        
        public void SyncDisposeObject(GameObjectSlot slot)
        {
            // if the object is in a temp slot, call syncdisposeobject(gameobject) instead
            if (slot.Object.Parent == null)
            {
                this.SyncDisposeObject(slot.Object);
                return;
            }
            NetworkObjects.Remove(slot.Object.RefID);

            byte[] data = Network.Serialize(w => TargetArgs.Write(w, slot));
            foreach (var player in Players.GetList())
                Enqueue(player, Packet.Create(player, PacketType.DisposeObject, data));

            slot.Clear();
        }

        static public void InstantiateMap(IMap map)
        {
            if (map == null)
                return;
            World = map.World;
            Instance.Map = map;
            Instance.Map.Net = Instance;

            foreach (var ch in map.GetActiveChunks().Values)
                InstantiateChunk(ch);
            foreach (var obj in Instance.NetworkObjects)
                obj.Value.MapLoaded(Instance.Map);
            Instance.SetMap(map);

            Instance.LightingEngine = map.LightingEngine;// LightingEngine.StartNew(map, Instance.SyncLight, Instance.SyncInvalidateCells);// new LightingEngine(this);

            Random = new RandomThreaded(Instance.Map.Random);
        }

        static readonly BlockingCollection<Vector2> ChunksToLoad = new(new ConcurrentQueue<Vector2>());
       
        public void UnloadChunk(Vector2 chunkPos)
        {
            var activeChunks = Instance.Map.GetActiveChunks();

            if (!activeChunks.TryGetValue(chunkPos, out Chunk chunk))
                return;

            // check distance of chunk from every player
            //if (Players.GetList().Any(pl => Vector2.Distance(chunkPos, pl.Character.Global.GetChunk(this.Map).MapCoords) <= Engine.ChunkRadius))
            //    return;

            foreach (var obj in chunk.GetObjects())
                //Sync // don't sync dispose cause client will dispose object when it receives the unload chunk packet
                this.DisposeObject(obj);

            chunk = activeChunks[chunkPos];
            activeChunks.Remove(chunkPos);

            if (!chunk.Saved)
                this.ChunkLoader.ScheduleForSaving(chunk);
            chunk.SaveThumbnails();
        }

        readonly ChunkLoader ChunkLoader;// = new ChunkLoader(this.Map);

        private static void LoadChunk(Vector2 chunkPos, Action<Chunk> callback)
        {
            if (Instance.Map.GetActiveChunks().TryGetValue(chunkPos, out Chunk existing))
            {
                callback(existing);
                return;
            }
            Instance.ChunkLoader.TryEnqueue(chunkPos,
                (c) =>
                {
                    HandleLoadedChunk(c);
                    callback(c);
                });
        }

        private static void HandleLoadedChunk(Chunk ch)
        {
            var actives = Instance.Map.GetActiveChunks();
            // TODO: did we load an existing chunk by error?
            if (actives.ContainsKey(ch.MapCoords))
                return;
            InstantiateChunk(ch);
            Instance.ChunksToActivate.Enqueue(ch);
        }
        
        public void SendChunks(List<Vector2> chunksToLoad, PlayerData player)
        {
            var actives = Instance.Map.GetActiveChunks();
            Task.Factory.StartNew(() =>
            {
                var existingPositions = from pos in chunksToLoad where actives.ContainsKey(pos) select pos;
                var existing = from pos in chunksToLoad where actives.ContainsKey(pos) select actives[pos];
                foreach (var item in existing)
                    SendChunk(player, item);
                var toLoad = chunksToLoad.Except(existingPositions);
                foreach (var item in toLoad)
                    LoadChunk(item, c => SendChunk(player, c));
            });
        }

        private static void UpdateChunkNeighborsLight(Chunk chunk)
        {
            var actives = Instance.Map.GetActiveChunks();
            if (actives.TryGetValue(chunk.MapCoords + new Vector2(1, 0), out Chunk neighbor))
                Instance.LightingEngine.Enqueue(neighbor.GetEdges(Edges.West));
            if (actives.TryGetValue(chunk.MapCoords + new Vector2(-1, 0), out neighbor))
                Instance.LightingEngine.Enqueue(neighbor.GetEdges(Edges.East));
            if (actives.TryGetValue(chunk.MapCoords + new Vector2(0, 1), out neighbor))
                Instance.LightingEngine.Enqueue(neighbor.GetEdges(Edges.North));
            if (actives.TryGetValue(chunk.MapCoords + new Vector2(0, -1), out neighbor))
                Instance.LightingEngine.Enqueue(neighbor.GetEdges(Edges.South));
        }
        public void SendChunk(PlayerData player, Chunk chunk)
        {
            chunk.UnloadTimer = Chunk.UnloadTimerMax;
            byte[] data = Network.Serialize(chunk.Write);
            Enqueue(player, Packet.Create(player, PacketType.Chunk, data, SendType.OrderedReliable)); // but send them ordered as well as a workaround for sending so fast that the client input buffer gets clogged and packets are missed
            // we need to send each chunk after the client has received the previous one, so as to avoid clogging the client receive buffer
            //data.Length.ToConsole();
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
                //obj.Net = Instance;
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
            if (Instance.Map == null)//.IsNull())
                return;
            if (!Instance.Map.Rules.SaveChunks)
            {
                Server.Instance.Log.Write("SERVER", "Saving is disabled for this map");
                return;
            }
            //    Instance.Map.SaveServer();
            foreach (var chunk in Instance.Map.GetActiveChunks().Values)
            {
                if (chunk.Saved)
                    continue;

                Instance.ChunkLoader.ScheduleForSaving(chunk);
            }
        }

        ///// <summary>
        ///// When this is called, any object state changes occured, are written 
        ///// </summary>
        //void BuildSnapshot(TimeSpan gt, BinaryWriter writer)
        //{
        //    writer.Write(gt.TotalMilliseconds);
        //    writer.Write(this.ObjectsChangedSinceLastSnapshot.Count);
        //    var objectsChanged = this.ObjectsChangedSinceLastSnapshot;

        //    this.ObjectsChangedSinceLastSnapshot = new HashSet<GameObject>();
        //    foreach (var obj in objectsChanged)
        //    {
        //        writer.Write(obj.RefID);
        //        ObjectSnapshot.Write(obj, writer);
        //    }
        //}

        /// <summary>
        /// Only sends updates for entities within a player's loaded chunk range
        /// TODO: store object instances instead of fetching them by id each time
        /// </summary>
        /// <param name="gt"></param>
        /// <param name="player"></param>
        /// <param name="writer"></param>
        void BuildSnapshot(TimeSpan gt, PlayerData player, IEnumerable<GameObject> objects, BinaryWriter writer)
        {
            writer.Write(gt.TotalMilliseconds);
            var inRange = (from obj in objects
                           where player.IsWithin(obj.Global)
                           select obj)
                           .ToList();
            writer.Write(inRange.Count);
            foreach (var obj in inRange)
            {
                writer.Write(obj.RefID);
                ObjectSnapshot.Write(obj, writer);
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
        //private static void SendSnapshotsOld(TimeSpan gt)
        //{
        //    if (Instance.ObjectsChangedSinceLastSnapshot.Count > 0)// || Instance.EventsSinceLastSnapshot.Count > 0)
        //    {
        //        foreach (var player in Instance.GetPlayers())
        //        {
        //            if (!player.IsActive)
        //                continue;
        //            byte[] data = Network.Serialize(w => Instance.BuildSnapshot(gt, player, Instance.ObjectsChangedSinceLastSnapshot, w));

        //            // send snapshots immediately or enqueue them?
        //            Packet.Create(player, PacketType.Snapshot, data).BeginSendTo(Listener, player.IP);
        //        }
        //        Instance.ObjectsChangedSinceLastSnapshot.Clear();// = new HashSet<int>();
        //    }
        //}

        // TODO make the values a list in case i want to sync more components instead of position
        //public Dictionary<int, Action<BinaryWriter>> DeltaStates = new Dictionary<int, Action<BinaryWriter>>();

        public bool LogStateChange(int netID)
        {
            return this.ObjectsChangedSinceLastSnapshot.Add(NetworkObjects[netID]);
        }

        #region Loot
        public void PopLoot(GameObject obj)
        {
            double angle = Random.NextDouble() * (Math.PI + Math.PI);
            double w = Math.PI / 4f;// Random.NextDouble() * Math.PI / 2f;

            float force = 0.5f;

            float x = 0.1f * (float)(Math.Sin(w) * Math.Cos(angle));
            float y = 0.1f * (float)(Math.Sin(w) * Math.Sin(angle));
            float z = (float)Math.Cos(w);

            var direction = new Vector3(x, y, z);
            obj.Velocity += force * direction;

            byte[] newData = Network.Serialize(ww =>
            {
                obj.Write(ww);
                ww.Write(obj.Global);
                ww.Write(obj.Velocity);
            });
            foreach (var player in Players.GetList())
                //player.Outgoing.
                Enqueue(player, Packet.Create(player, PacketType.SpawnObject, newData, SendType.Ordered | SendType.Reliable));
        }
        public void PopLoot(GameObject obj, GameObject parent)
        {
            this.PopLoot(obj, parent.Global, parent.Velocity);
        }
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
            double w = Math.PI / 4f;// Random.NextDouble() * Math.PI / 2f;

            //float force = 0.5f;
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
                //this.SyncInstantiate(obj);
            //this.SyncSpawn(obj, this.Map, startPosition, final);
            this.Map.SyncSpawn(obj, startPosition, final);

        }
        public IEnumerable<GameObject> GenerateLoot(LootTable lootTable)
        {
            foreach (var i in lootTable.Generate(Random))
                yield return i;
            //List<GameObject> loot = new List<GameObject>();
            //foreach (var l in lootTable)
            //    for (int i = 0; i < l.Generate(Random); i++)
            //    {
            //        var obj = l.Factory();
            //        var stacksize = Random.Next(l.StackMin, l.StackMax);
            //        obj.StackSize = stacksize;
            //        loot.Add(obj);
            //        //loot.Add(GameObject.Create(l.ObjID));
            //    }
            //return loot;
        }
        public List<GameObject> GenerateLootOld(LootTable lootTable)
        {
            var loot = new List<GameObject>();
            foreach (var l in lootTable)
                for (int i = 0; i < l.Generate(Random); i++)
                {
                    loot.Add(l.Factory());
                    //loot.Add(GameObject.Create(l.ObjID));
                }
            return loot;
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
                if (map != null)
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

        ServerCommandParser Parser;// = new ServerCommandParser(this);
        static public void Command(string command)
        {
            if (Instance.Parser == null)
                Instance.Parser = new ServerCommandParser(Instance);
            Instance.Parser.Command(command);
        }

        public void EventOccured(GameObject sender, ObjectEventArgs args) { }

        /// <summary>
        /// Syncs light at specified global positions across all clients.
        /// </summary>
        /// <param name="globals"></param>
        void SyncLight(IEnumerable<Vector3> globals)
        {
            byte[] data = Network.Serialize(a =>
            {
                a.Write(globals.Count()); //number of entries
                foreach (var global in globals.ToList())
                {
                    a.Write(global);
                    this.Map.GetLight(global, out byte sky, out byte block);
                    a.Write(sky);
                    a.Write(block);
                }
            });
            foreach (var pl in Players.GetList())
                Enqueue(pl, Packet.Create(pl, PacketType.SyncLight, data, SendType.Reliable | SendType.Ordered));
        }
       

        public bool LogLightChange(Vector3 global)
        {
            return this.LightUpdates.Add(global);
        }
        public void LogLightChange(IEnumerable<Vector3> globals)
        {
            foreach (var item in globals)
                this.LightUpdates.Add(item);
        }


        public void InventoryOperation(GameObject parent, Components.ArrangeChildrenArgs args)
        {
            GameObject sourceObj = args.Object.Object;
            if (!parent.TryGetChild(args.TargetSlotID, out GameObjectSlot targetSlot) ||
                !args.SourceEntity.Object.TryGetChild(args.SourceSlotID, out GameObjectSlot sourceSlot))
                return;
            if (targetSlot == sourceSlot)
                return;
            int amount = args.Amount;

            if (sourceObj.IsNull())
            {
                // object originating from a splitstack operation, instantiate it on network and resend message to parent
                GameObject newObj = sourceSlot.Object.Clone();
                InstantiateObject(newObj);
                args.Object = new TargetArgs(newObj);
            }
            this.Map.GetChunk(parent.Global).Invalidate();//.Saved = true;
            this.Map.GetChunk(args.SourceEntity.Object.Global).Invalidate();//.Saved = true;
            Network.InventoryOperation(this, sourceObj, targetSlot, sourceSlot, amount);
        }
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
                    obj.GetInfo().StackSize = amount;
                    sourceSlot.Object.GetInfo().StackSize -= amount;
                    this.InstantiateObject(obj);
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
        public void InventoryOperationOld(GameObjectSlot source, GameObjectSlot destination, int amount)
        {
            var sourceParent = source.Parent;
            var destinationParent = destination.Parent;
            if (destination == source)
                return;
            if (!destination.Filter(source.Object))
                return;
            //sourceParent.Global.GetChunk(this.Map).Invalidate();
            //destinationParent.Global.GetChunk(this.Map).Invalidate();
            this.Map.GetChunk(sourceParent.Global).Invalidate();
            this.Map.GetChunk(destinationParent.Global).Invalidate();
            Network.InventoryOperation(this, source, destination, amount);
        }
        public void TryGetRandomValue(Action<double> action)
        {
            action(this.GetRandom().NextDouble());
        }
        public void TryGetRandomValue(int min, int max, Action<int> action)
        {
            action(this.GetRandom().Next(min, max));
        }
        public bool TryGetRandomValue(int min, int max, out int rand)
        {
            rand = this.GetRandom().Next(min, max);
            return true;
        }
        public void RandomEvent(GameObject target, ObjectEventArgs a, Action<double> rnEvent)
        {
            double ran = this.GetRandom().NextDouble();
            rnEvent(ran);
            byte[] data = Network.Serialize(w =>
            {
                w.Write(this.Clock.TotalMilliseconds);
                TargetArgs.Write(w, target);
                w.Write((int)a.Type);
                w.Write(a.Data.Length);
                w.Write(a.Data);
                w.Write(ran);
            });
            foreach (var pl in Players.GetList())
                //pl.Outgoing.
                Enqueue(pl, Packet.Create(pl, PacketType.RandomEvent, data, SendType.Reliable | SendType.Ordered));
        }
        public void RandomEvent(TargetArgs target, ObjectEventArgs a, Action<double> rnEvent)
        {
            double ran = this.GetRandom().NextDouble();
            rnEvent(ran);
            byte[] data = Network.Serialize(w =>
            {
                w.Write(this.Clock.TotalMilliseconds);
                target.Write(w);
                w.Write((int)a.Type);
                w.Write(a.Data.Length);
                w.Write(a.Data);
                w.Write(ran);
            });
            foreach (var pl in Players.GetList())
                //pl.Outgoing.
                Enqueue(pl, Packet.Create(pl, PacketType.RandomEvent, data, SendType.Reliable | SendType.Ordered));
        }
        internal void RemoteProcedureCall(byte[] data)
        {
            Enqueue(PacketType.RemoteProcedureCall, data, SendType.OrderedReliable);
        }
        internal void RemoteProcedureCall(byte[] data, Vector3 global)
        {
            //Enqueue(PacketType.RemoteProcedureCall, data, SendType.OrderedReliable, (pl) => GameMode.Current.IsPlayerWithinRangeForPacket(pl.Character, global));
            Enqueue(PacketType.RemoteProcedureCall, data, SendType.OrderedReliable, global, true);
        }
        internal void RemoteProcedureCall(TargetArgs target, Message.Types type, Vector3 global)
        {
            this.RemoteProcedureCall(target, type, new byte[] { }, global);
        }
        internal void RemoteProcedureCall(TargetArgs target, Message.Types type, byte[] uncompressedData, Vector3 global)
        {
            var data = Network.Serialize(w =>
                {
                    target.Write(w);
                    w.Write((int)type);
                    w.Write(uncompressedData);
                });

            //Enqueue(PacketType.RemoteProcedureCall, data, SendType.OrderedReliable, (pl) => GameMode.Current.IsPlayerWithinRangeForPacket(pl.Character, global));
            Enqueue(PacketType.RemoteProcedureCall, data, SendType.OrderedReliable, global, true);
        }
        internal void RemoteProcedureCall(TargetArgs target, Message.Types type, Action<BinaryWriter> writer)
        {
            this.OutgoingStream.Write((int)PacketType.RemoteProcedureCall);
            target.Write(this.OutgoingStream);
            this.OutgoingStream.Write((int)type);
            writer(this.OutgoingStream);
            return;

        }

        static readonly ConcurrentDictionary<Vector3, Cell> BlocksToSync = new();
        public void UpdateBlock(Vector3 global, Action<Cell> updater)
        {
        
        }
        public void SyncSetBlock(Vector3 global, Block.Types type)
        {
            this.Map.SetBlock(global, type);
            SyncCell(global, Instance.Map.GetCell(global));
        }
        public void SyncSetBlock(Vector3 global, Block.Types type, byte data, int orientation)
        {
            this.SyncSetBlock(global, type, data, 0, orientation);
        }

        public void SyncSetBlock(Vector3 global, Block.Types type, byte data, int variation, int orientation)
        {
            // TODO: do all the below in the map class??
            //this.SetBlock(global, type, data, variation);
            //this.Map.SetBlock(global, type, data, variation);
            if (!this.Map.IsInBounds(global))
                return;
            var previousBlock = this.Map.GetBlock(global);
            previousBlock.Remove(this.Map, global);
            var block = Start_a_Town_.Block.Registry[type];
            block.Place(this.Map, global, data, variation, orientation);
            //this.SyncCell(global, global.GetCell(this.Map));
            this.SyncCell(global, this.Map.GetCell(global));
           
        }
        

        /// <summary>
        /// Syncs cell across the network
        /// </summary>
        /// <param name="global"></param>
        void SyncCell(Vector3 global)
        {
            //this.SyncCell(global, global.GetCell(this.Map));
            this.SyncCell(global, this.Map.GetCell(global));
        }
        /// <summary>
        /// Syncs cell across the network
        /// </summary>
        /// <param name="global"></param>
        void SyncCell(Vector3 global, Cell cell)
        {
            if (cell.IsNull())
                return;// throw new Exception();
            BlocksToSync.TryAdd(global, cell);
        }
        //void SyncInvalidateCell(Vector3 global)
        //{
        //    //Cell cell = global.GetCell(this.Map);
        //    Cell cell = this.Map.GetCell(global);

        //    this.InvalidateCell(global, cell);
        //    SyncCell(global, cell);
        //}
        //void InvalidateCell(Vector3 global)
        //{
        //    //this.InvalidateCell(global, global.GetCell(this.Map));
        //    this.InvalidateCell(global, this.Map.GetCell(global));

        //}
        void InvalidateCell(Vector3 global, Cell cell)
        {
            Instance.Map.InvalidateCell(global, cell);
           
        }
        public void UpdateLight(Vector3 global)
        {
            Chunk chunk = this.Map.GetChunk(global);
            var q = chunk.ResetHeightMapColumn(global.ToLocal());
            this.SyncLight(global.GetColumn());
            this.LightingEngine.Enqueue(new List<Vector3>() { global }.Concat(global.GetNeighbors()));
        }
        public void SpreadBlockLight(Vector3 global)
        {
            this.LightingEngine.EnqueueBlock(global);
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
        void SyncSlots(params GameObjectSlot[] slots)
        {
            byte[] data = Network.Serialize(w =>
            {
                w.Write(slots.Length);
                foreach (var slot in slots)
                {
                    TargetArgs.Write(w, slot);
                    w.Write(slot.StackSize);
                    if (slot.StackSize > 0)
                        w.Write(slot.Object.RefID);
                }
            });
            Instance.Enqueue(PacketType.EntityInventoryChange, data, SendType.OrderedReliable); // WARNING!!! TODO: handle case where each slot is owned by a different entity     
        }

        public void SendBlockMessage(Vector3 global, Components.Message.Types msg, params object[] parameters)
        {
            Start_a_Town_.Block.HandleMessage(this, global, ObjectEventArgs.Create(this, msg, parameters));
        }

        public AI AIHandler = new();

        int AISeed;
        Random AIRandom = new();//.Next(int.MinValue, int.MaxValue);
        public void SyncAI()
        {
            var rand = new Random();
            this.AISeed = rand.Next(int.MinValue, int.MaxValue);
            this.AIRandom = new Random(this.AISeed);
            foreach (var obj in this.NetworkObjects)
                PostLocalEvent(obj.Value, Message.Types.SyncAI, this.AISeed + obj.Key.GetHashCode());
            byte[] data = Network.Serialize(w => w.Write(this.AISeed));
            foreach (var pl in this.GetPlayers())
                Enqueue(pl, Packet.Create(pl, PacketType.SyncAI, data, SendType.OrderedReliable));
        }
        public void SyncAI(GameObject obj)
        {
            var rand = new Random();
            var newSeed = this.AIRandom.Next(int.MinValue, int.MaxValue);
            obj.GetComponent<AIComponent>().Sync(newSeed);
            //PostLocalEvent(obj, Message.Types.SyncAI, newSeed + obj.InstanceID.GetHashCode());
            byte[] data = Network.Serialize(w =>
            {
                w.Write(obj.RefID);
                w.Write(newSeed);
            });
            foreach (var pl in this.GetPlayers())
                Enqueue(pl, Packet.Create(pl, PacketType.SyncAI, data, SendType.OrderedReliable));
        }

        public void SyncBlockVariation(IMap map, Vector3 global, byte variation)
        {
            map.GetCell(global).Variation = variation;
            byte[] data = Network.Serialize(w =>
            {
                new TargetArgs(global).Write(w);
                w.Write(variation);
            });
            Enqueue(PacketType.SetBlockVariation, data, SendType.OrderedReliable, global);
        }

        public void SyncPlaceBlock(IMap map, Vector3 global, Block block, byte data, int variation, int orientation)
        {
            block.Place(map, global, data, variation, orientation);
            byte[] payload = Network.Serialize(w =>
            {
                new TargetArgs(global).Write(w);
                w.Write((int)block.Type);
                w.Write(data);
                w.Write(variation);
                w.Write(orientation);
            });
            Enqueue(PacketType.SyncPlaceBlock, payload, SendType.OrderedReliable, global);
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

                if (PacketHandlersNew.TryGetValue(type, out Action<IObjectProvider, BinaryReader> handlerAction))
                    handlerAction(Instance, r);
                else if (PacketHandlersNewNew.TryGetValue(type, out Action<IObjectProvider, PlayerData, BinaryReader> handlerActionNew))
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
            //Instance.OutgoingStream.Write((int)PacketType.SetSaving);
            //Instance.OutgoingStream.Write(true);
            Instance.IsSaving = true;
            Instance.Enqueue(PacketType.SetSaving, Network.Serialize(w => { w.Write(true); }));
        }
        static public void FinishSaving()
        {
            //Instance.OutgoingStream.Write((int)PacketType.SetSaving);
            //Instance.OutgoingStream.Write(false);
            Instance.IsSaving = false;
            Instance.Enqueue(PacketType.SetSaving, Network.Serialize(w => { w.Write(false); }));

        }
        public PlayerData CurrentPlayer => this.PlayerData;
        PlayerData PlayerData;
        public PlayerData GetPlayer()
        {
            return PlayerData;
            //throw new NotImplementedException();
            //return this.GetPlayer(PlayerData.ID);
        }
        public PlayerData GetPlayer(int id)
        {
            return this.Players.GetPlayer(id);
            //return this.Players.GetList().First(p => p.ID == id);
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


        public void Write(Log.EntryTypes type, string text)
        {
        }

        void ReceiveAcks(IObjectProvider net, PlayerData player, BinaryReader r)
        {
            //var plid = r.ReadInt32();
            var acksCount = r.ReadInt32();
            //var player = this.GetPlayer(plid);
            //if (player == null)
            //    return;
            for (int i = 0; i < acksCount; i++)
            {
                long ackID = r.ReadInt64();
                if (player.WaitingForAck.TryRemove(ackID, out Packet existing))
                {
                    if (existing.PacketType == PacketType.Ping) // i will calculate ping by the acking of the constant stream packets instead of a seperate ping packet
                        player.Connection.Ping.Stop();
                    //msg.Connection.Ping.Restart();
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
            //this.Log.Write("SERVER", text);
            this.Write(text);
        }

        public void SyncReport(string text)
        {
            this.Report(text);
            Network.SyncReport(this, text);
        }

        List<GameObject> IObjectProvider.GenerateLoot(LootTable loot)
        {
            throw new NotImplementedException();
        }
        public void WriteToStream(params object[] args)
        {
            this.GetOutgoingStream().Write(args);
        }

        //public void HandleTimestamped(BinaryReader r)
        //{
        //    throw new NotImplementedException();
        //}
    }
}