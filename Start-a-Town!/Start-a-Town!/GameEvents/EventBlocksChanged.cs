using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.GameEvents
{
    class EventBlocksChanged
    {
        static public object[] Write(MapBase map, IEnumerable<Vector3> positions)
        {
            return new object[] { map, positions };
        }
        static public void Read(object[] p, out MapBase map, out IEnumerable<Vector3> positions)
        {
            map = p[0] as MapBase;
            positions = (IEnumerable<Vector3>)p[1];
        }
    }
}
