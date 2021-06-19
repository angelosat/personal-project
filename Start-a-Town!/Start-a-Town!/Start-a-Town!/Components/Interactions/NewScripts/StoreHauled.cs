using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class StoreHauled : Interaction
    {
        public StoreHauled()
            : base(
            "Put in inventory",
            0
            )
        { }
        static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                new ScriptTaskCondition("Is Hauling", (a, t) => a.GetComponent<HaulComponent>().GetObject() != null)//.Slot.Object != null)
            ));
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }
        public override void Perform(GameObject actor, TargetArgs target)
        {
            //var hauled = actor.GetComponent<HaulComponent>().GetSlot();//.Slot;
            //actor.GetComponent<PersonalInventoryComponent>().Insert(actor, hauled);
            PersonalInventoryComponent.StoreHauled(actor);
        }

        public override object Clone()
        {
            return new StoreHauled();
        }
    }
}
