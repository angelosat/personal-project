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
            public Queue<Vector3> Queue = new();
            public Action Callback = () => { };
            public BatchToken()
            {

            }
            public BatchToken(IEnumerable<Vector3> positions)
            {
                this.Queue = new Queue<Vector3>(positions);
            }

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
            public Queue<WorldPosition> Queue = new();
            public BatchTokenWorldPositions()
            {

            }
            public BatchTokenWorldPositions(IEnumerable<WorldPosition> positions)
            {
                this.Queue = new Queue<WorldPosition>(positions);
            }
        }
        //public Queue<WorldPosition> Queue = new Queue<WorldPosition>();
        public BlockingCollection<BatchTokenWorldPositions> Queue = new();

        HashSet<Vector3> LightChanges = new();
        HashSet<Vector3> BlockChanges = new();

        //IObjectProvider Net { get; set; }
        public IMap Map;// { get; set; }
        public Action<Chunk, Cell> OutdoorBlockHandler = (chunk, cell) => { };
        public Action<Chunk, Cell> IndoorBlockHandler = (chunk, cell) => { };
        public Action<IEnumerable<Vector3>> LightCallback = vectors => { };
        public Action<IEnumerable<Vector3>> BlockCallback = vectors => { };
        public BlockingCollection<BatchToken> SkyLight;// { get; set; }
        public BlockingCollection<BatchToken> BlockLight;// { get; set; }

        public Queue<Vector3> ToDarken;// { get; set; }
        public Thread SkyThread;// { get; set; }
        public Thread BlockThread;// { get; set; }
        public bool Running;// { get; set; }

        public LightingEngine(IMap map)
        {
            this.Map = map;
        }

        public LightingEngine(IMap map, Action<IEnumerable<Vector3>> batchFinishedCallback, Action<IEnumerable<Vector3>> blockCallback)
        {
            //this.Net = net;
            this.Map = map;
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
        static public LightingEngine StartNew(IMap map, Action<IEnumerable<Vector3>> lightCallback, Action<IEnumerable<Vector3>> blockCallback)
        {
            return new LightingEngine(map, lightCallback, blockCallback);//.Start();

        }

        readonly CancellationTokenSource CancelToken = new();
        public void Stop()
        {
            this.CancelToken.Cancel();
        }
        public void EnqueueBlock(params Vector3[] globals)
        {
            this.BlockLight.Add(new BatchToken(globals));
        }

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
            this.SkyLight.Add(new BatchToken(vectorBatch));
        }
        public void Enqueue(IEnumerable<Vector3> vectorBatch, Action callback)
        {
            this.HandleBatchSync(vectorBatch);
            callback();
        }

        public void Enqueue(IEnumerable<WorldPosition> vectorBatch)
        {
            this.HandleBatchSync(vectorBatch.Select(t => t.Global));
        }
        public void HandleBatchSync(IEnumerable<Vector3> vectors)
        {
            var batch = new BatchToken(vectors);
            var queued = new HashSet<Vector3>(vectors);
            var block = new BatchToken(vectors);
            var skydeltas = new Dictionary<Vector3, byte>();
            var blockdeltas = new Dictionary<Vector3, byte>();
            this.ToDarken = new Queue<Vector3>();
            //HandleSkyGlobalNew(vectors, skydeltas);
            while (batch.Queue.Count > 0)
            //HandleSkyGlobal(batch.Queue.Dequeue(), batch.Queue, skydeltas);
            {
                var current = batch.Queue.Dequeue();
                queued.Remove(current);
                HandleSkyGlobalNew(current, batch.Queue, queued, skydeltas);
            }

            while (block.Queue.Count > 0)
                HandleBlockGlobal(block.Queue.Dequeue(), block.Queue, blockdeltas);

            //foreach (var item in skydeltas)
            //    this.Map.SetSkyLight(item.Key, item.Value);
            //foreach (var item in blockdeltas)
            //    this.Map.SetBlockLight(item.Key, item.Value);
            if (skydeltas.Any())
                this.Map.AddSkyLightChanges(skydeltas);
            //if (skydeltas.Count > 0)
            //    (this.Map.ToString() + " skylight changed").ToConsole();
            if (blockdeltas.Any())
                this.Map.AddBlockLightChanges(blockdeltas);


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

        public void HandleSkyLight(Queue<WorldPosition> queue)
        {
            while (queue.Count != 0)
            {
                var globalPos = queue.Dequeue();
                if (!globalPos.Exists)
                    return;
                var global = globalPos.Global;
                var local = globalPos.Local;
                //var thisCell = globalPos.Cell;
                var thisChunk = globalPos.Chunk;
                //byte nextLight;

                int 
                    //gx = (int)global.X, gy = (int)global.Y, 
                    z = (int)global.Z, lx = (int)local.X, ly = (int)local.Y;

                var neighbors = globalPos.GetNeighbors();

                var nextLight = GetNextSkyLight(globalPos, neighbors);

                thisChunk.SetSunlight(lx, ly, z, nextLight, out byte oldLight);

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
            //bool visible = false;
            foreach (var n in neighbors)
            {
                if (!n.Exists)
                    continue;
                if (n.Cell.Opaque)
                    continue;
                maxAdjLight = Math.Max(maxAdjLight, n.Chunk.GetSunlight(n.Cell.LocalCoords));
                //visible = true;
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
            var toDarken = new Queue<WorldPosition>();
            toDarken.Enqueue(pos);
            var handled = new List<WorldPosition>();
            while (toDarken.Count > 0)
            {
                var current = toDarken.Dequeue();
                if (!current.Exists)
                    continue;

                byte nlight;
                int 
                    //gx = (int)current.Global.X, gy = (int)current.Global.Y,
                    z = (int)current.Global.Z, lx = (int)current.Local.X, ly = (int)current.Local.Y;

                current.Chunk.SetSunlight(lx, ly, z, 0, out byte oldLight);

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
                    if (!n.Cell.Opaque)
                        if (nlight < oldLight)
                            toDarken.Enqueue(n);
                }
            }
        }
        void HandleSkyGlobalNew(Vector3 global, Queue<Vector3> queue, HashSet<Vector3> queued, Dictionary<Vector3, byte> deltas)
        {
            int gx = (int)global.X, gy = (int)global.Y, z = (int)global.Z;

            if (!this.Map.TryGetAll(gx, gy, z, out Chunk thisChunk, out Cell thisCell, out int lx, out int ly))
                return;
            var neighbors = global.GetAdjacentLazy();
            var nextLight = GetNextSunLight(thisCell, thisChunk, gx, gy, z, lx, ly, neighbors, deltas);

            if (!deltas.TryGetValue(global, out byte oldLight))
                oldLight = thisChunk.GetSunlight(lx, ly, z);

            int d = nextLight - oldLight;
            if (d != 0)//latest
                deltas[global] = nextLight;

            if (d > 1)
            {
                foreach (Vector3 n in neighbors)
                    if (!queued.Contains(n))
                    {
                        queue.Enqueue(n);// TODO: maybe check if the position is already queued?
                        queued.Add(n);
                    }
            }
            else if (d < -1)// 0)
            {
                foreach (Vector3 n in neighbors)
                    DarkenNew(n, queue, queued, deltas);// TODO: maybe check if the position is already queued?
            }
        }
        void DarkenNew(Vector3 global, Queue<Vector3> queue, HashSet<Vector3> queued, Dictionary<Vector3, byte> deltas)
        {
            this.ToDarken.Enqueue(global);
            var handled = new HashSet<Vector3>();
            var i = 0;
            while (this.ToDarken.Count > 0)
            {
                Vector3 current = this.ToDarken.Dequeue();
                //this.ToDarken.TryDequeue(out current);
                int gx = (int)current.X, gy = (int)current.Y, z = (int)current.Z;
                if (!this.Map.TryGetAll(gx, gy, z, out Chunk chunk, out Cell cell, out int lx, out int ly))
                    continue;
                if (chunk.IsAboveHeightMap(cell.LocalCoords))
                    continue;
                //var nn = new Vector3(gx, gy, z);
                if (!deltas.TryGetValue(current, out byte oldLight))
                    oldLight = chunk.GetSunlight(lx, ly, z);
                if (oldLight > 0)//latest
                    deltas[current] = 0;
                i++;

                var neighbors = current.GetAdjacentLazy();
                foreach (Vector3 n in neighbors)
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);
                    if (!this.Map.TryGetAll(n, out Chunk nchunk, out Cell ncell))
                        continue;

                    if (nchunk.IsAboveHeightMap(ncell.LocalCoords))
                    {
                        if (!queued.Contains(current))
                        {
                            queue.Enqueue(current);
                            queued.Add(current);
                        }
                        continue;
                    }
                    if (!deltas.TryGetValue(n, out byte nlight))
                        nlight = nchunk.GetSunlight(ncell.LocalCoords);
                    if (!ncell.Opaque)
                        if (!this.ToDarken.Contains(n))
                            this.ToDarken.Enqueue(n);
                }
            }
        }


        byte GetNextSunLight(Cell cell, Chunk chunk, int gx, int gy, int z, int lx, int ly, IEnumerable<Vector3> neighbors, Dictionary<Vector3, byte> deltas)
        {
            byte next, maxAdjLight = 0;// = GetMaxAdjLight(neighbors);

            bool visible = false;
            foreach (var n in neighbors)
            {
                if (!this.Map.TryGetAll(n, out var nchunk, out var ncell))
                    continue;
                if (ncell.Opaque)
                    continue;
                if (!deltas.TryGetValue(n, out byte l))
                    l = nchunk.GetSunlight(ncell.LocalCoords);
                maxAdjLight = Math.Max(maxAdjLight, l);
                visible = true;
            }
            if (visible)
                if (!Cell.IsInvisible(cell))
                {
                    this.BlockChanges.Add(new Vector3(gx, gy, z));
                    this.OutdoorBlockHandler(chunk, cell);
                }

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
            //Cell thisCell;
            //Chunk thisChunk;
            //byte thisLight, nextLight;

            if (!this.Map.TryGetAll(global, out Chunk thisChunk, out Cell thisCell))
                return;
            //List<Vector3> neighbors = Position.GetNeighbors(global);
            var neighbors = global.GetAdjacentLazy();
            if (!deltas.TryGetValue(global, out byte thisLight))
                thisLight = thisChunk.GetBlockLight(thisCell.LocalCoords);
            var nextLight = GetNextBlockLight(thisCell, thisChunk, neighbors, deltas);
            //byte oldLight;
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
            var handled = new HashSet<Vector3>();
            while (this.ToDarken.Count > 0)
            {
                Vector3 current = this.ToDarken.Dequeue();
                //this.ToDarken.TryDequeue(out current);

                if (!this.Map.TryGetAll(current, out Chunk chunk, out Cell cell))
                    return;
                if (cell.Opaque)
                    continue;
                if (!deltas.TryGetValue(current, out byte thisLight))
                    thisLight = chunk.GetBlockLight(cell.LocalCoords);
                //byte oldLight;
                if (!deltas.TryGetValue(current, out byte oldLight))
                    oldLight = chunk.GetBlockLight(cell.LocalCoords);
                deltas[current] = 0;
                handled.Add(current);
                i++;
                if (oldLight != 0)
                    this.LightChanges.Add(current);
                List<Vector3> neighbors = current.GetNeighbors().ToList();// Position.GetNeighbors(global);
                foreach (Vector3 n in neighbors)
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);

                    if (!this.Map.TryGetAll(n, out Chunk nchunk, out Cell ncell))
                        continue;

                    if (!deltas.TryGetValue(n, out byte nlight))
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
        }
        private byte GetNextBlockLight(Cell cell, Chunk chunk, IEnumerable<Vector3> neighbors, Dictionary<Vector3, byte> deltas)
        {
            byte next;
            byte maxAdjLight = 0;
            foreach (var n in neighbors)
            {
                //Chunk nchunk;
                //Cell ncell;

                if (!this.Map.TryGetAll(n, out Chunk nchunk, out Cell ncell))
                    continue;
                if (ncell.Opaque)
                    continue;
                if (!deltas.TryGetValue(n, out byte l))
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
            //Cell thisCell;
            //Chunk thisChunk;
            //byte oldLight, nextLight;
            int gx = (int)global.X, gy = (int)global.Y, z = (int)global.Z;

            if (!this.Map.TryGetAll(gx, gy, z, out Chunk thisChunk, out Cell thisCell, out int lx, out int ly))
                return;

            var neighbors = global.GetNeighbors();

            var nextLight = GetNextSunLight(thisCell, thisChunk, gx, gy, z, lx, ly, neighbors);

            thisChunk.SetSunlight(lx, ly, z, nextLight, out byte oldLight);

            // log change to server so it syncs it with clients
            if (oldLight != nextLight)
                this.LightChanges.Add(global);

            int d = nextLight - oldLight;

            if (d > 1)
            {
                foreach (Vector3 n in neighbors)
                    if (!queue.Contains(n))
                        queue.Enqueue(n);// TODO: maybe check if the position is already queued?
            }
            else if (d < -1)// 0)
            {
                foreach (Vector3 n in neighbors)
                    Darken(n, queue);// TODO: maybe check if the position is already queued?
            }
        }

        void Darken(Vector3 global, Queue<Vector3> queue)
        {
            this.ToDarken.Enqueue(global);
            var handled = new HashSet<Vector3>();
            var i = 0;
            while (this.ToDarken.Count > 0)
            {
                Vector3 current = this.ToDarken.Dequeue();

                //this.ToDarken.TryDequeue(out current);
                //Cell cell, ncell;
                //Chunk chunk, nchunk;
                //byte nlight;
                int gx = (int)current.X, gy = (int)current.Y, z = (int)current.Z;
                if (!this.Map.TryGetAll(gx, gy, z, out Chunk chunk, out _, out int lx, out int ly))
                    continue;

                //var nn = new Vector3(gx, gy, z);
                chunk.SetSunlight(lx, ly, z, 0, out byte oldLight);
                i++;
                // log change to server so it syncs it with clients

                if (oldLight != 0)
                    this.LightChanges.Add(global);

                //List<Vector3> neighbors = Position.GetNeighbors(current);
                var neighbors = current.GetAdjacentLazy();
                foreach (var n in neighbors)
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);
                    if (!this.Map.TryGetAll(n, out Chunk nchunk, out Cell ncell))
                        continue;

                    if (nchunk.IsAboveHeightMap(ncell.LocalCoords))
                    {
                        if (!queue.Contains(current))
                            queue.Enqueue(current);
                        continue;
                    }

                    //var nlight = nchunk.GetSunlight(ncell.LocalCoords);
                    if (!ncell.Opaque)
                        if (!this.ToDarken.Contains(n))
                            this.ToDarken.Enqueue(n);
                }
            }
        }

        //private void HandleBlockGlobal(Vector3 global, Queue<Vector3> queue)
        //{
        //    //Cell thisCell;
        //    //Chunk thisChunk;
        //    byte thisLight, nextLight;

        //    if (!this.Map.TryGetAll(global, out Chunk thisChunk, out Cell thisCell))
        //        return;
        //    var neighbors = Position.GetNeighbors(global);
        //    thisLight = thisChunk.GetBlockLight(thisCell.LocalCoords);
        //    nextLight = GetNextBlockLight(thisCell, thisChunk, neighbors);
        //    thisChunk.SetBlockLight(thisCell.LocalCoords, nextLight, out byte oldLight);
        //    // log change to server so it syncs it with clients
        //    if (oldLight != nextLight)
        //        this.LightChanges.Add(global);
        //    //this.Net.LogLightChange(global);
        //    if (nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
        //    {
        //        foreach (Vector3 n in neighbors)
        //            if (!queue.Contains(n))
        //                queue.Enqueue(n);
        //    }
        //    else if (nextLight < thisLight)//if the cell became darker, spread darkness surrounding cells
        //    {
        //        Vector3 source = Vector3.Zero;
        //        foreach (Vector3 n in neighbors)
        //            //DarkenCellLight(n, queue);
        //            DarkenBlock(n, queue);
        //    }
        //}
        void DarkenBlock(Vector3 global, Queue<Vector3> queue)
        {
            this.ToDarken.Enqueue(global);
            var i = 0;
            var handled = new HashSet<Vector3>();
            while (this.ToDarken.Count > 0)
            {
                Vector3 current = this.ToDarken.Dequeue();
                //this.ToDarken.TryDequeue(out current);
                //Cell cell, ncell;
                //Chunk chunk, nchunk;
                //byte nlight, thisLight;

                if (!this.Map.TryGetAll(current, out Chunk chunk, out Cell cell))
                    return;
                if (cell.Opaque)
                    continue;
                var thisLight = chunk.GetBlockLight(cell.LocalCoords);
                //byte oldLight;
                chunk.SetBlockLight(cell.LocalCoords, 0, out byte oldLight);
                handled.Add(current);
                i++;
                if (oldLight != 0)
                    this.LightChanges.Add(current);
                List<Vector3> neighbors = current.GetNeighbors().ToList();// Position.GetNeighbors(global);
                foreach (Vector3 n in neighbors)
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);

                    if (!this.Map.TryGetAll(n, out Chunk nchunk, out Cell ncell))
                        continue;

                    var nlight = nchunk.GetBlockLight(ncell.LocalCoords);
                    //if (nlight == 0)
                    //    i = i;
                    if (nlight < thisLight) // maybe i have to remvoe this line as i did with the darkenskyblocks?
                    {
                        if (nlight > 0)
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
        }
        byte GetNextSunLight(Cell cell, Chunk chunk, int gx, int gy, int z, int lx, int ly, IEnumerable<Vector3> neighbors)
        {
            byte next, maxAdjLight = 0;// = GetMaxAdjLight(neighbors);

            bool visible = false;
            foreach (var n in neighbors)
            {

                if (!this.Map.TryGetAll(n, out Chunk nchunk, out Cell ncell))
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
                    this.OutdoorBlockHandler(chunk, cell);
                }


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
               
                //if (!n.TryGetAll(this.Map, out nchunk, out ncell))

                if (!this.Map.TryGetAll(n, out Chunk nchunk, out Cell ncell))
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

        public void HandleImmediate(IEnumerable<Vector3> vectors)
        {
            var sw = System.Diagnostics.Stopwatch.StartNew();
            var queued = new HashSet<Vector3>(vectors);
            var batch = new BatchToken(vectors);
            var queue = batch.Queue;
            while (queue.Count > 0)
                HandleSkyGlobalImmediate(queue.Dequeue(), queue, queued);

            queued = new HashSet<Vector3>(vectors);
            var block = new BatchToken(vectors);
            queue = block.Queue;
            while (queue.Count > 0)
                HandleBlockGlobalImmediate(queue.Dequeue(), queue, queued);
            sw.Stop();
            //string.Format("batch of {0} handled in {1} ms", vectors.Count(), sw.ElapsedMilliseconds).ToConsole();
        }

        void HandleSkyGlobalImmediate(Vector3 global, Queue<Vector3> queue, HashSet<Vector3> queued)
        {
            byte oldLight, nextLight;
            int gx = (int)global.X, gy = (int)global.Y, z = (int)global.Z;

            if (!this.Map.TryGetAll(gx, gy, z, out var thisChunk, out var thisCell, out int lx, out int ly))
                return;
            var neighbors = global.GetAdjacentLazy();
            //nextLight = GetNextSunLight(thisCell, thisChunk, gx, gy, z, lx, ly, neighbors, deltas);
            nextLight = GetNextSunLightImmediate(thisCell, thisChunk, gx, gy, z, lx, ly, neighbors);

            //if (!deltas.TryGetValue(global, out oldLight))
            //    oldLight = thisChunk.GetSunlight(lx, ly, z);
            oldLight = thisChunk.GetSunlight(lx, ly, z);

            int d = nextLight - oldLight;
            if (d != 0)//latest
                //deltas[global] = nextLight;
                //thisChunk.SetSunlight(global.ToLocal(), nextLight);
                thisChunk.SetSunlight(thisCell.LocalCoords, nextLight);


            if (d > 1)
            {
                foreach (Vector3 n in neighbors)
                    if (!queued.Contains(n))
                    {
                        queue.Enqueue(n);// TODO: maybe check if the position is already queued?
                        queued.Add(n);
                    }
            }
            else if (d < -1)// 0)
            {
                DarkenImmediateWorking(global, queue, queued);
                //foreach (Vector3 n in neighbors)
                //    DarkenImmediate(n, queue, queued);// TODO: maybe check if the position is already queued?
            }
        }
        byte GetNextSunLightImmediate(Cell cell, Chunk chunk, int gx, int gy, int z, int lx, int ly, IEnumerable<Vector3> neighbors)
        {
            byte next, maxAdjLight = 0;// = GetMaxAdjLight(neighbors);

            bool visible = false;
            foreach (var n in neighbors)
            {
                if (!this.Map.TryGetAll(n, out var nchunk, out var ncell))
                    continue;
                if (ncell.Opaque)
                    continue;
                //byte l;
                //if (!deltas.TryGetValue(n, out l))
                //    l = nchunk.GetSunlight(ncell.LocalCoords);
                var l = nchunk.GetSunlight(ncell.LocalCoords);
                maxAdjLight = Math.Max(maxAdjLight, l);
                visible = true;
            }
            if (visible)
                if (!Cell.IsInvisible(cell))
                {
                    this.BlockChanges.Add(new Vector3(gx, gy, z));
                    this.OutdoorBlockHandler(chunk, cell);
                }

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
        void DarkenImmediateWorking(Vector3 global, Queue<Vector3> queue, HashSet<Vector3> queued)
        {
            var queueToDarken = new Queue<Vector3>();
            var queueToDarkenQueued = new HashSet<Vector3>();
            queueToDarken.Enqueue(global);
            queueToDarkenQueued.Add(global);
            while (queueToDarken.Count > 0)
            {
                Vector3 current = queueToDarken.Dequeue();
                byte nlight;
                //int gx = (int)current.X, gy = (int)current.Y, z = (int)current.Z;
                //if (!this.Map.TryGetAll(gx, gy, z, out var chunk, out var cell))
                if (!this.Map.TryGetAll(current, out var chunk, out var cell))
                    continue;

                var local = cell.LocalCoords;
                if (chunk.IsAboveHeightMap(local))
                    continue;
                chunk.SetSunlight(local, 0);

                var neighbors = current.GetAdjacentLazy();
                foreach (Vector3 n in neighbors)
                {
                    //if (handled.Contains(n))
                    //    continue;
                    //handled.Add(n);
                    if (!this.Map.TryGetAll(n, out var nchunk, out var ncell))
                        continue;

                    if (nchunk.IsAboveHeightMap(ncell.LocalCoords))
                    {
                        if (!queued.Contains(current))
                        {
                            queue.Enqueue(current);
                            queued.Add(current);
                        }
                        continue;
                    }
                    nlight = nchunk.GetSunlight(ncell.LocalCoords);

                    if (!ncell.Opaque)
                        if (!queueToDarkenQueued.Contains(n))
                        {
                            queueToDarken.Enqueue(n);
                            queueToDarkenQueued.Add(n);
                        }
                }
            }
        }
        

        private void HandleBlockGlobalImmediate(Vector3 global, Queue<Vector3> queue, HashSet<Vector3> queued)
        {
            byte nextLight;
            queued.Remove(global);
            if (!this.Map.TryGetAll(global, out var thisChunk, out var thisCell))
                return;
            //List<Vector3> neighbors = Position.GetNeighbors(global);
            var neighbors = global.GetAdjacent();
            var local = thisCell.LocalCoords;
            var thisLight = thisChunk.GetBlockLight(local);
           
            nextLight = GetNextBlockLightImmediate(thisCell, neighbors);
            thisChunk.SetBlockLight(local, nextLight);

            if (nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
            {
                //thisChunk.SetBlockLight(thisCell.LocalCoords, nextLight);

                for (int i = 0; i < neighbors.Length; i++)
                {
                    var n = neighbors[i];
                    if (!queued.Contains(n))
                    {
                        queue.Enqueue(n);
                        queued.Add(n);
                    }
                }
            }

            else if (nextLight < thisLight)//if the cell became darker, spread darkness surrounding cells
            {
                //for (int i = 0; i < neighbors.Length; i++)
                //{
                //    var n = neighbors[i];
                //    DarkenBlockImmediate(n, queue, queued);
                //}

                DarkenBlockImmediateWorking(global, queue, queued);

                //foreach (Vector3 n in neighbors)
                //    DarkenBlockImmediate(n, queue);
            }
        }
        private byte GetNextBlockLightImmediate(Cell cell, Vector3[] neighbors)
        {
            byte next;
            byte maxAdjLight = 0;
            //foreach (var n in neighbors)
            for (int i = 0; i < neighbors.Length; i++)
            {
                var n = neighbors[i];
                if(n.Y<0)
                {

                }
                if (!this.Map.TryGetAll(n, out var nchunk, out var ncell))
                //if (!this.Map.TryGetAllNew(n, out var nchunk, out var ncell))
                    continue;
                if (ncell.Opaque)
                    continue;
                //byte l;
                //if (!deltas.TryGetValue(n, out l))
                byte l = nchunk.GetBlockLight(ncell.LocalCoords);
                maxAdjLight = Math.Max(maxAdjLight, l);
            }

            if (cell.Opaque)
            {
                next = 0;
            }
            else
            {
                //return (byte)Math.Max(cell.Luminance, maxAdjLight - 1);
                if (cell.Luminance > 0)
                    next = cell.Luminance;
                else
                    next = (byte)Math.Max(0, maxAdjLight - 1);
            }
            return next;
        }
        void DarkenBlockImmediateWorking(Vector3 global, Queue<Vector3> queue, HashSet<Vector3> queued)
        {
            var queueToDarken = new Queue<Vector3>();
            var queueToDarkenQueued = new HashSet<Vector3>();
            queueToDarken.Enqueue(global);
            queueToDarkenQueued.Add(global);
            while (queueToDarken.Count > 0)
            {
                var current = queueToDarken.Dequeue();
                queueToDarkenQueued.Remove(current);
                if (!this.Map.TryGetAll(current, out var chunk, out var cell))
                    continue;
                if (cell.Opaque)
                    continue;
                var local = cell.LocalCoords;
                

                var prevLight = chunk.GetBlockLight(local);
                chunk.SetBlockLight(local, cell.Luminance);

                var neighbors = global.GetAdjacent();
                for (int i = 0; i < neighbors.Length; i++)
                {

                    //}
                    //foreach (Vector3 n in neighbors)
                    //{
                    var n = neighbors[i];
                    if (!this.Map.TryGetAll(n, out var nchunk, out var ncell))
                        continue;

                    //if (!deltas.TryGetValue(n, out nlight))
                    var nlight = nchunk.GetBlockLight(ncell.LocalCoords);

                    // if neighbor light was less then current previous light, it means that the neighbor was lit from the current cell. so turn the neighbor light off
                    if (nlight < prevLight) // maybe i have to remvoe this line as i did with the darkenskyblocks?
                    {
                        if (nlight > 0)
                            //// if the neighbor cell contains air, enqueue it to darken it
                            //if (ncell.Block.Type == Block.Types.Air
                            //    || ncell.Block.Type == Block.Types.Empty)

                            // if neighbor cell isn't opaque, enqueue it to darken it
                            if (!ncell.Opaque)
                                if (!queueToDarkenQueued.Contains(n))
                                {
                                    queueToDarken.Enqueue(n);
                                    queueToDarkenQueued.Add(n);
                                }
                    }
                    else
                    {
                        //if (!queued.Contains(n))
                        //{
                        //    queue.Enqueue(n);
                        //    queued.Add(n);
                        //}
                        if(!queued.Contains(n))
                        {
                            queue.Enqueue(n);
                            queued.Add(n);
                        }
                    }
                }
            }
        }

        //void DarkenBlockImmediateNew(Vector3 global, Queue<Vector3> queue, HashSet<Vector3> queued)
        //{
        //    this.ToDarken = new Queue<Vector3>();
        //    this.ToDarken.Enqueue(global);
        //    var i = 0;
        //    var handled = new HashSet<Vector3>();
        //    while (this.ToDarken.Count > 0)
        //    {
        //        Vector3 current = this.ToDarken.Dequeue();
        //        //this.ToDarken.TryDequeue(out current);
        //        //byte nlight, thisLight;

        //        if (!this.Map.TryGetAll(current, out Chunk chunk, out Cell cell))
        //            return;
        //        if (cell.Opaque)
        //            continue;
        //        //if (!deltas.TryGetValue(current, out thisLight))
        //        //    thisLight = chunk.GetBlockLight(cell.LocalCoords);
        //        //byte oldLight;
        //        //if (!deltas.TryGetValue(current, out oldLight))
        //        var oldLight = chunk.GetBlockLight(cell.LocalCoords);

        //        chunk.SetBlockLight(cell.LocalCoords, 0);
        //        //deltas[current] = 0;

        //        handled.Add(current);
        //        i++;
        //        //if (oldLight != 0)
        //        //    this.LightChanges.Add(current);
        //        //List<Vector3> neighbors = current.GetNeighbors().ToList();// Position.GetNeighbors(global);
        //        var neighbors = current.GetAdjacent();
        //        foreach (Vector3 n in neighbors)
        //        {
        //            if (handled.Contains(n))
        //                continue;
        //            handled.Add(n);

        //            if (!this.Map.TryGetAll(n, out Chunk nchunk, out Cell ncell))
        //                continue;

        //            //if (!deltas.TryGetValue(n, out nlight))
        //            var nlight = nchunk.GetBlockLight(ncell.LocalCoords);

        //            //if (nlight < thisLight) // maybe i have to remvoe this line as i did with the darkenskyblocks?
        //            if (nlight < oldLight) // maybe i have to remvoe this line as i did with the darkenskyblocks?

        //            {
        //                if (nlight > 0)
        //                    //// if the neighbor cell contains air, enqueue it to darken it
        //                    //if (ncell.Block.Type == Block.Types.Air
        //                    //    || ncell.Block.Type == Block.Types.Empty)

        //                    // if neighbor cell isn't opaque, enqueue it to darken it
        //                    if (!ncell.Opaque)
        //                        if (!this.ToDarken.Contains(n))
        //                            this.ToDarken.Enqueue(n);
        //            }
        //            else
        //            {
        //                if (!queue.Contains(n))
        //                    queue.Enqueue(n);
        //            }
        //        }
        //    }
        //}
        //void DarkenBlockImmediate(Vector3 global, Queue<Vector3> queue, HashSet<Vector3> queued)
        //{
        //    var queueToDarken = new Queue<Vector3>();
        //    var queueToDarkenQueued = new HashSet<Vector3>();
        //    queueToDarken.Enqueue(global);
        //    queueToDarkenQueued.Add(global);
        //    //var i = 0;
        //    //var handled = new HashSet<Vector3>();
        //    while (queueToDarken.Count > 0)
        //    {
        //        Vector3 current = queueToDarken.Dequeue();

        //        if (!this.Map.TryGetAll(current, out var chunk, out var cell))
        //            continue;
        //        if (cell.Opaque)
        //            continue;
        //        var local = cell.LocalCoords;
        //        var prevLight = chunk.GetBlockLight(local);
        //        chunk.SetBlockLight(local, 0);
        //        //handled.Add(current);


        //        //List<Vector3> neighbors = current.GetNeighbors().ToList();// Position.GetNeighbors(global);
        //        var neighbors = global.GetAdjacentLazy();

        //        foreach (Vector3 n in neighbors)
        //        {
        //            //if (handled.Contains(n))
        //            //    continue;
        //            //handled.Add(n);

        //            if (!this.Map.TryGetAll(n, out var nchunk, out var ncell))
        //                continue;

        //            //if (!deltas.TryGetValue(n, out nlight))
        //            var nlight = nchunk.GetBlockLight(ncell.LocalCoords);

        //            // if neighbor light was less then current previous light, it means that the neighbor was lit from the current cell. so turn the neighbor light off
        //            if (nlight < prevLight) // maybe i have to remvoe this line as i did with the darkenskyblocks?
        //            {
        //                if (nlight > 0)
        //                    //// if the neighbor cell contains air, enqueue it to darken it
        //                    //if (ncell.Block.Type == Block.Types.Air
        //                    //    || ncell.Block.Type == Block.Types.Empty)

        //                    // if neighbor cell isn't opaque, enqueue it to darken it
        //                    if (!ncell.Opaque)
        //                        if (!queueToDarkenQueued.Contains(n))
        //                        {
        //                            queueToDarken.Enqueue(n);
        //                            queueToDarkenQueued.Add(n);
        //                        }
        //            }
        //            else
        //            {
        //                if (!queued.Contains(n))
        //                {
        //                    queue.Enqueue(n);
        //                    queued.Add(n);
        //                }
        //            }
        //        }
        //    }
        //}
    }
}
