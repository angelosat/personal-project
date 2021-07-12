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
        public MapBase Map;

        public Queue<IntVec3> ToDarken;

        public LightingEngine(MapBase map)
        {
            this.Map = map;
        }

        public void Enqueue(IEnumerable<WorldPosition> vectorBatch)
        {
            this.HandleBatchSync(vectorBatch.Select(t => (IntVec3)t.Global));
        }

        public void HandleBatchSync(IEnumerable<IntVec3> vectors)
        {
            var batch = new BatchToken(vectors);
            var queued = new HashSet<IntVec3>(vectors);
            var block = new BatchToken(vectors);
            var skydeltas = new Dictionary<IntVec3, byte>();
            var blockdeltas = new Dictionary<IntVec3, byte>();
            this.ToDarken = new Queue<IntVec3>();
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
        }
        void HandleSkyGlobalNew(IntVec3 global, Queue<IntVec3> queue, HashSet<IntVec3> queued, Dictionary<IntVec3, byte> deltas)
        {
            int gx = (int)global.X, gy = (int)global.Y, z = (int)global.Z;

            if (!this.Map.TryGetAll(gx, gy, z, out Chunk thisChunk, out Cell thisCell, out int lx, out int ly))
                return;
            var neighbors = global.GetAdjacentLazy();
            var nextLight = GetNextSunLight(thisCell, thisChunk, z, lx, ly, neighbors, deltas);

            if (!deltas.TryGetValue(global, out byte oldLight))
                oldLight = thisChunk.GetSunlight(lx, ly, z);

            int d = nextLight - oldLight;
            if (d != 0)
                deltas[global] = nextLight;

            if (d > 1)
            {
                foreach (var n in neighbors)
                    if (!queued.Contains(n))
                    {
                        queue.Enqueue(n);// TODO: maybe check if the position is already queued?
                        queued.Add(n);
                    }
            }
            else if (d < -1)
            {
                foreach (var n in neighbors)
                    DarkenNew(n, queue, queued, deltas);// TODO: maybe check if the position is already queued?
            }
        }
        void DarkenNew(IntVec3 global, Queue<IntVec3> queue, HashSet<IntVec3> queued, Dictionary<IntVec3, byte> deltas)
        {
            this.ToDarken.Enqueue(global);
            var handled = new HashSet<IntVec3>();
            var i = 0;
            while (this.ToDarken.Count > 0)
            {
                var current = this.ToDarken.Dequeue();
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
        byte GetNextSunLight(Cell cell, Chunk chunk, int z, int lx, int ly, IEnumerable<IntVec3> neighbors, Dictionary<IntVec3, byte> deltas)
        {
            byte next, maxAdjLight = 0;
            foreach (var n in neighbors)
            {
                if (!this.Map.TryGetAll(n, out var nchunk, out var ncell))
                    continue;
                if (ncell.Opaque)
                    continue;
                if (!deltas.TryGetValue(n, out byte l))
                    l = nchunk.GetSunlight(ncell.LocalCoords);
                maxAdjLight = Math.Max(maxAdjLight, l);
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
        private void HandleBlockGlobal(IntVec3 global, Queue<IntVec3> queue, Dictionary<IntVec3, byte> deltas)
        {
            if (!this.Map.TryGetAll(global, out Chunk thisChunk, out Cell thisCell))
                return;
            var neighbors = global.GetAdjacentLazy();
            if (!deltas.TryGetValue(global, out byte thisLight))
                thisLight = thisChunk.GetBlockLight(thisCell.LocalCoords);
            var nextLight = GetNextBlockLight(thisCell, neighbors, deltas);
            deltas[global] = nextLight;

            if (nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
            {
                foreach (var n in neighbors)
                    if (!queue.Contains(n))
                        queue.Enqueue(n);
            }
            else if (nextLight < thisLight)//if the cell became darker, spread darkness surrounding cells
            {
                foreach (var n in neighbors)
                    DarkenBlock(n, queue, deltas);
            }
        }
        void DarkenBlock(IntVec3 global, Queue<IntVec3> queue, Dictionary<IntVec3, byte> deltas)
        {
            this.ToDarken.Enqueue(global);
            var i = 0;
            var handled = new HashSet<IntVec3>();
            while (this.ToDarken.Count > 0)
            {
                IntVec3 current = this.ToDarken.Dequeue();

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
              
                var neighbors = current.GetNeighbors().ToList();
                foreach (var n in neighbors)
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
        private byte GetNextBlockLight(Cell cell, IEnumerable<IntVec3> neighbors, Dictionary<IntVec3, byte> deltas)
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

        public void HandleImmediate(IEnumerable<IntVec3> vectors)
        {
            var queued = new HashSet<IntVec3>(vectors);
            var batch = new BatchToken(vectors);
            var queue = batch.Queue;
            while (queue.Count > 0)
                HandleSkyGlobalImmediate(queue.Dequeue(), queue, queued);

            queued = new HashSet<IntVec3>(vectors);
            var block = new BatchToken(vectors);
            queue = block.Queue;
            while (queue.Count > 0)
                HandleBlockGlobalImmediate(queue.Dequeue(), queue, queued);
        }
        void HandleSkyGlobalImmediate(IntVec3 global, Queue<IntVec3> queue, HashSet<IntVec3> queued)
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
                foreach (var n in neighbors)
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
        byte GetNextSunLightImmediate(Cell cell, Chunk chunk, int gx, int gy, int z, int lx, int ly, IEnumerable<IntVec3> neighbors)
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
        void DarkenImmediateWorking(IntVec3 global, Queue<IntVec3> queue, HashSet<IntVec3> queued)
        {
            var queueToDarken = new Queue<IntVec3>();
            var queueToDarkenQueued = new HashSet<IntVec3>();
            queueToDarken.Enqueue(global);
            queueToDarkenQueued.Add(global);
            while (queueToDarken.Count > 0)
            {
                var current = queueToDarken.Dequeue();
                byte nlight;
                if (!this.Map.TryGetAll(current, out var chunk, out var cell))
                    continue;

                var local = cell.LocalCoords;
                if (chunk.IsAboveHeightMap(local))
                    continue;
                chunk.SetSunlight(local, 0);

                var neighbors = current.GetAdjacentLazy();
                foreach (var n in neighbors)
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
        private void HandleBlockGlobalImmediate(IntVec3 global, Queue<IntVec3> queue, HashSet<IntVec3> queued)
        {
            byte nextLight;
            queued.Remove(global);
            if (!this.Map.TryGetAll(global, out var thisChunk, out var thisCell))
                return;
            var local = thisCell.LocalCoords;
            var thisLight = thisChunk.GetBlockLight(local);

            nextLight = GetNextBlockLightImmediate(thisCell, global);
            thisChunk.SetBlockLight(local, nextLight);

            if (nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
            {
                var adj = VectorHelper.AdjacentIntVec3;
                for (int i = 0; i < adj.Length; i++)
                {
                    var n = global + adj[i];
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
        private byte GetNextBlockLightImmediate(Cell cell, IntVec3 center)
        {
            byte next;
            byte maxAdjLight = 0;
            var adj = VectorHelper.AdjacentIntVec3;
            for (int i = 0; i < adj.Length; i++)
            {
                var n = center + adj[i];
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
        void DarkenBlockImmediateWorking(IntVec3 global, Queue<IntVec3> queue, HashSet<IntVec3> queued)
        {
            var queueToDarken = new Queue<IntVec3>();
            var queueToDarkenQueued = new HashSet<IntVec3>();
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

                var adj = VectorHelper.AdjacentIntVec3;
                for (int i = 0; i < adj.Length; i++)
                {
                    var n = global + adj[i];
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
