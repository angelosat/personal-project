using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Stats;

namespace Start_a_Town_.Components.Interactions
{
    class Lift : Interaction
    {
        public Lift()
            : base(
            "Lift",
            0)
        {
        }

        static readonly TaskConditions conds = new TaskConditions(
                new AllCheck(
                new RangeCheck(t => t.Global, Interaction.DefaultRange),
                new AnyCheck(
                    new TargetTypeCheck(TargetType.Position),
                    new AllCheck(
                        new TargetTypeCheck(TargetType.Entity),
                        new ScriptTaskCondition("Weight", CheckWeight, Message.Types.InteractionFailed))
                        )));
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

                    // new: if inventoryable insert to inventory, if carryable carry
                    // dont carry inventoriables (test)
                case TargetType.Entity:
                    //var hauling = actor.GetComponent<GearComponent>().EquipmentSlots[GearType.Hauling];
                    var hauling = actor.GetComponent<HaulComponent>().GetSlot();//.Slot;//.EquipmentSlots[GearType.Hauling];

                    if (hauling.HasValue)
                    {
                        actor.Net.PostLocalEvent(target.Object, Message.Types.Insert, hauling);
                        return;
                    }

                    //// put item in haulable slot first, in any case
                    //if (CheckWeight(actor, target))
                    //    actor.GetComponent<HaulComponent>().Carry(actor, target.Object.ToSlot());

                    /* 
                     * put item in hauled slot or inventory depending on size
                     * */
                    //if (target.Object.GetPhysics().Size == ObjectSize.Inventoryable)
                    //    actor.Net.PostLocalEvent(actor, Message.Types.Insert, target.Object.ToSlot());
                    //else if (target.Object.GetPhysics().Size == ObjectSize.Haulable)
                    //{
                    //    // check weight here or in gearcomponent?
                    //    if (CheckWeight(actor, target))
                    //        actor.GetComponent<HaulComponent>().Carry(actor, target.Object.ToSlot());
                    //}

                    // put the checks inside the target function (carry) instead of here
                    //if (target.Object.GetPhysics().Size == ObjectSize.Haulable)
                    //{
                    //    // check weight here or in gearcomponent?
                    //    if (CheckWeight(actor, target))
                    //        actor.GetComponent<HaulComponent>().Carry(actor, target.Object.ToSlot());
                    //}
                    actor.GetComponent<HaulComponent>().Carry(actor, target.Object.ToSlotLink());

                    return;

                default:
                    break;
            }
        }
        public override bool AvailabilityCondition(GameObject actor, TargetArgs target)
        {
            return this.Conditions.GetFailedCondition(actor, target) == null;
            //return this.Conditions(actor, target);
            //return new SkillCheck(this.Skill).Condition(actor, target);
        }
        static public bool CheckWeight(GameObject a, TargetArgs t)
        {
            return HaulComponent.CheckWeight(a, t.Object);
            //float w = t.Object.GetPhysics().Weight;
            //float maxW = StatsComponentNew.GetStatValueOrDefault(a, Stat.Types.MaxWeight, 0);
            //return maxW >= w;
        }

        public override object Clone()
        {
            return new Lift();
        }
    }
}
