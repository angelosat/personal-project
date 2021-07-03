using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Components.Interactions
{
    class UnequipItem : Interaction
    {
        public UnequipItem()
            : base(
            "Unequipping",
            0)
            
        { }

        public override void Perform(GameObject a, TargetArgs t)
        {
            PersonalInventoryComponent.Unequip(a, t.Object);
        }

        public override object Clone()
        {
            return new UnequipItem();
        }
    }
}
