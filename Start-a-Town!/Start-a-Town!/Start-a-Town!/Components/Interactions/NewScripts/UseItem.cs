using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    class UseItem : Interaction
    {
        public UseItem()
            : base(
            "Use",
            0
            )
        {
        }

        static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                new ScriptTaskCondition("ItemHasUse", AvailabilityCondition, Message.Types.InteractionFailed)));

        static bool AvailabilityCondition(GameObject actor, TargetArgs target)
        {
            //return GearComponent.GetSlot(actor, GearType.Hauling).Object != null;
            var slot = GearComponent.GetSlot(actor, GearType.Mainhand);
            if(slot.Object == null)
                return false;
            return UseComponent.GetInteraction(slot.Object) != null;
        }

        public override void Perform(GameObject actor, TargetArgs target)
        {
            //actor.GetComponent<WorkComponent>().UseTool(actor, target);
            var slot = GearComponent.GetSlot(actor, GearType.Mainhand);
            var interaction = UseComponent.GetInteraction(slot.Object);
            actor.GetComponent<WorkComponent>().Perform(actor, interaction, target);
        }

        public override object Clone()
        {
            return new UseItem();
        }
    }
}
