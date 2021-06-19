using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Pathfinding
{
    class PFTile
    {
        public Vector2 ScreenLocation;
        //public PFTile parent;
        //public int g =0, h = 0;

        public PFTile(Vector2 pos)
        {
            ScreenLocation = pos;
        }

        public float X
        {
            get { return ScreenLocation.X; }
            set { ScreenLocation.X = value; }
        }
        public float Y
        {
            get { return ScreenLocation.Y; }
            set { ScreenLocation.Y = value; }
        }
    }
}
