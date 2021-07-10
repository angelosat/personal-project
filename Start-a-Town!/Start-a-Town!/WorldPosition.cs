using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    public class WorldPosition
    {
        public Vector3 Global { get; private set; }
        public Vector3 Local { get; private set; }
        public MapBase Map { get; private set; }
        public Chunk Chunk { get; private set; }
        public Cell Cell { get; private set; }
        public bool Exists { get { return this.Cell != null; } }

        public WorldPosition(MapBase map, Vector3 global)
        {
            this.Map = map;
            this.Global = global;
            this.Local = global.ToLocal();
            this.Chunk = map.GetChunk(global);
            if(this.Chunk != null)
                this.Cell = this.Chunk[this.Local];
        }
    }
}
