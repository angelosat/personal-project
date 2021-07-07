using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.Visualizations
{
    class Isometric
    {
        Dictionary<Vector2, Dictionary<Vector3, BlockCacheEntry>> VisibleBlocks;
        bool Valid = false;
        IMap Map;
        Camera Camera;

        public Isometric(IMap map, Camera camera)
        {
            this.Map = map;
            this.VisibleBlocks = new Dictionary<Vector2, Dictionary<Vector3, BlockCacheEntry>>();
            this.Camera = camera;
        }

        private void HandleCell(Chunk chunk, Cell cell)
        {
            var local = cell.LocalCoords;
            var global = cell.LocalCoords.ToGlobal(chunk);
            chunk.UpdateBlockFaces(cell, Edges.All, VerticalEdges.All);
            var light = Camera.GetFinalLight(this.Camera, this.Map, chunk, cell, (int)global.X, (int)global.Y, (int)global.Z);
            var cached = new BlockCacheEntry(global, local, light.Sun, light.Block, cell.HorizontalEdges, cell.VerticalEdges);
        }
    }
}
