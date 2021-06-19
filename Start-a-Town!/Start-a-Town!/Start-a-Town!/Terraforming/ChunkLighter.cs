using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class ChunkLighter
    {
        Thread SkyThread, CellThread;
        bool Running;
      //  Dictionary<Chunk, Queue<ChunkLoader.ChunkHandler>> CallBackEvents;
     //   ConcurrentQueue<Chunk> ChunksToLight;
        ConcurrentQueue<Vector3> SkyToLight, CellsToLight, ToDarken = new ConcurrentQueue<Vector3>();
        //static public new string ToString()
        static public string Status
        {
            get
            {
                return Instance.SkyThread.Name + ": " + Instance.SkyThread.ThreadState.ToString() + (Instance.SkyThread.ThreadState == ThreadState.Running ? " (" + Instance.SkyToLight.Count + " Tasks)" : "") +
                    "\n" + Instance.CellThread.Name + ": " + Instance.CellThread.ThreadState.ToString() + (Instance.CellThread.ThreadState == ThreadState.Running ? 
                    "\n(" + Instance.CellsToLight.Count + " Tasks)\n("+Instance.ToDarken.Count+") to darken" : "");
            }
        }

        static ChunkLighter _Instance;
        public static ChunkLighter Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new ChunkLighter();
                return _Instance;
            }
            set { _Instance = value; }
        }

        ChunkLighter()
        {
            SkyToLight = new ConcurrentQueue<Vector3>();
            SkyThread = new Thread(LightSky);
            SkyThread.Name = "Sky light";
            CellsToLight = new ConcurrentQueue<Vector3>();
            CellThread = new Thread(LightCells);
            CellThread.Name = "Cell light";
        }

        static public void Begin()
        {
            Instance.Running = true;
            if (Instance.SkyThread.ThreadState == ThreadState.Unstarted)
                Instance.SkyThread.Start();
            else if (Instance.SkyThread.ThreadState == ThreadState.WaitSleepJoin)
                Instance.SkyThread.Interrupt();
            if (Instance.CellThread.ThreadState == ThreadState.Unstarted)
                Instance.CellThread.Start();
            else if (Instance.CellThread.ThreadState == ThreadState.WaitSleepJoin)
                Instance.CellThread.Interrupt();
        }
        static public void End()
        {
            Instance.Running = false;
            Instance.SkyThread.Interrupt();
            Instance.CellThread.Interrupt();
        }
        static public void Stop()
        {
            Instance.SkyToLight = new ConcurrentQueue<Vector3>();
            Instance.CellsToLight = new ConcurrentQueue<Vector3>();
            Instance.Running = false;
        }

        static public ThreadState SkyState
        { get { return Instance.SkyThread.ThreadState; } }
        static public ThreadState CellState
        { get { return Instance.CellThread.ThreadState; } }
        static public bool Paused = false;
        void LightSky()
        {
            object thisLock = new object();
            lock (thisLock)
            {
                do
                {
                    while (SkyToLight.Count > 0)
                        UpdateSunlight(Engine.Map, SkyToLight);
                    try { Thread.Sleep(Timeout.Infinite); }
                    catch (ThreadInterruptedException e) { }
                } while (Running);
            }
        }

        void LightCells()
        {
            object thisLock = new object();
            lock (thisLock)
            {
                do
                {
                    while (CellsToLight.Count > 0)
                    {
                        //if (Paused)
                        //    continue;

                        //Queue<Vector3> workingQueue = new Queue<Vector3>(CellsToLight);
                        //Map map = Engine.Map;
                        //while (workingQueue.Count > 0)
                        //    UpdateCellLight(map, workingQueue.Dequeue(), 15, workingQueue);
                        //CellsToLight = new ConcurrentQueue<Vector3>();

                        while (CellsToLight.Count > 0)
                            UpdateCellLight(Engine.Map, CellsToLight);
                    }

                    try { Thread.Sleep(Timeout.Infinite); }
                    catch (ThreadInterruptedException e) { }
                } while (Running);
            }
        }

        static public void Enqueue(IEnumerable<Vector3> globals)
        {
            foreach (Vector3 global in globals)
                Enqueue(global);
            Begin();
        }
        static public void Enqueue(Vector3 global)
        {
            try
            {
                Instance.SkyToLight.Enqueue(global);
                Instance.CellsToLight.Enqueue(global);
            }
            catch (Exception e) { Log.Enqueue(Log.EntryTypes.Default, e.Message); }
            Begin();
        }

        static public void UpdateSunlight(Map map, ConcurrentQueue<Vector3> queue)
        {
            Object thisLock = new Object();
            //lock (thisLock)
            //{
                //List<Vector3> handled = new List<Vector3>();

            // TODO: maybe check if the position is already queued?
                while (queue.Count > 0)
                {
                    Vector3 global;// = queue.Dequeue();
                    queue.TryDequeue(out global);
                    Cell thisCell;
                    Chunk thisChunk;
                    byte oldLight, nextLight;
                    int gx = (int)global.X, gy = (int)global.Y, z = (int)global.Z, lx, ly;

                    if (!Position.TryGet(map, gx, gy, z, out thisCell, out thisChunk, out lx, out ly))
                        continue;

                    List<Vector3> neighbors = Position.GetNeighbors(global);

                    nextLight = GetNextSunLight(map, thisCell, thisChunk, gx, gy, z, lx, ly, neighbors);

                    thisChunk.SetSunlight(lx, ly, z, nextLight, out oldLight);

                    int d = nextLight - oldLight;

                    if (d > 0)
                    //if(nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
                    {
                        foreach (Vector3 n in neighbors)
                            queue.Enqueue(n);// TODO: maybe check if the position is already queued?
                    }
                    else if (d < 0)
                    //else if(nextLight < thisLight)//if the cell became darker, spread darkness surrounding cells
                    {
                        foreach (Vector3 n in neighbors)
                            Darken(map, n, queue);// TODO: maybe check if the position is already queued?
                    }
                }
            //}
        }

        private static byte GetNextSunLight(Map map, Cell cell, Chunk chunk, int gx, int gy, int z, int lx, int ly, List<Vector3> neighbors)
        {
            byte next, maxAdjLight = GetMaxAdjLight(map, neighbors);
            if (!Cell.IsInvisible(cell))
                if (maxAdjLight > 0) // if I initialize sunlight to 15, i have to check this in order to prevent showing every opaque block
                    Chunk.Show(chunk, cell);
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
        
        static void Darken(Map map, Vector3 global, ConcurrentQueue<Vector3> toLight)
        {
            //  Queue<Vector3> toDarken = new Queue<Vector3>();
            Instance.ToDarken.Enqueue(global);
            List<Vector3> handled = new List<Vector3>();
            while (Instance.ToDarken.Count > 0)
            {
                //Vector3 current = Instance.ToDarken.TryDequeue(out current);
                Vector3 current;
                Instance.ToDarken.TryDequeue(out current);
                Cell cell, ncell;
                Chunk chunk, nchunk;
                byte nlight;
                int gx = (int)current.X, gy = (int)current.Y, z = (int)current.Z, lx, ly;

                if (!Position.TryGet(map, gx, gy, z, out cell, out chunk, out lx, out ly))
                    continue;
                //if (chunk.IsAboveHeightMap(lx, ly, z))
                //    return;
                byte oldLight;// = chunk.GetSunlight(lx, ly, z);
                chunk.SetSunlight(lx, ly, z, 0, out oldLight);
                List<Vector3> neighbors = Position.GetNeighbors(current);
                foreach (Vector3 n in neighbors)
                {
                    if (handled.Contains(n))
                        continue;
                    handled.Add(n);
                    if (!Position.TryGet(map, n, out ncell, out nchunk))
                        continue;

                    if (nchunk.IsAboveHeightMap(ncell.LocalCoords))
                    {
                        toLight.Enqueue(current);
                        continue;
                    }

                    nlight = nchunk.GetSunlight(ncell.LocalCoords);
                    if (ncell.Type == Block.Types.Air//)
                        ||                    cell.Type == Block.Types.Empty)
                        if (nlight < oldLight)
                            Instance.ToDarken.Enqueue(n);
                    //Darken(map, n, toLight);
                }
            }
        }
        
        static byte GetMaxAdjLight(Map map, Vector3 global)
        {
            byte max = 0;
            Chunk chunk;
            Cell cell;
            List<Vector3> neighbors = Position.GetNeighbors(global);
            foreach (Vector3 n in neighbors)
            {
                if (!Position.TryGet(map, n, out cell, out chunk))
                    continue;
                //if (cell.TileType != Tile.Types.Air && cell.TileType != Tile.Types.Water)
                //  //  if (cell.TileType != Tile.Types.Water) 
                //        continue;

                Block.Types tileType = cell.Type;
                //if(cell.TileType == Tile.Types.Air || cell.TileType == Tile.Types.Water)
                switch (tileType)
                {
                    case Block.Types.Air:
                    case Block.Types.Empty:
                    case Block.Types.Water:
                        max = Math.Max(max, chunk.GetSunlight(cell.LocalCoords));
                        break;
                    default:
                        break;
                }
            }
            return max;
        }
        static byte GetMaxAdjLight(Map map, List<Vector3> neighbors)
        {
            byte max = 0;
            Chunk chunk;
            Cell cell;
          //  List<Vector3> neighbors = Position.GetNeighbors(global);
            foreach (Vector3 n in neighbors)
            {
                if (!Position.TryGet(map, n, out cell, out chunk))
                    continue;
                //if (cell.TileType != Tile.Types.Air && cell.TileType != Tile.Types.Water)
                //  //  if (cell.TileType != Tile.Types.Water) 
                //        continue;

                Block.Types tileType = cell.Type;
                //if(cell.TileType == Tile.Types.Air || cell.TileType == Tile.Types.Water)
                switch (tileType)
                {
                    case Block.Types.Air:
                    case Block.Types.Empty:
                    case Block.Types.Water:
                        max = Math.Max(max, chunk.GetSunlight(cell.LocalCoords));
                        break;
                    default:
                        break;
                }
            }
            return max;
        }

        public void UpdateCellLight(Map map, ConcurrentQueue<Vector3> queue)
        {
            Object thisLock = new Object();
            //lock (thisLock)
            while(queue.Count>0)
            {
                Vector3 global;
                queue.TryDequeue(out global);
                Cell thisCell;
                Chunk thisChunk;
                byte thisLight, nextLight;
                if (!Position.TryGet(map, global, out thisCell, out thisChunk))
                    return;
                List<Vector3> neighbors = Position.GetNeighbors(global);
                thisLight = thisChunk.GetCellLight(thisCell.LocalCoords);
                nextLight = GetNextCellLight(map, thisCell, thisChunk, neighbors);
                thisChunk.SetCellLight(thisCell.LocalCoords, nextLight);
                if (nextLight > thisLight) //if the cell became brighter, queue surrounding cells to spread light to them
                {
                    foreach (Vector3 n in neighbors)
                        queue.Enqueue(n);

                }
                else if (nextLight < thisLight)//if the cell became darker, spread darkness surrounding cells
                {
                    Vector3 source = Vector3.Zero;
                    foreach (Vector3 n in neighbors)
                        //DarkenCellLight(n, queue);
                        DarkenCell(map, n, queue);
                }
            }
        }

        private byte GetNextCellLight(Map map, Cell cell, Chunk chunk, List<Vector3> neighbors)
        {
            byte next;
            byte maxAdjLight = GetMaxAdjCellLight(map, neighbors);
            if (!Cell.IsInvisible(cell))
                if (maxAdjLight > 0)
                    Chunk.Show(chunk, cell);
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
      
        void DarkenCell(Map map, Vector3 global, ConcurrentQueue<Vector3> queue)
        {
            Cell cell, ncell;
            Chunk chunk, nchunk;
            byte nlight, thisLight;
            if (!Position.TryGet(map, global, out cell, out chunk))
                return;
            thisLight = chunk.GetCellLight(cell.LocalCoords);
            chunk.SetCellLight(cell.LocalCoords, 0);
            List<Vector3> neighbors = Position.GetNeighbors(global);
            foreach (Vector3 n in neighbors)
            {
                if (!Position.TryGet(map, n, out ncell, out nchunk))
                    continue;

                    nlight = nchunk.GetCellLight(ncell.LocalCoords);

                    if (nlight < thisLight)
                        DarkenCell(map, n, queue);
                    else
                        queue.Enqueue(global);
            }
        }
        
        void DarkenCellLight(Map map, Vector3 global, Queue<Vector3> queue)
        {
            Cell cell, ncell;
            Chunk chunk, nchunk;
            byte nlight;
            if (!Position.TryGet(map, global, out cell, out chunk))
                return;
            chunk.SetCellLight(cell.LocalCoords, 0);
            List<Vector3> neighbors = Position.GetNeighbors(global);
            foreach (Vector3 n in neighbors)
            {
                if (!Position.TryGet(map, n, out ncell, out nchunk))
                    continue;

                //if(ncell.TileType == Tile.Types.Light)
                if(ncell.Luminance > 0)
                {
                    queue.Enqueue(global);
                    return;
                }
                nlight = nchunk.GetCellLight(ncell.LocalCoords);
                if (nlight > 0)
                    DarkenCellLight(map, n, queue);
            }
        }
        byte GetMaxAdjCellLight(Map map, Vector3 global)
        {
            byte max = 0;
            Chunk chunk;
            Cell cell;
            List<Vector3> neighbors = Position.GetNeighbors(global);
            foreach (Vector3 n in neighbors)
            {
                if (!Position.TryGet(map, n, out cell, out chunk))
                    continue;
                if (cell.Type == Block.Types.Air//)
                    || cell.Type == Block.Types.Empty)
                    //||                    cell.TileType != Tile.Types.Empty)// && cell.TileType != Tile.Types.Light)
                   // continue;
                max = Math.Max(max, chunk.GetCellLight(cell.LocalCoords));
            }
            return max;
        }
        byte GetMaxAdjCellLight(Map map, List<Vector3> neighbors)
        {
            byte max = 0;
            Chunk chunk;
            Cell cell;
            foreach (Vector3 n in neighbors)
            {
                if (!Position.TryGet(map, n, out cell, out chunk))
                    continue;
                if (cell.Type == Block.Types.Air//)
                    || cell.Type == Block.Types.Empty)
                    //||                    cell.TileType != Tile.Types.Empty)// && cell.TileType != Tile.Types.Light)
                    // continue;
                    max = Math.Max(max, chunk.GetCellLight(cell.LocalCoords));
            }
            return max;
        }
    }
}
