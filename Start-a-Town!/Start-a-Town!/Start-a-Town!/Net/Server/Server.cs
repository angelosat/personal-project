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
using System.Collections.Concurrent;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;
using Start_a_Town_.Net.Packets.Player;

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

        // TODO: figure out why it's glitching out if I set it lower than 10
        public const int ClockIntervalMS = 10;// 10 is working
        static TimeSpan TimeStarted;// = new TimeSpan();
        public TimeSpan Clock { get { return ServerClock; } }
        public static TimeSpan ServerClock;
        static System.Threading.Timer
            ServerClockTimer,
            SnapshotTimer,
            LightTimer,
            SaveTimer,
            PingTimer;

        public static UI.ConsoleBoxAsync Console = new UI.ConsoleBoxAsync(new Rectangle(0, 0, 800, 600)) { FadeText = false };
        public UI.ConsoleBoxAsync GetConsole()
        {
            return Console;
        }

        static int SaveIntervalMS = 10000; // save changed chunks every 30 seconds /// 10000; // save changed chunks every 10 seconds
        //static int SaveInterval = Engine.TargetFps * 2;
        //static int SaveTimer = SaveInterval;

        //public const int SnapshotIntervalMS = 50;// send 20 snapshots per second to clients
        public const int SnapshotIntervalMS = 10;// send 60 snapshots per second to clients
        public const int LightIntervalMS = 10;// send 60 light updates per second to clients
        //    SnapshotTimer = SnapshotIntervalMS;
        public const int PingInterval = 1000
            ;
        ConcurrentDictionary<int, GameObject> NetworkObjects;

        /// <summary>
        /// Contains objects that have changed since the last world delta state update
        /// </summary>
        //public HashSet<int> ObjectsChangedSinceLastSnapshot = new HashSet<int>();
        public HashSet<GameObject> ObjectsChangedSinceLastSnapshot = new HashSet<GameObject>();
        //public ConcurrentDictionary<int, GameObject> ObjectsChangedSinceLastSnapshot = new ConcurrentDictionary<int, GameObject>();
        public HashSet<Vector3> LightUpdates = new HashSet<Vector3>();


        public LightingEngine LightingEngine;
        //public ConcurrentQueue<EventSnapshot> EventsSinceLastSnapshot = new ConcurrentQueue<EventSnapshot>();
        ConcurrentQueue<ChunkRequest> ReadyChunks = new ConcurrentQueue<ChunkRequest>();
        Queue<Chunk> ChunksToActivate = new Queue<Chunk>();

        #region GameEvents
        //ServerEventHandler ServerEventHandlerInstance = new ServerEventHandler();

        Dictionary<PacketType, IServerPacketHandler> PacketHandlers = new Dictionary<PacketType, IServerPacketHandler>();
        public void RegisterPacketHandler(PacketType channel, IServerPacketHandler handler)
        {
            this.PacketHandlers.Add(channel, handler);
        }
        Dictionary<PacketType, Action<Server, Packet>> Packets = new Dictionary<PacketType, Action<Server, Packet>>();
        public void RegisterPacket(PacketType type, Action<Server, Packet> handler)
        {
            this.Packets.Add(type, handler);
        }

        public event EventHandler<GameEvent> GameEvent;
        static void OnGameEvent(GameEvent e)
        {
            //Instance.ServerEventHandlerInstance.HandleEvent(e);
            GameMode.Current.ServerEventHandler.HandleEvent(Instance, e);
            //GameMode.Sandbox.ServerEventHandler.HandleEvent(Instance, e);

            foreach (var item in Game1.Instance.GameComponents) //GetGameComponents())//.
                item.OnGameEvent(e);

            Instance.Map.OnGameEvent(e);  // WARNING!!! may cause problems as i was calling this only on client prior
            
            if (!Instance.GameEvent.IsNull())
                Instance.GameEvent(null, e);
        }
        static Queue<GameEvent> EventQueue = new Queue<GameEvent>();
        public void EventOccured(Components.Message.Types type, params object[] p)
        {
            GameEvent e = new GameEvent(this, ServerClock.TotalMilliseconds, type, p);
            EventQueue.Enqueue(e);
        }
        #endregion
        static public CancellationTokenSource ChunkLoaderToken = new CancellationTokenSource();
        static Server _Instance;
        static public Server Instance
        {
            get
            {
                if (_Instance.IsNull())
                    _Instance = new Server();
                return _Instance;
            }
        }

        object SnapshotLock = new object();
        Server()
        {
            //ServerClockTimer = new System.Threading.Timer((a) =>
            //{
            //    //Thread.CurrentThread.Name = "ServerClock";
            //    AdvanceClock();
            //    //SendInvalidatedCells();
            //}, new object(), 0, ClockIntervalMS);

            //SnapshotTimer = new System.Threading.Timer((a) =>
            //{
            //    SnapshotTimer.Change(Timeout.Infinite, Timeout.Infinite);
            //    if (!this.Map.IsNull())
            //        SendSnapshots(ServerClock);
            //    SnapshotTimer.Change(0, SnapshotIntervalMS);
            //}, new object(), Timeout.Infinite, Timeout.Infinite);
            //SnapshotTimer.Change(0, SnapshotIntervalMS);

            //SnapshotTimer = new System.Threading.Timer((a) =>
            //{
            //    if (Monitor.TryEnter(this.SnapshotLock))
            //    {
            //        try
            //        {
            //            if (!this.Map.IsNull())
            //                SendSnapshots(ServerClock);
            //        }
            //        finally
            //        {
            //            Monitor.Exit(this.SnapshotLock);
            //        }
            //    }
            //}, new object(), Timeout.Infinite, Timeout.Infinite);
            //SnapshotTimer.Change(SnapshotIntervalMS, SnapshotIntervalMS);


            SaveTimer = new System.Threading.Timer((a) =>
            {
                //SaveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                //Save();
                //SaveTimer.Change(SaveIntervalMS, SaveIntervalMS);
                if (Monitor.TryEnter(this.SnapshotLock))
                {
                    try
                    {
                        if (this.Map != null)
                            if (this.Map.Rules.Autosaving)
                                Save(); // i had this commented
                    }
                    finally
                    {
                        Monitor.Exit(this.SnapshotLock);
                    }
                }
            }, new object(), Timeout.Infinite, Timeout.Infinite);
            SaveTimer.Change(SaveIntervalMS, SaveIntervalMS);

            //SaveTimer = new System.Threading.Timer((a) =>
            //{
            //    Save();
            //}, new object(), 0, SaveIntervalMS);

            PingTimer = new System.Threading.Timer((a) =>
            {
                //Thread.CurrentThread.Name = "ServerPing";
                PingPlayers(); // TODO: uncomment
                //// TODO: don't send this every time
                //SendPlayerList();  // TODO: uncomment
            }, new object(), 0, PingInterval);
            NetworkObjects = new ConcurrentDictionary<int, GameObject>();
        }
        
        private void AdvanceClock()
        {
            ServerClock = ServerClock.Add(TimeSpan.FromMilliseconds(ClockIntervalMS));
            //delay--;
            //if (delay <= 0)
            //{
                SendUnreliable();
                SendReliable();
                //delay = delayMax;
            //}


            // TODO: if i don't do this, movement on client in jerky. maybe find a way to do this less times or incorporate it in other packets?
            // attempt to sync time on client when receiving snapshots
                //SyncTime(); // TODO: uncomment
        }


        private void PingPlayers()
        {
            // PING ACTIVE PLAYERS OR PING CONNECTIONS?

            foreach (var player in
                from p in Players.GetList()
                where !p.Connection.Ping.IsRunning
                select p)
            {
                Enqueue(player, Packet.Create(player, PacketType.Ping, new byte[0], SendType.Reliable));

                // sync time here?
                //Packet.Create(player, PacketType.SyncTime, Network.Serialize(w => w.Write(ServerClock.TotalMilliseconds))).BeginSendTo(Listener, player.IP);
            }
        }

        public NetworkSideType Type { get { return NetworkSideType.Server; } }

        static int _objID = 1;
        public static int ObjID
        {
            get { return _objID++; }
        }

        

        static int _playerID = 1;
        public static int PlayerID
        {
            get { return _playerID++; }
        }

        //public Map GetMap()
        //{
        //    return Map;
        //}

        //public Map Map { get; private set; }

        /// <summary>
        /// TODO: Make it a field
        /// </summary>
        public IMap Map { get; set; }

        private void SetMap(IMap map)
        {
            this.Map = map;
            if (this.ChunkLoader != null)
                this.ChunkLoader.Stop();
            //this.ChunkLoader = new ChunkLoader(value);
            this.ChunkLoader = ChunkLoader.StartNew(this.Map);
        }

        static IWorld _World;
        static IWorld World {
            get { return _World; }
            set { _World = value; }
        }
        public static int Port = 5541;
        static Socket Listener;
        //static UdpClient Listener;
        public PlayerList Players = new PlayerList();// = new List<IPAddress>();

        public List<PlayerData> GetPlayers()
        {
            return Players.GetList();
        }

        //public static List<Socket> Sockets;
        static public event EventHandler OnPlayerConnect, OnPlayersChanged;
        static ManualResetEvent Block;
        //static ConcurrentQueue<Packet> Incoming = new ConcurrentQueue<Packet>();
        static ConcurrentQueue<Packet> Outgoing = new ConcurrentQueue<Packet>();


        //static Dictionary<long, Packet> WaitingForAck = new Dictionary<long, Packet>();

        /// <summary>
        /// A list for reliable packets awaiting for ACK for each player.
        /// </summary>
       // static Dictionary<int, Dictionary<long, Packet>> WaitingForAck = new Dictionary<int, Dictionary<long, Packet>>();
        //static ConcurrentDictionary<long, Packet> WaitingForAck = new ConcurrentDictionary<long, Packet>();

        static ConcurrentDictionary<Vector2, ConcurrentQueue<PlayerData>> ChunkRequests = new ConcurrentDictionary<Vector2, ConcurrentQueue<PlayerData>>();
        static public RandomThreaded Random;

        static public void Stop()
        {
            Server.ChunkLoaderToken.Cancel();
            if (Instance.LightingEngine != null)
                Instance.LightingEngine.Stop();
            if (Listener != null)
            {
                Listener.Close();
                //Listener.Disconnect(false);
            }

            SaveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            //SnapshotTimer.Change(Timeout.Infinite, Timeout.Infinite);
            //ServerClockTimer.Change(Timeout.Infinite, Timeout.Infinite);
            SaveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            PingTimer.Change(Timeout.Infinite, Timeout.Infinite);
      
            if (Console != null)
                Console.Write("SERVER", "Stopped");
        }
        static public void Start()
        {
            SaveTimer.Change(SaveIntervalMS, SaveIntervalMS);
            //SnapshotTimer.Change(SnapshotIntervalMS, SnapshotIntervalMS);
            //ServerClockTimer.Change(ClockIntervalMS, ClockIntervalMS);
            SaveTimer.Change(SaveIntervalMS, SaveIntervalMS);
            PingTimer.Change(PingInterval, PingInterval);

            TimeStarted = DateTime.Now.TimeOfDay;
            ServerClock = new TimeSpan();
            //Console = new UI.ConsoleBox(new Rectangle(0, 0, 800, 600)) { FadeText = false };
            Console.Write("SERVER", "Started");
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Listener.ReceiveBufferSize = Listener.SendBufferSize = Packet.Size;
            Instance.Players = new PlayerList();
            IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, Port);
            Block = new ManualResetEvent(false);
            //Task.Factory.StartNew(() =>
            //{
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
            UdpConnection state = new UdpConnection("player", remote) { Buffer = new byte[Packet.Size], IP = remote };
            Listener.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ref remote, ar =>
            {
                ReceiveMessage(ar);
            }, state);
            //});
            Console.Write(Color.Yellow, "SERVER", "Listening to port " + Port + "...");
        }

        readonly float MapSaveInterval = Engine.TargetFps * 60f;// 10f;
        float MapSaveTimer = 0;
        readonly float BlockUpdateTimerMax = Engine.TargetFps / 10f;
        float BlockUpdateTimer = 0;
        static public void Update(GameTime gt)
        {
            //CurrentTime = gt.TotalGameTime - TimeStarted;
            HandleIncoming();

            //SendOutgoing(); // put this in clocktimer


            if (Instance.Map.IsNull())
                return;
            Instance.AdvanceClock();
            UpdateMap();
            SendSnapshots(ServerClock);

            Instance.BlockUpdateTimer--;
            if (Instance.BlockUpdateTimer <= 0)
            {
                Instance.BlockUpdateTimer = Instance.BlockUpdateTimerMax;
                Instance.SendRandomBlockUpdates();
                SendSyncCells();
            }
            Instance.MapSaveTimer--;
            if (Instance.MapSaveTimer <= 0)
            {
                Instance.MapSaveTimer = Instance.MapSaveInterval;
                //Instance.Map.Save(); // autosave different depending on gamemode!
            }
            //SendChunks();
            ProcessEvents();

            //process newely loaded chunks
            //while(Instance.ReadyChunks.Count>0)
            //if (Instance.ReadyChunks.Count > 0)
            //{
            //    ChunkRequest ch;
            //    Instance.ReadyChunks.TryDequeue(out ch);
            //    HandleLoadedChunk(ch.Chunk);
            //    ch.CachedCallback(ch.Chunk);
            //}
            while(Instance.ChunksToActivate.Count>0)
            {
                var ch = Instance.ChunksToActivate.Dequeue();
                AddChunk(ch);
            }

            /// TODO: maybe reduce frequency of that call?
            UnloadChunks();
        }

        private static void UpdateMap()
        {
            if (Instance.Map != null)
                Instance.Map.Update(Instance);
        }

        /// <summary>
        /// TODO: maybe reduce frequency of that call?
        /// </summary>
        private static void UnloadChunks()
        {
            if (!Instance.Map.Rules.UnloadChunks)
                return;
            var actives = Instance.Map.GetActiveChunks();
            if (Instance.Map.IsNull())
                return;
            var chunksToUnload = new List<Chunk>();
            foreach (var chunk in actives.Values.ToList())
            {
                bool unload = true;
                foreach (var player in Instance.GetPlayers())
                {
                    if (player.Character.IsNull())
                        continue;
                    var playerchunk = player.Character.Global.GetChunkCoords();
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

        private static void ProcessEvents()
        {
            while (EventQueue.Count > 0)
            {
                GameEvent e = EventQueue.Dequeue();
                OnGameEvent(e);
            }
        }
        Random RandomSimple = new System.Random();
        void SendRandomBlockUpdates()
        {
            var actives = Instance.Map.GetActiveChunks();

            Dictionary<Vector2, List<Vector3>> updates = new Dictionary<Vector2, List<Vector3>>();
            foreach (var chunk in actives.Values.ToList())
            {
                List<Vector3> coords = new List<Vector3>();
                for (int i = 0; i < 24; i++)
                {
                    int x, y, z;
                    //x = this.GetRandom().Next(Chunk.Size);
                    //y = this.GetRandom().Next(Chunk.Size);
                    //z = this.GetRandom().Next(Map.MaxHeight);

                    // TODO: prevent chance of selecting same cell
                    x = this.RandomSimple.Next(Chunk.Size);
                    y = this.RandomSimple.Next(Chunk.Size);
                    z = this.RandomSimple.Next(global::Start_a_Town_.Map.MaxHeight);
                    Vector3 l = new Vector3(x, y, z);
                    Vector3 g = l.ToGlobal(chunk.MapCoords);//.Key);
                    coords.Add(l);

                    //g.TryGetBlock(this.Map, (block, cell) => block.RandomBlockUpdate(this, g, cell));
                    Cell cell;
                    if (this.Map.TryGetCell(g, out cell))
                        cell.Block.RandomBlockUpdate(this, g, cell);
                }
                updates[chunk.MapCoords] = coords;//.Key] = coords;
            }
            // create and send packet
            // let's try creating the packet in a new thread because compressing is slow
            Task.Factory.StartNew(() =>
            {
                byte[] data = Network.Serialize(w =>
                {
                    w.Write(updates.Count);
                    foreach (var ch in updates)
                    {
                        w.Write(ch.Key);
                        foreach (var coord in ch.Value)
                            w.Write(coord);
                    }
                });
                //Enqueue(PacketType.RandomBlockUpdates, data, SendType.Ordered | SendType.Reliable);
                foreach (var player in Players.GetList())
                {
                    if (player.IsActive)
                        Enqueue(player, Packet.Create(player, PacketType.RandomBlockUpdates, data, SendType.Ordered | SendType.Reliable));
                }
            });
        }

        private static void HandleIncoming()
        {
            Packet msg;
            //foreach (var player in Players.GetList())
            foreach(var player in from conn in Connections select conn.Value.Player)
                while (player.Incoming.TryDequeue(out msg))
                    HandleMessage(msg);

            //Packet msg;
            //while (Incoming.TryDequeue(out msg))
            //    HandleMessage(msg);
        }

        void SendUnreliable()
        {
            foreach (var player in Players.GetList())
                while (player.OutUnreliable.Any())
                {
                    Packet p;
                    if (!player.OutUnreliable.TryDequeue(out p))
                        return;
                    p.BeginSendTo(Listener, player.IP);
                }
        }
        // WORKAROUND FOR CLIENT'S RECEIVE BUFFER OVERFLOW 
        int delayMax = Engine.TargetFps / 30;
        int delay = 0;
        void SendReliable()
        {
            foreach (var player in Players.GetList())
                while (player.OutReliable.Any())
                {
                    // WORKAROUND FOR CLIENT'S RECEIVE BUFFER OVERFLOW 
                    //delay--;
                    //if (delay > 0)
                    //    break;
                    //delay = delayMax;
                    //

                    Packet packet;
                    if (!player.OutReliable.TryDequeue(out packet))
                        return;
                    //if (packet.PacketType != PacketType.Ping && packet.PacketType != PacketType.SyncTime && packet.PacketType != PacketType.RandomBlockUpdates)
                    //    packet.PacketType.ToConsole();
                    int rtt = 5000;
                    packet.Player.WaitingForAck[packet.ID] = packet;
                    packet.RTT.Restart();
                    // TODO: reset timer (change) ONLY when packet has been sent (at beginsendto callback)
                    //Timer timer = new Timer(a =>
                    packet.ResendTimer = new Timer(a =>
                    {
                        Packet p = (Packet)a;
                        if (!p.Player.WaitingForAck.ContainsKey(p.ID))
                        {
                            p.ResendTimer.Change(Timeout.Infinite, Timeout.Infinite);
                            return;
                        }
                        if (--p.Retries < 0)
                        {
                            //throw new Exception("Packet send attempts exceeded maximum");
                            p.ResendTimer.Change(Timeout.Infinite, Timeout.Infinite);
                            Packet existing;
                            p.Player.WaitingForAck.TryRemove(p.ID, out existing);
                            Console.Write(UI.ConsoleMessageTypes.Acks, Color.Orange, "SERVER", "Send attempts exceeded maximum for packet " + p);
                            Console.Write(Color.Red, "SERVER", p.Player.Name + " timed out");
                            CloseConnection(p.Player.Connection);
                            return;
                        }
                        if (p.Retries < Packet.MaxAttempts - 1)
                        {
                            Console.Write(UI.ConsoleMessageTypes.Acks, Color.Orange, "SERVER", "Resending packet " + p);
                            ("Resending packet " + p.ToString()).ToConsole();
                        }

                        p.BeginSendTo(Listener, p.Player.IP);
                        //p.ID.ToConsole();
                        //packet.ToString().ToConsole();
                        p.ResendTimer.Change(rtt, Timeout.Infinite);
                    }, packet, Timeout.Infinite, Timeout.Infinite);//rtt);
                    packet.ResendTimer.Change(0, Timeout.Infinite);
                }
        }
        //static void SendOutgoing()
        //{
        //    List<PacketMessage> messages = new List<PacketMessage>(); ;
        //    PacketMessage msg;
        //    foreach (var player in Players.GetList())
        //    {
        //        //while (player.Outgoing.TryDequeue(out msg))
        //        if (player.Outgoing.TryDequeue(out msg))
        //        {
        //            if (msg.Retries > 0)
        //            {
        //                int rtt = 5000;
        //                BeginReliableSend(msg, rtt);
        //            }
        //            if (msg.SendType == SendType.Ordered)
        //                player.OrderedPackets.Enqueue(msg);
        //            else
        //            {
        //                messages.Add(msg);
        //                Packet.Create(messages).BeginSendTo(Listener, player.IP, ar =>
        //                {
        //                    SendOutgoing();
        //                });
        //            }
        //        }
        //        //if (messages.Count > 0)
        //        //    Packet.Create(messages).BeginSendTo(Listener, player.IP, ar =>
        //        //    {
        //        //        SendOutgoing();
        //        //    });
        //    }
        //} 

        //static void SendOrderedPackets()
        //{
        //    foreach (var player in Players.GetList())
        //    {
        //        PacketMessage packet;
        //        if (!player.TryGetNextPacket(out packet))
        //            continue;
        //        //packet.BeginSendTo(Listener, player.IP);
        //        //Enqueue(player, packet);
        //        //Send(packet);
        //        int rtt = 5000;
        //        BeginReliableSend(packet, rtt);
        //    }
        //}
        //static void SendOutgoingOld()
        //{
        //    PacketMessage packet;
        //    //  foreach (var player in Players.GetList())
        //    //if (player.Outgoing.TryDequeue(out packet))
        //    if (Outgoing.TryDequeue(out packet))
        //    {
        //        if (packet.Retries > 0 && (packet.SendType & SendType.Reliable) == 0)
        //            throw new Exception("Invalid packet type");
        //        if (packet.Retries > 0 && packet.SendType == 0)
        //            throw new Exception("Invalid packet type");
        //        if (packet.Retries > 0)
        //        {
        //            int rtt = 5000;
        //            BeginReliableSend(packet, rtt);
        //        }
        //        if (packet.SendType == SendType.Ordered)
        //        {
        //            throw new Exception("Invalid packet type");
        //            packet.Player.OrderedPackets.Enqueue(packet);
        //        }
        //        else
        //        {
        //            packet.BeginSendTo(Listener, packet.Player.IP, ar =>
        //            {
        //                // WHY CALL TO SEND AGAIN IF I HAVE A TIMER THAT SENDS?
        //                // BECAUSE: in case the queue is empty, the callback chain breaks and i need a loop that checks for new outgoing packets
        //                SendOutgoing();
        //            });
        //        }
        //    }
        //}

        //static void SendOutgoing()
        //{
        //    PacketMessage packet;
        //    if (Outgoing.TryDequeue(out packet))
        //        Send(packet);
        //}

        //private static void Send(PacketMessage packet)
        //{
        //    if (packet.Retries > 0 && (packet.SendType & SendType.Reliable) == 0)
        //        throw new Exception("Invalid packet type");
        //    if (packet.Retries > 0 && packet.SendType == 0)
        //        throw new Exception("Invalid packet type");
        //    //if ((packet.SendType & SendType.Ordered) == SendType.Ordered)
        //    //{
        //    //    packet.Player.OrderedPackets.Enqueue(packet);
        //    //    return;
        //    //}
        //    //if (packet.Retries > 0)
        //    //{
        //    //    int rtt = 5000;
        //    //    BeginReliableSend(packet, rtt);
        //    //}
        //    //if (packet.SendType == SendType.Ordered)
        //    //{
        //    //    throw new Exception("Invalid packet type");
        //    //    packet.Player.OrderedPackets.Enqueue(packet);
        //    //}
        //    //else
        //    //{
        //        packet.BeginSendTo(Listener, packet.Player.IP, ar =>
        //        {
        //            // WHY CALL TO SEND AGAIN IF I HAVE A TIMER THAT SENDS?
        //            // BECAUSE: in case the queue is empty, the callback chain breaks and i need a loop that checks for new outgoing packets
        //            SendOutgoing();
        //        });
        //    //}
        //}
        //private static void BeginReliableSend(PacketMessage packet, int rtt)
        //{
        //    //packet.Player.WaitingForAck.Add(packet.ID, packet); // TODO: figure out why it threw null reference exception //maybe because it tried again?
        //    packet.Player.WaitingForAck[packet.ID] = packet;
        //    packet.RTT.Restart();
        //    //if (!Connections.ContainsKey(packet.Connection.IP))
        //    //if (!Players.GetList().Contains(packet.Player))
        //    //{
        //    //    //drop packet
        //    //    packet.Player.WaitingForAck.TryRemove(packet.ID, out packet);
        //    //    return;
        //    //}

        //    // TODO: reset timer (change) ONLY when packet has been sent (at beginsendto callback)
        //    Timer timer = new Timer(a =>
        //    {
        //        PacketMessage p = (PacketMessage)a;
        //        //if ack already received, stop timer;
        //        //if (!WaitingForAck.Contains(p.ID))
        //        if (!packet.Player.WaitingForAck.ContainsKey(p.ID))
        //        {
        //            p.ResendTimer.Change(Timeout.Infinite, Timeout.Infinite);
        //       //     Console.Write(UI.ConsoleMessageTypes.Acks, Color.Lime, "SERVER", "Ack received for packet " + p);
        //            return;
        //        }
        //        if (--p.Retries < 0)
        //        {
        //            throw new Exception("Packet send attempts exceeded maximum");
        //            p.ResendTimer.Change(Timeout.Infinite, Timeout.Infinite);
        //            //packet.Player.WaitingForAck.Remove(p.ID);
        //            PacketMessage existing;
        //            packet.Player.WaitingForAck.TryRemove(p.ID, out existing);

        //            Console.Write(UI.ConsoleMessageTypes.Acks, Color.Orange, "SERVER", "Send attempts exceeded maximum for packet " + p);
        //            Console.Write(Color.Red, "SERVER", p.Player.Name + " timed out");
        //            CloseConnection(p.Player.Connection);
        //            return;
        //        }
        //        //("resending packet " + p).ToConsole();
        //        if (p.Retries < PacketMessage.MaxAttempts - 1)
        //            Console.Write(UI.ConsoleMessageTypes.Acks, Color.Orange, "SERVER", "Resending packet " + p);
        //        packet.BeginSendTo(Listener, packet.Player.IP);
        //    }, packet, 0, rtt);
        //    packet.ResendTimer = timer;
        //}

        //public void Enqueue(Packet packet, Vector3 global)
        //{
        //    foreach (var player in Players.GetList().Where(player => player.IsWithin(global)))
        //        if ((packet.SendType & SendType.Reliable) == SendType.Reliable)
        //            player.OutReliable.Enqueue(packet);
        //        else
        //            player.OutUnreliable.Enqueue(packet);
        //}
        public void Forward(Packet p)
        {
            this.Enqueue(p.PacketType, p.Payload);
        }
        internal void Enqueue(PacketType packetType, byte[] p)
        {
            this.Enqueue(packetType, p, SendType.OrderedReliable, true);
        }
        public void Enqueue(PlayerData player, Packet packet)
        {
            //player.Outgoing.Enqueue(packet);
            //Outgoing.Enqueue(packet);
            if ((packet.SendType & SendType.Reliable) == SendType.Reliable)
                player.OutReliable.Enqueue(packet);
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
                    var parts = PacketTransfer.Split(player.PacketSequence, data);
                    var part1 = parts[0];
                    var part2 = parts[1];
                    Enqueue(player, Packet.Create(player.PacketSequence, PacketType.Partial, part1, player, send));
                    Enqueue(player, Packet.Create(player.PacketSequence, PacketType.Partial, part2, player, send));
                }
                return;
            }
            foreach (var player in Players.GetList())
                Enqueue(player, Packet.Create(player, type, data, send));
        }
        public void Enqueue(PacketType type, byte[] data, SendType send, Vector3 global)
        {
            this.Enqueue(type, data, send, player=>player.IsWithin(global));
        }
        public void Enqueue(PacketType type, byte[] data, SendType send, Vector3 global, bool sync)
        {
            foreach (var player in Players.GetList().Where(player => player.IsWithin(global)))
            {
                var p = Packet.Create(player, type, data, send);
                var t = this.Clock.TotalMilliseconds;
                p.Tick = this.Clock.TotalMilliseconds;
                Enqueue(player, p);
            }
        }
        public void Enqueue(PacketType type, byte[] data, SendType send, bool sync)
        {
            foreach (var player in Players.GetList())
            {
                var p = Packet.Create(player, type, data, send);
                var t = this.Clock.TotalMilliseconds;
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
                    var parts = PacketTransfer.Split(player.PacketSequence, data);
                    var part1 = parts[0];
                    var part2 = parts[1];
                    Enqueue(player, Packet.Create(player.PacketSequence, PacketType.Partial, part1, player, send));
                    Enqueue(player, Packet.Create(player.PacketSequence, PacketType.Partial, part2, player, send));
                }
                return;
            }
            foreach (var player in Players.GetList().Where(filter))
                Enqueue(player, Packet.Create(player, type, data, send));
        }

        //private static void Enqueue(PacketType type, byte[] data, SendType send)
        //{
        //    if (data.Length > 60000)
        //    {
        //        foreach (var player in Players.GetList())
        //        {
        //            var parts = PacketTransfer.Split(player.PacketSequence, data);
        //            var part1 = parts[0];
        //            var part2 = parts[1];
        //            Enqueue(player, PacketMessage.Create(player.PacketSequence, PacketType.Partial, part1, player, send));
        //            Enqueue(player, PacketMessage.Create(player.PacketSequence, PacketType.Partial, part2, player, send));
        //        }
        //        return;
        //    }
        //    foreach (var player in Players.GetList())
        //        Enqueue(player, PacketMessage.Create(player.PacketSequence, type, data, player, send));
        //}
        //private static void Enqueue(PacketType type, byte[] data, SendType send)
        //{
        //    foreach (var player in Players.GetList())
        //    // player.
        //    {
        //        Outgoing.Enqueue(PacketMessage.Create(player.PacketSequence, type, data, player, PacketMessage.MaxAttempts, send));
        //    }
        //}
        static ConcurrentDictionary<EndPoint, UdpConnection> Connections = new ConcurrentDictionary<EndPoint, UdpConnection>();
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
                    if (packet.PacketType != PacketType.RequestPlayerID)
                    {
                        throw new Exception("Invalid sender");
                        return;
                    }
                    Server.Console.Write(Color.Yellow, "SERVER", remoteIP + " connecting...");
                    UdpConnection newConnection = CreateConnection(remoteIP);
                    state = newConnection;
                }

                packet.Connection = state;
                packet.Player = state.Player;

                //Incoming.Enqueue(packet);
                packet.Player.Incoming.Enqueue(packet);
            }
            catch (SocketException e)
            {
                "connection closed".ToConsole();
                CloseConnection(state);
            }
            catch (ObjectDisposedException) { }
        }

        private static UdpConnection CreateConnection(EndPoint remoteIP)
        {
            PlayerData newPlayer = new PlayerData(remoteIP);
            UdpConnection newConnection = new UdpConnection(newPlayer.IP.ToString(), remoteIP);
            newConnection.Player = newPlayer;
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
            Action<Server, Packet> registeredPacket;
            if (Instance.Packets.TryGetValue(msg.PacketType, out registeredPacket))
            {
                registeredPacket(Instance, msg);
                return;
            }

            IServerPacketHandler handler;
            if(Instance.PacketHandlers.TryGetValue(msg.PacketType, out handler))
            {
                handler.HandlePacket(Instance, msg);
                return;
            }
            switch (msg.PacketType)
            {
                case PacketType.RequestPlayerID:
                    //PlayerData temp = Network.Deserialize<PlayerData>(msg.Payload, PlayerData.Read);

                    string name = Network.Deserialize<string>(msg.Payload, r =>
                    {
                        return r.ReadString();
                        //Encoding.ASCII.GetString();
                    });

                    Server.Console.Write(Color.Lime, "SERVER", name + " connected from " + msg.Connection.IP);

                    PlayerData pl = msg.Connection.Player;//  new PlayerData(msg.Sender) { Socket = Listener, Name = name, ID = PlayerID };
                    pl.Name = name;
                    pl.ID = PlayerID;
                    msg.Player = pl;

                    


                    Instance.Enqueue(pl, Packet.Create(msg.Player, msg.PacketType, Network.Serialize(w =>
                    {
                        w.Write(msg.Player.ID);
                    }), SendType.Reliable | SendType.Ordered));
                    UdpConnection state = new UdpConnection(pl.Name + " listener", pl.IP) { Buffer = new byte[Packet.Size], Player = pl };
                    EndPoint ip = state.IP;
                    Listener.BeginReceiveFrom(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ref ip, ReceiveMessage, state);
                    foreach (var player in from p in Instance.Players.GetList() select p)
                        //player.Outgoing.Enqueue(PacketMessage.Create(msg.Player.PacketSequence, PacketType.PlayerConnecting, Network.Serialize(msg.Player.Write), player));
                        Instance.Enqueue(player, Packet.Create(msg.Player, PacketType.PlayerConnecting, Network.Serialize(msg.Player.Write), SendType.Reliable | SendType.Ordered));

                    // add to players when entering world instead?
                    Instance.Players.Add(pl);
                    //GameMode.Current.PacketHandler.Handle(Instance, msg);
                    GameMode.Current.HandlePacket(Instance, msg);

                    return;

                // the server sends back a copy of the player character
                case PacketType.PlayerEnterWorld:
                    //GameMode.Current.PacketHandler.Handle(Instance, msg);
                    GameMode.Current.HandlePacket(Instance, msg);

                    return;
                    //GameObject obj = Network.Deserialize<GameObject>(msg.Payload, GameObject.CreatePrefab);//CreateCustomObject);
                    GameObject obj = Network.Deserialize<GameObject>(msg.Payload, PlayerEntity.Create);//CreateCustomObject);
                   

                    // let obj instantiate itself
                    //Instance.Instantiate(obj);
                    obj.Instantiate(Instance.Instantiator);
                    // msg.Player.Character = obj;
                    //Instance.Spawn(obj);
                    msg.Player.CharacterID = obj.Network.ID;
                    msg.Player.Character = obj;
                    obj.Network.PlayerID = msg.Player.ID;

                    //add player to list of active players (whose character is in the world and must receive world updates
                    //Players.Add(msg.Player);


                    Instance.SendWorldInfo(msg.Player);
                    Instance.SendMapInfo(msg.Player);
                    msg.Player.IsActive = true;
                    //load chunks around player
                //Vector2.Zero.GetSpiral(3).ForEach(ChunksToLoad.Add);

                    obj.Global = Vector3.Zero;
                   
                    Vector2 chunkCoords = obj.Global.GetChunkCoords();
                    //LoadChunk(chunkCoords);

                    // if chunk already active just spawn player entity
                    Chunk chunk;
                    var activeChunks = Instance.Map.GetActiveChunks();
                    if (activeChunks.TryGetValue(chunkCoords, out chunk))
                    {
                        Instance.SendChunk(msg.Player, chunk);
                        Instance.InstantiateSpawnPlayerEntity(msg, obj);
                        foreach (var vector in chunkCoords.GetSpiral().Except(new Vector2[] { chunkCoords }))
                            if (activeChunks.TryGetValue(vector, out chunk))
                                Instance.SendChunk(msg.Player, chunk);
                            else
                                LoadChunk(vector, c=>Instance.SendChunk(msg.Player, c));
                    }
                    else
                        LoadChunk(chunkCoords, c =>
                        {
                            Instance.SendChunk(msg.Player, c);
                            Instance.InstantiateSpawnPlayerEntity(msg, obj);
                            // load chunks around player
                            foreach (var vector in chunkCoords.GetSpiral().Except(new Vector2[] { chunkCoords }))
                                if (activeChunks.TryGetValue(vector, out chunk))
                                    Instance.SendChunk(msg.Player, chunk);
                                else
                                    LoadChunk(vector, n => Instance.SendChunk(msg.Player, n));
                        });
                        //Instance.ChunkLoader.TryEnqueue(chunkCoords, ch =>
                        //    {
                        //        InstantiateChunk(ch);
                        //        AddChunk(ch);
                        //        Instance.SendChunk(msg.Player, ch);
                        //        InstantiateSpawnPlayerEntity(msg, obj);

                        //        // load chunks around player
                        //        foreach (var vector in chunkCoords.GetSpiral().Except(new Vector2[] { chunkCoords }))
                        //            LoadChunk(vector, (c) => Instance.SendChunk(msg.Player, c));

                        //        if (!ch.LightValid)
                        //            ResetLight(ch, () =>
                        //            {
                        //                ch.LightValid = true;
                        //            });
                        //    });
                    return;


                //case PacketType.PlayerExitWorld:
                //    GameObject plChar;
                //    if(!Instance.TryGetNetworkObject(msg.Player.CharacterID, out plChar))
                //        throw new Exception("Could not remove player character");

                //    // player exited so stop sending him world updates
                //    //Players.Remove(msg.Player);
                //    msg.Player.IsActive = false;

                //    Instance.Despawn(plChar);
                //    Instance.DisposeObject(plChar);
                //    foreach (var player in Players.GetList())
                //        if (player.IsActive)
                //            Enqueue(player, PacketMessage.Create(msg.Player, PacketType.PlayerDisconnected, Network.Serialize(w => msg.Player.Write(w)), SendType.Reliable | SendType.Ordered));
                //        //Enqueue(player, PacketMessage.Create(msg.Player.PacketSequence, PacketType.PlayerExitWorld, Network.Serialize(w => w.Write(msg.Player.ID)), player, SendType.Reliable | SendType.Ordered));
                //    break;

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

                case PacketType.PlayerData:
                    "playerdata".ToConsole();
                    break;

                case PacketType.PlayerList:
                    "playerlist".ToConsole();
                    break;

                case PacketType.RequestMapInfo:
                    Instance.SendMapInfo(msg.Player);
                    return;


                case PacketType.RequestWorldInfo:
                    Instance.SendWorldInfo(msg.Player);
                    return;

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
                
                //// the server sends back a copy of the player character
                //case PacketType.PlayerEnteredWorld:
                //    GameObject obj = Network.Deserialize<GameObject>(msg.Payload, GameObject.CreatePrefab);//CreateCustomObject);

                //    // let obj instantiate itself
                //    //Instance.Instantiate(obj);
                //    obj.Instantiate(Instance.Instantiator);
                //    // msg.Player.Character = obj;
                //    Instance.Spawn(obj);
                //    msg.Player.CharacterID = obj.NetworkID;

                //    // send only playerdata because it includes their character object and will be serialized along with other info
                //    //Packet.Create(PacketType.ObjectCreate, Net.Network.Serialize(obj.Write)).Send(from p in Players select p.Socket); // send to sender too to create their character
                //    //Packet.Create(msg.PacketType, Net.Network.Serialize(msg.Player.Write)).Send(from p in Players select p.Socket);
                //    foreach (var player in
                //        from p in Players.GetList()// select p)
                //        where p != msg.Player
                //        select p)
                //    {
                //        //Enqueue(Packet.Create(player.PacketSequence, PacketType.InstantiateObject, Net.Network.Serialize(obj.Write), player, 5)); // send to sender too to create their character
                //        Enqueue(Packet.Create(player.PacketSequence, PacketType.InstantiateAndSpawnObject, Net.Network.Serialize(obj.Write), player, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable)); // send to sender too to create their character
                //        Enqueue(Packet.Create(player.PacketSequence, msg.PacketType, Net.Network.Serialize(msg.Player.Write), player, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
                //    }
                //    // send a message to the newly connected client to own their character
                //    Outgoing.Enqueue(Packet.Create(msg.Player.PacketSequence, PacketType.AssignCharacter, Network.Serialize(obj.Write), msg.Player, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));//.Send(msg.Sender);
                //    //OutgoingMessages.Enqueue(Packet.Create(PacketID, PacketType.AssignCharacter, Network.Serialize(writer => writer.Write(obj.NetworkID)), msg.Player, 5));//.Send(msg.Sender);
                //    SyncTime();
                //    return;

                case PacketType.PlayerServerCommand:
                    msg.Payload.Deserialize(r =>
                    {
                        //Command(r.ReadASCII());
                        //CommandParser.Execute(Instance, r.ReadASCII());
                        CommandParser.Execute(Instance, msg.Player, r.ReadASCII());

                    });
                    break;

                case PacketType.Chat:
                    /* log chat text here
                    /
                     */

                    // check if server command
                    

                    Instance.Enqueue(PacketType.Chat, msg.Payload, SendType.OrderedReliable);
                    break;

                case PacketType.PlayerInputOld:
                    // handle player input differently ?
                    //GameObject plObj = Instance.GetNetworkObject(Players.GetList()[msg.Player.ID].CharacterID);
                    GameObject plObj;
                    if (!Instance.TryGetNetworkObject(msg.Player.CharacterID, out plObj))
                    {
                        Console.Write(Color.Red, "SERVER", "Error processing packet " + msg + " (player character doesn't exist)");
                        return;
                    };

                    msg.Payload.Deserialize(r =>
                    {
                        //TimeSpan playerInputTime = TimeSpan.FromMilliseconds(r.ReadDouble());
                        double timestamp = r.ReadDouble();

                        // TODO: make receiving player input args as a separate playerinput class, instead of objecteventargs
                        //ObjectEventArgs a = ObjectEventArgs.Create(Instance, r);
                        //Instance.PostLocalEvent(plObj, a);

                        TargetArgs recipient = TargetArgs.Read(Instance, r);
                        ObjectEventArgs a = ObjectEventArgs.Create(Instance, r);
                        Instance.PostLocalEvent(recipient.Object, a);

                        /*
                         * check for validity of input around here somewhere
                        */

                        // forward input to players (as an objectevent packet?)
                        // SEND EVENT RELIABLY
                        byte[] data = Network.Serialize(w =>
                        {
                            w.Write(timestamp);
                            recipient.Write(w);// w.Write(msg.Player.CharacterID);
                            w.Write((int)a.Type);
                            w.Write(a.Data.Length);
                            w.Write(a.Data);
                            //ObjectLocalEventArgs.Create(a.Type, new TargetArgs(plObj),
                        });
                        foreach (var p in Instance.Players.GetList())
                            //p.Outgoing.Enqueue(PacketMessage.Create(p.PacketSequence, PacketType.ObjectEvent, data, p, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable)); //
                            Instance.Enqueue(p, Packet.Create(p, PacketType.ObjectEvent, data, SendType.Ordered | SendType.Reliable)); //
                        //Instance.EventsSinceLastSnapshot.Enqueue(new EventSnapshot(plObj, msg.Payload));
                    });
                    return;

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
                                w.Write(source.Slot.Object.Network.ID);
                            destination.Write(w);
                            w.Write(destination.Slot.StackSize);
                            if (destination.Slot.StackSize > 0)
                                w.Write(destination.Slot.Object.Network.ID);
                        });
                        Instance.Enqueue(PacketType.PlayerInventoryChange, data, SendType.OrderedReliable, msg.Player.Character.Global);
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

                        byte[] data = Network.Serialize(w =>
                        {
                            w.Write(2);
                            source.Write(w);
                            w.Write(source.Slot.StackSize);
                            if (source.Slot.StackSize > 0)
                                w.Write(source.Slot.Object.Network.ID);
                            destination.Write(w);
                            w.Write(destination.Slot.StackSize);
                            if (destination.Slot.StackSize > 0)
                                w.Write(destination.Slot.Object.Network.ID);
                        });
                        Instance.Enqueue(PacketType.PlayerInventoryChange, data, SendType.OrderedReliable, msg.Player.Character.Global); // WARNING!!! TODO: handle case where each slot is owned by a different entity
                    });

                    return;
                case PacketType.PlayerInventoryOperationOld:
                    msg.Payload.Deserialize(r =>
                    {
                        double timestamp = r.ReadDouble();
                        TargetArgs recipient = TargetArgs.Read(Instance, r);
                        Components.ArrangeChildrenArgs invArgs = Components.ArrangeChildrenArgs.Translate(Instance, r);
                        Instance.InventoryOperation(recipient.Object, invArgs);
                        byte[] data = Network.Serialize(w =>
                        {
                            w.Write(timestamp);
                            recipient.Write(w);
                            invArgs.Write(w);
                        });
                        foreach (var p in Instance.Players.GetList())
                            //p.Outgoing.Enqueue(PacketMessage.Create(p.PacketSequence, PacketType.PlayerInventoryOperation, data, p, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
                            Instance.Enqueue(p, Packet.Create(p, PacketType.PlayerInventoryOperationOld, data, SendType.Ordered | SendType.Reliable));

                        //double timestamp = r.ReadDouble();
                        //TargetArgs recipient = TargetArgs.Read(Instance, r);
                        //ObjectEventArgs a = ObjectEventArgs.Create(Instance, r);
                        //Instance.PostLocalEvent(recipient.Object, a);
                        //byte[] data = Network.Serialize(w =>
                        //{
                        //    w.Write(timestamp);
                        //    recipient.Write(w);
                        //    w.Write((int)a.Type);
                        //    w.Write(a.Data.Length);
                        //    w.Write(a.Data);
                        //});
                        //foreach (var p in Players.GetList())
                        //    Enqueue(Packet.Create(p.PacketSequence, PacketType.ObjectEvent, data, p, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
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

                        if (!Instance.Map.PositionExists(global))
                            return;
                        // DONT CALL PREVIOUS BLOCK'S REMOVE METHOD
                        // when in block editing mode, we don't want to call block's remove method, so for example they don't pop out their contents or have any other effects to the world
                        // HOWEVER we want to dispose their contents (gameobjects) if any! 
                        // so 1) query their contents and dispose them here? 
                        //    2) call something like dispose() on them and let them dispose them themselves?
                        // TODO: DECIDE!
                        var previousBlock = Instance.Map.GetBlock(global);
                        previousBlock.Remove(Instance.Map, global);
                        //previousBlock.Dispose(Instance.Map, global);

                        var block = Start_a_Town_.Block.Registry[type];
                        block.Place(Instance.Map, global, data, variation, orientation);
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
                    msg.Player.Character.GetComponent<MobileComponent>().Start(msg.Player.Character);
                    Instance.Enqueue(PacketType.PlayerStartMoving, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    return;

                case PacketType.PlayerStopMoving:
                    //msg.Player.Character.GetComponent<ControlComponent>().FinishScript(Script.Types.Walk);
                    msg.Player.Character.GetComponent<MobileComponent>().Stop(msg.Player.Character);
                    Instance.Enqueue(PacketType.PlayerStopMoving, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    return;

                case PacketType.PlayerJump:
                    //msg.Player.Character.GetComponent<ControlComponent>().StartScript(Script.Types.Jumping, new ScriptArgs(Instance, msg.Player.Character));
                    entity = msg.Player.Character;
                    entity.GetComponent<MobileComponent>().Jump(entity);
                    Instance.Enqueue(PacketType.PlayerJump, msg.Payload, SendType.OrderedReliable, entity.Global, true);
                    return;

                case PacketType.PlayerChangeDirection:
                    var packet = new PacketPlayerChangeDirection(Instance, msg.Decompressed);//Payload);
                    packet.Entity.Direction = packet.Direction;
                    //Instance.Enqueue(PacketType.PlayerChangeDirection, msg.Payload, SendType.OrderedReliable, packet.Entity.Global, true);
                    // SEND DIRECTION PACKETS OR LET CLIENT CALCULATE DIRECTION FROM SNAPSHOTS?
                    // ALSO ALLOW CLIENT DIRECTION PREDICTION BY ASSIGNING IT DIRECTLY WHEN HANDLING INPUT?

                    //msg.Payload.Deserialize(r =>
                    //{
                    //    int netid = r.ReadInt32();
                    //    Vector3 direction = r.ReadVector3();
                    //    msg.Player.Character.Direction = direction;
                    //    Instance.Enqueue(PacketType.PlayerChangeDirection, msg.Payload, SendType.Ordered | SendType.Reliable);
                    //});
                    return;

                case PacketType.PlayerToggleWalk:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        bool toggle = r.ReadBoolean();
                        //msg.Player.Character.GetComponent<MobileComponent>().CurrentState = toggle ? MobileComponent.State.Walking : MobileComponent.State.Running;
                        msg.Player.Character.GetComponent<MobileComponent>().ToggleWalk(toggle);
                        Instance.Enqueue(PacketType.PlayerToggleWalk, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                case PacketType.PlayerToggleSprint:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        bool toggle = r.ReadBoolean();
                        //msg.Player.Character.GetComponent<MobileComponent>().CurrentState = toggle ? MobileComponent.State.Sprinting : MobileComponent.State.Running;
                        msg.Player.Character.GetComponent<MobileComponent>().ToggleSprint(toggle);
                        Instance.Enqueue(PacketType.PlayerToggleSprint, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                case PacketType.PlayerInteract:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        //msg.Player.Character.GetComponent<WorkComponent>().Perform(target.GetAvailableTasks(Instance).FirstOrDefault(), target);
                        msg.Player.Character.GetComponent<WorkComponent>().UseTool(msg.Player.Character, target);
                        Instance.Enqueue(PacketType.PlayerInteract, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                case PacketType.PlayerUse:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, target.GetAvailableTasks(Instance).FirstOrDefault(), target);
                        Instance.Enqueue(PacketType.PlayerUse, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                case PacketType.PlayerUseHauled:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        //var hauled = msg.Player.Character.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling].Object;
                        var hauled = msg.Player.Character.GetComponent<HaulComponent>().GetObject();//.Slot.Object;

                        if (hauled.IsNull())
                            return;
                        msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, hauled.GetHauledActions(target).FirstOrDefault(), target);
                        Instance.Enqueue(PacketType.PlayerUseHauled, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                case PacketType.PlayerDropHauled:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        //var interaction = target.GetContextAction(new KeyBinding(GlobalVars.KeyBindings.Drop));
                        //if(interaction == null)
                            //msg.Player.Character.GetComponent<GearComponent>().Throw(Vector3.Zero, msg.Player.Character);
                        msg.Player.Character.GetComponent<HaulComponent>().Throw(Vector3.Zero, msg.Player.Character);

                        //else
                        //    msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, interaction, target);
                        Instance.Enqueue(PacketType.PlayerDropHauled, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                case PacketType.PlayerDropInventory:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        byte slotid = r.ReadByte();
                        int amount = r.ReadInt32();
                        Instance.PostLocalEvent(msg.Player.Character, Message.Types.DropInventoryItem, (int)slotid, amount);
                        Instance.Enqueue(PacketType.PlayerDropInventory, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
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

                        //target.Object.HandleRemoteCall(ObjectEventArgs.Create(call, args));
                        target.HandleRemoteCall(Instance, ObjectEventArgs.Create(call, args));

                        Instance.Enqueue(PacketType.PlayerRemoteCall, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global);
                    });
                    return;

                case PacketType.PlayerInput:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        var input = new PlayerInput(r);
                        //var interaction = target.GetContextActionWorld(Instance, input) ?? PlayerInput.GetDefaultAction(input.Action);
                        var interaction = 
                            //target.GetContextActionWorld(Instance, input) ??
                            //PlayerInput.GetDefaultInput(input);
                            PlayerInput.GetDefaultInput(msg.Player.Character, target, input);

                        //interaction.ToConsole();
                        if (interaction == null)
                            return;
                        msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, interaction, target);
                        if (interaction.Conditions.Evaluate(msg.Player.Character, target))
                            //Instance.Enqueue(PacketType.EntityInteract, new PacketEntityInteractionTarget(msg.Player.Character, interaction.Name, target).Write(), SendType.OrderedReliable, msg.Player.Character.Global, true);
                            Instance.Enqueue(PacketType.PlayerInput, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                case PacketType.PlayerUseInteraction:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        string iname = r.ReadString();
                        var interaction = target.GetInteraction(Instance, iname);
                        if (interaction == null)
                            throw new Exception();
                        msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, interaction, target);
                        var p = new PacketEntityInteractionTarget(msg.Player.Character, iname, target);
                        Instance.Enqueue(PacketType.EntityInteract, Network.Serialize(p.Write));
                    });
                    return;

                case PacketType.PlayerPickUp:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        //switch (target.Type)
                        //{
                        //    case TargetType.Position:
                        //        // check if hauling and drop at target position
                        //        GameObject held = msg.Player.Character.GetComponent<GearComponent>().Holding.Take();
                        //        if (held.IsNull())
                        //            return;
                        //        //held.Global = target.FinalGlobal;
                        //        Instance.Spawn(held, target.FinalGlobal);
                        //        break;
                        //    case TargetType.Entity:
                        //        //pickup item or switch places with held item
                        //        Instance.PostLocalEvent(msg.Player.Character, Message.Types.Insert, target.Object.ToSlot());
                        //        break;
                        //    default:
                        //        break;
                        //}
                        msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, new Components.Interactions.PickUp(), target);
                        Instance.Enqueue(PacketType.PlayerPickUp, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                case PacketType.PlayerCarry:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        //msg.Player.Character.GetComponent<ControlComponent>().StartScript(Script.Types.Hauling, new ScriptArgs(Instance, msg.Player.Character, target));
                        msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, new Components.Interactions.Carry(), target);
                        Instance.Enqueue(PacketType.PlayerCarry, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                case PacketType.PlayerEquip:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        msg.Player.Character.GetComponent<ControlComponent>().StartScript(Script.Types.Equipping, new ScriptArgs(Instance, msg.Player.Character, target));
                        Instance.Enqueue(PacketType.PlayerEquip, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                case PacketType.PlayerUnequip:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        //PersonalInventoryComponent.InsertOld(msg.Player.Character, target.Slot);
                        PersonalInventoryComponent.Receive(msg.Player.Character, target.Slot, false);
                        Instance.Enqueue(PacketType.PlayerUnequip, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                case PacketType.EntityThrow:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        var dir = r.ReadVector3();
                        var all = r.ReadBoolean();
                        //msg.Player.Character.GetComponent<HaulComponent>().Throw(msg.Player.Character, dir, all);
                        HaulComponent.ThrowHauled(msg.Player.Character, dir, all);
                        Instance.Enqueue(PacketType.EntityThrow, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                case PacketType.PlayerStartAttack:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        //msg.Player.Character.GetComponent<ControlComponent>().StartScript(Script.Types.Attack, new ScriptArgs(Instance, msg.Player.Character, TargetArgs.Empty));
                        msg.Player.Character.GetComponent<AttackComponent>().Start(msg.Player.Character);
                        Instance.Enqueue(PacketType.PlayerStartAttack, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                case PacketType.PlayerFinishAttack:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        var dir = r.ReadVector3();
                        //msg.Player.Character.GetComponent<ControlComponent>().FinishScript(Script.Types.Attack, new ScriptArgs(Instance, msg.Player.Character, TargetArgs.Empty, w => w.Write(dir)));
                        msg.Player.Character.GetComponent<AttackComponent>().Finish(msg.Player.Character, dir);
                        Instance.Enqueue(PacketType.PlayerFinishAttack, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                case PacketType.PlayerStartBlocking:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        //msg.Player.Character.GetComponent<ControlComponent>().StartScript(Script.Types.Block, new ScriptArgs(Instance, msg.Player.Character, TargetArgs.Empty));
                        msg.Player.Character.GetComponent<Components.Combat.BlockingComponent>().Start(msg.Player.Character);
                        Instance.Enqueue(PacketType.PlayerStartBlocking, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                case PacketType.PlayerFinishBlocking:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        //msg.Player.Character.GetComponent<ControlComponent>().FinishScript(Script.Types.Block, new ScriptArgs(Instance, msg.Player.Character, TargetArgs.Empty));
                        msg.Player.Character.GetComponent<Components.Combat.BlockingComponent>().Stop(msg.Player.Character);
                        Instance.Enqueue(PacketType.PlayerFinishBlocking, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global, true);
                    });
                    return;

                //case PacketType.PlayerSlotClick:
                //    Network.Deserialize(msg.Payload, r =>
                //    {
                //        double timestamp = r.ReadDouble();
                //        TargetArgs actor = TargetArgs.Read(Instance, r);
                //        TargetArgs slot = TargetArgs.Read(Instance, r);
                //        Components.ArrangeChildrenArgs invArgs = Components.ArrangeChildrenArgs.Translate(Instance, r);
                //        Instance.InventoryOperation(recipient.Object, invArgs);
                //        byte[] data = Network.Serialize(w =>
                //        {
                //            w.Write(timestamp);
                //            recipient.Write(w);
                //            invArgs.Write(w);
                //        });
                //        foreach (var p in Players.GetList())
                //            Enqueue(Packet.Create(p.PacketSequence, PacketType.PlayerInventoryOperation, data, p, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
                //    });
                //    return;

                //case PacketType.PlayerCraft:
                //    Network.Deserialize(msg.Payload, r =>
                //    {
                //        int chid = r.ReadInt32();
                //        Components.Crafting.Reaction.Product.ProductMaterialPair product = new Components.Crafting.Reaction.Product.ProductMaterialPair(r);

                //        msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, new Components.Interactions.Craft(product), new TargetArgs());


                //        byte[] newData = Network.Serialize(w =>
                //        {
                //            w.Write(chid);
                //            product.Write(w);
                //        });
                //        Instance.Enqueue(PacketType.PlayerCraft, newData, SendType.OrderedReliable, msg.Player.Character.Global);
                //    });
                //    return;

                case PacketType.PlayerCraftRequest:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int chid = r.ReadInt32();
                        var crafting = new Components.Crafting.CraftOperation(Instance, r);
                        var reaction = Components.Crafting.Reaction.Dictionary[crafting.ReactionID];
                        if (reaction == null)
                            return;
                        var product = reaction.Products.First().GetProduct(reaction, crafting.Building.Object, crafting.Materials, crafting.Tool);
                        if (product == null)
                            return;
                        if (product.Tool != null)
                            GearComponent.Equip(msg.Player.Character, PersonalInventoryComponent.FindFirst(msg.Player.Character, foo => foo == product.Tool));
                        //msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, new Components.Interactions.Craft(product, crafting.Container), crafting.Building);
                        var workstation = Instance.Map.GetBlockEntity(crafting.WorkstationEntity) as Blocks.BlockWorkbench.Entity;
                        //msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, new Components.Crafting.InteractionCraftingWorkbench(product, workstation, crafting.WorkstationEntity), crafting.Building);

                        if(workstation == null)
                            msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, new Components.Crafting.InteractionCraftingPerson(product), crafting.Building);
                        else
                            msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, new Components.Crafting.InteractionCraftingWorkbench(product, workstation, crafting.WorkstationEntity), crafting.Building);

                        //byte[] newData = Network.Serialize(w =>
                        //{
                        //    w.Write(chid);
                        //    crafting.Building.Write(w);
                        //    product.Write(w);

                        //    w.Write(crafting.Container.Parent.Network.ID);
                        //    w.Write(crafting.Container.ID);
                        //});
                        //Instance.Enqueue(PacketType.PlayerCraft, newData, SendType.OrderedReliable, msg.Player.Character.Global);
                        Instance.Enqueue(PacketType.PlayerCraft, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global);

                    });
                    return;

                //case PacketType.PlaceBlockConstruction:
                //    Network.Deserialize(msg.Payload, r =>
                //    {
                //        var netid = r.ReadInt32();
                //        Components.Crafting.BlockConstruction.ProductMaterialPair product = new Components.Crafting.BlockConstruction.ProductMaterialPair(r);
                //        Vector3 global = r.ReadVector3();
                //        msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, new Components.Interactions.InteractionConstruct(product), new TargetArgs(global));
                //        Instance.Enqueue(PacketType.PlaceBlockConstruction, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global);
                //        return;

                //        obj = GameObject.Create(GameObject.Types.ConstructionBlock);
                //        obj.Global = global;
                //        obj.GetComponent<ConstructionComponent>().SetProduct(product);
                //        Instance.SyncInstantiate(Instance.Instantiate(obj));
                //        Instance.SyncSpawn(obj);
                //    });
                //    return;

                //case PacketType.PlaceConstruction:
                //    Network.Deserialize(msg.Payload, r =>
                //    {
                //        var netid = r.ReadInt32();
                //        //GameObject.Types type = (GameObject.Types)r.ReadInt32();
                //        Components.Crafting.Reaction.Product.ProductMaterialPair product = new Components.Crafting.Reaction.Product.ProductMaterialPair(r);
                //        Vector3 global = r.ReadVector3();
                //        msg.Player.Character.GetComponent<WorkComponent>().Perform(msg.Player.Character, new Modules.Construction.InteractionConstruct(product), new TargetArgs(global));
                //        Instance.Enqueue(PacketType.PlaceConstruction, msg.Payload, SendType.OrderedReliable, msg.Player.Character.Global);
                //        return;

                //        obj = GameObject.Create(GameObject.Types.Construction);
                //        obj.Global = global;
                //        //obj.GetComponent<StructureComponent>().Product = product;
                //        obj.GetComponent<StructureComponent>().SetProduct(product);
                //        Instance.SyncInstantiate(Instance.Instantiate(obj));
                //        Instance.SyncSpawn(obj);
                //    });
                //    return;

                case PacketType.IncreaseEntityQuantity:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        var senderID = r.ReadInt32();
                        var tarentityID = r.ReadInt32(); //TargetArgs.Read(Instance, r).Object;
                        var tarentity = Instance.NetworkObjects[tarentityID];
                        var quantity = r.ReadInt32();
                        tarentity.TryGetComponent<StackableComponent>(c => c.SetStacksize(tarentity, tarentity.StackSize + quantity));
                        // send to all players or in area?
                        Instance.Enqueue(PacketType.IncreaseEntityQuantity, msg.Payload, SendType.OrderedReliable, tarentity.Global);
                    });
                    return;

                case PacketType.SpawnChildObject:
                    SpawnChild(msg);
                    return;

                case PacketType.SpawnEntity:
                    SpawnEntity(msg);
                    return;

                case PacketType.InstantiateAndSpawnObject:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        obj = GameObject.CreatePrefab(r);
                        var g = r.ReadVector3();
                        var v = r.ReadVector3();
                        //Instance.Spawn(obj, pos);
                        obj.Instantiate(Instance.Instantiator);
                        obj.Global = g;
                        obj.Velocity = v;
                        Instance.SyncInstantiate(obj);
                        Instance.SyncSpawn(obj);
                    });
                    return;

                case PacketType.InstantiateObject:
                    obj = Network.Deserialize<GameObject>(msg.Payload, GameObject.CreateCustomObject);
                    Instance.InstantiateObject(obj);
                    return;

                /// received input from player to directly destroy an object (for example in editing mode)
                /// 
                case PacketType.DisposeObject:

                    //if (Instance.NetworkObjects.TryRemove(Network.Deserialize<GameObject>(msg.Payload, GameObject.Create).NetworkID, out obj))
                    //    obj.Remove();

                    //Instance.DestroyObject(Network.Deserialize<GameObject>(msg.Payload, GameObject.Create).NetworkID);
                    TargetArgs tar = Network.Deserialize<TargetArgs>(msg.Payload, r => TargetArgs.Read(Instance, r));

                    // maybe add a playercomponent to player controller objects to check them faster
                    if ((from p in Instance.Players.GetList()
                         where p.CharacterID == tar.Object.Network.ID
                         select p).Count() > 0)
                        break;

                    Instance.SyncDisposeObject(tar.Object);
                    //foreach (var player in Players.GetList())
                    //    Enqueue(msg.Copy(msg.Player.PacketSequence, player, 5));

                    break;

                case PacketType.Ack:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        //WaitingForAck.Remove(r.ReadInt64());

                        long ackID = r.ReadInt64();
                        Packet existing;
                        if (msg.Player.WaitingForAck.TryRemove(ackID, out existing))
                        {
                            if (existing.PacketType == PacketType.Ping)
                                msg.Connection.Ping.Stop();
                            existing.RTT.Stop();
                            msg.Connection.RTT = TimeSpan.FromMilliseconds(existing.RTT.ElapsedMilliseconds);
                            msg.Player.Ping = TimeSpan.FromMilliseconds(existing.RTT.ElapsedMilliseconds).Milliseconds;
                            if (msg.Player.OrderedPackets.Count>0)
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
                            //player.Outgoing.
                            Instance.Enqueue(player, Packet.Create(msg.Player, PacketType.RequestNewObject, Network.Serialize(w =>
                            {
                                TargetArgs.Write(w, toDrag);
                                w.Write(amount);
                            }), SendType.Ordered | SendType.Reliable));
                    });
                    break;

                case PacketType.JobCreate:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        TownJob job = TownJob.Read(r, Instance);
                        Instance.Map.GetTown().Jobs.Add(job);
                        throw new NotImplementedException();
                        //Enqueue(Players.GetList(), msg.PacketType, Network.Serialize(job.Write));
                    });
                    break;

                case PacketType.JobDelete:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int jobID = r.ReadInt32();
                        Instance.Map.GetTown().Jobs.Remove(jobID);
                        throw new NotImplementedException();
                        //Enqueue(Players.GetList(), msg.PacketType, msg.Payload);
                    });
                    break;


                default:
                    //GameMode.Current.PacketHandler.Handle(Instance, msg);
                    GameMode.Current.HandlePacket(Instance, msg);

                    break;
            }

            // send world state changes periodically (delta states)
            //foreach (var p in Players)
            //    msg.BeginSendTo(p.Socket, p.IP);
        }

        //private static void PlayerCraftBench(int playerID, int benchID, Components.Crafting.Reaction.Product.ProductMaterialPair productMaterialPair)
        //{
        //    // check validity of action and begin crafting interaction for player
        //    var player = Instance.NetworkObjects[playerID];
        //    var bench = Instance.NetworkObjects[benchID];
        //    player.GetComponent<WorkComponent>().Perform(player, new Components.Interactions.CraftBench(productMaterialPair), new TargetArgs(bench));

        //    // sync action across clients
        //    Instance.Enqueue(PacketType.PlayerCraftBench,
        //        Network.Serialize(w =>
        //        {
        //            w.Write(playerID);
        //            w.Write(benchID);
        //            productMaterialPair.Write(w);
        //        }), SendType.OrderedReliable, player.Global);
        //}

        private static void SpawnEntity(Packet msg)
        {
            Network.Deserialize(msg.Payload, r =>
            {
                var entity = GameObject.CreatePrefab(r);
                var target = TargetArgs.Read(Instance, r);
                target.Map = Instance.Map;
                //Instance.SyncInstantiate(entity);
                switch(target.Type)
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

        private static void SpawnChild(Packet msg)
        {
            Network.Deserialize(msg.Payload, r =>
            {
                GameObject obj = GameObject.CreatePrefab(r);
                int parentID = r.ReadInt32();
                GameObject parent;
                if (!Instance.TryGetNetworkObject(parentID, out parent))
                    throw (new Exception("Parent doesn't exist"));
                obj.Parent = parent;
                int childIndex = r.ReadInt32();
                var slot = parent.GetChildren()[childIndex];
                slot.Object = obj;

                Instance.SyncInstantiate(obj);
                byte[] newPayload = Network.Serialize(w =>
                {
                    obj.Write(w);
                    w.Write(parentID);
                    w.Write(childIndex);
                });

                Instance.Enqueue(PacketType.SpawnChildObject, newPayload, SendType.OrderedReliable, parent.Global);
            });
        }
        public static Vector3 FindValidSpawnPosition(GameObject obj)
        {
            var xy = Vector3.Zero + (Vector3.UnitX + Vector3.UnitY) * (GameModes.StaticMaps.StaticMap.MapSize.Default.Blocks / 2);
            int z = global::Start_a_Town_.Map.MaxHeight - (int)Math.Ceiling(obj.GetPhysics().Height);
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
            //var spawnPosition = FindValidSpawnPosition(obj);
            obj.Global = spawnPosition;

            // clients instantiate player entity by themselves by handling PlayerEnterWorld packet
            /*
            //instantiate character across network
            Instance.SyncInstantiate(obj);
            //spawn character actor network
            Instance.SyncSpawn(obj);
            */

            Instance.Spawn(obj);
            // send a message to the newly connected client to own their character
            byte[] entityData = Network.Serialize(obj.Write);
            //Enqueue(msg.Player, PacketMessage.Create(msg.Player, PacketType.AssignCharacter, entityData, SendType.Ordered | SendType.Reliable));//.Send(msg.Sender);

            byte[] playerData = Network.Serialize(msg.Player.Write);

            //foreach (var p in Instance.Players.GetList().Where(p => p != msg.Player))
            byte[] data = Network.Serialize(w =>
            {
                msg.Player.Write(w);
                obj.Write(w);
            });
            //signal all players to spawn player entity
            foreach (var p in Instance.Players.GetList())
                Instance.Enqueue(p, Packet.Create(p, PacketType.PlayerEnterWorld, data, SendType.Ordered | SendType.Reliable));
            // signal client to own their character
            Instance.Enqueue(msg.Player, Packet.Create(msg.Player, PacketType.AssignCharacter, Network.Serialize(w => w.Write(obj.Network.ID)), SendType.Ordered | SendType.Reliable));//.Send(msg.Sender);
        }

        

        

        //GameObject SyncCreate(GameObject.Types type)
        //{
        //    GameObject obj = GameObject.Create(type);
        //    this.Sync(this.Instantiate(obj));
        //    return obj;
        //}

        /// <summary>
        /// Creates the same item across the network (but doesn't spawn it in the game world)
        /// </summary>
        /// <param name="obj"></param>
        public byte[] SyncInstantiate(GameObject obj)
        {
            //if (obj.NetworkID > 0)
            //    return new byte[] { };

            if (obj.Network.ID == 0)
                Instantiate(obj);

            byte[] newData = Network.Serialize(w =>
            {
                obj.Write(w);

                // pos.Write(w);
            });
            //foreach (var player in Players.GetList())
            //    Enqueue(Packet.Create(player.PacketSequence, PacketType.InstantiateAndSpawnObject, newData, player, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
            // DONT SPAWN IT
            //foreach (var player in Players.GetList())
            //    Enqueue(Packet.Create(player.PacketSequence, PacketType.InstantiateObject, newData, player, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
            Sync(newData);
            return newData;
        }
        void Sync(byte[] data)
        {
            foreach (var player in Players.GetList())
                //player.Outgoing.
                    Enqueue(player, Packet.Create(player, PacketType.InstantiateObject, data, SendType.Ordered | SendType.Reliable));
        }

        public void SyncSpawn(GameObject obj)
        {
            obj.Spawn(this);
            byte[] newData = Network.Serialize(w =>
            {
                //obj.Write(w);
                w.Write(obj.Network.ID);
                w.Write(obj.Global);
                w.Write(obj.Velocity);
            });
            //SyncSpawn(newData);
            foreach (var player in Players.GetList())
                //player.Outgoing.Enqueue(PacketMessage.Create(player.PacketSequence, PacketType.SpawnObject, newData, player, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
                Enqueue(player, Packet.Create(player, PacketType.SpawnObject, newData, SendType.Ordered | SendType.Reliable));
            
            //return newData;
        }
        public void SyncSpawn(GameObject obj, GameObjectSlot slot)
        {
            byte[] newData = Network.Serialize(w =>
            {
                w.Write(obj.Network.ID);
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
                w.Write(parent.Network.ID);
                w.Write(childIndex);
            });
            foreach (var player in Players.GetList())
                //player.Outgoing.Enqueue(PacketMessage.Create(player.PacketSequence, PacketType.SpawnChildObject, data, player, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
                Enqueue(player, Packet.Create(player, PacketType.SpawnChildObject, data, SendType.Ordered | SendType.Reliable));
        }
        private static void SyncTime()
        {
            Instance.Players.GetList().ForEach(player =>
            {
                // BUILD THE PACKET ON SEND
                //Outgoing.Enqueue(Packet.Create(PacketID, PacketType.SyncTime, Network.Serialize(w => w.Write(ServerClock.TotalMilliseconds)), player));
                Packet.Create(player, PacketType.SyncTime, Network.Serialize(w => w.Write(ServerClock.TotalMilliseconds))).BeginSendTo(Listener, player.IP);
            });
        }
        private static void KickPlayer(int plid)
        {
            CloseConnection(Instance.Players.GetList().First(p => p.ID == plid).Connection);
        }
        //private static void KickPlayer(string plname)
        //{
        //    CloseConnection(Players.GetList().First(p => p.Name == plname).Connection);
        //}
        static void CloseConnection(UdpConnection connection)
        {
            //connection.Ping.Change(Timeout.Infinite, Timeout.Infinite);
            connection.Ping.Stop();
            //connection.Socket.Close();
            UdpConnection existing;
            if (!Connections.TryRemove(connection.IP, out existing))
            {
                throw new Exception("Tried to close nonexistent connection");
                "Tried to close nonexistent connection".ToConsole();
                return;
            }
            Instance.Players.Remove(existing.Player);
            if (existing.Player.IsActive)
                Instance.Despawn(existing.Player.Character);
            Instance.DisposeObject(existing.Player.CharacterID);
            Instance.Players.GetList().ForEach(p =>
            {
                if (p.IsActive)
                    //Enqueue(p, PacketMessage.Create(p, PacketType.PlayerDisconnected, Network.Serialize(existing.Player.Write), SendType.OrderedReliable));
                    Instance.Enqueue(p, Packet.Create(p, PacketType.PlayerDisconnected, Network.Serialize(w => w.Write(existing.Player.ID)), SendType.OrderedReliable));
            });
        }

        public GameObject InstantiateAndSync(GameObject obj)
        {
            this.SyncInstantiate(obj);
            return obj;

            obj.Instantiate(this.Instantiator);

            byte[] newData = Network.Serialize(w =>
            {
                obj.Write(w);

                // pos.Write(w);
            });
            foreach (var player in Players.GetList())
                //player.Outgoing.Enqueue(PacketMessage.Create(player.PacketSequence, PacketType.InstantiateObject, newData, player, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
            Enqueue(player, Packet.Create(player, PacketType.InstantiateObject, newData, SendType.Ordered | SendType.Reliable));

            return obj;
        }

        public void SyncEvent(GameObject recipient, Components.Message.Types msg, Action<BinaryWriter> writer)
        {
            byte[] data;
            using (BinaryWriter w = new BinaryWriter(new MemoryStream()))
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
            //Instance.PostLocalEvent(recipient, ObjectEventArgs.Create(msg, writer));
            recipient.HandleRemoteCall(ObjectEventArgs.Create(msg, writer));
            //Enqueue(PacketMessage.Create(player.PacketSequence, PacketType.ObjectEvent, payload, player, SendType.Ordered | SendType.Reliable));
            Enqueue(PacketType.ObjectEvent, payload, SendType.Ordered | SendType.Reliable);
            //foreach (var player in Players.GetList())
            //{
            //    //player.Outgoing.
            //        Enqueue(player, PacketMessage.Create(player.PacketSequence, PacketType.ObjectEvent, payload, player , SendType.Ordered | SendType.Reliable));
            //}
        }

        public GameObject Instantiate(GameObject obj)
        {
            obj.Instantiate(Instantiator);
            //Instantiator(obj);
            return obj;
        }
        public void Instantiator(GameObject obj)
        {
            obj.Net = this;
            Instance.NetworkObjects.AddOrUpdate(ObjID, id =>
            {
                if (Instance.NetworkObjects.Values.Contains(obj))
                    //("Duplicate object " + obj.Name).ToConsole();
                    throw new Exception("Duplicate object");
                obj.Network.ID = id;//["Network"]["ID"] = id;
                return obj;
            }, (id, o) =>
            {
                throw new Exception("Duplicate net ID");
            });
        }

        /// <summary>
        /// Instantiates an object on the server and syncs it across the clients
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public GameObject InstantiateObject(GameObject obj)
        {
            // InstantiateAndSpawn(obj);
            if (obj.Network.ID > 0)
                throw new Exception("object already has a network id");
            //throw new Exception("tried to network instantiate already instantiated object");

            //obj.Instantiate(Instance);
            obj.Instantiate(Instantiator);
            byte[] data = Network.Serialize(obj.Write);
            foreach (var player in Players.GetList())
            {
                //player.Outgoing.
                    Enqueue(player, Packet.Create(player, PacketType.InstantiateObject, data, SendType.Ordered | SendType.Reliable));
            }
            return obj;
            //   Players.GetList().ForEach(p => Packet.Create(p.PacketSequence, PacketType.InstantiateObject, Network.Serialize(obj.Write)).BeginSendTo(Listener, p.IP));
        }

        public void InstantiateInContainer(GameObject obj, GameObject parent, byte containerID, byte slotID, byte amount)
        {
            //instantiate the new object and sync it across clients
            Instance.InstantiateObject(obj);
            PostLocalEvent(parent, ObjectEventArgs.Create(Components.Message.Types.AddItem, new object[] { obj, containerID, slotID, amount }));
            byte[] data = Network.Serialize(w =>
            {
                ObjectEventArgs.Write(w, Components.Message.Types.ContainerOperation, ww =>
                Components.ArrangeInventoryEventArgs.Write(ww, new TargetArgs(parent), TargetArgs.Empty, new TargetArgs(obj), containerID, slotID, amount));
            });
            foreach (var player in Players.GetList())
                //player.Outgoing.Enqueue(PacketMessage.Create(player.PacketSequence, PacketType.ObjectEvent, data, player, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
            Enqueue(player, Packet.Create(player, PacketType.ObjectEvent, data, SendType.Ordered | SendType.Reliable));
        }
        public void InstantiateInContainer2(GameObject obj, GameObject container, byte containerID, byte slotID, byte amount)
        {
            obj.Instantiate(Instance.Instantiator);
            PostLocalEvent(container, ObjectEventArgs.Create(Components.Message.Types.AddItem, new object[] { obj, containerID, slotID, amount }));
            byte[] data = Network.Serialize(w =>
            {
                //TargetArgs.Write(w, obj);
                obj.Write(w);
                TargetArgs.Write(w, container);
                w.Write(containerID);
                w.Write(slotID);
                w.Write(amount);
            });
            foreach (var player in Players.GetList())
                //player.Outgoing.Enqueue(PacketMessage.Create(player.PacketSequence, PacketType.InstantiateInContainer, data, player, PacketMessage.MaxAttempts , SendType.Ordered | SendType.Reliable));
                Enqueue(player, Packet.Create(player, PacketType.InstantiateInContainer, data, SendType.Ordered | SendType.Reliable));
        }

        private static void InstantiateAndSpawn(GameObject obj, WorldPosition pos)
        {
            // let object instantiate itself in case of block
            obj.Instantiate(Instance.Instantiator);
            Instance.Spawn(obj, pos);
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
            obj.Net = this;
            this.SpawnObject(obj);
        }
        //public void Spawn(GameObject obj)
        //{
        //    this.Spawn(obj, obj.Global);
        //}
        public void Spawn(GameObject obj, Vector3 global)
        {
            obj.Parent = null;
            obj.Net = this;
            obj.Global = global;
            //this.Spawn(obj, new Position(this.Map, global));
            this.SpawnObject(obj);
        }
        public void Spawn(GameObject obj, WorldPosition pos)
        {
            obj.Parent = null;
            obj.Net = this;
            this.SpawnObject(obj);
        }
        public void Spawn(GameObject obj, GameObject parent, int childID)
        {
            //var slot = parent.GetChildren()[childID];
            //if(slot.HasValue)
            //    if (slot.Object.ID == obj.ID)
            //    {
            //        slot.Insert(obj.ToSlot());
            //        return;
            //    }
            if (obj.Network.ID == 0)
            {
                //obj.Instantiate(Instantiator);
                this.SyncInstantiate(obj);
                SyncChild(obj, parent, childID);
            }
            obj.Parent = parent;
            var slot = parent.GetChildren()[childID];
            slot.Object = obj;
        }
        public void Spawn(GameObject obj, GameObjectSlot slot)
        {
            if (obj.Network.ID == 0)
            {
                obj.Instantiate(Instantiator);
                SyncInstantiate(obj);
                //SyncSpawn(obj, slot);
                
                slot.Object = obj;
                SyncSlots(slot);
                return;
            }
            slot.Object = obj;
        }
        public void SyncSlotInsert(GameObjectSlot slot, GameObject obj) 
        {
            if (!slot.Insert(obj.ToSlot()))
                return;

            //sync slot
            var parent = slot.Object.Parent;
            var slotid = slot.ID;
            byte[] data = Network.Serialize(w =>
            {
                w.Write(parent.Network.ID);
                w.Write(slotid);
                slot.Write(w);
            });
            Enqueue(PacketType.SyncSlot, data, SendType.OrderedReliable);
        }

        void SpawnObject(GameObject obj)
        {
            if (obj.Network.ID == 0)
            {
                obj.Instantiate(Instantiator);
                SyncInstantiate(obj);
                SyncSpawn(obj);
                return;
            }
            //obj.Parent = null;
            //obj.Net = this;
            obj.Spawn(Instance);
        }

        public void SyncDespawn(GameObject entity)
        {
            this.Despawn(entity);
            Enqueue(PacketType.DespawnObject, Network.Serialize(w => w.Write(entity.Network.ID)), SendType.OrderedReliable);
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
            GameObject o;
            //if (NetworkObjects.TryRemove(objNetID, out o)) // WARNING! BLOCK OBJECTS DONT HAVE A NETID
              //  Despawn(o);
            //return NetworkObjects.TryRemove(obj.NetworkID, out o);
            return DisposeObject(obj.Network.ID);
        }
        /// <summary>
        /// Releases the object's networkID.
        /// </summary>
        /// <param name="objNetID"></param>
        public bool DisposeObject(int netID)
        {
            GameObject o;
            if (!NetworkObjects.TryRemove(netID, out o))
                return false;
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
            GameObject o;
            //if (NetworkObjects.TryRemove(obj.NetworkID, out o))
            //    Despawn(o);
            //else
            //    throw new Exception("Object mismatch!");

            //NetworkObjects.TryRemove(obj.NetworkID, out o);
            this.DisposeObject(obj);

            byte[] data = Network.Serialize(w => TargetArgs.Write(w, obj));
            foreach (var player in Players.GetList())
                //player.Outgoing.
                    Enqueue(player, Packet.Create(player, PacketType.DisposeObject, data));

            Despawn(obj);
        }
        public void SyncDisposeObject(GameObjectSlot slot)
        {
            GameObject o;

            // if the object is in a temp slot, call syncdisposeobject(gameobject) instead
            if (slot.Object.Parent == null)
            {
                this.SyncDisposeObject(slot.Object);
                return;
            }

            NetworkObjects.TryRemove(slot.Object.Network.ID, out o);

            byte[] data = Network.Serialize(w => TargetArgs.Write(w, slot));
            foreach (var player in Players.GetList())
                Enqueue(player, Packet.Create(player, PacketType.DisposeObject, data));

            slot.Clear();
        }

        static void PlayerNameChange(PlayerData player, string name)
        {
            player.Name = name;
            OnPlayersChanged(null, EventArgs.Empty);
        }

        //static void SendPlayerList(Socket client) //PlayerData player)//
        //{
        //    //Packet.Create(PacketType.PlayerList, Network.Serialize(Players.Write)).Send(client);//new ObjectState("Server", client));
        //    byte[] data = Packet.Create(PacketID, PacketType.PlayerList, Network.Serialize(Players.Write)).ToArray();

        //    //client.BeginSend(data, 0, data.Length, SocketFlags.None, a =>
        //    //{
        //    //    "sent".ToConsole();
        //    //    //SendPlayerList(client);
        //    //}, client);

        //    client.Send(data);
        //}
        static void SendPlayerList()//
        {
            foreach (var pl in Instance.Players.GetList())
                //pl.Outgoing.
                Instance.Enqueue(pl, Packet.Create(pl, PacketType.PlayerList, Network.Serialize(Instance.Players.Write), SendType.Reliable));
        }
        static void SendPlayerList(PlayerData player)//
        {
            //player.Outgoing.
            Instance.Enqueue(player, Packet.Create(player, PacketType.PlayerList, Network.Serialize(Instance.Players.Write)));
        }
        static public void LoadMap(IMap map)
        {
            if (map == null)
                return;
            World = map.GetWorld();
            Instance.Map = map;
            foreach (var ch in map.GetActiveChunks().Values)
                InstantiateChunk(ch);
            Instance.SetMap(map);
            //Instance.Map.Network = "Server";
            //Instance.Map.Net = Instance;
            map.SetNetwork(Instance);
            if (Instance.LightingEngine != null)
                Instance.LightingEngine.Stop();
            // TODO: make lightingengine's blockcallback return both chunk and cell so i don't have to re fetch them
            Instance.LightingEngine = LightingEngine.StartNew(Instance, Instance.SyncLight, Instance.SyncInvalidateCells);// new LightingEngine(this);

            //Instance.LightingEngine = new LightingEngine(map);// new LightingEngine(this);
             
            // why did i have to create a new dictionary here?
            //map.ActiveChunks = new System.Collections.Concurrent.ConcurrentDictionary<Vector2, Chunk>();
            //map.ActiveChunks = new Dictionary<Vector2, Chunk>();

            //Random = new RandomThreaded(Instance.Map.World);
            Random = new RandomThreaded(Instance.Map.GetWorld().GetRandom());
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="player">Sends to specific player, or to all players if null.</param>
        public void SendMapInfo(PlayerData player)
        {
            byte[] data = Network.Serialize(Instance.Map.GetData); // why does it let me do that?

         //   byte[] msg = Packet.ToArray(PacketType.MapData, mapData);
            if (player.IsNull())
                //Players.ForEach(p => p.Socket.BeginSend(msg, 0, msg.Length, SocketFlags.None, a => ("map data sent " + msg.Length + " (bytes)").ToConsole(), p.Socket));
                Instance.Players.GetList().ForEach(p => Instance.Enqueue(p, Packet.Create(player, PacketType.MapData, data, SendType.Ordered | SendType.Reliable)));
            else
                //sender.BeginSend(msg, 0, msg.Length, SocketFlags.None, a => ("map data sent " + msg.Length + " (bytes)").ToConsole(), sender);
                //player.Outgoing.
                Instance.Enqueue(player, Packet.Create(player, PacketType.MapData, data, SendType.Ordered | SendType.Reliable));
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="player">Sends to specific player, or to all players if null.</param>
        public void SendWorldInfo(PlayerData player)
        {
            byte[] data = Network.Serialize(Instance.Map.GetWorld().WriteData);

            if (player.IsNull())
                Instance.Players.GetList().ForEach(p => Instance.Enqueue(p, Packet.Create(player, PacketType.WorldInfo, data, SendType.Ordered | SendType.Reliable)));
            else
                //player.Outgoing.
                Instance.Enqueue(player, Packet.Create(player, PacketType.WorldInfo, data, SendType.Ordered | SendType.Reliable));
        }

        //static void HandleChunkRequest(Socket socket, Vector2 vec2)
        static void HandleChunkRequest(PlayerData player, Vector2 vec2)
        {
            Chunk chunk;
            if (Instance.Map.GetActiveChunks().TryGetValue(vec2, out chunk))
            {
                byte[] data = Network.Serialize(chunk.Write);
                //player.Outgoing.
                Instance.Enqueue(player, Packet.Create(player, PacketType.Chunk, data, SendType.Reliable));
            }
            else
            {
                //ChunkRequests.AddOrUpdate(vec2, new ConcurrentQueue<PlayerData>(new PlayerData[] { player }), (v, list) => { list.Enqueue(player); return list; });
                ChunksToLoad.Add(vec2);
            }
        }
        static BlockingCollection<Vector2> ChunksToLoad = new BlockingCollection<Vector2>(new ConcurrentQueue<Vector2>());
        //static void HandleChunkRequests()
        //{
        //    try
        //    {
        //        //Vector2 chunkPos = await ChunkRequestsNew.Take();
        //        foreach (var chunkPos in ChunksToLoad.GetConsumingEnumerable(ChunkLoaderToken.Token))
        //        {
        //            //if (!ChunkRequestsNew.TryDequeue(out chunkPos))
        //            //    throw new Exception("Error loading chunk");
        //            LoadChunk(chunkPos);
        //        }
        //    }
        //    catch (OperationCanceledException) { }
        //}
        public void UnloadChunk(Vector2 chunkPos)
        {
            Chunk chunk;
            var activeChunks = Instance.Map.GetActiveChunks();

            if (!activeChunks.TryGetValue(chunkPos, out chunk))
                return;

            // check distance of chunk from every player
            //if (Players.GetList().Any(pl => Vector2.Distance(chunkPos, pl.Character.Global.GetChunk(this.Map).MapCoords) <= Engine.ChunkRadius))
            //    return;

            foreach (var obj in chunk.GetObjects())
                //Sync // don't sync dispose cause client will dispose object when it receives the unload chunk packet
                    this.DisposeObject(obj);

            //Instance.Map.ActiveChunks.TryRemove(chunkPos, out chunk);
            chunk = activeChunks[chunkPos];
            activeChunks.Remove(chunkPos);

            if (!chunk.Saved)
                this.ChunkLoader.ScheduleForSaving(chunk);
            chunk.SaveThumbnails();
            //Instance.Enqueue(PacketType.UnloadChunk, Network.Serialize(w => w.Write(chunkPos)), SendType.OrderedReliable);
        }

        ChunkLoader ChunkLoader;// = new ChunkLoader(this.Map);
        //void HandleCachedChunk(Chunk chunk)
        //{
        //    AddChunk(chunk);
        //    SendChunk(chunk);
        //}
        //void HandleSavedChunk(Chunk chunk)
        //{
        //    InstantiateChunk(chunk);
        //    AddChunk(chunk);
        //    SendChunk(chunk);
        //}
        //void HandleNewChunk(Chunk chunk)
        //{
        //    InstantiateChunk(chunk);
        //    AddChunk(chunk);
        //    chunk.UpdateEdges(Instance.Map);
        //    SendChunk(chunk);

        //    Chunk neighbor;
        //    var neighs = chunk.MapCoords.GetNeighbors();
        //    foreach (var vector in neighs)
        //    {
        //        if (Instance.Map.ActiveChunks.TryGetValue(vector, out neighbor))
        //            neighbor.UpdateEdges(Instance.Map);//, v => Instance.SyncCell(v));
        //    }
        //    byte[] updateneighborsdata = Network.Serialize(w => w.Write(chunk.MapCoords));
        //    Instance.Enqueue(PacketType.UpdateChunkNeighbors, updateneighborsdata, SendType.Reliable | SendType.Ordered);

        //    //update light at neighbor chunk edges
        //    UpdateChunkNeighborsLight(chunk);

        //    if (!chunk.LightValid)
        //        ResetLight(chunk, () =>
        //        {
        //            chunk.LightValid = true;
        //        });
        //}
        private static void LoadChunk(Vector2 chunkPos, Action<Chunk> callback)
        {
            Chunk existing;
            if (Instance.Map.GetActiveChunks().TryGetValue(chunkPos, out existing))
            {
                callback(existing);
                return;
            }
            Instance.ChunkLoader.TryEnqueue(chunkPos,
                (c) =>
                {
                    //Instance.ReadyChunks.Enqueue(new ChunkRequest(c, callback));
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
            //Instance.ChunkLoader.Output.Enqueue(ch);
            InstantiateChunk(ch);
            //AddChunk(ch);
            Instance.ChunksToActivate.Enqueue(ch);
            // update neighbor chunks here? the faces of outer blocks
            return;
            foreach (var vector in ch.MapCoords.GetNeighbors())
            {
                Chunk neighbor;
                if (actives.TryGetValue(vector, out neighbor))
                {
                    neighbor.ResetVisibleOuterBlocks();
                    neighbor.LightCache2.Clear();
                }
            }
            return;
            if (!ch.LightValid)
            {
                ResetLight(ch, () =>
                {
                    ch.LightValid = true;
                    // only update neighbor lights if we updated this chunk's light
                    UpdateChunkNeighborsLight(ch);
                });
            }
            foreach (var vector in ch.MapCoords.GetNeighbors())
            {
                Chunk neighbor;
                if (actives.TryGetValue(vector, out neighbor))
                    neighbor.InvalidateEdges();
            }

        }
        public void SendChunks(PlayerData player, params Vector2[] chunks)
        {
            this.SendChunks(chunks.ToList(), player);
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
                //Task.Factory.StartNew(() => SendChunk(player, item));

                var toLoad = chunksToLoad.Except(existingPositions);
                foreach (var item in toLoad)
                    LoadChunk(item, c => SendChunk(player, c));
            });
        }

        private static void UpdateChunkNeighborsLight(Chunk chunk)
        {
            var actives = Instance.Map.GetActiveChunks();
            Chunk neighbor;
            if (actives.TryGetValue(chunk.MapCoords + new Vector2(1, 0), out neighbor))
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
            //Enqueue(player, Packet.Create(player, PacketType.Chunk, data, SendType.Reliable)); // we don't need to send chunks in order, so just send them reliable
            Enqueue(player, Packet.Create(player, PacketType.Chunk, data, SendType.OrderedReliable)); // but send them ordered as well as a workaround for sending so fast that the client input buffer size gets clogged and packets are missed

            //data.Length.ToConsole();
        }
        private static void AddChunk(Chunk chunk)
        {
            //Instance.Map.ActiveChunks.TryAdd(chunk.MapCoords, chunk);
            var actives = Instance.Map.GetActiveChunks();

            actives.Add(chunk.MapCoords, chunk);


            //Instance.Map.ActiveChunks.AddOrUpdate(chunk.MapCoords, chunk, (pos, existing) => {
            //    throw new Exception("Chunk already loaded");
            //    return existing;
            //});// existing);
        }
        private static void InstantiateChunk(Chunk chunk)
        {
            chunk.GetObjects().ForEach(obj =>
            {
                obj.Instantiate(Instance.Instantiator);
                obj.Net = Instance;
                obj.Transform.Exists = true;
            });
            //chunk.GetBlockObjects().ForEach(obj => obj.Instantiate(Instance.Instantiator)); //obj.Instantiate(Instance)
            foreach(var blockentity in chunk.BlockEntities)
                blockentity.Value.Instantiate(Instance.Instantiator);
        }


        private static void ResetLight(Chunk chunk)
        {
            Queue<Vector3> cellList = chunk.ResetHeightMap();
            //chunk.LightCache = new ConcurrentDictionary<Vector3, Color>();
            //chunk.Sunlight = new List<byte>();
            Instance.LightingEngine.Enqueue(cellList);
        }
        private static void ResetLight(Chunk chunk, Action callback)
        {
            Queue<Vector3> cellList = chunk.ResetHeightMap();
            //chunk.MapCoords.ToConsole();
            Instance.LightingEngine.Enqueue(cellList, callback);
        }
        //private static void ResetLight(Chunk chunk, Action<IEnumerable<Vector3>> callback)
        //{
        //    Queue<Vector3> cellList = chunk.ResetHeightMap();
        //    Instance.LightingEngine.Enqueue(cellList, callback);
        //}
        public GameObject GetNetworkObject(int netID)
        {
            GameObject obj;
            NetworkObjects.TryGetValue(netID, out obj);
            return obj;
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
                Server.Console.Write("SERVER", "Saving is disabled for this map");                
                return;
            }
            //    Instance.Map.SaveServer();
                foreach (var chunk in Instance.Map.GetActiveChunks().Values)
            {
                if(chunk.Saved)
                //if(chunk.MarkedForSave)
                //if(//chunk.Saved && 
                //    chunk.MarkedForSave)
                    continue;

                Instance.ChunkLoader.ScheduleForSaving(chunk);
                //chunk.MarkedForSave = true;
            }
        }

        /// <summary>
        /// When this is called, any object state changes occured, are written 
        /// </summary>
        void BuildSnapshot(TimeSpan gt, BinaryWriter writer)
        {
            writer.Write(gt.TotalMilliseconds);
            writer.Write(this.ObjectsChangedSinceLastSnapshot.Count);
            var objectsChanged = this.ObjectsChangedSinceLastSnapshot;
            //this.ObjectsChangedSinceLastSnapshot = new HashSet<int>();
            //foreach (var netID in objectsChanged)
            //{
            //    GameObject obj;
            //    writer.Write(netID);
            //    if (Instance.TryGetNetworkObject(netID, out obj))
            //        ObjectSnapshot.Write(obj, writer);
            //}

            this.ObjectsChangedSinceLastSnapshot = new HashSet<GameObject>();
            foreach (var obj in objectsChanged)
            {
                writer.Write(obj.Network.ID);
                ObjectSnapshot.Write(obj, writer);
            }

            //this.ObjectsChangedSinceLastSnapshot = new ConcurrentDictionary<int, GameObject>();
            //foreach (var item in objectsChanged)
            //{
            //    GameObject obj;
            //    writer.Write(item.Key);
            //    ObjectSnapshot.Write(item.Value, writer);
            //}
        }

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
            //this.ObjectsChangedSinceLastSnapshot = new HashSet<int>();
            writer.Write(inRange.Count);
            foreach (var obj in inRange)
            {
                writer.Write(obj.Network.ID);
                ObjectSnapshot.Write(obj, writer);
            }

            //writer.Write(gt.TotalMilliseconds);
            //var inRange = (from id in objects// this.ObjectsChangedSinceLastSnapshot 
            //               let obj = this.NetworkObjects[id] 
            //               where player.IsWithin(obj.Global) 
            //               //where player.IsWithin(Vector3.Zero) 
            //               select id)
            //               .ToList();
            ////this.ObjectsChangedSinceLastSnapshot = new HashSet<int>();
            //writer.Write(inRange.Count);
            //foreach (var netID in inRange)
            //{
            //    GameObject obj;
            //    writer.Write(netID);
            //    if (Instance.TryGetNetworkObject(netID, out obj))
            //        ObjectSnapshot.Write(obj, writer);
            //}
        }

        private static void SendSnapshots(TimeSpan gt)
        {
            // WARNING! BUILD SNAPSHOT ONLY ONCE BECAUSE THE STATE CHANGE LISTS ARE RESET (and for performance reasons, no need to build it multiple times)
            //if (Instance.ObjectsChangedSinceLastSnapshot.Count > 0)// || Instance.EventsSinceLastSnapshot.Count > 0)
            //{
            //    byte[] data = Network.Serialize(w => Instance.BuildSnapshot(gt, w));
            //    Instance.Players.GetList().ForEach(player =>
            //        {
            //            if (player.IsActive)
            //                Enqueue(player, PacketMessage.Create(player.PacketSequence, PacketType.Snapshot, data, player));
            //        });
            //}

            //if (Instance.ObjectsChangedSinceLastSnapshot.Count > 0)// || Instance.EventsSinceLastSnapshot.Count > 0)
            //{
            //    var objects = Instance.ObjectsChangedSinceLastSnapshot.Values.ToList();
            //    foreach (var player in Instance.GetPlayers())
            //    {
            //        byte[] data = Network.Serialize(w => Instance.BuildSnapshot(gt, player, objects, w));
            //        if (player.IsActive)
            //            Enqueue(player, PacketMessage.Create(player.PacketSequence, PacketType.Snapshot, data, player));
            //    }
            //    Instance.ObjectsChangedSinceLastSnapshot.Clear();// = new HashSet<int>();
            //}

            if (Instance.ObjectsChangedSinceLastSnapshot.Count > 0)// || Instance.EventsSinceLastSnapshot.Count > 0)
            {
                //var objects = Instance.ObjectsChangedSinceLastSnapshot.ToList();
                foreach (var player in Instance.GetPlayers())
                {
                    if (!player.IsActive)
                        continue;
                    byte[] data = Network.Serialize(w => Instance.BuildSnapshot(gt, player, Instance.ObjectsChangedSinceLastSnapshot, w));

                    // send snapshots immediately or enqueue them?
                    Packet.Create(player, PacketType.Snapshot, data).BeginSendTo(Listener, player.IP);
                    //Instance.Enqueue(player, Packet.Create(player, PacketType.Snapshot, data));
                }
                Instance.ObjectsChangedSinceLastSnapshot.Clear();// = new HashSet<int>();
            }
        }

        // TODO make the values a list in case i want to sync more components instead of position
        //public Dictionary<int, Action<BinaryWriter>> DeltaStates = new Dictionary<int, Action<BinaryWriter>>();

        public bool LogStateChange(int netID)
        {
            //return this.ObjectsChangedSinceLastSnapshot.TryAdd(netID, NetworkObjects[netID]);
            return this.ObjectsChangedSinceLastSnapshot.Add(NetworkObjects[netID]);
            //return ObjectsChangedSinceLastSnapshot.Add(netID);
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

            Vector3 direction = new Vector3(x, y, z);
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
        public void PopLoot(Components.LootTable table, Vector3 startPosition, Vector3 startVelocity)
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

            Vector3 direction = new Vector3(x, y, z);
            Vector3 final = startVelocity + force * direction;

            obj.Global = startPosition;
            obj.Velocity = final;

            if (obj.Network.ID == 0)
            //{
            //    obj.Instantiate(this.Instantiator);
                this.SyncInstantiate(obj);
            //}
            this.SyncSpawn(obj);

        }
        public List<GameObject> GenerateLoot(Components.LootTable lootTable)
        {
            List<GameObject> loot = new List<GameObject>();
            foreach (var l in lootTable)
                for (int i = 0; i < l.Generate(Random); i++)
                {
                    var obj = l.Factory();
                    var stacksize = Random.Next(l.StackMin, l.StackMax);
                    obj.StackSize = stacksize;
                    loot.Add(obj);
                    //loot.Add(GameObject.Create(l.ObjID));
                }
            return loot;
        }
        public List<GameObject> GenerateLootOld(Components.LootTable lootTable)
        {
            List<GameObject> loot = new List<GameObject>();
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

        //public static void SetWorld(string worldName)
        //{
        // //   if (Connections.Count > 0)
        //    if(!UnloadWorld())
        //    {
        //      //  Server.Console.Write(Color.Red, "SERVER", "Can't load new world while active connections exist");
        //        return;
        //    }
        //    Server.Console.Write(Color.Yellow, "SERVER", "Loading world " + worldName);
        //    World = World.Load(worldName);
        //    if (World.IsNull())
        //        Server.Console.Write(Color.Red, "SERVER", "Error loading world " + worldName);
        //    else
        //    {
        //        Server.Console.Write(Color.Lime, "SERVER", "World " + World.GetName() + " loaded");
        //        //LoadMap(World.Maps[Vector2.Zero]);
        //        LoadMap(World.GetMap(Vector2.Zero));
        //        Server.Console.Write(Color.Lime, "SERVER", "Map " + Instance.Map.GetOffset() + " loaded");
        //    }
        //}

        public static void SetWorld(IWorld world)
        {
            if (!UnloadWorld())
            {
                //  Server.Console.Write(Color.Red, "SERVER", "Can't load new world while active connections exist");
                return;
            }
            World = world;
            if (!World.IsNull())
            {
                //Server.Console.Write(Color.Yellow, "SERVER", "Loading world " + World.Name);
                Server.Console.Write(Color.Lime, "SERVER", "World " + World.GetName() + " loaded");
                //LoadMap(World.Maps[Vector2.Zero]);
                var map = World.GetMap(Vector2.Zero);
                if (map != null)
                {
                    //LoadMap(map);
                    //Server.Console.Write(Color.Lime, "SERVER", "Map " + Instance.Map.GetOffset() + " loaded");
                }
            }
        }
        private static bool UnloadWorld()
        {
            if (Connections.Count > 0)
            {
                Server.Console.Write(Color.Red, "SERVER", "Can't unload world while active connections exist");
                return false;
            }
            if (!World.IsNull())
            {
                Server.Console.Write(Color.Lime, "SERVER", "World " + World.GetName() + " unloaded");
                //World.Dispose();
            }
            World = null;
            return true;
        }

        ServerCommandParser Parser;// = new ServerCommandParser(this);
        static public void Command(string command)
        {
            if (Instance.Parser == null)
                Instance.Parser = new ServerCommandParser(Instance);
            //ServerCommandParser.Command(command);
            Instance.Parser.Command(command);
        }
        //static public void Command(string command)
        //{
        //    Queue<string> queue = new Queue<string>(command.Split(' '));
        //    try
        //    {
        //        switch (queue.Dequeue())
        //        {
        //            case "hello":
        //                Server.Console.Write("SERVER", "how are you?");
        //                break;

        //            case "loadworld":
        //                string worldName = queue.Dequeue();
        //                SetWorld(worldName);
        //                //Server.Console.Write(Color.Red, "SERVER", "World " + worldName + " doesn't exist");
        //                break;

        //            case "unloadworld":
        //                UnloadWorld();
        //                break;

        //            case "broadcast":
        //                string message = queue.Dequeue();
        //                byte[] data = Network.Serialize(w => w.WriteASCII(message));
        //                foreach (var p in Players.GetList())
        //                    //p.Outgoing.
        //                    Enqueue(p, PacketMessage.Create(p.PacketSequence, PacketType.ServerBroadcast, data, p));

        //                Server.Console.Write(Color.Orange, "SERVER", message);
        //                break;

        //            case "kick":
        //                //int plid = int.Parse(queue.Dequeue());
        //                int plid;
        //                if (int.TryParse(queue.Peek(), out plid))
        //                    KickPlayer(plid);
        //                //else 
        //                //    KickPlayer(queue.Peek());

                        
        //                break;

        //            case "acks":
        //            case "ack":
        //                if (!Console.Filters.Remove(UI.ConsoleMessageTypes.Acks))
        //                {
        //                    Server.Console.Write("SERVER", "ACK reporting on");
        //                    Console.Filters.Add(UI.ConsoleMessageTypes.Acks);
        //                }
        //                else
        //                    Server.Console.Write("SERVER", "ACK reporting off");
        //                break;

        //            case "updatechunkedges":
        //                if (queue.Count == 0)
        //                    if (Player.Actor != null)
        //                    {
        //                        var pos = Player.Actor.Global.GetChunk(Instance.Map).MapCoords;
        //                        Instance.Map.UpdateChunkEdges(pos);
        //                        Instance.Enqueue(PacketType.UpdateChunkEdges, Network.Serialize(w => w.Write(pos)), SendType.OrderedReliable);
        //                        Server.Console.Write("SERVER", "Updating chunk edges at player's location");
        //                    }
        //                break;

        //            case "resetlight":
        //                try
        //                {
        //                    if (queue.Count == 0)
        //                    {
        //                        if (Player.Actor != null)
        //                        {
        //                            ResetLight(Player.Actor.Global.GetChunk(Instance.Map));
        //                            Server.Console.Write("SERVER", "Resetting light of chunk at player's location");
        //                        }
        //                        break;
        //                    }
        //                    if (queue.Peek() == "all")
        //                    {
        //                        Server.Console.Write("SERVER", "Resetting light of all active chunks");
        //                        foreach (var ch in Instance.Map.ActiveChunks.Values)
        //                            ResetLight(ch);
        //                        break;
        //                    }
        //                    int x = int.Parse(queue.Dequeue());
        //                    int y = int.Parse(queue.Dequeue());
        //                    Vector2 pos = new Vector2(x, y);
        //                    Chunk chunk;
        //                    if (!Instance.Map.ActiveChunks.TryGetValue(pos, out chunk))
        //                    {
        //                        Server.Console.Write("SERVER", "Chunk " + pos.ToString() + " doesn't exist");
        //                        break;
        //                    }
        //                    Server.Console.Write("SERVER", "Resetting light of chunk " + pos.ToString());
        //                    ResetLight(chunk);
        //                }
        //                catch (Exception) { Server.Console.Write("SERVER", "Error in command " + command); }
        //                break;

        //            case "savechunk":
        //                try
        //                {
        //                    //if (queue.Count == 0)
        //                    //{
        //                    //    Server.Console.Write("SERVER", "Saving all active chunks");
        //                    //    foreach (var ch in Instance.Map.ActiveChunks.Values)
        //                    //        ch.SaveServer();
        //                    //    break;
        //                    //}
        //                    int x = int.Parse(queue.Dequeue());
        //                    int y = int.Parse(queue.Dequeue());
        //                    Vector2 pos = new Vector2(x, y);
        //                    Chunk chunk;
        //                    if (!Instance.Map.ActiveChunks.TryGetValue(pos, out chunk))
        //                    {
        //                        Server.Console.Write("SERVER", "Chunk " + pos.ToString() + " doesn't exist");
        //                        break;
        //                    }
        //                    Server.Console.Write("SERVER", "Saving chunk " + pos.ToString());
        //                    chunk.SaveServer();
        //                }
        //                catch (Exception) { Server.Console.Write("SERVER", "Syntax error in: " + command); }
        //                break;

        //            case "savechunks":

        //                Server.Console.Write("SERVER", "Saving all active chunks");
        //                foreach (var ch in Instance.Map.ActiveChunks.Values)
        //                    ch.SaveServer();


        //                break;

        //            case "save":
        //                Server.Console.Write("SERVER", "Saving...");
        //                Save();
        //                break;

        //            case "savethumb":
        //                Instance.Map.GenerateThumbnails();
        //                break;

        //            default:
        //                Server.Console.Write("SERVER", "Unknown command " + command);
        //                break;
        //        }
        //    }
        //    catch (Exception) { Server.Console.Write("SERVER", "Syntax error in: " + command); }
        //}

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
                    byte sky, block;
                    //global.GetLight(this.Map, out sky, out block);
                    this.Map.GetLight(global, out sky, out block);
                    a.Write(sky);
                    a.Write(block);
                }
            });
            foreach (var pl in Players.GetList())
                //pl.Outgoing.
                    Enqueue(pl, Packet.Create(pl, PacketType.SyncLight, data, SendType.Reliable | SendType.Ordered));
        }
        void SyncInvalidateCells(IEnumerable<Vector3> globals)
        {
            foreach (var vector in globals)
            {
                this.InvalidateCell(vector);
                //BlocksToSync.Add(vector);
                //BlocksToSync.TryAdd(vector, vector.GetCell(Instance.Map));
                SyncCell(vector);
            }
        }

        private void SendLight(TimeSpan ServerClock)
        {
            if (this.LightUpdates.Count == 0)
                return;
            var changes = this.LightUpdates;
            this.LightUpdates = new HashSet<Vector3>();
            SyncLight(changes);
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
            GameObjectSlot targetSlot, sourceSlot;
            //if (!args.SourceEntity.Object.TryGetChild(args.TargetSlotID, out targetSlot) ||
            //    !parent.TryGetChild(args.SourceSlotID, out sourceSlot))
            if (!parent.TryGetChild(args.TargetSlotID, out targetSlot) ||
                !args.SourceEntity.Object.TryGetChild(args.SourceSlotID, out sourceSlot))
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
             //   SyncEvent(parent, Components.Message.Types.InsertAt, args.Write);
             //   return;
            }
            //parent.Global.GetChunk(this.Map).Invalidate();//.Saved = true;
            //args.SourceEntity.Object.Global.GetChunk(this.Map).Invalidate();//.Saved = true;
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
                    //this.Spawn(obj, targetSlot);
                    this.InstantiateObject(obj);
                }
                else
                    sourceSlot.Clear();
                targetSlot.Object = obj;
                return;
            }
            if (sourceSlot.Object.ID == targetSlot.Object.ID)
            {
                if (sourceSlot.StackSize + targetSlot.StackSize <= targetSlot.StackMax)
                {
                    targetSlot.StackSize += sourceSlot.StackSize;
                    this.DisposeObject(sourceSlot.Object.Network.ID);
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
            //foreach (var pl in Players.GetList())
            //    Enqueue(pl, Packet.Create(pl, PacketType.RemoteProcedureCall, data, SendType.Reliable | SendType.Ordered));
            Enqueue(PacketType.RemoteProcedureCall, data, SendType.OrderedReliable);
        }
        internal void RemoteProcedureCall(byte[] data, Vector3 global)
        {
            //foreach (var pl in Players.GetList())
            Enqueue(PacketType.RemoteProcedureCall, data, SendType.OrderedReliable, (pl) => GameMode.Current.IsPlayerWithinRangeForPacket(pl.Character, global));
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

            Enqueue(PacketType.RemoteProcedureCall, data, SendType.OrderedReliable, (pl) => GameMode.Current.IsPlayerWithinRangeForPacket(pl.Character, global));
        }
        internal void RemoteProcedureCall(TargetArgs target, Message.Types type, Action<BinaryWriter> writer)
        {
            var data = Network.Serialize(w =>
            {
                target.Write(w);
                w.Write((int)type);
                writer(w);
            });

            Enqueue(PacketType.RemoteProcedureCall, data, SendType.OrderedReliable, (pl) => GameMode.Current.IsPlayerWithinRangeForPacket(pl.Character, target.Global));
        }
        //private void NewMethod(GameObject sourceObj, GameObjectSlot targetSlot, GameObjectSlot sourceSlot, int amount)
        //{
        //    if (!targetSlot.HasValue) // if target slot empty, set object of target slot without swapping and return
        //    {
        //        if (targetSlot.Set(sourceObj, amount))
        //            sourceSlot.StackSize -= amount;
        //        return;
        //    }
        //    if (sourceSlot.Object.ID == targetSlot.Object.ID)
        //    {
        //        if (sourceSlot.StackSize + targetSlot.StackSize <= targetSlot.StackMax)
        //        {
        //            targetSlot.StackSize += sourceSlot.StackSize;
        //            DisposeObject(sourceSlot.Object.NetworkID);
        //            sourceSlot.Clear();
        //            //merge slots
        //            return;
        //        }
        //    }
        //    else
        //        if (amount < sourceSlot.StackSize)
        //            return;

        //    targetSlot.Swap(sourceSlot);
        //}

        //static HashSet<Vector3> BlocksToSync = new HashSet<Vector3>();
        static ConcurrentDictionary<Vector3, Cell> BlocksToSync = new ConcurrentDictionary<Vector3, Cell>();
        public void UpdateBlock(Vector3 global, Action<Cell> updater)
        {
            //Cell cell;
            //if (!global.TryGetCell(this.Map, out cell))
            //    return;
            //updater(cell);
            //BlocksToSync.TryAdd(global, cell);
        }
        public void SyncSetBlock(Vector3 global, Block.Types type)
        {
            //this.SetBlock(global, type);
            this.Map.SetBlock(global, type);
            //SyncCell(global, global.GetCell(Instance.Map));
            SyncCell(global, Instance.Map.GetCell(global));

            //var neighbors = global.GetNeighbors();
            //foreach (var vector in neighbors)
            //{
            //    //Cell cell = vector.GetCell(this.Map);
            //    Cell cell = this.Map.GetCell(vector);

            //    if (cell.IsNull())
            //        continue;
            //    //Cell.UpdateEdges(this.Map, vector, Edges.All, VerticalEdges.All);
            //    //this.InvalidateCell(vector, cell);
            //    this.Map.InvalidateCell(vector);
            //    //Instance.Map.InvalidateCell(vector, cell);
            //    //SyncCell(vector, cell);
            //}

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
            if (!this.Map.PositionExists(global))
                return;
            var previousBlock = this.Map.GetBlock(global);
            previousBlock.Remove(this.Map, global);
            var block = Start_a_Town_.Block.Registry[type];
            block.Place(this.Map, global, data, variation, orientation);
            //this.SyncCell(global, global.GetCell(this.Map));
            this.SyncCell(global, this.Map.GetCell(global));
            return;

            var neighbors = global.GetNeighbors();
            foreach (var vector in neighbors)
            {
                //toupdate.Add(vector);
                Cell cell;
                //if (!vector.TryGetCell(this.Map, out cell))
                if (!this.Map.TryGetCell(vector, out cell))
                    continue;
                if (cell.AllEdges == 0)
                {
                    //stop drawing cell
                }
                else
                {
                    //draw cell
                }
                //BlocksToSync.TryAdd(vector, cell);
                this.Map.InvalidateCell(vector);
                //this.InvalidateCell(vector, cell);
                //this.SyncCell(vector, cell);
            }
            //foreach (var item in toupdate)
            //    BlocksToSync.Add(item);

        }
        public void SetBlock(Vector3 global, Block.Types type)
        {
            this.SetBlock(global, type, 0);
            //if (!global.TrySetCell(this, type))
            //    throw new Exception("Invalid cell position");
        }
        public void SetBlock(Vector3 global, Block.Types type, byte data)
        {
            this.SetBlock(global, type, data, 0);
            //if (!global.TrySetCell(this, type, data))
            //    throw new Exception("Invalid cell position");
        }
        public void SetBlock(Vector3 global, Block.Types type, byte data, int variation)
        {
            //if (!global.TrySetCell(this, type, data))
            if (!global.TrySetCell(this, type, data, variation))
                throw new Exception("Invalid cell position");
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
        void SyncInvalidateCell(Vector3 global)
        {
            //Cell cell = global.GetCell(this.Map);
            Cell cell = this.Map.GetCell(global);

            this.InvalidateCell(global, cell);
            SyncCell(global, cell);
        }
        void InvalidateCell(Vector3 global)
        {
            //this.InvalidateCell(global, global.GetCell(this.Map));
            this.InvalidateCell(global, this.Map.GetCell(global));

        }
        void InvalidateCell(Vector3 global, Cell cell)
        {
            Instance.Map.InvalidateCell(global, cell);
            return;
            //Chunk chunk = global.GetChunk(Instance.Map);
            Chunk chunk = Instance.Map.GetChunk(global);

            //Cell.UpdateEdges(Instance.Map, global, Edges.All, VerticalEdges.All);
            if(chunk!=null)
            chunk.InvalidateCell(cell);
        }
        public void UpdateLight(Vector3 global)
        {
            //Chunk chunk = global.GetChunk(this.Map);
            Chunk chunk = this.Map.GetChunk(global);

            var q = chunk.ResetHeightMapColumn(global.ToLocal());
            //q.Enqueue(global);
            //foreach (var item in q)
            //    LogLightChange(item);
            this.SyncLight(global.GetColumn());

            //SyncLight(q);
            //this.LightingEngine.Enqueue(q);
            //this.LightingEngine.Enqueue(global);
            this.LightingEngine.Enqueue(new List<Vector3>() { global }.Concat(global.GetNeighbors()));
            //this.LightingEngine.Enqueue(global.GetNeighbors());
        }
        public void SpreadBlockLight(Vector3 global)
        {
            
         //   Vector3 rounded = global.Round();
            //rounded.SetBlockLight(this.Map, blockLightValue);
            //this.LogLightChange(rounded);
            //this.LightingEngine.EnqueueBlock(Position.GetNeighbors(rounded).ToArray());

         //   rounded.GetCell(this.Map).Luminance = blockLightValue;
            this.LightingEngine.EnqueueBlock(global);
            //this.LightingEngine.EnqueueBlock(new Vector3[] { global }, this.SyncLight);
        }

        /// <summary>
        /// TODO: maybe do syncposition which syncs both cell and light data at given position?
        /// </summary>
        static void SendSyncCells()
        {
            if (BlocksToSync.Count == 0)
                return;
            //var copy = new ConcurrentDictionary<Vector3, Cell>(BlocksToSync);
            //BlocksToSync = new ConcurrentDictionary<Vector3, Cell>();
            
            // save snapshot of keys to send
            var keys = BlocksToSync.Keys.ToList();
            byte[] data = Network.Serialize(w =>
            {
                //w.Write(copy.Count);
                w.Write(keys.Count);
                //foreach (var item in copy)
                foreach (var key in keys)
                {
                    Cell cell;
                    if (!BlocksToSync.TryRemove(key, out cell))
                        throw new Exception();
                    w.Write(key);
                    cell.Write(w);

                    //Vector3 vector = item.Key;
                    //w.Write(vector);
                    //Cell cell = item.Value;
                    //cell.Write(w);
                }
            });
            Instance.Enqueue(PacketType.SyncBlocks, data, SendType.Ordered | SendType.Reliable);
        }
        //static void SendSyncCells(IEnumerable<Vector3> vectors)
        //{
        //    //var copy = vectors.ToList();
        //    byte[] data = Network.Serialize(w =>
        //    {
        //        w.Write(vectors.Count());
        //        foreach (var item in vectors)
        //        {
        //            Vector3 vector = item;
        //            w.Write(vector);
        //            //Cell cell = vector.GetCell(Instance.Map);
        //            //cell.Write(w);
        //        }
        //    });
        //    Instance.Enqueue(PacketType.SyncCells, data, SendType.Ordered | SendType.Reliable);
        //}
        void SyncSlots(params TargetArgs[] slots)
        {
            byte[] data = Network.Serialize(w =>
            {
                w.Write(slots.Length);
                foreach (var slot in slots)
                {
                    slot.Write(w);
                    w.Write(slot.Slot.StackSize);
                    if (slot.Slot.StackSize > 0)
                        w.Write(slot.Slot.Object.Network.ID);
                }
            });
            Instance.Enqueue(PacketType.PlayerInventoryChange, data, SendType.OrderedReliable); // WARNING!!! TODO: handle case where each slot is owned by a different entity     
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
                        w.Write(slot.Object.Network.ID);
                }
            });
            Instance.Enqueue(PacketType.PlayerInventoryChange, data, SendType.OrderedReliable); // WARNING!!! TODO: handle case where each slot is owned by a different entity     
        }

        public void SendBlockMessage(Vector3 global, Components.Message.Types msg, params object[] parameters)
        {
            Start_a_Town_.Block.HandleMessage(this, global, ObjectEventArgs.Create(this, msg, parameters));
        }

        public AI AIHandler = new AI();

        int AISeed;
        Random AIRandom = new Random();//.Next(int.MinValue, int.MaxValue);
        public void SyncAI()
        {
            Random rand = new Random();
            this.AISeed = rand.Next(int.MinValue, int.MaxValue);
            this.AIRandom = new Random(this.AISeed);
            foreach (var obj in this.NetworkObjects)
                PostLocalEvent(obj.Value, Message.Types.SyncAI, this.AISeed + obj.Key.GetHashCode());
            byte[] data = Network.Serialize(w=>w.Write(this.AISeed));
            foreach (var pl in this.GetPlayers())
                Enqueue(pl, Packet.Create(pl, PacketType.SyncAI, data, SendType.OrderedReliable));
        }
        public void SyncAI(GameObject obj)
        {
            Random rand = new Random();
            var newSeed = this.AIRandom.Next(int.MinValue, int.MaxValue);
            obj.GetComponent<AIComponent>().Sync(newSeed);
            //PostLocalEvent(obj, Message.Types.SyncAI, newSeed + obj.Network.ID.GetHashCode());
            byte[] data = Network.Serialize(w =>
            {
                w.Write(obj.Network.ID);
                w.Write(newSeed);
            });
            foreach (var pl in this.GetPlayers())
                Enqueue(pl, Packet.Create(pl, PacketType.SyncAI, data, SendType.OrderedReliable));
        }

        public void SyncBlockVariation(IMap map, Vector3 global, byte variation)
        {
            map.GetCell(global).Variation = variation;
            byte[] data = Net.Network.Serialize(w =>
            {
                new TargetArgs(global).Write(w);
                w.Write(variation);
            });
            Enqueue(PacketType.SetBlockVariation, data, SendType.OrderedReliable, global);
        }
        public void SyncPlaceBlock(IMap map, Vector3 global, Block block, byte data, int variation, int orientation)
        {
            block.Place(map, global, data, variation, orientation);
            byte[] payload = Net.Network.Serialize(w =>
            {
                new TargetArgs(global).Write(w);
                w.Write((int)block.Type);
                w.Write(data);
                w.Write(variation);
                w.Write(orientation);
            });
            Enqueue(PacketType.SyncPlaceBlock, payload, SendType.OrderedReliable, global);
        }
    }
}
