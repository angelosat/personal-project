using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.GameModes;

namespace Start_a_Town_.GameEvents
{
    class EventEntityDespawned
    {
        static public object[] Write(GameObject entity)
        {
            return new object[] { entity };
        }
        static public void Read(object[] p, out GameObject entity)
        {
            entity = p[0] as GameObject;
        }
    }
}
