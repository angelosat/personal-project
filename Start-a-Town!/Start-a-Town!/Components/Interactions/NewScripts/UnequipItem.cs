﻿namespace Start_a_Town_.Components.Interactions
{
    class UnequipItem : Interaction
    {
        public UnequipItem() : base("Unequipping", 0) { }

        public override void Perform(GameObject a, TargetArgs t)
        {
            PersonalInventoryComponent.Unequip(a, t.Object);
        }

        public override object Clone()
        {
            return new UnequipItem();
        }
    }
}
