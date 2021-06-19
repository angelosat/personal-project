using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    class Carry : Interaction
    {
        public Carry()
            : base("Hauling", 0)
        { }
        static readonly TaskConditions conds = new TaskConditions(
                new AllCheck(
                    new RangeCheck(t => t.Global, InteractionOld.DefaultRange),
                    new TargetTypeCheck(TargetType.Entity)
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
            GameObjectSlot objSlot = target.Object.Exists ? target.Object.ToSlot() : (from slot in actor.GetChildren() where slot.HasValue select slot).FirstOrDefault(foo => foo.Object == target.Object);// PersonalInventoryComponent.GetFirstOrDefault(parent, foo => foo == obj);
            //actor.GetComponent<GearComponent>().Carry(actor, objSlot);
            actor.GetComponent<HaulComponent>().Carry(actor, objSlot);


            //var hauling = actor.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling];
            //if (hauling.HasValue)
            //{
            //    actor.Net.PostLocalEvent(target.Object, Message.Types.Insert, hauling);
            //}
            //else if (target.Object.GetPhysics().Size == ObjectSize.Inventoryable)
            //    actor.Net.PostLocalEvent(actor, Message.Types.Insert, target.Object.ToSlot());
        }

        public override object Clone()
        {
            return new Carry();
        }
    }
}
