using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Control
{
    class MapControl
    {
        public void FindTopmost(Map map, Camera camera)
        {
            foreach (KeyValuePair<Vector2, Chunk> chunk in map.ActiveChunks.OrderBy(foo => -foo.Value.Depth))
            {
                foreach (KeyValuePair<float, Cell> cell in chunk.Value.VisibleOutdoorCells.Reverse())
                {

                }
            }
        }
    }
}
