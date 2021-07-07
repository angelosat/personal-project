using System;
using System.IO;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    [Obsolete]
    public class Position
    {
        public Vector3 Global;
        public Vector3 Velocity;

        public Vector3 GetLocal()
        {
            Cell cell = GetCell();
            return cell.LocalCoords;
        }

        Map _Map;
        public Map Map
        {
            get { return _Map; }
            set
            {
                _Map = value;
            }
        }
       
        static public bool TryGetChunk(Map map, Vector3 globalRounded, out Chunk chunk)
        {
            int chunkX = (int)Math.Floor(globalRounded.X / Chunk.Size);
            int chunkY = (int)Math.Floor(globalRounded.Y / Chunk.Size);
            return map.ActiveChunks.TryGetValue(new Vector2(chunkX, chunkY), out chunk);
        }

        static public bool TryGetChunk(Map map, int globalx, int globaly, out Chunk chunk)
        {
            float chunkX = (float)Math.Floor((float)globalx / Chunk.Size);
            float chunkY = (float)Math.Floor((float)globaly / Chunk.Size);
            return map.ActiveChunks.TryGetValue(new Vector2(chunkX, chunkY), out chunk);
        }

        static public Vector3 Floor(Vector3 global)
        {
            return new Vector3((int)Math.Floor(global.X), (int)Math.Floor(global.Y), (int)global.Z);
        }

        static public Cell GetCell(Map map, Vector3 global)
        {
            Vector3 globalRound = new Vector3((int)Math.Round(global.X), (int)Math.Round(global.Y), (int)Math.Floor(global.Z));
            Chunk chunk;
            if (TryGetChunk(map, globalRound, out chunk))
            {
                Cell cell = chunk[globalRound.X - chunk.Start.X, globalRound.Y - chunk.Start.Y, globalRound.Z];
                if (cell == null)
                    Console.WriteLine("GAMW TO SPITI");
                return cell;
            }
            return null;
        }
        public Cell GetCell()
        {
            Vector3 globalRound = Rounded;
            Chunk chunk;
            if (TryGetChunk(Map, globalRound, out chunk))
                return chunk[globalRound.X - chunk.Start.X, globalRound.Y - chunk.Start.Y, globalRound.Z];
            return null;
        }
        public Chunk GetChunk()
        {
            Vector3 globalRound = new Vector3((int)Math.Round(Global.X), (int)Math.Round(Global.Y), (int)Math.Floor(Global.Z));
            Chunk chunk;
            if (TryGetChunk(Map, globalRound, out chunk))
                return chunk;
            return null;
        }
        static public Chunk GetChunk(Map map, Vector3 global)
        {
            Vector3 globalRound = new Vector3((int)Math.Round(global.X), (int)Math.Round(global.Y), (int)Math.Floor(global.Z));
            Chunk chunk;
            if (TryGetChunk(map, globalRound, out chunk))
                return chunk;
            return null;
        }
       
        public Position()
        {

        }
        public Position(Vector3 global)
        {
            Global = global;
        }
        
        public Position(Map map, Vector3 global)
        {
            this.Map = map;
            Global = global;
        }
       
        public Position(Position pos)
        {
            this.Map = pos.Map;
            Global = pos.Global;
            Velocity = pos.Velocity;
        }

        public Vector3 Rounded
        {
            get { return new Vector3((float)Math.Round(Global.X), (float)Math.Round(Global.Y), (float)Math.Floor(Global.Z)); }
        }

        public override string ToString()
        {
            return "Global: " + Global.ToString() +
                "\nVelocity: " + Velocity.ToString();
        }
    }
}
