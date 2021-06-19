using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Start_a_Town_.Graphics;

namespace Start_a_Town_.Components.Interactions
{
    class EquipFromWorld : Interaction
    {
        public EquipFromWorld()
            : base("Equip", 0)
        { }
        static readonly TaskConditions conds = new TaskConditions(new AllCheck(new RangeCheck(t => t.Global, Interaction.DefaultRange)));
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }
        public override void Perform(GameObject a, TargetArgs t)
        {
            GearComponent.Equip(a, t.Object.ToSlotLink());
            //GameObjectSlot objSlot = 
            //    t.Object.Exists ? 
            //    t.Object.ToSlot() : 
            //    (from slot in a.GetChildren() where slot.HasValue select slot).FirstOrDefault(foo => foo.Object == t.Object);
            ////GameObjectSlot gearSlot = a.GetComponent<GearComponent>().EquipmentSlots[t.Object.GetComponent<EquipComponent>().Type];
            //GameObjectSlot gearSlot = a.GetComponent<GearComponent>().Equipment.Slots[(int)t.Object.GetComponent<EquipComponent>().Type.ID];

            ////a.Net.Despawn(objSlot.Object);
            ////if(gearSlot.HasValue)
            ////    a.Net.Spawn(gearSlot.Object, objSlot.Object.Global);
            ////gearSlot.Swap(objSlot);

            //// despawn item's entity from world
            //t.Object.Net.Despawn(objSlot.Object);

            //// attempt to store current equipped item in inventory, otherwise drop it if inventory is full
            //PersonalInventoryComponent.InsertOld(a, gearSlot);

            //// equip new item
            //gearSlot.Swap(objSlot);
        }

        public override object Clone()
        {
            return new EquipFromWorld();
        }
    }
}
