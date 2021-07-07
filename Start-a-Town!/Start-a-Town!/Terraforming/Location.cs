using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_
{
    [Obsolete]
    class Location
    {
        public IMap Map;
        public Vector3 Global;
        public Location(IMap map, Vector3 global)
        {
            this.Map = map;
            this.Global = global;
        }
        public Block GetBlock()
        {
            return this.Map.GetBlock(this.Global);
        }
        public float GetDensity()
        {
            var cell = this.Map.GetCell(this.Global);
            return cell.Block.GetDensity(cell.BlockData, this.Global);
        }
        public bool IsStandable()
        {
            var global = this.Global;
            var map = this.Map;
            var gravity = map.Gravity;
            var box = new BoundingBox(global - new Vector3(.25f, .25f, 0), global + new Vector3(.25f, .25f, 1));
            var corners = new Vector3[] { 
                    box.Min, 
                    new Vector3(box.Min.X, box.Max.Y, global.Z), 
                    new Vector3(box.Max.X, box.Min.Y, global.Z), 
                    new Vector3(box.Max.X, box.Max.Y, global.Z) 
                };
            return corners.Any(c => map.GetBlock(c + new Vector3(0, 0, gravity)).Density > 0);
        }
    }
}
