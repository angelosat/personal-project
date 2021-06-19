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
        class BlockCarpentryEntity : BlockEntity
        {
            public BlockCarpentryEntity()
            {
                this.Comps.Add(new BlockEntityCompWorkstation(IsWorkstation.Types.Carpentry));
            this.Comps.Add(new BlockEntityCompDeconstructible());
            }
            public override object Clone()
            {
                return new BlockCarpentryEntity();
            }
        }

}
