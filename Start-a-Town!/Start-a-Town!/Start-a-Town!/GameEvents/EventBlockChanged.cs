using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.GameEvents
{
    class EventBlockChanged
    {
        static public object[] Write(IMap map, Vector3 global)
        {
            return new object[] { map, global };
        }
        static public void Read(object[] p, out IMap map, out Vector3 global)
        {
            map = p[0] as IMap;
            global = (Vector3)p[1];
        }
    }
}
