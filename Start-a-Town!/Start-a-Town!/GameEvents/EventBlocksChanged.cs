using System.Collections.Generic;
using Microsoft.Xna.Framework;

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
