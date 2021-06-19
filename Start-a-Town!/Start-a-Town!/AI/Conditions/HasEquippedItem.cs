using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Start_a_Town_.Components;

namespace Start_a_Town_.AI.Behaviors
{
    class HasEquippedItem : BehaviorCondition
    {
        string ItemKey;
        public HasEquippedItem(string itemKey)
        {
            this.ItemKey = itemKey;
        }
        public override bool Evaluate(GameObject agent, AIState state)
        {
            var equip = GearComponent.GetSlot(agent, GearType.Mainhand);
            var item = state[this.ItemKey] as TargetArgs;
            return equip.Object == item.Object;
        }
        //TargetArgs Item;
        //public HasEquippedItem(TargetArgs item)
        //{
        //    this.Item = item;
        //}
        //public override bool Evaluate(GameObject agent, AIState state)
        //{
        //    var equip = GearComponent.GetSlot(agent, GearType.Mainhand);
        //     return equip.Object == Item.Object;
        //}
    }
}
