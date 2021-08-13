﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Start_a_Town_
{
    class BlockBricks : Block
    {
        public BlockBricks()
            : base("Bricks")
        {
            this.LoadVariations("bricks/bricks");
            this.BuildProperties.WorkAmount = 20;
            this.ToggleConstructionCategory(ConstructionsManager.Walls, true);
            this.Ingredient =// new Ingredient(RawMaterialDef.Boulders, null, null, 1);
                new Ingredient()
                    .SetAllow(MaterialTypeDefOf.Metal, true)
                    .SetAllow(MaterialTypeDefOf.Stone, true);
        }
        public override IEnumerable<MaterialDef> GetEditorVariations()
        {
            return Def.GetDefs<MaterialDef>().Where(m => m.Type == MaterialTypeDefOf.Stone || m.Type == MaterialTypeDefOf.Metal);
        }
    }
}
