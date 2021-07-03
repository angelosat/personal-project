﻿namespace Start_a_Town_.Components.Interactions
{
    class EquipFromInventory : Interaction
    {
        public EquipFromInventory()
            : base(
            "Equipping",
            0)
            
        { }
        
        public override void Perform(GameObject a, TargetArgs t)
        {
            PersonalInventoryComponent.Equip(a, t.Object);
        }

        public override object Clone()
        {
            return new EquipFromInventory();
        }
    }
}
