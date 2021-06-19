using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.GameEvents
{
    class EventBlocksChanged
    {
        static public object[] Write(IMap map, IEnumerable<Vector3> positions)
        {
            return new object[] { map, positions };
        }
        static public void Read(object[] p, out IMap map, out IEnumerable<Vector3> positions)
        {
            map = p[0] as IMap;
            positions = (IEnumerable<Vector3>)p[1];
        }
    }
}
