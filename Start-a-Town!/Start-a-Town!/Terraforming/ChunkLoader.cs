using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.UI;

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
    }
    [Obsolete]
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
        }
        public void Stop()
        {
            this.CancelToken.Cancel();
            this.Cache.Clear();
        }

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
                        ChunksInMemory[Engine.Map].TryRemove(pos, out Chunk ch);
                    }
                    Log.Enqueue(Log.EntryTypes.System, "chunks loaded in " + (DateTime.Now - start).TotalSeconds.ToString("0.00s"));
                    try { Thread.Sleep(Timeout.Infinite); }
                    catch (ThreadInterruptedException) { }
                } while (Running);

            }
        }
        
        private void OnChunkReady(IMap map, Vector2 chunkCoords, Chunk chunk)
        {
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

        public delegate void ChunkHandler(Chunk chunk);
        readonly Dictionary<IMap, Dictionary<Vector2, Queue<ChunkHandler>>> CallBackEvents;
        readonly ConcurrentQueue<QueueEnrty> ChunksToLoad;
        readonly Queue<Vector2> ChunksToUnload;
        public ConcurrentDictionary<IMap, ConcurrentDictionary<Vector2, Chunk>> ChunksInMemory;
        public static event EventHandler LoadingDone;
        public bool Running;

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
            get { return "(" + Instance.ChunksToLoad.Count + ")"; }

        }
       
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

        static public void Reset()
        {
            Instance.ChunksInMemory.Clear();
        }
        static public void Restart()
        {
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
            Instance.ChunksInMemory.AddOrUpdate(map, new ConcurrentDictionary<Vector2, Chunk>(), (m, dic) => dic);
            var list = Vector2.Zero.GetSpiral();
            Action<Chunk> a = onChunkInit ?? ((chunk) => { });
            Parallel.ForEach(list,
                new ParallelOptions
                {
                    MaxDegreeOfParallelism = Engine.MaxChunkLoadThreads,
                    CancellationToken = cancelToken,
                },
                foo =>
                {
                    Chunk chunk = Demand(map, foo);
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

        static public Chunk Retrieve(IMap map, Vector2 chunkCoords)
        {
            Chunk chunk;
            var w = new System.Diagnostics.Stopwatch();
            w.Start();
            chunk = Instance.LoadChunk(map, chunkCoords);
            w.Stop();

            return chunk;
        }

        static public int ChunkMemoryCapacity = (Start_a_Town_.Map.SizeInChunks * Start_a_Town_.Map.SizeInChunks);

        static public Chunk Load(Map map, Vector2 chunkCoords)
        {
            return Instance.LoadChunk(map, chunkCoords);
        }
        public Chunk LoadChunk(IMap map, Vector2 chunkLocalCoords)
        {
            Vector2 pos = chunkLocalCoords;
            _Loading = true;
            Chunk newChunk;
            string filename = Chunk.GetFilename(pos);
            string directory = map.GetFullPath() + "\\chunks\\";
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
                newChunk = Chunk.Create(map, (int)pos.X, (int)pos.Y); 
                newChunk.InitCells(map.World.GetMutators().ToList());
                newChunk.ResetHeightMap();
                map.World.GetMutators().ToList().ForEach(m => m.Finally(newChunk));
            }
            newChunk.Map = map;
            OnChunkReady(map, newChunk.MapCoords, newChunk);

            _Loading = false;


            return newChunk;
        }
        #endregion
    }
}
