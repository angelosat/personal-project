using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Crafting;
using Start_a_Town_.Components.Items;
using Start_a_Town_.Components;
using Start_a_Town_.Tokens;

namespace Start_a_Town_.Blocks
{
    partial class BlockCarpentryBench
    {
        class Entity : BlockEntityWorkstation
        {
            Container Storage = new Container(4);
            public override Container Input
            {
                get { return this.Storage; }
            }
            public override Tokens.IsWorkstation.Types Type
            {
                get { return IsWorkstation.Types.Carpentry; }
            }
            public override object Clone()
            {
                return new Entity();
            }
        }
    }
}
