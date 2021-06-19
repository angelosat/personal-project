using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class DropItem : Interaction
    {
        public DropItem()
            : base(
            "Drop",
            0
            )
        { }
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
        public override void Perform(GameObject actor, TargetArgs target)
        {
            switch (target.Type)
            {
                case TargetType.Position:
                    // check if hauling and drop at target position
                    //GameObject held = actor.GetComponent<GearComponent>().Holding.Take();
                    GameObject held = actor.GetComponent<HaulComponent>().Holding.Take();

                    if (held == null)
                        return;
                    actor.Net.Spawn(held, target.FinalGlobal);
                    break;
                case TargetType.Entity:
                    //var hauling = actor.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling];
                    var hauling = actor.GetComponent<HaulComponent>().GetSlot();//.Slot;

                    if (hauling.HasValue)
                    {
                        actor.Net.PostLocalEvent(target.Object, Message.Types.Insert, hauling);
                        break;
                    }
                    break;

                default:
                    break;
            }
        }

        public override object Clone()
        {
            return new DropItem();
        }
    }
}
