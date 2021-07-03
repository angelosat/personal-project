using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.UI;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    public class GearCheck : ScriptTaskCondition
    {
        public GearCheck(GearType type)
            : base("Gear")
        {
            throw new NotImplementedException();
            //this.Type = type;
            //this.ErrorEvent = Message.Types.NoDurability;
        }
        GameObjectSlot CachedSlot;
        GearType Type;
        public override bool Condition(GameObject actor, TargetArgs target)
        {
            if (CachedSlot == null)
                //CachedSlot = actor.GetComponent<GearComponent>().EquipmentSlots[this.Type];
                this.CachedSlot = GearComponent.GetSlot(actor, this.Type);
            var tool = CachedSlot.Object;
            if (tool == null)
                return false;
            return true;
        }
        public override void GetTooltip(UI.Control tooltip)
        {
            tooltip.Controls.Add(new Label("Requires usable equipment") { Location = tooltip.Controls.BottomLeft });
        }
    }
}
