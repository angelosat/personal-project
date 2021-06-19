using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class HaulFromInventory : Interaction
    {
        public HaulFromInventory()
            : base(
            "HaulFromInventory",
            0
            )
        {

        }

        static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                new RangeCheck(t => t.Global, Interaction.DefaultRange),
                new AnyCheck(
                    new TargetTypeCheck(TargetType.Position),
                    new TargetTypeCheck(TargetType.Entity)))
            );
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }
        public override void Perform(GameObject a, TargetArgs t)
        {
            PersonalInventoryComponent.HaulFromInventory(a, t.Object);
        }
        public override object Clone()
        {
            return new HaulFromInventory();
        }
    }
}
