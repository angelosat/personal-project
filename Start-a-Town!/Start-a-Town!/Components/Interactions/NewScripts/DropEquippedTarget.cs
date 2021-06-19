using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
            // TODO: Complete member initialization
            this.TargetEquipment = targetArgs;
        }
        
        //public override void Perform(GameObject a, TargetArgs t)
        //{
        //    var slot = a.GetComponent<GearComponent>().Equipment.Slots.First(s => s.Object == this.TargetEquipment.Object);
        //    if (slot == null)
        //        throw new Exception();
        //    a.Net.Spawn(slot.Object, a.Global + new Vector3(0, 0, a.Physics.Height));
        //    slot.Clear();
        //}
        internal override void InitAction(GameObject a, TargetArgs t)
        {
            var slot = a.GetComponent<GearComponent>().Equipment.Slots.First(s => s.Object == this.TargetEquipment.Object);
            if (slot == null)
                throw new Exception();
            a.Net.Spawn(slot.Object, a.Global + new Vector3(0, 0, a.Physics.Height));
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
            this.TargetEquipment = TargetArgs.Read((IObjectProvider)null, r);
        }
        protected override void AddSaveData(SaveTag tag)
        {
            tag.Add(this.TargetEquipment.Save("Target"));
        }
        public override void LoadData(SaveTag tag)
        {
            this.TargetEquipment = new TargetArgs(tag["Target"]);
        }
        internal override void Synced(IMap map)
        {
            this.TargetEquipment.Map = map;
        }
    }
}
