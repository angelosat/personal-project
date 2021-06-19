using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.UI;

namespace Start_a_Town_.Components.Interactions
{
    public class DurabilityCheck : ScriptTaskCondition
    {
        public DurabilityCheck()
            : base("Durability")
        {
            this.ErrorEvent = Message.Types.NoDurability;
        }
        GameObjectSlot CachedSlot;
        Resource CachedDurability;
        public override bool Condition(GameObject actor, TargetArgs target)
        {
            if (CachedSlot.IsNull())
                //CachedSlot = actor.GetComponent<GearComponent>().Holding;
                CachedSlot = actor.GetComponent<HaulComponent>().Holding;

            var tool = CachedSlot.Object;
            if (tool.IsNull())
                return false;
            if (CachedDurability.IsNull())
                CachedDurability = tool.GetComponent<EquipComponent>().Durability;
            return CachedDurability.Value > 0;
        }
        public override void GetTooltip(UI.Control tooltip)
        {
            tooltip.Controls.Add(new Label("Requires usable equipment") { Location = tooltip.Controls.BottomLeft });
        }
    }
}
