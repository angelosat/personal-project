using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public struct Time : IComparable
    {
        DateTime Value;
        public Time(DateTime value)
        {
            this.Value = value;
        }
        public override string ToString()
        {
            return this.Value.ToString("MMM dd, HH:mm:ss", System.Globalization.CultureInfo.GetCultureInfo("en-GB"));
        }
        public int CompareTo(object obj)
        {
            return this.CompareTo((Time)obj);
        }
        public int CompareTo(Time obj)
        {
            return Value.CompareTo(obj.Value);
        }
    }

    public class Position
    {
        public Vector3 Global;// { get; set; }
        public Vector3 Velocity;// { get; set; }

        public Vector3 Local
        {
            get
            {
                int x, y;
                if (Global.X < 0)
                    x = Chunk.Size + (int)Global.X % Chunk.Size;
                else
                    x = (int)Global.X % Chunk.Size;
                y = Global.Y < 0 ? Chunk.Size + (int)Global.Y % Chunk.Size : (int)Global.Y % Chunk.Size;
                return new Vector3(x, y, Global.Z);
            }
        }

        public Vector3 GetLocal()
        {
            //int x, y, globalX = Math.Round(Global.X), globalY = Math.Round(Global.Y);
            //if (Global.X < 0)
            //    x = Chunk.Size + (int)Global.X % Chunk.Size;
            //else
            //    x = (int)Global.X % Chunk.Size;
            //y = Global.Y < 0 ? Chunk.Size + (int)Global.Y % Chunk.Size : (int)Global.Y % Chunk.Size;
            //return new Vector3(x, y, Global.Z); 
            Cell cell = GetCell();
            return cell.LocalCoords;
        }

        //[Obsolete]
        //static public float GetDepth(Vector3 pos)
        //{
        //    return pos.X + pos.Y + pos.Z;
        //    //int x, y;
        //    //Coords.Rotate(camera, pos.X, pos.Y, pos.Z, out x, out y);
        //    //return x + y + pos.Z;
        //}

        static public bool TryGetCell(Map map, Vector3 global, out Cell cell)
        {
            Vector3 rounded = global.RoundXY();
            cell = null;
            if (rounded.Z < 0 || rounded.Z > map.World.MaxHeight - 1)
                return false;
            Chunk chunk;

            int chunkX = (int)Math.Floor(rounded.X / Chunk.Size);
            int chunkY = (int)Math.Floor(rounded.Y / Chunk.Size);
            // if (ChunkLoader.TryGetChunk(map, new Vector2(chunkX, chunkY), out chunk))
            if (map.ActiveChunks.TryGetValue(new Vector2(chunkX, chunkY), out chunk))
            {
                cell = chunk[(int)(rounded.X - chunk.Start.X), (int)(rounded.Y - chunk.Start.Y), (int)rounded.Z];
                return true;
            }
            return false;
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
        
        //Chunk Chunk;
        //Cell Cell;


        static public List<Chunk> GetChunks(Map map, Vector2 pos, int radius = 1)//, int radius = Engine.ChunkRadius)
        {
            Chunk ch;
            List<Chunk> list = new List<Chunk>();
            int x = (int)pos.X, y = (int)pos.Y;
            for (int i = x - radius; i <= x + radius; i++)
                for (int j = y - radius; j <= y + radius; j++)
                    //if (ChunkLoader.TryGetChunk(map, new Vector2(i, j), out ch))
                    if (map.ActiveChunks.TryGetValue(new Vector2(i, j), out ch))
                        list.Add(ch);
            return list;
        }




        static public bool TryGetChunk(Map map, Vector3 globalRounded, out Chunk chunk)
        {
            //float chunkX = (float)Math.Floor(globalRounded.X / Chunk.Size);
            //float chunkY = (float)Math.Floor(globalRounded.Y / Chunk.Size);

            //return ChunkLoader.TryGetChunk(map, new Vector2(chunkX, chunkY), out chunk);

            int chunkX = (int)Math.Floor(globalRounded.X / Chunk.Size);
            int chunkY = (int)Math.Floor(globalRounded.Y / Chunk.Size);
            return map.ActiveChunks.TryGetValue(new Vector2(chunkX, chunkY), out chunk);
        }

        static public bool TryGetChunk(Map map, int globalx, int globaly, out Chunk chunk)
        {
            float chunkX = (float)Math.Floor((float)globalx / Chunk.Size);
            float chunkY = (float)Math.Floor((float)globaly / Chunk.Size);

            // I changed this because chunkloader exists only on the server now
            // return ChunkLoader.TryGetChunk(map, new Vector2(chunkX, chunkY), out chunk);
            return map.ActiveChunks.TryGetValue(new Vector2(chunkX, chunkY), out chunk);
        }


        static public Vector3 Floor(Vector3 global)
        {
            return new Vector3((int)Math.Floor(global.X), (int)Math.Floor(global.Y), (int)global.Z);//(int)Math.Floor(global.Z));
            //return new Vector3((float)Math.Round(global.X), (float)Math.Round(global.Y), (float)Math.Floor(global.Z));
        }
        static public bool TryGetAll(Map map, Vector3 global, out Cell cell, out Chunk chunk, out Vector3 local)
        {
            Vector3 rnd = global.RoundXY();// Position.Round(global);
            local = rnd.ToLocal();// Position.ToLocal(rnd);
            return TryGet(map, global, out cell, out chunk);
        }
        static public bool TryGet(Map map, Vector3 global, out Cell cell, out Chunk chunk)
        {
            return TryGet(map, (int)Math.Round(global.X), (int)Math.Round(global.Y), (int)global.Z, out cell, out chunk);

            //int gz = (int)global.Z;
            //if (gz > Map.MaxHeight - 1 || gz < 0)
            //{
            //    chunk = null;
            //    cell = null;
            //    return false;
            //}
            ////if (gz < 0)
            ////{
            ////    chunk = null;
            ////    cell = null; 
            ////    return false;
            ////}

            //// Vector3 rounded = new Vector3((int)Math.Round(global.X), (int)Math.Round(global.Y), gz);// (int)Math.Floor(global.Z));
            //int gx = (int)Math.Round(global.X), gy = (int)Math.Round(global.Y);
            ////if (TryGetChunk(map, rounded, out chunk))

            //if (TryGetChunk(map, gx, gy, out chunk))
            //{
            //    cell = chunk[gx - chunk.Start.X, gy - chunk.Start.Y, gz];//rounded.Z];
            //    //  cell = chunk[Chunk.FindIndex(gx, gy, gz)];
            //    return true;
            //}

            //chunk = null;
            //cell = null;
            //return false;
        }
        //static public bool TryGet(Map map, Vector3 global, out Cell cell, out Chunk chunk, out Vector3 local)
        //{
        //    int lx, ly;
        //    bool found = TryGet(map, (int)Math.Round(global.X), (int)Math.Round(global.Y), (int)global.Z, out cell, out chunk, out lx, out ly);
        //    local = new Vector3(lx, ly, global.Z);
        //    return found;
        //}
        static public bool TryGet(Map map, int gx, int gy, int gz, out Cell cell, out Chunk chunk)
        {

            if (gz > Map.MaxHeight - 1 || gz < 0)
            {
                chunk = null;
                cell = null;
                return false;
            }
            //if (globalz < 0)
            //{
            //    chunk = null;
            //    cell = null;
            //    return false;
            //}
            if (TryGetChunk(map, gx, gy, out chunk))
            {
                cell = chunk[Chunk.GetCellIndex(gx - (int)chunk.Start.X, gy - (int)chunk.Start.Y, gz)];
                // cell = chunk[gx - chunk.Start.X, gy - chunk.Start.Y, gz];
                return true;
            }
            chunk = null;
            cell = null;
            return false;
        }
        static public bool TryGet(Map map, int gx, int gy, int gz, out Cell cell, out Chunk chunk, out int lx, out int ly)
        {
            if (gz > Map.MaxHeight - 1 || gz < 0)
            {
                lx = 0;
                ly = 0;
                chunk = null;
                cell = null;
                return false;
            }
            if (TryGetChunk(map, gx, gy, out chunk))
            {
                lx = gx - (int)chunk.Start.X;
                ly = gy - (int)chunk.Start.Y;
                cell = chunk[Chunk.GetCellIndex(lx, ly, gz)];
                return true;
            }
            lx = 0;
            ly = 0;
            chunk = null;
            cell = null;
            return false;
        }
        public bool TryGetInfo(out Chunk chunk, out Cell cell)
        {
            if (TryGetChunk(Map, Global, out chunk))
            {
                //cell = chunk[Global.X - chunk.Start.X, Global.Y - chunk.Start.Y, Global.Z];
                cell = chunk[(int)(Global.X - chunk.Start.X), (int)(Global.Y - chunk.Start.Y), (int)Global.Z];
                return true;
            }
            cell = null;
            return false;
        }

        //static public bool TryGetPosition(Vector3 global, out Chunk chunk, out Cell cell)
        //{
        //    if (TryGetChunk(global, out chunk))
        //    {
        //        //cell = chunk[Global.X - chunk.Start.X, Global.Y - chunk.Start.Y, Global.Z];
        //        cell = chunk[(int)(global.X - chunk.Start.X), (int)(global.Y - chunk.Start.Y), (int)global.Z];
        //        return true;
        //    }
        //    cell = null;
        //    return false;
        //}

        //static public void GetGlobal(Chunk chunk, int lx, int ly, out int gx, out int gy)
        //{
        //    gx = chunk.X * Chunk.Size + lx;
        //    gy = chunk.Y * Chunk.Size + ly;
        //}




        static public Cell GetCell(Map map, Vector3 global)
        {
            Vector3 globalRound = new Vector3((int)Math.Round(global.X), (int)Math.Round(global.Y), (int)Math.Floor(global.Z));
            Chunk chunk;// = ChunkLoader( Map.GetChunk(globalRound);
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
            Vector3 globalRound = Rounded;// new Vector3((int)Math.Round(Global.X), (int)Math.Round(Global.Y), (int)Math.Round(Global.Z));
            //Chunk chunk = Map.GetChunk(globalRound);
            Chunk chunk;// = ChunkLoader( Map.GetChunk(globalRound);
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
            //return Map.GetChunk(globalRound);
            Chunk chunk;// = ChunkLoader( Map.GetChunk(globalRound);
            if (TryGetChunk(map, globalRound, out chunk))
                return chunk;
            return null;
        }
        //public Position()
        //{
        //    Global = Vector3.Zero;
        //}
        public Position()
        {

        }
        public Position(Vector3 global)
        {
            Global = global;
        }
        public Position(Vector3 global, Vector3 velocity)
            : this(global)
        {
            this.Velocity = velocity;
        }
        public Position(Map map, Vector3 global)
        {
            this.Map = map;
            Global = global;
        }
        public Position(Map map, Vector3 global, Vector3 velocity)
            : this(map, global)
        {
            //this.Map = map;
            //this.Global = global;
            this.Velocity = velocity;
        }
        //public Position(Map map, Vector3 global)
        //{
        //    this.Global = global;
        //}
        public Position(Position pos)
        {
            this.Map = pos.Map;
            Global = pos.Global;
            Velocity = pos.Velocity;
        }

        public Position(Map map, Chunk chunk, Cell cell)
        {
            this.Map = map;
            Global = new Vector3(chunk.Start, 0) + cell.LocalCoords;
        }


        ///// <summary>
        ///// Returns true if the cell isn't solid and the cell below is solid. False otherwise.
        ///// </summary>
        //public bool IsWalkable
        //{
        //    get
        //    {
        //        Cell cell;
        //        Chunk chunk;
        //        if (TryGetInfo(out chunk, out cell))
        //        {
        //            //If this cell is solid, it's not walkable
        //            if (cell.Solid)
        //                return false;

        //            //Check if the tile below is solid
        //            Cell cellBelow = chunk[cell.X, cell.Y, cell.Z - 1];
        //            if (!cellBelow.Solid)
        //                return false;



        //            return true;
        //        }
        //        return false;
        //    }
        //}

        public bool IsOutOfBounds()
        {
            if (Global.Z < 0)
                return true;
            if (Global.Z > Map.MaxHeight - 1)
                return true;
            return false;
        }

        //public bool IsVisible()
        //{
        //    Cell Cell = GetCell();
        //    if(Cell.Type == Block.Types.Air || Cell.Type == Block.Types.Empty)
        //        return false;

        //    Position w, e, n, s, u, d;
        //    w = new Position(Map, new Vector3(Global.X - 1, Global.Y, Global.Z));
        //    e = new Position(Map, new Vector3(Global.X + 1, Global.Y, Global.Z));
        //    n = new Position(Map, new Vector3(Global.X, Global.Y - 1, Global.Z));
        //    s = new Position(Map, new Vector3(Global.X, Global.Y + 1, Global.Z));
        //    u = new Position(Map, new Vector3(Global.X, Global.Y, Global.Z + 1));
        //    d = new Position(Map, new Vector3(Global.X, Global.Y, Global.Z - 1));
        //    if (!w.GetCell().Solid ||
        //        !e.GetCell().Solid ||
        //        !n.GetCell().Solid ||
        //        !s.GetCell().Solid)
        //        return true;
        //    if (Global.Z < Map.MaxHeight - 1)
        //        if (!u.GetCell().Solid)
        //            return true;
        //    if (Global.Z > 0)
        //        if (!d.GetCell().Solid)
        //            return true;
        //    return false;
        //}

        ///// <summary>
        ///// Stops or starts drawing the cell at that position if it is not hidden
        ///// </summary>
        //public void Initialize()
        //{
        //    //Cell Cell = GetCell();
        //    Cell cell;
        //    Chunk chunk;
        //    if (!TryGetInfo(out chunk, out cell))
        //        return;

        //    //if (HasObjects(Global))
        //    //{
        //    //    //chunk.VisibleOutdoorCells[cell.Depth] = cell;
        //    //    chunk.QueueCell(cell);
        //    //    //chunk.VisibleOutdoorCells.Add(cell);
        //    //    //chunk.ActivateCell(cell);
        //    //    return;
        //    //}

        //    if(cell.TileType != Tile.Types.Air)
        //        if (IsVisible())
        //        {

        //            //chunk.QueueCell(cell);
        //            Chunk.Show(Map, this.Global);
        //            return;
        //        }

        //}



        public void Round()
        {
            Global = new Vector3((float)Math.Round(Global.X), (float)Math.Round(Global.Y), (float)Math.Round(Global.Z));
        }
        public Vector3 Rounded
        {
            //get { return new Vector3((float)Math.Round(Global.X), (float)Math.Round(Global.Y), (float)Math.Round(Global.Z)); }
            get { return new Vector3((float)Math.Round(Global.X), (float)Math.Round(Global.Y), (float)Math.Floor(Global.Z)); }
        }
        public Vector3 Floored
        {
            //get { return new Vector3((float)Math.Round(Global.X), (float)Math.Round(Global.Y), (float)Math.Round(Global.Z)); }
            get { return new Vector3((float)Math.Floor(Global.X), (float)Math.Floor(Global.Y), (float)Math.Floor(Global.Z)); }
        }

        //public bool IsTouchingWater
        //{
        //    get
        //    {
        //        //TODO optimize
        //        //foreach (Position pos in GetNeighbors(global))
        //        List<Position> n = GetNeighbors();
        //        foreach (Position pos in n)
        //            //if (pair != null)
        //            if (pos.Exists)
        //                if (pos.GetCell().Solid)
        //                    if (pos.GetCell().Type == Block.Types.Water)
        //                        return true;
        //        return false;
        //    }
        //}

    [Obsolete]
        static public List<Vector3> GetNeighbors(Vector3 global)
        {
            List<Vector3> neighbors = new List<Vector3>();
            neighbors.Add(global + new Vector3(1, 0, 0));
            neighbors.Add(global - new Vector3(1, 0, 0));
            neighbors.Add(global + new Vector3(0, 1, 0));
            neighbors.Add(global - new Vector3(0, 1, 0));
            neighbors.Add(global + new Vector3(0, 0, 1));
            neighbors.Add(global - new Vector3(0, 0, 1));
            return neighbors;
        }

        public List<Position> GetNeighbors()
        {
            List<Position> list = new List<Position>();

            //Position pos;
            //if (Map.TryGetPosition(Global + new Vector3(1, 0, 0), out pos))
            //    list.Add(pos);
            //if (Map.TryGetPosition(Global + new Vector3(0, 1, 0), out pos))
            //    list.Add(pos);
            //if (Map.TryGetPosition(Global + new Vector3(-1, 0, 0), out pos))
            //    list.Add(pos);
            //if (Map.TryGetPosition(Global + new Vector3(0, -1, 0), out pos))
            //    list.Add(pos);
            list.Add(new Position(Map, Global + new Vector3(1, 0, 0)));
            list.Add(new Position(Map, Global + new Vector3(0, 1, 0)));
            list.Add(new Position(Map, Global + new Vector3(-1, 0, 0)));
            list.Add(new Position(Map, Global + new Vector3(0, -1, 0)));
            if (Global.Z > 0)
                list.Add(new Position(Map, Global + new Vector3(0, 0, -1)));
            if (Global.Z < Map.MaxHeight - 1)
                list.Add(new Position(Map, Global + new Vector3(0, 0, 1)));
            return list;
        }

        public override string ToString()
        {
            //return "\rGlobal: " + Global.ToString() + 
            //    "\r\nChunk: " + (Chunk != null ? Chunk.MapCoords.ToString() : "null") + 
            //    "\r\nCell: " + (Cell != null ? Position.GetGlobal(Chunk, Cell).ToString() : "null");//Cell.LocalCoords.ToString() : "null");
            return "Global: " + Global.ToString() +
                "\nVelocity: " + Velocity.ToString() +
                "\nLocal: " + Local.ToString();// +
              //  "\n" + Map;
        }

        public bool Exists
        { get { return GetCell() != null; } }

        //static public Position Create(Map map, BinaryReader reader)
        //{      
        //    var global = reader.ReadVector3();
        //    var velocity = reader.ReadVector3();
        //    return new Position(map, global, velocity);
        //}
        //static public Position Create(BinaryReader reader)
        //{
        //    Position pos = new Position();
        //    pos.Global = reader.ReadVector3();
        //    pos.Velocity = reader.ReadVector3();
        //    return pos;
        //}
        public void Read(BinaryReader reader)
        {
            this.Global = reader.ReadVector3();
            this.Velocity = reader.ReadVector3();
        }
        public void Write(BinaryWriter writer)
        {
            writer.Write(this.Global);
            writer.Write(this.Velocity);
        }
        static public void Write(BinaryWriter writer, Vector3 global, Vector3 velocity)
        {
            writer.Write(global);
            writer.Write(velocity);
        }
    }
}
