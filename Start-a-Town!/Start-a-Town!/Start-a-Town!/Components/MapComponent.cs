using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Start_a_Town_.Components
{
    class MapComponent : Component
    {
        Map Map { get { return (Map)this["Map"]; } set { this["Map"] = value; } }

        public MapComponent(Map map = null)
        {
            this.Map = map;
        }

        public override object Clone()
        {
            return new MapComponent(this.Map);
        }
    }
}
