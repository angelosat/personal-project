using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    class DropCarried : Interaction
    {
        bool All;

        public DropCarried(bool all = false)
            : base(
            "Drop",
            0
            )
        {
            this.All = all;
        }
        static readonly TaskConditions conds = new TaskConditions(
                new AllCheck(
            //new ScriptTaskCondition("IsCarrying", (a, t) => GearComponent.GetSlot(a, GearType.Hauling).Object != null, Message.Types.InteractionFailed))
            //new ScriptTaskCondition("IsCarrying", (a, t) => a.GetComponent<HaulComponent>().Slot.Object != null, Message.Types.InteractionFailed))
                                    new RangeCheck() { Custom = (a, t) => true },

                new ScriptTaskCondition("IsCarrying", (a, t) => a.GetComponent<HaulComponent>().GetObject() != null, Message.Types.InteractionFailed))
                );
        public override TaskConditions Conditions
        {
	        get 
	        { 
		         return conds;
	        }
        }
        public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
        {
            //return GearComponent.GetSlot(actor, GearType.Hauling).Object != null;
            //return actor.GetComponent<HaulComponent>().Slot.Object != null;
            return actor.GetComponent<HaulComponent>().GetObject() != null;

        }

        public override void Perform(GameObject actor, TargetArgs target)
        {
            //actor.GetComponent<GearComponent>().Throw(Vector3.Zero, actor, this.All);
            actor.GetComponent<HaulComponent>().Throw(Vector3.Zero, actor, this.All);

        }

        // TODO: make it so i have access to the carried item's stacksize, and include it in the name ( Throw 1 vs Throw 16 for example)
        public override string ToString()
        {
            return this.Name + (this.All ? " All" : "");
        }

        public override object Clone()
        {
            return new DropCarried(this.All);
        }
    }
}
