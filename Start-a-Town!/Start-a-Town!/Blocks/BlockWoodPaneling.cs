using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;
using Start_a_Town_.Components.Crafting;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_
{
    class BlockWoodPaneling : Block
    {
        //public override bool IsDeconstructable => true;

        public override Material GetMaterial(byte blockdata)
        {
            return MaterialDefOf.LightWood;
        }
        public BlockWoodPaneling()
            : base(Block.Types.WoodPaneling, GameObject.Types.CobblestoneItem, 0, 1, true, true)
        {
            //this.Material = Material.LightWood;
            this.AssetNames = "woodvertical";
        }
    }
}
