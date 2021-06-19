using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Net;
using Start_a_Town_.UI;

namespace Start_a_Town_.Blocks
{
    partial class BlockPrefab : Block
    {
        class BlockPrefabEntity : BlockEntity
        {
            public override object Clone()
            {
                return new BlockPrefabEntity();
            }
        }
    }
}
