using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.GameModes.StaticMaps
{
    public class MapCollection : Dictionary<Vector2, IMap>
    {

        public override string ToString()
        {
            return Count.ToString();
        }
    }

}
