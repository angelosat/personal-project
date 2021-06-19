using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Materials;
using Start_a_Town_.Components.Skills;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    public class InteractionConsumeEquipped : Interaction
    {
        public InteractionConsumeEquipped()
            : base(
                "Consume",
                0)
        {
            this.Verb = "Consuming";
        }
        public override void Perform(GameObject actor, TargetArgs target)
        {
            //var consumableSlot = actor.GetComponent<GearComponent>().Holding;
            var consumableSlot = actor.GetComponent<HaulComponent>().Holding;

            if(consumableSlot.Object == null)
                throw new Exception();
            if(consumableSlot.StackSize > 1)
            {
                consumableSlot.Object.StackSize--;
            }
            else
            {
                actor.Net.DisposeObject(consumableSlot.Object);
                consumableSlot.Clear();
            }
        }

        public override object Clone()
        {
            return new InteractionConsumeEquipped();
        }
    }
}
