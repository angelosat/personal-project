using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Interactions;

namespace Start_a_Town_.Components.Interactions
{
    class Insert : Interaction
    {
        ItemContainer Container;

        public Insert(ItemContainer container)
            : base(
            "Insert",
            0
            )
        {
            this.Container = container;
        }
        static readonly TaskConditions conds = 
            new TaskConditions(new AllCheck(
                new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
                new TargetTypeCheck(TargetType.Entity)));

        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }
        public override void Perform(GameObject actor, TargetArgs target)
        {
            //var hauling = actor.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling];
            var hauling = actor.GetComponent<HaulComponent>().GetSlot();//.Slot;

            if (hauling.HasValue)
                this.Container.InsertObject(hauling);
        }

        public override object Clone()
        {
            return new Insert(this.Container);
        }
    }
}
