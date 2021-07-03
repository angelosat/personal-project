using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Start_a_Town_.Blocks;

namespace Start_a_Town_.Components.Interactions
{
    class Unequip : Interaction
    {
        GearType Type;
        public Unequip()
            : base(
                "Unequipping",
                0)
        {

        }
        public Unequip(GearType type)
            : base(
            "Unequipping",
            0)
        {
            this.Type = type;
        }
        public override void Perform(GameObject a, TargetArgs t)
        {
            PersonalInventoryComponent.Unequip(a, this.Type);
        }

        public override object Clone()
        {
            return new Unequip(this.Type);
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
