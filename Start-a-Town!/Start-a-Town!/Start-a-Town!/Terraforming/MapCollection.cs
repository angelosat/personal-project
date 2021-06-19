using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;

namespace Start_a_Town_
{
    public class MapCollection : Dictionary<Vector2, IMap>
    {

        public override string ToString()
        {
            return Count.ToString();
        }
    }

}
