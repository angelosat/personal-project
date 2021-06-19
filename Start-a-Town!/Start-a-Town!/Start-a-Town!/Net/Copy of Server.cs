using System;
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

namespace Start_a_Town_.Net
{
    class Server : IObjectProvider//, IObjectFactory
    {
        // TODO: figure out why it's glitching out if I set it lower than 10
        public const int ClockIntervalMS = 10;// 10 is working
        static TimeSpan TimeStarted;// = new TimeSpan();
        static TimeSpan ServerClock;
        static System.Threading.Timer
            ServerClockTimer,
            SnapshotTimer,
            SaveTimer;

        static int SaveIntervalMS = 10000; // save changed chunks every 10 seconds
        //static int SaveInterval = Engine.TargetFps * 2;
        //static int SaveTimer = SaveInterval;

        //public const int SnapshotIntervalMS = 50;// send 20 snapshots per second to clients
        public const int SnapshotIntervalMS = 10;// send 60 snapshots per second to clients
        //    SnapshotTimer = SnapshotIntervalMS;

        ConcurrentDictionary<int, GameObject> NetworkObjects;

        /// <summary>
        /// Contains objects that have changed since the last world delta state update
        /// </summary>
        public HashSet<int> ObjectsChangedSinceLastSnapshot = new HashSet<int>();
        public ConcurrentQueue<EventSnapshot> EventsSinceLastSnapshot = new ConcurrentQueue<EventSnapshot>();

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
        Server()
        {
            ServerClockTimer = new System.Threading.Timer((a) =>
            {
                ServerClock = ServerClock.Add(TimeSpan.FromMilliseconds(ClockIntervalMS));
                //SyncTime();
            }, new object(), 0, ClockIntervalMS);
            SnapshotTimer = new System.Threading.Timer((a) =>
            {
                // TODO: find a way to not have to check this
                if (!Map.IsNull())
                    SendSnapshots(ServerClock);
            }, new object(), 0, SnapshotIntervalMS);
            SaveTimer = new System.Threading.Timer((a) =>
            {
                Save();
            }, new object(), 0, SaveIntervalMS);
            NetworkObjects = new ConcurrentDictionary<int, GameObject>();
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

        public Map GetMap()
        {
            return Map;
        }
        static Map Map;
        static int Port = 5541;
        static Socket Listener;
        public static PlayerList Players;// = new List<IPAddress>();
        //public static List<Socket> Sockets;
        static public event EventHandler OnPlayerConnect, OnPlayersChanged;
        static ManualResetEvent Block;
        static ConcurrentQueue<Packet> MessageQueue = new ConcurrentQueue<Packet>();
        static ConcurrentQueue<Packet> OutgoingMessages = new ConcurrentQueue<Packet>();
        //static ConcurrentQueue<Message> ChunkRequests = new ConcurrentQueue<Message>();
        static ConcurrentDictionary<Vector2, ConcurrentQueue<PlayerData>> ChunkRequests = new ConcurrentDictionary<Vector2, ConcurrentQueue<PlayerData>>();
        static public RandomThreaded Random;

        static public void Stop()
        {
            Listener.Close();
            Network.Console.Write("[SERVER] Stopped");
        }
        static public void Start()
        {
            //NetworkObjects = new ConcurrentDictionary<int, GameObject>();
            TimeStarted = DateTime.Now.TimeOfDay;
            ServerClock = new TimeSpan();
            
            UI.LobbyWindow.Instance.Console.Write("[SERVER] Started...");
            //Listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Udp);
            Listener = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            Players = new PlayerList();// List<PlayerData>();
            IPEndPoint ip = new IPEndPoint(IPAddress.Any, Port);
            Block = new ManualResetEvent(false);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Listener.Bind(ip);
                    Listener.Listen(4);
                    while (true)
                    {
                        Block.Reset();
                        Listener.BeginAccept(ar =>
                        {
                            try
                            {
                             //   Listener.EndAccept(ar);
                                OnConnect(ar);
                                Block.Set();
                            }
                            catch (ObjectDisposedException e) { }
                            //ReceivePlayerData(ar);
                        }, Listener);
                        Block.WaitOne();
                    }
                }
                catch (Exception e)
                {
                    //System.Windows.Forms.MessageBox.Show(e.Message);
                    ScreenManager.Remove();
                    e.ShowDialog();
                    //UI.MessageBox.Create(e.ToString(), "ErrorCode: " + e.ErrorCode.ToString(), () => ScreenManager.Remove(), () => ScreenManager.Remove()).ShowDialog();
                }
                //Socket.BeginAccept(new AsyncCallback(OnConnect), new object());
                
            });
            UI.LobbyWindow.Instance.Console.Write("[SERVER] Listening to port " + Port + "...");
        }

        static public void Update(GameTime gt)
        {
            //CurrentTime = gt.TotalGameTime - TimeStarted;
            Packet msg;
            while (MessageQueue.TryDequeue(out msg))
                HandleMessage(msg);

            if (Map.IsNull())
                return;

            //SaveTimer -= 1;
            //if (SaveTimer < 0)
            //{
            //    Map.SaveServer();
            //    SaveTimer = SaveInterval;
            //}

            //SnapshotTimer -= 1;
            //if (SnapshotTimer < 0)
            //{
            //    SendObjectStates(CurrentTime);
            //    SnapshotTimer = SnapshotIntervalMS;
            //}
            Map.Update(Instance);
            SendOutgoing();
            SendChunks();
            
        }
        //static void SendOutgoing()
        //{
        //    Packet packet;
        //    if (OutgoingMessages.TryDequeue(out packet))
        //    {
        //        packet.Send(ar =>
        //        {
        //            ("sent " + packet.PacketType.ToString() + " " + (packet.Length + 5).ToString() + " bytes").ToConsole();
        //            SendOutgoing();
        //        });
        //    }
        //}
        static void SendOutgoing()
        {
            Packet packet;
            while (OutgoingMessages.TryDequeue(out packet))
            {
               //("sent " + packet.PacketType.ToString()).ToConsole();
               // packet.Send();
                packet.Player.Socket.Send(packet.ToArray());
                ("server sent " + packet.PacketType.ToString()).ToConsole();
            }
        }
        

        static void ReceiveMessage(IAsyncResult ar)
        {
            ObjectState state = (ObjectState)ar.AsyncState;
            try
            {
                Packet msg = Packet.Create(state.Buffer);
                int bytesReceived = state.Socket.EndReceive(ar);

                //("received " + msg.PacketType.ToString() + " (" + state.Socket.EndReceive(ar).ToString() + " bytes)").ToConsole();
                msg.Sender = state.Socket;
                msg.Player = state.Player;
                MessageQueue.Enqueue(msg);


                state.Buffer = new byte[Packet.Size];
                state.Socket.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, ReceiveMessage, state);
            }
            catch (SocketException e)
            {
                "connection closed".ToConsole();
                //Players.RemoveAll(p => p.Socket == state.Socket);
                RemovePlayer(state.Player);
            }
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
            switch (msg.PacketType)
            {
                case PacketType.RequestPlayerID:
                    PlayerData temp = Network.Deserialize<PlayerData>(msg.Payload, PlayerData.Read);
                    msg.Player.Name = temp.Name;
                    msg.Player.ID = PlayerID;
                    Players.Add(msg.Player);
                    //Packet.Create(msg.PacketType, Network.Serialize(w => w.Write(msg.Player.ID))).Send(msg.Player.Socket);
                    //Packet.Create(PacketType.PlayerConnecting, Network.Serialize(msg.Player.Write)).Send(from p in Players select p.Socket);

                    OutgoingMessages.Enqueue(Packet.Create(msg.PacketType, Network.Serialize(w => w.Write(msg.Player.ID)), msg.Player));//.Send(msg.Player.Socket);
                    foreach(var player in from p in Players select p)
                        OutgoingMessages.Enqueue(Packet.Create(PacketType.PlayerConnecting, Network.Serialize(msg.Player.Write), player));//.Send(from p in Players select p.Socket);
                    return;

                //case PacketType.PlayerConnecting:
                //    PlayerData temp = Network.Deserialize<PlayerData>(msg.Payload, PlayerData.Read);
                //    msg.Player.Name = temp.Name;
                //    msg.Player.ID = PlayerID;
                //    Players.Add(msg.Player);
                //    Packet.Create(msg.PacketType, Net.Network.Serialize(msg.Player.Write)).Send(from p in Players select p.Socket);
                //    //break;
                    //return;

                case PacketType.PlayerData:
                    "playerdata".ToConsole();
                    break;

                case PacketType.PlayerList:
                    "playerlist".ToConsole();
                    break;

                case PacketType.RequestMapInfo:
                    SendMapInfo(msg.Player);
                    return;
                    

                case PacketType.RequestWorldInfo:
                    SendWorldInfo(msg.Player);
                    return;

                case PacketType.RequestChunk:
                    Vector2 vec2 = Network.Deserialize<Vector2>(msg.Payload, reader =>
                    {
                        return reader.ReadVector2();
                    });
                    HandleChunkRequest(msg.Player, vec2);
                    return;

                case PacketType.PlayerEnteredWorld:
                    GameObject obj = Network.Deserialize<GameObject>(msg.Payload, GameObject.Create);

                    obj.Instantiate(Instance);
                    msg.Player.CharacterID = obj.NetworkID;
                    // msg.Player.Character = obj;
                    Instance.Spawn(obj);

                    // send only playerdata because it includes their character object and will be serialized along with other info
                    //Packet.Create(PacketType.ObjectCreate, Net.Network.Serialize(obj.Write)).Send(from p in Players select p.Socket); // send to sender too to create their character
                    //Packet.Create(msg.PacketType, Net.Network.Serialize(msg.Player.Write)).Send(from p in Players select p.Socket);
                    foreach(var player in from p in Players select p)
                    {
                        OutgoingMessages.Enqueue( Packet.Create(PacketType.ObjectCreate, Net.Network.Serialize(obj.Write), player)); // send to sender too to create their character
                        OutgoingMessages.Enqueue(Packet.Create(msg.PacketType, Net.Network.Serialize(msg.Player.Write), player)); 
                    }
                    // send a message to the newly connected client to own their character
                    OutgoingMessages.Enqueue(Packet.Create(PacketType.AssignCharacter, Network.Serialize(writer => writer.Write(obj.NetworkID)), msg.Player));//.Send(msg.Sender);

                    return;

                case PacketType.PlayerInput:
                    Network.Deserialize(msg.Payload, reader =>
                    {
                        Components.Message.Types type = (Components.Message.Types)reader.ReadByte();
                        GameObject target = Instance.NetworkObjects[msg.Player.CharacterID];

                        List<byte> p = new List<byte>();

                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                            p.Add(reader.ReadByte());

                        target.PostMessage(type, target, Instance, p.ToArray());
                    });
                    return;

                case PacketType.ObjectEvent:
                    Network.Deserialize(msg.Payload, reader =>
                    {
                        Components.Message.Types type = (Components.Message.Types)reader.ReadByte();
                        bool isBlock = reader.ReadBoolean();
                        GameObject target;
                        if (isBlock)
                        {
                            Vector3 global = reader.ReadVector3();
                            if (!Cell.TryGetObject(Map, global, out target))
                                throw new Exception("Could not create block object at " + global.ToString());
                        }
                        else
                        {
                            int targetID = reader.ReadInt32();
                            target = Instance.NetworkObjects[targetID];
                        }

                        List<byte> p = new List<byte>();

                        while (reader.BaseStream.Position < reader.BaseStream.Length)
                            p.Add(reader.ReadByte());

                        // receive timestamp from client to use instead of server timestamp (or store both?)
                        Instance.EventsSinceLastSnapshot.Enqueue(new EventSnapshot() { Data = msg.Payload }); //Time = ServerClock, 
                        
                        target.PostMessage(type, target, Instance, p.ToArray());
                    });
                    
                    // TODO: find way to relay object events that aren't player initiated, to the players, along with the world snapshots
                    return;
                    //break; // send world state changes periodically (delta states)// instead of immediately

                case PacketType.ObjectCreate:
                    obj = Network.Deserialize<GameObject>(msg.Payload, GameObject.Create);
                    CreateAndSyncObject(obj);
                    return;

                case PacketType.ObjectDestroy:

                    //if (Instance.NetworkObjects.TryRemove(Network.Deserialize<GameObject>(msg.Payload, GameObject.Create).NetworkID, out obj))
                    //    obj.Remove();
                    Instance.DestroyObject(Network.Deserialize<GameObject>(msg.Payload, GameObject.Create).NetworkID);
                    break;

                default:

                    break;
            }

            // send world state changes periodically (delta states)
            foreach (var p in Players)
                msg.Send(p.Socket);
        }

        private static void CreateAndSyncObject(GameObject obj)
        {
            InstantiateAndSpawn(obj);
            Players.ForEach(p => Packet.Create(PacketType.ObjectCreate, Network.Serialize(obj.Write)).Send(p.Socket));
        }

        private static void InstantiateAndSpawn(GameObject obj)
        {
            obj.Instantiate(Instance);
            Instance.Spawn(obj);
        }

        static void OnConnect(IAsyncResult ar)
        {
            Socket sock = Listener.EndAccept(ar);
            SendPlayerList(sock);
            SyncTime();
            ObjectState state = new ObjectState(sock.RemoteEndPoint.ToString(), sock) { Buffer = new byte[Packet.Size], Player = new PlayerData(sock) };
            sock.BeginReceive(state.Buffer, 0, state.Buffer.Length, SocketFlags.None, a => ReceiveMessage(a), state);

        }

        //private static void SyncTime(Socket sock)
        //{
        //    Packet.Create(PacketType.SyncTime, Network.Serialize(w=>w.Write(ServerClock.TotalMilliseconds))).Send(sock);
        //}
        private static void SyncTime()
        {
            //Packet.Create(PacketType.SyncTime, Network.Serialize(w => w.Write(ServerClock.TotalMilliseconds))).Send(from p in Players select p.Socket);
            Players.ForEach(player => Packet.Create(PacketType.SyncTime, Network.Serialize(w => w.Write(ServerClock.TotalMilliseconds)), player));//).Send(from p in Players select p.Socket);
        }

        static void RemovePlayer(PlayerData player)
        {
            Players.Remove(player);
            Instance.DestroyObject(player.CharacterID);
            Players.ForEach(p => Packet.Create(PacketType.PlayerDisconnected, Network.Serialize(player.Write)).Send(p.Socket));
        }


        /// <summary>
        /// Removes an object from the game world without releasing its NetworkID.
        /// </summary>
        /// <param name="obj"></param>
        public void Despawn(GameObject obj)
        {
            obj.Remove();
        }
        public void Spawn(GameObject obj)
        {
            Chunk.AddObject(obj, Map);
        }

        /// <summary>
        /// Both removes an object form the game world and releases its networkID
        /// </summary>
        /// <param name="objNetID"></param>
        public void DestroyObject(int objNetID)
        {
            GameObject o;
            if (NetworkObjects.TryRemove(objNetID, out o))
                //o.Remove();
                Despawn(o);
        }
        public void DestroyObject(GameObject obj)
        {
            GameObject o;
            if (NetworkObjects.TryRemove(obj.NetworkID, out o))
                Despawn(o);
            else
                throw new Exception("Object mismatch!");
        }
        static void PlayerNameChange(PlayerData player, string name)
        {
            player.Name = name;
            OnPlayersChanged(null, EventArgs.Empty);
        }

        static void SendPlayerList(Socket client)
        {
            Packet.Create(PacketType.PlayerList, Network.Serialize(Players.Write)).Send(new ObjectState("Server", client));
        }

        static public void LoadMap(Map map)
        {
            Map = map;
            map.ActiveChunks = new System.Collections.Concurrent.ConcurrentDictionary<Vector2, Chunk>();
            Random = new RandomThreaded(Map.World);
            //  ChunkLoader.LoadAsync(map, new System.Threading.CancellationToken(), chunk => UI.Nameplate.Initialize(chunk));
        }

        private static void SendMapInfo(PlayerData player)
        {
            byte[] data = Network.Serialize(Map.GetData); // why does it let me do that?

         //   byte[] msg = Packet.ToArray(PacketType.MapData, mapData);
            if (player.IsNull())
                //Players.ForEach(p => p.Socket.BeginSend(msg, 0, msg.Length, SocketFlags.None, a => ("map data sent " + msg.Length + " (bytes)").ToConsole(), p.Socket));
                Players.ForEach(p => OutgoingMessages.Enqueue(Packet.Create(PacketType.MapData, data, p)));
            else
                //sender.BeginSend(msg, 0, msg.Length, SocketFlags.None, a => ("map data sent " + msg.Length + " (bytes)").ToConsole(), sender);
                OutgoingMessages.Enqueue(Packet.Create(PacketType.MapData, data, player));
            "map info queued".ToConsole();
        }

        private static void SendWorldInfo(PlayerData player)
        {
            byte[] data = Network.Serialize(Map.World.WriteData);

           // byte[] msg = Packet.ToArray(PacketType.WorldInfo, data);
            if (player.IsNull())
                //Players.ForEach(p => p.Socket.BeginSend(msg, 0, msg.Length, SocketFlags.None, a => ("world info sent " + msg.Length + " (bytes)").ToConsole(), p.Socket));
                Players.ForEach(p => OutgoingMessages.Enqueue(Packet.Create(PacketType.WorldInfo, data, p)));
            else
                OutgoingMessages.Enqueue(Packet.Create(PacketType.WorldInfo, data, player));
                //player.BeginSend(msg, 0, msg.Length, SocketFlags.None, a => ("world info sent " + msg.Length + " (bytes)").ToConsole(), sender);
            "world info queued".ToConsole();
        }

        //static void SendChunk(Socket socket, Vector2 vec2)
        //{
        //    ChunkLoader.LoadAsync(Map, vec2, new CancellationToken(), chunk =>
        //    {
        //        byte[] data = Network.Serialize(chunk.Write);
        //        //List<byte> msg = new List<byte>();
        //        //msg.Add((byte)MessageType.Chunk);
        //        //msg.Add(byte.MaxValue);
        //        //msg.AddRange(data);
        //        byte[] msg = Message.Create(MessageType.Chunk, data);
        //        socket.BeginSend(msg, 0, msg.Length, SocketFlags.None, a => (vec2 + " data sent " + data.Length + " (bytes)").ToConsole(), socket);

        //    });
        //}

        //static void HandleChunkRequest(Socket socket, Vector2 vec2)
        static void HandleChunkRequest(PlayerData player, Vector2 vec2)
        {
            Chunk chunk;
            if (Map.ActiveChunks.TryGetValue(vec2, out chunk))
            {
                byte[] data = Network.Serialize(chunk.Write);
                byte[] msg = Packet.ToArray(PacketType.Chunk, data);
             //   player.Socket.BeginSend(msg, 0, msg.Length, SocketFlags.None, a => (vec2 + " data sent " + data.Length + " (bytes)").ToConsole(), player.Socket);
                OutgoingMessages.Enqueue(Packet.Create(PacketType.Chunk, data, player));
            }
            else
            {
                //ChunkRequests.AddOrUpdate(vec2, new ConcurrentQueue<Socket>(new Socket[] { socket }), (v, list) => { list.Enqueue(socket); return list; });
                ChunkRequests.AddOrUpdate(vec2, new ConcurrentQueue<PlayerData>(new PlayerData[] { player }), (v, list) => { list.Enqueue(player); return list; });
            }
        }

        private static void SendChunks()
        {
            //while (ChunkRequests.Count > 0)
            if (ChunkRequests.Count > 0)
            //Parallel.ForEach(ChunkRequests,
            //    new ParallelOptions()
            //    {
            //        MaxDegreeOfParallelism = Engine.MaxChunkLoadThreads,
            //        CancellationToken = new CancellationToken(),
            //    }, 
            //    foo =>
                {
                    var entry = ChunkRequests.First(); //foo;// 
                    Vector2 vec2 = entry.Key;
                    ConcurrentQueue<PlayerData> queue;// = entry.Value;
                    if (ChunkRequests.TryRemove(vec2, out queue))
                        ChunkLoader.LoadAsync(Map, vec2, new CancellationToken(), chunk =>
                        {
                            //chunk.GetObjects().ForEach(obj => Instance.NetworkObjects.AddOrUpdate(NetID, id =>
                            //{
                            //    obj["Network"]["ID"] = id;
                            //    return obj;
                            //}, (id, o) =>
                            //{
                            //    throw (new Exception("Tried to add object with duplicate network id"));
                            //}));
                            chunk.GetObjects().ForEach(obj => obj.Instantiate(Instance));
                            byte[] data = Network.Serialize(chunk.Write);
                            byte[] msg = Packet.ToArray(PacketType.Chunk, data);
                            System.Diagnostics.Debug.Assert(data.Length <= 30005, data.Length.ToString());
                               // "something wrong".ToConsole();
                            while (queue.Count > 0)
                            {
                                PlayerData pl;
                                if (queue.TryDequeue(out pl))
                                {
                                    OutgoingMessages.Enqueue(Packet.Create(PacketType.Chunk, data, pl));
                                    //so.BeginSend(msg, 0, msg.Length, SocketFlags.None, a =>
                                    //{
                                    //    int bytesSent = so.EndSend(a);
                                    //    if (msg.Length != bytesSent)
                                    //        throw new Exception("Bytes sent less than packet length");
                                    //    (vec2 + " data sent (" + bytesSent + " bytes)").ToConsole();
                                    //}, so);
                                }
                                //so.BeginSend(msg, 0, msg.Length, SocketFlags.None, a =>  (vec2 + " data sent " + data.Length + " (bytes)").ToConsole(), so);

                            }
                        });
                }
                //}
           // );
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


        public GameObject Instantiate(GameObject obj)
        {
            NetworkObjects.AddOrUpdate(ObjID, id =>
            {
                obj["Network"]["ID"] = id;
                return obj;
            }, (id, o) =>
            {
                throw (new Exception("Tried to add object with duplicate network id"));
            });
            return obj;
        }
        //public GameObject Spawn(GameObject obj, Vector3 global, Vector3 speed)
        //{
        //    obj.Global = global;
        //    obj["Position"]["Speed"] = speed;
        //    Chunk.AddObject(
        //    return obj;
        //}

        static void Save()
        {
            Map.SaveServer();
        }

        /// <summary>
        /// syncs an object over the network
        /// </summary>
        /// <param name="obj"></param>
        public void Sync(GameObject obj)
        {
            Packet.Create(PacketType.ObjectSync, Network.Serialize(w =>
            {
                w.Write(obj.NetworkID);
                obj.Write(w);
            })).Send(from p in Players select p.Socket);
        }

        /// <summary>
        /// When this is called, any object state changes occured, are written 
        /// </summary>
        void BuildSnapshot(TimeSpan gt, BinaryWriter writer)
        {
            writer.Write(gt.TotalMilliseconds);
            writer.Write(this.ObjectsChangedSinceLastSnapshot.Count);
            var objectsChanged = this.ObjectsChangedSinceLastSnapshot;
            this.ObjectsChangedSinceLastSnapshot = new HashSet<int>();
            foreach (var netID in objectsChanged)
            {
                GameObject obj;
                writer.Write(netID);
                if (TryGetNetworkObject(netID, out obj))
                    ObjectSnapshot.Write(obj, writer);
                    // obj["Position"].Write(writer); // TODO: change system so i can update specific components that have changed
            }
            

            //write events
            writer.Write(this.EventsSinceLastSnapshot.Count);
            var oldQueue = this.EventsSinceLastSnapshot;
            Instance.EventsSinceLastSnapshot = new ConcurrentQueue<EventSnapshot>();
            EventSnapshot e;
            while (oldQueue.TryDequeue(out e))
                e.Write(writer);
        }

        private static void SendSnapshots(TimeSpan gt)
        {
            if (Instance.ObjectsChangedSinceLastSnapshot.Count > 0 || Instance.EventsSinceLastSnapshot.Count > 0)
                //Packet.Create(PacketType.Snapshot, Network.Serialize(w => Instance.BuildSnapshot(gt, w))).Send(from p in Players select p.Socket);
                Players.ForEach(player=>
                    OutgoingMessages.Enqueue(Packet.Create(PacketType.Snapshot, Network.Serialize(w => Instance.BuildSnapshot(gt, w)), player)));
        }

        // TODO make the values a list in case i want to sync more components instead of position
        public Dictionary<int, Action<BinaryWriter>> DeltaStates = new Dictionary<int, Action<BinaryWriter>>();
        public bool LogStateChange(int netID)
        {
            //DeltaStates[netID] = stateWriter;
            return ObjectsChangedSinceLastSnapshot.Add(netID);
        }
        public void PopLoot(Components.LootTable table, GameObject parent)
        {
            foreach (var obj in GenerateLoot(table))
                PopLoot(parent, obj);
        }
        public void PopLoot(Components.LootTable table, Vector3 startPosition, Vector3 startVelocity)
        {
            foreach (var obj in GenerateLoot(table))
            {
                double angle = Random.NextDouble() * (Math.PI + Math.PI);
                double w = Random.NextDouble() * Math.PI / 2f;
                // TODO: randomize but normalize all 3 axis
                Vector3 velocity = startVelocity + new Vector3((float)Math.Cos(angle) * 0.05f, (float)Math.Sin(angle) * 0.05f, (float)Math.Sin(w) * 0.05f);
               // obj.Global = startPosition;
                obj.ChangePosition(startPosition);
                obj.Velocity = velocity;
                CreateAndSyncObject(obj);
            }
        }
        public void PopLoot(GameObject obj, Vector3 startPosition, Vector3 startVelocity)
        {
            double angle = Random.NextDouble() * (Math.PI + Math.PI);
            double w = Random.NextDouble() * Math.PI / 2f;
            // TODO: randomize but normalize all 3 axis
            Vector3 velocity = startVelocity + new Vector3((float)Math.Cos(angle) * 0.05f, (float)Math.Sin(angle) * 0.05f, (float)Math.Sin(w) * 0.05f);
            //obj.Global = startPosition;
            obj.ChangePosition(startPosition);
            obj.Velocity = velocity;
            CreateAndSyncObject(obj);
            // return obj;
        }
        public void PopLoot(GameObject parent, GameObject obj)
        {
            this.PopLoot(obj, parent.Global + new Vector3(0, 0, (float)parent["Physics"]["Height"]), parent.Velocity);
        }
        public List<GameObject> GenerateLoot(Components.LootTable lootTable)
        {
            List<GameObject> loot = new List<GameObject>();
            foreach (var l in lootTable)
                for (int i = 0; i < l.Generate(Random); i++)
                    loot.Add(GameObject.Create(l.ObjID));
            return loot;
        }

        public GameObject GenerateItem()
        {
            return new Items.ItemFactory().Generate(Random);
        }

        public RandomThreaded GetRandom()
        {
            return Random;
        }
    }
}
