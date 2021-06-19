using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    class ChunkCollection
    {
        Dictionary<Vector2, Chunk> Chunks = new Dictionary<Vector2, Chunk>();
        public ChunkCollection(IEnumerable<Chunk> chunks)
        {
            foreach (var ch in chunks)
                this.Chunks.Add(ch.MapCoords, ch);
        }
        public ChunkCollection(Dictionary<Vector2, Chunk> chunks)
        {
            this.Chunks = new Dictionary<Vector2, Chunk>(chunks);
        }

        public Chunk GetChunk(Vector2 chunkCoords)
        {
            Chunk chunk;
            this.Chunks.TryGetValue(chunkCoords, out chunk);
            return chunk;
        }
        public bool TryGetChunk(Vector2 chunkCoords, out Chunk chunk)
        {
            return this.Chunks.TryGetValue(chunkCoords, out chunk);
        }
    }
}
