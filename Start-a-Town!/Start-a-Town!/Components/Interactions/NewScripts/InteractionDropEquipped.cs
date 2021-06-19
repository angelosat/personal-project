using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Components;

namespace Start_a_Town_
{
    class InteractionDropEquipped : Interaction
    {
        GearType Type;
        static readonly public string InteractionName = "DropEquipped";
        public InteractionDropEquipped()
            : base("DropEquipped")
        {

        }
        public InteractionDropEquipped(GearType type):base("DropEquipped")
        {
            this.Type = type;
        }
        
        //public override void Perform(GameObject a, TargetArgs t)
        //{
        //    var slot = GearComponent.GetSlot(a, this.Type);
        //    if (slot.Object == null)
        //        return;
        //        //throw new Exception();
        //    a.Net.Spawn(slot.Object, a.Global + new Vector3(0, 0, a.Physics.Height));
        //    slot.Clear();
        //}
        internal override void InitAction(GameObject a, TargetArgs t)
        {
            var slot = GearComponent.GetSlot(a, this.Type);
            if (slot.Object == null)
                return;
            //a.Net.Spawn(slot.Object, a.Global + new Vector3(0, 0, a.Physics.Height));
            slot.Object.Spawn(a.Map, a.Global + new Vector3(0, 0, a.Physics.Height));
            slot.Clear();
        }
        public override object Clone()
        {
            return new InteractionDropEquipped(this.Type);
        }
        protected override void WriteExtra(System.IO.BinaryWriter w)
        {
            w.Write((int)this.Type.ID);
        }
        protected override void ReadExtra(System.IO.BinaryReader r)
        {
            this.Type = GearType.Dictionary[(GearType.Types)r.ReadInt32()];
        }
    }
}
