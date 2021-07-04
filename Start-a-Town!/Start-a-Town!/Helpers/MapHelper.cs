using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    static public class MapHelper
    {
        static public IEnumerable<Vector3> FloodFill(this IMap map, Vector3 begin)
        {
            var discovered = new HashSet<Vector3>();
            var current = begin;
            var cell = map.GetCell(current);
            discovered.Add(current);
            if (cell.IsRoomBorder)
                throw new Exception();
            var tohandle = new Queue<Vector3>();
            tohandle.Enqueue(current);
            yield return current;
            while (tohandle.Any())
            {
                current = tohandle.Dequeue();
                foreach (var n in current.GetAdjacentLazy())
                {
                    if (discovered.Contains(n))
                        continue;
                    if (map.TryGetCell(n, out var ncell))
                    {
                        yield return n;
                        if (!ncell.IsRoomBorder)
                        {
                            tohandle.Enqueue(n);
                            discovered.Add(n);
                        }
                    }
                    else
                    {

                    }
                }
            }
        }

        public static bool IsObstructed(this Vector3 global, Map map)
        {
            foreach (var obj in map.GetObjects())
                if (obj.Global.RoundXY() == global)
                    return true;
            return false;
        }

        /// <summary>
        /// returns the old cell data
        /// </summary>
        /// <param name="global"></param>
        /// <param name="map"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        public static byte SetData(this Vector3 global, Map map, byte data = 0)
        {
            Cell cell = global.GetCell(map);
            byte old = cell.BlockData;
            cell.BlockData = data;
            return old;
        }

        public static byte GetData(this Vector3 global, IObjectProvider net)
        {
            return GetData(global, net.Map);
        }
        public static byte GetData(this Vector3 global, IMap map)
        {
            return map.TryGetCell(global, out Cell cell) ? cell.BlockData : (byte)0;
        }
        public static Cell GetCell(this Vector3 global, Map map)
        {
            return Position.GetCell(map, global);
        }
        public static bool TryGetCell(this Vector3 global, Map map, out Cell cell)
        {
            return global.TryGetAll(map, out Chunk chunk, out cell);
        }
        public static bool TryGetAll(this Vector3 global, Map map, out Chunk chunk, out Cell cell)
        {
            cell = null;
            chunk = null;
            if (map == null)
                return false;
            Vector3 rounded = global.RoundXY();
            if (rounded.Z < 0 || rounded.Z > map.World.MaxHeight - 1)
                return false;
            int chunkX = (int)Math.Floor(rounded.X / Chunk.Size);
            int chunkY = (int)Math.Floor(rounded.Y / Chunk.Size);
            if (map.ActiveChunks.TryGetValue(new Vector2(chunkX, chunkY), out chunk))
            {
                cell = chunk[(int)(rounded.X - chunk.Start.X), (int)(rounded.Y - chunk.Start.Y), (int)rounded.Z];
                return true;
            }
            return false;
        }
        public static bool TryGetChunk(this Vector3 global, Map map, out Chunk chunk)
        {
            chunk = Position.GetChunk(map, global);
            return chunk != null;
        }
        public static Chunk GetChunk(this Vector3 global, Map map)
        {
            return Position.GetChunk(map, global);
        }
        public static Vector2 GetChunkCoords(this Vector3 global)
        {
            int chunkX = (int)Math.Floor(Math.Round(global.X) / Chunk.Size);
            int chunkY = (int)Math.Floor(Math.Round(global.Y) / Chunk.Size);
            return new Vector2(chunkX, chunkY);
        }

        public static byte GetSunLight(this Vector3 global, Map map)
        {
            Chunk.TryGetSunlight(map, global, out byte sunlight);
            return sunlight;
        }
        public static byte GetBlockLight(this Vector3 global, Map map)
        {
            Chunk.TryGetBlocklight(map, global, out byte blocklight);
            return blocklight;
        }
        public static bool GetLight(this Vector3 global, Map map, out byte sky, out byte block)
        {
            return Chunk.TryGetFinalLight(map, (int)global.X, (int)global.Y, (int)global.Z, out sky, out block);
        }
        public static void SetLight(this Vector3 global, Map map, byte sky, byte block)
        {
            Chunk ch = global.GetChunk(map);
            if (ch is null)
                return;
            Vector3 loc = global.ToLocal();
            ch.SetSunlight(loc, sky);
            ch.SetBlockLight(loc, block);
            ch.InvalidateLight(global);
            return;
        }
        public static void SetBlockLight(this Vector3 global, Map map, byte blockLight)
        {
            Chunk ch = global.GetChunk(map);
            if (ch is null)
                return;
            Vector3 loc = global.ToLocal();
            ch.SetBlockLight(loc, blockLight);
            return;
        }

        static public bool IsWithinChunkBounds(this Vector3 local)
        {
            return !(local.X < 0 || local.X > Chunk.Size - 1 || local.Y < 0 || local.Y > Chunk.Size - 1 || local.Z < 0 || local.Z > Start_a_Town_.Map.MaxHeight - 1);

        }
        static public bool IsZWithinBounds(this Vector3 local)
        {
            return !(local.Z < 0 || local.Z > Map.MaxHeight - 1);
        }

    }
}
