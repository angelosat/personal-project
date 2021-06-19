using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components
{
    class Spawner 
    {
        public bool Finished { get; protected set; }
        public virtual void Spawn(IObjectProvider world, Vector3 global) { }
    }
}
