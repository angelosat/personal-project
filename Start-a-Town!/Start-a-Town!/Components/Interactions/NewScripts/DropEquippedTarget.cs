﻿using System;
using System.Linq;
using Microsoft.Xna.Framework;

namespace Start_a_Town_.Components.Interactions
{
    class DropEquippedTarget : Interaction
    {
        TargetArgs TargetEquipment;
        static readonly public string InteractionName = "DropEquippedTarget";
        public DropEquippedTarget()
            : base("DropEquippedTarget")
        {

        }

        public DropEquippedTarget(TargetArgs targetArgs)
            : base("DropEquippedTarget")
        {
            this.TargetEquipment = targetArgs;
        }
        
        internal override void InitAction(Actor a, TargetArgs t)
        {
            var slot = a.GetComponent<GearComponent>().Equipment.Slots.First(s => s.Object == this.TargetEquipment.Object);
            if (slot == null)
                throw new Exception();
            slot.Object.Spawn(a.Map, a.Global + new Vector3(0, 0, a.Physics.Height));
            slot.Clear();
        }
        public override object Clone()
        {
            return new DropEquippedTarget(this.TargetEquipment);
        }
        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            this.TargetEquipment.Write(w);
        }
        protected override void ReadExtra(System.IO.BinaryReader r)
        {
            this.TargetEquipment = TargetArgs.Read((INetwork)null, r);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.TargetEquipment.Save("Target"));
        }
        public override void LoadData(SaveTag tag)
        {
            this.TargetEquipment = new TargetArgs(tag["Target"]);
        }
        internal override void Synced(MapBase map)
        {
            this.TargetEquipment.Map = map;
        }
    }
}
