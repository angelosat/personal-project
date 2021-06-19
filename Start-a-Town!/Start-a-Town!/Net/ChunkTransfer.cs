using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Net
{
    class ChunkTransfer
    {
        public enum SegmentTypes { CellGrid, Objects, Light, VisibleCells }
        public class ChunkSegment
        {
            public SegmentTypes Type;
            public byte[] Data;
            public ChunkSegment(SegmentTypes seg, byte[] data)
            {
                this.Type = seg;
                this.Data = data;
            }
        }

        struct PartialChunk
        {
            public Vector2 Coords;
            public Dictionary<SegmentTypes, byte[]> Segments;
            public PartialChunk(Vector2 coords)
            {
                this.Coords = coords;
                this.Segments = new Dictionary<SegmentTypes, byte[]>(){
                {SegmentTypes.CellGrid, null},
                {SegmentTypes.Objects, null},
                {SegmentTypes.Light, null},
                {SegmentTypes.VisibleCells, null}
                };
            }
            public bool Receive(ChunkSegment seg)
            {
                this.Segments[seg.Type] = seg.Data;
                return this.Segments.Values.All(data => !data.IsNull());
            }

        }
        PartialChunk Partial;
        Action<Chunk> Callback;
        static public ChunkTransfer BeginReceive(Vector2 chunkCoords, Action<Chunk> callback)
        {
            return new ChunkTransfer(chunkCoords, callback);
        }
        public ChunkTransfer(Vector2 chunkCoords)
        {
            this.Partial = new PartialChunk(chunkCoords);
        }
        public ChunkTransfer(Vector2 chunkCoords, Action<Chunk> callback)
        {
            this.Partial = new PartialChunk(chunkCoords);
            this.Callback = callback;
        }
        public void Receive(ChunkSegment seg)
        {
            if (this.Partial.Receive(seg))
            {
                byte[] merged = new byte[0];
                Chunk chunk = merged.Deserialize<Chunk>(Chunk.Create);
                this.Callback(chunk);
            }
        }
    }
}
