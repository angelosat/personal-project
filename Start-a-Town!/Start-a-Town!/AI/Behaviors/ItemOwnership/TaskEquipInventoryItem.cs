using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Start_a_Town_.AI.Behaviors.ItemOwnership
{
    class TaskEquipInventoryItem : AITask
    {
        public TargetArgs TargetItem;

        public TaskEquipInventoryItem(TargetArgs item)
        {
            // TODO: Complete member initialization
            this.TargetItem = item;
            //this.Behavior = new BehaviorEquipInventoryItem(this);
        }
        
        //public override Behavior GetBehavior(GameObject actor)
        //{
        //    return new BehaviorGetItem(this);
        //}
    }
}
