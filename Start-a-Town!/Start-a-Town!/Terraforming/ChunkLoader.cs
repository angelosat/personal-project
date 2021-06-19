using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;
using Start_a_Town_.GameModes;

namespace Start_a_Town_
{
    struct QueueEnrty
    {
        public Map Map;
        public Vector2 ChunkCoords;
        public override string ToString()
        {
            return this.Map.Name + " " + ChunkCoords.ToString();
        }
        //    public ChunkLoader.ChunkHandler CallBackEvent;
    }
    public class ChunkLoader
    {
        class ChunkRequest
        {
            public Vector2 Vector;
            public Action<Chunk> CachedCallback;
            public ChunkRequest(Vector2 vector, Action<Chunk> callback)
            {
                this.Vector = vector;
                this.CachedCallback = callback;
            }
            //public Action<Chunk> SavedCallback, NewCallback;
            //public ChunkRequest(Vector2 vector, Action<Chunk> cachedcallback, Action<Chunk> savedcallback, Action<Chunk> newcallback)
            //{
            //    this.Vector = vector;
            //    this.CachedCallback = cachedcallback;
            //    this.SavedCallback = savedcallback;
            //    this.NewCallback = newcallback;
            //}
        }

        public void ScheduleForSaving(Chunk chunk)
        {
            if (this.ToSave.Contains(chunk))
                return;
            this.ToSave.TryAdd(chunk);
        }
        public void Enqueue(Vector2 request, Action<Chunk> callback)
        {
            this.Requests.TryAdd(new ChunkRequest(request, callback));
            //this.Start();
        }
        public bool TryEnqueue(Vector2 request, Action<Chunk> callback)
        {
            return this.TryEnqueue(request, callback, out _);
        }
        public bool TryEnqueue(Vector2 request, Action<Chunk> callback, out Chunk cached)
        {
            if (this.Cache.TryGetValue(request, out cached))
            {
                callback(cached);
                return true;
            }
            this.Requests.TryAdd(new ChunkRequest(request, callback));
            return false;
        }
        //public void Enqueue(Vector2 request, Action<Chunk> cachedcallback, Action<Chunk> savedcallback, Action<Chunk> newcallback)
        //{
        //    this.Requests.TryAdd(new ChunkRequest(request, cachedcallback, savedcallback, newcallback));
        //    //this.Start();
        //}
        //Task Loader, Saver;
        CancellationTokenSource CancelToken = new();
        readonly BlockingCollection<ChunkRequest> Requests = new(new ConcurrentQueue<ChunkRequest>());
        readonly BlockingCollection<Chunk> ToSave = new(new ConcurrentQueue<Chunk>());
        readonly Dictionary<Vector2, Chunk> Cache = new();
        public ConcurrentQueue<Chunk> Output = new();
        readonly IMap Map;
        static public ChunkLoader StartNew(IMap map)
        {
            var loader = new ChunkLoader(map);
            loader.Start();
            return loader;
        }
        void Start()
        {
            this.CancelToken.Cancel();
            this.Cache.Clear();
            this.CancelToken = new CancellationTokenSource();
            //this.Loader = Task.Factory.StartNew(() => { System.Threading.Thread.CurrentThread.Name = "Loader"; this.Load(); }, this.CancelToken.Token);
            //this.Saver = Task.Factory.StartNew(() => { System.Threading.Thread.CurrentThread.Name = "Saver"; this.Save(); }, this.CancelToken.Token);
        }
        public void Stop()
        {
            this.CancelToken.Cancel();
            this.Cache.Clear();
        }

        //void Save()
        //{
        //    try
        //    {
        //        foreach (var chunk in this.ToSave.GetConsumingEnumerable(this.CancelToken.Token))
        //        {
        //            // if for some reason there was a duplicate request
        //            if (chunk.Saved)
        //                continue;
        //            //return;
        //            chunk.SaveToFile();
        //            (chunk.MapCoords.ToString() + " saved").ToConsole();
        //        }
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        // if canceled clear queue
        //        while (this.ToSave.Count > 0)
        //        {
        //            this.ToSave.TryTake(out _);
        //        }
        //    }
        //}

        //void Load()
        //{
        //    try
        //    {
        //        foreach (var request in this.Requests.GetConsumingEnumerable(this.CancelToken.Token))
        //        {
        //            //if (this.Cache.TryGetValue(request.Vector, out chunk))
        //            //    request.CachedCallback(chunk);
        //            //else if (this.FromFile(request.Vector, out chunk))
        //            //{
        //            //    this.Cache[request.Vector] = chunk;
        //            //    request.SavedCallback(chunk);
        //            //}
        //            //else
        //            //{
        //            //    chunk = this.Generate(request.Vector);
        //            //    this.Cache[request.Vector] = chunk;
        //            //    request.NewCallback(chunk);
        //            //}



        //            if (!this.Cache.TryGetValue(request.Vector, out Chunk chunk))
        //            {
        //                if (!this.FromFile(request.Vector, out chunk)) //comment this if i want to force fresh generated chunks
        //                    chunk = this.Generate(request.Vector);
        //                this.Cache[request.Vector] = chunk;
        //                request.CachedCallback(chunk);
        //            }
        //            else
        //            {
        //                request.CachedCallback(chunk);
        //                //(chunk.ToString() + " from cache").ToConsole();
        //            }
        //            //this.Output.Enqueue(chunk);
        //        }
        //    }
        //    catch (OperationCanceledException)
        //    {
        //        // if canceled clear queue
        //        while (this.Requests.Count > 0)
        //        {
        //            this.Requests.TryTake(out _);
        //        }
        //    }
        //}

        public ChunkLoader(IMap map)
        {
            this.Map = map;
        }
        public bool FromFile(Vector2 chunkvector, out Chunk chunk)
        {
            string filename = Chunk.GetFilename(chunkvector);
            string directory = this.Map.GetFullPath() + "\\chunks\\" + Chunk.GetDirName(chunkvector) + "\\";
            if (!File.Exists(directory + filename))
            {
                chunk = null;
                return false;
            }
            chunk = Chunk.Load(this.Map, directory + filename);
            return chunk != null;
        }
        public Chunk Generate(Vector2 chunkvector)
        {
            var watch = new System.Diagnostics.Stopwatch();
            Chunk newChunk = Chunk.Create(this.Map, (int)chunkvector.X, (int)chunkvector.Y); //(Map.Instance.MapArgs, (int)pos.X, (int)pos.Y, Map.Instance.GetSeedArray());

            newChunk.InitCells(this.Map.World.GetMutators().ToList());//.FinalizeCells(Server.Random); // WARNING!
            //newChunk.ResetVisibleCells(); //i've moved this one below because some blocks didn't have their exposed faces detected correctly

            newChunk.UpdateHeightMap();


            //newChunk.SkylightUpdated = true;

            //var toLight = newChunk.ResetHeightMap();
            ////newChunk.Map.ActiveChunks.Add(newChunk.MapCoords, newChunk);
            //Map tempMap = new Start_a_Town_.Map(this.Map.World, this.Map.Coordinates);
            //tempMap.ActiveChunks = this.Map.ActiveChunks.ToDictionary(f => f.Key, f => f.Value);
            //tempMap.ActiveChunks.Add(newChunk.MapCoords, newChunk);
            ////new LightingEngine(this.Map).HandleSkyLight(toLight);
            //Queue<WorldPosition> positions = new Queue<WorldPosition>(from global in toLight select new WorldPosition(tempMap, global));
            //new LightingEngine(this.Map).HandleSkyLight(positions);

            this.Map.World.GetMutators().ToList().ForEach(m => m.Finally(newChunk));
            newChunk.ResetVisibleCells(); //i've moved this one here because some blocks didn't have their exposed faces detected correctly
            //newChunk.ResetVisibleCells();

            // update neighbor chunks here? the faces of outer blocks
            // i want to update existing chunks only ONCE per newely generated neighboring chunk, so this seems the right place to do it
            //foreach (var vector in chunkvector.GetNeighbors())
            //{
            //    Chunk neighbor;
            //    if (Instance.Map.ActiveChunks.TryGetValue(vector, out neighbor))
            //        neighbor.UpdateOuterBlocks();
            //}
            
            return newChunk;
        }

        //public Chunk LoadChunk(Map map, Vector2 chunkLocalCoords, LightingEngine engine)
        //{
        //    //Map map = Engine.Map;
        //    Vector2 pos = chunkLocalCoords;//map.Coordinates * Map.SizeInChunks + chunkLocalCoords;
        //    //Vector2 pos = map.Coordinates * (Engine.ChunkRadius * 2) + chunkLocalCoords;
        //    _Loading = true;
        //    Chunk newChunk;
        //    string filename = Chunk.GetFilename(pos);
        //    string directory = map.GetFullPath() + "\\chunks\\";// GlobalVars.SaveDir;
        //    if (File.Exists(directory + filename))
        //    {
        //        newChunk = Chunk.Load(map, directory + filename);
        //        if (!this.ChunksInMemory.ContainsKey(map))
        //            this.ChunksInMemory[map] = new ConcurrentDictionary<Vector2, Chunk>();
        //        ChunksInMemory[map][pos] = newChunk;
        //    }
        //    else
        //    {
        //        var watch = new System.Diagnostics.Stopwatch();
        //        newChunk = Chunk.Create(map, (int)pos.X, (int)pos.Y); //(Map.Instance.MapArgs, (int)pos.X, (int)pos.Y, Map.Instance.GetSeedArray());
           
        //        newChunk.InitCells(map.World.Mutators.ToList());//.FinalizeCells(Server.Random); // WARNING!
        //        newChunk.ResetHeightMap();
        //        map.World.Mutators.ToList().ForEach(m => m.Finally(newChunk));

        //    }
        //    newChunk.Map = map;
        //    OnChunkReady(map, newChunk.MapCoords, newChunk);

        //    _Loading = false;

        //    return newChunk;
        //}

        #region old
        static public ThreadState State
        { get { return Instance.Thread.ThreadState; } }

        static public bool Paused = false;
        void AsyncLoadChunks(object vector)
        {
            var thisLock = new object();
            lock (thisLock)
            {
                do
                {
                    DateTime start = DateTime.Now;
                    if (ChunksToLoad.Count > 0)
                    {
                        //Workload = ChunksToLoad.Count;
                        while (ChunksToLoad.Count > 0)
                        {
                            if (Paused)
                                continue;

                            if (!ChunksToLoad.TryDequeue(out QueueEnrty entry))
                                continue;

                            Chunk chunk = Retrieve(entry.Map, entry.ChunkCoords);



                        }
                        OnLoadingDone();
                    }
                    if (ChunksToUnload.Count > 0)
                    {
                        Vector2 pos = ChunksToUnload.Dequeue();
                        // ChunksInMemory[Engine.Map].Remove(pos);
                        ChunksInMemory[Engine.Map].TryRemove(pos, out Chunk ch);
                    }
                    Log.Enqueue(Log.EntryTypes.System, "chunks loaded in " + (DateTime.Now - start).TotalSeconds.ToString("0.00s"));
                    try { Thread.Sleep(Timeout.Infinite); }
                    //catch (Exception e) { Log.Enqueue(Log.EntryTypes.Default, e.Message); }
                    catch (ThreadInterruptedException) { }
                } while (Running);

            }
        }
        //private void OnChunkReady(Chunk chunk)
        //{
        //    this.OnChunkReady(chunk.Map, chunk.MapCoords, chunk);
        //}
        private void OnChunkReady(IMap map, Vector2 chunkCoords, Chunk chunk)
        {

            //if (!CallBackEvents.ContainsKey(map))
            //    return;
            if (!CallBackEvents.TryGetValue(map, out Dictionary<Vector2, Queue<ChunkHandler>> callbackQueues))
                return;
            if (callbackQueues.ContainsKey(chunkCoords))
            {
                Queue<ChunkHandler> queue = callbackQueues[chunkCoords];
                callbackQueues.Remove(chunkCoords);
                foreach (ChunkHandler handler in queue)
                    handler(chunk);
            }
            if (callbackQueues.Count == 0)
                CallBackEvents.Remove(map);
        }
        void OnLoadingDone()
        {
            LoadingDone?.Invoke(this, EventArgs.Empty);
        }

        //Dictionary<Vector2, List<EventHandler<MapEventArgs>>> CallBackEvents;
        public delegate void ChunkHandler(Chunk chunk);
        readonly Dictionary<IMap, Dictionary<Vector2, Queue<ChunkHandler>>> CallBackEvents;
        readonly ConcurrentQueue<QueueEnrty> ChunksToLoad;
        //readonly ConcurrentStack<ConcurrentQueue<QueueEnrty>> MapsToLoad;
        readonly Queue<Vector2> ChunksToUnload;
        //Dictionary<Vector2, Chunk> ChunksInMemory;
        public ConcurrentDictionary<IMap, ConcurrentDictionary<Vector2, Chunk>> ChunksInMemory;
        public static event EventHandler LoadingDone;
        public bool Running;
        //  static public Map Map;
        //int Workload;

        readonly Thread Thread;

        static ChunkLoader _Instance;
        public static ChunkLoader Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ChunkLoader();
                return _Instance;
            }
            set { _Instance = value; }
        }

        static public void UnloadChunk(Vector2 chunkCoords)
        {
            Instance.ChunksToUnload.Enqueue(chunkCoords);

        }


        static public string Status
        { get { return Instance.Thread.ThreadState.ToString() + (Instance.Thread.ThreadState == ThreadState.Running ? " (" + Instance.ChunksToLoad.Count + " Tasks)" : ""); } }
        static public string Counter
        {
            get { return "(" + Instance.ChunksToLoad.Count + ")"; }// return Instance.ChunksInMemory.Count.ToString() + "/" + Instance.Workload.ToString(); }// Instance.ChunksToLoad.Count.ToString(); }

        }
        //static public string ToString()
        //{
        //    return Status +
        //        "\nChunksInMemory: " + Instance.ChunksInMemory.Count +
        //        "\nChunksToLoad: " + Instance.ChunksToLoad.Count +
        //        "\nChunksToUnload: " + Instance.ChunksToUnload.Count;

        //}


        static public bool TryGetChunk(Map map, Vector2 chunkCoords, out Chunk chunk)
        {


            if (!Instance.ChunksInMemory.TryGetValue(map, out ConcurrentDictionary<Vector2, Chunk> chunks))
            {
                chunk = null;
                return false;
            }

            bool found = chunks.TryGetValue(chunkCoords, out chunk);

            return found;
        }

        //static public void Stop()
        //{
        //    Paused = true;
        //}
        static public void Reset()
        {
            Instance.ChunksInMemory.Clear();
        }
        static public void Restart()
        {
            //Instance.ChunksInMemory = new ConcurrentDictionary<Map, ConcurrentDictionary<Vector2, Chunk>>();
            //Instance.CallBackEvents = new Dictionary<Map, Dictionary<Vector2, Queue<ChunkHandler>>>();
            //Instance.MapsToLoad = new ConcurrentStack<ConcurrentQueue<QueueEnrty>>();
            //Instance.ChunksToLoad = new ConcurrentQueue<QueueEnrty>();
            //Instance.ChunksToUnload = new Queue<Vector2>();
            //Instance.Thread.Abort();
            //Instance.Thread = new Thread(new ParameterizedThreadStart(Instance.AsyncLoadChunks));
            //Instance.Thread.Name = "ChunkLoader";
            //Paused = false;
            //Engine.InitializeComponents();
            GC.Collect();
        }
        /// <summary>
        /// Stops the ChunkLoader's thread.
        /// </summary>
        static public void End()
        {
            Instance.Running = false;
            Instance.Thread.Interrupt();
        }
        static public bool TryRequest(Map map, Vector2 chunkCoords, out Chunk chunk)
        {
            return Instance.ChunksInMemory[map].TryGetValue(chunkCoords, out chunk);
        }

        static public Chunk Demand(IMap map, Vector2 chunkCoords)
        {
            return Retrieve(map, chunkCoords);
        }





        bool _Loading;
        static public int Count
        { get { return Instance.ChunksInMemory.Count; } }
        static public bool Loading
        {
            get { return Instance._Loading; }
        }

        ChunkLoader()
        {
            ChunksToLoad = new ConcurrentQueue<QueueEnrty>();
            ChunksToUnload = new Queue<Vector2>();
            CallBackEvents = new Dictionary<IMap, Dictionary<Vector2, Queue<ChunkHandler>>>();
            //MapsToLoad = new ConcurrentStack<ConcurrentQueue<QueueEnrty>>();
            ChunksInMemory = new ConcurrentDictionary<IMap, ConcurrentDictionary<Vector2, Chunk>>();
            Thread = new(new ParameterizedThreadStart(AsyncLoadChunks));
            Thread.Name = "ChunkLoader";
            Game1.Instance.Exiting += new EventHandler<EventArgs>(Instance_Exiting);
        }

        void Instance_Exiting(object sender, EventArgs e)
        {
            Thread.Abort();

        }

        static public void ForceLoad(Rooms.GameScreen screen, IMap map, CancellationToken cancelToken, Action<Chunk> onChunkInit = null)
        {
            //screen.Camera.CenterOn(Vector3.Zero);
            //  ChunkLoader.Restart();
            var box = new LoadingBox()
            {
                Location = UIManager.Size * new Vector2(1, 1.5f) * 0.5f,
                ProgressFunc = () => map.LoadProgress.ToString("##0%"),
                TextFunc = () => "Loading map...",
                TintFunc = () => Color.Lerp(Color.Red, Color.Lime, map.LoadProgress),
            };
            Task.Factory.StartNew(() =>
            {
                box.Show(screen.WindowManager);
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    ChunkLoader.ForceLoad(map, cancelToken, onChunkInit);
                }
                catch (OperationCanceledException) { box.Hide(); }
                finally
                {
                    if (cancelToken.IsCancellationRequested)
                        Instance.ChunksInMemory.TryRemove(map, out ConcurrentDictionary<Vector2, Chunk> bar);
                    else
                        Log.Enqueue(Log.EntryTypes.System, "Map loaded in " + watch.Elapsed.TotalSeconds.ToString("0.00s"));
                    box.Hide();
                    watch.Stop();
                }

            }, cancelToken);
        }
        static public void ForceLoad(IMap map, CancellationToken cancelToken, Action<Chunk> onChunkInit = null)
        {
            //map.ActiveChunks = new ConcurrentDictionary<Vector2, Chunk>();
            //map.ActiveChunks = new Dictionary<Vector2, Chunk>();
            Instance.ChunksInMemory.AddOrUpdate(map, new ConcurrentDictionary<Vector2, Chunk>(), (m, dic) => dic);
            //var list = map.GetSpiral(Vector2.Zero);
            var list = Vector2.Zero.GetSpiral();
            Action<Chunk> a = onChunkInit ?? ((chunk) => { });
            //try
            //{
            Parallel.ForEach(list,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Engine.MaxChunkLoadThreads,
                    CancellationToken = cancelToken,
                },
                foo =>
                {
                    Chunk chunk = Demand(map, foo);
                    //   map.ActiveChunks.TryAdd(foo, chunk);
                    map.GetActiveChunks().AddOrUpdate(foo, chunk, (pos, existing) => existing);
                    a(chunk);
                });

        }
        static public void LoadAsync(Map map, CancellationToken cancelToken, Action<Chunk> onChunkInit = null)
        {
            LoadingBox box = new()
            {
                Location = UIManager.Size * new Vector2(1, 1.5f) * 0.5f,
                ProgressFunc = () => map.LoadProgress.ToString("##0%"),
                TextFunc = () => "Loading map...",
                TintFunc = () => Color.Lerp(Color.Red, Color.Lime, map.LoadProgress),
                WindowManager = ScreenManager.Instance.WindowManager
            };
            Task.Factory.StartNew(() =>
            {
                box.Show(ScreenManager.Instance.WindowManager);
                System.Diagnostics.Stopwatch watch = System.Diagnostics.Stopwatch.StartNew();
                try
                {
                    ChunkLoader.ForceLoad(map, cancelToken, onChunkInit);
                }
                catch (OperationCanceledException) { box.Hide(); }
                finally
                {
                    if (cancelToken.IsCancellationRequested)
                        Instance.ChunksInMemory.TryRemove(map, out ConcurrentDictionary<Vector2, Chunk> bar);
                    else
                        Log.Enqueue(Log.EntryTypes.System, "Map loaded in " + watch.Elapsed.TotalSeconds.ToString("0.00s"));
                    box.Hide();
                    watch.Stop();
                }

            }, cancelToken);
        }
        static public void LoadAsync(Map map, Vector2 chunkCoords, CancellationToken cancelToken, Action<Chunk> onChunkInit = null)
        {
            Instance.ChunksInMemory.AddOrUpdate(map, new ConcurrentDictionary<Vector2, Chunk>(), (m, dic) => dic);
            Task.Factory.StartNew(() =>
            {
                try
                {
                    Chunk chunk = Demand(map, chunkCoords);
                    map.ActiveChunks.AddOrUpdate(chunkCoords, chunk, (pos, existing) => existing);
                    onChunkInit(chunk);
                }
                catch (OperationCanceledException) { }
                finally
                {
                    if (cancelToken.IsCancellationRequested)
                        Instance.ChunksInMemory.TryRemove(map, out ConcurrentDictionary<Vector2, Chunk> bar);
                }
            }, cancelToken);
        }
        //static public void LoadAsync(Map map, Vector2 chunkCoords, CancellationToken cancelToken, IObjectProvider net, Action<Chunk> onChunkInit = null)
        //{
        //    Instance.ChunksInMemory.AddOrUpdate(map, new ConcurrentDictionary<Vector2, Chunk>(), (m, dic) => dic);
        //    Task.Factory.StartNew(() =>
        //    {
        //        try
        //        {
        //            Chunk chunk = Demand(net, map, chunkCoords);
        //            map.ActiveChunks.AddOrUpdate(chunkCoords, chunk, (pos, existing) => existing);
        //            onChunkInit(chunk);
        //        }
        //        catch (OperationCanceledException) { }
        //        finally
        //        {
        //            ConcurrentDictionary<Vector2, Chunk> bar;
        //            if (cancelToken.IsCancellationRequested)
        //                Instance.ChunksInMemory.TryRemove(map, out bar);
        //        }
        //    }, cancelToken);
        //}

        static public Chunk Retrieve(IMap map, Vector2 chunkCoords)
        {
            Chunk chunk;

            // i'm already checking this on the server
            //if(map.ActiveChunks.TryGetValue(chunkCoords, out chunk))
            //{
            //    Instance.OnChunkReady(map, chunkCoords, chunk);
            //    return chunk;
            //}

            var w = new System.Diagnostics.Stopwatch();
            w.Start();
            chunk = Instance.LoadChunk(map, chunkCoords);
            w.Stop();

            return chunk;
        }

        //static public int ChunkMemoryCapacity = (Map.SizeInChunks * Map.SizeInChunks); //5*5+1;
        //static public int ChunkMemoryCapacity = (Instance.Map.GetSizeInChunks() * Instance.Map.GetSizeInChunks()); //5*5+1;
        static public int ChunkMemoryCapacity = (Start_a_Town_.Map.SizeInChunks * Start_a_Town_.Map.SizeInChunks); //5*5+1;

        static public Chunk Load(Map map, Vector2 chunkCoords)
        {
            return Instance.LoadChunk(map, chunkCoords);
        }
        public Chunk LoadChunk(IMap map, Vector2 chunkLocalCoords)
        {
            //Map map = Engine.Map;
            Vector2 pos = chunkLocalCoords;//map.Coordinates * Map.SizeInChunks + chunkLocalCoords;
            //Vector2 pos = map.Coordinates * (Engine.ChunkRadius * 2) + chunkLocalCoords;
            _Loading = true;
            Chunk newChunk;
            string filename = Chunk.GetFilename(pos);
            string directory = map.GetFullPath() + "\\chunks\\";// GlobalVars.SaveDir;
            if (File.Exists(directory + filename))
            {
                newChunk = Chunk.Load(map, directory + filename);
                if (!this.ChunksInMemory.ContainsKey(map))
                    this.ChunksInMemory[map] = new ConcurrentDictionary<Vector2, Chunk>();
                ChunksInMemory[map][pos] = newChunk;
            }
            else
            {
                var watch = new System.Diagnostics.Stopwatch();
                newChunk = Chunk.Create(map, (int)pos.X, (int)pos.Y); //(Map.Instance.MapArgs, (int)pos.X, (int)pos.Y, Map.Instance.GetSeedArray());

                newChunk.InitCells(map.World.GetMutators().ToList());//.FinalizeCells(Server.Random); // WARNING!
                newChunk.ResetHeightMap();
                map.World.GetMutators().ToList().ForEach(m => m.Finally(newChunk));
                //newChunk.ResetEdges();

            }
            newChunk.Map = map;
            OnChunkReady(map, newChunk.MapCoords, newChunk);

            _Loading = false;


            return newChunk;
        }
        #endregion
    }
}
