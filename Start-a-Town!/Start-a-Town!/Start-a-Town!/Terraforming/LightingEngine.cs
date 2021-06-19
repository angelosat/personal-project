using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.GameModes;

namespace Start_a_Town_
{
    public class LightingEngine
    {
        public class BatchToken
        {
            public Queue<Vector3> Queue = new Queue<Vector3>();
            //public Action<IEnumerable<Vector3>> Callback = (changes) => { };
            public Action Callback = () => { };
            public BatchToken()
            {

            }
            public BatchToken(IEnumerable<Vector3> positions)
            {
                this.Queue = new Queue<Vector3>(positions);
            }
            //public BatchToken(Action<IEnumerable<Vector3>> callback)
            //{
            //    this.Callback = callback;
            //}
            //public BatchToken(IEnumerable<Vector3> positions, Action<IEnumerable<Vector3>> callback)
            //{
            //    this.Callback = callback;
            //    this.Queue = new Queue<Vector3>(positions);
            //}

            public BatchToken(Action callback)
            {
                this.Callback = callback;
            }
            public BatchToken(IEnumerable<Vector3> positions, Action callback)
            {
                this.Callback = callback;
                this.Queue = new Queue<Vector3>(positions);
            }
        }
        public class BatchTokenWorldPositions
        {
            public Queue<WorldPosition> Queue = new Queue<WorldPosition>();
            public BatchTokenWorldPositions()
            {

            }
            public BatchTokenWorldPositions(IEnumerable<WorldPosition> positions)
            {
                this.Queue = new Queue<WorldPosition>(positions);
            }
        }
        //public Queue<WorldPosition> Queue = new Queue<WorldPosition>();
        public BlockingCollection<BatchTokenWorldPositions> Queue = new BlockingCollection<BatchTokenWorldPositions>();
        /// <summary>
        /// Updates to block lighting (both sun and block sources)
        /// </summary>
        HashSet<Vector3> LightChanges = new HashSet<Vector3>();

        /// <summary>
        /// Updates to the visibility state of blocks
        /// </summary>
        HashSet<Vector3> BlockChanges = new HashSet<Vector3>();

        IObjectProvider Net { get; set; }
        public IMap Map { get; set; }
        public Action<Chunk, Cell> OutdoorBlockHandler = (chunk, cell) => { };
        public Action<Chunk, Cell> IndoorBlockHandler = (chunk, cell) => { };
        public Action<IEnumerable<Vector3>> LightCallback = vectors => { };
        public Action<IEnumerable<Vector3>> BlockCallback = vectors => { };
        //public ConcurrentQueue<Vector3> SkyLight { get; set; }
        public BlockingCollection<BatchToken> SkyLight { get; set; }
        public BlockingCollection<BatchToken> BlockLight { get; set; }
       // public BlockingCollection<Vector3> SkyLight { get; set; }
        public ConcurrentQueue<Vector3> CellLight { get; set; }
        public ConcurrentQueue<Vector3> ToDarken { get; set; }
        public Thread SkyThread { get; set; }
        public Thread BlockThread { get; set; }
        public bool Running { get; set; }
        LightingEngine()
        {
            //this.SkyLight = new BlockingCollection<Vector3>(new ConcurrentQueue<Vector3>());
            this.SkyLight = new BlockingCollection<BatchToken>(new ConcurrentQueue<BatchToken>());
            this.BlockLight = new BlockingCollection<BatchToken>(new ConcurrentQueue<BatchToken>());
            //Task.Run(() =>
            //{
            //    try { HandleSkyAsync(net); }
            //    catch (OperationCanceledException) { }
            //}, this.CancelToken.Token);
            //this.SkyLight = new ConcurrentQueue<Vector3>();
            this.CellLight = new ConcurrentQueue<Vector3>();
            this.ToDarken = new ConcurrentQueue<Vector3>();
            this.Running = true;
            //this.SkyThread = new Thread(SkyWorker);
            //this.BlockThread = new Thread(BlockWorker);
        }
        public LightingEngine(IMap map)
            : this()
        {
            this.Map = map;
        }
        /// <summary>
        /// // TODO: make lightingengine's blockcallback return both chunk and cell so i don't have to re fetch them
        /// </summary>
        /// <param name="net"></param>
        /// <param name="batchFinishedCallback"></param>
        /// <param name="blockCallback"></param>
        public LightingEngine(IObjectProvider net, Action<IEnumerable<Vector3>> batchFinishedCallback, Action<IEnumerable<Vector3>> blockCallback)
            : this()
        {
            this.Net = net;
            this.Map = net.Map;
            this.LightCallback = batchFinishedCallback;
            this.BlockCallback = blockCallback;
            //this.SkyLight = new BlockingCollection<Vector3>(new ConcurrentQueue<Vector3>());
            ////Task.Run(() =>
            ////{
            ////    try { HandleSkyAsync(net); }
            ////    catch (OperationCanceledException) { }
            ////}, this.CancelToken.Token);
            ////this.SkyLight = new ConcurrentQueue<Vector3>();
            //this.BlockLight = new ConcurrentQueue<Vector3>();
            //this.ToDarken = new ConcurrentQueue<Vector3>();
            //this.Running = true;
            //this.SkyThread = new Thread(SkyWorker);
            //this.BlockThread = new Thread(BlockWorker);

            //Begin();
        }
        static public LightingEngine StartNew(IObjectProvider net, Action<IEnumerable<Vector3>> lightCallback, Action<IEnumerable<Vector3>> blockCallback)
        {
            return new LightingEngine(net, lightCallback, blockCallback).Start();
        }
        public LightingEngine Start()
        {
            Task.Run(() =>
            {
                //try { HandleSkyAsync(); }
                Thread.CurrentThread.Name = "SkyLighter";
                try
                {
                    //HandleBatchAsync();
                    foreach (var batch in this.Queue.GetConsumingEnumerable(this.CancelToken.Token))
                        this.HandleSkyLight(batch.Queue);
                }
                catch (OperationCanceledException) { }
            }, this.CancelToken.Token);
            Task.Run(() =>
            {
                //try { HandleSkyAsync(); }
                Thread.CurrentThread.Name = "BlockLighter";
                try { HandleBlockBatchAsync(); }
                catch (OperationCanceledException) { }
            }, this.CancelToken.Token);
            return this;
        }
        CancellationTokenSource CancelToken = new CancellationTokenSource();
        public void Stop()
        {
            this.CancelToken.Cancel();
        }
        void Begin()
        {
            Running = true;
            if (SkyThread.ThreadState == ThreadState.Unstarted)
                SkyThread.Start();
            else if (SkyThread.ThreadState == ThreadState.WaitSleepJoin)
                SkyThread.Interrupt();
            if (BlockThread.ThreadState == ThreadState.Unstarted)
                BlockThread.Start();
            else if (BlockThread.ThreadState == ThreadState.WaitSleepJoin)
                BlockThread.Interrupt();
        }
        public void EnqueueBlock(params Vector3[] globals)
        {
            this.BlockLight.Add(new BatchToken(globals));
        }
        //public void EnqueueBlock(IEnumerable<Vector3> globals, Action<IEnumerable<Vector3>> callback)
        //{
        //    this.BlockLight.Add(new BatchToken(globals, callback));
        //}
        public void EnqueueBlock(IEnumerable<Vector3> globals, Action callback)
        {
            this.BlockLight.Add(new BatchToken(globals, callback));
        }
        public void Enqueue(params Vector3[] vectors)
        {
            this.SkyLight.Add(new BatchToken(vectors));
        }
        public void Enqueue(IEnumerable<Vector3> vectorBatch)
        {
            //BlockingCollection<Vector3> batch = new BlockingCollection<Vector3>();
            //foreach (var v in vectorBatch)
            //    batch.Add(v);
            //this.SkyLight.Add(batch);

            //this.SkyLight.Add(new Queue<Vector3>(vectorBatch));
            this.SkyLight.Add(new BatchToken(vectorBatch));

            //foreach (var v in vectorBatch)
            //    this.SkyLight.Add(v);
        }
        public void Enqueue(IEnumerable<Vector3> vectorBatch, Action callback)
        {
            this.SkyLight.Add(new BatchToken(vectorBatch, callback));
        }

        public void Enqueue(IEnumerable<WorldPosition> vectorBatch)
        {
            this.Queue.Add(new BatchTokenWorldPositions(vectorBatch));
        }
        public void HandleBatchSync(IEnumerable<Vector3> vectors)
        {
            BatchToken batch = new BatchToken(vectors);
            BatchToken block = new BatchToken(vectors);
            var skydeltas = new Dictionary<Vector3, byte>();
            var blockdeltas = new Dictionary<Vector3, byte>();

            while (batch.Queue.Count > 0)
                HandleSkyGlobal(batch.Queue.Dequeue(), batch.Queue, skydeltas);
            while (block.Queue.Count > 0)
                HandleBlockGlobal(block.Queue.Dequeue(), block.Queue, blockdeltas);

            //foreach (var item in skydeltas)
            //    this.Map.SetSkyLight(item.Key, item.Value);
            //foreach (var item in blockdeltas)
            //    this.Map.SetBlockLight(item.Key, item.Value);

            this.Map.AddSkyLightChanges(skydeltas);
            if (skydeltas.Count > 0)
                 "skylight changed".ToConsole();
            this.Map.AddBlockLightChanges(blockdeltas);


            batch.Callback();
            this.LightCallback(this.LightChanges);
            this.BlockCallback(this.BlockChanges);
            this.BlockChanges = new HashSet<Vector3>();
            this.LightChanges = new HashSet<Vector3>();
        }
        public void HandleBatchSyncOld(IEnumerable<Vector3> vectors)
        {
            BatchToken batch = new BatchToken(vectors);
            BatchToken block = new BatchToken(vectors);
            while (batch.Queue.Count > 0)
                HandleSkyGlobal(batch.Queue.Dequeue(), batch.Queue);
            while (block.Queue.Count > 0)
                HandleBlockGlobal(block.Queue.Dequeue(), block.Queue);
            batch.Callback();
            this.LightCallback(this.LightChanges);
            this.BlockCallback(this.BlockChanges);
            this.BlockChanges = new HashSet<Vector3>();
            this.LightChanges = new HashSet<Vector3>();
        }
        public void HandleBatchSync()
        {
            foreach (var batch in this.SkyLight)
            {
                //foreach (var global in batch)
                while (batch.Queue.Count > 0)
                    HandleSkyGlobal(batch.Queue.Dequeue(), batch.Queue);
                batch.Callback();
                this.LightCallback(this.LightChanges);
                this.BlockCallback(this.BlockChanges);
                this.BlockChanges = new HashSet<Vector3>();
                this.LightChanges = new HashSet<Vector3>();
            }
        }
        void HandleBatchAsync()
        {
            foreach (var batch in this.SkyLight.GetConsumingEnumerable(this.CancelToken.Token))
            {
                //foreach (var global in batch.GetConsumingEnumerable(this.CancelToken.Token))
                //HandleGlobal(global, batch);
                while (batch.Queue.Count > 0)
                    HandleSkyGlobal(batch.Queue.Dequeue(), batch.Queue);
                batch.Callback();
                this.LightCallback(this.LightChanges.ToList());
                this.LightChanges = new HashSet<Vector3>();
                this.BlockCallback(this.BlockChanges.ToList());
                this.BlockChanges = new HashSet<Vector3>();
            }
        }
        void HandleBlockBatchAsync()
        {
            foreach (var batch in this.BlockLight.GetConsumingEnumerable(this.CancelToken.Token))
            {
                while (batch.Queue.Count > 0)
                    HandleBlockGlobal(batch.Queue.Dequeue(), batch.Queue);
                batch.Callback();
                this.LightCallback(this.LightChanges.ToList());
                this.LightChanges = new HashSet<Vector3>();
                this.BlockCallback(this.BlockChanges.ToList());
                this.BlockChanges = new HashSet<Vector3>();
            }
        }

        public void HandleSkyLight(Queue<WorldPosition> queue)
        {
            while(queue.Count != 0)
            {
                var globalPos = queue.Dequeue();
                if (!globalPos.Exists)
                    return;
                var global = globalPos.Global;
                var local = globalPos.Local;
                var thisCell = globalPos.Cell;
                var thisChunk = globalPos.Chunk;
                byte oldLight, nextLight;

                int gx = (int)global.X, gy = (int)global.Y, z = (int)global.Z, lx = (int)local.X, ly = (int)local.Y;

                var neighbors = globalPos.GetNeighbors();

                nextLight = GetNextSkyLight(globalPos, neighbors);

                thisChunk.SetSunlight(lx, ly, z, nextLight, out oldLight);

                // log change
                if (oldLight != nextLight)
                    this.LightChanges.Add(global);

                int d = nextLight - oldLight;

                if (d > 1) //if the cell became brighter, queue surrounding cells to spread light to them
                {
                    foreach (var n in neighbors)
                        queue.Enqueue(n);// TODO: maybe check if the position is already queued?
                }
                else if (d < -1)//if the cell became darker, spread darkness surrounding cells
                {
                    foreach (var n in neighbors)
                        Darken(n, queue);// TODO: maybe check if the position is already queued?
                }
            }
        }
        byte GetNextSkyLight(WorldPosition position, IEnumerable<WorldPosition> neighbors)
        {
            byte next, maxAdjLight = 0;
            bool visible = false;
            foreach (var n in neighbors)
            {
                if (!n.Exists)
                    continue;
                if (n.Cell.Opaque)
                    continue;
                maxAdjLight = Math.Max(maxAdjLight, n.Chunk.GetSunlight(n.Cell.LocalCoords));
                visible = true;
            }
            //if (visible)
            //    if (!Cell.IsInvisible(position.Cell))
            //    {
            //        position.Chunk.VisibleOutdoorCells[Chunk.FindIndex(position.Local)] = position.Cell;
            //        this.BlockChanges.Add(position.Global);
            //        this.OutdoorBlockHandler(position.Chunk, position.Cell);
            //    }

            if (position.Cell.Opaque)
            {
                next = 0;
            }
            else
            {
                if (position.Chunk.IsAboveHeightMap(position.Local))
                    next = 15;
                else
                    next = (byte)Math.Max(0, maxAdjLight - 1);
            }
            return next;
        }
        void Darken(WorldPosition pos, Queue<WorldPosition> queue)
        {
            Queue<WorldPosition> toDarken = new Queue<WorldPosition>();
            toDarken.Enqueue(pos);
            List<WorldPosition> handled = new List<WorldPosition>();
            while (toDarken.Count > 0)
            {
                var current = toDarken.Dequeue();
                if (!current.Exists)
                    continue;

                byte nlight;
                int gx = (int)current.Global.X, gy = (int)current.Global.Y, z = (int)current.Global.Z, lx = (int)current.Local.X, ly = (int)current.Local.Y;

                byte oldLight;
                current.Chunk.SetSunlight(lx, ly, z, 0, out oldLight);

                // log change to server so it syncs it with clients

                if (oldLight != 0)
                    this.LightChanges.Add(current.Global);

                var neighbors = current.GetNeighbors();
                foreach (var n in neighbors)
                {
                    if (!n.Exists)
                        continue;
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);

                    if (n.Chunk.IsAboveHeightMap(n.Cell.LocalCoords))
                    {
                        queue.Enqueue(current);
                        continue;
                    }

                    nlight = n.Chunk.GetSunlight(n.Cell.LocalCoords);
                    //if (n.Cell.Block.Type == Block.Types.Air//)
                    //    || current.Cell.Block.Type == Block.Types.Empty)
                    if(!n.Cell.Opaque)
                        if (nlight < oldLight)
                            toDarken.Enqueue(n);
                }
            }
        }

        void HandleSkyGlobal(Vector3 global, Queue<Vector3> queue, Dictionary<Vector3, byte> deltas)
        {
            if (global == new Vector3(28, 21, 67))
                "wraia".ToConsole();
            Cell thisCell;
            Chunk thisChunk;
            byte oldLight, nextLight;
            int gx = (int)global.X, gy = (int)global.Y, z = (int)global.Z, lx, ly;

            if (!this.Map.TryGetAll(gx, gy, z, out thisChunk, out thisCell, out lx, out ly))
                return;
            var neighbors = global.GetNeighbors();

            nextLight = GetNextSunLight(thisCell, thisChunk, gx, gy, z, lx, ly, neighbors, deltas);

            //thisChunk.SetSunlight(lx, ly, z, nextLight, out oldLight);
            if (!deltas.TryGetValue(global, out oldLight))
                oldLight = thisChunk.GetSunlight(lx, ly, z);

            //deltas[global] = nextLight;
            int d = nextLight - oldLight;
            if(d!=0)//latest
                deltas[global] = nextLight;

            if (d > 1)// || nextLight == 15)//0)
            //if(nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
            {
                foreach (Vector3 n in neighbors)
                    if (!queue.Contains(n))
                        queue.Enqueue(n);// TODO: maybe check if the position is already queued?
            }
            else if (d < -1)// 0)
            //else if(nextLight < thisLight)//if the cell became darker, spread darkness surrounding cells
            {
                foreach (Vector3 n in neighbors)
                    Darken(n, queue, deltas);// TODO: maybe check if the position is already queued?
            }
        }
        void Darken(Vector3 global, Queue<Vector3> queue, Dictionary<Vector3, byte> deltas)
        {
            if (global == new Vector3(28, 21, 67))
                "wraia".ToConsole();
            //  Queue<Vector3> toDarken = new Queue<Vector3>();
            this.ToDarken.Enqueue(global);
            List<Vector3> handled = new List<Vector3>();
            var i = 0;
            while (this.ToDarken.Count > 0)
            {
                Vector3 current;
                this.ToDarken.TryDequeue(out current);
                Cell cell, ncell;
                Chunk chunk, nchunk;
                byte nlight;
                int gx = (int)current.X, gy = (int)current.Y, z = (int)current.Z, lx, ly;
                //if (!Position.TryGet(this.Map, gx, gy, z, out cell, out chunk, out lx, out ly))
                //var g = new Vector3(gx, gy, z);
                if (!this.Map.TryGetAll(gx, gy, z, out chunk, out cell, out lx, out ly))
                    continue;
                if (chunk.IsAboveHeightMap(cell.LocalCoords))
                    continue;
                byte oldLight;
                var nn = new Vector3(gx, gy, z);
                //chunk.SetSunlight(lx, ly, z, 0, out oldLight);
                if(!deltas.TryGetValue(current, out oldLight))
                    oldLight = chunk.GetSunlight(lx, ly, z);
                if (oldLight > 0)//latest
                    deltas[current] = 0;
                i++;

                List<Vector3> neighbors = Position.GetNeighbors(current);
                foreach (Vector3 n in neighbors)
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);
                    //if (!Position.TryGet(this.Map, n, out ncell, out nchunk))
                    if (!this.Map.TryGetAll(n, out nchunk, out ncell))
                        continue;

                    if (nchunk.IsAboveHeightMap(ncell.LocalCoords))
                    {
                        if (!queue.Contains(current))
                            queue.Enqueue(current);
                        continue;
                    }
                    if (!deltas.TryGetValue(n, out nlight))
                        nlight = nchunk.GetSunlight(ncell.LocalCoords);
                    //if (ncell.Block.Type == Block.Types.Air//) // if the neighbor cell contains air, enqueue it to darken it
                    //    || ncell.Block.Type == Block.Types.Empty)
                    if (!ncell.Opaque)
                        //if (nlight < oldLight) // THIS LINE was responsible for darkness not spreading to below heightmap cells
                        if (!this.ToDarken.Contains(n))
                            this.ToDarken.Enqueue(n);
                }
            }
            //if (this.Map.GetNetwork() is Server)
            //    (i.ToString() + " blocks had sun value darkened").ToConsole();
        }
        byte GetNextSunLight(Cell cell, Chunk chunk, int gx, int gy, int z, int lx, int ly, IEnumerable<Vector3> neighbors, Dictionary<Vector3, byte> deltas)
        {
            byte next, maxAdjLight = 0;// = GetMaxAdjLight(neighbors);

            //if (!Cell.IsInvisible(cell))
            //    if (maxAdjLight > 0) // if I initialize sunlight to 15, i have to check this in order to prevent showing every opaque block
            //    {
            //        // SYNC WITH CLIENTS
            //        //Chunk.Show(chunk, cell);
            //        if (this.Net != null)
            //            this.Net.ShowBlock(cell.GetGlobalCoords(chunk));
            //        this.OutdoorBlockHandler(chunk, cell);
            //    }
            bool visible = false;
            foreach (var n in neighbors)
            {
                Chunk nchunk;
                Cell ncell;
                //if (!n.TryGetAll(this.Map, out nchunk, out ncell))

                if (!this.Map.TryGetAll(n, out nchunk, out ncell))
                    continue;
                if (ncell.Opaque)
                    continue;
                byte l;
                if (!deltas.TryGetValue(n, out l))
                    l = nchunk.GetSunlight(ncell.LocalCoords);
                maxAdjLight = Math.Max(maxAdjLight, l);
                visible = true;
            }
            if (visible)
                if (!Cell.IsInvisible(cell))
                {
                    this.BlockChanges.Add(new Vector3(gx, gy, z));
                    //if (this.Net != null)
                    //    this.Net.ShowBlock(cell.GetGlobalCoords(chunk));
                    this.OutdoorBlockHandler(chunk, cell);
                }

            //if (!Cell.IsInvisible(cell))
            //foreach (var item in neighbors)// from vec in neighbors select vec.GetCell(chunk.Map))
            //{
            //    Cell c;
            //    if (!item.TryGetCell(chunk.Map, out c))
            //        continue;
            //    if (c.Type == Block.Types.Air)
            //    {
            //        if (this.Net != null)
            //            this.Net.ShowBlock(cell.GetGlobalCoords(chunk));
            //        this.ShowBlockHandler(chunk, cell);
            //        break;
            //    }
            //}

            if (cell.Opaque)
            {
                next = 0;
            }
            else
            {
                if (chunk.IsAboveHeightMap(lx, ly, z))
                    next = 15;
                else
                    next = (byte)Math.Max(0, maxAdjLight - 1);// global) - 1);
            }
            return next;
        }
        
        private void HandleBlockGlobal(Vector3 global, Queue<Vector3> queue, Dictionary<Vector3, byte> deltas)
        {
            Cell thisCell;
            Chunk thisChunk;
            byte thisLight, nextLight;
            //if (!Position.TryGet(this.Map, global, out thisCell, out thisChunk))

            if (!this.Map.TryGetAll(global, out thisChunk, out thisCell))
                return;
            List<Vector3> neighbors = Position.GetNeighbors(global);

            if (!deltas.TryGetValue(global, out thisLight))
                thisLight = thisChunk.GetBlockLight(thisCell.LocalCoords);
            nextLight = GetNextBlockLight(thisCell, thisChunk, neighbors, deltas);
            byte oldLight;
            //thisChunk.SetBlockLight(thisCell.LocalCoords, nextLight, out oldLight);
            deltas[global] = nextLight;

            if (nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
            {
                foreach (Vector3 n in neighbors)
                    if (!queue.Contains(n))
                        queue.Enqueue(n);
            }
            else if (nextLight < thisLight)//if the cell became darker, spread darkness surrounding cells
            {
                Vector3 source = Vector3.Zero;
                foreach (Vector3 n in neighbors)
                    //DarkenCellLight(n, queue);
                    DarkenBlock(n, queue, deltas);
            }
        }
        void DarkenBlock(Vector3 global, Queue<Vector3> queue, Dictionary<Vector3, byte> deltas)
        {
            this.ToDarken.Enqueue(global);
            var i = 0;
            var handled = new List<Vector3>();
            while (this.ToDarken.Count > 0)
            {
                Vector3 current;
                this.ToDarken.TryDequeue(out current);
                Cell cell, ncell;
                Chunk chunk, nchunk;
                byte nlight, thisLight;

                if (!this.Map.TryGetAll(current, out chunk, out cell))
                    return;
                //if (cell.Block.Type != Block.Types.Air)
                //    continue;
                if (cell.Opaque)
                    continue;
                if(!deltas.TryGetValue(current, out thisLight))
                thisLight = chunk.GetBlockLight(cell.LocalCoords);
                byte oldLight;
                //chunk.SetBlockLight(cell.LocalCoords, 0, out oldLight);
                if (!deltas.TryGetValue(current, out oldLight))
                    oldLight = chunk.GetBlockLight(cell.LocalCoords);
                deltas[current] = 0;
                handled.Add(current);
                i++;
                if (oldLight != 0)
                    this.LightChanges.Add(current);
                List<Vector3> neighbors = current.GetNeighbors().ToList();// Position.GetNeighbors(global);
                foreach (Vector3 n in neighbors)
                {
                    //if (!Position.TryGet(this.Map, n, out ncell, out nchunk))
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);

                    if (!this.Map.TryGetAll(n, out nchunk, out ncell))
                        continue;

                    if(!deltas.TryGetValue(n, out nlight))
                        nlight = nchunk.GetBlockLight(ncell.LocalCoords);
      
                    if (nlight < thisLight) // maybe i have to remvoe this line as i did with the darkenskyblocks?
                    {
                        if (nlight > 0)
                            //// if the neighbor cell contains air, enqueue it to darken it
                            //if (ncell.Block.Type == Block.Types.Air
                            //    || ncell.Block.Type == Block.Types.Empty)

                            // if neighbor cell isn't opaque, enqueue it to darken it
                            if (!ncell.Opaque)
                                if (!this.ToDarken.Contains(n))
                                    this.ToDarken.Enqueue(n);
                    }
                    else
                    {
                        if (!queue.Contains(n))
                            queue.Enqueue(n);
                    }
                }
            }
            //if (this.Map.GetNetwork() is Server)
            //    (i.ToString() + " blocks had block value darkened").ToConsole();
        }
        private byte GetNextBlockLight(Cell cell, Chunk chunk, List<Vector3> neighbors, Dictionary<Vector3, byte> deltas)
        {
            byte next;
            byte maxAdjLight = 0;
            foreach (var n in neighbors)
            {
                Chunk nchunk;
                Cell ncell;

                if (!this.Map.TryGetAll(n, out nchunk, out ncell))
                    continue;
                if (ncell.Opaque)
                    continue;
                byte l;
                if(!deltas.TryGetValue(n, out l))
                    l = nchunk.GetBlockLight(ncell.LocalCoords);
                maxAdjLight = Math.Max(maxAdjLight, l);
            }
      
            if (cell.Opaque)
            {
                next = 0;
            }
            else
            {
                if (cell.Luminance > 0)
                    next = cell.Luminance;
                else
                    next = (byte)Math.Max(0, maxAdjLight - 1);
            }
            return next;
        }

        #region old
        void HandleSkyGlobal(Vector3 global, Queue<Vector3> queue)
        {
            Cell thisCell;
            Chunk thisChunk;
            byte oldLight, nextLight;
            int gx = (int)global.X, gy = (int)global.Y, z = (int)global.Z, lx, ly;

            //if (!Position.TryGet(this.Map, gx, gy, z, out thisCell, out thisChunk, out lx, out ly))
            if (!this.Map.TryGetAll(gx, gy, z, out thisChunk, out thisCell, out lx, out ly))
                return;

            var neighbors = global.GetNeighbors();

            nextLight = GetNextSunLight(thisCell, thisChunk, gx, gy, z, lx, ly, neighbors);

            thisChunk.SetSunlight(lx, ly, z, nextLight, out oldLight);

            // log change to server so it syncs it with clients
            if (oldLight != nextLight)
                this.LightChanges.Add(global);
            //    if (this.Net != null)
            //        this.Net.LogLightChange(global);

            int d = nextLight - oldLight;

            if (d > 1)// || nextLight == 15)//0)
            //if(nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
            {
                foreach (Vector3 n in neighbors)
                    if (!queue.Contains(n))
                        queue.Enqueue(n);// TODO: maybe check if the position is already queued?
            }
            else if (d < -1)// 0)
            //else if(nextLight < thisLight)//if the cell became darker, spread darkness surrounding cells
            {
                foreach (Vector3 n in neighbors)
                    Darken(n, queue);// TODO: maybe check if the position is already queued?
            }
        }
  
        void Darken(Vector3 global, Queue<Vector3> queue)
        {
            //  Queue<Vector3> toDarken = new Queue<Vector3>();
            this.ToDarken.Enqueue(global);
            List<Vector3> handled = new List<Vector3>();
            var i = 0;
            while (this.ToDarken.Count > 0)
            {
                Vector3 current;
                this.ToDarken.TryDequeue(out current);
                Cell cell, ncell;
                Chunk chunk, nchunk;
                byte nlight;
                int gx = (int)current.X, gy = (int)current.Y, z = (int)current.Z, lx, ly;
                //if (!Position.TryGet(this.Map, gx, gy, z, out cell, out chunk, out lx, out ly))
                //var g = new Vector3(gx, gy, z);
                if (!this.Map.TryGetAll(gx, gy, z, out chunk, out cell, out lx, out ly))
                    continue;

                byte oldLight;
                var nn = new Vector3(gx, gy, z);
                chunk.SetSunlight(lx, ly, z, 0, out oldLight);
                i++;
                // log change to server so it syncs it with clients

                if (oldLight != 0)
                    this.LightChanges.Add(global);
                //if (this.Net != null)
                //    this.Net.LogLightChange(current);

                List<Vector3> neighbors = Position.GetNeighbors(current);
                foreach (Vector3 n in neighbors)
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);
                    //if (!Position.TryGet(this.Map, n, out ncell, out nchunk))
                    if (!this.Map.TryGetAll(n, out nchunk, out ncell))
                        continue;

                    if (nchunk.IsAboveHeightMap(ncell.LocalCoords))
                    {
                        if (!queue.Contains(current))
                            queue.Enqueue(current);
                        continue;
                    }

                    nlight = nchunk.GetSunlight(ncell.LocalCoords);
                    //if (ncell.Block.Type == Block.Types.Air//) // if the neighbor cell contains air, enqueue it to darken it
                    //    || ncell.Block.Type == Block.Types.Empty)
                    if (!ncell.Opaque)
                        //if (nlight < oldLight) // THIS LINE was responsible for darkness not spreading to below heightmap cells
                        if (!this.ToDarken.Contains(n))
                            this.ToDarken.Enqueue(n);
                }
            }
            //if (this.Map.GetNetwork() is Server)
            //    (i.ToString() + " blocks had sun value darkened").ToConsole();
        }

        private void HandleBlockGlobal(Vector3 global, Queue<Vector3> queue)
        {
            Cell thisCell;
            Chunk thisChunk;
            byte thisLight, nextLight;
            //if (!Position.TryGet(this.Map, global, out thisCell, out thisChunk))

            if (!this.Map.TryGetAll(global, out thisChunk, out thisCell))
                return;
            List<Vector3> neighbors = Position.GetNeighbors(global);
            thisLight = thisChunk.GetBlockLight(thisCell.LocalCoords);
            nextLight = GetNextBlockLight(thisCell, thisChunk, neighbors);
            byte oldLight;
            thisChunk.SetBlockLight(thisCell.LocalCoords, nextLight, out oldLight);
            // log change to server so it syncs it with clients
            if (oldLight != nextLight)
                this.LightChanges.Add(global);
            //this.Net.LogLightChange(global);
            if (nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
            {
                foreach (Vector3 n in neighbors)
                    if (!queue.Contains(n))
                        queue.Enqueue(n);
            }
            else if (nextLight < thisLight)//if the cell became darker, spread darkness surrounding cells
            {
                Vector3 source = Vector3.Zero;
                foreach (Vector3 n in neighbors)
                    //DarkenCellLight(n, queue);
                    DarkenBlock(n, queue);
            }
        }
        void DarkenBlock(Vector3 global, Queue<Vector3> queue)
        {
            this.ToDarken.Enqueue(global);
            var i = 0;
            var handled = new List<Vector3>();
            while (this.ToDarken.Count > 0)
            {
                Vector3 current;
                this.ToDarken.TryDequeue(out current);
                Cell cell, ncell;
                Chunk chunk, nchunk;
                byte nlight, thisLight;

                if (!this.Map.TryGetAll(current, out chunk, out cell))
                    return;
                //if (cell.Block.Type != Block.Types.Air)
                //    continue;
                if (cell.Opaque)
                    continue;
                thisLight = chunk.GetBlockLight(cell.LocalCoords);
                byte oldLight;
                chunk.SetBlockLight(cell.LocalCoords, 0, out oldLight);
                //if (current == new Vector3(12, 19, 69))
                //    i = i;
                //if (oldLight == 0)
                //    i = i;
                //if (cell.Block.Type != Block.Types.Air)
                //    i = i;
                handled.Add(current);
                i++;
                if (oldLight != 0)
                    this.LightChanges.Add(current);
                List<Vector3> neighbors = current.GetNeighbors().ToList();// Position.GetNeighbors(global);
                foreach (Vector3 n in neighbors)
                {
                    //if (!Position.TryGet(this.Map, n, out ncell, out nchunk))
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);

                    if (!this.Map.TryGetAll(n, out nchunk, out ncell))
                        continue;

                    nlight = nchunk.GetBlockLight(ncell.LocalCoords);
                    //if (nlight == 0)
                    //    i = i;
                    if (nlight < thisLight) // maybe i have to remvoe this line as i did with the darkenskyblocks?
                    {
                        if (nlight > 0)
                            //// if the neighbor cell contains air, enqueue it to darken it
                            //if (ncell.Block.Type == Block.Types.Air
                            //    || ncell.Block.Type == Block.Types.Empty)

                            // if neighbor cell isn't opaque, enqueue it to darken it
                            if (!ncell.Opaque)
                                if (!this.ToDarken.Contains(n))
                                    this.ToDarken.Enqueue(n);
                    }
                    else
                    {
                        if (!queue.Contains(n))
                            queue.Enqueue(n);
                    }
                }
            }
            //if (this.Map.GetNetwork() is Server)
            //    (i.ToString() + " blocks had block value darkened").ToConsole();
        }
        byte GetNextSunLight(Cell cell, Chunk chunk, int gx, int gy, int z, int lx, int ly, IEnumerable<Vector3> neighbors)
        {
            byte next, maxAdjLight = 0;// = GetMaxAdjLight(neighbors);

            //if (!Cell.IsInvisible(cell))
            //    if (maxAdjLight > 0) // if I initialize sunlight to 15, i have to check this in order to prevent showing every opaque block
            //    {
            //        // SYNC WITH CLIENTS
            //        //Chunk.Show(chunk, cell);
            //        if (this.Net != null)
            //            this.Net.ShowBlock(cell.GetGlobalCoords(chunk));
            //        this.OutdoorBlockHandler(chunk, cell);
            //    }
            bool visible = false;
            foreach (var n in neighbors)
            {
                Chunk nchunk;
                Cell ncell;
                //if (!n.TryGetAll(this.Map, out nchunk, out ncell))

                if (!this.Map.TryGetAll(n, out nchunk, out ncell))
                    continue;
                if (ncell.Opaque)
                    continue;
                var l = nchunk.GetSunlight(ncell.LocalCoords);
                maxAdjLight = Math.Max(maxAdjLight, l);
                visible = true;
            }
            if (visible)
                if (!Cell.IsInvisible(cell))
                {
                    this.BlockChanges.Add(new Vector3(gx, gy, z));
                    //if (this.Net != null)
                    //    this.Net.ShowBlock(cell.GetGlobalCoords(chunk));
                    this.OutdoorBlockHandler(chunk, cell);
                }

            //if (!Cell.IsInvisible(cell))
            //foreach (var item in neighbors)// from vec in neighbors select vec.GetCell(chunk.Map))
            //{
            //    Cell c;
            //    if (!item.TryGetCell(chunk.Map, out c))
            //        continue;
            //    if (c.Type == Block.Types.Air)
            //    {
            //        if (this.Net != null)
            //            this.Net.ShowBlock(cell.GetGlobalCoords(chunk));
            //        this.ShowBlockHandler(chunk, cell);
            //        break;
            //    }
            //}

            if (cell.Opaque)
            {
                next = 0;
            }
            else
            {
                if (chunk.IsAboveHeightMap(lx, ly, z))
                    next = 15;
                else
                    next = (byte)Math.Max(0, maxAdjLight - 1);// global) - 1);
            }
            return next;
        }

        private byte GetNextBlockLight(Cell cell, Chunk chunk, List<Vector3> neighbors)
        {
            byte next;
            byte maxAdjLight = 0;// GetMaxAdjBlockLight(neighbors);
            //if (!Cell.IsInvisible(cell))
            //    if (maxAdjLight > 0)
            //    {
            //        // SYNC WITH CLIENTS
            //        //Chunk.Show(chunk, cell);
            //        //this.Net.ShowBlock(cell.GetGlobalCoords(chunk));
            //        this.BlockChanges.Add(cell.GetGlobalCoords(chunk));
            //    }
            bool visible = false;
            foreach (var n in neighbors)
            {
                Chunk nchunk;
                Cell ncell;
                //if (!n.TryGetAll(this.Map, out nchunk, out ncell))

                if (!this.Map.TryGetAll(n, out nchunk, out ncell))
                    continue;
                if (ncell.Opaque)
                    continue;
                maxAdjLight = Math.Max(maxAdjLight, nchunk.GetBlockLight(ncell.LocalCoords));
                visible = true;
            }
            if (visible)
                if (!Cell.IsInvisible(cell))
                    this.BlockChanges.Add(cell.GetGlobalCoords(chunk));

            if (cell.Opaque)
            {
                next = 0;
            }
            else
            {
                if (cell.Luminance > 0)
                    next = cell.Luminance;
                else
                    next = (byte)Math.Max(0, maxAdjLight - 1);
            }
            return next;
        }
        #endregion

       

        //void DarkenBlockOld(Vector3 global, Queue<Vector3> queue)
        //{
        //    Cell cell, ncell;
        //    Chunk chunk, nchunk;
        //    byte nlight, thisLight;
        //    //if (!Position.TryGet(this.Map, global, out cell, out chunk))

        //    if (!this.Map.TryGetAll(global, out chunk, out cell))
        //        return;
        //    thisLight = chunk.GetBlockLight(cell.LocalCoords);
        //    byte oldLight;
        //    chunk.SetBlockLight(cell.LocalCoords, 0, out oldLight);
        //    if (oldLight != 0)
        //        this.LightChanges.Add(global);
        //        //this.Net.LogLightChange(global);
        //    List<Vector3> neighbors = Position.GetNeighbors(global);
        //    foreach (Vector3 n in neighbors)
        //    {
        //        //if (!Position.TryGet(this.Map, n, out ncell, out nchunk))

        //        if (!this.Map.TryGetAll(n, out nchunk, out ncell))
        //            continue;

        //        nlight = nchunk.GetBlockLight(ncell.LocalCoords);

        //        if (nlight < thisLight)
        //            DarkenBlock(n, queue);
        //        else
        //            queue.Enqueue(global);
        //    }
        //}
    }
}
