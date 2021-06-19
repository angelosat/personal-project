using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
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
        static UI.ConsoleBoxAsync _Console;
        public static UI.ConsoleBoxAsync Console
        {
            get
            {
                if (_Console.IsNull())
                    _Console = new UI.ConsoleBoxAsync(UI.LobbyWindow.Instance.Console.Size) { FadeText = false }; ;
                return _Console;
            }
        }
        public UI.ConsoleBoxAsync GetConsole()
        {
            return Console;
        }

        static long _packetID = 1;
        public static long PacketID
        {
            get { return _packetID++; }
        }
        static long RemoteSequence = 0;
        static public long RemoteOrderedReliableSequence = 0;
        ConcurrentDictionary<int, GameObject> NetworkObjects;
        public IMap Map { set { Engine.Map = value; } get { return Engine.Map; } }
        //public Map GetMap()
        //{
        //    return Engine.Map;
        //}
        static Client _Instance;
        static public Client Instance
        {
            get
            {
                if (_Instance.IsNull())
                    _Instance = new Client();
                return _Instance;
            }
        }
        Client()
        {
            // NONONO THIS WAS WRONG, UPDATE CLOCK ON UPDATE, NOT ON SEPARATE THREAD!!!
            //ClockTimer = new System.Threading.Timer((a) =>
            //{
            //    ClientClock = ClientClock.Add(TimeSpan.FromMilliseconds(Server.ClockIntervalMS));

            //    //ExecuteEvents();
            //    //CurrentTime.ToConsole();
            //}, new object(), 0, Server.ClockIntervalMS);

            PlayerSaveTimer = new Timer(a =>
            {
                // todo: check if Monitor class is better for this job
                PlayerSaveTimer.Change(Timeout.Infinite, Timeout.Infinite);
                //SavePlayerCharacter(); // let gamemode save player character
                PlayerSaveTimer.Change(PlayerSaveInterval, PlayerSaveInterval);
                //"player saved".ToConsole();
            }, new object(), Timeout.Infinite, Timeout.Infinite);//PlayerSaveInterval, PlayerSaveInterval);

            NetworkObjects = new ConcurrentDictionary<int, GameObject>();
        }
        public NetworkSideType Type { get { return NetworkSideType.Local; } }

        static readonly int TimeoutLength = Engine.TargetFps * 2;//5000;
        static int TimeoutTimer = -1;

        static System.Threading.Timer ClockTimer;
        static Timer PlayerSaveTimer;
        static int PlayerSaveInterval = 1000;

        static public PlayerData PlayerData;
        static public Socket Host;
        static public EndPoint RemoteIP;
        static public PlayerList Players;
        static PacketTransfer PartialPacketReceiver = new PacketTransfer(HandleMessage);
        static ConcurrentQueue<Packet> IncomingAll = new ConcurrentQueue<Packet>();
        static PriorityQueue<long, Packet> IncomingOrdered = new PriorityQueue<long, Packet>();
        static PriorityQueue<long, Packet> IncomingOrderedReliable = new PriorityQueue<long, Packet>();
        static PriorityQueue<long, Packet> IncomingSynced = new PriorityQueue<long, Packet>();
        static ConcurrentDictionary<Vector2, ConcurrentQueue<Action<Chunk>>> ChunkCallBackEvents;
        static TimeSpan ClientClock = new TimeSpan();
        static double LastReceivedTime = int.MinValue;

        public TimeSpan Clock { get { return ClientClock; } }

        Dictionary<Vector2, ChunkTransfer> PartialChunks = new Dictionary<Vector2, ChunkTransfer>();

        static Queue<WorldSnapshot> WorldStateBuffer = new Queue<WorldSnapshot>();
        //    static PlayerSavingState SaveCharacterFlag = PlayerSavingState.Saved;
        static int WorldStateBufferSize = 5;
        const int ClientClockDelayMS = Server.SnapshotIntervalMS * 4;//2;

        public void EnterWorld(GameObject playerCharacter)
        {
            Packet.Create(PacketID, PacketType.PlayerEnterWorld, Net.Network.Serialize(playerCharacter.Write))
                .BeginSendTo(Host, RemoteIP, (a) => { });
        }

        public static void Disconnect()
        {
            Instance.World = null;
            Engine.Map = null;
            TimeoutTimer = -1;
            Instance.NetworkObjects = new ConcurrentDictionary<int, GameObject>();
            Packet.Create(PacketID, PacketType.PlayerDisconnected).BeginSendTo(Host, RemoteIP, a => { });
            if (PlayerSaveTimer != null)
                PlayerSaveTimer.Change(Timeout.Infinite, Timeout.Infinite);
        }

        /// <summary>
        /// Called when communication with server times out
        /// </summary>
        private static void Disconnected()
        {
            TimeoutTimer = -1;
            Instance.World = null;
            Engine.Map = null;
            Instance.NetworkObjects = new ConcurrentDictionary<int, GameObject>();
            PlayerSaveTimer.Change(Timeout.Infinite, Timeout.Infinite);
            //ScreenManager.Remove();
            ScreenManager.GameScreens.Clear();
            ScreenManager.Add(Rooms.MainScreen.Instance);

            //UI.MessageBox.Create("Warning!", "Disconnected from server", "Ok", () => { }).ShowDialog();
            //new UI.MessageBox("Warning!", "Disconnected from server", new UI.Button("Ok") { LeftClickAction = () => { } }).ShowDialog();
            new UI.MessageBox("Warning!", "Disconnected from server", new ContextAction(() => "Ok", () => { })).ShowDialog();
        }

        //static public void Exit()  
        //{
        //    World = null;
        //    Engine.Map = null;
        //    Network.Serialize(w =>
        //    {
        //        w.Write(PlayerData.ID);
        //    }).Send(PacketID, PacketType.PlayerExitWorld, Host, RemoteIP);
        //}

        static public void Connect(string address, string playername, AsyncCallback callBack)
        {
            Connect(address, new PlayerData(playername), callBack);
        }
        static public void Connect(string address, PlayerData playerData, AsyncCallback callBack)
        {
            //Console = new UI.ConsoleBox(UI.LobbyWindow.Instance.Console.Size) { FadeText = false };
            ChunkCallBackEvents = new ConcurrentDictionary<Vector2, ConcurrentQueue<Action<Chunk>>>();
            RecentPackets = new Queue<long>();
            RemoteSequence = 0;

            PlayerData = playerData;

            Players = new PlayerList();
            if (Host != null)
                Host.Close();
            Host = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Host.ReceiveBufferSize = Host.SendBufferSize = Packet.Size;
            RemoteIP = new IPEndPoint(IPAddress.Parse(address), 5541);
            UdpConnection state = new UdpConnection("Server", Host) { Buffer = new byte[Packet.Size] };
            Host.Bind(new IPEndPoint(IPAddress.Any, 0));

            byte[] data = Packet.Create(PacketID, PacketType.RequestPlayerID, Network.Serialize(w =>
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

        static void SendPlayerData(PlayerData player)
        {
            byte[] msg = Packet.Create(PacketID, PacketType.RequestPlayerID, Network.Serialize(player.Write)).ToArray();
            Host.Send(msg);
        }

        static void ReceivePlayerList(byte[] data)
        {
            Players = Network.Deserialize<PlayerList>(data, PlayerList.Read);
        }

        public List<PlayerData> GetPlayers()
        {
            return Players.GetList();
        }

        static public void Update()
        {
            //UnloadChunks();
            if (TimeoutTimer == 0)
                Disconnected();
            TimeoutTimer--;
            ProcessSyncedPackets();
            ProcessIncomingPackets();
            //UpdateWorldState();
            HandleOrderedPackets();
            HandleOrderedReliablePackets();
            if (GameMode.Current != null)
                GameMode.Current.PacketHandler.Update(Instance);
            UpdateWorldState();
            ProcessEvents();
            ClientClock = ClientClock.Add(TimeSpan.FromMilliseconds(Server.ClockIntervalMS));

        }

        private static void UnloadChunks()
        {
            if (Player.Actor.IsNull())
                return;
            if (Instance.Map.IsNull())
                return;
            var playerchunk = Player.Actor.Global.GetChunkCoords();
            foreach (var chunk in Instance.Map.GetActiveChunks().Values.ToList())
                if (Vector2.Distance(chunk.MapCoords, playerchunk) > Engine.ChunkRadius)
                {
                    // if -1 it means timer is stopped and chunk isn't unloaded. do we need this? the timer only counts down if it's out of range
                    //if (chunk.Value.UnloadTimer > 0)
                    chunk.UnloadTimer--;
                    if (chunk.UnloadTimer <= 0)
                        Instance.UnloadChunk(chunk.MapCoords);
                }
        }

        private static void ProcessEvents()
        {
            while (EventQueue.Count > 0)
            {
                GameEvent e = EventQueue.Dequeue();
                OnGameEvent(e);
            }
        }
        public event EventHandler<GameEvent> GameEvent;
        static void OnGameEvent(GameEvent e)
        {
            //GameMode.Sandbox.ClientEventHandler.HandleEvent(Instance, e);
            GameMode.Current.ClientEventHandler.HandleEvent(Instance, e);

            foreach (var item in Game1.Instance.GameComponents) //GetGameComponents())//.
                item.OnGameEvent(e);
            UI.TooltipManager.OnGameEvent(e);
            ScreenManager.CurrentScreen.WindowManager.OnGameEvent(e);
            PlayerControl.ToolManager.OnGameEvent(e);
            Instance.Map.OnGameEvent(e);
            if (!Instance.GameEvent.IsNull())
                Instance.GameEvent(null, e);
        }


        Dictionary<PacketType, IClientPacketHandler> PacketHandlers = new Dictionary<PacketType, IClientPacketHandler>();
        public void RegisterPacketHandler(PacketType channel, IClientPacketHandler handler)
        {
            this.PacketHandlers.Add(channel, handler);
        }
        //Dictionary<PacketType, Action<IObjectProvider, BinaryReader>> Packets = new Dictionary<PacketType, Action<IObjectProvider, BinaryReader>>();
        //public void RegisterPacket(PacketType channel, Action<IObjectProvider, BinaryReader> handler)
        //{
        //    this.Packets.Add(channel, handler);
        //}
        Dictionary<PacketType, Action<Client, Packet>> Packets = new Dictionary<PacketType, Action<Client, Packet>>();
        public void RegisterPacket(PacketType channel, Action<Client, Packet> handler)
        {
            this.Packets.Add(channel, handler);
        }

        static Queue<GameEvent> EventQueue = new Queue<GameEvent>();
        //public void EventOccured(GameObject sender, ObjectEventArgs args)
        //{
        //    ObjectEvent e = new ObjectEvent(ClientClock.TotalMilliseconds, sender, args);
        //    EventQueue.Enqueue(e);
        //}
        public void EventOccured(Components.Message.Types type, params object[] p)
        {
            GameEvent e = new GameEvent(this, ClientClock.TotalMilliseconds, type, p);
            EventQueue.Enqueue(e);
        }

        //Dictionary<PacketType, IPacketHandler> PacketHandlers = new Dictionary<PacketType, Action<byte[]>>{
        //    {PacketType.Snapshot, Instance.ReadSnapshot},
        //    {PacketType.Chunk, Instance.ReceiveChunk}
        //};
        private static void ProcessSyncedPackets()
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
        private static void ProcessIncomingPackets()
        {
            Packet packet;
            //IncomingAll.Count.ToConsole();
            while (IncomingAll.TryDequeue(out packet))
            {
                if (packet.PacketType == PacketType.Chunk)
                    (DateTime.Now.ToString() + " " + packet.PacketType.ToString() + " dequeued").ToConsole();
                //(DateTime.Now.ToString() + " " + packet.PacketType.ToString() + " dequeued").ToConsole();

                // if the timer is not stopped (not -1), reset it
                if (TimeoutTimer > -1)
                    TimeoutTimer = TimeoutLength;

                if (IsDuplicate(packet))
                {
                    //  "dropped duplicate packet".ToConsole();
                    continue;
                }
                RecentPackets.Enqueue(packet.ID);
                if (RecentPackets.Count > RecentPacketBufferSize)
                    RecentPackets.Dequeue();

                // for ordered packets, only handle last one (store most recent and discard and older ones)
                //if ((packet.SendType & SendType.Ordered) == SendType.Ordered)
                if (packet.SendType == SendType.Ordered)
                {
                    IncomingOrdered.Enqueue(packet.ID, packet);//e);
                    //("enqueued " + packet.ToString()).ToConsole();
                }
                else if (packet.SendType == SendType.OrderedReliable)
                {
                    //packet.PacketType.ToConsole();
                        IncomingOrderedReliable.Enqueue(packet.OrderedReliableID, packet);
                }
                else
                {
                    //if (packet.PacketType == PacketType.Chunk)
                    //    "handling chunk packet".ToConsole();
                    HandleMessage(packet);
                    //if (packet.PacketType != PacketType.Ping && packet.PacketType != PacketType.SyncTime)
                    //    packet.PacketType.ToConsole();
                }
            }
        }

        public static void SavePlayerCharacter()
        {
            string fullpath = "", filename = "", savefolder = "";
            try
            {

                string working = Directory.GetCurrentDirectory();
                savefolder = @"\Saves\Characters\";
                DirectoryInfo directory = new DirectoryInfo(working + savefolder);
                if (!Directory.Exists(directory.FullName))
                    Directory.CreateDirectory(directory.FullName);

                GameObject actor;// = Player.Actor.Clone();
                if (!Instance.TryGetNetworkObject(PlayerData.CharacterID, out actor))
                    return;
                actor = actor.Clone();


                filename = actor.Name + ".character.sat";
                string tempFile = "_" + filename;

                using (MemoryStream stream = new MemoryStream())
                {
                    BinaryWriter writer = new BinaryWriter(stream);

                    SavePlayer(actor, writer);
                    Chunk.Compress(stream, directory + tempFile);
                }

                fullpath = working + savefolder;
                if (File.Exists(fullpath + filename))
                    File.Replace(fullpath + tempFile, fullpath + filename, fullpath + filename + ".bak");
                else
                    File.Move(fullpath + tempFile, fullpath + filename);

            }
            catch (Exception e)
            {
                Net.Client.Console.Write(Color.Orange, "CLIENT", "Error saving character (" + e.Message + ")");
            }
        }

        public static void SavePlayer(GameObject actor, BinaryWriter writer)
        {
            SaveTag tag = new SaveTag(SaveTag.Types.Compound, "Character");

            SaveTag charTag = new SaveTag(Start_a_Town_.SaveTag.Types.Compound, "PlayerCharacter", actor.Save());

            // save metadata such as hotbar
            //SaveTag hotbarTag = UI.Hud.Instance.HotBar.Save();
            SaveTag hotbarTag = Rooms.Ingame.Instance.Hud.HotBar.Save();

            //SaveTag hotbarTag = new SaveTag(Start_a_Town_.SaveTag.Types.Compound, "HotBar", t);
            tag.Add(charTag);
            tag.Add(hotbarTag);

            //charTag.WriteTo(writer);
            //hotbarTag.WriteTo(writer);
            tag.WriteTo(writer);
        }


        private static void HandleMessage(Packet msg)
        {
            //Action<byte[]> handler;
            //if (Instance.PacketHandlers.TryGetValue(msg.PacketType, out handler))
            //{
            //    handler(msg.Decompressed);
            //    return;
            //}
            //GameMode.Current.ClientPacketHandler.Handle(Instance, msg);

            Action<Client, Packet> registeredPacket;
            if (Instance.Packets.TryGetValue(msg.PacketType, out registeredPacket))
            {
                //msg.Payload.Deserialize(r => registeredPacket(Instance, r));
                registeredPacket(Instance, msg);
                return;
            }

            IClientPacketHandler handler;
            if (Instance.PacketHandlers.TryGetValue(msg.PacketType, out handler))
            {
                handler.HandlePacket(Instance, msg);
                return;
            }
            switch (msg.PacketType)
            {
                case PacketType.Ping:
                    break;

                case PacketType.RequestPlayerID:
                    TimeoutTimer = TimeoutLength;
                    PlayerData.ID = msg.Payload.Deserialize<int>(r => r.ReadInt32());// Network.Deserialize<int>(msg.Payload, r => r.ReadInt32());
                    Console.Write(Color.Lime, "CLIENT", "Connected to " + RemoteIP.ToString());
                    // call the handler or create specific methods for specific events? like PlayerIDReceived() in the client handler?
                    //GameMode.Current.PacketHandler.Handle(Instance, msg);
                    GameMode.Current.HandlePacket(Instance, msg);

                    //if(Player.Actor!=null)
                    //    EnterWorld(Player.Actor);
                    break;

                case PacketType.PlayerConnecting:
                    PlayerData player = PlayerConnected(Network.Deserialize<PlayerData>(msg.Payload, PlayerData.Read));//msg.Payload);
                    //     UI.LobbyWindow.Instance.Console.Write(player.Name + " connected");
                    UI.LobbyWindow.RefreshPlayers(Players.GetList());
                    //  UI.LogWindow.Write(new LogEventArgs(new Log.Entry(Log.EntryTypes.Default, player.Name + " connecting...")));
                    UI.LobbyWindow.Instance.Console.Write(Color.Yellow, player.Name + " connecting...");
                    Console.Write(Color.Lime, "CLIENT", player.Name + " connecting...");
                    break;

                case PacketType.PlayerDisconnected:
                    int plid = msg.Payload.Deserialize<int>(r => r.ReadInt32());
                    //PlayerDisconnected(Network.Deserialize<PlayerData>(msg.Payload, PlayerData.Read));
                    PlayerDisconnected(plid);
                    break;

                //case PacketType.PlayerExitWorld:
                //    PlayerData pl = Players.GetList()[Network.Deserialize<int>(msg.Payload, r => r.ReadInt32())];
                //    PlayerDisconnected(pl);
                //    break;

                case PacketType.PlayerData:
                    "playerdata".ToConsole();
                    break;

                case PacketType.PlayerList:
                    ReceivePlayerList(msg.Payload);
                    UI.LobbyWindow.RefreshPlayers(Players.GetList());

                    break;

                case PacketType.Chat:
                    //name = Encoding.ASCII.GetString(msg.Payload, 1, msg.Payload[0]);
                    //int textLength = msg.Payload[name.Length + 1];
                    //string text = Encoding.ASCII.GetString(msg.Payload, name.Length + 2, textLength);

                    Network.Deserialize(msg.Payload, reader =>
                    {
                        PlayerData pla = PlayerData.Read(reader);
                        string chatText = reader.ReadASCII();

                        //UI.LobbyWindow.Instance.Console.Write("[" + PlayerData.Read(reader).Name + "] " + reader.ReadASCII());
                        GameObject plachar;
                        if (!Instance.TryGetNetworkObject(pla.CharacterID, out plachar))
                            throw new Exception("Player character with id: " + pla.CharacterID + " is missing (" + plachar.Name + ")");
                        //UI.LobbyWindow.Instance.Console.Write("[" + plachar.Name + "] " + chatText);
                        Instance.EventOccured(Message.Types.Chat, plachar, chatText);
                        //UI.SpeechBubble.Create(plachar, chatText);
                    });


                    //name = "";
                    //string text = "";
                    //Network.Deserialize(msg.Payload, reader =>
                    //{
                    //    name = Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadInt32()));
                    //    text = Encoding.ASCII.GetString(reader.ReadBytes(reader.ReadInt32()));
                    //});
                    //UI.LobbyWindow.Instance.Console.Write("[" + name + "] " + text);
                    break;


                case PacketType.MapData:
                    //GameMode.Current.PacketHandler.Handle(Instance, msg);
                    GameMode.Current.HandlePacket(Instance, msg);

                    break;

                    //if (!Engine.Map.IsNull())
                    //{
                    //    throw new Exception("map already received");
                    //    "map already received, dropping packet".ToConsole();
                    //    break;
                    //}
                    //if (Instance.World.IsNull())
                    //    throw new Exception("map received before world");
                    //Map info = Network.Deserialize<Map>(msg.Payload, Start_a_Town_.Map.ReadData);// as Map;
                    //info.World = Instance.World;
                    //Engine.Map = info;
                    //info.Net = Instance;

                    break;

                case PacketType.WorldInfo:
                    //GameMode.Current.PacketHandler.Handle(Instance, msg);
                    GameMode.Current.HandlePacket(Instance, msg);

                    return;
                    //if (!Instance.World.IsNull())
                    //{
                    //    throw new Exception("world already received");
                    //    "world already received, dropping packet".ToConsole();
                    //    break;
                    //}
                    //World world = Network.Deserialize<World>(msg.Payload, World.ReadData);// as World;
                    //Instance.World = world;
                    break;

                case PacketType.UpdateChunkNeighbors:
                    var vector = Network.Deserialize<Vector2>(msg.Payload, r => r.ReadVector2());
                    //foreach (var item in vector.GetNeighbors())
                    //    Instance.Map.UpdateChunkEdges(item);
                    break;

                case PacketType.UpdateChunkEdges:
                    vector = Network.Deserialize<Vector2>(msg.Payload, r => r.ReadVector2());
                    //Instance.Map.UpdateChunkEdges(vector);
                    break;

                case PacketType.Chunk:
                    //GameMode.Current.PacketHandler.Handle(Instance, msg);
                    GameMode.Current.HandlePacket(Instance, msg);

                    return;
                    if (Engine.Map.IsNull())
                        throw new Exception("chunk received before map");
                    // TODO: find way to receive 
                    //Chunk chunk = Network.Deserialize<Chunk>(msg.Payload, Chunk.Read);
                    //Chunk chunk = ((PacketChunk)msg).Chunk;
                    Chunk chunk = Chunk.Read(msg.Decompressed);
                    Instance.ReceiveChunk(chunk);
                    break;

                case PacketType.UnloadChunk:

                    vector = msg.Payload.Deserialize<Vector2>(r => r.ReadVector2());

                    //if (Instance.Map.ActiveChunks.TryRemove(vector, out chunk))
                    //{
                    //    foreach (var o in chunk.GetObjects())
                    //        Instance.DisposeObject(o);
                    //}
                    if (Instance.Map.GetActiveChunks().TryGetValue(vector, out chunk))
                    {
                        foreach (var o in chunk.GetObjects())
                            Instance.DisposeObject(o);
                        Instance.Map.GetActiveChunks().Remove(vector);
                    }
                    break;

                case PacketType.Partial:
                    PartialPacketReceiver.Receive(msg);
                    break;

                //case PacketType.ChunkSegment:
                //    Network.Deserialize(msg.Payload, r =>
                //    {
                //        Vector2 chCoords = r.ReadVector2();
                //        ChunkTransfer.SegmentTypes type = (ChunkTransfer.SegmentTypes)r.ReadInt32();
                //        int dataLength = r.ReadInt32();
                //        byte[] data = r.ReadBytes(dataLength);
                //        Instance.ReceiveChunkSegment(chCoords, new ChunkTransfer.ChunkSegment(type, data));
                //    });
                //    break;

                case PacketType.PlayerEnterWorld:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        PlayerData newPlayer = PlayerData.Read(r);
                        //Players.Add(newPlayer);
                        UI.LobbyWindow.Instance.Console.Write(Color.Yellow, newPlayer.Name + " connected");
                        Console.Write(Color.Lime, "CLIENT", newPlayer.Name + " connected");

                        // instantiate and spawn player entity
                        GameObject entity = PlayerEntity.Create(r);
                        Instance.Instantiate(entity);
                        Instance.Spawn(entity);
                    });
                    break;

                case PacketType.AssignCharacter:
                    // TODO: iterate through all game components and call their playerassigned method
                    GameObject playerChar = Instance.NetworkObjects[msg.Payload.Deserialize<int>(r => r.ReadInt32())];
                    PlayerData.CharacterID = playerChar.Network.ID;
                    Player.Actor = Instance.NetworkObjects[playerChar.Network.ID];
                    Rooms.Ingame.Instance.Hud.Initialize(Player.Actor);
                    //Player.Instance.HotBar.Initialize(Player.Actor);
                    Rooms.Ingame.Instance.Camera.CenterOn(Player.Actor.Global);
                    PlayerSaveTimer.Change(PlayerSaveInterval, PlayerSaveInterval);
                    break;

                //case PacketType.AssignCharacter:
                //    GameObject playerChar = Network.Deserialize<GameObject>(msg.Payload, GameObject.CreatePrefab);//CreateCustomObject);
                //    PlayerData.CharacterID = playerChar.NetworkID;
                //    //playerChar.Instantiate(Instance.Instantiator);
                //    //Instance.Spawn(playerChar);
                //    Player.Actor = Instance.NetworkObjects[playerChar.NetworkID];
                //    Player.Instance.HotBar.Initialize(Player.Actor);
                //    break;

                case PacketType.EntityInteract:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        PacketEntityInteractionTarget pack = new PacketEntityInteractionTarget(Instance, r);
                        pack.Handle(Instance);

                        //var entity = Instance.GetNetworkObject(pack.EntityID);
                        //var interaction = pack.Target.GetInteraction(Instance, pack.InteractionName);
                        //if (interaction == null)
                        //    throw new Exception("Interaction " + interaction.Name + " doesn't exist on " + pack.Target.ToString());
                        ////entity.ChangePosition(pack.EntityGlobal); // WORKAROUND until tickstamped packets
                        ////entity.Velocity = pack.EntityVelocity;
                        ////System.Console.WriteLine("entity global changed to: " + entity.Global.ToString());
                        //entity.TryGetComponent<WorkComponent>(c => c.Perform(entity, interaction, pack.Target));
                    });
                    break;

                case PacketType.EntityInterrupt:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        var p = new PacketEntity(r.ReadInt32());
                        WorkComponent.Stop(Instance.GetNetworkObject(p.EntityID));
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

                case PacketType.PlayerStartMoving:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int netid = r.ReadInt32();
                        //GameObject obj = Instance.NetworkObjects[netid];
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        obj.GetComponent<MobileComponent>().Start(obj);
                    });
                    break;
                case PacketType.PlayerStopMoving:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int netid = r.ReadInt32();
                        //GameObject obj = Instance.NetworkObjects[netid];
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        obj.GetComponent<MobileComponent>().Stop(obj);
                    });
                    break;
                case PacketType.PlayerJump:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int netid = r.ReadInt32();
                        //GameObject obj = Instance.NetworkObjects[netid];
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
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

                case PacketType.PlayerToggleWalk:
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
                        obj.GetComponent<MobileComponent>().ToggleWalk(toggle);
                    });
                    return;

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
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        //GameObject obj = Instance.NetworkObjects[netid];
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
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

                //case PacketType.PlaceBlockConstruction:
                //    Network.Deserialize(msg.Payload, r =>
                //    {
                //        int netid = r.ReadInt32();
                //        GameObject obj;
                //        if (!Instance.TryGetNetworkObject(netid, out obj))
                //        {
                //            RequestEntityFromServer(netid);
                //            return;
                //        }
                //        Components.Crafting.BlockConstruction.ProductMaterialPair product = new Components.Crafting.BlockConstruction.ProductMaterialPair(r);
                //        Vector3 global = r.ReadVector3();
                //        obj.GetComponent<WorkComponent>().Perform(obj, new Components.Interactions.InteractionConstruct(product), new TargetArgs(global));
                //        return;
                //    });
                //    return;

                //case PacketType.PlaceConstruction:
                //    Network.Deserialize(msg.Payload, r =>
                //    {
                //        var netid = r.ReadInt32();
                //        //GameObject.Types type = (GameObject.Types)r.ReadInt32();
                //        Components.Crafting.Reaction.Product.ProductMaterialPair product = new Components.Crafting.Reaction.Product.ProductMaterialPair(r);
                //        Vector3 global = r.ReadVector3();
                //        GameObject obj;
                //        if (!Instance.TryGetNetworkObject(netid, out obj))
                //        {
                //            Instance.RequestEntityFromServer(netid);
                //            return;
                //        }
                //        obj.GetComponent<WorkComponent>().Perform(obj, new Modules.Construction.InteractionConstruct(product), new TargetArgs(global));
                //        return;
                //    });
                //    return;

                case PacketType.PlayerInput:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
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
                        //GameObject obj = Instance.NetworkObjects[netid];
                        //GameObject obj = Instance.NetworkObjects[netid];
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
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
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        obj.GetComponent<WorkComponent>().Perform(obj, new Components.Interactions.PickUp(), target);
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

                case PacketType.PlayerEquip:
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
                        obj.GetComponent<ControlComponent>().StartScript(Script.Types.Equipping, new ScriptArgs(Instance, obj, target));
                    });
                    return;

                case PacketType.PlayerUnequip:
                    msg.Payload.Deserialize(r =>
                    {
                        int netid = r.ReadInt32();
                        TargetArgs target = TargetArgs.Read(Instance, r);
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
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
                        GameObject obj;
                        if (!Instance.TryGetNetworkObject(netid, out obj))
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
                        obj.GetComponent<Components.Combat.BlockingComponent>().Start(obj);

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
                        obj.GetComponent<Components.Combat.BlockingComponent>().Stop(obj);

                    });
                    return;

                case PacketType.PlayerCraft:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        //var actor = Instance.NetworkObjects[r.ReadInt32()];
                        //var building = TargetArgs.Read(Instance, r);
                        //var prod = new Components.Crafting.Reaction.Product.ProductMaterialPair(r);
                        //var containerEntity = Instance.GetNetworkObject(r.ReadInt32());
                        //var container = containerEntity.GetContainer(r.ReadInt32());
                        //actor.GetComponent<WorkComponent>().Perform(actor, new Components.Interactions.Craft(prod, container), building);// new TargetArgs());

                        int netid = r.ReadInt32();
                        GameObject actor;
                        if (!Instance.TryGetNetworkObject(netid, out actor))
                        {
                            Instance.RequestEntityFromServer(netid);
                            return;
                        }
                        var crafting = new Components.Crafting.CraftOperation(Instance, r);
                        var reaction = Components.Crafting.Reaction.Dictionary[crafting.ReactionID];
                        if (reaction == null)
                            return;
                        var product = reaction.Products.First().GetProduct(reaction, crafting.Building.Object, crafting.Materials, crafting.Tool);
                        if (product == null)
                            return;
                        if (product.Tool != null)
                            GearComponent.Equip(actor, PersonalInventoryComponent.FindFirst(actor, foo => foo == product.Tool));

                        //actor.GetComponent<WorkComponent>().Perform(actor, new Components.Interactions.Craft(product, crafting.Container), crafting.Building);
                        var workstation = Instance.Map.GetBlockEntity(crafting.WorkstationEntity) as Blocks.BlockWorkbench.Entity;
                        //actor.GetComponent<WorkComponent>().Perform(actor, new Components.Crafting.InteractionCraftingWorkbench(product, workstation, crafting.WorkstationEntity), crafting.Building);
                        if (workstation == null)
                            actor.GetComponent<WorkComponent>().Perform(actor, new Components.Crafting.InteractionCraftingPerson(product), crafting.Building);
                        else
                            actor.GetComponent<WorkComponent>().Perform(actor, new Components.Crafting.InteractionCraftingWorkbench(product, workstation, crafting.WorkstationEntity), crafting.Building);


                        //int netid = r.ReadInt32();
                        //int productid = r.ReadInt32();

                        //GameObject obj;
                        //if (!Instance.TryGetNetworkObject(netid, out obj))
                        //{
                        //    RequestEntityFromServer(netid);
                        //    return;
                        //}
                        //GameObject product = Instance.NetworkObjects[productid];
                        //Instance.PostLocalEvent(obj, Message.Types.Insert, product.ToSlot());
                    });
                    return;

                //case PacketType.PlayerCraftBench:
                //    Network.Deserialize(msg.Payload, r =>
                //    {
                //        var pl = Instance.NetworkObjects[r.ReadInt32()];
                //        var bench = Instance.NetworkObjects[r.ReadInt32()];
                //        var pr = new Components.Crafting.Reaction.Product.ProductMaterialPair(r);
                //        pl.GetComponent<WorkComponent>().Perform(pl, new Components.Interactions.CraftBench(pr), new TargetArgs(bench));
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
                        if (obj.Network.ID == 0)
                            throw new Exception("Uninstantiated entity");

                        // instantiate entity on client if it isn't already instantiated
                        if (!Instance.NetworkObjects.ContainsKey(obj.Network.ID))
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
                    ent.Instantiate(Instance.Instantiator).ComponentsCreated();
                    //if (ent.Exists)
                    //    Instance.Spawn(ent);
                    return;

                case PacketType.InstantiateAndSpawnObject: //register netID to list and spawn
                    Network.Deserialize(msg.Payload, r =>
                    {
                        GameObject ob = GameObject.CreatePrefab(r).ComponentsCreated(); // the obj is received with a netid but without a position component// a  position component and netid
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
                            if (tar.Object.IsNull())
                            {
                                Console.Write(Color.Orange, "CLIENT", "Can't dispose null entity");//doesn't exist.");
                                break;
                            }
                            GameObject o;
                            //Instance.NetworkObjects.TryRemove(tar.Object.NetworkID, out o);
                            Instance.DisposeObject(tar.Object);
                            Instance.Despawn(tar.Object);
                            break;

                        case TargetType.Slot:
                            o = tar.Slot.Object;
                            //Instance.NetworkObjects.TryRemove(o.NetworkID, out o);
                            Instance.DisposeObject(o);
                            tar.Slot.Clear();
                            break;

                        default:
                            throw new Exception("Invalid object");
                            break;
                    }
                    //GameObject o;
                    //Instance.NetworkObjects.TryRemove(tar.Object.NetworkID, out o);
                    //Instance.Despawn(tar.Object);
                    break;

                case PacketType.SyncEntity:
                    //Network.Deserialize<GameObject>(msg.Payload, r =>
                    //{
                    //    int netid = r.ReadInt32();
                    //    GameObject entity;
                    //    if (Instance.TryGetNetworkObject(netid, out entity))
                    //    {
                    //        // TODO: sync existing entity's values from packet here
                    //        //existing.Update(r);
                    //        return entity;
                    //    }
                    //    entity = GameObject.CreatePrefab(r);
                    //    Instance.Instantiate(entity);
                    //    if (entity.Exists)
                    //        Instance.Spawn(entity);
                    //    return entity;
                    //});
                    Network.Deserialize<GameObject>(msg.Payload, r =>
                    {
                        GameObject entity = GameObject.CreatePrefab(r);
                        GameObject existing;
                        if (Instance.TryGetNetworkObject(entity.Network.ID, out existing))
                        {
                            // TODO: sync existing entity's values from packet here
                            //existing.Update(r);
                            return existing;
                        }
                        Instance.Instantiate(entity);
                        if (!entity.Exists)
                            Instance.Spawn(entity);
                        return entity;
                    });
                    //Network.Deserialize<GameObject>(msg.Payload, r =>
                    //{
                    //    int netid = r.ReadInt32();
                    //    GameObject existing;
                    //    if (Instance.TryGetNetworkObject(netid, out existing))
                    //        existing.Update(r);
                    //    return existing;
                    //});
                    break;

                case PacketType.IncreaseEntityQuantity:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        var senderID = r.ReadInt32();
                        //var entity = TargetArgs.Read(Instance, r).Object;
                        var tarentityID = r.ReadInt32(); //TargetArgs.Read(Instance, r).Object;
                        var entity = Instance.NetworkObjects[tarentityID];
                        var quantity = r.ReadInt32();
                        entity.TryGetComponent<StackableComponent>(c => c.SetStacksize(entity, entity.StackSize + quantity));
                    });
                    return;

                case PacketType.SpawnObject:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        //GameObject ob = GameObject.CreatePrefab(r).ComponentsCreated();
                        //if (ob.NetworkID == 0)
                        //    throw (new Exception("Tried to spawn uninstantiated object"));
                        //if (Instance.NetworkObjects.ContainsKey(ob.NetworkID))
                        //    throw (new Exception("Tried to spawn duplicate object"));
                        int nid = r.ReadInt32();
                        GameObject toSpawn = Instance.NetworkObjects[nid];
                        var pos = r.ReadVector3();
                        var vel = r.ReadVector3();
                        Instance.Spawn(toSpawn);

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

                case PacketType.Snapshot:
                    Instance.ReadSnapshot(msg.Decompressed);
                    break;

                case PacketType.SyncTime:
                    msg.Payload.Deserialize(r =>
                    {
                        SyncTime(r.ReadDouble());
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
                        DragDropManager.Create(new DragDropSlot(Player.Actor, GameObjectSlot.Empty, new GameObjectSlot(obj.Object, amount), DragDropEffects.Move | DragDropEffects.Copy));

                    });
                    break;

                case PacketType.JobCreate:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        //TargetArgs creator = TargetArgs.Read(Instance, r);
                        //TargetArgs targ = TargetArgs.Read(Instance, r);
                        //Components.Script.Types script = (Components.Script.Types)r.ReadInt32();
                        TownJob job = TownJob.Read(r, Instance);
                        Instance.Map.GetTown().Jobs.Add(job);//new TownJobStep(creator.Object, targ, script));
                        Towns.TownJobsWindow.Instance.Show(Instance.Map.GetTown());
                    });
                    break;

                case PacketType.JobDelete:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int jobID = r.ReadInt32();
                        //int removed = Instance.Map.Town.Jobs.RemoveAll(j => j.ID == jobID);
                        Instance.Map.GetTown().Jobs.Remove(jobID);
                        Towns.TownJobsWindow.Instance.Refresh(Instance.Map.GetTown());//.Jobs);
                    });
                    break;

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

                        if (!Instance.Map.PositionExists(global))
                            return;
                        var previousBlock = Instance.Map.GetBlock(global);
                        previousBlock.Remove(Instance.Map, global);
                        var block = Start_a_Town_.Block.Registry[type];
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
                            Controller.Instance.Mouseover.Target = null;

                            return;



                            //Cell cell = global.GetCell(Instance.Map);//.Read(r);
                            Cell cell = Instance.Map.GetCell(global);//.Read(r);

                            if (cell.IsNull())
                                return;
                            cell.Read(r);

                            //Chunk ch = Instance.Map.GetChunk(global);
                            ////ch.InvalidateLight(cell); // no need, invalidatecell does this
                            //ch.InvalidateCell(cell);
                            Instance.Map.InvalidateCell(global);
                            foreach (var n in global.GetNeighbors())
                                Instance.Map.InvalidateCell(n);
                        }
                    });
                    break;

                //case PacketType.SyncCells:
                //    Network.Deserialize(msg.Payload, r =>
                //    {
                //        int count = r.ReadInt32();
                //        List<Vector3> vectors = new List<Vector3>();
                //        for (int i = 0; i < count; i++)
                //        {
                //            Vector3 global = r.ReadVector3();
                //            Cell cell = global.GetCell(Instance.Map);//.Read(r);
                //            if (cell.IsNull())
                //                return;
                //            //cell.Read(r);
                //            //global.TrySetCell(Instance, cell.Type, cell.BlockData);
                //            Chunk ch = global.GetChunk(Instance.Map);
                //            ch.InvalidateLight(cell);
                //            ch.InvalidateCell(cell);
                //        }
                //    });
                //    break;

                //case PacketType.ShowBlock:
                //    Network.Deserialize(msg.Payload, r =>
                //    {
                //        Vector3 glob = r.ReadVector3();
                //        Instance.ShowBlock(glob);
                //    });
                //    break;

                case PacketType.PlayerInventoryChange:
                    Network.Deserialize(msg.Payload, r =>
                    {
                        int count = r.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            GameObjectSlot slot = TargetArgs.Read(Instance, r).Slot;
                            var stacksize = r.ReadInt32();
                            if (stacksize > 0)
                                slot.Object = Instance.GetNetworkObject(r.ReadInt32());
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

                case PacketType.RandomBlockUpdates:
                    msg.Payload.Deserialize(r =>
                    {
                        int count = r.ReadInt32();
                        for (int i = 0; i < count; i++)
                        {
                            Vector2 chunkCoords = r.ReadVector2();
                            for (int n = 0; n < 24; n++)
                            {
                                Vector3 blockLocal = r.ReadVector3();
                                Vector3 g = blockLocal.ToGlobal(chunkCoords);

                                // blockGlobal.GetBlock(Instance.Map).RandomBlockUpdate(Instance, blockGlobal);
                                //g.TryGetBlock(Instance.Map, (block, c) => block.RandomBlockUpdate(Instance, g, c));

                                //Instance.Map.TryGetBlock(g, (block, c) => block.RandomBlockUpdate(Instance, g, c));
                                Cell cell;
                                if (Instance.Map.TryGetCell(g, out cell))
                                    cell.Block.RandomBlockUpdate(Instance, g, cell);

                                //GameObject blockObj;
                                //if (Cell.TryGetObject(Instance.Map, blockGlobal, out blockObj))
                                //    blockObj.RandomBlockUpdate(Instance);
                            }
                        }
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
                        Console.Write("Conversation started between " + e1.Name + " and " + e2.Name);
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
                        AI.PacketDialogueOptions p = new AI.PacketDialogueOptions(Instance, r);
                        Instance.EventOccured(Components.Message.Types.Dialogue, p);
                    });
                    break;

                //case PacketType.MergeEntities:
                //    msg.Payload.Deserialize(r =>
                //    {
                //        PacketMergeEntities.Handle(Instance, r);
                //    });
                //    break;

                default:
                    //    msg.ToConsole();
                    //GameMode.Current.PacketHandler.Handle(Instance, msg);
                    GameMode.Current.HandlePacket(Instance, msg);

                    break;
            }
        }

        private static void SyncTime(double serverMS)
        {
            if (LastReceivedTime > serverMS)
            {
                ("sync time packet dropped (last: " + LastReceivedTime.ToString() + ", received: " + serverMS.ToString()).ToConsole();// + "server: " + Server.ServerClock.TotalMilliseconds.ToString() + ")").ToConsole();
                return;
            }
            //else
            //{
            //    ("sync time packet accepted (last: " + LastReceivedTime.ToString() + ", received: " + serverMS.ToString()).ToConsole();// + "server: " + Server.ServerClock.TotalMilliseconds.ToString() + ")").ToConsole();
            //}
            LastReceivedTime = serverMS;
            var newtime = serverMS - ClientClockDelayMS;
            TimeSpan serverTime = TimeSpan.FromMilliseconds(newtime);
            ClientClock = serverTime;
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

        private void ReceiveChunkSegment(Vector2 chCoords, ChunkTransfer.ChunkSegment chunkSegment)
        {
            ChunkTransfer chunkTransfer;
            if (!this.PartialChunks.TryGetValue(chCoords, out chunkTransfer))
                this.PartialChunks[chCoords] = ChunkTransfer.BeginReceive(chCoords, ChunkTransferComplete);
            chunkTransfer.Receive(chunkSegment);
        }
        void ChunkTransferComplete(Chunk chunk)
        {
            this.PartialChunks.Remove(chunk.MapCoords);
        }

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

        //private static void HandleObjectEvent(PacketMessage msg)
        //{
        //    Network.Deserialize(msg.Payload, r =>
        //    {
        //        double timestamp = r.ReadDouble();
        //        int objID = r.ReadInt32();
        //        Components.Message.Types type = (Components.Message.Types)r.ReadInt32();
        //        byte[] data = r.ReadBytes(r.ReadInt32());

        //        GameObject obj = Instance.GetNetworkObject(objID);


        //        //Instance.PostLocalEvent(obj, ObjectLocalEventArgs.Create(type, data));
        //        //return;
        //        //if ((msg.SendType & SendType.Ordered) == SendType.Ordered)
        //        //{
        //            // ENSURE ORDERED EXECUTION OF EVENTS
        //            ObjectEvent e = new ObjectEvent(timestamp, obj, ObjectEventArgs.Create(type, data));
        //            IncomingOrdered.Enqueue(msg.ID, msg);//e);
        //        //}
        //        //else
        //        //    Instance.PostLocalEvent(obj, ObjectLocalEventArgs.Create(type, data));
        //    });
        //}
        //static void ExecuteOrderedEvents()
        //{
        //    while (IncomingOrdered.Count > 0)
        //    {
        //        PacketMessage packet = IncomingOrdered.Peek();
        //        Network.Deserialize(packet.Payload, r =>
        //        {
        //            double timestamp = r.ReadDouble();
        //            int objID = r.ReadInt32();
        //            Components.Message.Types type = (Components.Message.Types)r.ReadInt32();
        //            byte[] data = r.ReadBytes(r.ReadInt32());

        //            GameObject obj = Instance.GetNetworkObject(objID);

        //            ObjectEvent objEvent = new ObjectEvent(timestamp, obj, ObjectEventArgs.Create(type, data));
        //            if (objEvent.TimeStamp > ClientClock.TotalMilliseconds)
        //                return;
        //            Instance.PostLocalEvent(objEvent.Recipient, objEvent.Args);
        //            IncomingOrdered.Dequeue();
        //           // Client.Console.Write(packet.ID + " " + objEvent.Args.Type.ToString());
        //        });
        //    }

        //    //while(OrderedPackets.Count>0)
        //    //{
        //    //    ObjectEvent objEvent= OrderedPackets.Peek();
        //    //    if (objEvent.TimeStamp > ClientClock.TotalMilliseconds)
        //    //        return;
        //    //    Instance.PostLocalEvent(objEvent.Recipient, objEvent.Args);
        //    //    OrderedPackets.Dequeue();
        //    //    Client.Console.Write(objEvent.Args.Type.ToString());
        //    //}
        //}
        static int OrderedPacketsHistoryCapacity = 32;
        static Queue<Packet> OrderedPacketsHistory = new Queue<Packet>(32);
        static void HandleOrderedPackets()
        {
            while (IncomingOrdered.Count > 0)
            {
                Packet packet = IncomingOrdered.Dequeue();
                //("dequeued " + packet.ToString()).ToConsole();
                HandleMessage(packet);
                OrderedPacketsHistory.Enqueue(packet);
                while (OrderedPacketsHistory.Count > OrderedPacketsHistoryCapacity)
                    OrderedPacketsHistory.Dequeue();
            }

            //while(OrderedPackets.Count>0)
            //{
            //    ObjectEvent objEvent= OrderedPackets.Peek();
            //    if (objEvent.TimeStamp > ClientClock.TotalMilliseconds)
            //        return;
            //    Instance.PostLocalEvent(objEvent.Recipient, objEvent.Args);
            //    OrderedPackets.Dequeue();
            //    Client.Console.Write(objEvent.Args.Type.ToString());
            //}
        }
        static Queue<Packet> SyncedPackets = new Queue<Packet>();
        static readonly int OrderedReliablePacketsHistoryCapacity = 64;
        static Queue<Packet> OrderedReliablePacketsHistory = new Queue<Packet>(OrderedReliablePacketsHistoryCapacity);
        static void HandleOrderedReliablePackets()
        {
            while (IncomingOrderedReliable.Count > 0)
            {
                var next = IncomingOrderedReliable.Peek();
                long nextid = next.OrderedReliableID;


                //if (next.Tick <= Instance.Clock.TotalMilliseconds // TEST
                    //)
                    //if (
                        //&&
                       if( nextid == RemoteOrderedReliableSequence + 1)
                {
                           
                    //if (next.Tick == Instance.Clock.TotalMilliseconds)
                    //    "good".ToConsole();
                    //else
                    //{
                    //    if (next.Tick > 0)
                    //        "bad".ToConsole();
                    //}
                    RemoteOrderedReliableSequence = nextid;
                    Packet packet = IncomingOrderedReliable.Dequeue();
                    if (next.Tick > Instance.Clock.TotalMilliseconds)
                    {
                        SyncedPackets.Enqueue(next);
                        continue;
                    }
                    HandleMessage(packet);
                    OrderedReliablePacketsHistory.Enqueue(packet);
                    while (OrderedReliablePacketsHistory.Count > OrderedReliablePacketsHistoryCapacity)
                        OrderedReliablePacketsHistory.Dequeue();
                }
                else return;

                ////if (next.Tick > Instance.Clock.TotalMilliseconds)
                ////    "gamietai".ToConsole();
                //if (next.Tick <= Instance.Clock.TotalMilliseconds // TEST
                //    //)
                //    //if (
                //        &&
                //        nextid == RemoteOrderedReliableSequence + 1)
                //    {
                //        //if (next.Tick == Instance.Clock.TotalMilliseconds)
                //        //    "good".ToConsole();
                //        //else
                //        //{
                //        //    if (next.Tick > 0)
                //        //        "bad".ToConsole();
                //        //}
                //        RemoteOrderedReliableSequence = nextid;
                //        Packet packet = IncomingOrderedReliable.Dequeue();
                //        HandleMessage(packet);
                //        OrderedReliablePacketsHistory.Enqueue(packet);
                //        while (OrderedReliablePacketsHistory.Count > OrderedReliablePacketsHistoryCapacity)
                //            OrderedReliablePacketsHistory.Dequeue();
                //    }
                //    else return;
            }
        }

        //static void DestroyObject(GameObject obj)
        /// <summary>
        /// Both removes an object form the game world and releases its networkID
        /// </summary>
        /// <param name="objNetID"></param>
        public bool DisposeObject(GameObject obj)
        {
            GameObject o;

            // ONLY SERVER CAN DESTROY OBJECTS???
            //if (Instance.NetworkObjects.TryRemove(objNetID, out o))
            //    Instance.Despawn(o);

            //return Instance.NetworkObjects.TryRemove(obj.NetworkID, out o);
            return DisposeObject(obj.Network.ID);
        }
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

        public void Instantiator(GameObject ob)
        {
            ob.Net = this;
            Instance.NetworkObjects.AddOrUpdate(ob.Network.ID, (i) =>
            {
                if (Instance.NetworkObjects.Values.Contains(ob))
                    throw new Exception("Duplicate object");
                return ob;
            }, (id, o) =>
            {
                //("Tried to instantiate duplicate object" + ob.Name).ToConsole();
                //Client.Console.Write(Color.Orange, "CLIENT", "Duplicate network ID: " + ob.NetworkID.ToString() + " (" + ob.Name + ")");

                //throw new Exception("Duplicate network ID: " + ob.NetworkID.ToString() + " (" + ob.Name + ")");
                return o;
            });

            //Instance.NetworkObjects.AddOrUpdate(ob.NetworkID, ob, (id, o) =>
            //{
            //    "Tried to instantiate duplicate object".ToConsole();
            //    throw new Exception("Duplicate net ID");
            //    // return o;
            //});
        }
        //if (Instance.NetworkObjects.Values.Contains(obj))
        //throw new Exception("Duplicate object");

        //World _world;
        public IWorld World;
        //{
        //    get { return _world; }
        //    set
        //    {
        //        _world = value;
        //    }
        //}

        private static PlayerData PlayerConnected(PlayerData player)
        {
            Players.Add(player);
            return player;
        }

        static void PlayerDisconnected(PlayerData player)
        {
            Players.Remove(player);
            if (Instance.Map != null)
                Instance.Despawn(Instance.NetworkObjects[player.CharacterID]);
            Instance.DisposeObject(player.CharacterID);
            Network.Console.Write(Color.Yellow, player.Name + " disconnected");
            Console.Write(Color.Lime, "CLIENT", player.Name + " disconnected");
        }
        static void PlayerDisconnected(int playerID)
        {
            //PlayerData player = Players.GetList()[playerID];
            PlayerData player = Players.GetList().FirstOrDefault(p => p.ID == playerID);
            if (player.IsNull())
                return;
            PlayerDisconnected(player);
        }
        static void ReceiveMessage(IAsyncResult ar)
        {
            try
            {
                UdpConnection state = (UdpConnection)ar.AsyncState;
                int bytesRead = state.Socket.EndReceive(ar);
                if (bytesRead == Packet.Size)
                    throw new Exception("buffer full");

                // to palio
                //byte[] bytesReceived = state.Buffer.Take(bytesRead).ToArray();
                //state.Buffer = new byte[Packet.Size];
                //Host.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveMessage, state);

                // mia nea prospatheia alla ta idia skata
                //var recvbuf = state.Buffer;
                //    state.Buffer = new byte[Packet.Size];
                //    Host.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveMessage, state);
                //byte[] bytesReceived = recvbuf.Take(bytesRead).ToArray();

                // gia na doume auto
                byte[] bytesReceived = state.Buffer.Take(bytesRead).ToArray();
                Host.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveMessage, state);

                //Packet fullpacket = Packet.Create(bytesReceived);
                //foreach (var packet in fullpacket.Messages)
                //{
                Packet packet = Packet.Read(bytesReceived);

                //if (packet.PacketType == PacketType.Chunk)
                //    packet = new PacketChunk(packet.Payload) { ID = packet.ID, SendType = packet.SendType };
                // send ack if the packet is reliable

                if ((packet.SendType & SendType.Reliable) == SendType.Reliable)
                    Packet.Send(PacketID, PacketType.Ack, Network.Serialize(w => w.Write(packet.ID)), Host, RemoteIP);
                //if (packet.PacketType != PacketType.Ping && packet.PacketType != PacketType.PlayerList && packet.PacketType != PacketType.SyncTime)
                //    "ASDASD".ToConsole();

                //if (packet.Length + 17 != bytesRead) // 8 for seq id, 4 for sendtype, 1 for packettype, 4 for packet length, 
                //    throw new Exception("packet length mismatch");
                //if (packet.PacketType == PacketType.Chunk)
                //    (DateTime.Now.ToString() + " " + packet.PacketType.ToString() + " received").ToConsole();

                //if (packet.PacketType == PacketType.Chunk)
                //{
                //    Instance.chunksreceived++;
                //    Instance.chunksreceived.ToConsole();
                //}
                //if (packet.PacketType != PacketType.Ping)
                //{
                //    packet.PacketType.ToConsole();
                //}


                    IncomingAll.Enqueue(packet);

                //}
            }
            catch (ObjectDisposedException)
            {

            }
            catch (Exception e)
            {
                ScreenManager.Remove();
                e.ShowDialog();
            }
        }
        static int RecentPacketBufferSize = 32;
        static Queue<long> RecentPackets = new Queue<long>();
        //    int LastPacketsBitMask;
        private static bool IsDuplicate(Packet packet)
        {
            long id = packet.ID;
            if (id > RemoteSequence)
            {
                if (id - RemoteSequence > 31)
                {
                    // very large jump in packets
                    Console.Write(Color.Orange, "CLIENT", "Warning! Large gap in received packets!");
                }
                RemoteSequence = id;
                return false;
            }
            else if (id == RemoteSequence)
                return true;
            BitVector32 field = GenerateBitmask();
            int distance = (int)(RemoteSequence - id);
            //bool found = mask[distance];// (mask.Data & (1 << distance));// mask[distance];
            if (distance > 31)
            {
                // very old packet
                //return false;
                Console.Write(Color.Orange, "CLIENT", "Warning! Received severely outdated packet: " + packet.PacketType.ToString());
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

        //static BitVector32 GenerateBitmask()
        //{
        //    BitVector32 mask = new BitVector32(0);
        //   // int field = 0;
        //    foreach (var recent in RecentPackets)
        //    {
        //        int distance = (int)(RemoteSequence - recent);
        //        mask[distance] = true;
        //    }
        //    return mask;
        //}
        static BitVector32 GenerateBitmask()
        {
            int mask = 0;
            foreach (var recent in RecentPackets)
            {
                int distance = (int)(RemoteSequence - recent);
                if (distance > 31)
                    continue;
                mask |= 1 << distance;
                BitVector32 test = new BitVector32(mask);
            }
            BitVector32 bitvector = new BitVector32(mask);
            return bitvector;
        }
        //static void ReceiveMessage(IAsyncResult ar)
        //{
        //    ObjectState state = (ObjectState)ar.AsyncState;
        //    int bytesRead = state.Socket.EndReceive(ar);
        //    //    Socket socket = (Socket)ar.AsyncState;
        //    try
        //    {
        //        // byte[] buffer = state.Buffer;
        //        var newPackets = state.Buffer.GetPackets(bytesRead);
        //        //     Packet msg = Packet.Create(state.Buffer);
        //        //    lastdata = buffer.ToArray();
        //        while (newPackets.Count > 0)
        //        {
        //            var msg = newPackets.Dequeue();
        //            msg.Sender = state.Socket;
        //            if (msg.Length > Packet.Size - 5)
        //            {

        //                if (msg.PacketType.IsNull())
        //                    throw new Exception("Invalid packet type");
        //                //System.Diagnostics.Debug.Assert(msg.Length <= 30005, msg.PacketType.ToString() + " " + msg.Length.ToString());
        //                if (msg.Length > 30005)
        //                    throw new Exception(msg.PacketType.ToString() + " " + msg.Length.ToString());
        //                int remain = msg.Length - Packet.Size + 5;
        //                //    ObjectState newState = new ObjectState("Multi receiving", Host) { Buffer = new byte[remain] };
        //                ("start recieving multi-packet of length " + msg.Length.ToString()).ToConsole();
        //                //Host.BeginReceive(newState.Buffer, 0, remain, SocketFlags.None, a => AppendMessage(a, msg), newState);

        //                //Host.Receive(newState.Buffer, 0, remain, SocketFlags.None);
        //                //AppendMessage(msg, newState.Buffer);

        //                Host.Receive(msg.Payload, Packet.Size - 5, remain, SocketFlags.None);

        //                //("finished recieving multi-packet of length " + msg.Length.ToString()).ToConsole();
        //                ("finished recieving multi-packet " + (msg.Length).ToString() + " bytes").ToConsole();
        //                MessageQueue.Enqueue(msg);
        //                //    state.Buffer = new byte[Packet.Size];
        //                Host.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveMessage, state);
        //                return;
        //                //System.Windows.Forms.MessageBox.Show(msg.Length.ToString());
        //            }


        //            // TO EPOMENO PACKET KANEI CONSUME TA PROHGOUMENA
        //            // TO BUFFER BOREI NA PERIEXEI PANW APO ENA PACKET KAI MOLIS FTIAXNW TO PRWTO TA EPOMENA GINONTAI DISCARD

        //            MessageQueue.Enqueue(msg);
        //            ("client successfully received and enqueued packet: " + msg.ToString()).ToConsole();
        //        }
        //        state.Buffer = new byte[Packet.Size];
        //        Host.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveMessage, state);
        //    }
        //    catch (ObjectDisposedException)
        //    {
        //    }
        //    //catch (Exception e)
        //    //{
        //    //    ScreenManager.Remove();
        //    //    e.ShowDialog();
        //    //}
        //}
        static void AppendMessage(Packet msg, byte[] data)
        {
            byte[] full = new byte[msg.Length];
            msg.Payload.CopyTo(full, 0);
            Array.Copy(data, 0, full, msg.Payload.Length, data.Length);
            msg.Payload = full;
        }
        int chunksreceived = 0;
        static void AppendMessage(IAsyncResult ar, Packet msg)
        {
            //Socket socket = (Socket)ar.AsyncState;
            UdpConnection state = (UdpConnection)ar.AsyncState;
            // int newBytesLength = state.Socket.EndReceive(ar);
            "end receive append".ToConsole();


            //byte[] full = new byte[msg.Payload.Length + newBytesLength];
            //msg.Payload.CopyTo(full, 0);
            //Array.Copy(state.Buffer, 0, full, msg.Payload.Length, newBytesLength);
            //msg.Payload = full;
            //int remain = msg.Length - msg.Payload.Length;


            byte[] full = new byte[msg.Length];
            msg.Payload.CopyTo(full, 0);
            Array.Copy(state.Buffer, 0, full, msg.Payload.Length, state.Buffer.Length);
            msg.Payload = full;
            int remain = msg.Length - msg.Payload.Length;


            if (msg.Payload.Length == msg.Length)
            {
                ("finished recieving multi-packet of length " + msg.Length.ToString()).ToConsole();
                IncomingAll.Enqueue(msg);

                int newBytesLength = state.Socket.EndReceive(ar);
                //state.Buffer = new byte[1024];
                UdpConnection oldState = new UdpConnection("normal receiving", Host) { Buffer = new byte[Packet.Size] };
                Host.BeginReceive(oldState.Buffer, 0, oldState.Buffer.Length, SocketFlags.None, ReceiveMessage, oldState);
                return;
            }
            else
                throw new Exception("didn't receive full data");
            state.Buffer = new byte[remain];
            ("recieved partial packet, " + remain.ToString() + " bytes remaining").ToConsole();
            Host.BeginReceive(state.Buffer, 0, remain, SocketFlags.None, a => AppendMessage(a, msg), state);
        }
        static public void Send(byte[] data, AsyncCallback onSent = null)
        {
            Host.BeginSend(data, 0, data.Length, SocketFlags.None, onSent ?? (a => { }), Host);
        }
        static public void Send(Packet msg)
        {
            //msg.Send();//new ObjectState("Client", Host));
            msg.BeginSendTo(Host, RemoteIP);
        }

        static public void RequestMapInfo()
        {
            //Host.BeginSend(new byte[] { (byte)PacketType.RequestMapInfo }, 0, 1, SocketFlags.None, a => { }, Host);
            Packet.Create(PacketID, PacketType.RequestMapInfo, new byte[] { }).BeginSendTo(Host, RemoteIP);
        }
        static public void RequestWorldInfo()
        {
            //Host.BeginSend(new byte[] { (byte)PacketType.RequestWorldInfo }, 0, 1, SocketFlags.None, a => { }, Host);
            //Packet.Create(PacketType.RequestWorldInfo, new byte[] { }).Send(Host);
            Packet.Create(PacketID, PacketType.RequestWorldInfo, new byte[] { }).BeginSendTo(Host, RemoteIP);
        }

        static public HashSet<Vector2> ChunkRequests = new HashSet<Vector2>();
        //static public void RequestChunks(IEnumerable<Vector2> vecs)
        //{
        //    foreach (var vec in vecs)
        //    {
        //        // create empty chunks to start receiving light values
        //        RequestChunk(vec);
        //    }
        //}

        //static public void RequestChunk(Vector2 vec)
        //{
        //    ChunkRequests.Add(vec);
        //    Instance.Map.ActiveChunks[vec] = new Chunk(Instance.Map, vec);
        //    Packet.Create(PacketID, PacketType.RequestChunk, Network.Serialize(w => w.Write(vec))).BeginSendTo(Host, RemoteIP);
        //}


        static public void NotifyChunkLoaded(Vector2 chunkCoords, Action<Chunk> callback)
        {
            Chunk existingChunk;
            if (!Instance.Map.IsNull())
                if (Instance.Map.GetActiveChunks().TryGetValue(chunkCoords, out existingChunk))
                {
                    callback(existingChunk);
                    return;
                }
            ChunkCallBackEvents.AddOrUpdate(chunkCoords, new ConcurrentQueue<Action<Chunk>>(new Action<Chunk>[] { callback }), (vec, queue) => { queue.Enqueue(callback); return queue; });
        }
        static public void OnChunkReceived(Vector2 chunkCoords, Action<Chunk> callback)
        {
            // TODO: keep a list of requested chunks until they are received
            ChunkCallBackEvents.AddOrUpdate(chunkCoords, new ConcurrentQueue<Action<Chunk>>(new Action<Chunk>[] { callback }), (vec, queue) => { queue.Enqueue(callback); return queue; });
        }
        //List<Chunk> chunksgot = new List<Chunk>();
        void ReceiveChunk(byte[] data)
        {
            this.ReceiveChunk(Chunk.Read(data));
        }
        public void ReceiveChunk(Chunk chunk)
        {
            //chunksgot.Add(chunk);
            ChunkRequests.Remove(chunk.MapCoords);

            //var playerchunk = Player.Actor.Global.GetChunk(this.Map);
            //// discard chunk if outside chunk radius
            //if (playerchunk != null)
            //    if (Vector2.Distance(playerchunk.MapCoords, chunk.MapCoords) > Engine.ChunkRadius)
            //        return;
            //{
            //    //if (!playerchunk.MapCoords.GetSpiral().Contains(chunk.MapCoords))
            //    //    return;
            //}

            if (this.Map.GetActiveChunks().ContainsKey(chunk.MapCoords))
            {
                (chunk.MapCoords.ToString() + " already loaded").ToConsole();
                return;
            }
            chunk.Map = this.Map;


            chunk.GetObjects().ForEach(obj =>
            {
                obj.Instantiate(Instance.Instantiator);
                obj.Net = Instance;
                obj.Transform.Exists = true;
                obj.ObjectLoaded();
            });
            //chunk.GetBlockObjects().ForEach(obj =>
            //{
            //    obj.Instantiate(Instance.Instantiator);
            //});
            foreach (var blockentity in chunk.BlockEntities.Values)
                blockentity.Instantiate(Instance.Instantiator);


            //var chunks = Engine.Map.GetActiveChunks();
            //chunks.AddOrUpdate(chunk.MapCoords, chunk, (vec2, old) => //chunk);
            //    {
            //        // BECAUSE I'VE ALREADY STARTED RECEIVING LIGHT ON EMPTY CHUNKS
            //        old.CopyFrom(chunk);
            //        return old;
            //    });
            Instance.Map.AddChunk(chunk);


            //chunk.ResetVisibleCells();
            //chunk.ResetVisibleOuterBlocks();
            return;
            foreach (var vector in chunk.MapCoords.GetNeighbors())
            {
                Chunk neighbor;
                if (Instance.Map.GetActiveChunks().TryGetValue(vector, out neighbor))
                {
                    neighbor.ResetVisibleOuterBlocks();
                    neighbor.LightCache2.Clear();
                }
            }
            return;
            foreach (var vector in chunk.MapCoords.GetNeighbors())
            {
                Chunk neighbor;
                if (!Instance.Map.GetActiveChunks().TryGetValue(vector, out neighbor))
                    continue;
                //neighbor.InvalidateEdges();//.UpdateEdges(Instance.Map);//, v => Instance.SyncCell(v));
                //neighbor.ClearLightCache();
            }

            // the server does that when it generates the chunk
            //Chunk neighbor;
            //if (this.Map.ActiveChunks.TryGetValue(chunk.MapCoords + new Vector2(1, 0), out neighbor))
            //    neighbor.UpdateEdges(this.Map);
            //if (this.Map.ActiveChunks.TryGetValue(chunk.MapCoords + new Vector2(-1, 0), out neighbor))
            //    neighbor.UpdateEdges(this.Map);
            //if (this.Map.ActiveChunks.TryGetValue(chunk.MapCoords + new Vector2(0, 1), out neighbor))
            //    neighbor.UpdateEdges(this.Map);
            //if (this.Map.ActiveChunks.TryGetValue(chunk.MapCoords + new Vector2(0, -1), out neighbor))
            //    neighbor.UpdateEdges(this.Map);
            //chunk.UpdateEdges(this.Map);

            ConcurrentQueue<Action<Chunk>> queue;
            if (ChunkCallBackEvents.TryRemove(chunk.MapCoords, out queue))
                while (queue.Count > 0)
                {
                    Action<Chunk> callBack;
                    if (queue.TryDequeue(out callBack))
                        callBack(chunk);
                }
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
            obj.Net = this;
            SpawnObject(obj);
        }
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
            SpawnObject(obj);
        }
        public void Spawn(GameObject obj, GameObjectSlot slot)
        {

        }
        void SpawnObject(GameObject obj)
        {
            if (obj.Network.ID == 0)
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

        static public void AddObject(GameObject obj)
        {
            Packet.Create(PacketID, PacketType.InstantiateObject, Network.Serialize(obj.Write)).BeginSendTo(Host, RemoteIP);
        }
        static public void AddObject(GameObject obj, Vector3 global)
        {
            Packet.Create(PacketID, PacketType.InstantiateAndSpawnObject, Network.Serialize(w =>
            {
                obj.Write(w);
                //w.Write(global);
                Position.Write(w, global, Vector3.Zero);
            })).BeginSendTo(Host, RemoteIP);
        }
        static public void AddObject(GameObject obj, Vector3 global, Vector3 speed)
        {
            obj.SetGlobal(global).Velocity = speed;
            Packet.Create(PacketID, PacketType.InstantiateObject, Network.Serialize(obj.Write)).BeginSendTo(Host, RemoteIP);
        }
        static public void RemoveObject(GameObject obj)
        {
            if (obj.IsNull())
                throw new ArgumentNullException();
            //if (obj.ID == GameObject.Types.Actor)
            //    return;
            if (obj.IsPlayerEntity())
                return;
            //Packet.Create(PacketID, PacketType.ObjectDestroy, Network.Serialize(obj.Write)).BeginSendTo(Host, RemoteIP);
            Packet.Create(PacketID, PacketType.DisposeObject, Network.Serialize(w => TargetArgs.Write(w, obj))).BeginSendTo(Host, RemoteIP);
        }


        public void InstantiateInContainer(GameObject obj, GameObject container, byte containerID, byte slotID, byte amount)
        { }

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
            GameObject obj;
            NetworkObjects.TryGetValue(netID, out obj);
            return obj;
        }
        public bool TryGetNetworkObject(int netID, out GameObject obj)
        {
            return NetworkObjects.TryGetValue(netID, out obj);
        }

        //static public void Post(Packet packet)
        //{
        //    MessageQueue.Enqueue(packet);
        //}

        /// <summary>
        /// syncs the object provided by the server
        /// </summary>
        /// <param name="obj"></param>
        //public void Sync(GameObject obj)
        //{
        //    //GameObject obj = Network.Deserialize<GameObject>(
        //    //GameObject existing;
        //    //if(TryGetNetworkObject(obj.NetworkID, out existing))
        //    //    existing.Read(
        //}

        HashSet<int> ChangedObjects = new HashSet<int>();
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

        void ReadSnapshot(BinaryReader reader)
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
                    Console.Write(Color.Orange, "CLIENT", "Networked object doesn't exist on client"); // SOMETIMES THIS HAPPENS WHEN A SNAPSHOT IS RECEIVED CONTAINED A NEWLY INSTANTIATED ITEM, BEFORE THE PACKET THAT INSTANTIATES IT ARRIVES
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
                WorldSnapshot discarded = WorldStateBuffer.Dequeue();
                //if (discarded.EventSnapshots.Count > 0)
                //    Client.Console.Write(Color.Yellow, "CLIENT", "WARNING! " + discarded.EventSnapshots.Count + " events discarded with world snapshot " + discarded.ToString());
            }
        }

        void ReadSnapshot(byte[] data)
        {
            data.Translate(reader =>
            {
                double totalMs = reader.ReadDouble();
                SyncTime(totalMs); // attempt to sync time here instead of receiving separate synctime packets from server

                TimeSpan time = TimeSpan.FromMilliseconds(totalMs);
                WorldSnapshot worldState = new WorldSnapshot() { Time = time };

                int count = reader.ReadInt32();
                for (int i = 0; i < count; i++)
                {
                    GameObject obj;
                    int netID = reader.ReadInt32();
                    obj = Instance.NetworkObjects.GetValueOrDefault(netID);
                    var objsnapshot = ObjectSnapshot.Create(time, obj, reader);
                    if (Server.Instance.GetNetworkObject(netID) == this.GetNetworkObject(netID))
                        throw new Exception("TI STO DIAOLO");
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
            });
        }
        private static void UpdateWorldState()
        {
            // iterate through the state buffer and find position
            List<WorldSnapshot> list = WorldStateBuffer.ToList();
            for (int i = 0; i < WorldStateBuffer.Count - 1; i++)
            {
                WorldSnapshot
                    prev = list[i],
                    next = list[i + 1];


                //ExecuteEvents(prev); // TODO: find better way to execute missed events

                if (prev.Time <= ClientClock &&
                    ClientClock < next.Time)
                {
                    LerpObjectPositions(prev, next);

                    //ExecuteEvents(next);
                    return;
                }
                else if (ClientClock == next.Time)
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

        private static void SnapObjectPositions(WorldSnapshot prev, WorldSnapshot next)
        {
            foreach (var objSnapshot in next.ObjectSnapshots)
            {
                ObjectSnapshot previousObjState = prev.ObjectSnapshots.Find(o => o.Object == objSnapshot.Object);
                if (previousObjState.IsNull())
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


        private static void LerpObjectPositions(WorldSnapshot prev, WorldSnapshot next)
        {
            //interpolate between states, apply and return
            foreach (var nextObjSnapshot in next.ObjectSnapshots)
            {
                // only update objects that have a snapshot in the next worldstate
                // (don't update objects that their state change stopped in the previous snapshot)

                // continue if there's not an initial value to interpolate from (the object will start changing at the next snapshot)
                // TODO: find a way to not have to search for objects
                ObjectSnapshot previousObjState = prev.ObjectSnapshots.Find(o => o.Object == nextObjSnapshot.Object);

                if (previousObjState.IsNull())
                    continue;

                // smooth error in prediction
                // for objects that have moved locally (client prediction) before the server update arrived
                // get current (predicted) object state
                //  ObjectSnapshot predicted = new ObjectSnapshot(objSnapshot.Object) { Time = previousObjState.Time };

                // get interpolated state
                // maybe use something more precise than clientclock, since it ticks at the same rate as the server sends snapshots???
                ObjectSnapshot interpolatedState = previousObjState.Interpolate(nextObjSnapshot, ClientClock);

                //   ObjectSnapshot afterSmoothing = predicted.Interpolate(interpolatedState, ClientClock);


                // apply values
                //objSnapshot.Object.Global = interpolatedState.Position;
                //objSnapshot.Object.Velocity = interpolatedState.Velocity;

                // smooth between predicted and actual values
                // correct if outside error margin
                float errorMargin = 0.5f;
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
                    nextObjSnapshot.Object.ChangePosition(interpolatedState.Position);
                    //objSnapshot.Object.ChangePosition(Engine.Map, interpolatedState.Position);
                    nextObjSnapshot.Object.Velocity = interpolatedState.Velocity;
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
                writer.Write(ClientClock.TotalMilliseconds);
                TargetArgs.Write(writer, Player.Actor);
                ObjectEventArgs.Write(writer, type, dataWriter);
            }).Send(PacketID, PacketType.PlayerInputOld, Host, RemoteIP);
        }
        static public void PlayerInventoryOperation(GameObject inventoryOwner, GameObjectSlot source, GameObjectSlot destination, int amount)
        {
            Network.Serialize(writer =>
            {
                TargetArgs.Write(writer, inventoryOwner);
                TargetArgs.Write(writer, source);
                TargetArgs.Write(writer, destination);
                writer.Write(amount);
            }).Send(PacketID, PacketType.PlayerInventoryOperation, Host, RemoteIP);
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
            }).Send(PacketID, PacketType.PlayerInventoryOperationNew, Host, RemoteIP);
        }
        static public void PlayerInventoryOperationNew(TargetArgs source, TargetArgs target, int amount)
        {
            Network.Serialize(w =>
            {
                source.Write(w);
                target.Write(w);
                w.Write(amount);
            }).Send(PacketID, PacketType.PlayerInventoryOperationNew, Host, RemoteIP);
        }
        static public void PlayerInventoryOperationOld(GameObject inventoryOwner, Action<BinaryWriter> dataWriter)
        {
            Network.Serialize(writer =>
            {
                writer.Write(ClientClock.TotalMilliseconds);
                TargetArgs.Write(writer, inventoryOwner);
                dataWriter(writer);
            }).Send(PacketID, PacketType.PlayerInventoryOperationOld, Host, RemoteIP);
        }
        static public void PlayerSlotInteraction(GameObjectSlot slot)
        {
            Network.Serialize(writer =>
            {
                TargetArgs.Write(writer, Player.Actor);
                TargetArgs.Write(writer, slot);
            }).Send(PacketID, PacketType.PlayerSlotClick, Host, RemoteIP);
        }
        static public void PlayerSlotRightClick(TargetArgs parent, GameObject child)
        {
            Network.Serialize(writer =>
            {
                TargetArgs.Write(writer, Player.Actor);
                parent.Write(writer);
                TargetArgs.Write(writer, child);
            }).Send(PacketID, PacketType.PlayerSlotRightClick, Host, RemoteIP);
        }
        static public void PostPlayerInput(GameObject recipient, Components.Message.Types type, Action<BinaryWriter> dataWriter, bool clientPrediction = true)
        {
            //if (clientPrediction)
            //    Player.Actor.PostMessageLocal(type, Player.Actor, Client.Instance, dataWriter);
            Network.Serialize(writer =>
            {
                writer.Write(ClientClock.TotalMilliseconds);
                TargetArgs.Write(writer, recipient);
                ObjectEventArgs.Write(writer, type, dataWriter);
                //writer.Write(ClientClock.TotalMilliseconds);
                //writer.Write((byte)type);
                //TargetArgs.Write(writer, recipient);
                //dataWriter(writer);
            }).Send(PacketID, PacketType.PlayerInputOld, Host, RemoteIP);
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
            }).Send(PacketID, PacketType.PlayerSetBlock, Host, RemoteIP);
        }
        internal static void PlayerRemoveBlock(Vector3 global)
        {
            Network.Serialize(w =>
            {
                w.Write(global);
            }).Send(PacketID, PacketType.PlayerRemoveBlock, Host, RemoteIP);
        }
        internal static void PlayerStartMoving()
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
            }).Send(PacketID, PacketType.PlayerStartMoving, Host, RemoteIP);
        }
        internal static void PlayerStopMoving()
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
            }).Send(PacketID, PacketType.PlayerStopMoving, Host, RemoteIP);
        }
        internal static void PlayerChangeDirection(Vector3 direction)
        {
            // assign direction directly for prediction?
            Player.Actor.Direction = direction;
            new PacketPlayerChangeDirection(Player.Actor, direction).Send(Host, RemoteIP);
        }
        internal static void PlayerJump()
        {
            //Player.Actor.Global.ToConsole();

            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
            }).Send(PacketID, PacketType.PlayerJump, Host, RemoteIP);
        }
        internal static void PlayerToggleWalk(bool toggle)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                w.Write(toggle);
            }).Send(PacketID, PacketType.PlayerToggleWalk, Host, RemoteIP);
        }
        internal static void PlayerToggleSprint(bool toggle)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                w.Write(toggle);
            }).Send(PacketID, PacketType.PlayerToggleSprint, Host, RemoteIP);
        }
        internal static void PlayerInteract(TargetArgs target)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                target.Write(w);
            }).Send(PacketID, PacketType.PlayerInteract, Host, RemoteIP);
        }
        internal static void PlayerUseInteraction(TargetArgs target, string interactionName)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                target.Write(w);
                w.Write(interactionName);
            }).Send(PacketID, PacketType.PlayerUseInteraction, Host, RemoteIP);
        }
        internal static void PlayerUse(TargetArgs target)
        {
            if (target.IsNull())
                return;
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                target.Write(w);
            }).Send(PacketID, PacketType.PlayerUse, Host, RemoteIP);
        }
        internal static void PlayerUseHauled(TargetArgs targetArgs)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                targetArgs.Write(w);
            }).Send(PacketID, PacketType.PlayerUseHauled, Host, RemoteIP);
        }
        internal static void PlayerDropHauled(TargetArgs targetArgs)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                targetArgs.Write(w);
            }).Send(PacketID, PacketType.PlayerDropHauled, Host, RemoteIP);
        }
        internal static void PlayerPickUp(TargetArgs targetArgs)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                targetArgs.Write(w);
            }).Send(PacketID, PacketType.PlayerPickUp, Host, RemoteIP);
        }
        internal static void PlayerCarry(TargetArgs targetArgs)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                targetArgs.Write(w);
            }).Send(PacketID, PacketType.PlayerCarry, Host, RemoteIP);
        }
        internal static void PlayerEquip(TargetArgs targetArgs)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                targetArgs.Write(w);
            }).Send(PacketID, PacketType.PlayerEquip, Host, RemoteIP);
        }
        internal static void PlayerUnequip(TargetArgs targetArgs)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                targetArgs.Write(w);
            }).Send(PacketID, PacketType.PlayerUnequip, Host, RemoteIP);
        }
        internal static void PlayerThrow(Vector3 dir, bool all)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                w.Write(dir);
                w.Write(all);
            }).Send(PacketID, PacketType.EntityThrow, Host, RemoteIP);
        }
        internal static void PlayerAttack()
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
            }).Send(PacketID, PacketType.PlayerStartAttack, Host, RemoteIP);
        }
        internal static void PlayerFinishAttack(Vector3 vector3)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                w.Write(vector3);
            }).Send(PacketID, PacketType.PlayerFinishAttack, Host, RemoteIP);
        }
        internal static void PlayerStartBlocking()
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
            }).Send(PacketID, PacketType.PlayerStartBlocking, Host, RemoteIP);
        }
        internal static void PlayerFinishBlocking()
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
            }).Send(PacketID, PacketType.PlayerFinishBlocking, Host, RemoteIP);
        }
        //static public void PlayerConstruct(Components.Crafting.BlockConstruction.ProductMaterialPair construction, Vector3 global)
        //{
        //    Packet.Create(PacketID, PacketType.PlaceBlockConstruction, Network.Serialize(w =>
        //    {
        //        //w.Write((int)type);
        //        //w.Write(data);
        //        w.Write(Player.Actor.Network.ID);
        //        construction.Write(w);
        //        w.Write(global);
        //    })).BeginSendTo(Host, RemoteIP);
        //}
        static public void PlayerBuild(Components.Crafting.Reaction.Product.ProductMaterialPair product, Vector3 global)
        {
            Packet.Create(PacketID, PacketType.PlaceConstruction, Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                product.Write(w);
                w.Write(global);
            })).BeginSendTo(Host, RemoteIP);
        }
        internal static void PlayerCraft(Components.Crafting.Reaction.Product.ProductMaterialPair product)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                product.Write(w);
            }).Send(PacketID, PacketType.PlayerCraftRequest, Host, RemoteIP);
        }
        internal static void PlayerCraftRequest(Components.Crafting.CraftOperation crafting)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                crafting.WriteOld(w);
            }).Send(PacketID, PacketType.PlayerCraftRequest, Host, RemoteIP);
        }
        internal static void PlayerDropInventory(byte slotID, int amount)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                w.Write(slotID);
                w.Write(amount);
            }).Send(PacketID, PacketType.PlayerDropInventory, Host, RemoteIP);
        }
        //internal static void PlayerRemoteCall(GameObject target, Message.Types types)
        internal static void PlayerRemoteCall(TargetArgs target, Message.Types types)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                target.Write(w);
                w.Write((int)types);
            }).Send(PacketID, PacketType.PlayerRemoteCall, Host, RemoteIP);
        }
        //internal static void PlayerRemoteCall(GameObject target, Message.Types type, Action<BinaryWriter> argsWriter)
        internal static void PlayerRemoteCall(TargetArgs target, Message.Types type, Action<BinaryWriter> argsWriter)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                target.Write(w);
                w.Write((int)type);
                argsWriter(w);
            }).Send(PacketID, PacketType.PlayerRemoteCall, Host, RemoteIP);
        }

        internal static void PlayerInput(TargetArgs targetArgs, PlayerInput input)
        {
            Network.Serialize(w =>
            {
                w.Write(Player.Actor.Network.ID);
                targetArgs.Write(w);
                input.Write(w);
            }).Send(PacketID, PacketType.PlayerInput, Host, RemoteIP);
        }
        internal static void PlayerCommand(string command)
        {
            //Packet.Create(Net.Client.PacketID, PacketType.PlayerServerCommand, Net.Network.Serialize(writer =>
            //{
            //    writer.WriteASCII(gotText.TrimStart('/'));
            //})).BeginSendTo(Net.Client.Host, Net.Client.RemoteIP);

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
                        chunk.Rebuild();
                    }
                    return;

                default:
                    break;
            }

            Net.Network.Serialize(writer =>
            {
                writer.WriteASCII(command);
            }).Send(PacketID, PacketType.PlayerServerCommand, Host, RemoteIP);
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
        public void PopLoot(Components.LootTable table, GameObject parent) { }
        //public void PopLoot(GameObject parent, GameObject obj) { }
        public void PopLoot(GameObject loot, Vector3 startPosition, Vector3 startVelocity) { }
        public void PopLoot(Components.LootTable table, Vector3 startPosition, Vector3 startVelocity) { }
        public List<GameObject> GenerateLoot(Components.LootTable loot) { return new List<GameObject>(); }

        internal static void RequestNewObject(GameObject gameObject, byte amount)
        {
            Packet.Create(PacketID, PacketType.RequestNewObject, Network.Serialize(w =>
            {
                //TargetArgs.Write(w, gameObject);
                gameObject.Clone().Write(w);
                w.Write(amount);
            })).BeginSendTo(Host, RemoteIP);
        }
        internal static void Request(PacketType type, Action<BinaryWriter> writer)
        {
            Packet.Create(PacketID, type, Network.Serialize(w =>
            {
                writer(w);
            })).BeginSendTo(Host, RemoteIP);
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
            this.Map.GetChunk(sourceParent.Global).Invalidate();
            this.Map.GetChunk(destinationParent.Global).Invalidate();
            Network.InventoryOperation(this, source, destination, amount);
        }
        void InventoryOperation(GameObject parent, Components.ArrangeChildrenArgs args)
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
            Chunk chunk;
            if (!Instance.Map.GetActiveChunks().TryGetValue(chunkPos, out chunk))
                return;
            //if (!Instance.Map.ActiveChunks.TryRemove(chunkPos, out chunk))
            //return;
            Instance.Map.GetActiveChunks().Remove(chunkPos);

            foreach (var obj in chunk.GetObjects())
                //Sync // don't sync dispose cause client will dispose object when it receives the unload chunk packet
                this.DisposeObject(obj);
            if (chunkPos == new Vector2(-1, 1))
                (chunkPos.ToString() + " unloaded, player chunk: " + Player.Actor.Global.GetChunkCoords().ToString()).ToConsole();
        }



        public static GameObjectSlot sourceSlot { get; set; }

        public static GameObjectSlot destinationSlot { get; set; }

        public void Forward(Packet p)
        {

        }

    }
}
