using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.Components.Interactions
{
    class DropInventoryItem : Interaction
    {
        //GameObject Item;
        public DropInventoryItem()
            : base(
            "DropInventoryItem",
            0
            )
        {

        }
        //public DropInventoryItem(GameObject item)
        //    : base(
        //    "DropInventoryItem",
        //    0
        //    )
        //{ this.Item = item; }
        static readonly TaskConditions conds = new TaskConditions(new AllCheck(
                new RangeCheck(t => t.Global, Interaction.DefaultRange),
                new AnyCheck(
                    new TargetTypeCheck(TargetType.Position),
                    new TargetTypeCheck(TargetType.Entity)))
            );
        public override TaskConditions Conditions
        {
            get
            {
                return conds;
            }
        }
        public override void Perform(GameObject actor, TargetArgs target)
        {
            PersonalInventoryComponent.DropInventoryItem(actor, target.Object);
        }

        public override object Clone()
        {
            return new DropInventoryItem();//this.Item);
        }
    }
}
