using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Start_a_Town_.Components;

namespace Start_a_Town_
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
            if (CachedSlot is null)
                CachedSlot = actor.GetComponent<HaulComponent>().Holding;

            var tool = CachedSlot.Object;
            if (tool is null)
                return false;
            if (CachedDurability is null)
                CachedDurability = tool.GetComponent<EquipComponent>().Durability;
            return CachedDurability.Value > 0;
        }
        public override void GetTooltip(UI.Control tooltip)
        {
            tooltip.Controls.Add(new Label("Requires usable equipment") { Location = tooltip.Controls.BottomLeft });
        }
    }
}
