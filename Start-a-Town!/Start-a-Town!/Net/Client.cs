using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.IO.Compression;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.GameModes;
using Start_a_Town_.Net.Packets;
using Start_a_Town_.Net.Packets.Player;
using Start_a_Town_.AI;
using Start_a_Town_.Components.AI;

namespace Start_a_Town_.Net
{
    enum PlayerSavingState { Saved, Changed, Saving }

    public class Client : IObjectProvider
    {
        static Client _Instance;
        static public Client Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new Client();
                return _Instance;
            }
        }
        public double CurrentTick => ClientClock.TotalMilliseconds;
        //static UI.ConsoleBoxAsync _Console;
        //public static UI.ConsoleBoxAsync Console
        //{
        //    get
        //    {
        //        if (_Console == null)
        //            _Console = new UI.ConsoleBoxAsync(UI.LobbyWindow.Instance.Console.Size) { FadeText = false }; ;
        //        return _Console;
        //    }
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
        public IMap Map { set { 
                Engine.Map = value;
            } get { return Engine.Map; } }


        public NetworkSideType Type { get { return NetworkSideType.Local; } }

        readonly int TimeoutLength = Engine.TicksPerSecond * 2;//5000;
        int Timeout = -1;

        readonly PacketTransfer PartialPacketReceiver;// = new PacketTransfer(HandleMessage);
        const int OrderedReliablePacketsHistoryCapacity = 64;
        readonly Queue<Packet> OrderedReliablePacketsHistory = new(OrderedReliablePacketsHistoryCapacity);
        public Client()
        {
            PartialPacketReceiver = new PacketTransfer(HandleMessage);
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
        public BinaryWriter GetOutgoingStreamTimestamped()
        {
            return this.OutgoingStreamTimestamped;
        }

        readonly Dictionary<Vector2, ChunkTransfer> PartialChunks = new();

        readonly Queue<WorldSnapshot> WorldStateBuffer = new();
        readonly int WorldStateBufferSize = 10;//50; //5;
        public const int ClientClockDelayMS = Server.SnapshotIntervalMS * 4;// * 10;// //2;
        int _Speed = 0;// 1;
        public int Speed { get { return this._Speed; } set { this._Speed = value; } }
        public void EnterWorld(GameObject playerCharacter)
        {
            //return;
            //PacketPlayerEnterWorld.Send(this, PlayerData.ID, playerCharacter);

            ////Packet.Create(PacketID, PacketType.PlayerEnterWorld, Network.Serialize(playerCharacter.Write))
            ////    .BeginSendTo(Host, RemoteIP, (a) => { });
        }
      
        public void Disconnect()
        {
            IsRunning = false;
            Instance.World = null;
            Engine.Map = null;
            Timeout = -1;
            //Instance.NetworkObjects = new ConcurrentDictionary<int, GameObject>();
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

            //Instance.NetworkObjects = new ConcurrentDictionary<int, GameObject>();
            Instance.NetworkObjects.Clear();
            ScreenManager.GameScreens.Clear();
            ScreenManager.Add(Rooms.MainScreen.Instance);
            //new UI.MessageBox("Warning!", "Disconnected from server", new ContextAction(() => "Ok", () => { })).ShowDialog();
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

            //IPAddress ipAddress;
            if (!IPAddress.TryParse(address, out IPAddress ipAddress))
            {
                var fromdns = Dns.GetHostEntry(address);
                ipAddress = fromdns.AddressList[0];
            }

            //RemoteIP = new IPEndPoint(IPAddress.Parse(address), 5541);
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
                //enter main receive loop
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
        public IEnumerable<PlayerData> GetOtherPlayers()
        {
            return Players.GetList().Where(pl => pl.ID != PlayerData.ID);
        }
        
        public void Update()
        {
            Timeout--;
            if (this.Timeout == 0)
                Disconnected();
            //Timeout--;
            if (!IsRunning)
                return;

            //ProcessSyncedPackets();
            //ProcessIncomingPackets();

            // CALL THESE IN THE GAMESPEED LOOP
            HandleOrderedPackets();
            HandleOrderedReliablePackets();
            ProcessSyncedPackets();
            ProcessIncomingPackets();
            if (GameMode.Current != null)
                GameMode.Current.Update(Instance);

            //UpdateWorldState();

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
            if (PlayerData != null && this.Map != null)
                //PacketMousePosition.Send(Instance, PlayerData.ID, ScreenManager.CurrentScreen.Camera.Coordinates, ScreenManager.CurrentScreen.Camera.Zoom, UI.UIManager.Mouse - UI.UIManager.Size / 2, ToolManager.CurrentTarget);
                PacketMousePosition.Send(Instance, PlayerData.ID, ToolManager.CurrentTarget); // TODO: do this at the toolmanager class instead of here

            // call these here or in the gamespeed loop?
            this.SendAcks();
            this.SendOutgoingStream();
        }
        //Dictionary<ulong, byte[]> BufferTimestamped = new();
        //PriorityQueue<ulong, byte[]> BufferTimestamped = new();
        //SortedList<ulong, byte[]> BufferTimestamped = new();
        //SortedDictionary<ulong, byte[]> BufferTimestamped = new();
        SortedDictionary<ulong, (ulong worldtick, double servertick, byte[] data)> BufferTimestamped = new();

        ulong lasttickreceived;
        public void HandleTimestamped(BinaryReader r)
        {
            var currenttick = this.Map.World.CurrentTick;
            var t = this.CurrentTick;
            for (int i = 0; i < this.Speed; i++)
            {
                var tick = r.ReadUInt64();
                var serverTick = r.ReadDouble();
                var length = r.ReadInt64();
                //if (tick < currenttick)
                //    "ti fash re pousth".ToConsole();
                ////throw new Exception();
                //else
                //    "ola kala twra e".ToConsole();
                if (length > 0)
                {
                    var array = r.ReadBytes((int)length);
                    if (tick == currenttick)
                        this.UnmergePackets(array);
                    else
                        //this.BufferTimestamped.Add(tick, array);
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
            //if (this.BufferTimestamped.Any())
            {
                var item = this.BufferTimestamped.First();
                var currenttick = this.Map.World.CurrentTick;
                var clienttick = this.CurrentTick;
                //if (item.Key < currenttick)
                //    return;
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
            //this.OutgoingStream.Write(this.PlayerData.ID);
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

            foreach (var item in Game1.Instance.GameComponents) //GetGameComponents())//.
                item.OnGameEvent(e);
            UI.TooltipManager.OnGameEvent(e);
            //ScreenManager.CurrentScreen.WindowManager.OnGameEvent(e);
            ScreenManager.CurrentScreen.OnGameEvent(e);

            ToolManager.OnGameEvent(e);
            if(this.Map!=null)
            this.Map.OnGameEvent(e);
        }

        readonly Dictionary<PacketType, IClientPacketHandler> PacketHandlers = new();
        public void RegisterPacketHandler(PacketType channel, IClientPacketHandler handler)
        {
            this.PacketHandlers.Add(channel, handler);
        }
        readonly static Dictionary<PacketType, Action<IObjectProvider, BinaryReader>> PacketHandlersNew = new();
        static public void RegisterPacketHandler(PacketType channel, Action<IObjectProvider, BinaryReader> handler)
        {
            PacketHandlersNew.Add(channel, handler);
        }
        
        readonly Dictionary<PacketType, Action<Client, Packet>> Packets = new();
        public void RegisterPacket(PacketType channel, Action<Client, Packet> handler)
        {
            this.Packets.Add(channel, handler);
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

        public void EventOccured(Components.Message.Types type, params object[] p)
        {
            var e = new GameEvent(this, ClientClock.TotalMilliseconds, type, p);
            OnGameEvent(e);
        }
        public void EventOccured(object id, params object[] p)
        {
            var e = new GameEvent(this, ClientClock.TotalMilliseconds, id, p);
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
                    //SyncTime(packet.Tick);
                    var clientms = packet.Tick - ClientClockDelayMS;
                    if (this.CurrentTick < clientms)
                    {
                        this.ClientClock = TimeSpan.FromMilliseconds(clientms);
                        //this.Report($"client clock caught up");
                        "client clock caught up".ToConsole();
                    }
                    HandleMessage(packet);
                }
            }
        }

        public void SavePlayerCharacter()
        {
            try
            {

                string working = Directory.GetCurrentDirectory();
                var savefolder = @"\Saves\Characters\";
                var directory = new DirectoryInfo(working + savefolder);
                if (!Directory.Exists(directory.FullName))
                    Directory.CreateDirectory(directory.FullName);

                if (!Instance.TryGetNetworkObject(this.PlayerData.CharacterID, out GameObject actor))
                    return;
                actor = actor.Clone();


                var filename = actor.Name + ".character.sat";
                string tempFile = "_" + filename;

                using (var stream = new MemoryStream())
                {
                    var writer = new BinaryWriter(stream);

                    SavePlayer(actor, writer);
                    Chunk.Compress(stream, directory + tempFile);
                }

                var fullpath = working + savefolder;
                if (File.Exists(fullpath + filename))
                    File.Replace(fullpath + tempFile, fullpath + filename, fullpath + filename + ".bak");
                else
                    File.Move(fullpath + tempFile, fullpath + filename);

            }
            catch (Exception e)
            {
                this.Log.Write(Color.Orange, "CLIENT", "Error saving character (" + e.Message + ")");
            }
        }

        public void SavePlayer(GameObject actor, BinaryWriter writer)
        {
            var tag = new SaveTag(SaveTag.Types.Compound, "Character");

            var charTag = new SaveTag(Start_a_Town_.SaveTag.Types.Compound, "PlayerCharacter", actor.SaveInternal());

            // save metadata such as hotbar
            //SaveTag hotbarTag = UI.Hud.Instance.HotBar.Save();
            var hotbarTag = Rooms.Ingame.Instance.Hud.HotBar.Save();

            //SaveTag hotbarTag = new SaveTag(Start_a_Town_.SaveTag.Types.Compound, "HotBar", t);
            tag.Add(charTag);
            tag.Add(hotbarTag);

            //charTag.WriteTo(writer);
            //hotbarTag.WriteTo(writer);
            tag.WriteTo(writer);
        }
        private void UnmergePackets(byte[] data)
        {
            using var mem = new MemoryStream(data);
            using var r = new BinaryReader(mem);
            var lastPos = mem.Position;
            //while (mem.Position < data.Length)
            //{

            //if (mem.Position < data.Length)
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


                //GameMode.Current.HandlePacket(Instance, type, r);
                if (mem.Position == lastPos)
                    break;
            }

            //}
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
                        //TargetArgs recipient = TargetArgs.Create(Instance.Map, r);
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
            //Action<byte[]> handler;
            //if (Instance.PacketHandlers.TryGetValue(msg.PacketType, out handler))
            //{
            //    handler(msg.Decompressed);
            //    return;
            //}
            //GameMode.Current.ClientPacketHandler.Handle(Instance, msg);
            //SyncTime(msg.Tick); // is this right to be here? or sync time immediately when packet is received and depending on whether it's reliable or unreliable?

            if (Instance.Packets.TryGetValue(msg.PacketType, out Action<Client, Packet> registeredPacket))
            {
                //msg.Payload.Deserialize(r => registeredPacket(Instance, r));
                registeredPacket(Instance, msg);
                return;
            }

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
                //case PacketType.Ping:
                //    break;

                case PacketType.RequestConnection:
                    Instance.Timeout = Instance.TimeoutLength;
                    //PlayerData.ID = msg.Payload.Deserialize<int>(r => r.ReadInt32());// Network.Deserialize<int>(msg.Payload, r => r.ReadInt32());
                    msg.Payload.Deserialize(r =>
                    {
                        Instance.PlayerData.ID = r.ReadInt32();
                        Instance.Players = PlayerList.Read(Instance, r);
                        Instance.Speed = r.ReadInt32();
                    });
                    Log.Write(Color.Lime, "CLIENT", "Connected to " + Instance.RemoteIP.ToString());
                    //Rooms.Ingame.Instance.Hud.Chat.Write(Log.EntryTypes.System, "Connected to " + RemoteIP.ToString());
                    //GameMode.Current.HandlePacket(Instance, msg);
                    GameMode.Current.PlayerIDAssigned(Instance);
                    //var tick = msg.Tick;
                    Instance.SyncTime(msg.Tick);
                    Instance.EventOccured(Message.Types.ServerResponseReceived);
                    break;


                case PacketType.PlayerDisconnected:
                    int plid = msg.Payload.Deserialize<int>(r => r.ReadInt32());
                    //PlayerDisconnected(Network.Deserialize<PlayerData>(msg.Payload, PlayerData.Read));
                    Instance.PlayerDisconnected(plid);
                    break;


                case PacketType.PlayerList:
                    Instance.ReceivePlayerList(msg.Payload);
                    UI.LobbyWindow.RefreshPlayers(Instance.Players.GetList());
                    break;


                case PacketType.UpdateChunkNeighbors:
                    var vector = Network.Deserialize<Vector2>(msg.Payload, r => r.ReadVector2());
                    break;

                case PacketType.UpdateChunkEdges:
                    vector = Network.Deserialize<Vector2>(msg.Payload, r => r.ReadVector2());
                    break;

                case PacketType.UnloadChunk:
                    vector = msg.Payload.Deserialize<Vector2>(r => r.ReadVector2());
                    Chunk chunk;
                    if (Instance.Map.GetActiveChunks().TryGetValue(vector, out chunk))
                    {
                        foreach (var o in chunk.GetObjects())
                            Instance.DisposeObject(o);
                        Instance.Map.GetActiveChunks().Remove(vector);
                    }
                    break;

                case PacketType.Partial:
                    Instance.PartialPacketReceiver.Receive(msg);
                    break;

                case PacketType.EntityInterrupt:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        var p = new PacketEntity(r.ReadInt32());
                        WorkComponent.End(Instance.GetNetworkObject(p.EntityID));
                    });
                    break;

                case PacketType.RemoteCall:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        double timestamp = r.ReadDouble();
                        TargetArgs recipient = TargetArgs.Read(Instance, r);
                        Components.Message.Types type = (Components.Message.Types)r.ReadInt32();
                        byte[] data = r.ReadBytes(r.ReadInt32());
                        GameObject obj = recipient.Object;// Instance.GetNetworkObject(objID);
                        //Instance.PostLocalEvent(obj, ObjectEventArgs.Create(type, data));
                        obj.HandleRemoteCall(ObjectEventArgs.Create(type, data));
                    });
                    break;

               
                case PacketType.PlayerJump:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int netid = r.ReadInt32();
                        //GameObject obj = Instance.NetworkObjects[netid];
                        if (!Instance.TryGetNetworkObject(netid, out GameObject obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        obj.GetComponent<MobileComponent>().Jump(obj);
                    });
                    break;
                case PacketType.PlayerChangeDirection:
                    var packet = new PacketPlayerChangeDirection(Instance, msg.Decompressed);//Payload);
                    if (packet.Entity.IsNull())
                    {
                        Instance.RequestEntityFromServer(packet.EntityID);
                        return;
                    }
                    packet.Entity.Direction = packet.Direction;

                    //Network.Deserialize(msg.Payload, r =>
                    //{
                    //    int netid = r.ReadInt32();
                    //    Vector3 direction = r.ReadVector3();
                    //    Instance.NetworkObjects[netid].Direction = direction;
                    //});
                    break;

                //case PacketType.PlayerToggleWalk:
                //    msg.Payload.Deserialize(r =>
                //    {
                //        int netid = r.ReadInt32();
                //        bool toggle = r.ReadBoolean();
                //        //GameObject obj = Instance.NetworkObjects[netid];
                //        GameObject obj;
                //        if (!Instance.TryGetNetworkObject(netid, out obj))
                //        {
                //            Instance.RequestEntityFromServer(netid);
                //            return;
                //        }
                //        obj.GetComponent<MobileComponent>().ToggleWalk(toggle);
                //    });
                //    return;

                case PacketType.PlayerToggleSprint:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        bool toggle = r.ReadBoolean();
                        //GameObject obj = Instance.NetworkObjects[netid];
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
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
                        //GameObject obj = Instance.NetworkObjects[netid];
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        //GameObject obj = Instance.NetworkObjects[netid];

                        obj.GetComponent<WorkComponent>().UseTool(obj, target);
                    });
                    return;

                case PacketType.PlayerUse:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        //GameObject obj = Instance.NetworkObjects[netid];
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        //GameObject obj = Instance.NetworkObjects[netid];
                        obj.GetComponent<WorkComponent>().Perform(obj, target.GetAvailableTasks(Instance).FirstOrDefault(), target);
                    });
                    return;

                case PacketType.PlayerUseHauled:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        //GameObject obj = Instance.NetworkObjects[netid];
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        //GameObject obj = Instance.NetworkObjects[netid];
                        //var hauled = obj.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling].Object;
                        var hauled = obj.GetComponent<HaulComponent>().GetObject();//.Slot.Object;

                        if (hauled.IsNull())
                            return;
                        obj.GetComponent<WorkComponent>().Perform(obj, hauled.GetHauledActions(target).FirstOrDefault(), target);
                    });
                    return;

                case PacketType.PlayerDropHauled:
                    msg.Payload.Deserialize(r =>
                    {
                        var netid = r.ReadInt32();
                        var target = TargetArgs.Read(Instance, r);
                        //GameObject obj = Instance.NetworkObjects[netid];
                        if (!Instance.TryGetNetworkObject(netid, out GameObject obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        //var interaction = target.GetContextAction(new KeyBinding(GlobalVars.KeyBindings.Drop));
                        //if (interaction == null)
                        //obj.GetComponent<GearComponent>().Throw(Vector3.Zero, obj);
                        obj.GetComponent<HaulComponent>().Throw(Vector3.Zero, obj);

                        //else
                        //    obj.GetComponent<WorkComponent>().Perform(obj, interaction, target);

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
                        //var interaction = target.GetContextActionWorld(Instance, input) ?? Start_a_Town_.PlayerInput.GetDefaultAction(input.Action);
                        var interaction =
                            //target.GetContextActionWorld(Instance, input) ?? 
                            //Start_a_Town_.PlayerInput.GetDefaultInput(input);
                            Start_a_Town_.PlayerInput.GetDefaultInput(obj, target, input);

                        //if (interaction == null)
                        //    return;

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

                        //target.Object.HandleRemoteCall(ObjectEventArgs.Create(call, args));
                        target.HandleRemoteCall(Instance, ObjectEventArgs.Create(call, args));

                        //target.Object.HandleRemoteCall(call);
                    });
                    return;

                case PacketType.PlayerPickUp:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        //GameObject obj = Instance.NetworkObjects[netid];
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

                case PacketType.PlayerCarry:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        //GameObject obj = Instance.NetworkObjects[netid];
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        //obj.GetComponent<ControlComponent>().StartScript(Script.Types.Hauling, new ScriptArgs(Instance, obj, target));
                        obj.GetComponent<WorkComponent>().Perform(obj, new Components.Interactions.Carry(), target);
                    });
                    return;

                //case PacketType.PlayerEquip:
                //    msg.Payload.Deserialize(r =>
                //    {
                //        int netid = r.ReadInt32();
                //        TargetArgs target = TargetArgs.Read(Instance, r);
                //        //GameObject obj = Instance.NetworkObjects[netid];
                //        GameObject obj;
                //        if (!Instance.TryGetNetworkObject(netid, out obj))
                //        {
                //            Instance.RequestEntityFromServer(netid);
                //            return;
                //        }
                //        obj.GetComponent<ControlComponent>().StartScript(Script.Types.Equipping, new ScriptArgs(Instance, obj, target));
                //    });
                //    return;

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
                        //PersonalInventoryComponent.InsertOld(obj, target.Slot);
                        PersonalInventoryComponent.Receive(obj, target.Slot, false);
                    });
                    return;

                case PacketType.EntityThrow:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        var dir = r.ReadVector3();
                        var all = r.ReadBoolean();
                        //GameObject obj = Instance.NetworkObjects[netid];
                        if (!Instance.TryGetNetworkObject(netid, out GameObject obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        //obj.GetComponent<HaulComponent>().Throw(obj, dir);
                        HaulComponent.ThrowHauled(obj, dir, all);

                    });
                    return;

                case PacketType.PlayerStartAttack:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        //GameObject obj = Instance.NetworkObjects[netid];
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        //obj.GetComponent<ControlComponent>().StartScript(Script.Types.Attack, new ScriptArgs(Instance, obj, TargetArgs.Empty));
                        obj.GetComponent<AttackComponent>().Start(obj);
                    });
                    return;

                case PacketType.PlayerFinishAttack:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        var dir = r.ReadVector3();
                        //GameObject obj = Instance.NetworkObjects[netid];
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        //obj.GetComponent<ControlComponent>().FinishScript(Script.Types.Attack, new ScriptArgs(Instance, obj, TargetArgs.Empty, w => w.Write(dir)));
                        obj.GetComponent<AttackComponent>().Finish(obj, dir);
                    });
                    return;

                case PacketType.EntityCancelAttack:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        obj.GetComponent<AttackComponent>().Cancel(obj);
                    });
                    return;

                

                case PacketType.PlayerStartBlocking:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        //GameObject obj = Instance.NetworkObjects[netid];
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        //obj.GetComponent<ControlComponent>().StartScript(Script.Types.Block, new ScriptArgs(Instance, obj, TargetArgs.Empty));
                        obj.GetComponent<Components.BlockingComponent>().Start(obj);

                    });
                    return;

                case PacketType.PlayerFinishBlocking:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        //GameObject obj = Instance.NetworkObjects[netid];
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        //obj.GetComponent<ControlComponent>().FinishScript(Script.Types.Block, new ScriptArgs(Instance, obj, TargetArgs.Empty));
                        obj.GetComponent<Components.BlockingComponent>().Stop(obj);

                    });
                    return;

                //case PacketType.PlayerCraft:
                //    Network.Deserialize(msg.Payload, r =>
                //    {
                //        //var actor = Instance.NetworkObjects[r.ReadInt32()];
                //        //var building = TargetArgs.Read(Instance, r);
                //        //var prod = new Components.Crafting.Reaction.Product.ProductMaterialPair(r);
                //        //var containerEntity = Instance.GetNetworkObject(r.ReadInt32());
                //        //var container = containerEntity.GetContainer(r.ReadInt32());
                //        //actor.GetComponent<WorkComponent>().Perform(actor, new Components.Interactions.Craft(prod, container), building);// new TargetArgs());

                //        int netid = r.ReadInt32();
                //        GameObject actor;
                //        if (!Instance.TryGetNetworkObject(netid, out actor))
                //        {
                //            Instance.RequestEntityFromServer(netid);
                //            return;
                //        }
                //        var crafting = new Components.Crafting.CraftOperation(Instance, r);
                //        var reaction = Components.Crafting.Reaction.Dictionary[crafting.ReactionID];
                //        if (reaction == null)
                //            return;
                //        var product = reaction.Products.First().GetProduct(reaction, crafting.Building.Object, crafting.Materials, crafting.Tool);
                //        if (product == null)
                //            return;
                //        if (product.Tool != null)
                //            GearComponent.Equip(actor, PersonalInventoryComponent.FindFirst(actor, foo => foo == product.Tool));

                //        //actor.GetComponent<WorkComponent>().Perform(actor, new Components.Interactions.Craft(product, crafting.Container), crafting.Building);
                //        var workstation = Instance.Map.GetBlockEntity(crafting.WorkstationEntity) as Blocks.BlockWorkbenchEntity;
                //        //actor.GetComponent<WorkComponent>().Perform(actor, new Components.Crafting.InteractionCraftingWorkbench(product, workstation, crafting.WorkstationEntity), crafting.Building);
                //        if (workstation == null)
                //            actor.GetComponent<WorkComponent>().Perform(actor, new Components.Crafting.InteractionCraftingPerson(product), crafting.Building);
                //        else
                //            actor.GetComponent<WorkComponent>().Perform(actor, new Components.Crafting.InteractionCraftingWorkbench(product, workstation, crafting.WorkstationEntity), crafting.Building);

                //    });
                //    return;



                case PacketType.RandomEvent:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        double timestamp = r.ReadDouble();
                        TargetArgs recipient = TargetArgs.Read(Instance, r);
                        Components.Message.Types type = (Components.Message.Types)r.ReadInt32();
                        byte[] data = r.ReadBytes(r.ReadInt32());
                        double ran = r.ReadDouble();
                        var e = RandomObjectEventArgs.Create(type, data, ran);
                        e.Network = Instance;
                        if (recipient.Type == TargetType.Position)//.Object.IsBlock())
                            Block.HandleMessage(Instance, recipient.Global, e);
                        //recipient.Global.GetBlock(Instance.Map).HandleMessage(recipient.Global, e);
                        else
                        {
                            GameObject obj = recipient.Object;
                            obj.HandleRandom(e);
                        }
                        // Instance.PostLocalEvent(obj, RandomObjectEventArgs.Create(type, data, ran));
                    });
                    break;

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
                case PacketType.InstantiateInContainer:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        //   TargetArgs obj = TargetArgs.Read(Instance, r);
                        GameObject obj = GameObject.CreatePrefab(r);
                        TargetArgs container = TargetArgs.Read(Instance, r);
                        byte containerID = r.ReadByte();
                        byte slotID = r.ReadByte();
                        byte amount = r.ReadByte();

                        obj.Instantiate(Instance.Instantiator);
                        Instance.PostLocalEvent(container.Object, ObjectEventArgs.Create(Components.Message.Types.AddItem, new object[] { obj, containerID, slotID, amount }));
                    });
                    return;

                case PacketType.SpawnObjectInSlot:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        var netid = r.ReadInt32();
                        var target = TargetArgs.Read(Instance, r);
                        var obj = Instance.GetNetworkObject(netid);
                        target.Slot.Object = obj;
                    });
                    return;

                case PacketType.SpawnChildObject:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        GameObject obj = GameObject.CreatePrefab(r);
                        if (obj.RefID == 0)
                            throw new Exception("Uninstantiated entity");

                        // instantiate entity on client if it isn't already instantiated
                        if (!Instance.NetworkObjects.ContainsKey(obj.RefID))
                            Instance.Instantiate(obj);

                        int parentID = r.ReadInt32();
                        GameObject parent;
                        if (!Instance.TryGetNetworkObject(parentID, out parent))
                            throw (new Exception("Parent doesn't exist"));
                        //Instance.Instantiate(obj);

                        obj.Parent = parent;
                        int childIndex = r.ReadInt32();
                        var slot = parent.GetChildren()[childIndex];
                        slot.Object = obj;
                        //slot.Object.StackSize.ToConsole();
                    });
                    return;

                case PacketType.SyncSlot:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int netid = r.ReadInt32();
                        byte slotid = r.ReadByte();
                        GameObject parent;
                        if (!Instance.TryGetNetworkObject(netid, out parent))
                            throw (new Exception("Parent doesn't exist"));
                        parent.GetChild(slotid).Read(r);
                    });
                    break;

                case PacketType.InstantiateObject: //register netID to list without spawning
                    var ent = Network.Deserialize<GameObject>(msg.Payload, GameObject.CreatePrefab);
                    ent.Instantiate(Instance.Instantiator);//.ObjectCreated();
                    //if (ent.Exists)
                    //    Instance.Spawn(ent);
                    return;

                case PacketType.InstantiateAndSpawnObject: //register netID to list and spawn
                    Network.Deserialize(msg.Payload, r =>
                    {
                        GameObject ob = GameObject.CreatePrefab(r).ObjectCreated(); // the obj is received with a netid but without a position component// a  position component and netid
                        //Instance.Spawn(ob.Instantiate(Instance.Instantiator));
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
                                Log.Write(Color.Orange, "CLIENT", "Can't dispose null entity");//doesn't exist.");
                                break;
                            }
                            GameObject o;
                            //Instance.NetworkObjects.TryRemove(tar.Object.NetworkID, out o);
                            Instance.Despawn(tar.Object);
                            Instance.DisposeObject(tar.Object);
                            break;

                        case TargetType.Slot:
                            o = tar.Slot.Object;
                            //Instance.NetworkObjects.TryRemove(o.NetworkID, out o);
                            Instance.DisposeObject(o);
                            tar.Slot.Clear();
                            break;

                        default:
                            throw new Exception("Invalid object");
                            //break;
                    }
                    //GameObject o;
                    //Instance.NetworkObjects.TryRemove(tar.Object.NetworkID, out o);
                    //Instance.Despawn(tar.Object);
                    break;

                case PacketType.SyncEntity:

                    Network.Deserialize<GameObject>(msg.Payload, r =>
                    {
                        GameObject entity = GameObject.CreatePrefab(r);
                        GameObject existing;
                        if (Instance.TryGetNetworkObject(entity.RefID, out existing))
                        {
                            // TODO: sync existing entity's values from packet here
                            //existing.Update(r);
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
                        //var senderID = r.ReadInt32();
                        //var entity = TargetArgs.Read(Instance, r).Object;
                        var tarentityID = r.ReadInt32(); //TargetArgs.Read(Instance, r).Object;
                        var entity = Instance.NetworkObjects[tarentityID];
                        var quantity = r.ReadInt32();
                        entity.StackSize += quantity;
                        //entity.TryGetComponent<StackableComponent>(c => c.SetStacksize(entity, entity.StackSize + quantity));
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
                        //Instance.Spawn(toSpawn);
                        toSpawn.Spawn(this.Map, pos);

                        // WARNING!!! chainging position AFTER spawning WILL cause entity to get updated and eventually attempted to be removed from a DIFFERENT chunk than the one in which it has spawned
                        // TODO: find another way to spawn it in the correct location and not the previously stored location before the entity became a child of another entity
                        //toSpawn.Global = pos;
                        //toSpawn.Velocity = vel;
                    });
                    break;

                case PacketType.DespawnObject:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int nid = r.ReadInt32();
                        Instance.Despawn(nid);
                    });
                    break;


                case PacketType.SyncTime:
                    throw new Exception();
                    msg.Payload.Deserialize(r =>
                    {
                        Instance.SyncTime(r.ReadDouble());
                    });
                    break;

                case PacketType.ServerBroadcast:
                    Network.Deserialize(msg.Payload, reader =>
                    {
                        string chatText = reader.ReadASCII();
                        Network.Console.Write(Color.Yellow, "SERVER", chatText);
                    });
                    break;

                //case PacketType.ConnectionRefused

                case PacketType.RequestNewObject:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        TargetArgs obj = TargetArgs.Read(Instance, r);
                        byte amount = r.ReadByte();
                        DragDropManager.Create(new DragDropSlot(PlayerOld.Actor, GameObjectSlot.Empty, new GameObjectSlot(obj.Object, amount), DragDropEffects.Move | DragDropEffects.Copy));

                    });
                    break;

                //case PacketType.JobCreate:
                //    Network.Deserialize(msg.Payload, r =>
                //    {

                //        TownJob job = TownJob.Read(r, Instance);
                //        Instance.Map.GetTown().Jobs.Add(job);//new TownJobStep(creator.Object, targ, script));
                //        Towns.TownJobsWindow.Instance.Show(Instance.Map.GetTown());
                //    });
                //    break;

                //case PacketType.JobDelete:
                //    Network.Deserialize(msg.Payload, r =>
                //    {
                //        int jobID = r.ReadInt32();
                //        //int removed = Instance.Map.Town.Jobs.RemoveAll(j => j.ID == jobID);
                //        Instance.Map.GetTown().Jobs.Remove(jobID);
                //        Towns.TownJobsWindow.Instance.Refresh(Instance.Map.GetTown());//.Jobs);
                //    });
                //    break;

                case PacketType.SyncLight:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int count = r.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            Vector3 glob = r.ReadVector3();
                            byte sky = r.ReadByte(), block = r.ReadByte();
                            //glob.SetLight(Instance.Map, sky, block);
                            Instance.Map.SetLight(glob, sky, block);
                        }
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
                        //orientation.ToConsole();

                        if (!Instance.Map.IsInBounds(global))
                            return;
                        //var previousBlock = Instance.Map.GetBlock(global);
                        //previousBlock.Remove(Instance.Map, global);
                        Instance.Map.RemoveBlock(global);
                        var block = Block.Registry[type];
                        if (block != BlockDefOf.Air)
                            //Instance.Map.PlaceBlockNew(global, type, data, variation, orientation);
                            block.Place(Instance.Map, global, data, variation, orientation);
                    });
                    break;

                case PacketType.SetBlockVariation:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        var t = TargetArgs.Read(Instance, r);
                        byte variation = r.ReadByte();
                        Instance.Map.GetCell(t.Global).Variation = variation;
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
                        //Instance.Map.GetCell(t.Global).Variation = variation;
                        var block = Block.Registry[blocktype];
                        block.Place(Instance.Map, t.Global, data, variation, orientation);
                    });
                    break;

                case PacketType.SyncBlocks:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int count = r.ReadInt32();
                        List<Vector3> vectors = new List<Vector3>();
                        for (int i = 0; i < count; i++)
                        {
                            Vector3 global = r.ReadVector3();
                            var blockType = (Block.Types)r.ReadByte();
                            var x = r.ReadByte();
                            var y = r.ReadByte();
                            var z = r.ReadByte();
                            var variation = r.ReadByte();
                            var orientation = r.ReadByte();

                            //this.Data = new BitVector32(reader.ReadInt32());
                            var data = new BitVector32(r.ReadInt32());
                            var blockdata = r.ReadByte();
                            var block = Start_a_Town_.Block.Registry[blockType];
                            var previousBlock = Instance.Map.GetBlock(global);
                            previousBlock.Remove(Instance.Map, global);
                            block.Place(Instance.Map, global, blockdata, (int)variation, (int)orientation);
                            // TODO: WARNING: player mouseover target in CONTROLLER class (not current tool) is almost always the block that has changed! careful! maybe reset the target to null here to prevent problems?
                            //Rooms.Ingame.Instance.ToolManager.ActiveTool.Target = null;
                            // set mouseover to null before the current tool fetches is later in the current frame
                            Controller.Instance.MouseoverBlock.Target = TargetArgs.Null;// null;

                            //return;



                            ////Cell cell = global.GetCell(Instance.Map);//.Read(r);
                            //Cell cell = Instance.Map.GetCell(global);//.Read(r);

                            //if (cell.IsNull())
                            //    return;
                            //cell.Read(r);

                            ////Chunk ch = Instance.Map.GetChunk(global);
                            //////ch.InvalidateLight(cell); // no need, invalidatecell does this
                            ////ch.InvalidateCell(cell);
                            //Instance.Map.InvalidateCell(global);
                            //foreach (var n in global.GetNeighbors())
                            //    Instance.Map.InvalidateCell(n);
                        }
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

                case PacketType.PlayerInventoryOperationOld:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        double timestamp = r.ReadDouble();
                        TargetArgs recipient = TargetArgs.Read(Instance, r);
                        Components.ArrangeChildrenArgs invArgs = Components.ArrangeChildrenArgs.Translate(Instance, r);
                        Instance.InventoryOperation(recipient.Object, invArgs);
                    });
                    break;

                case PacketType.PlayerSlotRightClick:
                    msg.Payload.Deserialize(r =>
                    {
                        TargetArgs actor = TargetArgs.Read(Instance, r);
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        TargetArgs child = TargetArgs.Read(Instance, r);
                        target.HandleRemoteCall(Instance, new ObjectEventArgs(Components.Message.Types.PlayerSlotRightClick, actor.Object, child.Object));
                        //Instance.PostLocalEvent(target.Object, Components.Message.Types.PlayerSlotRightClick, actor.Object, child.Object);
                    });
                    return;

                case PacketType.PlayerSlotClick:
                    msg.Payload.Deserialize(r =>
                    {
                        //GameObject plChar = Instance.NetworkObjects[msg.Player.CharacterID];
                        TargetArgs actor = TargetArgs.Read(Instance, r);
                        TargetArgs t = TargetArgs.Read(Instance, r);
                        var parent = t.Slot.Parent as GameObject;
                        Instance.PostLocalEvent(parent, Components.Message.Types.SlotInteraction, actor.Object, t.Slot);
                    });
                    return;



                case PacketType.SyncAI:
                    msg.Payload.Deserialize(r =>
                    {
                        //var aiEntity = Instance.NetworkObjects[r.ReadInt32()];
                        GameObject aiEntity;
                        if (!Instance.NetworkObjects.TryGetValue(r.ReadInt32(), out aiEntity))
                            return;
                        var seed = r.ReadInt32();
                        Instance.SyncAI(aiEntity, seed);
                    });
                    break;

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

                case PacketType.ConversationStart:
                    msg.Payload.Deserialize(r =>
                    {
                        var e1 = Instance.GetNetworkObject(r.ReadInt32());
                        var e2 = Instance.GetNetworkObject(r.ReadInt32());
                        var convo = new Conversation(e1, e2);
                        convo.Start();
                        Log.Write("Conversation started between " + e1.Name + " and " + e2.Name);
                    });
                    break;

                case PacketType.ConversationFinish:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        var text = r.ReadString();
                        //obj.GetComponent<SpeechComponent>().FinishConversation(obj, text);
                        Instance.PostLocalEvent(obj, Message.Types.ConversationFinish, text);
                    });
                    break;

                case PacketType.Conversation:
                    msg.Payload.Deserialize(r =>
                    {
                        //GameObject entity;
                        //if (!Instance.NetworkObjects.TryGetValue(r.ReadInt32(), out entity))
                        //    return;
                        var p = new AI.PacketDialogueOptions(Instance, r);
                        Instance.EventOccured(Components.Message.Types.Dialogue, p);
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
            //else
            //{
            //    ("sync time packet accepted (last: " + LastReceivedTime.ToString() + ", received: " + serverMS.ToString()).ToConsole();// + "server: " + Server.ServerClock.TotalMilliseconds.ToString() + ")").ToConsole();
            //}
            this.LastReceivedTime = serverMS;
            var newtime = serverMS - ClientClockDelayMS;
         
            TimeSpan serverTime = TimeSpan.FromMilliseconds(newtime);
            
            this.ClientClock = serverTime;
        }

        private void ParseCommand(string command)
        {
            CommandParser.Execute(this, command);
            //var p = command.Split(' ');
            //var type = p[0];
            //switch (type)
            //{
            //    case "set":
            //        switch (p[1])
            //        {
            //            case "time":
            //                int t = int.Parse(p[2]);
            //                this.Map.Time = new TimeSpan(this.Map.Time.Days, t, this.Map.Time.Minutes, this.Map.Time.Seconds);
            //                foreach (var ch in this.Map.GetActiveChunks())
            //                    ch.Value.LightCache.Clear();
            //                break;

            //            default:
            //                break;
            //        }
            //        break;

            //    default:
            //        break;
            //}
        }

        //int AISeed;
        void SyncAI(GameObject entity, int seed)
        {
            //this.AISeed = seed;
            // iterate through all entities and sync them
            //foreach (var obj in this.NetworkObjects)
            //PostLocalEvent(obj.Value, Message.Types.SyncAI, this.AISeed + obj.Key.GetHashCode());
            //PostLocalEvent(entity, Message.Types.SyncAI, seed + obj.Key.GetHashCode());
            entity.GetComponent<AIComponent>().Sync(seed);
        }

        public void RequestEntityFromServer(int entityNetID)
        {
            Network.Serialize(w =>
            {
                w.Write(entityNetID);
            }).Send(PacketID, PacketType.RequestEntity, Host, RemoteIP);
        }

        //private void ReceiveChunkSegment(Vector2 chCoords, ChunkTransfer.ChunkSegment chunkSegment)
        //{
        //    if (!this.PartialChunks.TryGetValue(chCoords, out ChunkTransfer chunkTransfer))
        //        this.PartialChunks[chCoords] = ChunkTransfer.BeginReceive(chCoords, ChunkTransferComplete);
        //    chunkTransfer.Receive(chunkSegment);
        //}
        //void ChunkTransferComplete(Chunk chunk)
        //{
        //    this.PartialChunks.Remove(chunk.MapCoords);
        //}

        public void SyncEvent(GameObject recipient, Components.Message.Types msg, Action<BinaryWriter> writer) { }

        public class ObjectEvent : EventArgs
        {
            public double TimeStamp { get; set; }
            public ObjectEventArgs Args { get; set; }
            public GameObject Recipient { get; set; }

            public ObjectEvent(double timestamp, GameObject recipient, ObjectEventArgs args)
            {
                this.TimeStamp = timestamp;
                this.Recipient = recipient;
                this.Args = args;
            }
        }


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
                    //if (packet.Synced)
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
            //GameObject o;

            // ONLY SERVER CAN DESTROY OBJECTS???
            //if (Instance.NetworkObjects.TryRemove(objNetID, out o))
            //    Instance.Despawn(o);

            //return Instance.NetworkObjects.TryRemove(obj.NetworkID, out o);
            return DisposeObject(obj.RefID);
        }
        public bool DisposeObject(int netID)
        {
            // concurrent old
            //GameObject o;
            //if (!NetworkObjects.TryRemove(netID, out o))
            //    return false;
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
            // NO LOGIC HERE FOR CLIENT ! HANDLE OBJECTDESTROY PACKET IN HANDLEMESSAGE

            //GameObject o;
            ////if (Instance.NetworkObjects.TryRemove(obj.NetworkID, out o))
            ////    Instance.Despawn(o);
            ////else
            ////    throw new Exception("Object mismatch!");

            //NetworkObjects.TryRemove(obj.NetworkID, out o);
            //Despawn(obj);
        }
        public void SyncDisposeObject(GameObjectSlot slot)
        {
            // NO LOGIC HERE FOR CLIENT ! HANDLE OBJECTDESTROY PACKET IN HANDLEMESSAGE
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
        public void InstantiateAndSpawn(IEnumerable<GameObject> objects) { }

        public void Instantiator(GameObject ob)
        {
            //ob.Net = this;
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
            //Console.Write(Color.Yellow, "CLIENT", player.Name + " connected");
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
            //Console.Write(Color.Yellow, "CLIENT", player.Name + " disconnected");
            UI.UIChat.Instance.Write(new Log.Entry(Start_a_Town_.Log.EntryTypes.System, player.Name + " disconnected"));
            //UI.Chat.Instance.Write(Color.Yellow, "CLIENT", player.Name + " disconnected");

        }
        public void PlayerDisconnected(int playerID)
        {
            //PlayerData player = Players.GetList()[playerID];
            PlayerData player = Players.GetList().FirstOrDefault(p => p.ID == playerID);
            if (player == null)
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
                    //this.AckQueue.Enqueue(packet.ID); // slow sending of acks resulting in problems


                this.IncomingAll.Enqueue(packet);

            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception e)
            {
                //ScreenManager.Remove(); // this is not the main thread. if i remove from here then in case the main thread is drawing, it won't be able to access the ingame camera class
                //// so either don't remove the screen here, or pass the camera to the root draw call instead of accessing it through the screenmanager currentscreen property
                //Disconnected();
                Timeout = 0;
                e.ShowDialog();
            }
        }
        readonly int RecentPacketBufferSize = 32;
        Queue<long> RecentPackets = new();
        //    int LastPacketsBitMask;
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
            //bool found = mask[distance];// (mask.Data & (1 << distance));// mask[distance];
            if (distance > 31)
            {
                // very old packet
                //return false;
                Log.Write(Color.Orange, "CLIENT", "Warning! Received severely outdated packet: " + packet.PacketType.ToString());
                return false;
            }
            int mask = (1 << distance);
            bool found = (field.Data & mask) == mask;// mask[distance];
            if (found)
                if (distance < 32)
                    if (!RecentPackets.Contains(id))
                        throw new Exception("duplicate detection error");
            return found;
            //create bitmask 
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
        
        //void AppendMessage(Packet msg, byte[] data)
        //{
        //    byte[] full = new byte[msg.Length];
        //    msg.Payload.CopyTo(full, 0);
        //    Array.Copy(data, 0, full, msg.Payload.Length, data.Length);
        //    msg.Payload = full;
        //}
        //void AppendMessage(IAsyncResult ar, Packet msg)
        //{
        //    //Socket socket = (Socket)ar.AsyncState;
        //    UdpConnection state = (UdpConnection)ar.AsyncState;
        //    // int newBytesLength = state.Socket.EndReceive(ar);
        //    "end receive append".ToConsole();


        //    //byte[] full = new byte[msg.Payload.Length + newBytesLength];
        //    //msg.Payload.CopyTo(full, 0);
        //    //Array.Copy(state.Buffer, 0, full, msg.Payload.Length, newBytesLength);
        //    //msg.Payload = full;
        //    //int remain = msg.Length - msg.Payload.Length;


        //    byte[] full = new byte[msg.Length];
        //    msg.Payload.CopyTo(full, 0);
        //    Array.Copy(state.Buffer, 0, full, msg.Payload.Length, state.Buffer.Length);
        //    msg.Payload = full;
        //    int remain = msg.Length - msg.Payload.Length;


        //    if (msg.Payload.Length == msg.Length)
        //    {
        //        ("finished recieving multi-packet of length " + msg.Length.ToString()).ToConsole();
        //        IncomingAll.Enqueue(msg);

        //        int newBytesLength = state.Socket.EndReceive(ar);
        //        //state.Buffer = new byte[1024];
        //        var oldState = new UdpConnection("normal receiving", Host) { Buffer = new byte[Packet.Size] };
        //        Host.BeginReceive(oldState.Buffer, 0, oldState.Buffer.Length, SocketFlags.None, ReceiveMessage, oldState);
        //        return;
        //    }
        //    else
        //        throw new Exception("didn't receive full data");
        //    //state.Buffer = new byte[remain];
        //    //("recieved partial packet, " + remain.ToString() + " bytes remaining").ToConsole();
        //    //Host.BeginReceive(state.Buffer, 0, remain, SocketFlags.None, a => AppendMessage(a, msg), state);
        //}
        


        public HashSet<Vector2> ChunkRequests = new();
        


        public void NotifyChunkLoaded(Vector2 chunkCoords, Action<Chunk> callback)
        {
            if (Instance.Map != null)
                if (Instance.Map.GetActiveChunks().TryGetValue(chunkCoords, out Chunk existingChunk))
                {
                    callback(existingChunk);
                    return;
                }
            Instance.ChunkCallBackEvents.AddOrUpdate(chunkCoords, new ConcurrentQueue<Action<Chunk>>(new Action<Chunk>[] { callback }), (vec, queue) => { queue.Enqueue(callback); return queue; });
        }
        public void OnChunkReceived(Vector2 chunkCoords, Action<Chunk> callback)
        {
            // TODO: keep a list of requested chunks until they are received
            this.ChunkCallBackEvents.AddOrUpdate(chunkCoords, new ConcurrentQueue<Action<Chunk>>(new Action<Chunk>[] { callback }), (vec, queue) => { queue.Enqueue(callback); return queue; });
        }
        void ReceiveChunk(byte[] data)
        {
            this.ReceiveChunk(Chunk.Create(data));
        }
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
            
            //foreach (var blockentity in chunk.BlockEntitiesByPosition)
            //{
            //    blockentity.Value.MapLoaded(this.Map, blockentity.Key);
            //    blockentity.Value.Instantiate(blockentity.Key.ToGlobal(chunk), Instance.Instantiator);
            //}
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
            //obj.Despawn(Instance);
            obj.Despawn();
        }
        public void Despawn(int netID)
        {
            GameObject entity;
            if (this.NetworkObjects.TryGetValue(netID, out entity))
                //this.Despawn(entity);
                entity.Despawn();
        }

        public void Spawn(GameObject obj)
        {
            obj.Parent = null;
            //obj.Net = this;
            obj.Map = this.Map;
            SpawnObject(obj);
        }
        public void Spawn(GameObject obj, Vector3 global)
        {
            obj.Parent = null;
            //obj.Net = this;
            obj.Map = this.Map;
            obj.Global = global;
            //this.Spawn(obj, new Position(this.Map, global));
            this.SpawnObject(obj);
        }
        public void Spawn(GameObject obj, WorldPosition pos)
        {
            obj.Parent = null;
            SpawnObject(obj);
        }
        public void Spawn(GameObject obj, GameObjectSlot slot)
        {

        }
        void SpawnObject(GameObject obj)
        {
            if (obj.RefID == 0)
                return;
            //obj.Parent = null;
            obj.Spawn(Instance);
        }
        public void Spawn(GameObject obj, GameObject parent, int childID)
        {
            //var slot = parent.GetChildren()[childID];
            //if (slot.HasValue)
            //    if (slot.Object.ID == obj.ID)
            //    {
            //        slot.Insert(obj.ToSlot());
            //        return;
            //    }
        }
        public void SyncSlotInsert(GameObjectSlot slot, GameObject obj) { }

        public void AddObject(GameObject obj, Vector3 global)
        {
            Packet.Create(this.PacketID, PacketType.InstantiateAndSpawnObject, Network.Serialize(w =>
            {
                obj.Write(w);
                //w.Write(global);
                Position.Write(w, global, Vector3.Zero);
            })).BeginSendTo(Host, RemoteIP);
        }
        public void AddObject(GameObject obj, Vector3 global, Vector3 speed)
        {
            obj.SetGlobal(global).Velocity = speed;
            Packet.Create(this.PacketID, PacketType.InstantiateObject, Network.Serialize(obj.Write)).BeginSendTo(Host, RemoteIP);
        }
        public void RemoveObject(GameObject obj)
        {
            if (obj == null)
                throw new ArgumentNullException();

            if (obj.IsPlayerEntity())
                return;
            //Packet.Create(PacketID, PacketType.ObjectDestroy, Network.Serialize(obj.Write)).BeginSendTo(Host, RemoteIP);
            Packet.Create(this.PacketID, PacketType.DisposeObject, Network.Serialize(w => TargetArgs.Write(w, obj))).BeginSendTo(Host, RemoteIP);
        }


        public void InstantiateInContainer(GameObject obj, GameObject container, byte containerID, byte slotID, byte amount)
        { }
        public void InstantiateInContainer(GameObject input, Vector3 blockentity, byte slotID)
        {
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
        //public List<GameObject> GetNetworkObjects(params int[] netID)
        //{
        //    return (from o in this.NetworkObjects where netID.Contains(o.Key) select o.Value).ToList();
        //}
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

 
        readonly HashSet<int> ChangedObjects = new();
        /// <summary>
        /// find way to write specific changes, maybe by passing a state Object
        /// </summary>
        /// <param name="netID"></param>
        /// <returns></returns>
        public bool LogStateChange(int netID) //{ return false; }
        {
            //if (netID == PlayerData.CharacterID)
            //    if (SaveCharacterFlag == PlayerSavingState.Saved)
            //        SaveCharacterFlag = PlayerSavingState.Changed;
            return false;
            //return ChangedObjects.Add(netID);
        }

        void ReadSnapshotOld(BinaryReader reader)
        {
            double totalMs = reader.ReadDouble();
            TimeSpan time = TimeSpan.FromMilliseconds(totalMs);
            WorldSnapshot worldState = new WorldSnapshot() { Time = time };
            /*
            if (WorldStateBuffer.Count > 0)
                if (worldState.Time < WorldStateBuffer.Last().Time)
                {
                    //throw new Exception("snapshot received is older than most recent one");
                    //("snapshot dropped, is older than most recent one").ToConsole();
                    if (worldState.EventSnapshots.Count > 0)
                    {
                        Console.Write(Color.Orange, "CLIENT", "Executing " + worldState.EventSnapshots.Count + " events from outdated world snapshot: " + worldState.ToString());
                        ExecuteEvents(worldState);
                    }
                    return;
                }
            */
            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                GameObject obj;
                int netID = reader.ReadInt32();
                if (Server.Instance.GetNetworkObject(netID) == this.GetNetworkObject(netID))
                    throw new Exception("TI STO DIAOLO RE MALAKA GAMW TO XRISTO");
                if (!this.TryGetNetworkObject(netID, out obj))
                {
                    //throw new Exception("Networked object doesn't exist on client");
                    Log.Write(Color.Orange, "CLIENT", "Networked object doesn't exist on client"); // SOMETIMES THIS HAPPENS WHEN A SNAPSHOT IS RECEIVED CONTAINED A NEWLY INSTANTIATED ITEM, BEFORE THE PACKET THAT INSTANTIATES IT ARRIVES
                    continue;
                }
                worldState.ObjectSnapshots.Add(ObjectSnapshot.Create(time, obj, reader));
            }

            /*
            // read events
            int eventsCount = reader.ReadInt32();
            for (int i = 0; i < eventsCount; i++)
            {
                worldState.EventSnapshots.Enqueue(EventSnapshot.Read(time, this, reader));
            }
            */

            // insert world snapshot to world snapshot history
            WorldStateBuffer.Enqueue(worldState);
            //   Network.Console.Write(worldState.ToString());
            while (WorldStateBuffer.Count > WorldStateBufferSize)
            {
                _ = WorldStateBuffer.Dequeue();
                //if (discarded.EventSnapshots.Count > 0)
                //    Client.Console.Write(Color.Yellow, "CLIENT", "WARNING! " + discarded.EventSnapshots.Count + " events discarded with world snapshot " + discarded.ToString());
            }
        }

        //void ReadSnapshot(byte[] data)
        //{
        //    data.Translate(reader =>
        //    {
        //        ReadSnapshot(reader);
        //    });
        //}

        internal void ReadSnapshot(BinaryReader reader)
        {
            double totalMs = reader.ReadDouble();
            //SyncTime(totalMs); // attempt to sync time here instead of receiving separate synctime packets from server
            // i moved this to when receinving unreliable packet each tick
            // WARNING! in case of no current entities in map, it will never sync! sync time from incoming packets

            TimeSpan time = TimeSpan.FromMilliseconds(totalMs);
            WorldSnapshot worldState = new WorldSnapshot() { Time = time };

            int count = reader.ReadInt32();
            for (int i = 0; i < count; i++)
            {
                GameObject obj;
                int netID = reader.ReadInt32();
                obj = Instance.NetworkObjects.GetValueOrDefault(netID);
                var objsnapshot = ObjectSnapshot.Create(time, obj, reader);
                //if (Server.Instance.GetNetworkObject(netID) == this.GetNetworkObject(netID))
                //    throw new Exception("TI STO DIAOLO");
                //if (!this.TryGetNetworkObject(netID, out obj))
                if (obj == null)
                {
                    //throw new Exception("Networked object doesn't exist on client");
                    // SOMETIMES THIS HAPPENS WHEN A SNAPSHOT IS RECEIVED CONTAINED A NEWLY INSTANTIATED ITEM, BEFORE THE PACKET THAT INSTANTIATES IT ARRIVES
                    //Console.Write(Color.Orange, "CLIENT", "Networked object doesn't exist on client");   
                    //(netID.ToString() + ": Networked object doesn't exist on client").ToConsole();
                    continue;
                }
                worldState.ObjectSnapshots.Add(objsnapshot);
            }

            // insert world snapshot to world snapshot history
            WorldStateBuffer.Enqueue(worldState);
            //   Network.Console.Write(worldState.ToString());
            while (WorldStateBuffer.Count > WorldStateBufferSize)
            {
                WorldSnapshot discarded = WorldStateBuffer.Dequeue();
                //if (discarded.EventSnapshots.Count > 0)
                //    Client.Console.Write(Color.Yellow, "CLIENT", "WARNING! " + discarded.EventSnapshots.Count + " events discarded with world snapshot " + discarded.ToString());
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


                //ExecuteEvents(prev); // TODO: find better way to execute missed events

                //if (prev.Time <= ClientClock &&
                //    ClientClock < next.Time)
                var prevTime = next.Time - TimeSpan.FromMilliseconds(Server.ClockIntervalMS);
                // temporarily removed interpolation
                //if (prevTime <= ClientClock &&
                //    ClientClock < next.Time)
                //{
                //    LerpObjectPositions(prev, next);
                //    //ExecuteEvents(next);
                //    return;
                //}
                //else 
                if (this.ClientClock == next.Time)
                {
                    SnapObjectPositions(prev, next);
                    //ExecuteEvents(next);
                    return;
                }
                //else
                //    throw new Exception();
                //// execute missed events from last snapshot
                //if (i + 1 == WorldStateBuffer.Count)
                //    if (next.EventSnapshots.Count > 0)
                //    {
                //        Network.Console.Write("WARNING! Executing " + next.EventSnapshots.Count + " events from last snapshot!");
                //        ExecuteEvents(next);
                //    }
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
                //objSnapshot.Object.ChangePosition(Engine.Map, objSnapshot.Position);
                objSnapshot.Object.Velocity = objSnapshot.Velocity;
                //("snaped to " + next.ToString()).ToConsole();
                objSnapshot.Object.Direction = objSnapshot.Orientation;
                if (float.IsNaN(objSnapshot.Object.Direction.X) || float.IsNaN(objSnapshot.Object.Direction.Y))
                    throw new Exception();
            }
        }


        private void LerpObjectPositions(WorldSnapshot prev, WorldSnapshot next)
        {
            //interpolate between states, apply and return
            foreach (var nextObjSnapshot in next.ObjectSnapshots)
            {
                // only update objects that have a snapshot in the next worldstate
                // (don't update objects that their state change stopped in the previous snapshot)

                // continue if there's not an initial value to interpolate from (the object will start changing at the next snapshot)
                // TODO: find a way to not have to search for objects
                ObjectSnapshot previousObjState = prev.ObjectSnapshots.Find(o => o.Object == nextObjSnapshot.Object);

                if (previousObjState == null)
                    continue;

                // smooth error in prediction
                // for objects that have moved locally (client prediction) before the server update arrived
                // get current (predicted) object state
                //  ObjectSnapshot predicted = new ObjectSnapshot(objSnapshot.Object) { Time = previousObjState.Time };

                // get interpolated state
                // maybe use something more precise than clientclock, since it ticks at the same rate as the server sends snapshots???
                ObjectSnapshot interpolatedState = previousObjState.Interpolate(nextObjSnapshot, this.ClientClock);

                //   ObjectSnapshot afterSmoothing = predicted.Interpolate(interpolatedState, ClientClock);


                // apply values
                //objSnapshot.Object.Global = interpolatedState.Position;
                //objSnapshot.Object.Velocity = interpolatedState.Velocity;

                // smooth between predicted and actual values
                // correct if outside error margin
                float errorMargin = 0;// .01f;// 0.5f;
                if (Vector3.Distance(nextObjSnapshot.Object.Global, interpolatedState.Position) < errorMargin)
                {
                    nextObjSnapshot.Object.ChangePosition(nextObjSnapshot.Object.Global + (interpolatedState.Position - nextObjSnapshot.Object.Global) * SnapshotSmoothing);
                    //objSnapshot.Object.ChangePosition(Engine.Map, objSnapshot.Object.Global + (interpolatedState.Position - objSnapshot.Object.Global) * 0.3f);
                    nextObjSnapshot.Object.Velocity += (interpolatedState.Velocity - nextObjSnapshot.Object.Velocity) * SnapshotSmoothing; // apparently i don't need to sync velocity // EDIT: I DO, I DONT SYNC DIRECTION ANYMORE THRU PACKETS
                    //nextObjSnapshot.Object.Direction += (interpolatedState.Orientation - nextObjSnapshot.Object.Direction) * SnapshotSmoothing;
                    // we don't care about direction smoothing
                }
                else
                {
                    if (interpolatedState.Position == nextObjSnapshot.Object.Global)
                         "ti diaolo".ToConsole();
                    nextObjSnapshot.Object.ChangePosition(interpolatedState.Position);
                    //objSnapshot.Object.ChangePosition(Engine.Map, interpolatedState.Position);
                   // nextObjSnapshot.Object.Velocity = interpolatedState.Velocity;
                    // we don't care about direction smoothing
                    //nextObjSnapshot.Object.Direction = interpolatedState.Orientation;
                }
                nextObjSnapshot.Object.Direction = nextObjSnapshot.Orientation;

                if (float.IsNaN(nextObjSnapshot.Object.Direction.X) || float.IsNaN(nextObjSnapshot.Object.Direction.Y))
                    throw new Exception();
            }
        }
        const float SnapshotSmoothing = .3f;//.5f;//

        static public void PostPlayerInput(Components.Message.Types type, bool clientPrediction = true)
        {
            PostPlayerInput(type, w => { }, clientPrediction);
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
        static public void PlayerInventoryOperation(GameObject inventoryOwner, GameObjectSlot source, GameObjectSlot destination, int amount)
        {
            Network.Serialize(writer =>
            {
                TargetArgs.Write(writer, inventoryOwner);
                TargetArgs.Write(writer, source);
                TargetArgs.Write(writer, destination);
                writer.Write(amount);
            }).Send(Instance.PacketID, PacketType.PlayerInventoryOperation, Instance.Host, Instance.RemoteIP);
        }
        static public void PlayerInventoryOperationNew(GameObjectSlot sourceSlot, GameObjectSlot destinationSlot, int amount)
        {
            Network.Serialize(writer =>
            {
                //TargetArgs.Write(writer, sourceParent);
                //TargetArgs.Write(writer, destParent);
                TargetArgs.Write(writer, sourceSlot);
                TargetArgs.Write(writer, destinationSlot);
                writer.Write(amount);
            }).Send(Instance.PacketID, PacketType.PlayerInventoryOperationNew, Instance.Host, Instance.RemoteIP);
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
        static public void PlayerInventoryOperationOld(GameObject inventoryOwner, Action<BinaryWriter> dataWriter)
        {
            Network.Serialize(writer =>
            {
                writer.Write(Instance.ClientClock.TotalMilliseconds);
                TargetArgs.Write(writer, inventoryOwner);
                dataWriter(writer);
            }).Send(Instance.PacketID, PacketType.PlayerInventoryOperationOld, Instance.Host, Instance.RemoteIP);
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
            //if (clientPrediction)
            //    Player.Actor.PostMessageLocal(type, Player.Actor, Client.Instance, dataWriter);
            Network.Serialize(writer =>
            {
                writer.Write(Instance.ClientClock.TotalMilliseconds);
                TargetArgs.Write(writer, recipient);
                ObjectEventArgs.Write(writer, type, dataWriter);
                //writer.Write(ClientClock.TotalMilliseconds);
                //writer.Write((byte)type);
                //TargetArgs.Write(writer, recipient);
                //dataWriter(writer);
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
        internal static void PlayerRemoveBlock(Vector3 global)
        {
            Network.Serialize(w =>
            {
                w.Write(global);
            }).Send(Instance.PacketID, PacketType.PlayerRemoveBlock, Instance.Host, Instance.RemoteIP);
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
            //Player.Actor.Global.ToConsole();

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
        
        static public void PlayerBuild(Components.Crafting.Reaction.Product.ProductMaterialPair product, Vector3 global)
        {
            Packet.Create(Instance.PacketID, PacketType.PlaceConstruction, Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                product.Write(w);
                w.Write(global);
            })).BeginSendTo(Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerCraft(Components.Crafting.Reaction.Product.ProductMaterialPair product)
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                product.Write(w);
            }).Send(Instance.PacketID, PacketType.PlayerCraftRequest, Instance.Host, Instance.RemoteIP);
        }
        internal static void PlayerCraftRequest(Components.Crafting.CraftOperation crafting)
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                crafting.WriteOld(w);
            }).Send(Instance.PacketID, PacketType.PlayerCraftRequest, Instance.Host, Instance.RemoteIP);
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
        //internal static void PlayerRemoteCall(GameObject target, Message.Types types)
        internal static void PlayerRemoteCall(TargetArgs target, Message.Types types)
        {
            Network.Serialize(w =>
            {
                w.Write(PlayerOld.Actor.RefID);
                target.Write(w);
                w.Write((int)types);
            }).Send(Instance.PacketID, PacketType.PlayerRemoteCall, Instance.Host, Instance.RemoteIP);
        }
        //internal static void PlayerRemoteCall(GameObject target, Message.Types type, Action<BinaryWriter> argsWriter)
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
            //Packet.Create(Client.PacketID, PacketType.PlayerServerCommand, Net.Network.Serialize(writer =>
            //{
            //    writer.WriteASCII(gotText.TrimStart('/'));
            //})).BeginSendTo(Client.Host, Client.RemoteIP);

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

        public void Send(PacketType packetType)
        {
            this.Send(packetType, new byte[] { });
        }
        public void Send(PacketType packetType, byte[] data)
        {
            //data.Compress().Send(PacketID, packetType, Host, RemoteIP);
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


        public void PopLoot(GameObject obj) { }
        public void PopLoot(GameObject obj, GameObject parent)
        {
            //obj.Global = parent.Global;
            //obj.Velocity = parent.Velocity;
        }
        public void PopLoot(LootTable table, GameObject parent) { }
        //public void PopLoot(GameObject parent, GameObject obj) { }
        public void PopLoot(GameObject loot, Vector3 startPosition, Vector3 startVelocity) { }
        public void PopLoot(LootTable table, Vector3 startPosition, Vector3 startVelocity) { }
        public List<GameObject> GenerateLoot(LootTable loot) { return new List<GameObject>(); }

        internal static void RequestNewObject(GameObject gameObject, byte amount)
        {
            Packet.Create(Instance.PacketID, PacketType.RequestNewObject, Network.Serialize(w =>
            {
                //TargetArgs.Write(w, gameObject);
                gameObject.Clone().Write(w);
                w.Write(amount);
            })).BeginSendTo(Instance.Host, Instance.RemoteIP);
        }
        internal static void Request(PacketType type, Action<BinaryWriter> writer)
        {
            Packet.Create(Instance.PacketID, type, Network.Serialize(w =>
            {
                writer(w);
            })).BeginSendTo(Instance.Host, Instance.RemoteIP);
        }

        public void UpdateLight(Vector3 global) { }
        public bool LogLightChange(Vector3 global) { return false; }

        public void TryGetRandomValue(Action<double> action) { }
        public void TryGetRandomValue(int min, int max, Action<int> action) { }
        public bool TryGetRandomValue(int min, int max, out int rand) { rand = 0; return false; }
        public void RandomEvent(GameObject target, ObjectEventArgs a, Action<double> rnEvent)
        {
            //RandomObjectEventArgs e = a as RandomObjectEventArgs;
            //if (e.IsNull())
            //    return;
            //rnEvent(e.Value);
        }
        public void RandomEvent(TargetArgs target, ObjectEventArgs a, Action<double> rnEvent)
        {
            //RandomObjectEventArgs e = a as RandomObjectEventArgs;
            //if (e.IsNull())
            //    return;
            //rnEvent(e.Value);
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

            this.Map.GetChunk(sourceParent.Global).Invalidate();
            this.Map.GetChunk(destinationParent.Global).Invalidate();

            var obj = sourceSlot.Object;
            if (!targetSlot.HasValue) // if target slot empty, set object of target slot without swapping and return
            {
                if (amount < sourceSlot.StackSize) // if the amount moved is smaller than the source amount
                {
                    //obj = sourceSlot.Object.Clone();
                    //obj.GetInfo().StackSize = amount;
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
        public void InventoryOperationOld(GameObjectSlot source, GameObjectSlot destination, int amount)
        {
            var sourceParent = source.Parent;
            var destinationParent = destination.Parent;
            if (destination == source)
                return;
            if (!destination.Filter(source.Object))
                return;
            this.Map.GetChunk(sourceParent.Global).Invalidate();
            this.Map.GetChunk(destinationParent.Global).Invalidate();
            Network.InventoryOperation(this, source, destination, amount);
        }
        void InventoryOperation(GameObject parent, Components.ArrangeChildrenArgs args)
        {
            GameObject sourceObj = args.Object.Object;
            //GameObjectSlot targetSlot, sourceSlot;
            //if (!args.SourceEntity.Object.TryGetChild(args.TargetSlotID, out targetSlot) ||
            //    !parent.TryGetChild(args.SourceSlotID, out sourceSlot))
            if (!parent.TryGetChild(args.TargetSlotID, out GameObjectSlot targetSlot) ||
                !args.SourceEntity.Object.TryGetChild(args.SourceSlotID, out GameObjectSlot sourceSlot))
                return;
            if (targetSlot == sourceSlot)
                return;

            int amount = args.Amount;

            if (sourceObj.IsNull())
            {
                // we are at client so discard object if not instantiated
                return;
            }
            Network.InventoryOperation(this, sourceObj, targetSlot, sourceSlot, amount);
            //if (!targetSlot.HasValue) // if target slot empty, set object of target slot without swapping and return
            //{
            //    //targetSlot.Set(sourceObj, amount);
            //    if (targetSlot.Set(sourceObj, amount))
            //        sourceSlot.StackSize -= amount;

            //    return;
            //}
            //if (sourceSlot.Object.ID == targetSlot.Object.ID)
            //{
            //    if (sourceSlot.StackSize + targetSlot.StackSize <= targetSlot.StackMax)
            //    {
            //        targetSlot.StackSize += sourceSlot.StackSize;
            //        DisposeObject(sourceSlot.Object.NetworkID);
            //        sourceSlot.Clear();
            //        //merge slots
            //        return;
            //    }
            //}
            //else
            //    if (amount < sourceSlot.StackSize)
            //        return;

            //targetSlot.Swap(sourceSlot);
        }

        public void SyncSetBlock(Vector3 global, Block.Types type)
        {
        }
        public void SyncSetBlock(Vector3 global, Block.Types type, byte data, int orientation)
        {
        }
        public void SetBlock(Vector3 global, Block.Types type)
        {
            // set block locally until the sync from server is received
            if (!global.TrySetCell(this, type))
                throw new Exception("Invalid cell position");
        }
        public void SetBlock(Vector3 global, Block.Types type, byte data)
        {
            if (!global.TrySetCell(this, type))
                throw new Exception("Invalid cell position");
        }
        public void UpdateBlock(Vector3 global, Action<Cell> updater)
        {
        }

        public void SpreadBlockLight(Vector3 global)
        {

        }

        public void SendBlockMessage(Vector3 global, Components.Message.Types msg, params object[] parameters)
        {
            Block.HandleMessage(this, global, ObjectEventArgs.Create(this, msg, parameters));
        }

        public void UnloadChunk(Vector2 chunkPos)
        {
            if (!Instance.Map.GetActiveChunks().TryGetValue(chunkPos, out Chunk chunk))
                return;
            //if (!Instance.Map.ActiveChunks.TryRemove(chunkPos, out chunk))
            //return;
            Instance.Map.GetActiveChunks().Remove(chunkPos);

            foreach (var obj in chunk.GetObjects())
                //Sync // don't sync dispose cause client will dispose object when it receives the unload chunk packet
                this.DisposeObject(obj);
            if (chunkPos == new Vector2(-1, 1))
                (chunkPos.ToString() + " unloaded, player chunk: " + PlayerOld.Actor.Global.GetChunkCoords().ToString()).ToConsole();
        }



        public static GameObjectSlot sourceSlot { get; set; }

        public static GameObjectSlot destinationSlot { get; set; }

        public void Forward(Packet p)
        {

        }


        public static bool IsSaving;

        public PlayerData GetPlayer(int id)
        {
            //return Players.GetList().First(p => p.ID == id);
            return Players.GetPlayer(id);// .GetList().FirstOrDefault(p => p.ID == id);
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

        //[Obsolete]
        //internal void UpdateMousePosition(int playerid, Vector2 camerapos, float camerazoom, Vector2 mousepos, TargetArgs target)
        //{
        //    var player = this.GetPlayer(playerid);
        //    if (player == null)
        //        return;
        //    player.CameraPosition = camerapos;
        //    player.CameraZoom = camerazoom;
        //    player.MousePosition = mousepos;
        //    player.Target = target;
        //}
        //internal void UpdateMousePosition(int playerid, TargetArgs target)
        //{
        //    var player = this.GetPlayer(playerid);
        //    if (player == null)
        //        return;
        //    player.Target = target;
        //}
        internal void HandleServerResponse(int playerID, PlayerList playerList, int speed)
        {
            throw new Exception();
            //PlayerData.ID = playerID;
            //Players = playerList;
            //this.Speed = speed;
            //Console.Write(Color.Lime, "CLIENT", "Connected to " + RemoteIP.ToString());
            //GameMode.Current.PlayerIDAssigned(Instance);
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
        public void Write(Log.EntryTypes type, string text)
        {
            Rooms.Ingame.Instance.Hud.Chat.Write(type, text);
        }
        public void Report(string text)
        {
            //this.Log.Write("CLIENT", text);
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
