using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public partial class LightingEngine
    {
        HashSet<Vector3> LightChanges = new();
        HashSet<Vector3> BlockChanges = new();

        public IMap Map;
        public Action<Chunk, Cell> OutdoorBlockHandler = (chunk, cell) => { };
        public Action<IEnumerable<Vector3>> LightCallback = vectors => { };
        public Action<IEnumerable<Vector3>> BlockCallback = vectors => { };
        public BlockingCollection<BatchToken> SkyLight;
        public BlockingCollection<BatchToken> BlockLight;
        readonly CancellationTokenSource CancelToken = new();

        public Queue<Vector3> ToDarken;

        public LightingEngine(IMap map)
        {
            this.Map = map;
        }
        public LightingEngine(IMap map, Action<IEnumerable<Vector3>> batchFinishedCallback, Action<IEnumerable<Vector3>> blockCallback)
        {
            this.Map = map;
            this.LightCallback = batchFinishedCallback;
            this.BlockCallback = blockCallback;
        }

        static public LightingEngine StartNew(IMap map, Action<IEnumerable<Vector3>> lightCallback, Action<IEnumerable<Vector3>> blockCallback)
        {
            return new LightingEngine(map, lightCallback, blockCallback);
        }
        public void Stop()
        {
            this.CancelToken.Cancel();
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
            while (batch.Queue.Count > 0)
            {
                var current = batch.Queue.Dequeue();
                queued.Remove(current);
                HandleSkyGlobalNew(current, batch.Queue, queued, skydeltas);
            }

            while (block.Queue.Count > 0)
                HandleBlockGlobal(block.Queue.Dequeue(), block.Queue, blockdeltas);

            if (skydeltas.Any())
                this.Map.AddSkyLightChanges(skydeltas);
            if (blockdeltas.Any())
                this.Map.AddBlockLightChanges(blockdeltas);

            batch.Callback();
            this.LightCallback(this.LightChanges);
            this.BlockCallback(this.BlockChanges);
            this.BlockChanges = new HashSet<Vector3>();
            this.LightChanges = new HashSet<Vector3>();
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
            if (d != 0)
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
            else if (d < -1)
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
                int gx = (int)current.X, gy = (int)current.Y, z = (int)current.Z;
                if (!this.Map.TryGetAll(gx, gy, z, out Chunk chunk, out Cell cell, out int lx, out int ly))
                    continue;
                if (chunk.IsAboveHeightMap(cell.LocalCoords))
                    continue;
                if (!deltas.TryGetValue(current, out byte oldLight))
                    oldLight = chunk.GetSunlight(lx, ly, z);
                if (oldLight > 0)
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
            byte next, maxAdjLight = 0;

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
                    next = (byte)Math.Max(0, maxAdjLight - 1);
            }
            return next;
        }
        private void HandleBlockGlobal(Vector3 global, Queue<Vector3> queue, Dictionary<Vector3, byte> deltas)
        {
            if (!this.Map.TryGetAll(global, out Chunk thisChunk, out Cell thisCell))
                return;
            var neighbors = global.GetAdjacentLazy();
            if (!deltas.TryGetValue(global, out byte thisLight))
                thisLight = thisChunk.GetBlockLight(thisCell.LocalCoords);
            var nextLight = GetNextBlockLight(thisCell, thisChunk, neighbors, deltas);
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

                if (!this.Map.TryGetAll(current, out Chunk chunk, out Cell cell))
                    return;
                if (cell.Opaque)
                    continue;
                if (!deltas.TryGetValue(current, out byte thisLight))
                    thisLight = chunk.GetBlockLight(cell.LocalCoords);
                if (!deltas.TryGetValue(current, out byte oldLight))
                    oldLight = chunk.GetBlockLight(cell.LocalCoords);
                deltas[current] = 0;
                handled.Add(current);
                i++;
                if (oldLight != 0)
                    this.LightChanges.Add(current);
                List<Vector3> neighbors = current.GetNeighbors().ToList();
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

        public void HandleImmediate(IEnumerable<Vector3> vectors)
        {
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
        }
        void HandleSkyGlobalImmediate(Vector3 global, Queue<Vector3> queue, HashSet<Vector3> queued)
        {
            byte oldLight, nextLight;
            int gx = (int)global.X, gy = (int)global.Y, z = (int)global.Z;

            if (!this.Map.TryGetAll(gx, gy, z, out var thisChunk, out var thisCell, out int lx, out int ly))
                return;
            var neighbors = global.GetAdjacentLazy();
            nextLight = GetNextSunLightImmediate(thisCell, thisChunk, gx, gy, z, lx, ly, neighbors);

            oldLight = thisChunk.GetSunlight(lx, ly, z);

            int d = nextLight - oldLight;
            if (d != 0)
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
            else if (d < -1)
            {
                DarkenImmediateWorking(global, queue, queued);
            }
        }
        byte GetNextSunLightImmediate(Cell cell, Chunk chunk, int gx, int gy, int z, int lx, int ly, IEnumerable<Vector3> neighbors)
        {
            byte next, maxAdjLight = 0;

            bool visible = false;
            foreach (var n in neighbors)
            {
                if (!this.Map.TryGetAll(n, out var nchunk, out var ncell))
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
                    next = (byte)Math.Max(0, maxAdjLight - 1);
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
                if (!this.Map.TryGetAll(current, out var chunk, out var cell))
                    continue;

                var local = cell.LocalCoords;
                if (chunk.IsAboveHeightMap(local))
                    continue;
                chunk.SetSunlight(local, 0);

                var neighbors = current.GetAdjacentLazy();
                foreach (Vector3 n in neighbors)
                {
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
            var neighbors = global.GetAdjacent();
            var local = thisCell.LocalCoords;
            var thisLight = thisChunk.GetBlockLight(local);
           
            nextLight = GetNextBlockLightImmediate(thisCell, neighbors);
            thisChunk.SetBlockLight(local, nextLight);

            if (nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
            {
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
                DarkenBlockImmediateWorking(global, queue, queued);
            }
        }
        private byte GetNextBlockLightImmediate(Cell cell, Vector3[] neighbors)
        {
            byte next;
            byte maxAdjLight = 0;
            for (int i = 0; i < neighbors.Length; i++)
            {
                var n = neighbors[i];
                if (!this.Map.TryGetAll(n, out var nchunk, out var ncell))
                    continue;
                if (ncell.Opaque)
                    continue;
                byte l = nchunk.GetBlockLight(ncell.LocalCoords);
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
                    var n = neighbors[i];
                    if (!this.Map.TryGetAll(n, out var nchunk, out var ncell))
                        continue;

                    var nlight = nchunk.GetBlockLight(ncell.LocalCoords);

                    // if neighbor light was less then current previous light, it means that the neighbor was lit from the current cell. so turn the neighbor light off
                    if (nlight < prevLight) // maybe i have to remvoe this line as i did with the darkenskyblocks?
                    {
                        if (nlight > 0)
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
                        if(!queued.Contains(n))
                        {
                            queue.Enqueue(n);
                            queued.Add(n);
                        }
                    }
                }
            }
        }
    }
}
