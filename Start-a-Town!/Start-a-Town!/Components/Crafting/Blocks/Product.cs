﻿using System.Collections.Generic;

namespace Start_a_Town_.Components.Crafting
{
    partial class BlockRecipe
    {
        public partial class Product
        {
            public Block Block { get; set; }
            public Block.Data State { get; set; }
            public List<Modifier> Modifiers { get; set; }
            public Product(Block block, params Modifier[] modifiers)
            {
                this.Block = block;
                this.Modifiers = new List<Modifier>(modifiers);
            }
        }
    }
}
