using Microsoft.Xna.Framework;

namespace Start_a_Town_.GameEvents
{
    class EventBlockChanged
    {
        static public object[] Write(MapBase map, Vector3 global)
        {
            return new object[] { map, global };
        }
        static public void Read(object[] p, out MapBase map, out Vector3 global)
        {
            map = p[0] as MapBase;
            global = (Vector3)p[1];
        }
    }
}
