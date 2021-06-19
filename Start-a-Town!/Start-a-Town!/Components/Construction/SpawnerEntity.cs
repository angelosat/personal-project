using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Net;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components
{
    class SpawnerEntity : Spawner
    {
        public GameObject Entity;

        public SpawnerEntity(GameObject entity)
        {
            this.Entity = entity;
        }

        public override void Spawn(IObjectProvider world, Vector3 global)
        {
            if (this.Finished)
                return;
            this.Finished = true;
            this.Entity.Global = global;
            world.Spawn(this.Entity);
        }
    }
}
