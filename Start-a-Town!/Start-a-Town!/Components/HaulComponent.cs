﻿using System;
using Microsoft.Xna.Framework;
using Start_a_Town_.Animations;

namespace Start_a_Town_.Components
{
    partial class HaulComponent : EntityComponent
    {

        public override string Name { get; } = "Haul"; 
        InventoryComponent Inventory;
        public GameObjectSlot GetSlot()
        {
            return this.Inventory.GetHauling();
        }
        public GameObject GetObject()
        {
            return this.Inventory.GetHauling().Object;
        }

        public Animation AnimationHaul = new(AnimationDef.Haul) { Weight = 0 };

        public override void OnObjectCreated(GameObject parent)
        {
            this.Inventory = parent.GetComponent<InventoryComponent>();
            if (this.Inventory == null || this.Inventory.Capacity == 0)
                throw new Exception("HaulComponent requires a parent entity with PersonalInventoryComponent and an inventory of at least size 1");
            parent.AddResourceModifier(new ResourceRateModifier(ResourceRateModifierDef.HaulingStaminaDrain));
            parent.AddStatModifier(new StatNewModifier(StatModifierDef.WalkSpeedHaulingWeight));
            parent.AddAnimation(this.AnimationHaul);
        }
        
        static public GameObjectSlot GetHolding(GameObject parent)
        {
            return parent.GetComponent<HaulComponent>().Holding;
        }
        public GameObjectSlot Holding
        {
            get
            {
                GameObjectSlot slot = this.GetSlot();//.Slot;
                return slot;
            }
        }

        public override object Clone()
        {
            return new HaulComponent();
        }
        public override void Write(System.IO.BinaryWriter w)
        {
            this.AnimationHaul.Write(w);

        }
        public override void Read(System.IO.BinaryReader r)
        {
            this.AnimationHaul.Read(r);
        }
        
        internal override void SaveExtra(SaveTag tag)
        {
            tag.Add(this.AnimationHaul.Save("AnimationHaul"));
        }
        internal override void LoadExtra(SaveTag save)
        {
            save.TryGetTag("AnimationHaul", this.AnimationHaul.Load);
        }
    }
}
