using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;

namespace Start_a_Town_.Components
{
    class SpawnerBlock : Spawner
    {
        public Block.Types Block;
        public byte Data;

        public SpawnerBlock(Block.Types block, byte data)
        {
            this.Block = block;
            this.Data = data;
        }

        public override void Spawn(IObjectProvider world, Vector3 global)
        {
            if (this.Finished)
                return;
            this.Finished = true;
            world.SyncSetBlock(global, this.Block, this.Data, 0);
        }
    }
}
