using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Skills;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    public class InteractionConsume : Interaction
    {
        ConsumableComponent Comp;
        public InteractionConsume(ConsumableComponent comp)
            : base(
                "Consume",
                0)
        {
            this.Verb = "Consuming";
            this.Comp = comp;
        }
        public override void Perform(GameObject actor, TargetArgs target)
        {
            var slot = target.Slot;
            if(slot==null)
                return;
            if (slot.Object == null)
                return;
            //var cnsmblCmpnt = slot.Object.GetComponent<ConsumableComponent>();
            //if (cnsmblCmpnt == null)
            //    return;
            //cnsmblCmpnt.OnConsume(actor);
            //var consumableSlot = target.Slot;
            //consumableSlot.Consume(1);
            this.Comp.Consume(actor);
            target.Slot.Consume(1);

            //if(consumableSlot.Object == null)
            //    throw new Exception();
            //if(consumableSlot.StackSize > 1)
            //{
            //    consumableSlot.Object.StackSize--;
            //}
            //else
            //{
            //    actor.Net.DisposeObject(consumableSlot.Object);
            //    consumableSlot.Clear();
            //}
        }

        public override object Clone()
        {
            return new InteractionConsume(this.Comp);
        }

        //public override string ToString()
        //{
        //    return this.Comp.Verb;
        //}
    }
}
