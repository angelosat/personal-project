using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Visualizations
{
    class BlockCacheEntry
    {
        public Vector3 Global, Local;
        public Color SunLight;
        public Vector4 BlockLight;
        public Edges HorEdges;
        public VerticalEdges VerEdges;

        public BlockCacheEntry(Vector3 global, Vector3 local, Color sunLight, Vector4 blockLight, Edges horEdges, VerticalEdges verEdges)
        {
            this.Global = global;
            this.Local = local;
            this.SunLight = sunLight;
            this.BlockLight = blockLight;
            this.HorEdges = horEdges;
            this.VerEdges = verEdges;
        }
    }
}
