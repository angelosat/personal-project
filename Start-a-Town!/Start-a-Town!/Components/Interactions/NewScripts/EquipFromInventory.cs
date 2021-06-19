using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components.Skills;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Components.Interactions
{
    class EquipFromInventory : Interaction
    {
        public EquipFromInventory()
            : base(
            "Equipping",
            0)
            
        { }
        //static readonly TaskConditions conds = new TaskConditions(
        //        new AllCheck(
        //            new TargetTypeCheck(TargetType.Slot),
        //            new RangeCheck(t => t.Slot.Object.Global, InteractionOld.DefaultRange),
        //            new ScriptTaskCondition("SlotHasValue", (a, t) => t.Slot.HasValue),
        //            new ScriptTaskCondition("IsEquipment", (a, t) => t.Slot.Object.HasComponent<EquipComponent>())));
        //public override TaskConditions Conditions
        //{
        //    get
        //    {
        //        return conds;
        //    }
        //}
        public override void Perform(GameObject a, TargetArgs t)
        {
            //var gearType = t.Slot.Object.GetComponent<EquipComponent>().Type;
            ////GameObjectSlot gearSlot = a.GetComponent<GearComponent>().EquipmentSlots[gearType];
            //GameObjectSlot gearSlot = a.GetComponent<GearComponent>().Equipment.Slots[(int)t.Object.GetComponent<EquipComponent>().Type.ID];
            //gearSlot.Swap(t.Slot);

            PersonalInventoryComponent.Equip(a, t.Object);
            return;
            //GearComponent.Equip(a, t.Slot);
        }

        public override object Clone()
        {
            return new EquipFromInventory();
        }
    }
}
