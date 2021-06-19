using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;

namespace Start_a_Town_
{
    public class WorldPosition
    {
        public Vector3 Global { get; private set; }
        public Vector3 Local { get; private set; }
        public IMap Map { get; private set; }
        public Chunk Chunk { get; private set; }
        public Cell Cell { get; private set; }
        //public bool Exists { get { return this.Chunk != null; } }
        public bool Exists { get { return this.Cell != null; } }

        public WorldPosition(IMap map, Vector3 global)
        {
            this.Map = map;
            this.Global = global;
            this.Local = global.ToLocal();
            this.Chunk = map.GetChunk(global);// global.GetChunk(map);
            if(this.Chunk != null)
                this.Cell = this.Chunk[this.Local];
            //if (this.Exists)
            //    this.Cell = this.Chunk[this.Local];
            //if (this.Exists && this.Cell == null)
            //    "e?".ToConsole();
        }

        public WorldPosition[] GetNeighbors()
        {
            WorldPosition[] neighbors = new WorldPosition[6];
            neighbors[0] = new WorldPosition(this.Map, this.Global + new Vector3(1, 0, 0));
            neighbors[1] = new WorldPosition(this.Map, this.Global - new Vector3(1, 0, 0));
            neighbors[2] = new WorldPosition(this.Map, this.Global + new Vector3(0, 1, 0));
            neighbors[3] = new WorldPosition(this.Map, this.Global - new Vector3(0, 1, 0));
            neighbors[4] = new WorldPosition(this.Map, this.Global + new Vector3(0, 0, 1));
            neighbors[5] = new WorldPosition(this.Map, this.Global - new Vector3(0, 0, 1));
            return neighbors;
        }
    }
}
