using Start_a_Town_.AI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class InteractionStoreHauled : Interaction
    {
        public InteractionStoreHauled()
            : base(
            "Put in inventory",
            0
            )
        {
        }
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
            //PersonalInventoryComponent.StoreHauled(actor);
            var cachedObject = target.Object;
            actor.StoreCarried();
            (actor as Actor).Log.Write(string.Format("Stored {0} in inventory", cachedObject));
        }

        public override object Clone()
        {
            return new InteractionStoreHauled();
        }
    }
}
